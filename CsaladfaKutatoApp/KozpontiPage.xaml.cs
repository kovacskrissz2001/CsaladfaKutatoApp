using CsaladfaKutatoApp.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CsaladfaKutatoApp.Models;
using CsaladfaKutatoApp.Models.DTO;
using System.IO;
using CsaladfaKutatoApp.Segedeszkozok;
using System.Reflection;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Security.Cryptography;


namespace CsaladfaKutatoApp
{
    /// <summary>
    /// Interaction logic for KozpontiPage.xaml
    /// </summary>
    public partial class KozpontiPage : Page
    {
        private readonly CsaladfaAdatbazisContext _context;
        private readonly int? _felhasznaloId;
        private Point _lastMousePosition;
        private bool _isDragging = false;
        BitmapImage bitmap;
        private Border kijeloltBorder = null;
        public RajzoltSzemely LegutobbKijeloltSzemely { get; set; }
        public List<RajzoltSzemely> RajzoltSzemelyek;
        private Dictionary<int, Border> szemelyBorderDictionary = new Dictionary<int, Border>();
        private Dictionary<int, int> partnerMap = new Dictionary<int, int>();



        public KozpontiPage(CsaladfaAdatbazisContext context, int? felhasznaloId)
        {
            InitializeComponent();
            _context = context;
            _felhasznaloId = felhasznaloId;

            TartalomValto.Content = new KezdoTartalomControl(this, _context);

            Loaded += KozpontiPage_Loaded; // hívás betöltéskor
        }

        private void KozpontiPage_Loaded(object sender, RoutedEventArgs e)
        {
            // Kisebb zoom (pl. 80% méret)
            CanvasScaleTransform.ScaleX = 0.8;
            CanvasScaleTransform.ScaleY = 0.8;

            MegjelenitCsaladfat();
        }

        public void TartalomValtas(UserControl ujTartalom)
        {
            TartalomValto.Content = ujTartalom;
        }

        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _isDragging = true;
            _lastMousePosition = e.GetPosition(this);
            CsaladfaVaszon.CaptureMouse();
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDragging)
            {
                Point currentPosition = e.GetPosition(this);
                Vector delta = currentPosition - _lastMousePosition;

                CanvasTranslateTransform.X += delta.X;
                CanvasTranslateTransform.Y += delta.Y;

                _lastMousePosition = currentPosition;
            }
        }

        private void Canvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _isDragging = false;
            CsaladfaVaszon.ReleaseMouseCapture();
        }

        private void CsaladfaScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            double zoomFactor = e.Delta > 0 ? 1.1 : 1 / 1.1;

            CanvasScaleTransform.ScaleX *= zoomFactor;
            CanvasScaleTransform.ScaleY *= zoomFactor;

            e.Handled = true;
        }
        

        private Border RajzolSzemelyt(RajzoltSzemely szemely, double pozicioX, double pozicioY)
        {
            var tartalomDoboz = new StackPanel
            {
                Orientation = Orientation.Vertical,
                Width = 100,
                Tag = szemely //mentjük a személy objektumot későbbi használatra
            };

            string alapKepBase64 = szemely.Nem == "Férfi"
                 ? AlapertelmezettKepek.FerfiBase64
                 : AlapertelmezettKepek.NoBase64;

            szemely.KepBase64 = alapKepBase64;


            if (!string.IsNullOrWhiteSpace(szemely.KepBase64))
            {
                try
                {
                    byte[] kepBytes = Convert.FromBase64String(szemely.KepBase64);
                    var memstream = new MemoryStream(kepBytes); 
                    bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.StreamSource = memstream;
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    bitmap.Freeze();
                }
                catch
                {
                    bitmap = new BitmapImage(new Uri("Kepek/alap.png", UriKind.Relative)); 
                }
            }
            else
            {
                bitmap = new BitmapImage(new Uri("Kepek/alap.png", UriKind.Relative)); // fallback
            }

            var kep = new Image
            {
                Source = bitmap,
                Width = 40,
                Height = 40,
                Margin = new Thickness(10)
            };

            var nevSzoveg = new TextBlock
            {
                Text = szemely.VNev +" "+szemely.KNev,
                TextAlignment = TextAlignment.Center,
                FontWeight = FontWeights.Bold,
                TextWrapping = TextWrapping.Wrap,
                MaxWidth = 90
            };

            var datumSzoveg = new TextBlock
            {
                Text = szemely.SzuletesiDatum?.ToString("yyyy-MM-dd") ?? "",
                TextAlignment = TextAlignment.Center,
                FontSize = 10
            };

            tartalomDoboz.Children.Add(kep);
            tartalomDoboz.Children.Add(nevSzoveg);
            tartalomDoboz.Children.Add(datumSzoveg);

            // Border hozzáadása
            var border = new Border
            {
                Child = tartalomDoboz,
                BorderThickness = new Thickness(2),
                BorderBrush = Brushes.Transparent, // alapértelmezett nem látható
                Tag = szemely
            };

            // Kattintás esemény kezelése
            border.MouseLeftButtonDown += SzemelyBorder_MouseLeftButtonDown;

            Canvas.SetLeft(border, pozicioX);
            Canvas.SetTop(border, pozicioY);

            CsaladfaVaszon.Children.Add(border);
            szemelyBorderDictionary[szemely.Azonosito] = border;
            return border;
            
        }
        public void TorolSzemelyACanvasrol(RajzoltSzemely torlendoSzemely)
        {
            if (torlendoSzemely?.UIElem == null) return;

            // Törlés a Canvasról
            CsaladfaVaszon.Children.Remove(torlendoSzemely.UIElem);

            // Frissítjük a személyek listáját
            RajzoltSzemelyek.Remove(torlendoSzemely);

            // Új kijelölés: következő kisebb ID-jú személy
            var kovetkezoSzemely = RajzoltSzemelyek
                .Where(s => s.Azonosito < torlendoSzemely.Azonosito)
                .OrderByDescending(s => s.Azonosito)
                .FirstOrDefault();

            LegutobbKijeloltSzemely = kovetkezoSzemely;

            // Kijelölés vizuális frissítése (például border szín változtatással)
            
            if (LegutobbKijeloltSzemely != null && LegutobbKijeloltSzemely.UIElem != null && LegutobbKijeloltSzemely?.UIElem is Border ujBorder)
            { 
                ujBorder.BorderBrush = Brushes.OrangeRed;
                kijeloltBorder = ujBorder;
            }
            

            // Aktuális KezdoTartalomControl elérése
            if (TartalomValto.Content is KezdoTartalomControl aktivTartalom)
            {
                aktivTartalom.DataContext = LegutobbKijeloltSzemely;
            }
        }


        void SzemelyBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var border = sender as Border;

            // Előző kijelölés eltüntetése
            if (kijeloltBorder != null)
                kijeloltBorder.BorderBrush = Brushes.Transparent;

            // Aktuális kijelölés
            border.BorderBrush = Brushes.OrangeRed;
            kijeloltBorder = border;

            var szemely = border.Tag as RajzoltSzemely;

            // Ezt elmentjük a későbbi visszatéréshez!
            LegutobbKijeloltSzemely = szemely;

            // Aktuális KezdoTartalomControl elérése
            if (TartalomValto.Content is KezdoTartalomControl aktivTartalom)
            {
                aktivTartalom.DataContext = szemely;
            }
        }

        private void RajzolVonalat(double kezdoX, double kezdoY, double vegX, double vegY)
        {
            var vonal = new Line
            {
                X1 = kezdoX,
                Y1 = kezdoY,
                X2 = vegX,
                Y2 = vegY,
                Stroke = Brushes.DarkSlateGray,
                StrokeThickness = 2
            };

            CsaladfaVaszon.Children.Add(vonal);
        }

        private List<RajzoltSzemely> BetoltSzemelyekAdatbazisbol()
        {
            return _context.Szemelyeks
                .Include(s => s.Fotoks)
                .Where(s => s.FelhasznaloId == _felhasznaloId)
                .Select(s => new RajzoltSzemely
                {
                    Azonosito = s.SzemelyId,
                    KNev = s.Keresztnev,
                    VNev = s.Vezeteknev,
                    Nem = s.Neme,
                    SzuletesiDatum = s.SzuletesiDatum,
                    KepBase64 = s.Fotoks.Select(f => f.FotoBase64).FirstOrDefault(),
                    GyermekAzonositoLista = new List<int>() // később betöltjük kapcsolatokkal
                })
                .ToList();
        }

        /* private void BetoltKapcsolatokat(List<RajzoltSzemely> szemelyek)
         {
             var kapcsolatok = _context.Kapcsolatoks.ToList();

             foreach (var kapcsolat in kapcsolatok)
             {
                 var szulo = szemelyek.FirstOrDefault(s => s.Azonosito == kapcsolat.KapcsolodoSzemelyId);
                 var gyerek = szemelyek.FirstOrDefault(s => s.Azonosito == kapcsolat.SzemelyId);

                 if (szulo != null && gyerek != null && kapcsolat.KapcsolatTipusa == "Gyermek")
                 {
                     szulo.GyermekAzonositoLista.Add(gyerek.Azonosito);
                 }
             }
         }*/
        private void MegjelenitCsaladfat()
        {
            var szemelyek = BetoltSzemelyekAdatbazisbol();
            var kapcsolatok = BetoltKapcsolatok();
            var utolso = szemelyek.OrderByDescending(s => s.Azonosito).FirstOrDefault();


            RajzoltSzemelyek = szemelyek;

            // 👉 Először kiszámoljuk a generációkat (fontos!)
            SzamoldKiGeneraciokat(szemelyek, kapcsolatok);



            double xKezdo = 50;
            double yKezdo = 50;
            double xTav = 200;
            double yTav = 150;

            double gyerekXTav = 100;

            Dictionary<int, RajzoltSzemely> szemelyDict = szemelyek.ToDictionary(s => s.Azonosito);

            // 👉 Női személyek, akinek van párja vagy gyermeke
            var anyak = kapcsolatok
                .Where(k => k.KapcsolatTipusa == "Gyermek")
                .Select(k => k.SzemelyId)
                .Distinct()
                .ToList();

            //nincs gyerekük de párkapolatban lévő nők
            var nokCsakKapcsolatban = kapcsolatok
                .Where(k => k.KapcsolatTipusa == "Partner")
                .Where(k => !kapcsolatok.Any(gy => gy.SzemelyId == k.SzemelyId  && gy.KapcsolatTipusa == "Gyermek"))
                .Select(k => k.SzemelyId)
                .Distinct()
                .ToList();

            // nem családtagok de párkapolatban lévő férfiak
            var ferfiakCsakKapcsolatban = kapcsolatok
                .Where(k => k.KapcsolatTipusa == "Partner")
                .Where(k => !kapcsolatok.Any(gy => gy.KapcsolodoSzemelyId == k.KapcsolodoSzemelyId && gy.KapcsolatTipusa == "Gyermek"))
                .Select(k => k.KapcsolodoSzemelyId)
                .Distinct()
                .ToList();

            //párkapcsolatban élő férfiak akik a családfa tagjai
            var ferfiHazasCsaladtagok = kapcsolatok
                .Where(k => k.KapcsolatTipusa == "Partner")
                .Where(k => kapcsolatok.Any(gy => gy.KapcsolodoSzemelyId == k.KapcsolodoSzemelyId && gy.KapcsolatTipusa == "Gyermek"))
                .Select(k => k.KapcsolodoSzemelyId)
                .Distinct()
                .ToList();



            var nemKapcsSzemelyek = szemelyek
                .Where(s => !kapcsolatok
                .Any(k => k.SzemelyId == s.Azonosito || k.KapcsolodoSzemelyId == s.Azonosito &&
                  (k.KapcsolatTipusa == "Partner" || k.KapcsolatTipusa == "Gyermek")))
                .Select(s => s.Azonosito)
                .ToList();

            if (anyak != null)
            {
                foreach (var anyaId in anyak)
                {
                    if (!szemelyDict.TryGetValue(anyaId, out var anya)) continue;

                    // Amelyik anyánál van gyerek, ott kellett partner is, megkeressük
                    var partnerKapcsolat = kapcsolatok.FirstOrDefault(k => k.SzemelyId == anyaId && k.KapcsolatTipusa == "Partner");
                    RajzoltSzemely partner = szemelyDict[partnerKapcsolat.KapcsolodoSzemelyId];
                    // szemelyDict.ContainsKey(partnerKapcsolat.KapcsolodoSzemelyId))


                    // Szülők kirajzolása
                    var anyaBorder = RajzolSzemelyt(anya, xKezdo, yKezdo);
                    szemelyBorderDictionary[anya.Azonosito] = anyaBorder;
                    anya.UIElem = anyaBorder;

                    //Automatikusan kiválasztjuk az utolsót
                    if (utolso != null && anya.Azonosito == utolso.Azonosito)
                    {
                        anyaBorder.BorderBrush = Brushes.OrangeRed;
                        kijeloltBorder = anyaBorder;

                        var rajzolt = anyaBorder.Tag as RajzoltSzemely;

                        // 👉 Itt beállítjuk manuálisan, ha nem történténne kattintás
                        LegutobbKijeloltSzemely = rajzolt;

                        // Aktuális KezdoTartalomControl elérése
                        if (TartalomValto.Content is KezdoTartalomControl aktivTartalom)
                        {
                            aktivTartalom.DataContext = anya;
                        }
                    }



                    var partnerBorder = RajzolSzemelyt(partner, xKezdo + xTav, yKezdo);
                    szemelyBorderDictionary[partner.Azonosito] = partnerBorder;
                    partner.UIElem = partnerBorder;

                    //Automatikusan kiválasztjuk az utolsót
                    if (utolso != null && partner.Azonosito == utolso.Azonosito)
                    {
                        partnerBorder.BorderBrush = Brushes.OrangeRed;
                        kijeloltBorder = partnerBorder;

                        var rajzolt = partnerBorder.Tag as RajzoltSzemely;

                        // 👉 Itt beállítjuk manuálisan, ha nem történténne kattintás
                        LegutobbKijeloltSzemely = rajzolt;

                        // Aktuális KezdoTartalomControl elérése
                        if (TartalomValto.Content is KezdoTartalomControl aktivTartalom)
                        {
                            aktivTartalom.DataContext = partner;
                        }
                    }




                    // Szülők közös vonala
                    RajzolVonalat(
                        Canvas.GetLeft(anyaBorder) + anyaBorder.ActualWidth + 105,
                        Canvas.GetTop(anyaBorder) + anyaBorder.ActualHeight + 100,
                        Canvas.GetLeft(partnerBorder) + partnerBorder.ActualWidth / 2,
                        Canvas.GetTop(partnerBorder) + partnerBorder.ActualHeight + 100
                    );

                    // Középpont kiszámítása
                    double szulokKozepX = (Canvas.GetLeft(anyaBorder) + anyaBorder.ActualWidth + 110 + Canvas.GetLeft(partnerBorder)) / 2;
                    double szulokKozepY = Canvas.GetTop(anyaBorder) + anyaBorder.ActualHeight + 105;

                    // Gyermekek kirajzolása
                    var gyerekek = kapcsolatok
                        .Where(k => k.SzemelyId == anyaId && k.KapcsolatTipusa == "Gyermek")
                        .Select(k => k.KapcsolodoSzemelyId)
                        .Where(id => szemelyDict.ContainsKey(id))
                        .Select(id => szemelyDict[id])
                        .ToList();

                    double aktualisGyerekX = xKezdo;

                    foreach (var gyerek in gyerekek)
                    {
                        var gyerekBorder = RajzolSzemelyt(gyerek, aktualisGyerekX, yKezdo + yTav);
                        szemelyBorderDictionary[gyerek.Azonosito] = gyerekBorder;
                        gyerek.UIElem = gyerekBorder;

                        //Automatikusan kiválasztjuk az utolsót
                        if (utolso != null && gyerek.Azonosito == utolso.Azonosito)
                        {
                            gyerekBorder.BorderBrush = Brushes.OrangeRed;
                            kijeloltBorder = gyerekBorder;

                            var rajzolt = gyerekBorder.Tag as RajzoltSzemely;

                            // 👉 Itt beállítjuk manuálisan, ha nem történténne kattintás
                            LegutobbKijeloltSzemely = rajzolt;

                            // Aktuális KezdoTartalomControl elérése
                            if (TartalomValto.Content is KezdoTartalomControl aktivTartalom)
                            {
                                aktivTartalom.DataContext = gyerek;
                            }
                        }


                        // Gyerekeket felfelé kötjük a szülői középponthoz
                        RajzolVonalat(
                            Canvas.GetLeft(gyerekBorder) + gyerekBorder.ActualWidth + 60,
                            Canvas.GetTop(gyerekBorder),
                            szulokKozepX,
                            szulokKozepY
                        );

                        aktualisGyerekX += gyerekXTav;
                    }

                    // Elcsúsztatjuk a következő párt
                    

                    xKezdo = aktualisGyerekX + 400;



                }
            }

            //ezek a nők nem családtagok,nincs gyerekük de párkapcsolatban vannak
            if (nokCsakKapcsolatban != null)
            {
                foreach (var noId in nokCsakKapcsolatban)
                {
                    if (!szemelyDict.TryGetValue(noId, out var noCsakParral)) continue;

                    //megkerestük a rekordokat amik ezek a nőkhöz tartoznak
                    var partnerKapcsolat = kapcsolatok.FirstOrDefault(k => k.SzemelyId == noId && k.KapcsolatTipusa == "Partner");
                    RajzoltSzemely partner = szemelyDict[partnerKapcsolat.KapcsolodoSzemelyId];

                    // Meg kell nézni, hogy a nő családtag-e

                    bool noCsaladtag = kapcsolatok.Any(gy => gy.KapcsolodoSzemelyId == noId && gy.KapcsolatTipusa == "Gyermek");
                   

                    //meg kell nézni, hogy a férfi partner családtag-e vagy nem kapcsolódó személy volt.
                     bool ferfiCsakKapcsolatban = false;
                     foreach (var partnerId in ferfiakCsakKapcsolatban)
                     {
                         if (partnerKapcsolat.KapcsolodoSzemelyId == partnerId)
                         {
                             ferfiCsakKapcsolatban = true;
                             break;
                         }
                     }
                     //Nő az családtag de férfi nem
                    if (noCsaladtag && ferfiCsakKapcsolatban == true)
                    { }
                    //Férfi az családtag de a nő nem
                    else if (!noCsaladtag && ferfiCsakKapcsolatban == false)
                    {
                    }
                    //nem családtag, nem gyerekes párok kirajzolása
                    else if (ferfiCsakKapcsolatban == true && !noCsaladtag)
                    {
                        // Szülők kirajzolása
                        var noCsakParralBorder = RajzolSzemelyt(noCsakParral, xKezdo, yKezdo);
                        szemelyBorderDictionary[noCsakParral.Azonosito] = noCsakParralBorder;
                        noCsakParral.UIElem = noCsakParralBorder;

                        //Automatikusan kiválasztjuk az utolsót
                        if (utolso != null && noCsakParral.Azonosito == utolso.Azonosito)
                        {
                            noCsakParralBorder.BorderBrush = Brushes.OrangeRed;
                            kijeloltBorder = noCsakParralBorder;

                            var rajzolt = noCsakParralBorder.Tag as RajzoltSzemely;

                            // 👉 Itt beállítjuk manuálisan, ha nem történténne kattintás
                            LegutobbKijeloltSzemely = rajzolt;

                            // Aktuális KezdoTartalomControl elérése
                            if (TartalomValto.Content is KezdoTartalomControl aktivTartalom)
                            {
                                aktivTartalom.DataContext = noCsakParral;
                            }
                        }



                        var partnerBorder = RajzolSzemelyt(partner, xKezdo + xTav, yKezdo);
                        szemelyBorderDictionary[partner.Azonosito] = partnerBorder;
                        partner.UIElem = partnerBorder;

                        //Automatikusan kiválasztjuk az utolsót
                        if (utolso != null && partner.Azonosito == utolso.Azonosito)
                        {
                            partnerBorder.BorderBrush = Brushes.OrangeRed;
                            kijeloltBorder = partnerBorder;

                            var rajzolt = partnerBorder.Tag as RajzoltSzemely;

                            // 👉 Itt beállítjuk manuálisan, ha nem történténne kattintás
                            LegutobbKijeloltSzemely = rajzolt;

                            // Aktuális KezdoTartalomControl elérése
                            if (TartalomValto.Content is KezdoTartalomControl aktivTartalom)
                            {
                                aktivTartalom.DataContext = partner;
                            }
                        }




                        // Szülők közös vonala
                        RajzolVonalat(
                            Canvas.GetLeft(noCsakParralBorder) + noCsakParralBorder.ActualWidth + 105,
                            Canvas.GetTop(noCsakParralBorder) + noCsakParralBorder.ActualHeight + 100,
                            Canvas.GetLeft(partnerBorder) + partnerBorder.ActualWidth / 2,
                            Canvas.GetTop(partnerBorder) + partnerBorder.ActualHeight + 100
                        );

                        xKezdo += 400;
                    }
                    else
                    {

                    }
                }
            }
                //nem kapcsolódó személyek alapértelmezett megjelenítése
            if (nemKapcsSzemelyek != null)
            {
                foreach (var nemKapcsSzemelyId in nemKapcsSzemelyek)
                {
                    if (!szemelyDict.TryGetValue(nemKapcsSzemelyId, out var nemKapcsSzemely)) continue;
                    var nemKapcsSzemelyBorder = RajzolSzemelyt(nemKapcsSzemely, xKezdo, yKezdo);
                    szemelyBorderDictionary[nemKapcsSzemely.Azonosito] = nemKapcsSzemelyBorder;
                    nemKapcsSzemely.UIElem = nemKapcsSzemelyBorder;

                    //Automatikusan kiválasztjuk az utolsót
                    if (utolso != null && nemKapcsSzemely.Azonosito == utolso.Azonosito)
                    {
                        nemKapcsSzemelyBorder.BorderBrush = Brushes.OrangeRed;
                        kijeloltBorder = nemKapcsSzemelyBorder;

                        var rajzolt = nemKapcsSzemelyBorder.Tag as RajzoltSzemely;

                        // 👉 Itt beállítjuk manuálisan, ha nem történténne kattintás
                        LegutobbKijeloltSzemely = rajzolt;

                        // Aktuális KezdoTartalomControl elérése
                        if (TartalomValto.Content is KezdoTartalomControl aktivTartalom)
                        {
                            aktivTartalom.DataContext = nemKapcsSzemely;
                        }
                    }
                    xKezdo += 400;
                }
            }
            

                   
        }




        private List<Kapcsolatok> BetoltKapcsolatok()
        {
            var kapcsolatok = _context.Kapcsolatoks
                .ToList(); // Szűrés FelhasznaloId szerint ha szükséges
            foreach (var kapcsolat in kapcsolatok)
            {
                if (kapcsolat.KapcsolatTipusa == "Partner")
                {
                    // A nő SzemelyId-ja kulcs, a férfi KapcsolodoSzemelyId-ja az érték
                    if (!partnerMap.ContainsKey(kapcsolat.SzemelyId))
                    {
                        partnerMap.Add(kapcsolat.SzemelyId, kapcsolat.KapcsolodoSzemelyId);
                    }
                }
            }
            return kapcsolatok;
        }


        private void RajzolKapcsolatVonalakat()
        {
            // Feltételezzük, hogy minden RajzoltSzemely objektum ismeri a generációját (Generacio),
            // a Canvas-on lévő UI elemét (UIElem), és hogy van partner-kapcsolat listánk partnerMap.
            foreach (var szulo in RajzoltSzemelyek)
            {
                // Csak akkor foglalkozunk vele, ha vannak gyermekei
                if (szulo.GyermekAzonositoLista.Any())
                {
                    // Szülők középpontjának X koordinátája (ha van partner, a kettő közé, ha nincs, akkor a saját közép)
                    double szuloKozepX;
                    double szuloTopY = Canvas.GetTop(szulo.UIElem);
                    double szuloBottomY = szuloTopY + szulo.UIElem.RenderSize.Height;
                    if (partnerMap.ContainsKey(szulo.Azonosito))
                    {
                        // Ha van partner, vegyük a két partner border X középpontját
                        var partner = RajzoltSzemelyek.First(s => s.Azonosito == partnerMap[szulo.Azonosito]);
                        double partnerLeftX = Canvas.GetLeft(partner.UIElem);
                        double szuloLeftX = Canvas.GetLeft(szulo.UIElem);
                        // Középpontok kiszámítása (a border szélességének felét hozzáadva)
                        double szuloCenterX = szuloLeftX + szulo.UIElem.RenderSize.Width / 2;
                        double partnerCenterX = partnerLeftX + partner.UIElem.RenderSize.Width / 2;
                        szuloKozepX = (szuloCenterX + partnerCenterX) / 2;
                        // Vízszintes vonal a két szülő között
                        RajzolVonalat(szuloCenterX, szuloBottomY + 5, partnerCenterX, szuloBottomY + 5);
                    }
                    else
                    {
                        // Nincs partner, a saját border középpontját vesszük
                        double szuloLeftX = Canvas.GetLeft(szulo.UIElem);
                        szuloKozepX = szuloLeftX + szulo.UIElem.RenderSize.Width / 2;
                    }

                    // Gyermekek koordinátáinak lekérése
                    var gyerekek = szulo.GyermekAzonositoLista
                                    .Select(id => RajzoltSzemelyek.First(s => s.Azonosito == id))
                                    .ToList();
                    // Gyerekek középpontjainak X koordinátái
                    var gyerekCenterXs = gyerekek.Select(gy => Canvas.GetLeft(gy.UIElem) + gy.UIElem.RenderSize.Width / 2).ToList();
                    // Gyerekek top (felső) Y koordinátái (mind azonos generációban vannak, tehát elvileg egyforma Y)
                    double gyerekTopY = Canvas.GetTop(gyerekek.First().UIElem);
                    double gyerekCenterY = gyerekTopY + gyerekek.First().UIElem.RenderSize.Height / 2;

                    if (gyerekek.Count == 1)
                    {
                        // Egyetlen gyerek - közvetlen függőleges vonal a szülő(k) középpontjától a gyerek középpontjáig
                        RajzolVonalat(szuloKozepX, szuloBottomY + 5, gyerekCenterXs[0], gyerekCenterY);
                    }
                    else
                    {
                        // Több gyerek esetén testvér-vízszintes vonal és függőleges ágak
                        double minX = gyerekCenterXs.Min();
                        double maxX = gyerekCenterXs.Max();
                        // Vízszintes vonal a testvérek között (a szülő és gyerekek között félúton, pl. szülő alsó Y + 50)
                        double siblingsY = szuloBottomY + ((gyerekTopY - szuloBottomY) / 2);
                        RajzolVonalat(minX, siblingsY, maxX, siblingsY);
                        // Függőleges vonal a szülőpár közepétől a testvéreket összekötő vonalig
                        RajzolVonalat(szuloKozepX, szuloBottomY + 5, szuloKozepX, siblingsY);
                        // Függőleges vonalak a testvér-vonaltól minden egyes gyerekhez
                        foreach (double childCenterX in gyerekCenterXs)
                        {
                            RajzolVonalat(childCenterX, siblingsY, childCenterX, gyerekCenterY);
                        }
                    }
                }
            }

        }
        private Dictionary<int, int> GeneralPartnerMap()
        {
            var partnerKapcsolatok = _context.Kapcsolatoks
                .Where(k => k.KapcsolatTipusa == "Partner")
                .ToList();

            var map = new Dictionary<int, int>();

            foreach (var kapcsolat in partnerKapcsolatok)
            {
                // Kapcsolatokban mindig a nő az SzemelyId, férfi a KapcsolodoSzemelyId!
                if (!map.ContainsKey(kapcsolat.SzemelyId))
                {
                    map.Add(kapcsolat.SzemelyId, kapcsolat.KapcsolodoSzemelyId);
                }
            }

            return map;
        }


        private void SzamoldKiGeneraciokat(List<RajzoltSzemely> rajzoltSzemelyek, List<Kapcsolatok> kapcsolatok)
        {
            // Alaphelyzet: mindenki a legalsó szinten
            foreach (var szemely in rajzoltSzemelyek)
            {
                szemely.GeneracioSzint = 0;
            }

            // Addig próbálkozunk, amíg már nincs változás
            bool valtozasTortent;
            do
            {
                valtozasTortent = false;
                foreach (var kapcsolat in kapcsolatok)
                {
                    var gyermek = rajzoltSzemelyek.FirstOrDefault(x => x.Azonosito == kapcsolat.KapcsolodoSzemelyId);
                    var szulo = rajzoltSzemelyek.FirstOrDefault(x => x.Azonosito == kapcsolat.SzemelyId);

                    if (szulo != null && gyermek != null)
                    {
                        int elvartGyermekSzint = szulo.GeneracioSzint + 1;
                        if (gyermek.GeneracioSzint < elvartGyermekSzint)
                        {
                            gyermek.GeneracioSzint = elvartGyermekSzint;
                            valtozasTortent = true;
                        }
                    }
                }
            }
            while (valtozasTortent);
        }



        private void Kijelentkezes_Click(object sender, RoutedEventArgs e)
        {

            // Navigálás a BejelentkezesPage oldalra.
            ((MainWindow)System.Windows.Application.Current.MainWindow).MainFrame.Navigate(new BejelentkezesPage(_context));
        }
    }
}

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
using System.Diagnostics.Eventing.Reader;


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
        private Dictionary<int, int> GeneracioSzintek = new();
        private Dictionary<int, int> partnerMapNoknek = new();
        private Dictionary<int, int> partnerMapFerfiaknak = new();
        private Dictionary<int?, double> xtavolsagMapElsoGyerekeknek = new();
        private const double BoxWidth = 100;
        private const double BoxHeight = 100;
        private const double SpacingX = 50;
        private const double SpacingY = 100;



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
                Text = szemely.VNev + " " + szemely.KNev,
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
                    GyermekekSzama = 0,
                    Apa = null,
                    Anya = null,
                    ApaRajzolElobb = null,
                    NotRajzoljunkElobbParbol = null,
                    GyermekAzIlleto=false,
                    ELsoGyerek = false,
                    ELsoGyerekId = 0
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
            var szemelyek = BetoltSzemelyekAdatbazisbol();//rajzolt személyek listája
            var kapcsolatok = BetoltKapcsolatok();//betöltsük a kapcsolatok rekordokat a kapcsolatok táblából listába
            var utolso = szemelyek.OrderByDescending(s => s.Azonosito).FirstOrDefault();


            RajzoltSzemelyek = szemelyek;

            double xKezdo = 50;
            double yKezdo = 50;
            double xTav = 150;
            double yTav = 150;

            double gyerekXTav = 100;



            // 👉 Először kiszámoljuk a generációkat (fontos!)
            int generaciokSzama = SzamoldKiGeneraciokat(szemelyek, kapcsolatok);

            //MessageBox.Show($"Generációk: {generaciokSzama}");
            List<RajzoltSzemely>[] generaciosListakTomb = new List<RajzoltSzemely>[generaciokSzama];//tömb amely generációk szerint tárolja a rajzoltszemélyes listákat


            //végigmegyünk az összes generáción, külön generációk szerint listákba gyűjtjük a rajzolt személyeket és ezeket a listákat egyesítjük egy nagy tömbben.
            for (int i = 0; i < generaciokSzama; i++)
            {
                List<RajzoltSzemely> RajzolandoSzemelyek = new List<RajzoltSzemely>();
                foreach (var szemely in szemelyek)
                {
                    if (i == szemely.GeneracioSzint)
                    {

                        RajzolandoSzemelyek.Add(szemely);

                    }
                }
                generaciosListakTomb[i] = RajzolandoSzemelyek;//eltároljuk a listában az egyik egy generációhoz tartozó listában található személyeket

            }

            

            //mielőtt kirajzolunk, rendezni kell a generációs listákat a megfelelő kirajzoláshoz
            //először szülő párosokat tároljuk, utána a, párokat, utána a szingliket


            Dictionary<int, RajzoltSzemely> szemelyDict = szemelyek.ToDictionary(s => s.Azonosito);

            // 👉 Női személyek, akinek van párja vagy gyermeke
            var anyak = kapcsolatok
                .Where(k => k.KapcsolatTipusa == "Gyermek")
                .Select(k => k.SzemelyId)
                .Distinct()
                .ToList();

            //nincs gyerekük de párkapolatban lévő nem családtag nők, nem családtag férfiakkal
            var nokCsakKapcsolatban = kapcsolatok
                .Where(k => k.KapcsolatTipusa == "Partner")
                .Where(k => !kapcsolatok.Any(gy => gy.SzemelyId == k.SzemelyId && gy.KapcsolatTipusa == "Gyermek"))
                .Where(k => !kapcsolatok.Any(gy => gy.KapcsolodoSzemelyId == k.SzemelyId && gy.KapcsolatTipusa == "Gyermek"))
                .Where(k => !kapcsolatok.Any(p =>
                        p.SzemelyId == k.SzemelyId &&
                        p.KapcsolodoSzemelyId == k.KapcsolodoSzemelyId &&
                        p.KapcsolatTipusa == "Partner" &&
                        kapcsolatok.Any(gy =>
                        gy.KapcsolodoSzemelyId == p.KapcsolodoSzemelyId &&
                        gy.KapcsolatTipusa == "Gyermek"))
                        )
                .Select(k => k.SzemelyId)
                .Distinct()
                .ToList();

            //párkapcsolatban lévő gyermektelen női családtagok
            var hazasNoiCsaladtagok = kapcsolatok
                .Where(k => k.KapcsolatTipusa == "Partner")
                .Where(k => !kapcsolatok.Any(gy => gy.SzemelyId == k.SzemelyId && gy.KapcsolatTipusa == "Gyermek"))
                .Where(k => kapcsolatok.Any(gy => gy.KapcsolodoSzemelyId == k.SzemelyId && gy.KapcsolatTipusa == "Gyermek"))
                .Select(k => k.SzemelyId)
                .Distinct()
                .ToList();
            

            var nemKapcsSzemelyek = szemelyek
                .Where(s => !kapcsolatok
                .Any(k => k.SzemelyId == s.Azonosito || k.KapcsolodoSzemelyId == s.Azonosito &&
                  (k.KapcsolatTipusa == "Partner" || k.KapcsolatTipusa == "Gyermek")))
                .Select(s => s.Azonosito)
                .ToList();

            List<RajzoltSzemely> gyerekLista = new List<RajzoltSzemely>();//A családfában aktuális generációba tartozó szülők gyerekeinek listája

            for (int i = 0; i < generaciokSzama; i++)
            {
                List<RajzoltSzemely> rendezettGeneraciosLista = new List<RajzoltSzemely>();

                if (i == 0)
                {
                    foreach (var szemely in generaciosListakTomb[i])
                    {
                        bool szemelyMegtalalva = false;
                        if (anyak != null && szemelyMegtalalva == false)
                        {
                            foreach (var anyaId in anyak)
                            {

                                if (anyaId == szemely.Azonosito)
                                {
                                    if (!szemelyDict.TryGetValue(anyaId, out var anya)) continue;
                                    // Amelyik anyánál van gyerek, ott kellett partner is, megkeressük
                                    var partnerKapcsolat = kapcsolatok.FirstOrDefault(k => k.SzemelyId == anyaId && k.KapcsolatTipusa == "Partner");
                                    RajzoltSzemely partner = null;
                                    if (szemelyDict.ContainsKey(partnerKapcsolat.KapcsolodoSzemelyId))
                                    {
                                        partner = szemelyDict[partnerKapcsolat.KapcsolodoSzemelyId]; // Biztonságosan lekérjük az objektumot
                                    }

                                    rendezettGeneraciosLista.Add(anya);
                                    rendezettGeneraciosLista.Add(partner);

                                    anya.NotRajzoljunkElobbParbol = true;

                                    //Anyuka gyermekei
                                    var gyerekek = kapcsolatok
                                        .Where(k => k.SzemelyId == anyaId && k.KapcsolatTipusa == "Gyermek")
                                        .Select(k => k.KapcsolodoSzemelyId)
                                        .ToList();

                                    anya.GyermekekSzama = gyerekek.Count;
                                    partner.GyermekekSzama = gyerekek.Count;

                                    var rendezettGyerekek = gyerekek
                                        .OrderBy(x => x)
                                        .ToList();

                                    var legelsoGyerekId = rendezettGyerekek
                                        .FirstOrDefault();

                                    anya.ELsoGyerekId = legelsoGyerekId;

                                    foreach (var gyerekId in rendezettGyerekek)
                                    {
                                        if (!szemelyDict.TryGetValue(gyerekId, out var gyerek)) continue;
                                        gyerekLista.Add(gyerek);

                                        gyerek.Anya = anya;
                                        gyerek.Apa = partner;
                                        gyerek.ApaRajzolElobb = false;
                                        gyerek.GyermekAzIlleto = true;
                                        if (gyerek.Azonosito == legelsoGyerekId)
                                            gyerek.ELsoGyerek = true;

                                        
                                    }

                                    szemelyMegtalalva = true;
                                    break;
                                }


                                // Szülők közös vonala
                                /* RajzolVonalat(
                                     Canvas.GetLeft(anyaBorder) + anyaBorder.ActualWidth + 105,
                                     Canvas.GetTop(anyaBorder) + anyaBorder.ActualHeight + 100,
                                     Canvas.GetLeft(partnerBorder) + partnerBorder.ActualWidth / 2,
                                     Canvas.GetTop(partnerBorder) + partnerBorder.ActualHeight + 100
                                 );

                                 // Középpont kiszámítása
                                 double szulokKozepX = (Canvas.GetLeft(anyaBorder) + anyaBorder.ActualWidth + 110 + Canvas.GetLeft(partnerBorder)) / 2;
                                 double szulokKozepY = Canvas.GetTop(anyaBorder) + anyaBorder.ActualHeight + 105;
                                */


                                //.Where(id => szemelyDict.ContainsKey(id))
                                //.Select(id => szemelyDict[id])
                                /* double aktualisGyerekX = xKezdo;

                                 foreach (var gyerek in gyerekek)
                                 {


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

                            }*/
                            }

                            if (nokCsakKapcsolatban != null && szemelyMegtalalva == false)
                            {
                                foreach (var noId in nokCsakKapcsolatban)
                                {
                                    if (noId == szemely.Azonosito)
                                    {
                                        if (!szemelyDict.TryGetValue(noId, out var noCsakParral)) continue;

                                        //megkerestük a rekordokat amik ezek a nőkhöz tartoznak
                                        var partnerKapcsolat = kapcsolatok.FirstOrDefault(k => k.SzemelyId == noId && k.KapcsolatTipusa == "Partner");
                                        RajzoltSzemely partner = null;
                                        if (szemelyDict.ContainsKey(partnerKapcsolat.KapcsolodoSzemelyId))
                                        {
                                            partner = szemelyDict[partnerKapcsolat.KapcsolodoSzemelyId]; // Biztonságosan lekérjük az objektumot
                                        }

                                        rendezettGeneraciosLista.Add(noCsakParral);
                                        rendezettGeneraciosLista.Add(partner);
                                        noCsakParral.NotRajzoljunkElobbParbol = true;
                                        szemelyMegtalalva = true;
                                        break;
                                    }

                                }
                            }
                            if (nemKapcsSzemelyek != null && szemelyMegtalalva == false)
                            {
                                foreach (var nemKapcsSzemelyId in nemKapcsSzemelyek)
                                {
                                    if (nemKapcsSzemelyId == szemely.Azonosito)
                                    {
                                        if (!szemelyDict.TryGetValue(nemKapcsSzemelyId, out var nemKapcsSzemely)) continue;
                                        rendezettGeneraciosLista.Add(nemKapcsSzemely);
                                        szemelyMegtalalva = true;
                                        break;
                                    }



                                }
                            }

                        }
                    }
                }
                List<RajzoltSzemely> ujGyerekLista = new List<RajzoltSzemely>();
                if (i > 0 && gyerekLista.Count != 0)//innentől kezdve akit ábrázolunk majd, mindenki mindenki valakinek a gyereke, vagy a családfában egy személynek a párja
                {

                    //végigmegyünk a gyereklistán plussz megnézzük van-e a személyeknek párja gyereke
                    foreach (var szemely in gyerekLista)
                    {
                        bool szemelyMegtalalva = false;
                        if (anyak != null && szemelyMegtalalva == false)
                        {
                            foreach (var anyaId in anyak)
                            {

                                if (anyaId == szemely.Azonosito)
                                {
                                    if (!szemelyDict.TryGetValue(anyaId, out var anya)) continue;
                                    // Amelyik anyánál van gyerek, ott kellett partner is, megkeressük
                                    var partnerKapcsolat = kapcsolatok.FirstOrDefault(k => k.SzemelyId == anyaId && k.KapcsolatTipusa == "Partner");
                                    RajzoltSzemely partner = null;
                                    if (szemelyDict.ContainsKey(partnerKapcsolat.KapcsolodoSzemelyId))
                                    {
                                        partner = szemelyDict[partnerKapcsolat.KapcsolodoSzemelyId]; // Biztonságosan lekérjük az objektumot
                                    }

                                    rendezettGeneraciosLista.Add(anya);
                                    rendezettGeneraciosLista.Add(partner);

                                    anya.NotRajzoljunkElobbParbol = true;

                                    //Anyuka gyermekei
                                    var gyerekek = kapcsolatok
                                        .Where(k => k.SzemelyId == anyaId && k.KapcsolatTipusa == "Gyermek")
                                        .Select(k => k.KapcsolodoSzemelyId)
                                        .ToList();

                                    anya.GyermekekSzama = gyerekek.Count;
                                    partner.GyermekekSzama = gyerekek.Count;

                                    var rendezettGyerekek = gyerekek
                                      .OrderBy(x => x)
                                      .ToList();

                                    var legelsoGyerekId = rendezettGyerekek
                                        .FirstOrDefault();

                                    anya.ELsoGyerekId = legelsoGyerekId;

                                    foreach (var gyerekId in rendezettGyerekek)
                                    {
                                        if (!szemelyDict.TryGetValue(gyerekId, out var gyerek)) continue;
                                        ujGyerekLista.Add(gyerek);//itt nem a régi hanem az ujgyereklistahoz adunk

                                        gyerek.Anya = anya;
                                        gyerek.Apa = partner;
                                        gyerek.ApaRajzolElobb = false;
                                        gyerek.GyermekAzIlleto = true;
                                        if (gyerek.Azonosito == legelsoGyerekId)
                                            gyerek.ELsoGyerek = true;
                                    }

                                    szemelyMegtalalva = true;
                                    break;
                                }
                            }
                        }
                        if (hazasNoiCsaladtagok != null && szemelyMegtalalva == false)
                        {
                            foreach (var noId in hazasNoiCsaladtagok)
                            {
                                if (noId == szemely.Azonosito)
                                {
                                    if (!szemelyDict.TryGetValue(noId, out var CsakHazasNo)) continue;

                                    //megkerestük a rekordokat amik ezek a nőkhöz tartoznak
                                    var partnerKapcsolat = kapcsolatok.FirstOrDefault(k => k.SzemelyId == noId && k.KapcsolatTipusa == "Partner");
                                    RajzoltSzemely partner = null;
                                    if (szemelyDict.ContainsKey(partnerKapcsolat.KapcsolodoSzemelyId))
                                    {
                                        partner = szemelyDict[partnerKapcsolat.KapcsolodoSzemelyId]; // Biztonságosan lekérjük az objektumot
                                    }

                                    rendezettGeneraciosLista.Add(CsakHazasNo);
                                    rendezettGeneraciosLista.Add(partner);
                                    CsakHazasNo.NotRajzoljunkElobbParbol = true;
                                    szemelyMegtalalva = true;
                                    break;

                                }

                            }
                        }


                        //párkapcsolatban élő férfi akik a családfa tagjai de nincs gyerekük
                        var ferfiHazasCsaladtagok = kapcsolatok
                        // 1. Csak olyan rekordok, ahol a férfi a kapcsolódó személy
                        .Where(k => k.KapcsolatTipusa == "Partner")
                        .Select(k => k.KapcsolodoSzemelyId)
                        .Distinct()
                        .Where(k =>
                        // A férfi családtag (szerepel valakinek gyermekeként)
                        kapcsolatok.Any(k =>
                            k.KapcsolodoSzemelyId == szemely.Azonosito &&
                            k.KapcsolatTipusa == "Gyermek") &&

                        // Van partnere (nő, azaz olyan kapcsolat, ahol ő a KapcsolodoSzemelyId és partner)
                        kapcsolatok.Any(k =>
                            k.KapcsolodoSzemelyId == szemely.Azonosito &&
                            k.KapcsolatTipusa == "Partner") &&

                        // De NINCS gyermek rekord a partneréhez rendelve
                        !kapcsolatok.Any(p =>
                        p.KapcsolodoSzemelyId == szemely.Azonosito &&
                        p.KapcsolatTipusa == "Partner" &&
                        kapcsolatok.Any(gy =>
                        gy.SzemelyId == p.SzemelyId &&
                        gy.KapcsolatTipusa == "Gyermek"))
                        )
                        .ToList();


                        //házas férfi családtagok
                        if (ferfiHazasCsaladtagok != null && szemelyMegtalalva == false)
                        {
                            foreach (var ferfiId in ferfiHazasCsaladtagok)
                            {
                                if (ferfiId == szemely.Azonosito)
                                {
                                    if (!szemelyDict.TryGetValue(ferfiId, out var CsakHazasFerfi)) continue;

                                    //megkerestük a rekordokat amik ehhez a férfihoz tartoznak
                                    var partnerKapcsolat = kapcsolatok.FirstOrDefault(k => k.KapcsolodoSzemelyId == ferfiId && k.KapcsolatTipusa == "Partner");
                                   
                                    RajzoltSzemely partner = null;
                                    if (szemelyDict.ContainsKey(partnerKapcsolat.SzemelyId))
                                    {
                                        partner = szemelyDict[partnerKapcsolat.SzemelyId]; // Biztonságosan lekérjük az objektumot
                                    }

                                    rendezettGeneraciosLista.Add(partner);
                                    rendezettGeneraciosLista.Add(CsakHazasFerfi);
                                    CsakHazasFerfi.NotRajzoljunkElobbParbol = false;
                                    szemelyMegtalalva = true;
                                    break;

                                }

                            }
                        }

                        var ferfiHazasCsaladtagokGyerekkel = kapcsolatok
                            // 1. Csak olyan rekordok, ahol a férfi a kapcsolódó személy
                            .Where(k => k.KapcsolatTipusa == "Partner")
                            .Select(k => k.KapcsolodoSzemelyId)
                            .Distinct()
                            .Where(k =>
                            // A férfi családtag (szerepel valakinek gyermekeként)
                            kapcsolatok.Any(k =>
                                k.KapcsolodoSzemelyId == szemely.Azonosito &&
                                k.KapcsolatTipusa == "Gyermek") &&

                            // Van partnere (nő, azaz olyan kapcsolat, ahol ő a KapcsolodoSzemelyId és partner)
                            kapcsolatok.Any(k =>
                                k.KapcsolodoSzemelyId == szemely.Azonosito &&
                                k.KapcsolatTipusa == "Partner") &&

                            // De VAN  gyermek rekord a partneréhez rendelve
                            kapcsolatok.Any(p =>
                            p.KapcsolodoSzemelyId == szemely.Azonosito &&
                            p.KapcsolatTipusa == "Partner" &&
                            kapcsolatok.Any(gy =>
                            gy.SzemelyId == p.SzemelyId &&
                            gy.KapcsolatTipusa == "Gyermek"))
                            )
                            .ToList();


                        //házas férfi családtagok van gyerekük is
                        if (ferfiHazasCsaladtagokGyerekkel != null && szemelyMegtalalva == false)
                        {
                            foreach (var ferfiId in ferfiHazasCsaladtagokGyerekkel)
                            {
                                if (ferfiId == szemely.Azonosito)
                                {
                                    if (!szemelyDict.TryGetValue(ferfiId, out var apa)) continue;

                                    //megkerestük a rekordokat amik ehhez a férfihoz tartoznak
                                    var partnerKapcsolat = kapcsolatok.FirstOrDefault(k => k.KapcsolodoSzemelyId == ferfiId && k.KapcsolatTipusa == "Partner");
                                    RajzoltSzemely partner = null;
                                    if (szemelyDict.ContainsKey(partnerKapcsolat.SzemelyId))
                                    {
                                        partner = szemelyDict[partnerKapcsolat.SzemelyId]; // Biztonságosan lekérjük az objektumot
                                    }

                                    rendezettGeneraciosLista.Add(partner);
                                    rendezettGeneraciosLista.Add(apa);
                                    apa.NotRajzoljunkElobbParbol = false;


                                    //Anyuka gyermekei
                                    var gyerekek = kapcsolatok
                                        .Where(k => k.SzemelyId == partner.Azonosito && k.KapcsolatTipusa == "Gyermek")
                                        .Select(k => k.KapcsolodoSzemelyId)
                                        .ToList();
                                    apa.GyermekekSzama = gyerekek.Count;
                                    partner.GyermekekSzama = gyerekek.Count;

                                    var rendezettGyerekek = gyerekek
                                      .OrderBy(x => x)
                                      .ToList();

                                    var legelsoGyerekId = rendezettGyerekek
                                        .FirstOrDefault();

                                    apa.ELsoGyerekId = legelsoGyerekId;

                                    foreach (var gyerekId in rendezettGyerekek)
                                    {
                                        if (!szemelyDict.TryGetValue(gyerekId, out var gyerek)) continue;
                                        ujGyerekLista.Add(gyerek);//itt nem a régi hanem az ujgyereklistahoz adunk
                                        gyerek.Apa = apa;
                                        gyerek.Anya = partner;
                                        gyerek.ApaRajzolElobb = true;
                                        gyerek.GyermekAzIlleto = true;
                                        if (gyerek.Azonosito == legelsoGyerekId)
                                            gyerek.ELsoGyerek = true;
                                    }

                                    szemelyMegtalalva = true;
                                    break;

                                }

                            }
                        }

                        //önállóan gyerek
                        if (szemelyMegtalalva == false)
                        {
                            rendezettGeneraciosLista.Add(szemely);
                        }


                    }

                    
                    // miután végigértünk mindenkin és hozzáadtuk a tömbhoz régigyereklista=uj gyereklista ha nem üres
                    if (ujGyerekLista.Count != 0)
                        gyerekLista = ujGyerekLista;

                }
                generaciosListakTomb[i] = rendezettGeneraciosLista;



            }
                // jöhet  kirajzolása
                for (int i = 0; i < generaciokSzama; i++)
                {
                    // jöhet a személyek kirajzolása
                    xKezdo = 50;
                    foreach (var rajzoltszemely in generaciosListakTomb[i])
                    {
                        if(i==0)
                        {
                            
                            if (rajzoltszemely.GyermekekSzama != 0)
                            {
                               
                                var szemelyBorder = RajzolSzemelyt(rajzoltszemely, xKezdo += xTav, yKezdo);
                                if(rajzoltszemely.Nem == "Nő" )
                                {
                                    if(rajzoltszemely.NotRajzoljunkElobbParbol==true)
                                    {
                                        if (rajzoltszemely.ELsoGyerekId != 0)
                                            xtavolsagMapElsoGyerekeknek[rajzoltszemely.ELsoGyerekId] = xKezdo;
                                    }
                                    else
                                    {
                                        xKezdo += (rajzoltszemely.GyermekekSzama * 600);//ha gyermek van eltoljuk a személyeket jobbra az adott generációban
                                    }
                                        
                                }
                                if (rajzoltszemely.Nem == "Férfi")
                                {
                                    if (rajzoltszemely.NotRajzoljunkElobbParbol == false)
                                    {
                                        if (rajzoltszemely.ELsoGyerekId != 0)
                                            xtavolsagMapElsoGyerekeknek[rajzoltszemely.ELsoGyerekId] = xKezdo;
                                    }
                                    else
                                    {
                                        xKezdo += (rajzoltszemely.GyermekekSzama * 600);//ha gyermek van eltoljuk a személyeket jobbra az adott generációban
                                    }
                                }
                                
                                
                                szemelyBorderDictionary[rajzoltszemely.Azonosito] = szemelyBorder;
                                rajzoltszemely.UIElem = szemelyBorder;
                                //Automatikusan kiválasztjuk az utolsót
                                if (utolso != null && rajzoltszemely.Azonosito == utolso.Azonosito)
                                {
                                    szemelyBorder.BorderBrush = Brushes.OrangeRed;
                                    kijeloltBorder = szemelyBorder;

                                    var rajzolt = szemelyBorder.Tag as RajzoltSzemely;

                                    // 👉 Itt beállítjuk manuálisan, ha nem történténne kattintás
                                    LegutobbKijeloltSzemely = rajzolt;

                                    // Aktuális KezdoTartalomControl elérése
                                    if (TartalomValto.Content is KezdoTartalomControl aktivTartalom)
                                    {
                                        aktivTartalom.DataContext = rajzoltszemely;
                                    }
                                }
                            }
                            else
                            {
                                var szemelyBorder = RajzolSzemelyt(rajzoltszemely, xKezdo += xTav, yKezdo);
                                szemelyBorderDictionary[rajzoltszemely.Azonosito] = szemelyBorder;
                                rajzoltszemely.UIElem = szemelyBorder;
                                //Automatikusan kiválasztjuk az utolsót
                                if (utolso != null && rajzoltszemely.Azonosito == utolso.Azonosito)
                                {
                                    szemelyBorder.BorderBrush = Brushes.OrangeRed;
                                    kijeloltBorder = szemelyBorder;

                                    var rajzolt = szemelyBorder.Tag as RajzoltSzemely;

                                    // 👉 Itt beállítjuk manuálisan, ha nem történténne kattintás
                                    LegutobbKijeloltSzemely = rajzolt;

                                    // Aktuális KezdoTartalomControl elérése
                                    if (TartalomValto.Content is KezdoTartalomControl aktivTartalom)
                                    {
                                        aktivTartalom.DataContext = rajzoltszemely;
                                    }
                                }


                            }

                            

                            

                        }

                        //ha már gyerekek kirajzolásához érünk
                        if (i>0)
                        {
                            if (rajzoltszemely.ELsoGyerek == true)
                            {
                                double xertek = 0;
                                if (xtavolsagMapElsoGyerekeknek.ContainsKey(rajzoltszemely.Azonosito))
                                {
                                    xertek = xtavolsagMapElsoGyerekeknek[rajzoltszemely.Azonosito]; // Biztonságosan lekérjük az objektumot
                                    xKezdo = xertek;
                                    var szemelyBorder = RajzolSzemelyt(rajzoltszemely, xKezdo, yKezdo);
                                    szemelyBorderDictionary[rajzoltszemely.Azonosito] = szemelyBorder;
                                    rajzoltszemely.UIElem = szemelyBorder;
                                    //Automatikusan kiválasztjuk az utolsót
                                    if (utolso != null && rajzoltszemely.Azonosito == utolso.Azonosito)
                                    {
                                        szemelyBorder.BorderBrush = Brushes.OrangeRed;
                                        kijeloltBorder = szemelyBorder;

                                        var rajzolt = szemelyBorder.Tag as RajzoltSzemely;

                                        // 👉 Itt beállítjuk manuálisan, ha nem történténne kattintás
                                        LegutobbKijeloltSzemely = rajzolt;

                                        // Aktuális KezdoTartalomControl elérése
                                        if (TartalomValto.Content is KezdoTartalomControl aktivTartalom)
                                        {
                                            aktivTartalom.DataContext = rajzoltszemely;
                                        }
                                    }
                                    if (rajzoltszemely.GyermekekSzama != 0)//neki is lehetnek gyerekei
                                    {
                                        if (rajzoltszemely.Nem == "Nő")
                                        {
                                            if (rajzoltszemely.NotRajzoljunkElobbParbol == true)
                                            {
                                                if (rajzoltszemely.ELsoGyerekId != 0)
                                                    xtavolsagMapElsoGyerekeknek[rajzoltszemely.ELsoGyerekId] = xKezdo;
                                            }
                                            else
                                            {
                                                xKezdo += ((rajzoltszemely.GyermekekSzama * 150)-150);//ha gyermek van eltoljuk a személyeket jobbra az adott generációban
                                            }

                                        }
                                        if (rajzoltszemely.Nem == "Férfi")
                                        {
                                            if (rajzoltszemely.NotRajzoljunkElobbParbol == false)
                                            {
                                                if (rajzoltszemely.ELsoGyerekId != 0)
                                                    xtavolsagMapElsoGyerekeknek[rajzoltszemely.ELsoGyerekId] = xKezdo;
                                            }
                                            else
                                            {
                                                xKezdo += ((rajzoltszemely.GyermekekSzama * 150)-150) ;//ha gyermek van eltoljuk a személyeket jobbra az adott generációban
                                            }
                                        }

                                    }


                                }
                                
                            }
                            else if (rajzoltszemely.GyermekekSzama != 0)
                            {

                                var szemelyBorder = RajzolSzemelyt(rajzoltszemely, xKezdo += xTav, yKezdo);
                                if (rajzoltszemely.Nem == "Nő")
                                {
                                    if (rajzoltszemely.NotRajzoljunkElobbParbol == true)
                                    {
                                        if (rajzoltszemely.ELsoGyerekId != 0)
                                            xtavolsagMapElsoGyerekeknek[rajzoltszemely.ELsoGyerekId] = xKezdo;
                                    }
                                    else
                                    {
                                        xKezdo += ((rajzoltszemely.GyermekekSzama * 150)-150);//ha gyermek van eltoljuk a személyeket jobbra az adott generációban
                                    }

                                }
                                if (rajzoltszemely.Nem == "Férfi")
                                {
                                    if (rajzoltszemely.NotRajzoljunkElobbParbol == false)
                                    {
                                        if (rajzoltszemely.ELsoGyerekId != 0)
                                            xtavolsagMapElsoGyerekeknek[rajzoltszemely.ELsoGyerekId] = xKezdo;
                                    }
                                    else
                                    {
                                        xKezdo += ((rajzoltszemely.GyermekekSzama * 150)-150);//ha gyermek van eltoljuk a személyeket jobbra az adott generációban
                                    }
                                }
                                szemelyBorderDictionary[rajzoltszemely.Azonosito] = szemelyBorder;
                                rajzoltszemely.UIElem = szemelyBorder;
                                //Automatikusan kiválasztjuk az utolsót
                                if (utolso != null && rajzoltszemely.Azonosito == utolso.Azonosito)
                                {
                                    szemelyBorder.BorderBrush = Brushes.OrangeRed;
                                    kijeloltBorder = szemelyBorder;

                                    var rajzolt = szemelyBorder.Tag as RajzoltSzemely;

                                    // 👉 Itt beállítjuk manuálisan, ha nem történténne kattintás
                                    LegutobbKijeloltSzemely = rajzolt;

                                    // Aktuális KezdoTartalomControl elérése
                                    if (TartalomValto.Content is KezdoTartalomControl aktivTartalom)
                                    {
                                        aktivTartalom.DataContext = rajzoltszemely;
                                    }
                                }
                            }
                            else
                            {

                                var szemelyBorder = RajzolSzemelyt(rajzoltszemely, xKezdo += xTav, yKezdo);

                                szemelyBorderDictionary[rajzoltszemely.Azonosito] = szemelyBorder;
                                rajzoltszemely.UIElem = szemelyBorder;
                                //Automatikusan kiválasztjuk az utolsót
                                if (utolso != null && rajzoltszemely.Azonosito == utolso.Azonosito)
                                {
                                    szemelyBorder.BorderBrush = Brushes.OrangeRed;
                                    kijeloltBorder = szemelyBorder;

                                    var rajzolt = szemelyBorder.Tag as RajzoltSzemely;

                                    // 👉 Itt beállítjuk manuálisan, ha nem történténne kattintás
                                    LegutobbKijeloltSzemely = rajzolt;

                                    // Aktuális KezdoTartalomControl elérése
                                    if (TartalomValto.Content is KezdoTartalomControl aktivTartalom)
                                    {
                                        aktivTartalom.DataContext = rajzoltszemely;
                                    }
                            }
                            

                        }



                    }

                        
                    }
                    // jöhet a vonalak kirajzolása
                    foreach (var rajzoltszemely in generaciosListakTomb[i])
                    {
                    //vonalak rajzolása
                        int ferfiId = 0;
                        int noiId = 0;

                        Border aktualisSzemelyBorder = szemelyBorderDictionary[rajzoltszemely.Azonosito];

                   

                        if (rajzoltszemely.Nem == "Nő" )
                        {
                            if (partnerMapNoknek.ContainsKey(rajzoltszemely.Azonosito))
                            {
                                ferfiId = partnerMapNoknek[rajzoltszemely.Azonosito]; // Biztonságosan lekérjük az objektumot
                                //if (!szemelyDict.TryGetValue(ferfiId, out var ParFerfiTagja)) continue;
                                Border ferfiBorder = szemelyBorderDictionary[ferfiId];

                                //párkapcsolat vonal kirajzolása
                                if (rajzoltszemely.NotRajzoljunkElobbParbol != null && rajzoltszemely.NotRajzoljunkElobbParbol == true)
                                    RajzolVonal(aktualisSzemelyBorder, ferfiBorder, true);
                                else if (rajzoltszemely.NotRajzoljunkElobbParbol != null && rajzoltszemely.NotRajzoljunkElobbParbol == false)
                                    RajzolVonal(ferfiBorder, aktualisSzemelyBorder, true);
                            }
                        
                        }
                        else
                        {
                            if (partnerMapNoknek.ContainsKey(rajzoltszemely.Azonosito))
                            {
                                noiId = partnerMapFerfiaknak[rajzoltszemely.Azonosito]; // Biztonságosan lekérjük az objektumot
                                //if (!szemelyDict.TryGetValue(noId, out var ParNoiTagja)) continue;
                                Border noiBorder = szemelyBorderDictionary[noiId];

                                //párkapcsolat vonal kirajzolása
                                if (rajzoltszemely.NotRajzoljunkElobbParbol != null && rajzoltszemely.NotRajzoljunkElobbParbol == true)
                                    RajzolVonal(noiBorder, aktualisSzemelyBorder, true);
                                else if (rajzoltszemely.NotRajzoljunkElobbParbol != null && rajzoltszemely.NotRajzoljunkElobbParbol == false)
                                    RajzolVonal(aktualisSzemelyBorder, noiBorder, true);
                            }
                        
                        }
                            

                        
                        // gyermek-szülő vonal kirajzolása, ha i>0
                        if (i > 0 && rajzoltszemely.GyermekAzIlleto == true) //mindenkinek van szülője
                        {
                            Border Anya = szemelyBorderDictionary[rajzoltszemely.Anya.Azonosito];
                            Border Apa = szemelyBorderDictionary[rajzoltszemely.Apa.Azonosito];
                            Border Gyerek = aktualisSzemelyBorder;

                            //Anyukát rajzoljuk előbb
                            if (rajzoltszemely.ApaRajzolElobb == false)
                                RajzolVonalKozep(Anya, Apa, Gyerek);
                            //Apukát rajzoljuk előbb
                            else if (rajzoltszemely.ApaRajzolElobb == true)
                                RajzolVonalKozep(Apa, Anya, Gyerek);

                        }
                    }
                    yKezdo += 200;
                }
            
        }

            private void RajzolVonal(Border bal, Border jobb, bool szaggatott)
            {
                double y = Canvas.GetTop(bal) + BoxHeight;
                var line = new Line
                {
                    X1 = Canvas.GetLeft(bal) + BoxWidth,
                    Y1 = y,
                    X2 = Canvas.GetLeft(jobb),
                    Y2 = y,
                    Stroke = Brushes.Black,
                    StrokeThickness = 1.5,
                    StrokeDashArray = szaggatott ? new DoubleCollection { 4, 2 } : null
                };
                CsaladfaVaszon.Children.Add(line);
            }

            private void RajzolVonalKozep(Border szulo1, Border szulo2, Border gyerek)
            {
            double felsoPontMagassag = BoxHeight + 5;
                
                double x1 = Canvas.GetLeft(szulo1);
                double x2 = Canvas.GetLeft(szulo2);
                double y1 = Canvas.GetTop(szulo1) + felsoPontMagassag;
                double gyY = Canvas.GetTop(gyerek);
                double gyX = Canvas.GetLeft(gyerek) + BoxWidth / 2;

                double szuloKozepX = (x1 + BoxWidth + x2) / 2;

                CsaladfaVaszon.Children.Add(new Line
                {
                    X1 = szuloKozepX,
                    Y1 = y1,
                    X2 = gyX,
                    Y2 = gyY,
                    Stroke = Brushes.Black,
                    StrokeThickness = 2
                });
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
                        if (!partnerMapNoknek.ContainsKey(kapcsolat.SzemelyId))
                        {
                            partnerMapNoknek.Add(kapcsolat.SzemelyId, kapcsolat.KapcsolodoSzemelyId);
                        }
                        // A nő SzemelyId-ja az érték, a férfi KapcsolodoSzemelyId-ja a kulcs
                        if (!partnerMapFerfiaknak.ContainsKey(kapcsolat.KapcsolodoSzemelyId))
                        {
                            partnerMapFerfiaknak.Add(kapcsolat.KapcsolodoSzemelyId, kapcsolat.SzemelyId);
                        }
                }
                }
                return kapcsolatok;
            }



            private void SzamoljGeneraciokat(int szemelyId, int szint, List<Kapcsolatok> kapcsolatok)
            {
                if (GeneracioSzintek.ContainsKey(szemelyId) && GeneracioSzintek[szemelyId] <= szint)
                    return;

                GeneracioSzintek[szemelyId] = szint;

                foreach (var kapcsolat in kapcsolatok.Where(k => k.KapcsolatTipusa == "Gyermek" && k.SzemelyId == szemelyId))
                {
                    SzamoljGeneraciokat(kapcsolat.KapcsolodoSzemelyId, szint + 1, kapcsolatok);
                }

                foreach (var kapcsolat in kapcsolatok.Where(k => k.KapcsolatTipusa == "Partner" && k.SzemelyId == szemelyId))
                {
                    SzamoljGeneraciokat(kapcsolat.KapcsolodoSzemelyId, szint, kapcsolatok);
                }
            }


            private int SzamoldKiGeneraciokat(List<RajzoltSzemely> rajzoltSzemelyek, List<Kapcsolatok> kapcsolatok)
            {
                int max = 0;
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
                        var gyermek = rajzoltSzemelyek.FirstOrDefault(x => x.Azonosito == kapcsolat.KapcsolodoSzemelyId && kapcsolat.KapcsolatTipusa == "Gyermek");
                        var szulo = rajzoltSzemelyek.FirstOrDefault(x => x.Azonosito == kapcsolat.SzemelyId && kapcsolat.KapcsolatTipusa == "Gyermek");

                        if (szulo != null && gyermek != null)
                        {
                            int elvartGyermekSzint = szulo.GeneracioSzint + 1;
                            if (gyermek.GeneracioSzint < elvartGyermekSzint)
                            {
                                gyermek.GeneracioSzint = elvartGyermekSzint;
                                valtozasTortent = true;

                            }
                            if (elvartGyermekSzint > max)
                                max = elvartGyermekSzint;
                        }
                    }
                }
                while (valtozasTortent);
                return max + 1;
            }



            private void Kijelentkezes_Click(object sender, RoutedEventArgs e)
            {

                // Navigálás a BejelentkezesPage oldalra.
                ((MainWindow)System.Windows.Application.Current.MainWindow).MainFrame.Navigate(new BejelentkezesPage(_context));
            }
        }
    
}

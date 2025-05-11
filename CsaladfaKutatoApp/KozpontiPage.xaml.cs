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
using Microsoft.Win32;
using System.Reflection;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Security.Cryptography;
using System.Diagnostics.Eventing.Reader;
using System.Collections.ObjectModel;


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
        public Border kijeloltBorder = null;
        public RajzoltSzemely LegutobbKijeloltSzemely { get; set; }
        public List<RajzoltSzemely> RajzoltSzemelyek;
        private Dictionary<int, Border> szemelyBorderDictionary = new Dictionary<int, Border>();
        int? felhasznaloId = ((MainWindow)System.Windows.Application.Current.MainWindow).BejelentkezettFelhasznaloId;
        private Dictionary<int, int> GeneracioSzintek = new();
        private Dictionary<int, int> partnerMapNoknek = new();
        private Dictionary<int, int> partnerMapFerfiaknak = new();
        private Dictionary<int?, double> xtavolsagMapElsoGyerekeknek = new();
        public ObservableCollection<RajzoltSzemely>[] generaciosListakTomb;
        public string LegutobbiKapcsolatTipus;
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

            //frissítünk a profilkép miatt, az legutóbb hozzáadott személy profilképje jelenik meg vagy a sablon
            KezdoTartalomControl frissitettOldal = new KezdoTartalomControl(this, _context);
            frissitettOldal.DataContext = this.LegutobbKijeloltSzemely;
            frissitettOldal.BetoltSzemelyKepet();
            frissitettOldal.ToltsdBeSzemelyAdatokatListViewhoz();
            TartalomValto.Content = frissitettOldal;

        }


        public void TartalomValtas(UserControl ujTartalom)
        {
            TartalomValto.Content = ujTartalom;
        }

        private void ExportPng_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.Filter = "PNG képek (*.png)|*.png";
            saveDialog.FileName = "csaladfa.png";
            if (saveDialog.ShowDialog() == true)
            {
                MentsCanvasKepbe(CsaladfaVaszon, saveDialog.FileName);
                MessageBox.Show("Mentés kész: " + saveDialog.FileName);
            }
        }


        private void Segitseg_Click(object sender, RoutedEventArgs e)
        {

            // Mindig az IsOpen állapotból indulunk ki
            bool jelenlegNyitva = BuborekPopup.IsOpen;

            // Ha nyitva van, zárd be
            if (jelenlegNyitva)
            {
                BuborekPopup.IsOpen = false;
            }
            else
            {
                // Nyitás előtt biztosítsd, hogy tényleg bezárt állapotból indulunk
                BuborekPopup.IsOpen = true;
                BuborekPopup.StaysOpen = true;
            }

            PopupUnezet.Text = "A menüsávban megjelenő gombok közül az Új családfa gombbal lehetséges teljesen elölről kezdeni egy másik családfát, kitörölve ezzel a régit. " +
                "Az Exportálás png gombbal képeket készíthetünk png formátumba exportálva a családfa aktuális állapotáról. " +
                "A Kijelentkezés gombbal lehetőség van kilépni az aktuális fiókból és bejelentkezni egy másikba. "+
                "A Fiók törlése gomb törli az aktuális fiókot és a hozzá tartozó e-mail címet, felhasználó nevet és a jelszót is. "+
                "A jobb oldali panelen láthatók az aktuálisan kijelölt személy adatai és a Kapcsolat létrehozása felirat alatti lenyíló lista segítségével "+
                "lehetőség van az aktuálisan kijeölt személyhez családtagot adni. "+
                "A lenyíló listában ki kell választani, hogy milyen családtagot szeretnénk hozzáadni az adott személyhez. "+
                "A megjelenő elemek közül rá kell kattintani a megfelelőre és kitölteni a jobb oldalon megjelenő családtag hozzáadása oldal mezőit. "+
                "A kitöltés után rá kell nyomni a Hozzáadás gombra és megjelenik az új családtag a családfában. "+
                "A családfát ezzek a módszerrel lépésről lépésre lehet kialakítani, így a felhasználó elkészítheti apránként a saját családfáját.";

        }

        private void Panel_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            // Mindig az IsOpen állapotból indulunk ki
            bool jelenlegNyitva = BuborekPopup.IsOpen;

            if (jelenlegNyitva && !BuborekPopup.IsMouseOver && !InfoGomb.IsMouseOver)
            {
                BuborekPopup.StaysOpen = false;

            }
        }



        private void MentsCanvasKepbe(Canvas vaszon, string fajlNev)
        {
            // Eredeti méret megjegyzése
            double eredetiSzelesseg = vaszon.ActualWidth;
            double eredetiMagassag = vaszon.ActualHeight;

            double width = 3000;
            double height = 2000;
            vaszon.Measure(new Size(width, height));
            vaszon.Arrange(new Rect(0, 0, width, height));

            var renderBitmap = new RenderTargetBitmap(
                (int)vaszon.ActualWidth, (int)vaszon.ActualHeight,
                96d, 96d, PixelFormats.Pbgra32);

            renderBitmap.Render(vaszon);

            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(renderBitmap));

            using (var fileStream = new FileStream(fajlNev, FileMode.Create))
            {
                encoder.Save(fileStream);
            }

            // Vászon állapotának visszaállítása és újrarajzolás
            vaszon.Arrange(new Rect(0, 0, eredetiSzelesseg, eredetiMagassag));
            vaszon.UpdateLayout();
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

            //Megézzük, hogy az adatbázisban van-e hozzá tartozó kép, ha van azt jelenítjük meg, ha nincs
            //akkor az alapértelmezettet
            var alapKepBase64 = _context.Fotoks
            .Where(f => f.SzemelyId == szemely.Azonosito)
            .Select(f => f.FotoBase64)
            .FirstOrDefault();

            var beallitottKep = _context.Fotoks
            .Where(f => f.SzemelyId == szemely.Azonosito &&
                _context.Torteneteks.Any(t => t.FotoId == f.FotoId))
            .Select(f => f.FotoBase64)
            .FirstOrDefault();

            if (beallitottKep != null)
                alapKepBase64 = beallitottKep;

            if (alapKepBase64 == null)
            {
                 alapKepBase64 = szemely.Nem == "Férfi"
                 ? AlapertelmezettKepek.FerfiBase64
                 : AlapertelmezettKepek.NoBase64;

            }
                

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

            // Törlés a Canvasról: először vonalakat aztán személyeket. Elég egyszer törölni.
            if (torlendoSzemely?.Gyermek_Szulo_Vonal != null)
            {
                CsaladfaVaszon.Children.Remove(torlendoSzemely.Gyermek_Szulo_Vonal);
                
                //mivel mindig csak a gyereket lehet törölni előőb és itt is azt töröljük ezért annak
                //már felesleges nullra állítani a Gyermek_Szulo_Vonal tulajdonságát
                //az apa és anya gyermek_szulo vonalát azért nem töröljül mert az a szülőket köti össze az ő szüleikkel

                //a szülők Gyermekei listájából is törölni kell az illetőt
                torlendoSzemely.Anya.Gyermekei.Remove(torlendoSzemely);
                torlendoSzemely.Apa.Gyermekei.Remove(torlendoSzemely);

                
            }
                
            if (torlendoSzemely?.Parkapcsolat_Vonal != null)
            {
                CsaladfaVaszon.Children.Remove(torlendoSzemely.Parkapcsolat_Vonal);
                torlendoSzemely.Parja.Parkapcsolat_Vonal = null;//mert itt oda vissza számított a vonal
                torlendoSzemely.Parja.Parja = null;

            }
                
            
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
                aktivTartalom = new KezdoTartalomControl(this, _context);
                aktivTartalom.DataContext = szemely;
                aktivTartalom.BetoltSzemelyKepet();
                aktivTartalom.ToltsdBeSzemelyAdatokatListViewhoz();
                TartalomValto.Content = aktivTartalom;
            }

            if (TartalomValto.Content is KapcsolodoSzemelyLetrehozControl aktivTartalom2)
            {
                if (LegutobbiKapcsolatTipus != "")
                {
                    aktivTartalom2 = new KapcsolodoSzemelyLetrehozControl(this, _context, LegutobbiKapcsolatTipus);
                    aktivTartalom2.DataContext = szemely;
                    aktivTartalom2.KapcsolatTipusBeallitas(LegutobbiKapcsolatTipus);
                    TartalomValto.Content = aktivTartalom2;
                }
                
            }

            if (TartalomValto.Content is SzemelyKapcsolataiControl aktivTartalom3)
            {
                aktivTartalom3.DataContext = szemely;
            }

            if (TartalomValto.Content is SzemelySzerkeszteseControl aktivTartalom4)
            {
                aktivTartalom4 = new SzemelySzerkeszteseControl(this, _context);
                aktivTartalom4.DataContext = szemely;
                TartalomValto.Content = aktivTartalom4;
            }
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
                    Parja = null,
                    ApaRajzolElobb = null,
                    NotRajzoljunkElobbParbol = null,
                    GyermekAzIlleto = false,
                    ELsoGyerek = false,
                    ELsoGyerekId = 0,
                    Gyermekei = new List<RajzoltSzemely>(),
                    Testverek = new List<RajzoltSzemely>(),
                    Gyermek_Szulo_Vonal = null,
                    Parkapcsolat_Vonal = null
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
             generaciosListakTomb = new ObservableCollection<RajzoltSzemely>[generaciokSzama];//tömb amely generációk szerint tárolja a rajzoltszemélyes listákat


            //végigmegyünk az összes generáción, külön generációk szerint listákba gyűjtjük a rajzolt személyeket és ezeket a listákat egyesítjük egy nagy tömbben.
            for (int i = 0; i < generaciokSzama; i++)
            {
                ObservableCollection<RajzoltSzemely> RajzolandoSzemelyek = new ObservableCollection<RajzoltSzemely>();
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

            //Az összes betöltött rajzolt személy hozzáadásra került a szótárhoz
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

            ObservableCollection<RajzoltSzemely> gyerekLista = new ObservableCollection<RajzoltSzemely>();//A családfában aktuális generációba tartozó szülők gyerekeinek listája

            for (int i = 0; i < generaciokSzama; i++)
            {
                ObservableCollection<RajzoltSzemely> rendezettGeneraciosLista = new ObservableCollection<RajzoltSzemely>();

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
                                    if(anya == null)
                                    {
                                        anya = szemelyek.FirstOrDefault(k => k.Azonosito == anyaId);
                                        szemelyDict[anyaId] = anya;
                                    }
                                       
                                    // Amelyik anyánál van gyerek, ott kellett partner is, megkeressük
                                    var partnerKapcsolat = kapcsolatok.FirstOrDefault(k => k.SzemelyId == anyaId && k.KapcsolatTipusa == "Partner");
                                    RajzoltSzemely partner = null;
                                    if (szemelyDict.ContainsKey(partnerKapcsolat.KapcsolodoSzemelyId))
                                    {
                                        partner = szemelyDict[partnerKapcsolat.KapcsolodoSzemelyId]; // Biztonságosan lekérjük az objektumot
                                    }
                                    else
                                    {

                                        partner = szemelyek.FirstOrDefault(k => k.Azonosito == partnerKapcsolat.KapcsolodoSzemelyId);
                                        szemelyDict[partnerKapcsolat.KapcsolodoSzemelyId] = partner;
                                    }

                                    rendezettGeneraciosLista.Add(anya);
                                    rendezettGeneraciosLista.Add(partner);

                                    
                                    anya.Parja = partner;
                                    partner.Parja = anya;

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
                                        if(gyerek==null)
                                        {
                                            gyerek = szemelyek.FirstOrDefault(k => k.Azonosito == gyerekId);
                                            szemelyDict[gyerekId] = gyerek;
                                        }

                                        
                                        gyerekLista.Add(gyerek);

                                        gyerek.Anya = anya;
                                        gyerek.Apa = partner;
                                        gyerek.ApaRajzolElobb = false;
                                        gyerek.GyermekAzIlleto = true;
                                        if (gyerek.Azonosito == legelsoGyerekId)
                                            gyerek.ELsoGyerek = true;

                                        anya.Gyermekei.Add(gyerek);
                                        partner.Gyermekei.Add(gyerek);

                                        //gyerek testvéreinek hozzáadása listához
                                        foreach(var gyId in rendezettGyerekek)
                                        {
                                            if (gyId == gyerekId)
                                                continue;

                                            if (!szemelyDict.TryGetValue(gyId, out var testver)) continue;
                                            if (testver == null)
                                            {
                                                testver = szemelyek.FirstOrDefault(k => k.Azonosito == gyId);
                                                szemelyDict[gyId] = testver;
                                            }
                                            gyerek.Testverek.Add(testver);
                                        }


                                    }

                                    szemelyMegtalalva = true;
                                    break;
                                }


                            }

                            if (nokCsakKapcsolatban != null && szemelyMegtalalva == false)
                            {
                                foreach (var noId in nokCsakKapcsolatban)
                                {
                                    if (noId == szemely.Azonosito)
                                    {
                                        if (!szemelyDict.TryGetValue(noId, out var noCsakParral)) continue;
                                        if (noCsakParral == null)
                                        {
                                            noCsakParral = szemelyek.FirstOrDefault(k => k.Azonosito == noId);
                                            szemelyDict[noId] = noCsakParral;
                                        }

                                        //megkerestük a rekordokat amik ezek a nőkhöz tartoznak
                                        var partnerKapcsolat = kapcsolatok.FirstOrDefault(k => k.SzemelyId == noId && k.KapcsolatTipusa == "Partner");
                                        RajzoltSzemely partner = null;
                                        if (szemelyDict.ContainsKey(partnerKapcsolat.KapcsolodoSzemelyId))
                                        {
                                            partner = szemelyDict[partnerKapcsolat.KapcsolodoSzemelyId]; // Biztonságosan lekérjük az objektumot
                                        }
                                        else
                                        {

                                            partner = szemelyek.FirstOrDefault(k => k.Azonosito == partnerKapcsolat.KapcsolodoSzemelyId);
                                            szemelyDict[partnerKapcsolat.KapcsolodoSzemelyId] = partner;
                                        }

                                        rendezettGeneraciosLista.Add(noCsakParral);
                                        rendezettGeneraciosLista.Add(partner);

                                        noCsakParral.Parja = partner;
                                        partner.Parja = noCsakParral;

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
                ObservableCollection<RajzoltSzemely> ujGyerekLista = new ObservableCollection<RajzoltSzemely>();
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
                                    if (anya == null)
                                    {
                                        anya = szemelyek.FirstOrDefault(k => k.Azonosito == anyaId);
                                        szemelyDict[anyaId] = anya;
                                    }
                                    // Amelyik anyánál van gyerek, ott kellett partner is, megkeressük
                                    var partnerKapcsolat = kapcsolatok.FirstOrDefault(k => k.SzemelyId == anyaId && k.KapcsolatTipusa == "Partner");
                                    RajzoltSzemely partner = null;
                                    if (szemelyDict.ContainsKey(partnerKapcsolat.KapcsolodoSzemelyId))
                                    {
                                        partner = szemelyDict[partnerKapcsolat.KapcsolodoSzemelyId]; // Biztonságosan lekérjük az objektumot
                                    }
                                    else
                                    {

                                        partner = szemelyek.FirstOrDefault(k => k.Azonosito == partnerKapcsolat.KapcsolodoSzemelyId);
                                        szemelyDict[partnerKapcsolat.KapcsolodoSzemelyId] = partner;
                                    }

                                    rendezettGeneraciosLista.Add(anya);
                                    rendezettGeneraciosLista.Add(partner);

                                    anya.Parja = partner;
                                    partner.Parja = anya;

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
                                        if (gyerek == null)
                                        {
                                            gyerek = szemelyek.FirstOrDefault(k => k.Azonosito == gyerekId);
                                            szemelyDict[gyerekId] = gyerek;
                                        }
                                        ujGyerekLista.Add(gyerek);

                                        gyerek.Anya = anya;
                                        gyerek.Apa = partner;
                                        gyerek.ApaRajzolElobb = false;
                                        gyerek.GyermekAzIlleto = true;
                                        if (gyerek.Azonosito == legelsoGyerekId)
                                            gyerek.ELsoGyerek = true;

                                        anya.Gyermekei.Add(gyerek);
                                        partner.Gyermekei.Add(gyerek);

                                        //gyerek testvéreinek hozzáadása listához
                                        foreach (var gyId in rendezettGyerekek)
                                        {
                                            if (gyId == gyerekId)
                                                continue;

                                            if (!szemelyDict.TryGetValue(gyId, out var testver)) continue;
                                            if (testver == null)
                                            {
                                                testver = szemelyek.FirstOrDefault(k => k.Azonosito == gyId);
                                                szemelyDict[gyId] = testver;
                                            }
                                            gyerek.Testverek.Add(testver);
                                        }
                                    }
                                    //testvéreket még itt hozzáadni a gyerekeikhez meg előző anyukánál a gyerekeknél is
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
                                    if (CsakHazasNo == null)
                                    {
                                        CsakHazasNo = szemelyek.FirstOrDefault(k => k.Azonosito == noId);
                                        szemelyDict[noId] = CsakHazasNo;
                                    }

                                    //megkerestük a rekordokat amik ezek a nőkhöz tartoznak
                                    var partnerKapcsolat = kapcsolatok.FirstOrDefault(k => k.SzemelyId == noId && k.KapcsolatTipusa == "Partner");
                                    RajzoltSzemely partner = null;
                                    if (szemelyDict.ContainsKey(partnerKapcsolat.KapcsolodoSzemelyId))
                                    {
                                        partner = szemelyDict[partnerKapcsolat.KapcsolodoSzemelyId]; // Biztonságosan lekérjük az objektumot
                                    }
                                    else
                                    {

                                        partner = szemelyek.FirstOrDefault(k => k.Azonosito == partnerKapcsolat.KapcsolodoSzemelyId);
                                        szemelyDict[partnerKapcsolat.KapcsolodoSzemelyId] = partner;
                                    }

                                    rendezettGeneraciosLista.Add(CsakHazasNo);
                                    rendezettGeneraciosLista.Add(partner);

                                    CsakHazasNo.Parja = partner;
                                    partner.Parja = CsakHazasNo;

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
                                    if (CsakHazasFerfi == null)
                                    {
                                        CsakHazasFerfi = szemelyek.FirstOrDefault(k => k.Azonosito == ferfiId);
                                        szemelyDict[ferfiId] = CsakHazasFerfi;
                                    }

                                    //megkerestük a rekordokat amik ehhez a férfihoz tartoznak
                                    var partnerKapcsolat = kapcsolatok.FirstOrDefault(k => k.KapcsolodoSzemelyId == ferfiId && k.KapcsolatTipusa == "Partner");
                                   
                                    RajzoltSzemely partner = null;
                                    if (szemelyDict.ContainsKey(partnerKapcsolat.SzemelyId))
                                    {
                                        partner = szemelyDict[partnerKapcsolat.SzemelyId]; // Biztonságosan lekérjük az objektumot
                                    }
                                    else
                                    {

                                        partner = szemelyek.FirstOrDefault(k => k.Azonosito == partnerKapcsolat.SzemelyId);
                                        szemelyDict[partnerKapcsolat.SzemelyId] = partner;
                                    }
                                    rendezettGeneraciosLista.Add(CsakHazasFerfi);
                                    rendezettGeneraciosLista.Add(partner);
                                    
                                    CsakHazasFerfi.Parja = partner;
                                    partner.Parja = CsakHazasFerfi;
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
                                    if (apa == null)
                                    {
                                        apa = szemelyek.FirstOrDefault(k => k.Azonosito == ferfiId);
                                        szemelyDict[ferfiId] = apa;
                                    }

                                    //megkerestük a rekordokat amik ehhez a férfihoz tartoznak
                                    var partnerKapcsolat = kapcsolatok.FirstOrDefault(k => k.KapcsolodoSzemelyId == ferfiId && k.KapcsolatTipusa == "Partner");
                                    RajzoltSzemely partner = null;
                                    if (szemelyDict.ContainsKey(partnerKapcsolat.SzemelyId))
                                    {
                                        partner = szemelyDict[partnerKapcsolat.SzemelyId]; // Biztonságosan lekérjük az objektumot
                                    }
                                    else
                                    {
                                        partner = szemelyek.FirstOrDefault(k => k.Azonosito == partnerKapcsolat.SzemelyId);
                                        szemelyDict[partnerKapcsolat.SzemelyId] = partner;
                                    }

                                    rendezettGeneraciosLista.Add(apa);
                                    rendezettGeneraciosLista.Add(partner);
                                    
                                    apa.Parja = partner;
                                    partner.Parja = apa;
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
                                        if (gyerek == null)
                                        {
                                            gyerek = szemelyek.FirstOrDefault(k => k.Azonosito == gyerekId);
                                            szemelyDict[gyerekId] = gyerek;
                                        }
                                        ujGyerekLista.Add(gyerek);

                                        gyerek.Anya = partner;
                                        gyerek.Apa = apa;
                                        gyerek.ApaRajzolElobb = true;
                                        gyerek.GyermekAzIlleto = true;
                                        if (gyerek.Azonosito == legelsoGyerekId)
                                            gyerek.ELsoGyerek = true;

                                        apa.Gyermekei.Add(gyerek);
                                        partner.Gyermekei.Add(gyerek);

                                        //gyerek testvéreinek hozzáadása listához
                                        foreach (var gyId in rendezettGyerekek)
                                        {
                                            if (gyId == gyerekId)
                                                continue;

                                            if (!szemelyDict.TryGetValue(gyId, out var testver)) continue;
                                            if (testver == null)
                                            {
                                                testver = szemelyek.FirstOrDefault(k => k.Azonosito == gyId);
                                                szemelyDict[gyId] = testver;
                                            }
                                            gyerek.Testverek.Add(testver);
                                        }
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
                                        aktivTartalom.DataContext = LegutobbKijeloltSzemely;
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
                                        aktivTartalom.DataContext = LegutobbKijeloltSzemely;
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
                                            aktivTartalom.DataContext = LegutobbKijeloltSzemely;
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
                                        aktivTartalom.DataContext = LegutobbKijeloltSzemely;
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
                                        aktivTartalom.DataContext = LegutobbKijeloltSzemely;
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
                                if (!szemelyDict.TryGetValue(ferfiId, out var ParFerfiTagja)) continue;
                                Border ferfiBorder = szemelyBorderDictionary[ferfiId];

                                //párkapcsolat vonal kirajzolása
                                if (rajzoltszemely.NotRajzoljunkElobbParbol != null && rajzoltszemely.NotRajzoljunkElobbParbol == true)
                                    RajzolVonal(aktualisSzemelyBorder, ferfiBorder, true, rajzoltszemely, ParFerfiTagja);
                                else if (rajzoltszemely.NotRajzoljunkElobbParbol != null && rajzoltszemely.NotRajzoljunkElobbParbol == false)
                                    RajzolVonal(ferfiBorder, aktualisSzemelyBorder, true, ParFerfiTagja, rajzoltszemely);
                            }
                        
                        }
                        else
                        {
                            if (partnerMapFerfiaknak.ContainsKey(rajzoltszemely.Azonosito))
                            {
                                noiId = partnerMapFerfiaknak[rajzoltszemely.Azonosito]; // Biztonságosan lekérjük az objektumot
                                if (!szemelyDict.TryGetValue(noiId, out var ParNoiTagja)) continue;
                                Border noiBorder = szemelyBorderDictionary[noiId];

                                //párkapcsolat vonal kirajzolása
                                if (rajzoltszemely.NotRajzoljunkElobbParbol != null && rajzoltszemely.NotRajzoljunkElobbParbol == true)
                                    RajzolVonal(noiBorder, aktualisSzemelyBorder, true, ParNoiTagja, rajzoltszemely);
                                else if (rajzoltszemely.NotRajzoljunkElobbParbol != null && rajzoltszemely.NotRajzoljunkElobbParbol == false)
                                    RajzolVonal(aktualisSzemelyBorder, noiBorder, true, rajzoltszemely, ParNoiTagja);
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
                                RajzolVonalKozep(Anya, Apa, Gyerek, rajzoltszemely);
                            //Apukát rajzoljuk előbb
                            else if (rajzoltszemely.ApaRajzolElobb == true)
                                RajzolVonalKozep(Apa, Anya, Gyerek, rajzoltszemely);

                        }
                    }
                    yKezdo += 200;
                }



        

            
        }

            private void RajzolVonal(Border bal, Border jobb, bool szaggatott, RajzoltSzemely szemely1, RajzoltSzemely szemely2)
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

                szemely1.Parkapcsolat_Vonal = line;
                szemely2.Parkapcsolat_Vonal = line;
                CsaladfaVaszon.Children.Add(line);
            }

            private void RajzolVonalKozep(Border szulo1, Border szulo2, Border gyerek, RajzoltSzemely gyerekSzemely)
            {
                double felsoPontMagassag = BoxHeight + 5;
                
                    double x1 = Canvas.GetLeft(szulo1);
                    double x2 = Canvas.GetLeft(szulo2);
                    double y1 = Canvas.GetTop(szulo1) + felsoPontMagassag;
                    double gyY = Canvas.GetTop(gyerek);
                    double gyX = Canvas.GetLeft(gyerek) + BoxWidth / 2;

                    double szuloKozepX = (x1 + BoxWidth + x2) / 2;

                

                var line = new Line
                {
                    X1 = szuloKozepX,
                    Y1 = y1,
                    X2 = gyX,
                    Y2 = gyY,
                    Stroke = Brushes.Black,
                    StrokeThickness = 2
                };
                gyerekSzemely.Gyermek_Szulo_Vonal=line;//mindig csak a gyerekhez állítjuk be
                

                CsaladfaVaszon.Children.Add(line);
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

        private void UjCsaladfa_Click(object sender, RoutedEventArgs e)
        {
            
            if (felhasznaloId == null)
            {
                MessageBox.Show("Hiba: Nem található bejelentkezett felhasználó.");
                return;
            }

            // Megerősítő kérdés
            var result = MessageBox.Show(
                "Biztosan új családfát szeretnél készíteni? Minden eddigi adatod a családfából törlődni fog.",
                "Megerősítés szükséges",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
            {
                return; // Felhasználó mégsem akar törölni
            }


            var szemelyek = _context.Szemelyeks.Where(s => s.FelhasznaloId == felhasznaloId).ToList();
            var szemelyIds = szemelyek.Select(s => s.SzemelyId).ToList();
            var helyszinIds = szemelyek.Select(s => s.HelyszinId).Where(h => h != null).Distinct().ToList();

            var kapcsolatok = _context.Kapcsolatoks
                .Where(k => szemelyIds.Contains(k.SzemelyId) || szemelyIds.Contains(k.KapcsolodoSzemelyId));
            _context.Kapcsolatoks.RemoveRange(kapcsolatok);

            var tortenetek = _context.Torteneteks.Where(t => szemelyIds.Contains(t.SzemelyId));
            _context.Torteneteks.RemoveRange(tortenetek);

            var fotok = _context.Fotoks.Where(f => szemelyIds.Contains(f.SzemelyId));
            _context.Fotoks.RemoveRange(fotok);

            _context.Szemelyeks.RemoveRange(szemelyek);

            var helyszinek = _context.Helyszineks
                .Where(h => helyszinIds.Contains(h.HelyszinId));
            _context.Helyszineks.RemoveRange(helyszinek);

            _context.SaveChanges();

            var ujOldal = new ElsoCsaladtagHozzaadPage(_context);
            ((MainWindow)System.Windows.Application.Current.MainWindow).MainFrame.Navigate(ujOldal);
        }


        private void FiokTorlese_Click(object sender, RoutedEventArgs e)
        {
            
            if (felhasznaloId == null)
            {
                MessageBox.Show("Hiba: Nem található bejelentkezett felhasználó.");
                return;
            }

            var megerosites = MessageBox.Show(
                "Biztosan törölni szeretnéd a fiókodat? Minden adatod véglegesen törlődik.",
                "Fiók törlése megerősítés",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);


            if (megerosites != MessageBoxResult.Yes)
                return;



            //Személyek és kapcsolódó adatok törlése
            var szemelyek = _context.Szemelyeks.Where(s => s.FelhasznaloId == felhasznaloId).ToList();
            var szemelyIds = szemelyek.Select(s => s.SzemelyId).ToList();
            var helyszinIds = szemelyek.Select(s => s.HelyszinId).Where(h => h != null).Distinct().ToList();

            var kapcsolatok = _context.Kapcsolatoks
                .Where(k => szemelyIds.Contains(k.SzemelyId) || szemelyIds.Contains(k.KapcsolodoSzemelyId));
            _context.Kapcsolatoks.RemoveRange(kapcsolatok);

            var tortenetek = _context.Torteneteks.Where(t => szemelyIds.Contains(t.SzemelyId));
            _context.Torteneteks.RemoveRange(tortenetek);

            var fotok = _context.Fotoks.Where(f => szemelyIds.Contains(f.SzemelyId));
            _context.Fotoks.RemoveRange(fotok);

            _context.Szemelyeks.RemoveRange(szemelyek);

            var helyszinek = _context.Helyszineks
                .Where(h => helyszinIds.Contains(h.HelyszinId));
            _context.Helyszineks.RemoveRange(helyszinek);

            //Végül a felhasználó törlése
            var felhasznalo = _context.Felhasznaloks.FirstOrDefault(f => f.FelhasznaloId == felhasznaloId);
            if (felhasznalo != null)
                _context.Felhasznaloks.Remove(felhasznalo);

            _context.SaveChanges();

            MessageBox.Show("A fiók és az összes hozzá tartozó adat sikeresen törölve lett.", "Fiók törölve", MessageBoxButton.OK, MessageBoxImage.Information);

            //Navigáció vissza a bejelentkezési oldalra
            ((MainWindow)Application.Current.MainWindow).BejelentkezettFelhasznaloId = null;
            var loginPage = new BejelentkezesPage(_context);
            ((MainWindow)Application.Current.MainWindow).MainFrame.Navigate(loginPage);
        }

    }

}

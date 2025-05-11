using CsaladfaKutatoApp.Models;
using CsaladfaKutatoApp.Models.DTO;
using System;
using System.Collections.Generic;
using System.IO;
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

namespace CsaladfaKutatoApp
{
    /// <summary>
    /// Interaction logic for FotokTortenetPage.xaml
    /// </summary>
    public partial class FotokTortenetPage : Page
    {
        private KozpontiPage KpOldal;
        private readonly CsaladfaAdatbazisContext _context;
        private readonly int? userId;
        private string _base64Kep;
        private List<dynamic> _kepek = new();
        public FotokTortenetPage(KozpontiPage klodal, CsaladfaAdatbazisContext context,  int? felhasznaloId)
        {
            InitializeComponent();

            _context = context;
            KpOldal = klodal;
            userId = felhasznaloId;

            BetoltKepValasztot();

            var szemelyId = KpOldal.LegutobbKijeloltSzemely.Azonosito;

            var tortenet = _context.Torteneteks
                .FirstOrDefault(t => t.SzemelyId == szemelyId);

            if (tortenet != null && tortenet.FotoId != null)
            {
                var kep = _context.Fotoks.FirstOrDefault(f => f.FotoId == tortenet.FotoId);

                if (kep != null)
                {
                    // Base64 konvertálása és megjelenítése
                    PreviewImage.Source = Base64ToImage(kep.FotoBase64);
                    PreviewImage.Visibility = Visibility.Visible;
                    PlaceholderText.Visibility = Visibility.Collapsed;

                    // A szövegdobozok feltöltése
                    Leiras_TextBox.Text = kep.Leiras;
                    Tortenet_TextBox.Text = tortenet.Tortenet;
                    _base64Kep = kep.FotoBase64;
                }
            }
        }

        private void Vissza_Click(object sender, RoutedEventArgs e)
        {
;
            // Navigáljunk a KozpontiPage-re.
            ((MainWindow)System.Windows.Application.Current.MainWindow).MainFrame.Navigate(new KozpontiPage(_context,userId)) ;
        }

        private void Mentes_Click(object sender, RoutedEventArgs e)
        {
            var szemelyId = KpOldal.LegutobbKijeloltSzemely.Azonosito;

            string leiras = Leiras_TextBox.Text?.Trim();
            string tortenetSzoveg = Tortenet_TextBox.Text?.Trim();

            //Kötelező mezők ellenőrzése
            if (string.IsNullOrWhiteSpace(_base64Kep) ||
                string.IsNullOrWhiteSpace(leiras) ||
                string.IsNullOrWhiteSpace(tortenetSzoveg))
            {
                MessageBox.Show("Tölts fel képet, és töltsd ki a leírást és a történetet.");
                return;
            }

            // Ne szerepeljen ugyanaz a kép kétszer
            bool vanIlyenKep = _context.Fotoks.Any(f => f.FotoBase64 == _base64Kep);
            if (vanIlyenKep)
            {
                MessageBox.Show("Ez a kép már szerepel az adatbázisban.");
                return;
            }


            // Ne legyen duplikált történetszöveg
            bool vanIlyenTortenet = _context.Torteneteks.Any(t => t.Tortenet == tortenetSzoveg);
            if (vanIlyenTortenet)
            {
                MessageBox.Show("Ez a történet már szerepel az adatbázisban.");
                return;
            }

            //Duplikált SzemelyId és FotoId ellenőrzése a Tortenetek-ben
            bool vanIlyenParositas1 = _context.Torteneteks
                .Any(t => t.SzemelyId == szemelyId);

            if (vanIlyenParositas1)
            {
                MessageBox.Show("Ehhez a személyhez már tartozik történet.");
                return;
            }

            //Kép mentése
            var foto = new Fotok
            {
                SzemelyId = szemelyId,
                FotoBase64 = _base64Kep,
                Leiras = leiras
            };
            _context.Fotoks.Add(foto);
            _context.SaveChanges();

            

            bool vanIlyenParositas2 = _context.Torteneteks
                .Any(t =>  t.FotoId == foto.FotoId);

            if (vanIlyenParositas2)
            {
                MessageBox.Show("Ehhez a fotóhoz már tartozik történet.");
                var torlendoKep = _context.Fotoks
                    .FirstOrDefault(f => f.FotoBase64 == _base64Kep);

                if (torlendoKep != null)
                {
                    _context.Fotoks.Remove(torlendoKep);
                    _context.SaveChanges();
                    
                }
                return;
            }

            // Történet mentése
            var tortenet = new Tortenetek
            {
                SzemelyId = szemelyId,
                FotoId = foto.FotoId,
                Tortenet = tortenetSzoveg
            };
            _context.Torteneteks.Add(tortenet);
            _context.SaveChanges();

            MessageBox.Show("Sikeresen elmentve.");
            

            BetoltKepValasztot(); // frissítjük a betöltött képlistát


        }




        private void Border_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                string imagePath = files.FirstOrDefault();

                MessageBox.Show($"Betöltött fájl: {imagePath}");

                if (File.Exists(imagePath))
                {
                    byte[] imageBytes = File.ReadAllBytes(imagePath);
                    string base64 = Convert.ToBase64String(imageBytes);
                    _base64Kep = base64;

                    // Képmegjelenítés
                    PreviewImage.Source = Base64ToImage(base64);
                    PreviewImage.Visibility = Visibility.Visible;
                    PlaceholderText.Visibility = Visibility.Collapsed;
                }
            }
        }
        private BitmapImage Base64ToImage(string base64String)
        {
            byte[] imageBytes = Convert.FromBase64String(base64String);
            using (var stream = new MemoryStream(imageBytes))
            {
                var image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.StreamSource = stream;
                image.EndInit();
                return image;
            }
        }

        private void BetoltKepValasztot()
        {
            var szemelyId = KpOldal.LegutobbKijeloltSzemely.Azonosito;

            var kepek = _context.Fotoks
                .Where(f => f.SzemelyId == szemelyId)
                .Select(f => new { f.FotoId, f.Leiras, f.FotoBase64 })
                .ToList();

            kepek.Insert(0, new { FotoId = -1, Leiras = "-- Válassz egy elemet --", FotoBase64 = (string)null });

            KepValaszto.ItemsSource = kepek;
            KepValaszto.SelectedIndex = 0;
        }

        private void KepValaszto_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var kivalasztott = KepValaszto.SelectedItem as dynamic;

            if (kivalasztott?.FotoId == -1 || string.IsNullOrWhiteSpace(kivalasztott?.FotoBase64))
            {
                PreviewImage.Source = null;
                PreviewImage.Visibility = Visibility.Collapsed;
                PlaceholderText.Visibility = Visibility.Visible;
                return;
            }

            PreviewImage.Source = Base64ToImage(kivalasztott.FotoBase64);
            PreviewImage.Visibility = Visibility.Visible;
            PlaceholderText.Visibility = Visibility.Collapsed;
        }

        private void Torol_Click(object sender, RoutedEventArgs e)
        {
            string base64 = _base64Kep; // vagy amit korábban eltároltál
            string leiras = Leiras_TextBox.Text?.Trim();
            string tortenetSzoveg = Tortenet_TextBox.Text?.Trim();

            TorolKepEsTortenet(base64, leiras, tortenetSzoveg);


            _base64Kep = null;
            PreviewImage.Source = null;
            PreviewImage.Visibility = Visibility.Collapsed;
            PlaceholderText.Visibility = Visibility.Visible;
            Leiras_TextBox.Text = string.Empty;
            Tortenet_TextBox.Text = string.Empty;

        }

        public void TorolKepEsTortenet(string base64Kep, string leiras, string tortenetSzoveg)
        {
            var valasz = MessageBox.Show("Biztosan törölni szeretnéd a kiválasztott képet és a hozzá tartozó történetet?",
                                 "Törlés megerősítése",
                                 MessageBoxButton.YesNo,
                                 MessageBoxImage.Warning);

            if (valasz != MessageBoxResult.Yes)
            {
                return; // Felhasználó nem erősítette meg a törlést.
            }

            var kep = _context.Fotoks
                .FirstOrDefault(f => f.FotoBase64 == base64Kep );

            if (kep == null)
            {
                MessageBox.Show("A megadott kép nem található az adatbázisban.");
                return;
            }

            // Lekérjük a hozzá tartozó történetet (ha van)
            var tortenet = _context.Torteneteks
                .FirstOrDefault(t => t.FotoId == kep.FotoId );

            // Először töröljük a történetet, ha van
            if (tortenet != null)
            {
                _context.Torteneteks.Remove(tortenet);
            }

            // Majd töröljük a képet
            _context.Fotoks.Remove(kep);

            _context.SaveChanges();
            MessageBox.Show("A kép és a történet sikeresen törölve.");
        }

        private void CsakFotoMentes_Click(object sender, RoutedEventArgs e)
        {
            var szemelyId = KpOldal.LegutobbKijeloltSzemely.Azonosito;
            string leiras = Leiras_TextBox.Text?.Trim();

            // Kötelező mezők ellenőrzése
            if (string.IsNullOrWhiteSpace(_base64Kep) || string.IsNullOrWhiteSpace(leiras))
            {
                MessageBox.Show("Tölts fel képet, és add meg a leírást.");
                return;
            }

            // Duplikált kép ellenőrzése
            bool letezikMar = _context.Fotoks.Any(f => f.FotoBase64 == _base64Kep);
            if (letezikMar)
            {
                MessageBox.Show("Ez a kép már szerepel az adatbázisban.");
                return;
            }

            // Kép mentése
            var ujKep = new Fotok
            {
                SzemelyId = szemelyId,
                FotoBase64 = _base64Kep,
                Leiras = leiras
            };

            _context.Fotoks.Add(ujKep);
            _context.SaveChanges();

            MessageBox.Show("Kép sikeresen elmentve a Fotok táblába.");

            // Űrlap alaphelyzetbe
            _base64Kep = null;
            PreviewImage.Source = null;
            PreviewImage.Visibility = Visibility.Collapsed;
            PlaceholderText.Visibility = Visibility.Visible;
            Leiras_TextBox.Text = string.Empty;
            Tortenet_TextBox.Text = string.Empty;

            BetoltKepValasztot();
        }

    }
}

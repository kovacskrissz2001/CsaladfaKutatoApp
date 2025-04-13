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

namespace CsaladfaKutatoApp
{
    /// <summary>
    /// Interaction logic for KozpontiPage.xaml
    /// </summary>
    public partial class KozpontiPage : Page
    {
        private readonly CsaladfaAdatbazisContext _context;

        public KozpontiPage(CsaladfaAdatbazisContext context)
        {
            InitializeComponent();
            _context = context;

            Loaded += (s, e) => MegjelenitCsaladfat(); // hívás betöltéskor
        }

        private void RajzolSzemelyt(RajzoltSzemely szemely, double pozicioX, double pozicioY)
        {
            var tartalomDoboz = new StackPanel
            {
                Orientation = Orientation.Vertical,
                Width = 80
            };

            BitmapImage bitmap;

            if (!string.IsNullOrWhiteSpace(szemely.KepBase64))
            {
                try
                {
                    byte[] kepBytes = Convert.FromBase64String(szemely.KepBase64);
                    using var memstream = new MemoryStream(kepBytes);
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
                FontWeight = FontWeights.Bold
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

            Canvas.SetLeft(tartalomDoboz, pozicioX);
            Canvas.SetTop(tartalomDoboz, pozicioY);

            CsaladfaVaszon.Children.Add(tartalomDoboz);
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
                .Select(s => new RajzoltSzemely
                {
                    Azonosito = s.SzemelyId,
                    KNev = s.Keresztnev,
                    VNev = s.Vezeteknev,
                    SzuletesiDatum = s.SzuletesiDatum,
                    KepBase64 = s.Fotoks.Select(f => f.FotoBase64).FirstOrDefault(),
                    GyermekAzonositoLista = new List<int>() // később betöltjük kapcsolatokkal
                })
                .ToList();
        }
        private void MegjelenitCsaladfat()
        {
            var szemelyek = BetoltSzemelyekAdatbazisbol();

            double kezdoX = 50; // kezdő vízszintes pozíció
            double y = 50;      // állandó függőleges pozíció

            foreach (var szemely in szemelyek)
            {
                RajzolSzemelyt(szemely, kezdoX, y);
                kezdoX += 150; // következő személy jobbra
            }
        }

        private void NemKapcsolodoHozzaadasa_Click(object sender, RoutedEventArgs e)
        {

        }

        private void LanyaHozzaadasa_Click(object sender, RoutedEventArgs e)
        {

        }

        private void FiaHozzaadasa_Click(object sender, RoutedEventArgs e)
        {

        }

        private void PartnerHozzaadasa_Click(object sender, RoutedEventArgs e)
        {

        }

        private void FiuTestverHozzaadasa_Click(object sender, RoutedEventArgs e)
        {

        }

        private void LanyTestverHozzaadasa_Click(object sender, RoutedEventArgs e)
        {

        }

        private void AnyaHozzaadasa_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ApaHozzaadasa_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void Kijelentkezes_Click(object sender, RoutedEventArgs e)
        {

            // Navigálás a BejelentkezesPage oldalra.
            ((MainWindow)System.Windows.Application.Current.MainWindow).MainFrame.Navigate(new BejelentkezesPage(_context));
        }
    }
}

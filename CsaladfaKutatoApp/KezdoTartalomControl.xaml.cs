using CsaladfaKutatoApp.Models;
using CsaladfaKutatoApp.Models.DTO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
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


namespace CsaladfaKutatoApp
{
    /// <summary>
    /// Interaction logic for KezdoTartalomControl.xaml
    /// </summary>
    public partial class KezdoTartalomControl : UserControl
    {
        private KozpontiPage KpOldal;
        private readonly CsaladfaAdatbazisContext _context;

        public KezdoTartalomControl(KozpontiPage Szulo, CsaladfaAdatbazisContext context)
        {
            InitializeComponent();
            KpOldal = Szulo;
            _context = context;
         
        }

        private void NavigalKapcsolodoLetrehoz(string kapcsolatTipus)
        {
            var control = new KapcsolodoSzemelyLetrehozControl(KpOldal, _context, kapcsolatTipus, KpOldal.LegutobbKijeloltSzemely);
            // Ha van legutóbb kijelölt, beállítjuk
            if (KpOldal.LegutobbKijeloltSzemely is not null)
            {
                control.DataContext = KpOldal.LegutobbKijeloltSzemely;
                
            }
            control.KapcsolatTipusBeallitas(kapcsolatTipus);
            KpOldal.TartalomValtas(control);

        }

        private void Kapcsolatok_Click(object sender, RoutedEventArgs e)
        {
            var control = new SzemelyKapcsolataiControl(KpOldal, _context, KpOldal.LegutobbKijeloltSzemely);
            if (KpOldal.LegutobbKijeloltSzemely is not null)
            {
                control.DataContext = KpOldal.LegutobbKijeloltSzemely;

            }
            KpOldal.TartalomValtas(control);
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void NemKapcsolodoHozzaadasa_Click(object sender, RoutedEventArgs e) => NavigalKapcsolodoLetrehoz("Nem kapcsolódó személy");
        private void ApaHozzaadasa_Click(object sender, RoutedEventArgs e) => NavigalKapcsolodoLetrehoz("Apa");
        private void AnyaHozzaadasa_Click(object sender, RoutedEventArgs e) => NavigalKapcsolodoLetrehoz("Anya");
        private void LanyaHozzaadasa_Click(object sender, RoutedEventArgs e) => NavigalKapcsolodoLetrehoz("Lány gyermek");
        private void FiaHozzaadasa_Click(object sender, RoutedEventArgs e) => NavigalKapcsolodoLetrehoz("Fiú gyermek");
        private void PartnerHozzaadasa_Click(object sender, RoutedEventArgs e) => NavigalKapcsolodoLetrehoz("Partner");
        private void LanyTestverHozzaadasa_Click(object sender, RoutedEventArgs e) => NavigalKapcsolodoLetrehoz("Lány testvér");
        private void FiuTestverHozzaadasa_Click(object sender, RoutedEventArgs e) => NavigalKapcsolodoLetrehoz("Fiú testvér");



        private CsaladfaAdatbazisContext UjDbContextPeldany()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<CsaladfaAdatbazisContext>();
            optionsBuilder.UseSqlServer(config.GetConnectionString("DefaultConnection"));

            return new CsaladfaAdatbazisContext(optionsBuilder.Options);
        }


        private void TorlesButton_Click(object sender, RoutedEventArgs e)
        {
            var kijeloltSzemely = KpOldal.LegutobbKijeloltSzemely;
            if (kijeloltSzemely == null)
            {
                MessageBox.Show("Nincs kijelölt személy.", "Figyelmeztetés", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Megerősítő ablak
            var result = MessageBox.Show(
                $"Biztosan törölni kívánja a következő személyt?\n\n{kijeloltSzemely.KNev} {kijeloltSzemely.VNev}",
                "Törlés megerősítése",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    using (var context = UjDbContextPeldany())
                    {
                        var szemely = context.Szemelyeks
                            .Include(sz => sz.KapcsolatokSzemelies)
                            .Include(sz => sz.KapcsolatokKapcsolodoSzemelies)
                            .Include(sz => sz.Fotoks)
                            .Include(sz => sz.Forrasoks)
                            .Include(sz => sz.Mellekleteks)
                            .Include(sz => sz.Torteneteks)
                            .FirstOrDefault(sz => sz.SzemelyId == kijeloltSzemely.Azonosito);

                        if (szemely != null)
                        {
                            // Kapcsolatok törlése
                            context.Kapcsolatoks.RemoveRange(szemely.KapcsolatokSzemelies);
                            context.Kapcsolatoks.RemoveRange(szemely.KapcsolatokKapcsolodoSzemelies);

                            // Fotókhoz tartozó történetek törlése
                            foreach (var foto in szemely.Fotoks)
                            {
                                context.Torteneteks.RemoveRange(foto.Torteneteks);
                            }
                            context.Fotoks.RemoveRange(szemely.Fotoks);

                            // További kapcsolódó rekordok törlése
                            context.Forrasoks.RemoveRange(szemely.Forrasoks);
                            context.Mellekleteks.RemoveRange(szemely.Mellekleteks);
                            context.Torteneteks.RemoveRange(szemely.Torteneteks);

                            // Ha szükséges, ellenőrizd a Helyszin törlését (csak akkor, ha nem tartozik más személyhez)
                            if (szemely.HelyszinId.HasValue)
                            {
                                var helyszin = context.Helyszineks
                                    .Include(h => h.Szemelyeks)
                                    .FirstOrDefault(h => h.HelyszinId == szemely.HelyszinId);

                                if (helyszin != null && helyszin.Szemelyeks.Count == 1) // csak ez a személy tartozik hozzá
                                {
                                    context.Helyszineks.Remove(helyszin);
                                }
                            }

                            // Végül maga a személy törlése
                            context.Szemelyeks.Remove(szemely);

                            context.SaveChanges();
                        }
                    }

                    // Canvas frissítése
                    KpOldal.TorolSzemelyACanvasrol(kijeloltSzemely);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Hiba történt a törlés során: " + ex.Message, "Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        
    }
}

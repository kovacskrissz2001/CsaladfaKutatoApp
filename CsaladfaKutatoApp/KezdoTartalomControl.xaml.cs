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
        private int? felhasznaloId = ((MainWindow)Application.Current.MainWindow).BejelentkezettFelhasznaloId;



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

        private void NemKapcsolodoHozzaadasa_Click(object sender, RoutedEventArgs e)
        {
           
            KpOldal.LegutobbiKapcsolatTipus = "Nem kapcsolódó személy";
            NavigalKapcsolodoLetrehoz("Nem kapcsolódó személy");
        }

        private void LanyaHozzaadasa_Click(object sender, RoutedEventArgs e) {
            KpOldal.LegutobbiKapcsolatTipus = "Lány gyermek";
            NavigalKapcsolodoLetrehoz("Lány gyermek"); 
        }
        private void FiaHozzaadasa_Click(object sender, RoutedEventArgs e)
        {
            KpOldal.LegutobbiKapcsolatTipus = "Fiú gyermek";
            NavigalKapcsolodoLetrehoz("Fiú gyermek");
        }
        private void PartnerHozzaadasa_Click(object sender, RoutedEventArgs e)
        {
            KpOldal.LegutobbiKapcsolatTipus = "Partner";
            NavigalKapcsolodoLetrehoz("Partner");
        }
        



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

        private void KovetkezoButton_Click(object sender, RoutedEventArgs e)
        {
            // Új kijelölés: következő kisebb ID-jú személy
            var kovetkezoSzemely = KpOldal.RajzoltSzemelyek
                .Where(s => s.Azonosito > KpOldal.LegutobbKijeloltSzemely.Azonosito)
                .OrderBy(s => s.Azonosito)
                .FirstOrDefault();


            // Aktuális KezdoTartalomControl elérése
            if (KpOldal.TartalomValto.Content is KezdoTartalomControl aktivTartalom)
            {
                if (kovetkezoSzemely != null)
                {
                    // Előző kijelölés eltüntetése
                    if (KpOldal.kijeloltBorder != null)
                        KpOldal.kijeloltBorder.BorderBrush = Brushes.Transparent;

                    KpOldal.LegutobbKijeloltSzemely = kovetkezoSzemely;

                    // Kijelölés vizuális frissítése (border szín változtatással)

                    if (KpOldal.LegutobbKijeloltSzemely != null && KpOldal.LegutobbKijeloltSzemely.UIElem != null && KpOldal.LegutobbKijeloltSzemely?.UIElem is Border ujBorder)
                    {
                        ujBorder.BorderBrush = Brushes.OrangeRed;
                        KpOldal.kijeloltBorder = ujBorder;
                    }
                    aktivTartalom.DataContext = KpOldal.LegutobbKijeloltSzemely;

                }
            }

        }

        private void ElozoButton_Click(object sender, RoutedEventArgs e)
        {
            // Új kijelölés: következő kisebb ID-jú személy
            var elozoSzemely = 
            KpOldal.RajzoltSzemelyek
                .Where(s => s.Azonosito < KpOldal.LegutobbKijeloltSzemely.Azonosito)
                .OrderByDescending(s => s.Azonosito)
                .FirstOrDefault();


            // Aktuális KezdoTartalomControl elérése
            if (KpOldal.TartalomValto.Content is KezdoTartalomControl aktivTartalom)
                {
                    if (elozoSzemely != null)
                    {
                        // Előző kijelölés eltüntetése
                        if (KpOldal.kijeloltBorder != null)
                            KpOldal.kijeloltBorder.BorderBrush = Brushes.Transparent;

                        KpOldal.LegutobbKijeloltSzemely = elozoSzemely;

                        // Kijelölés vizuális frissítése (border szín változtatással)

                        if (KpOldal.LegutobbKijeloltSzemely != null && KpOldal.LegutobbKijeloltSzemely.UIElem != null && KpOldal.LegutobbKijeloltSzemely?.UIElem is Border ujBorder)
                        {
                            ujBorder.BorderBrush = Brushes.OrangeRed;
                            KpOldal.kijeloltBorder = ujBorder;
                        }
                        aktivTartalom.DataContext = KpOldal.LegutobbKijeloltSzemely;
                    }
                    
             }
            
            
        }
    }
}

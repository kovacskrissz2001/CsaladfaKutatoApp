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
using CsaladfaKutatoApp.Segedeszkozok;


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
        



        


        private void TorlesButton_Click(object sender, RoutedEventArgs e)
        {
           Torles torol = new Torles();

            torol.TorolSzemelyt(KpOldal);
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

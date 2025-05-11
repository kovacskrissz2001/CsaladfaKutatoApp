using CsaladfaKutatoApp.Models;
using CsaladfaKutatoApp.Segedeszkozok;
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
    /// Interaction logic for SzemelySzerkeszteseControl.xaml
    /// </summary>
    public partial class SzemelySzerkeszteseControl : UserControl
    {

        KozpontiPage KpOldal;
        CsaladfaAdatbazisContext _context;
        private readonly int? felhasznaloId = ((MainWindow)Application.Current.MainWindow).BejelentkezettFelhasznaloId;


        public SzemelySzerkeszteseControl(KozpontiPage kpolad, CsaladfaAdatbazisContext context)
        {
            InitializeComponent();

            KpOldal = kpolad;
            _context = context;
        }

        private void Modositas_Click(object sender, RoutedEventArgs e)
        {

            int szemelyId = KpOldal.LegutobbKijeloltSzemely.Azonosito;

            //Ezek már bizosan benne vannak az adatbázisba.
            var szemely = _context.Szemelyeks.FirstOrDefault(s => s.SzemelyId == szemelyId);
            var helyszin = _context.Helyszineks.FirstOrDefault(h => h.HelyszinId == szemely.HelyszinId);


            // Személy mezők frissítése, ha van változás
            if (!string.IsNullOrWhiteSpace(KeresztnevTextBox.Text))
                szemely.Keresztnev = KeresztnevTextBox.Text.Trim();
            if (!string.IsNullOrWhiteSpace(VezeteknevTextBox.Text))
                szemely.Vezeteknev = VezeteknevTextBox.Text.Trim();



            if (!string.IsNullOrWhiteSpace(SzuletesiDatumTextBox.Text))
            {
                // Születési dátum formátum ellenőrzés
                if (!DateOnly.TryParseExact(SzuletesiDatumTextBox.Text.Trim(), "yyyy-MM-dd", out DateOnly szuletesiDatum))
                {
                    MessageBox.Show("A születési dátum formátuma érvénytelen! Helyes formátum Pl: 1990-05-01", "Hibás dátum", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                else
                    szemely.SzuletesiDatum = szuletesiDatum;
            }
            

            if (!string.IsNullOrWhiteSpace(HalalDatumTextBox.Text))
            {
                if (!DateOnly.TryParseExact(HalalDatumTextBox.Text.Trim(), "yyyy-MM-dd", out DateOnly halalDatum))
                {
                    MessageBox.Show("A halálozási dátum formátuma érvénytelen! Helyes formátum Pl: 1990-05-01", "Hibás dátum", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                else
                    szemely.HalalozasiDatum = halalDatum;
            }

            //Összehasonlítsuk hogy az adatbázisban szereplő és a most megadott érték megegyezik-e vagy változott.
            bool eloSzemelyAdatbazisban = _context.Szemelyeks
            .Where(s => s.SzemelyId == szemelyId)
            .Select(s => s.EloSzemely)
            .FirstOrDefault();

            bool valtozottAzEloSzemelyErtek = false;

            if (EloSzemelyCheckBox.IsChecked == true && eloSzemelyAdatbazisban == true)
                valtozottAzEloSzemelyErtek = false;
            if (EloSzemelyCheckBox.IsChecked == false && eloSzemelyAdatbazisban == false)
                valtozottAzEloSzemelyErtek = false;
            if (EloSzemelyCheckBox.IsChecked == true && eloSzemelyAdatbazisban == false)
            {
                valtozottAzEloSzemelyErtek = true;
                szemely.EloSzemely = true;
            }
            if (EloSzemelyCheckBox.IsChecked == false && eloSzemelyAdatbazisban == true)
            {
                valtozottAzEloSzemelyErtek = true;
                szemely.EloSzemely = false;
            }


            if (!string.IsNullOrWhiteSpace(TanulmanyaiTextBox.Text))
                szemely.Tanulmanya = TanulmanyaiTextBox.Text.Trim();
            if (!string.IsNullOrWhiteSpace(FoglalkozasaTextBox.Text))
                szemely.Foglalkozasa = FoglalkozasaTextBox.Text.Trim();
            if (!string.IsNullOrWhiteSpace(VallasaTextBox.Text))
                szemely.Vallasa = VallasaTextBox.Text.Trim();

            // Helyszín mezők frissítése
            if (!string.IsNullOrWhiteSpace(SzuletesiOrszagTextBox.Text))
                helyszin.SzuletesiOrszag = SzuletesiOrszagTextBox.Text.Trim();
            if (!string.IsNullOrWhiteSpace(SzuletesiTelepulesTextBox.Text))
                helyszin.SzuletesiTelepules = SzuletesiTelepulesTextBox.Text.Trim();
            if (!string.IsNullOrWhiteSpace(SzuletesiRegioTextBox.Text))
                helyszin.SzuletesiRegio = SzuletesiRegioTextBox.Text.Trim();
            if (!string.IsNullOrWhiteSpace(HalalhelyOrszagTextBox.Text))
                helyszin.HalalozasiOrszag = HalalhelyOrszagTextBox.Text.Trim();
            if (!string.IsNullOrWhiteSpace(HalalhelyRegioTextBox.Text))
                helyszin.HalalozasiRegio = HalalhelyRegioTextBox.Text.Trim();
            if (!string.IsNullOrWhiteSpace(HalalhelyTelepulesTextBox.Text))
                helyszin.HalalozasiTelepules = HalalhelyTelepulesTextBox.Text.Trim();
            if (!string.IsNullOrWhiteSpace(OrokNyugalomHelyeOrszagTextBox.Text))
                helyszin.OrokNyugalomHelyeOrszag = OrokNyugalomHelyeOrszagTextBox.Text.Trim();
            if (!string.IsNullOrWhiteSpace(OrokNyugalomHelyeRegioTextBox.Text))
                helyszin.OrokNyugalomHelyeRegio = OrokNyugalomHelyeRegioTextBox.Text.Trim();
            if (!string.IsNullOrWhiteSpace(OrokNyugalomHelyeTelepulesTextBox.Text))
                helyszin.OrokNyugalomHelyeTelepules = OrokNyugalomHelyeTelepulesTextBox.Text.Trim();


            // Ellenőrizzük, hogy történt-e bármilyen tényleges módosítás
            bool semmitNemValtozott =
                string.IsNullOrWhiteSpace(KeresztnevTextBox.Text) &&
                string.IsNullOrWhiteSpace(VezeteknevTextBox.Text) &&
                string.IsNullOrWhiteSpace(SzuletesiDatumTextBox.Text) &&
                string.IsNullOrWhiteSpace(HalalDatumTextBox.Text) &&
                valtozottAzEloSzemelyErtek == false &&
                string.IsNullOrWhiteSpace(TanulmanyaiTextBox.Text) &&
                string.IsNullOrWhiteSpace(FoglalkozasaTextBox.Text) &&
                string.IsNullOrWhiteSpace(VallasaTextBox.Text) &&
                string.IsNullOrWhiteSpace(SzuletesiOrszagTextBox.Text) &&
                string.IsNullOrWhiteSpace(SzuletesiRegioTextBox.Text) &&
                string.IsNullOrWhiteSpace(SzuletesiTelepulesTextBox.Text) &&
                string.IsNullOrWhiteSpace(HalalhelyOrszagTextBox.Text) &&
                string.IsNullOrWhiteSpace(HalalhelyRegioTextBox.Text) &&
                string.IsNullOrWhiteSpace(HalalhelyTelepulesTextBox.Text) &&
                string.IsNullOrWhiteSpace(OrokNyugalomHelyeOrszagTextBox.Text) &&
                string.IsNullOrWhiteSpace(OrokNyugalomHelyeRegioTextBox.Text) &&
                string.IsNullOrWhiteSpace(OrokNyugalomHelyeTelepulesTextBox.Text);

            // A kötelező mezőket mindig frissítjük, tehát csak opcionális mezők változása esetén jelezzük, ha nincs változás
            if (semmitNemValtozott)
            {
                MessageBox.Show("Nem történt változás a személy adatait illetően.", "Nincs változás", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            else
            {
                // Mentés
                _context.SaveChanges();
                MessageBox.Show("A kijelölt személy adatai sikeresen frissítve lettek.", "Siker", MessageBoxButton.OK, MessageBoxImage.Information);
                ((MainWindow)Application.Current.MainWindow).MainFrame.Navigate(new KozpontiPage(_context, felhasznaloId));

            }

            
        }


        private void Vissza_Click(object sender, RoutedEventArgs e)
        {
            var kezdo = new KezdoTartalomControl(KpOldal, _context);

            // Ha van legutóbb kijelölt, beállítjuk
            if (KpOldal.LegutobbKijeloltSzemely is not null)
            {
                kezdo = new KezdoTartalomControl(KpOldal, _context);
                kezdo.DataContext = KpOldal.LegutobbKijeloltSzemely;
                kezdo.BetoltSzemelyKepet();
                kezdo.ToltsdBeSzemelyAdatokatListViewhoz();
            }

            KpOldal.TartalomValtas(kezdo);
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

            PopupUnezet.Text = "A Módosítás gombra kattintva módosítjuk a kijelölt személy adatait a családfában. " +
                "A dátum típusú mezőkhöz YYYY-MM-DD formátumú adatot szíveskedjenek megadni! " +
                "Azokat a mezőket kell csak kitölteni, amelyeket módosítani szertené. " +
                "A Régió tipúsú mezők alatt megyét, tartományt vagy államot értünk. "+
                "Figyelem A CheckBox ki nem töltése is okozhat változást az adatbázisban, "+
                "ha a korábbi érték igaz volt itt viszont, hamis lesz, ha nincs kitöltve! ";
                
        }

        private void TorlesButton_Click(object sender, RoutedEventArgs e)
        {
            Torles torol = new Torles();

            torol.TorolSzemelyt(KpOldal);
        }

        private void ElozoButton_Click(object sender, RoutedEventArgs e)
        {
            // Új kijelölés: következő kisebb ID-jú személy
            var elozoSzemely = KpOldal.RajzoltSzemelyek
                .Where(s => s.Azonosito < KpOldal.LegutobbKijeloltSzemely.Azonosito)
                .OrderByDescending(s => s.Azonosito)
                .FirstOrDefault();

            if (KpOldal.TartalomValto.Content is SzemelySzerkeszteseControl aktivTartalom)
            {
                if (elozoSzemely != null)
                {

                    if (KpOldal.LegutobbiKapcsolatTipus != "")
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


                        //KezdoTartalomControl kezdoTartalom = new KezdoTartalomControl(this, _context);
                        aktivTartalom = new SzemelySzerkeszteseControl(KpOldal, _context);
                        aktivTartalom.DataContext = KpOldal.LegutobbKijeloltSzemely;
                        KpOldal.TartalomValto.Content = aktivTartalom;
                    }
                }

            }
        }

        private void EloSzemelyCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            HalalDatumTextBox.IsEnabled = false;
            HalalhelyOrszagTextBox.IsEnabled = false;
            HalalhelyRegioTextBox.IsEnabled = false;
            HalalhelyTelepulesTextBox.IsEnabled = false;
            OrokNyugalomHelyeOrszagTextBox.IsEnabled = false;
            OrokNyugalomHelyeRegioTextBox.IsEnabled = false;
            OrokNyugalomHelyeTelepulesTextBox.IsEnabled = false;
        }

        private void EloSzemelyCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            HalalDatumTextBox.IsEnabled = true;
            HalalhelyOrszagTextBox.IsEnabled = true;
            HalalhelyRegioTextBox.IsEnabled = true;
            HalalhelyTelepulesTextBox.IsEnabled = true;
            OrokNyugalomHelyeOrszagTextBox.IsEnabled = true;
            OrokNyugalomHelyeRegioTextBox.IsEnabled = true;
            OrokNyugalomHelyeTelepulesTextBox.IsEnabled = true;
        }

        private void KovetkezoButton_Click(object sender, RoutedEventArgs e)
        {
            // Új kijelölés: következő kisebb ID-jú személy
            var kovetkezoSzemely =
            KpOldal.RajzoltSzemelyek
                .Where(s => s.Azonosito > KpOldal.LegutobbKijeloltSzemely.Azonosito)
                .OrderBy(s => s.Azonosito)
                .FirstOrDefault();



            if (KpOldal.TartalomValto.Content is SzemelySzerkeszteseControl aktivTartalom)
            {
                if (kovetkezoSzemely != null)
                {

                    if (KpOldal.LegutobbiKapcsolatTipus != "")
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


                        //KezdoTartalomControl kezdoTartalom = new KezdoTartalomControl(this, _context);
                        aktivTartalom = new SzemelySzerkeszteseControl(KpOldal, _context);
                        aktivTartalom.DataContext = KpOldal.LegutobbKijeloltSzemely;
                        KpOldal.TartalomValto.Content = aktivTartalom;
                    }
                }

            }
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
    }
}

using CsaladfaKutatoApp.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
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
using System.Data.Common;
using CsaladfaKutatoApp.Models.DTO;

namespace CsaladfaKutatoApp
{
    /// <summary>
    /// Interaction logic for KapcsolodoSzemelyLetrehozControl.xaml
    /// </summary>
    public partial class KapcsolodoSzemelyLetrehozControl : UserControl
    {
        private KozpontiPage KpOldal;
        private readonly CsaladfaAdatbazisContext _context;
        private int? _felhasznaloId = ((MainWindow)System.Windows.Application.Current.MainWindow).BejelentkezettFelhasznaloId ?? 0;
        private string _kapcsolatTipus;
        private RajzoltSzemely _legutobbKijeloltSzemely;
        private string kapcsolatTipusAdatbazisba;



        public KapcsolodoSzemelyLetrehozControl(KozpontiPage kpoldal, CsaladfaAdatbazisContext context,
           string kapcsolatTipus, RajzoltSzemely legutobbKijeloltSzemely)
        {
            InitializeComponent();
            KpOldal = kpoldal;
            _context = context;
            _kapcsolatTipus = kapcsolatTipus;
            _legutobbKijeloltSzemely = legutobbKijeloltSzemely;
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

            PopupUnezet.Text = "A Hozzáadás gombra kattintva hozzáadhatjuk az új személyt a családfához. " +
                "A dátum típusú mezőkhöz YYYY-MM-DD formátumú adatot szíveskedjenek megadni! " +
                " Kötelező kitöltendő mezők a Keresztnév, Vezetéknév, Neme, Születési dátum, Születési Ország, Születési Település. A többi mező kitöltése opcionális. " +
                "A Régió tipúsú mezők alatt megyét, tartományt vagy államot értünk.";

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

        private void Vissza_Click(object sender, RoutedEventArgs e)
        {
            var kezdo = new KezdoTartalomControl(KpOldal, _context);

            // Ha van legutóbb kijelölt, beállítjuk
            if (KpOldal.LegutobbKijeloltSzemely is not null)
            {
                kezdo.DataContext = KpOldal.LegutobbKijeloltSzemely;
            }

            KpOldal.TartalomValtas(kezdo);
            
        }

        private bool VanKozepenVesszo(string szoveg)
        {
            if (string.IsNullOrWhiteSpace(szoveg))
                return false;

            int vesszoIndex = szoveg.IndexOf(",");
            return vesszoIndex > 0 && vesszoIndex < szoveg.Length - 1;
        }

        public void KapcsolatTipusBeallitas(string tipus)
        {
            KapcsolodoSzemelyTextBlock.Text = tipus+" "+"hozzáadása";

            // RadioButton logika
            if (tipus == "Apa" || tipus == "Fiú gyermek" || tipus == "Fiú testvér")
            {
                FerfiRadioButton.IsChecked = true;
                FerfiRadioButton.IsEnabled = false;
                NoRadioButton.IsEnabled = false;
            }
            else if (tipus == "Anya" || tipus == "Lány gyermek" || tipus == "Lány testvér")
            {
                NoRadioButton.IsChecked = true;
                FerfiRadioButton.IsEnabled = false;
                NoRadioButton.IsEnabled = false;
            }
            else if (tipus == "Nem kapcsolódó személy")
            {
                FerfiRadioButton.IsEnabled = true;
                NoRadioButton.IsEnabled = true;
            }
            else if (tipus == "Partner" && _legutobbKijeloltSzemely.Nem == "Nő")
            {
                FerfiRadioButton.IsChecked = true;
                FerfiRadioButton.IsEnabled = false;
                NoRadioButton.IsEnabled = false;

                kapcsolatTipusAdatbazisba = "Partner";
            }
            else if (tipus == "Partner" && _legutobbKijeloltSzemely.Nem == "Férfi")
            {
                NoRadioButton.IsChecked = true;
                FerfiRadioButton.IsEnabled = false;
                NoRadioButton.IsEnabled = false;

                kapcsolatTipusAdatbazisba = "Partner";

                if (tipus == "Lány gyermek" || tipus == "Fiú gyermek")
                {
                    kapcsolatTipusAdatbazisba = "Gyermek";
                }
            }
        }

        //Ellenőtizzük vele, hogy az új kapcsolat létrehozásánál létre lett-e hozva korábban ennek a személynek párkapcsolat,
        //van-e rekord a Kapcsolatok táblában, ami a megadott személyhez tartozik és a kapcsolatTipus mező a Partner értéket veszi fel.
        public bool VaneMarParkapcsolatNo(int szemelyAzonosito)
        {
            bool vanKapcsolat = _context.Kapcsolatoks
            .Any(k => k.SzemelyId == szemelyAzonosito && k.KapcsolatTipusa == "Partner");

            
            return vanKapcsolat;
        }

        public bool VaneMarParkapcsolatFerfi(int kapcsSzemelyAzonosito)
        {
            bool vanKapcsolat = _context.Kapcsolatoks
            .Any(k => k.KapcsolodoSzemelyId == kapcsSzemelyAzonosito && k.KapcsolatTipusa == "Partner");


            return vanKapcsolat;
        }

        //Ellenőrizzük hogy tartozik-e már az adott szülőhöz konkrét gyerek.
        public bool VaneMarIlyenGyerek(int szemelyAzonosito, int kapcsSzemelyAzonosito)
        {
            bool vanKapcsolat = _context.Kapcsolatoks
            .Any(k => k.SzemelyId == szemelyAzonosito && k.KapcsolodoSzemelyId == kapcsSzemelyAzonosito && k.KapcsolatTipusa == "Gyermek");

            return vanKapcsolat;

        }
        public Helyszinek ujHelyszinVisszaad()
        {
            var ujHelyszin = new Helyszinek
            {
                SzuletesiOrszag = SzuletesiOrszagTextBox.Text.Trim(),
                SzuletesiRegio = string.IsNullOrWhiteSpace(SzuletesiRegioTextBox.Text) ? null : SzuletesiRegioTextBox.Text.Trim(),
                SzuletesiTelepules = SzuletesiTelepulesTextBox.Text.Trim(),
                HalalozasiOrszag = string.IsNullOrWhiteSpace(HalalhelyOrszagTextBox.Text) ? null : HalalhelyOrszagTextBox.Text.Trim(),
                HalalozasiRegio = string.IsNullOrWhiteSpace(HalalhelyRegioTextBox.Text) ? null : HalalhelyRegioTextBox.Text.Trim(),
                HalalozasiTelepules = string.IsNullOrWhiteSpace(HalalhelyTelepulesTextBox.Text) ? null : HalalhelyTelepulesTextBox.Text.Trim(),
                OrokNyugalomHelyeOrszag = string.IsNullOrWhiteSpace(OrokNyugalomHelyeOrszagTextBox.Text) ? null : OrokNyugalomHelyeOrszagTextBox.Text.Trim(),
                OrokNyugalomHelyeRegio = string.IsNullOrWhiteSpace(OrokNyugalomHelyeRegioTextBox.Text) ? null : OrokNyugalomHelyeRegioTextBox.Text.Trim(),
                OrokNyugalomHelyeTelepules = string.IsNullOrWhiteSpace(OrokNyugalomHelyeTelepulesTextBox.Text) ? null : OrokNyugalomHelyeTelepulesTextBox.Text.Trim()
                
            };
            _context.Helyszineks.Add(ujHelyszin);
            _context.SaveChanges();


            return ujHelyszin;
        }

        public Szemelyek UjSzemelyVisszaad(DateTime szDatum, DateTime? hDatum, Helyszinek ujHelyszin)
        {
            // Új személy létrehozása
            var ujSzemely = new Szemelyek
            {
                Vezeteknev = VezeteknevTextBox.Text.Trim(),
                Keresztnev = KeresztnevTextBox.Text.Trim(),
                Neme = FerfiRadioButton.IsChecked == true ? "Férfi" : "Nő",
                SzuletesiDatum = DateOnly.FromDateTime(szDatum),
                HalalozasiDatum = hDatum.HasValue ? DateOnly.FromDateTime(hDatum.Value) : null,
                EloSzemely = EloSzemelyCheckBox.IsChecked == true ? true : false,
                Tanulmanya = string.IsNullOrEmpty(TanulmanyaiTextBox.Text) ? null : TanulmanyaiTextBox.Text.Trim(),
                Foglalkozasa = string.IsNullOrEmpty(FoglalkozasaTextBox.Text) ? null : FoglalkozasaTextBox.Text.Trim(),
                Vallasa = string.IsNullOrEmpty(VallasaTextBox.Text) ? null : VallasaTextBox.Text.Trim(),
                FelhasznaloId = _felhasznaloId ?? 0,
                HelyszinId = ujHelyszin.HelyszinId
            };
            _context.Szemelyeks.Add(ujSzemely);
            _context.SaveChanges();

            return ujSzemely;
        }

        public void TorolUjonnanLetrehozottSzemelytEsHelyszint(Szemelyek ujSzemely, Helyszinek ujHelyszin)
        {
            _context.Szemelyeks.Remove(ujSzemely);
            _context.Helyszineks.Remove(ujHelyszin);
            _context.SaveChanges();
        }

        private void Hozzaadas_Click(object sender, RoutedEventArgs e)
        {
            // Kötelező mezők ellenőrzése
            if (string.IsNullOrWhiteSpace(KeresztnevTextBox.Text) ||
                string.IsNullOrWhiteSpace(VezeteknevTextBox.Text) ||
                string.IsNullOrWhiteSpace(SzuletesiDatumTextBox.Text) ||
                string.IsNullOrWhiteSpace(SzuletesiOrszagTextBox.Text) ||
                string.IsNullOrWhiteSpace(SzuletesiTelepulesTextBox.Text) ||
                (FerfiRadioButton.IsChecked == false && NoRadioButton.IsChecked == false))
            {
                MessageBox.Show("Kérlek, töltsd ki az összes kötelező mezőt!", "Hiányzó adatok", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Dátum formátum ellenőrzése (YYYY-MM-DD)
            if (!DateTime.TryParseExact(SzuletesiDatumTextBox.Text, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out DateTime szuletesiDatum))
            {
                MessageBox.Show("A születési dátum formátuma nem megfelelő! (Pl: 1990-05-01)", "Dátum hiba", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DateTime? halalozasiDatum = null;
            if (!string.IsNullOrWhiteSpace(HalalDatumTextBox.Text))
            {
                if (!DateTime.TryParseExact(HalalDatumTextBox.Text, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out DateTime parsedDate))
                {
                    MessageBox.Show("A halálozási dátum formátuma nem megfelelő! (Pl: 1990-05-01)", "Dátum hiba", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                halalozasiDatum = parsedDate;
            }
           


            //Megnézzük, hogy szerepel-e már a személy az adatbázisban és ha nem csak akkor adjuk hozzá.
            var neme = FerfiRadioButton.IsChecked == true ? "Férfi" : "Nő";

            bool letezikSzemely = _context.Szemelyeks
            .Include(s => s.Helyszin)
            .Any(s =>
                s.Keresztnev == KeresztnevTextBox.Text.Trim() &&
                s.Vezeteknev == VezeteknevTextBox.Text.Trim() &&
                s.Neme == neme &&
                s.SzuletesiDatum == DateOnly.FromDateTime(szuletesiDatum) &&
                s.FelhasznaloId == _felhasznaloId &&
                s.Helyszin != null &&
                s.Helyszin.SzuletesiOrszag == SzuletesiOrszagTextBox.Text.Trim() &&
                s.Helyszin.SzuletesiTelepules == SzuletesiTelepulesTextBox.Text.Trim());

            //ellenőrizni kell,hogy a családfában már szereplő embereket ne lehessen hozzáadni senkihez
            //Ezt úgy érjük el hogy ellenőrizzük van e már a személyhet tartozó rekord Gyermek típussa, úgy hogy ő a KapcsSzemelyId
            //azt már ellenőrizük hogy az adott személy meglévő gyrekét ne adhassuk újra a szülőhöz
            //azt is ellenőrizni kell hogy más gyerekét se
            //illetve azt is ellenőrizni kell hogy rokonokat ne adhassunk parnerként egyáshoz

            try
            {
                if (letezikSzemely && _kapcsolatTipus == "Nem kapcsolódó személy")
                {
                    MessageBox.Show("Ilyen személy már szerepel az adatbázisban!", "Figyelem", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                else
                {
                    //kapcsolódás az adatbázishoz
                    var kapcsolat = _context.Database.GetDbConnection();
                    if (_kapcsolatTipus != null && _kapcsolatTipus == "Nem kapcsolódó személy" && _legutobbKijeloltSzemely != null)
                    {
                        using (var parancs = kapcsolat.CreateCommand())
                        {
                            kapcsolat.Open();
                            parancs.CommandText = "sp_HozzaadNemKapcsolodoSzemely";
                            parancs.CommandType = CommandType.StoredProcedure;

                            // Paraméterek
                            void AddParam(string name, object value)
                            {
                                var p = parancs.CreateParameter();
                                p.ParameterName = name;
                                p.Value = value ?? DBNull.Value;
                                parancs.Parameters.Add(p);
                            }

                            AddParam("@Keresztnev", KeresztnevTextBox.Text.Trim());
                            AddParam("@Vezeteknev", VezeteknevTextBox.Text.Trim());
                            AddParam("@SzuletesiDatum", szuletesiDatum);
                            AddParam("@HalalDatum", string.IsNullOrWhiteSpace(HalalDatumTextBox.Text) ? null : HalalDatumTextBox.Text.Trim());
                            AddParam("@Neme", FerfiRadioButton.IsChecked == true ? "Férfi" : "Nő");
                            AddParam("@EloSzemely", EloSzemelyCheckBox.IsChecked == true ? 1 : 0);
                            AddParam("@FelhasznaloId", _felhasznaloId); //  dinamikusan jön)

                            AddParam("@SzuletesiOrszag", SzuletesiOrszagTextBox.Text.Trim());
                            AddParam("@SzuletesiTelepules", SzuletesiTelepulesTextBox.Text.Trim());
                            AddParam("@SzuletesiRegio", string.IsNullOrWhiteSpace(SzuletesiRegioTextBox.Text) ? null : SzuletesiRegioTextBox.Text.Trim());
                            AddParam("@HalalozasiOrszag", string.IsNullOrWhiteSpace(HalalhelyOrszagTextBox.Text) ? null : HalalhelyOrszagTextBox.Text.Trim());
                            AddParam("@HalalozasiRegio", string.IsNullOrWhiteSpace(HalalhelyRegioTextBox.Text) ? null : HalalhelyRegioTextBox.Text.Trim());
                            AddParam("@HalalozasiTelepules", string.IsNullOrWhiteSpace(HalalhelyTelepulesTextBox.Text) ? null : HalalhelyTelepulesTextBox.Text.Trim());
                            AddParam("@OrokNyugalomHelyeOrszag", string.IsNullOrWhiteSpace(OrokNyugalomHelyeOrszagTextBox.Text) ? null : OrokNyugalomHelyeOrszagTextBox.Text.Trim());
                            AddParam("@OrokNyugalomHelyeRegio", string.IsNullOrWhiteSpace(OrokNyugalomHelyeRegioTextBox.Text) ? null : OrokNyugalomHelyeRegioTextBox.Text.Trim());
                            AddParam("@OrokNyugalomHelyeTelepules", string.IsNullOrWhiteSpace(OrokNyugalomHelyeTelepulesTextBox.Text) ? null : OrokNyugalomHelyeTelepulesTextBox.Text.Trim());

                            AddParam("@Foglalkozas", string.IsNullOrWhiteSpace(FoglalkozasaTextBox.Text) ? null : FoglalkozasaTextBox.Text.Trim());
                            AddParam("@Tanulmany", string.IsNullOrWhiteSpace(TanulmanyaiTextBox.Text) ? null : TanulmanyaiTextBox.Text.Trim());
                            AddParam("@Vallas", string.IsNullOrWhiteSpace(VallasaTextBox.Text) ? null : VallasaTextBox.Text.Trim());

                            // Kapcsolat megnyitása, ha még nincs
                            if (kapcsolat.State != System.Data.ConnectionState.Open)
                                kapcsolat.Open();

                            parancs.ExecuteNonQuery();

                            kapcsolat.Close();

                            MessageBox.Show("A személy vagy kapcsolat sikeresen hozzáadásra került!", "Siker", MessageBoxButton.OK, MessageBoxImage.Information);

                            ((MainWindow)Application.Current.MainWindow).MainFrame.Navigate(new KozpontiPage(_context, _felhasznaloId));
                        }
                    }

                }
                    
                    // Új Kapcsolat beszúrása (kivéve ha "Nem kapcsolódó személy")
                    if (_kapcsolatTipus != null && _kapcsolatTipus != "Nem kapcsolódó személy" && _legutobbKijeloltSzemely != null)
                    {
                        string ujSzemelyNeme = FerfiRadioButton.IsChecked == true ? "Férfi" : "Nő";
                        //  Kapcsolat létrehozása
                        /*
                         Ell kell dönteni hogy partnert vagy gyereket adunk hozzá és úgy állítjuk be.
                        A már korábban létrehozott id-kat használjuk ez megvan.

                         */
                        //bonyolódik, csak egy irányba adhatunk tovább, pl nőnek partner és gyerek, először partner

                        // Nőhöz adjuk hozzá a férfit.
                        if (_kapcsolatTipus == "Partner" && ujSzemelyNeme == "Férfi" && _legutobbKijeloltSzemely.Nem != null && _legutobbKijeloltSzemely.Nem == "Nő")
                        {

                        Helyszinek ujHelyszin = null;
                        Szemelyek ujSzemely;
                        if (letezikSzemely)
                        {
                            ujSzemely = _context.Szemelyeks
                            .Include(s => s.Helyszin)
                            .FirstOrDefault(s =>
                                s.Keresztnev == KeresztnevTextBox.Text.Trim() &&
                                s.Vezeteknev == VezeteknevTextBox.Text.Trim() &&
                                s.Neme == neme &&
                                s.SzuletesiDatum == DateOnly.FromDateTime(szuletesiDatum) &&
                                s.FelhasznaloId == (_felhasznaloId ?? 0) &&
                                s.Helyszin != null &&
                                s.Helyszin.SzuletesiOrszag == SzuletesiOrszagTextBox.Text.Trim() &&
                                s.Helyszin.SzuletesiTelepules == SzuletesiTelepulesTextBox.Text.Trim());
                        }
                        else
                        {
                            ujHelyszin = ujHelyszinVisszaad();
                            ujSzemely = UjSzemelyVisszaad(szuletesiDatum, halalozasiDatum, ujHelyszin);
                        }

                        if (_legutobbKijeloltSzemely.Azonosito == null || _legutobbKijeloltSzemely.Azonosito <= 0)
                        {
                            MessageBox.Show("Nincs kijelölt személy, akihez kapcsolódhatna az új személy.", "Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                        if (ujSzemely.SzemelyId == null || _legutobbKijeloltSzemely.Azonosito <= 0)
                        {
                            MessageBox.Show("Nincs új személy, akihez kapcsolódhatna az kijelölt személy.", "Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                        if (string.IsNullOrWhiteSpace(kapcsolatTipusAdatbazisba) || !new[] { "Gyermek", "Partner" }.Contains(kapcsolatTipusAdatbazisba))
                        {
                            MessageBox.Show("Érvénytelen kapcsolattípus az adatbázishoz való beszúráshoz.", "Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }


                            // ellenőrzés, létre lett-e hozva már kapcsolat a nőhöz és a férfihoz, csak a nőhöz tartozik rekord de a rekord egyik mzőjében szerepelhet a férfi
                            bool vanKapcsolatNo = VaneMarParkapcsolatNo(_legutobbKijeloltSzemely.Azonosito);
                            bool vanKapcsolatFerfi = VaneMarParkapcsolatFerfi(ujSzemely.SzemelyId);

                            //ha nincs még az adatbázisban ilyen rekord akkor hozzáadjuk
                            if (vanKapcsolatNo == false && vanKapcsolatFerfi == false)
                            {
                            kapcsolatTipusAdatbazisba = "Partner";
                            var ujKapcsolat = new Kapcsolatok
                                {
                                    SzemelyId = _legutobbKijeloltSzemely.Azonosito,// kijelölt személy ID
                                    KapcsolodoSzemelyId = ujSzemely.SzemelyId, // új személy  ID
                                    KapcsolatTipusa = kapcsolatTipusAdatbazisba
                                };
                            _context.Kapcsolatoks.Add(ujKapcsolat);
                            _context.SaveChanges();

                            MessageBox.Show("A személy vagy kapcsolat sikeresen hozzáadásra került!", "Siker", MessageBoxButton.OK, MessageBoxImage.Information);

                            ((MainWindow)Application.Current.MainWindow).MainFrame.Navigate(new KozpontiPage(_context, _felhasznaloId));
                            }
                            else
                            {
                                MessageBox.Show("Hiba történt a mentés során:\n Van már párkapcsolat létrehozva a személyek között vagy valamelyik személynek.", "Hiba", MessageBoxButton.OK, MessageBoxImage.Warning);
                                if (letezikSzemely == false)
                                {
                                    TorolUjonnanLetrehozottSzemelytEsHelyszint(ujSzemely, ujHelyszin);
                                }
                            }



                            //TODO kódaba is elmentjük a férfi adatait, pl ha a nő partnernek vette fel a férfit, akkor
                            // a férfi automatikusan kapja meg partnernek a nőt kódban és ugyanez a logika a gyerekeknél is, a nőket adatbázisból keressük ki
                        }


                        //Férfihez adjuk hozzá a nőt
                        if (_kapcsolatTipus == "Partner" && ujSzemelyNeme == "Nő" && _legutobbKijeloltSzemely.Nem != null && _legutobbKijeloltSzemely.Nem == "Férfi")
                        {


                        Helyszinek ujHelyszin = null;
                        Szemelyek ujSzemely;
                        if (letezikSzemely)
                        {
                            ujSzemely = _context.Szemelyeks
                            .Include(s => s.Helyszin)
                            .FirstOrDefault(s =>
                                s.Keresztnev == KeresztnevTextBox.Text.Trim() &&
                                s.Vezeteknev == VezeteknevTextBox.Text.Trim() &&
                                s.Neme == neme &&
                                s.SzuletesiDatum == DateOnly.FromDateTime(szuletesiDatum) &&
                                s.FelhasznaloId == (_felhasznaloId ?? 0) &&
                                s.Helyszin != null &&
                                s.Helyszin.SzuletesiOrszag == SzuletesiOrszagTextBox.Text.Trim() &&
                                s.Helyszin.SzuletesiTelepules == SzuletesiTelepulesTextBox.Text.Trim());
                        }
                        else
                        {
                            ujHelyszin = ujHelyszinVisszaad();
                            ujSzemely = UjSzemelyVisszaad(szuletesiDatum, halalozasiDatum, ujHelyszin);
                        }

                        if (_legutobbKijeloltSzemely.Azonosito == null || _legutobbKijeloltSzemely.Azonosito <= 0)
                        {
                            MessageBox.Show("Nincs kijelölt személy, akihez kapcsolódhatna az új személy.", "Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                        if (ujSzemely.SzemelyId == null || _legutobbKijeloltSzemely.Azonosito <= 0)
                        {
                            MessageBox.Show("Nincs új személy, akihez kapcsolódhatna az kijelölt személy.", "Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                        if (string.IsNullOrWhiteSpace(kapcsolatTipusAdatbazisba) || !new[] { "Gyermek", "Partner" }.Contains(kapcsolatTipusAdatbazisba))
                        {
                            MessageBox.Show("Érvénytelen kapcsolattípus az adatbázishoz való beszúráshoz.", "Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }

                        //Mivel csak egy irányba tárolunk kapcsolatokat az adatbázisban ezért mindig a nők a viszonyítási pont, az ő adataikat tároljuk pl, partner, gyerek

                        // ellenőrzés, létre lett-e hozva már kapcsolat a nőhöz és a férfihoz
                        bool vanKapcsolatNonek = VaneMarParkapcsolatNo(ujSzemely.SzemelyId);
                            bool vanKapcsolatFerfi = VaneMarParkapcsolatFerfi(_legutobbKijeloltSzemely.Azonosito);
                            //ha nincs még az adatbázisban ilyen rekord akkor hozzáadjuk
                            if (vanKapcsolatNonek == false && vanKapcsolatFerfi == false)
                            {
                            kapcsolatTipusAdatbazisba = "Partner";
                            var ujKapcsolat = new Kapcsolatok
                                {
                                    // A nőhöz mentjük a férfit annak ellenére hogy a férfi a személy akihez a nőt adjuk partnerként, mert egy irányba tárolunk adatokat az egyszerűség kedvéért.
                                    SzemelyId = ujSzemely.SzemelyId, // új személy  ID
                                    KapcsolodoSzemelyId = _legutobbKijeloltSzemely.Azonosito, // kijelölt személy ID
                                    KapcsolatTipusa = kapcsolatTipusAdatbazisba
                                };
                            _context.Kapcsolatoks.Add(ujKapcsolat);
                            _context.SaveChanges();
                            MessageBox.Show("A személy vagy kapcsolat sikeresen hozzáadásra került!", "Siker", MessageBoxButton.OK, MessageBoxImage.Information);

                            ((MainWindow)Application.Current.MainWindow).MainFrame.Navigate(new KozpontiPage(_context, _felhasznaloId));
                        }
                            else
                            {
                                MessageBox.Show("Hiba történt a mentés során:\n Van már párkapcsolat létrehozva a személyek között vagy valamelyik személynek.", "Hiba", MessageBoxButton.OK, MessageBoxImage.Warning);
                                if (letezikSzemely == false)
                                {
                                    TorolUjonnanLetrehozottSzemelytEsHelyszint(ujSzemely, ujHelyszin);
                                }
                            }

                            //TODO kódba férfi adatai mentés
                        }

                        // a gyerekek hozzáadásához először meg kell nézni, hogy az illetőnek van-e már partnere a Kapcsolatok táblában
                        // ha igen, akkor adhatunk hozzá gyereket

                        

                       
                        //TESZTELVE, JÓ

                        //Anyukához adjuk hozzá a lány gyereket
                        if (_kapcsolatTipus == "Lány gyermek" && _legutobbKijeloltSzemely.Nem != null && _legutobbKijeloltSzemely.Nem == "Nő")
                        {

                        //TODO: megcsinálni hogy a gyerek születési dátuma legalább kb 11 évvel nagyobb legyen

                        bool vanePartner = VaneMarParkapcsolatNo(_legutobbKijeloltSzemely.Azonosito);
                            if (vanePartner == true)//ebben az esetben van féri partnere a nőnek tehát hozzáadhatunk lány gyereket is
                            {

                            kapcsolatTipusAdatbazisba = "Gyermek";


                            Helyszinek ujHelyszin = null;
                            Szemelyek ujSzemely;
                            if (letezikSzemely)
                            {
                                ujSzemely = _context.Szemelyeks
                                .Include(s => s.Helyszin)
                                .FirstOrDefault(s =>
                                    s.Keresztnev == KeresztnevTextBox.Text.Trim() &&
                                    s.Vezeteknev == VezeteknevTextBox.Text.Trim() &&
                                    s.Neme == neme &&
                                    s.SzuletesiDatum == DateOnly.FromDateTime(szuletesiDatum) &&
                                    s.FelhasznaloId == (_felhasznaloId ?? 0) &&
                                    s.Helyszin != null &&
                                    s.Helyszin.SzuletesiOrszag == SzuletesiOrszagTextBox.Text.Trim() &&
                                    s.Helyszin.SzuletesiTelepules == SzuletesiTelepulesTextBox.Text.Trim());
                            }
                            else
                            {
                                ujHelyszin = ujHelyszinVisszaad();
                                ujSzemely = UjSzemelyVisszaad(szuletesiDatum, halalozasiDatum, ujHelyszin);
                            }

                            if (_legutobbKijeloltSzemely.Azonosito == null || _legutobbKijeloltSzemely.Azonosito <= 0)
                            {
                                MessageBox.Show("Nincs kijelölt személy, akihez kapcsolódhatna az új személy.", "Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
                                return;
                            }
                            if (ujSzemely.SzemelyId == null || _legutobbKijeloltSzemely.Azonosito <= 0)
                            {
                                MessageBox.Show("Nincs új személy, akihez kapcsolódhatna az kijelölt személy.", "Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
                                return;
                            }
                            if (string.IsNullOrWhiteSpace(kapcsolatTipusAdatbazisba) || !new[] { "Gyermek", "Partner" }.Contains(kapcsolatTipusAdatbazisba))
                            {
                                MessageBox.Show("Érvénytelen kapcsolattípus az adatbázishoz való beszúráshoz.", "Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
                                return;
                            }

                            // ellenőrzés, létre lett-e hozva már gyermek kapcsolat a nőhöz
                            bool vanKapcsolat = VaneMarIlyenGyerek(_legutobbKijeloltSzemely.Azonosito, ujSzemely.SzemelyId);// azért kell mind két azonosító, mert így egy konkrét gyerekre fókuszálunk

                                //ha nincs még az adatbázisban ilyen rekord akkor hozzáadjuk
                                if (vanKapcsolat == false)
                                {
                                    
                                    var ujKapcsolat = new Kapcsolatok
                                    {
                                        SzemelyId = _legutobbKijeloltSzemely.Azonosito,// kijelölt személy ID
                                        KapcsolodoSzemelyId = ujSzemely.SzemelyId, // új személy ID, gyereké
                                        KapcsolatTipusa = kapcsolatTipusAdatbazisba
                                    };
                                
                                    _context.Kapcsolatoks.Add(ujKapcsolat);
                                    _context.SaveChanges();

                                MessageBox.Show("A személy vagy kapcsolat sikeresen hozzáadásra került!", "Siker", MessageBoxButton.OK, MessageBoxImage.Information);

                                ((MainWindow)Application.Current.MainWindow).MainFrame.Navigate(new KozpontiPage(_context, _felhasznaloId));
                                }
                                else
                                {
                                    MessageBox.Show("Hiba történt a mentés során:\n Szerepel már ez a gyermeke a megadott személynek a családfában.", "Hiba", MessageBoxButton.OK, MessageBoxImage.Warning);
                                //Ha mégis van már ilyen gyereke a nőnek akkor ki kell törölni a szemely és hely rekordokat mik újonnan kerültek be az adatbázisba és korábban nem léteztek
                                    if (letezikSzemely == false)
                                    {
                                        TorolUjonnanLetrehozottSzemelytEsHelyszint(ujSzemely, ujHelyszin);
                                    }
                                }

                                //TODO lánya gyermekhez felvenni anyját kódban

                            }
                            else
                            {
                                MessageBox.Show("Hiba történt a mentés során:\n Nincs megadva partner a személyhez, így nem adhatunk hozzá gyereket.", "Hiba", MessageBoxButton.OK, MessageBoxImage.Warning);
                            }


                        }

                        //Anyukához adjuk hozzá a fiú gyereket
                        if (_kapcsolatTipus == "Fiú gyermek" && _legutobbKijeloltSzemely.Nem != null && _legutobbKijeloltSzemely.Nem == "Nő")
                        {
                            bool vanePartner = VaneMarParkapcsolatNo(_legutobbKijeloltSzemely.Azonosito);

                        if (vanePartner == true)//ebben az esetben van féri partnere a nőnek tehát hozzáadhatunk fiú gyereket is
                            {

                            kapcsolatTipusAdatbazisba = "Gyermek";

                            Helyszinek ujHelyszin = null;
                            Szemelyek ujSzemely;
                            if (letezikSzemely)
                            {
                                ujSzemely = _context.Szemelyeks
                                .Include(s => s.Helyszin)
                                .FirstOrDefault(s =>
                                    s.Keresztnev == KeresztnevTextBox.Text.Trim() &&
                                    s.Vezeteknev == VezeteknevTextBox.Text.Trim() &&
                                    s.Neme == neme &&
                                    s.SzuletesiDatum == DateOnly.FromDateTime(szuletesiDatum) &&
                                    s.FelhasznaloId == (_felhasznaloId ?? 0) &&
                                    s.Helyszin != null &&
                                    s.Helyszin.SzuletesiOrszag == SzuletesiOrszagTextBox.Text.Trim() &&
                                    s.Helyszin.SzuletesiTelepules == SzuletesiTelepulesTextBox.Text.Trim());
                            }
                            else
                            {
                                ujHelyszin = ujHelyszinVisszaad();
                                ujSzemely = UjSzemelyVisszaad(szuletesiDatum, halalozasiDatum, ujHelyszin);
                            }
                            if (_legutobbKijeloltSzemely.Azonosito == null || _legutobbKijeloltSzemely.Azonosito <= 0)
                            {
                                MessageBox.Show("Nincs kijelölt személy, akihez kapcsolódhatna az új személy.", "Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
                                return;
                            }
                            if (ujSzemely.SzemelyId == null || _legutobbKijeloltSzemely.Azonosito <= 0)
                            {
                                MessageBox.Show("Nincs új személy, akihez kapcsolódhatna az kijelölt személy.", "Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
                                return;
                            }
                            if (string.IsNullOrWhiteSpace(kapcsolatTipusAdatbazisba) || !new[] { "Gyermek", "Partner" }.Contains(kapcsolatTipusAdatbazisba))
                            {
                                MessageBox.Show("Érvénytelen kapcsolattípus az adatbázishoz való beszúráshoz.", "Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
                                return;
                            }
                            // ellenőrzés, létre lett-e hozva már kapcsolat a nőhöz, azért csak a nőt ellerőrizzük mert az adatbázisban a nőkhöz adunk hozzá gyereket és partnert az egyszerűség kedvéért
                            bool vanKapcsolat = VaneMarIlyenGyerek(_legutobbKijeloltSzemely.Azonosito, ujSzemely.SzemelyId);

                                //ha nincs még az adatbázisban ilyen rekord akkor hozzáadjuk
                                if (vanKapcsolat == false)
                                {
                                
                                    var ujKapcsolat = new Kapcsolatok
                                    {
                                        SzemelyId = _legutobbKijeloltSzemely.Azonosito,// kijelölt személy ID
                                        KapcsolodoSzemelyId = ujSzemely.SzemelyId, // új személy  ID, gyereké
                                        KapcsolatTipusa = kapcsolatTipusAdatbazisba
                                    };
                                _context.Kapcsolatoks.Add(ujKapcsolat);
                                _context.SaveChanges();

                                MessageBox.Show("A személy vagy kapcsolat sikeresen hozzáadásra került!", "Siker", MessageBoxButton.OK, MessageBoxImage.Information);

                                ((MainWindow)Application.Current.MainWindow).MainFrame.Navigate(new KozpontiPage(_context, _felhasznaloId));
                                }
                                else
                                {
                                    MessageBox.Show("Hiba történt a mentés során:\n Szerepel már ez a gyermeke a megadott személynek a családfában.", "Hiba", MessageBoxButton.OK, MessageBoxImage.Warning);
                                    if (letezikSzemely == false)
                                    {
                                        TorolUjonnanLetrehozottSzemelytEsHelyszint(ujSzemely, ujHelyszin);
                                    }
                                }

                                //TODO fiú gyermekhez hozzáadni az anyját kódban
                            }
                            else
                            {
                                MessageBox.Show("Hiba történt a mentés során:\n Nincs megadva partner a személyhez, így nem adhatunk hozzá gyereket.", "Hiba", MessageBoxButton.OK, MessageBoxImage.Warning);
                            }

                        }

                        //Megnézzük a férfinek van e már kapcsolata és ha igen akkor kódban hozzá tudunk adni gyereket, ha
                        // előbb nyomok a férfira mint a női partnerére és még a nőhöz nincs hozzáadva gyerek akkor adatbázisba is mentek
                        //tehát három ellenőrzés: van-e partner, kódban van-e gyerek és adatbázisban van e gyerek

                        //Apukához hozzáadjuk a fiú gyermeket kódban és az anyukához adatbázisban ha még nincs létrehozva ott
                        if (_kapcsolatTipus == "Fiú gyermek" && _legutobbKijeloltSzemely.Nem != null && _legutobbKijeloltSzemely.Nem == "Férfi")
                        {

                        bool vanePartner = VaneMarParkapcsolatFerfi(_legutobbKijeloltSzemely.Azonosito);
                        if (vanePartner == true)//ebben az esetben ha van női partnere a férfinak, hozzáadhatunk fiú gyereket is
                            {

                            kapcsolatTipusAdatbazisba = "Gyermek";
                            Helyszinek ujHelyszin = null;
                            Szemelyek ujSzemely;
                            if (letezikSzemely)
                            {
                                ujSzemely = _context.Szemelyeks
                                .Include(s => s.Helyszin)
                                .FirstOrDefault(s =>
                                    s.Keresztnev == KeresztnevTextBox.Text.Trim() &&
                                    s.Vezeteknev == VezeteknevTextBox.Text.Trim() &&
                                    s.Neme == neme &&
                                    s.SzuletesiDatum == DateOnly.FromDateTime(szuletesiDatum) &&
                                    s.FelhasznaloId == (_felhasznaloId ?? 0) &&
                                    s.Helyszin != null &&
                                    s.Helyszin.SzuletesiOrszag == SzuletesiOrszagTextBox.Text.Trim() &&
                                    s.Helyszin.SzuletesiTelepules == SzuletesiTelepulesTextBox.Text.Trim());
                            }
                            else
                            {
                                ujHelyszin = ujHelyszinVisszaad();
                                ujSzemely = UjSzemelyVisszaad(szuletesiDatum, halalozasiDatum, ujHelyszin);
                            }
                            if (_legutobbKijeloltSzemely.Azonosito == null || _legutobbKijeloltSzemely.Azonosito <= 0)
                            {
                                MessageBox.Show("Nincs kijelölt személy, akihez kapcsolódhatna az új személy.", "Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
                                return;
                            }
                            if (ujSzemely.SzemelyId == null || _legutobbKijeloltSzemely.Azonosito <= 0)
                            {
                                MessageBox.Show("Nincs új személy, akihez kapcsolódhatna az kijelölt személy.", "Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
                                return;
                            }
                            if (string.IsNullOrWhiteSpace(kapcsolatTipusAdatbazisba) || !new[] { "Gyermek", "Partner" }.Contains(kapcsolatTipusAdatbazisba))
                            {
                                MessageBox.Show("Érvénytelen kapcsolattípus az adatbázishoz való beszúráshoz.", "Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
                                return;
                            }
                            //Mivel a férfi ID-ját a KapcsolodoSzemelyId mezőben tárolhatjuk a Kapcsolatok táblában, ezért erre keresünk rá és a SzemelyId lesz a nő ID-ja.
                            var feleseg_SzemelyId = _context.Kapcsolatoks
                                .Where(k => k.KapcsolodoSzemelyId == _legutobbKijeloltSzemely.Azonosito && k.KapcsolatTipusa == "Partner")
                                .Select(k => k.SzemelyId)
                                .FirstOrDefault();


                                // Ellenőrzés, létre lett-e hozva már gyermek kapcsolat a nőhöz aki a férfi partnere. Azért csak a nőt ellerőrizzük mert az adatbázisban a nőkhöz adunk hozzá gyereket és partnert az egyszerűség kedvéért
                                bool vanKapcsolat = VaneMarIlyenGyerek(feleseg_SzemelyId, ujSzemely.SzemelyId);

                                //ha nincs még az adatbázisban ilyen rekord akkor hozzáadjuk
                                if (vanKapcsolat == false)
                                {
                                
                                var ujKapcsolat = new Kapcsolatok
                                    {
                                        SzemelyId = feleseg_SzemelyId,// kijelölt személy nejének az ID-ja
                                        KapcsolodoSzemelyId = ujSzemely.SzemelyId, // új személy  ID, a gyereké
                                        KapcsolatTipusa = kapcsolatTipusAdatbazisba
                                    };
                                _context.Kapcsolatoks.Add(ujKapcsolat);
                                _context.SaveChanges();

                                MessageBox.Show("A személy vagy kapcsolat sikeresen hozzáadásra került!", "Siker", MessageBoxButton.OK, MessageBoxImage.Information);

                                ((MainWindow)Application.Current.MainWindow).MainFrame.Navigate(new KozpontiPage(_context, _felhasznaloId));
                                }
                                else
                                {
                                    MessageBox.Show("Hiba történt a mentés során:\n Szerepel már ez a gyermeke a megadott személynek a családfában.", "Hiba", MessageBoxButton.OK, MessageBoxImage.Warning);
                                    if (letezikSzemely == false)
                                    {
                                        TorolUjonnanLetrehozottSzemelytEsHelyszint(ujSzemely, ujHelyszin);
                                    }
                                }

                                //TODO fiú gyermekhez hozzáadni az apját kódban
                            }
                            else
                            {
                                MessageBox.Show("Hiba történt a mentés során:\n Nincs megadva partner a személyhez, így nem adhatunk hozzá gyereket.", "Hiba", MessageBoxButton.OK, MessageBoxImage.Warning);
                            }

                        }

                        //Apukához hozzáadjuk a lány gyermeket kódban és az anyukához adatbázisban ha még nincs létrehozva ott
                        if (_kapcsolatTipus == "Lány gyermek" && _legutobbKijeloltSzemely.Nem != null && _legutobbKijeloltSzemely.Nem == "Férfi")
                        {
                        kapcsolatTipusAdatbazisba = "Gyermek";
                        bool vanePartner = VaneMarParkapcsolatFerfi(_legutobbKijeloltSzemely.Azonosito);
                            if (vanePartner == true)//ebben az esetben ha van női partnere a férfinak,  hozzáadhatunk lány gyereket is
                            {


                            //ezzel vagy létrehozunk egy új személy, vagy ha már
                            //korábban létezett akkor nem kell létrehozni újra csak használni
                            Helyszinek ujHelyszin = null;
                            Szemelyek ujSzemely;
                            if (letezikSzemely)
                            {
                                ujSzemely = _context.Szemelyeks
                                .Include(s => s.Helyszin)
                                .FirstOrDefault(s =>
                                    s.Keresztnev == KeresztnevTextBox.Text.Trim() &&
                                    s.Vezeteknev == VezeteknevTextBox.Text.Trim() &&
                                    s.Neme == neme &&
                                    s.SzuletesiDatum == DateOnly.FromDateTime(szuletesiDatum) &&
                                    s.FelhasznaloId == (_felhasznaloId ?? 0) &&
                                    s.Helyszin != null &&
                                    s.Helyszin.SzuletesiOrszag == SzuletesiOrszagTextBox.Text.Trim() &&
                                    s.Helyszin.SzuletesiTelepules == SzuletesiTelepulesTextBox.Text.Trim());
                            }
                            else
                            {
                                ujHelyszin = ujHelyszinVisszaad();
                                ujSzemely = UjSzemelyVisszaad(szuletesiDatum, halalozasiDatum, ujHelyszin);
                            }

                            if (_legutobbKijeloltSzemely.Azonosito == null || _legutobbKijeloltSzemely.Azonosito <= 0)
                            {
                                MessageBox.Show("Nincs kijelölt személy, akihez kapcsolódhatna az új személy.", "Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
                                return;
                            }
                            if (ujSzemely.SzemelyId == null || _legutobbKijeloltSzemely.Azonosito <= 0)
                            {
                                MessageBox.Show("Nincs új személy, akihez kapcsolódhatna az kijelölt személy.", "Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
                                return;
                            }
                            if (string.IsNullOrWhiteSpace(kapcsolatTipusAdatbazisba) || !new[] { "Gyermek", "Partner" }.Contains(kapcsolatTipusAdatbazisba))
                            {
                                MessageBox.Show("Érvénytelen kapcsolattípus az adatbázishoz való beszúráshoz.", "Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
                                return;
                            }
                            //Mivel a férfi ID-ját a KapcsolodoSzemelyId mezőben tárolhatjuk a Kapcsolatok táblában, ezért erre keresünk rá és a SzemelyId lesz a nő ID-ja.
                            var feleseg_SzemelyId = _context.Kapcsolatoks
                                .Where(k => k.KapcsolodoSzemelyId == _legutobbKijeloltSzemely.Azonosito && k.KapcsolatTipusa == "Partner")
                                .Select(k => k.SzemelyId)
                                .FirstOrDefault();


                                // Ellenőrzés, létre lett-e hozva már gyermek kapcsolat a nőhöz aki a férfi partnere. Azért csak a nőt ellerőrizzük mert az adatbázisban a nőkhöz adunk hozzá gyereket és partnert az egyszerűség kedvéért.
                                bool vanKapcsolat = VaneMarIlyenGyerek(feleseg_SzemelyId, ujSzemely.SzemelyId);

                                //ha nincs még az adatbázisban ilyen rekord akkor hozzáadjuk
                                if (vanKapcsolat == false)
                                {
                                
                                var ujKapcsolat = new Kapcsolatok
                                    {
                                        SzemelyId = feleseg_SzemelyId,// kijelölt személy nejének az ID-ja
                                        KapcsolodoSzemelyId = ujSzemely.SzemelyId, // új személy  ID, a gyereké
                                        KapcsolatTipusa = kapcsolatTipusAdatbazisba
                                    };
                                _context.Kapcsolatoks.Add(ujKapcsolat);
                                _context.SaveChanges();
                                MessageBox.Show("A személy vagy kapcsolat sikeresen hozzáadásra került!", "Siker", MessageBoxButton.OK, MessageBoxImage.Information);

                                ((MainWindow)Application.Current.MainWindow).MainFrame.Navigate(new KozpontiPage(_context, _felhasznaloId));

                                }
                                else
                                {
                                    MessageBox.Show("Hiba történt a mentés során:\n Szerepel már ez a gyermeke a megadott személynek a családfában.", "Hiba", MessageBoxButton.OK, MessageBoxImage.Warning);
                                    if (letezikSzemely == false)
                                    {
                                        TorolUjonnanLetrehozottSzemelytEsHelyszint(ujSzemely, ujHelyszin);
                                    }
                            }

                                //TODO lány gyermekhez hozzáadni az apját kódban
                            }
                            else
                            {
                                MessageBox.Show("Hiba történt a mentés során:\n Nincs megadva partner a személyhez, így nem adhatunk hozzá gyereket.", "Hiba", MessageBoxButton.OK, MessageBoxImage.Warning);
                            }

                        }

                    }

                 
            }
            catch (Exception ex)
            {
                string hibauzenet = "Hiba történt a mentés során:\n" + ex.Message;

                if (ex.InnerException != null)
                {
                    hibauzenet += "\nRészletek: " + ex.InnerException.Message;
                }

                MessageBox.Show(hibauzenet, "Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
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
    }
}

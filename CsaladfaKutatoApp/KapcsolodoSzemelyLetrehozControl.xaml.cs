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

namespace CsaladfaKutatoApp
{
    /// <summary>
    /// Interaction logic for KapcsolodoSzemelyLetrehozControl.xaml
    /// </summary>
    public partial class KapcsolodoSzemelyLetrehozControl : UserControl
    {
        private KozpontiPage KpOldal;
        private readonly CsaladfaAdatbazisContext _context;
        int? _felhasznaloId = ((MainWindow)Application.Current.MainWindow).BejelentkezettFelhasznaloId;


        public KapcsolodoSzemelyLetrehozControl(KozpontiPage kpoldal, CsaladfaAdatbazisContext context)
        {
            InitializeComponent();
            KpOldal = kpoldal;
            _context = context;
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

            PopupUnezet.Text = "A Mentés gombra kattintva hozzáadhatjuk az új személyt a családfához. " +
                "A dátum típusú mezőkhöz YYYY-MM-DD formátumú adatot szíveskedjenek megadni! A helyszínes mezőköz pedig " +
                "Országnév, Településnév forma a megfelelő! Kötelező kitöltendő mezők a Keresztnév, Vezetéknév, Neme, Születési dátum, Születési hely.";

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
            KpOldal.TartalomValtas(new KezdoTartalomControl(KpOldal, _context));
        }

        private bool VanKozepenVesszo(string szoveg)
        {
            if (string.IsNullOrWhiteSpace(szoveg))
                return false;

            int vesszoIndex = szoveg.IndexOf(",");
            return vesszoIndex > 0 && vesszoIndex < szoveg.Length - 1;
        }

        private void Hozzaadas_Click(object sender, RoutedEventArgs e)
        {
            // Kötelező mezők ellenőrzése
            if (string.IsNullOrWhiteSpace(KeresztnevTextBox.Text) ||
                string.IsNullOrWhiteSpace(VezeteknevTextBox.Text) ||
                string.IsNullOrWhiteSpace(SzuletesiDatumTextBox.Text) ||
                string.IsNullOrWhiteSpace(SzuletesiHelyTextBox.Text) ||
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

            // Helyszín mezők ellenőrzése: ha nem üresek, kötelező bennük a vessző
            if (!string.IsNullOrWhiteSpace(SzuletesiHelyTextBox.Text) &&
                !VanKozepenVesszo(SzuletesiHelyTextBox.Text))
            {
                MessageBox.Show("A Születési hely mezőn rossz formátumot adott meg! A megfelelő formátum Pl: Országnév, Településnév", "Hibás helyszín", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!string.IsNullOrWhiteSpace(HalalhelyTextBox.Text) &&
                !VanKozepenVesszo(HalalhelyTextBox.Text))
            {
                MessageBox.Show("A Halál helye mezőn rossz formátumot adott meg! A megfelelő formátum Pl: Országnév, Településnév", "Hibás helyszín", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (!string.IsNullOrWhiteSpace(OrokNyugalomHelyeTextBox.Text) &&
                !VanKozepenVesszo(OrokNyugalomHelyeTextBox.Text))
            {
                MessageBox.Show("Az örök nyugalom helye mezőn rossz formátumot adott meg! A megfelelő formátum Pl: Országnév, Településnév", "Hibás helyszín", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            //kapcsolódás az adatbázishoz
            var kapcsolat = _context.Database.GetDbConnection();

            try
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
                        AddParam("@Neme", FerfiRadioButton.IsChecked == true ? "Férfi" : "Nő");
                        AddParam("@EloSzemely", EloSzemelyCheckBox.IsChecked == true ? 1 : 0);
                        AddParam("@FelhasznaloId", ((MainWindow)System.Windows.Application.Current.MainWindow).BejelentkezettFelhasznaloId); //  dinamikusan jön)

                        AddParam("@SzuletesiHely", SzuletesiHelyTextBox.Text.Trim());
                        AddParam("@HalalozasiHely", string.IsNullOrWhiteSpace(HalalhelyTextBox.Text) ? null : HalalhelyTextBox.Text.Trim());
                        AddParam("@OrokNyugalomHelye", string.IsNullOrWhiteSpace(OrokNyugalomHelyeTextBox.Text) ? null : OrokNyugalomHelyeTextBox.Text.Trim());

                        AddParam("@Foglalkozas", string.IsNullOrWhiteSpace(FoglalkozasaTextBox.Text) ? null : FoglalkozasaTextBox.Text.Trim());
                        AddParam("@Tanulmany", string.IsNullOrWhiteSpace(TanulmanyaiTextBox.Text) ? null : TanulmanyaiTextBox.Text.Trim());
                        AddParam("@Vallas", string.IsNullOrWhiteSpace(VallasaTextBox.Text) ? null : VallasaTextBox.Text.Trim());

                        // Kapcsolat megnyitása, ha még nincs
                        if (kapcsolat.State != System.Data.ConnectionState.Open)
                            kapcsolat.Open();
                        
                            


                        parancs.ExecuteNonQuery();

                        kapcsolat.Close();
                }

                    
                

                MessageBox.Show("A személy sikeresen hozzáadásra került!", "Siker", MessageBoxButton.OK, MessageBoxImage.Information);
                ((MainWindow)Application.Current.MainWindow).MainFrame.Navigate(new KozpontiPage(_context, _felhasznaloId));
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hiba történt a mentés során:\n" + ex.Message, "Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        private void EloSzemelyCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            HalalDatumTextBox.IsEnabled = false;
            HalalhelyTextBox.IsEnabled = false;
            OrokNyugalomHelyeTextBox.IsEnabled = false;
        }

        private void EloSzemelyCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            HalalDatumTextBox.IsEnabled = true;
            HalalhelyTextBox.IsEnabled = true;
            OrokNyugalomHelyeTextBox.IsEnabled = true;
        }
    }
}

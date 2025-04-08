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
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System.Text.RegularExpressions;
using static System.Net.Mime.MediaTypeNames;
using CsaladfaKutatoApp.Models;
using CsaladfaKutatoApp.Segedeszkozok;
using Microsoft.EntityFrameworkCore;

namespace CsaladfaKutatoApp
{
    /// <summary>
    /// Interaction logic for BejelentkezesPage.xaml
    /// </summary>
    public partial class BejelentkezesPage : Page
    {
        private readonly CsaladfaAdatbazisContext _context;

        public BejelentkezesPage(CsaladfaAdatbazisContext context)
        {
            InitializeComponent();
            _context = context;
        }
        // Felhasználónév ellenőrzése
        private static bool FelhasznalonevVizsgalat(string felhnev) 
        {
            string minta = @"^[a-zA-Z0-9_.]{3,20}$";
            return Regex.IsMatch(felhnev, minta);
        }

        // Email ellenőrzése
        private static bool EmailVizsgalat(string email)
        {
            string minta = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            return Regex.IsMatch(email, minta);
        }
      
        //AzonositoTextBox adatainak ellenőrzése és hibaüzenet
        private bool AzonositoAdatokEllenorzese(string azonosito, bool emailMod, out string EmailHibaUzenet)
        {
            EmailHibaUzenet = "";
            if (string.IsNullOrWhiteSpace(azonosito))
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(azonosito) && emailMod==true && !EmailVizsgalat(azonosito))
            {
                EmailHibaUzenet = "Nem megfelelő email formátum!";
                return false;
            }

            return true;
        }

        //PasswordBox adatainak ellenőrzése és hibaüzenet
        private bool JelszoAdatokEllenorzese(string jelszo)
        {

            if (string.IsNullOrWhiteSpace(jelszo))
            {
                return false;
            }

            return true;
        }


        private void FrissitBejelentkezesGombAllapotLogin()
        {
            string azonosito = AzonositoTextBox.Text;
            string jelszo = PasswordBox.Visibility == Visibility.Visible
                            ? PasswordBox.Password
                            : PasswordTextBox.Text;

            bool emailMod = BejelentkezesiMod.IsOn;
            bool ervenyesAzonosito = AzonositoAdatokEllenorzese(azonosito, emailMod, out string EmailHibaUzenet);
            bool ervenyesJelszo = JelszoAdatokEllenorzese(jelszo);

            HibaUzenetAzonosito.Text = ervenyesAzonosito ? "" : EmailHibaUzenet;

            //Bejelentkezés gob aktiválása
            if (ervenyesAzonosito == true && ervenyesJelszo == true)
            {
                BejelentkezButton.IsEnabled = true;
            }
            else
            {
                BejelentkezButton.IsEnabled = false;
            }
        }

        // ToggleSwitch esemény, ami frissíti a Watermark-ot
        private void BejelentkezesiMod_Valtva(object sender, RoutedEventArgs e)
        {
            bool emailMod = BejelentkezesiMod.IsOn;

            // Dinamikus Watermark frissítés
            AzonositoTextBox.SetValue(MahApps.Metro.Controls.TextBoxHelper.WatermarkProperty,
            emailMod ? "Email cím" : "Felhasználónév");

            FrissitBejelentkezesGombAllapotLogin();
        }
        private void Hyperlink_Regisztracio(object sender, RequestNavigateEventArgs e)
        {
            // A "MainWindow" Frame-jén keresztül navigálunk
            ((MainWindow)System.Windows.Application.Current.MainWindow).MainFrame.Navigate(new RegisztracioPage(_context));
            e.Handled = true;
        } 

        private void JelszoLathatosagValtas(object sender, RoutedEventArgs e)
        {
            if (PasswordBox.Visibility == Visibility.Visible)
            {
                PasswordTextBox.Text = PasswordBox.Password;
                PasswordBox.Visibility = Visibility.Collapsed;
                PasswordTextBox.Visibility = Visibility.Visible;              
                PasswordTextBox.Focus();
                PasswordTextBox.SelectionStart = PasswordTextBox.Text.Length;
            }
            else
            {
                PasswordBox.Password = PasswordTextBox.Text;
                PasswordBox.Visibility = Visibility.Visible;
                PasswordTextBox.Visibility = Visibility.Collapsed;

                PasswordBox.Focus();
                // Ez a kódrészlet azt éri el, hogy a kurzor mindig a jelszó végére kerüljön, reflection segítségével.
                typeof(PasswordBox).GetMethod("Select", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                    .Invoke(PasswordBox, new object[] { PasswordBox.Password.Length, 0 });
            }
        }
        private void Bejelentkezes_Click(object sender, RoutedEventArgs e)
        {
            string azonosito = AzonositoTextBox.Text;
            string jelszo = PasswordBox.Visibility == Visibility.Visible ? PasswordBox.Password : PasswordTextBox.Text;
            bool emailMod = BejelentkezesiMod.IsOn;
            string aktualisMod = emailMod ? "email" : "felhasznalonev";
            HibaUzenetAzonosito.Text = "";
            HibaUzenetJelszo.Text = "";




            try
            {
                // Lekérjük a felhasználót az adatbázisból, megfelelő BejelentkezesiMod szerint
                var felhasznalo = emailMod
                    ? _context.Felhasznaloks.FirstOrDefault(f => f.Email == azonosito)
                    : _context.Felhasznaloks.FirstOrDefault(f => f.Felhasznalonev == azonosito);
               
                if (felhasznalo == null)
                {
                    HibaUzenetAzonosito.Text = "Nincs ilyen felhasználó.";
                    return;
                }
               

                // Jelszó hash újragenerálása
                string ujHash = JelszoHasher.HashJelszoSalttal(jelszo, felhasznalo.JelszoSalt);

                // Összehasonlítás
                if (ujHash == felhasznalo.JelszoHash)
                {
                    MessageBox.Show("Sikeres bejelentkezés!", "Üdvözlünk", MessageBoxButton.OK, MessageBoxImage.Information);
                    // Csak itt, sikeres bejelentkezés után fut le a tárolt eljárás!
                    var connection = _context.Database.GetDbConnection();
                    connection.Open();
                    ((MainWindow)System.Windows.Application.Current.MainWindow).BejelentkezettFelhasznaloId = felhasznalo.FelhasznaloId;
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "sp_ModositBejelentkezesiMod";
                        command.CommandType = System.Data.CommandType.StoredProcedure;

                        var p1 = command.CreateParameter();
                        p1.ParameterName = "@AzonositoVagyEmail";
                        p1.Value = azonosito;
                        command.Parameters.Add(p1);

                        var p2 = command.CreateParameter();
                        p2.ParameterName = "@UjMod";
                        p2.Value = aktualisMod;
                        command.Parameters.Add(p2);

                        command.ExecuteNonQuery();
                    }

                    connection.Close();

                    // Navigálás ElsoCsaladtagHozzaadPage oldalra.
                    ((MainWindow)System.Windows.Application.Current.MainWindow).MainFrame.Navigate(new ElsoCsaladtagHozzaadPage(_context));
                }
                else
                {
                    HibaUzenetJelszo.Text = "Hibás jelszó.";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Hiba történt a bejelentkezés során: {ex.Message}", "Hiba", MessageBoxButton.OK, MessageBoxImage.Error);

            }
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Keyboard.ClearFocus();  // Törli a fókuszt minden vezérlőről
            FocusManager.SetFocusedElement(this, (Grid)sender); // Átállítja a fókuszt a Grid-re, annak érdekében, hogy a kurzor, ne ragadjon bent a mezőkben.
        }

        private void BemenetiSzovegValtozas(object sender, TextChangedEventArgs e)
        {
            FrissitBejelentkezesGombAllapotLogin();
        }

        private void BemenetiJelszoValtozas(object sender, RoutedEventArgs e)
        {
            FrissitBejelentkezesGombAllapotLogin();       
        }     
    }
}

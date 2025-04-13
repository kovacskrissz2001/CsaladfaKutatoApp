using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
using BCrypt.Net;
using System.Security.Cryptography;
using System.Text;
using CsaladfaKutatoApp.Models;
using Microsoft.EntityFrameworkCore;
using CsaladfaKutatoApp.Segedeszkozok;

namespace CsaladfaKutatoApp
{
    /// <summary>
    /// Interaction logic for RegisztracioPage.xaml
    /// </summary>
    public partial class RegisztracioPage : Page
    {
        private readonly CsaladfaAdatbazisContext _context;
        public RegisztracioPage(CsaladfaAdatbazisContext context)
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
        private static bool AzonositoAdatokEllenorzese(string azonosito,  out string AzonositoHibaUzenet)
        {
            AzonositoHibaUzenet = "";

            if (string.IsNullOrWhiteSpace(azonosito))
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(azonosito) && !FelhasznalonevVizsgalat(azonosito))
            {
                AzonositoHibaUzenet = "A felhasználónév nem megfelelő!(3-20 karakter, betű, szám, _, .)";
                return false;
            }
            return true;
        }

        private static bool EmailAdatokEllenorzese(string email, out string EmailHibaUzenet)
        {
            EmailHibaUzenet = "";

            if (string.IsNullOrWhiteSpace(email))
            {
                return false;
            }


            if (!string.IsNullOrWhiteSpace(email) && !EmailVizsgalat(email))
            {
                EmailHibaUzenet = "Nem megfelelő email formátum!";
                return false;
            }


            return true;
        }

        //PasswordBox adatainak ellenőrzése és hibaüzenet
        private static bool JelszoAdatokEllenorzese(string jelszo, out string JelszoHibaUzenet)
        {
            JelszoHibaUzenet = "";

            if (string.IsNullOrWhiteSpace(jelszo))
            {
                return false;
            }
            if (!string.IsNullOrWhiteSpace(jelszo) && jelszo.Length < 8)
            {
                JelszoHibaUzenet = "A jelszónak legalább 8 karakter hosszúnak kell lennie.";
                return false;
            }

            if (!string.IsNullOrWhiteSpace(jelszo) && !jelszo.Any(char.IsUpper))
            {
                JelszoHibaUzenet = "A jelszónak tartalmaznia kell legalább egy nagybetűt.";
                return false;
            }

            if (!string.IsNullOrWhiteSpace(jelszo) && !jelszo.Any(char.IsLower))
            {
                JelszoHibaUzenet = "A jelszónak tartalmaznia kell legalább egy kisbetűt.";
                return false;
            }

            if (!string.IsNullOrWhiteSpace(jelszo) && !jelszo.Any(char.IsDigit))
            {
                JelszoHibaUzenet = "A jelszónak tartalmaznia kell legalább egy számot.";
                return false;
            }

            if (!string.IsNullOrWhiteSpace(jelszo) && !jelszo.Any(ch => "!@#$%^&*()_-+=[]{}|;:,.<>?".Contains(ch)))
            {
                JelszoHibaUzenet = "A jelszónak tartalmaznia kell legalább egy speciális karaktert.";
                return false;
            }

            if (!string.IsNullOrWhiteSpace(jelszo) && jelszo.Any(char.IsWhiteSpace))
            {
                JelszoHibaUzenet = "A jelszó nem tartalmazhat szóközt.";
                return false;
            }

            return true;
        }

        private void FrissitBejelentkezesGombAllapot()
        {
            string azonosito = AzonositoTextBox.Text;
            string email = EmailTextBox.Text;
            string jelszo1 = PasswordBox1.Visibility == Visibility.Visible
                            ? PasswordBox1.Password
                            : PasswordTextBox1.Text;
            string jelszo2 = PasswordBox2.Visibility == Visibility.Visible
                           ? PasswordBox2.Password
                           : PasswordTextBox2.Text;

            bool ervenyesAzonosito = AzonositoAdatokEllenorzese(azonosito, out string AzonositoHibaUzenet);
            bool ervenyesEmail = EmailAdatokEllenorzese(email, out string EmailHibaUzenet);
            bool ervenyesJelszo1 = JelszoAdatokEllenorzese(jelszo1, out string JelszoHibaUzenet);
            bool ervenyesJelszo2 = jelszo1 == jelszo2 ? true : false;
            //hibaüzenetet valós időben:
            HibaUzenetAzonosito.Text = ervenyesAzonosito ? "" : AzonositoHibaUzenet;
            HibaUzenetEmail.Text = ervenyesEmail ? "" : EmailHibaUzenet;
            HibaUzenetPasswordBox1.Text = ervenyesJelszo1 ? "" : JelszoHibaUzenet;
            if (!string.IsNullOrWhiteSpace(jelszo2))
                HibaUzenetPasswordBox2.Text = ervenyesJelszo2 ? "" : "A két jelszó nem egyezik meg!";

            //Bejelentkezés gob aktiválása
            if (ervenyesAzonosito == true && ervenyesEmail==true && ervenyesJelszo1 == true && ervenyesJelszo2 == true)
            {
                RegButton.IsEnabled = true;
            }
            else
            {
                RegButton.IsEnabled = false;
            }
        }

        private void Hyperlink_Bejelentkezes(object sender, RequestNavigateEventArgs e)
        {
            // A "MainWindow" Frame-jén keresztül navigálunk
            ((MainWindow)System.Windows.Application.Current.MainWindow).MainFrame.Navigate(new BejelentkezesPage(_context));
            e.Handled = true;
        }

        private void JelszoLathatosagValtas1(object sender, RoutedEventArgs e)
        {
            if (PasswordBox1.Visibility == Visibility.Visible)
            {
                PasswordTextBox1.Text = PasswordBox1.Password;
                PasswordBox1.Visibility = Visibility.Collapsed;
                PasswordTextBox1.Visibility = Visibility.Visible;
                PasswordTextBox1.Focus();
                PasswordTextBox1.SelectionStart = PasswordTextBox1.Text.Length;
            }
            else
            {
                PasswordBox1.Password = PasswordTextBox1.Text;
                PasswordBox1.Visibility = Visibility.Visible;
                PasswordTextBox1.Visibility = Visibility.Collapsed;

                PasswordBox1.Focus();
                // Ez a kódrészlet azt éri el, hogy a kurzor mindig a jelszó végére kerüljön, reflection segítségével.
                typeof(PasswordBox).GetMethod("Select", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                    .Invoke(PasswordBox1, new object[] { PasswordBox1.Password.Length, 0 });
            }
        }

        private void JelszoLathatosagValtas2(object sender, RoutedEventArgs e)
        {
            if (PasswordBox2.Visibility == Visibility.Visible)
            {
                PasswordTextBox2.Text = PasswordBox2.Password;
                PasswordBox2.Visibility = Visibility.Collapsed;
                PasswordTextBox2.Visibility = Visibility.Visible;
                PasswordTextBox2.Focus();
                PasswordTextBox2.SelectionStart = PasswordTextBox2.Text.Length;
            }
            else
            {
                PasswordBox2.Password = PasswordTextBox2.Text;
                PasswordBox2.Visibility = Visibility.Visible;
                PasswordTextBox2.Visibility = Visibility.Collapsed;

                PasswordBox2.Focus();
                // Ez a kódrészlet azt éri el, hogy a kurzor mindig a jelszó végére kerüljön, reflection segítségével.
                typeof(PasswordBox).GetMethod("Select", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                    .Invoke(PasswordBox2, new object[] { PasswordBox2.Password.Length, 0 });
            }
        }
        private void Azonosito_SzovegValtozas(object sender, TextChangedEventArgs e)
        {
            FrissitBejelentkezesGombAllapot();
        }

        private void Email_SzovegValtozas(object sender, TextChangedEventArgs e)
        {
            FrissitBejelentkezesGombAllapot();
        }

        private void PasswordBox1_Valtozas(object sender, RoutedEventArgs e)
        {
            FrissitBejelentkezesGombAllapot();
        }

        private void PasswordBox2_Valtozas(object sender, RoutedEventArgs e)
        {
            FrissitBejelentkezesGombAllapot();
        }

        private void PasswordTextBox1_SzovegValtozas(object sender, TextChangedEventArgs e)
        {
            FrissitBejelentkezesGombAllapot();
        }

        private void PasswordTextBox2_SzovegValtozas(object sender, TextChangedEventArgs e)
        {
            FrissitBejelentkezesGombAllapot();
        }

        private void Regisztracio_Click(object sender, RoutedEventArgs e)
        {
            string felhasznalonev = AzonositoTextBox.Text;
            string email = EmailTextBox.Text;
            string jelszo = PasswordBox1.Password;
            string bejelentkezesiMod = "felhasznalonev";

            // 🔐 Só generálás
            string salt = JelszoHasher.SaltGeneralas();


            // 🔐 Hash készítés
            string hash = JelszoHasher.HashJelszoSalttal(jelszo, salt);

            //kapcsolódás az adatbázishoz
            var connection = _context.Database.GetDbConnection();

            //Tárolt eljárás meghívása a meglévő context-tel
            try
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "sp_RegisztraljFelhasznalot";
                    command.CommandType = System.Data.CommandType.StoredProcedure;

                    // Paraméterek átadása
                    var pAzonosito = command.CreateParameter();
                    pAzonosito.ParameterName = "@Felhasznalonev";
                    pAzonosito.Value = felhasznalonev;
                    command.Parameters.Add(pAzonosito);

                    var pEmail = command.CreateParameter();
                    pEmail.ParameterName = "@Email";
                    pEmail.Value = email;
                    command.Parameters.Add(pEmail);

                    var pHash = command.CreateParameter();
                    pHash.ParameterName = "@JelszoHash";
                    pHash.Value = hash;
                    command.Parameters.Add(pHash);

                    var pSalt = command.CreateParameter();
                    pSalt.ParameterName = "@Salt";
                    pSalt.Value = salt;
                    command.Parameters.Add(pSalt);

                    var pBejelentkezesMod = command.CreateParameter();
                    pBejelentkezesMod.ParameterName = "@BejelentkezesiMod";
                    pBejelentkezesMod.Value = bejelentkezesiMod;
                    command.Parameters.Add(pBejelentkezesMod);

                    command.ExecuteNonQuery();

                    MessageBox.Show("Sikeres regisztráció!", "Kész", MessageBoxButton.OK, MessageBoxImage.Information);
                    // Navigálás ElsoCsaladtagHozzaadPage oldalra.
                    ((MainWindow)System.Windows.Application.Current.MainWindow).MainFrame.Navigate(new ElsoCsaladtagHozzaadPage(_context));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Hiba történt a regisztrációnál: {ex.Message}", "Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                connection.Close();
            }
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Keyboard.ClearFocus();  // Törli a fókuszt minden vezérlőről
            FocusManager.SetFocusedElement(this, (Grid)sender); // Átállítja a fókuszt a Grid-re, annak érdekében, hogy a kurzor, ne ragadjon bent a mezőkben.
        }

        
    }
}

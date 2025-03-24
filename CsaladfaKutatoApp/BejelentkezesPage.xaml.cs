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


namespace CsaladfaKutatoApp
{
    /// <summary>
    /// Interaction logic for BejelentkezesPage.xaml
    /// </summary>
    public partial class BejelentkezesPage : Page
    {
        public BejelentkezesPage()
        {
            InitializeComponent();
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
        private bool AzonositoAdatokEllenorzese(string azonosito, bool emailMod, out string AzonositoHibaUzenet)
        {
            AzonositoHibaUzenet = "";

            if (string.IsNullOrWhiteSpace(azonosito))
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(azonosito) && emailMod && !EmailVizsgalat(azonosito))
            {
                AzonositoHibaUzenet = "Nem megfelelő email formátum!";
                return false;
            }

            if (!string.IsNullOrWhiteSpace(azonosito) && !emailMod && !FelhasznalonevVizsgalat(azonosito))
            {
                AzonositoHibaUzenet = "A felhasználónév nem megfelelő!(3-20 karakter, betű, szám, _, .)";
                return false;
            }
            return true;
        }

        //PasswordBox adatainak ellenőrzése és hibaüzenet
        private bool JelszoAdatokEllenorzese(string jelszo, out string JelszoHibaUzenet)
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
            string jelszo = PasswordBox.Visibility == Visibility.Visible
                            ? PasswordBox.Password
                            : PasswordTextBox.Text;

            bool emailMod = BejelentkezesiMod.IsOn;
            bool ervenyesAzonosito = AzonositoAdatokEllenorzese(azonosito, emailMod, out string AzonositoHibaUzenet);
            bool ervenyesJelszo = JelszoAdatokEllenorzese(jelszo, out string JelszoHibaUzenet);

            //hibaüzenetet valós időben:
            HibaUzenetAzonosito.Text = ervenyesAzonosito ? "" : AzonositoHibaUzenet;
            HibaUzenetJelszo.Text = ervenyesJelszo ? "" : JelszoHibaUzenet;

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

            FrissitBejelentkezesGombAllapot();
        }
        private void Hyperlink_Regisztracio(object sender, RequestNavigateEventArgs e)
        {
            // A "MainWindow" Frame-jén keresztül navigálunk
            ((MainWindow)System.Windows.Application.Current.MainWindow).MainFrame.Navigate(new RegisztracioPage());
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
            HibaUzenetBejelentkezes.Text = "";

            // További bejelentkezési logika
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Keyboard.ClearFocus();  // Törli a fókuszt minden vezérlőről
            FocusManager.SetFocusedElement(this, (Grid)sender); // Átállítja a fókuszt a Grid-re, annak érdekében, hogy a kurzor, ne ragadjon bent a mezőkben.
        }

        private void BemenetiSzovegValtozas(object sender, TextChangedEventArgs e)
        {
            FrissitBejelentkezesGombAllapot();
        }

        private void BemenetiJelszoValtozas(object sender, RoutedEventArgs e)
        {
            FrissitBejelentkezesGombAllapot();       
        }     
    }
}

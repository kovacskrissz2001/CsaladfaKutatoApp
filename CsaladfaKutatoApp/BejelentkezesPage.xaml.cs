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
        private bool FelhasznalonevVizsgalat(string felhnev)
        {
            string minta = @"^[a-zA-Z0-9_.]{3,20}$";
            return Regex.IsMatch(felhnev, minta);
        }

        // Email ellenőrzése
        private bool EmailVizsgalat(string email)
        {
            string minta = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            return Regex.IsMatch(email, minta);
        }
        private bool BejelenkezesiAdatokEllenorzese(string azonosito, string jelszo, bool emailMod, out string hibaUzenet)
        {
            hibaUzenet = "";

            if (string.IsNullOrWhiteSpace(azonosito) || string.IsNullOrWhiteSpace(jelszo))
            {
                hibaUzenet = "Minden mezőt ki kell tölteni!";
                return false;
            }

            if (emailMod && !EmailVizsgalat(azonosito))
            {
                hibaUzenet = "Nem megfelelő email formátum!";
                return false;
            }

            if (!emailMod && !FelhasznalonevVizsgalat(azonosito))
            {
                hibaUzenet = "A felhasználónév nem megfelelő (3-20 karakter, betű, szám, _, . lehet)!";
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

            bool ervenyes = BejelenkezesiAdatokEllenorzese(azonosito, jelszo, emailMod, out string hiba);
            BejelentkezButton.IsEnabled = ervenyes;
            //hibaüzenetet valós időben:
            HibaUzenet.Text = ervenyes ? "" : hiba;
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
            ((MainWindow)Application.Current.MainWindow).MainFrame.Navigate(new RegisztracioPage());
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
               // Ez a kódrészlet azt éri el, hogy a kurzor mindig a jelszó végére kerüljönreflection segítségével.
                typeof(PasswordBox).GetMethod("Select", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                    .Invoke(PasswordBox, new object[] { PasswordBox.Password.Length, 0 });
            }
        }

       
        private void Bejelentkezes_Click(object sender, RoutedEventArgs e)
        {
            string azonosito = AzonositoTextBox.Text;
            string jelszo = PasswordBox.Visibility == Visibility.Visible ? PasswordBox.Password : PasswordTextBox.Text;
            bool emailMod = BejelentkezesiMod.IsOn;

            if (!BejelenkezesiAdatokEllenorzese(azonosito, jelszo, emailMod, out string hiba))
            {
                HibaUzenet.Text = hiba;
                return;
            }

            HibaUzenet.Text = "";


            // További bejelentkezési logika
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Keyboard.ClearFocus();  // Törli a fókuszt minden vezérlőről
            FocusManager.SetFocusedElement(this, (Grid)sender); // Átállítja a fókuszt a Grid-re
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

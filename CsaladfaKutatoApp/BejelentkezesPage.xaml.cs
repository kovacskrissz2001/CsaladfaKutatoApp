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

        // ToggleSwitch esemény, ami frissíti a Watermark-ot
        private void BejelentkezesiMod_Valtva(object sender, RoutedEventArgs e)
        {
            bool emailMod = BejelentkezesiMod.IsOn;

            // Dinamikus Watermark frissítés
            AzonositoTextBox.SetValue(MahApps.Metro.Controls.TextBoxHelper.WatermarkProperty,
            emailMod ? "Email cím" : "Felhasználónév");
        }
        private void Hyperlink_Regisztracio(object sender, RequestNavigateEventArgs e)
        {
            // A "MainWindow" Frame-jén keresztül navigálunk
            ((MainWindow)Application.Current.MainWindow).MainFrame.Navigate(new RegisztracioPage());
            e.Handled = true;
        }

        private void PasswordBoxes_Valtozas(object sender, RoutedEventArgs e)
        {

        }

        private void PasswordBox_FokuszVesztes(object sender, RoutedEventArgs e)
        {

        }

        private void PasswordTextBox_SzovegValtozas(object sender, TextChangedEventArgs e)
        {

        }

        private void JelszoLathatosagValtas(object sender, RoutedEventArgs e)
        {

        }

        private void Bejelentkezes_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}

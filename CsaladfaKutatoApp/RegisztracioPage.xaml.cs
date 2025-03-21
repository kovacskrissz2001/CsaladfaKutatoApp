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

namespace CsaladfaKutatoApp
{
    /// <summary>
    /// Interaction logic for RegisztracioPage.xaml
    /// </summary>
    public partial class RegisztracioPage : Page
    {
        public RegisztracioPage()
        {
            InitializeComponent();
        }
        private void Hyperlink_Bejelentkezes(object sender, RequestNavigateEventArgs e)
        {
            // A "MainWindow" Frame-jén keresztül navigálunk
            ((MainWindow)Application.Current.MainWindow).MainFrame.Navigate(new BejelentkezesPage());
            e.Handled = true;
        }

        private void JelszoLathatosagValtas1(object sender, RoutedEventArgs e)
        {

        }

        private void JelszoLathatosagValtas2(object sender, RoutedEventArgs e)
        {

        }

        private void PasswordBoxes_Valtozas(object sender, RoutedEventArgs e)
        {

        }

        private void PasswordBox_FokuszVesztes(object sender, RoutedEventArgs e)
        {

        }

        private void PasswordTextBox1_SzovegValtozas(object sender, TextChangedEventArgs e)
        {

        }

        private void PasswordTextBox2_SzovegValtozas(object sender, TextChangedEventArgs e)
        {

        }

        private void Regisztracio_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}

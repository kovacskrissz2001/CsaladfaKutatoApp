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
    /// Interaction logic for BejelentkezesPage.xaml
    /// </summary>
    public partial class BejelentkezesPage : Page
    {
        public BejelentkezesPage()
        {
            InitializeComponent();
        }

        private void Hyperlink_Regisztracio(object sender, RequestNavigateEventArgs e)
        {
            // A "MainWindow" Frame-jén keresztül navigálunk
            ((MainWindow)Application.Current.MainWindow).MainFrame.Navigate(new RegisztracioPage());
            e.Handled = true;
        }
    }
}

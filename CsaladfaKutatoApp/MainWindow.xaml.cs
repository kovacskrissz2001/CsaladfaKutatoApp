using System.Text;
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
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            // Itt töltjük be a kezdőoldalt (bejelentkezés)
            //MainFrame.Navigate(new BejelentkezesPage());

            MainFrame.Navigate(new ElsoCsaladtagHozzaadPage());
        }

        
    }
}
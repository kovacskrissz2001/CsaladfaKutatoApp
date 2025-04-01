using System.IO;
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
using CsaladfaKutatoApp.Models;
using MahApps.Metro.Controls;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace CsaladfaKutatoApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        private CsaladfaAdatbazisContext _context;
        public MainWindow()
        {
            InitializeComponent();
            // Itt töltjük be a kezdőoldalt (bejelentkezés)
            //MainFrame.Navigate(new BejelentkezesPage());

            MainFrame.Navigate(new ElsoCsaladtagHozzaadPage());

            // Konfiguráció beolvasása
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<CsaladfaAdatbazisContext>();
            optionsBuilder.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));

            // DbContext példányosítása
            _context = new CsaladfaAdatbazisContext(optionsBuilder.Options);

        }


    }
}
using CsaladfaKutatoApp.Models;
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


namespace CsaladfaKutatoApp
{
    /// <summary>
    /// Interaction logic for KezdoTartalomControl.xaml
    /// </summary>
    public partial class KezdoTartalomControl : UserControl
    {
        private KozpontiPage SzuloOlal;
        private readonly CsaladfaAdatbazisContext _context;

        public KezdoTartalomControl(KozpontiPage Szulo, CsaladfaAdatbazisContext context)
        {
            InitializeComponent();
            SzuloOlal = Szulo;
            _context = context;
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void NemKapcsolodoHozzaadasa_Click(object sender, RoutedEventArgs e)
        {
            SzuloOlal.TartalomValtas(new KapcsolodoSzemelyLetrehozControl(SzuloOlal, _context));
        }

        private void LanyaHozzaadasa_Click(object sender, RoutedEventArgs e)
        {

        }

        private void FiaHozzaadasa_Click(object sender, RoutedEventArgs e)
        {

        }

        private void PartnerHozzaadasa_Click(object sender, RoutedEventArgs e)
        {

        }

        private void FiuTestverHozzaadasa_Click(object sender, RoutedEventArgs e)
        {

        }

        private void LanyTestverHozzaadasa_Click(object sender, RoutedEventArgs e)
        {

        }

        private void AnyaHozzaadasa_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ApaHozzaadasa_Click(object sender, RoutedEventArgs e)
        {

        }

        
    }
}

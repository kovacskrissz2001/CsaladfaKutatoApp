using CsaladfaKutatoApp.Models;
using CsaladfaKutatoApp.Models.DTO;
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
    /// Interaction logic for SzemelyKapcsolataiControl.xaml
    /// </summary>
    public partial class SzemelyKapcsolataiControl : UserControl
    {
        private KozpontiPage KpOldal;
        private readonly CsaladfaAdatbazisContext _context;
        private RajzoltSzemely _legutobbKijeloltSzemely;
        private List<RajzoltSzemely> szemelyGyerekei;
        private List<RajzoltSzemely> szemelyTestverei;

        public SzemelyKapcsolataiControl(KozpontiPage kpoldal, CsaladfaAdatbazisContext context, RajzoltSzemely legutobbKijeloltSzemely)
        {
            InitializeComponent();
            KpOldal = kpoldal;
            _context = context;
            _legutobbKijeloltSzemely = legutobbKijeloltSzemely;


            szemelyGyerekei = _legutobbKijeloltSzemely.Gyermekei;
            szemelyTestverei = _legutobbKijeloltSzemely.Testverek;
        }


        private void Panel_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void Vissza_Click(object sender, RoutedEventArgs e)
        {
            var kezdo = new KezdoTartalomControl(KpOldal, _context);

            // Ha van legutóbb kijelölt, beállítjuk
            if (KpOldal.LegutobbKijeloltSzemely is not null)
            {
                kezdo.DataContext = KpOldal.LegutobbKijeloltSzemely;
            }

            KpOldal.TartalomValtas(kezdo);
        }
    }
}

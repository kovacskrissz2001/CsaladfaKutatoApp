using System;
using System.Collections.Generic;
using System.Data.Common;
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
using CsaladfaKutatoApp.Models;
using MahApps.Metro.Controls;
using Microsoft.EntityFrameworkCore;

namespace CsaladfaKutatoApp
{
    /// <summary>
    /// Interaction logic for ElsoCsaladtagHozzaadPage.xaml
    /// </summary>
    public partial class ElsoCsaladtagHozzaadPage : Page
    {
        private readonly CsaladfaAdatbazisContext _context;
        public ElsoCsaladtagHozzaadPage(CsaladfaAdatbazisContext context)
        {
            InitializeComponent();
            _context= context;
        }
        private string FindCheckedRadioButton(string groupName)
        {
            foreach (var rb in FindVisualChildren<RadioButton>(this))
            {
                if (rb.GroupName == groupName && rb.IsChecked == true)
                    return rb.Content.ToString();
            }
            return null;
        }

        private DbParameter CreateParameter(DbCommand command, string name, object value)
        {
            var param = command.CreateParameter();
            param.ParameterName = name;
            param.Value = value ?? DBNull.Value;
            return param;
        }

        private static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T t)
                        yield return t;

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                        yield return childOfChild;
                }
            }
        }
        private void Regisztracio_Click(object sender, RoutedEventArgs e)
        {

            // A "MainWindow" Frame-jén keresztül navigálunk
            ((MainWindow)System.Windows.Application.Current.MainWindow).MainFrame.Navigate(new RegisztracioPage(_context));
            e.Handled = true;
        }

        private void Hozzaadas_Click(object sender, RoutedEventArgs e)
        {
            string keresztnev = KeresztnevTextBox.Text.Trim();
            string vezeteknev = VezeteknevTextBox.Text.Trim();
            string szuletesiDatumSzoveg = SzuletesiDatumTextBox.Text.Trim();
            string szuletesiHely = SzuletesiHelyTextBox.Text.Trim();

            // Nem (rádiógomb) kiválasztása
            string neme = FindCheckedRadioButton("Nem");
            // Élő státusz kiválasztása (Igen = 1, Nem = 0)
            string eloSzoveg = FindCheckedRadioButton("Elo");
            bool? elo = eloSzoveg == "Igen" ? true : eloSzoveg == "Nem" ? false : null;

            // Validáció
            if (string.IsNullOrWhiteSpace(keresztnev) ||
                string.IsNullOrWhiteSpace(vezeteknev) ||
                string.IsNullOrWhiteSpace(szuletesiDatumSzoveg) ||
                string.IsNullOrWhiteSpace(szuletesiHely) ||
                string.IsNullOrWhiteSpace(neme) ||
                !elo.HasValue)
            {
                MessageBox.Show("Minden mező kitöltése kötelező!", "Hiba", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            //További validáció
            int? felhasznaloId = ((MainWindow)Application.Current.MainWindow).BejelentkezettFelhasznaloId;

            if (!felhasznaloId.HasValue)
            {
                MessageBox.Show("Hiba: a felhasználó nincs bejelentkezve.", "Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!DateTime.TryParse(szuletesiDatumSzoveg, out DateTime szuletesiDatum))
            {
                MessageBox.Show("Hibás születési dátum formátum!", "Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                var connection = _context.Database.GetDbConnection();
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "sp_HozzaadElsoCsaladtag";
                    command.CommandType = System.Data.CommandType.StoredProcedure;

                    command.Parameters.Add(CreateParameter(command, "@Keresztnev", keresztnev));
                    command.Parameters.Add(CreateParameter(command, "@Vezeteknev", vezeteknev));
                    command.Parameters.Add(CreateParameter(command, "@SzuletesiDatum", szuletesiDatum));
                    command.Parameters.Add(CreateParameter(command, "@Neme", neme));
                    command.Parameters.Add(CreateParameter(command, "@EloSzemely", elo.Value));
                    command.Parameters.Add(CreateParameter(command, "@SzuletesiHely", szuletesiHely));
                    command.Parameters.Add(CreateParameter(command, "@FelhasznaloId", ((MainWindow)System.Windows.Application.Current.MainWindow).BejelentkezettFelhasznaloId)); //  dinamikusan jön
                    

                    command.ExecuteNonQuery();
                }

                connection.Close();

                MessageBox.Show("Családtag sikeresen hozzáadva!", "Siker", MessageBoxButton.OK, MessageBoxImage.Information);

                // Navigálás Főolalra.
               ((MainWindow)System.Windows.Application.Current.MainWindow).MainFrame.Navigate(new KozpontiPage(_context));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Hiba történt a mentés során: {ex.Message}", "Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Megse_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}

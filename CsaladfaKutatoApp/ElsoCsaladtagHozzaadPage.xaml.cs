﻿using System;
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
using CsaladfaKutatoApp.Models;
using MahApps.Metro.Controls;

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

        private void Regisztracio_Click(object sender, RoutedEventArgs e)
        {

            // A "MainWindow" Frame-jén keresztül navigálunk
            ((MainWindow)System.Windows.Application.Current.MainWindow).MainFrame.Navigate(new RegisztracioPage(_context));
            e.Handled = true;
        }
    }
}

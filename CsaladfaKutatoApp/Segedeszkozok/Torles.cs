using CsaladfaKutatoApp.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CsaladfaKutatoApp.Segedeszkozok
{
    public class Torles
    {

        private CsaladfaAdatbazisContext UjDbContextPeldany()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<CsaladfaAdatbazisContext>();
            optionsBuilder.UseSqlServer(config.GetConnectionString("DefaultConnection"));

            return new CsaladfaAdatbazisContext(optionsBuilder.Options);
        }


        public void TorolSzemelyt(KozpontiPage KpOldal)
        {
            var kijeloltSzemely = KpOldal.LegutobbKijeloltSzemely;
            if (kijeloltSzemely == null)
            {
                MessageBox.Show("Nincs kijelölt személy.", "Figyelmeztetés", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (kijeloltSzemely.Gyermekei.Count != 0)
            {
                MessageBox.Show("Először a kijelölt személy gyermekeit lehetséges törölni, csak utána törölhető a kijelölt személy. Addig nem törölhető egy személy amíg van a családfában gyermeke.", "Figyelmeztetés", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }


            // Megerősítő ablak
            var result = MessageBox.Show(
                $"Biztosan törölni kívánja a következő személyt?\n\n{kijeloltSzemely.KNev} {kijeloltSzemely.VNev}",
                "Törlés megerősítése",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    using (var context = UjDbContextPeldany())
                    {
                        var szemely = context.Szemelyeks
                            .Include(sz => sz.KapcsolatokSzemelies)
                            .Include(sz => sz.KapcsolatokKapcsolodoSzemelies)
                            .Include(sz => sz.Fotoks)
                            .Include(sz => sz.Forrasoks)
                            .Include(sz => sz.Torteneteks)
                            .FirstOrDefault(sz => sz.SzemelyId == kijeloltSzemely.Azonosito);

                        if (szemely != null)
                        {
                            // Kapcsolatok törlése
                            context.Kapcsolatoks.RemoveRange(szemely.KapcsolatokSzemelies);
                            context.Kapcsolatoks.RemoveRange(szemely.KapcsolatokKapcsolodoSzemelies);

                            // Fotókhoz tartozó történetek törlése
                            foreach (var foto in szemely.Fotoks)
                            {
                                context.Torteneteks.RemoveRange(foto.Torteneteks);
                            }
                            context.Fotoks.RemoveRange(szemely.Fotoks);

                            // További kapcsolódó rekordok törlése
                            context.Forrasoks.RemoveRange(szemely.Forrasoks);
                            context.Torteneteks.RemoveRange(szemely.Torteneteks);

                            // Ha szükséges, ellenőrizzük a Helyszin törlését (csak akkor, ha nem tartozik más személyhez)
                            if (szemely.HelyszinId.HasValue)
                            {
                                var helyszin = context.Helyszineks
                                    .Include(h => h.Szemelyeks)
                                    .FirstOrDefault(h => h.HelyszinId == szemely.HelyszinId);

                                if (helyszin != null && helyszin.Szemelyeks.Count == 1) // csak ez a személy tartozik hozzá
                                {
                                    context.Helyszineks.Remove(helyszin);
                                }
                            }

                            // Végül maga a személy törlése
                            context.Szemelyeks.Remove(szemely);

                            context.SaveChanges();
                        }
                    }

                    // Canvas frissítése
                    KpOldal.TorolSzemelyACanvasrol(kijeloltSzemely);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Hiba történt a törlés során: " + ex.Message, "Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

        }



    }
}

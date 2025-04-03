using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CsaladfaKutatoApp.Segedeszkozok
{
    public static class JelszoHasher
    {
        // Hash készítése jelszó + só kombinációból
        public static string HashJelszoSalttal(string jelszo, string so)
        {
            var combined = Encoding.UTF8.GetBytes(jelszo + so);
            using (var sha = SHA256.Create())
            {
                var hashBytes = sha.ComputeHash(combined);
                return Convert.ToBase64String(hashBytes);
            }
        }

        // Só generálása (pl. 16 bájt = 128 bit)
        public static string SaltGeneralas()
        {
            byte[] saltBytes = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(saltBytes);
            }
            return Convert.ToBase64String(saltBytes);
        }
    }
}

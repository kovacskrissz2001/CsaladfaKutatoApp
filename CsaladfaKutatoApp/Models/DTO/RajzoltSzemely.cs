using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;
using System.Windows.Controls;

namespace CsaladfaKutatoApp.Models.DTO
{
    [NotMapped]
    public class RajzoltSzemely
    {
        public int Azonosito { get; set; }
        public string KNev { get; set; } = null!;
        public string VNev { get; set; } = null!;
        public string Nem { get; set; } = null!;
        public DateOnly? SzuletesiDatum { get; set; }
        public string? KepBase64 { get; set; }
        public Border UIElem { get; set; }
        public int GeneracioSzint { get; set; }
        public List<int> GyermekAzonositoLista { get; set; } = new();
    }
}

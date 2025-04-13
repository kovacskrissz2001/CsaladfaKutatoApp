using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;

namespace CsaladfaKutatoApp.Models.DTO
{
    [NotMapped]
    public class RajzoltSzemely
    {
        public int Azonosito { get; set; }
        public string KNev { get; set; }
        public string VNev { get; set; }
        public DateOnly? SzuletesiDatum { get; set; }
        public string? KepBase64 { get; set; }
        public List<int> GyermekAzonositoLista { get; set; } = new();
    }
}

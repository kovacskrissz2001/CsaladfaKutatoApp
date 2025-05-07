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
        public int GyermekekSzama { get; set; } 
        public RajzoltSzemely? Apa {  get; set; }
        public RajzoltSzemely? Anya { get; set; }
        public List<RajzoltSzemely>? Testverek { get; set; }
        public RajzoltSzemely? Parja { get; set; }
        public List<RajzoltSzemely>? Gyermekei { get; set; }
        public bool? ApaRajzolElobb { get; set; }
        public bool? NotRajzoljunkElobbParbol { get; set; }
        public bool GyermekAzIlleto {  get; set; }
        public bool? ELsoGyerek {  get; set; }//A szülő első gyereke-e a személy
        public int? ELsoGyerekId { get; set; }//A személy első gyerekének az azonosítója
    }
}

using System;
using System.Collections.Generic;

namespace CsaladfaKutatoApp.Models;

public partial class Helyszinek
{
    public int HelyszinId { get; set; }

    public string? SzuletesiHely { get; set; }

    public string? HalalozasiHely { get; set; }

    public string? OrokNyugalomHelye { get; set; }

    public string? Leiras { get; set; }

    public virtual ICollection<Szemelyek> Szemelyeks { get; set; } = new List<Szemelyek>();
}

using System;
using System.Collections.Generic;

namespace CsaladfaKutatoApp.Models;

public partial class Helyszinek
{
    public int HelyszinId { get; set; }

    public string? SzuletesiOrszag { get; set; } = null!;

    public string? SzuletesiTelepules { get; set; } = null!;

    public string? SzuletesiRegio { get; set; } 

    public string? HalalozasiOrszag { get; set; }

    public string? HalalozasiRegio { get; set; }

    public string? HalalozasiTelepules { get; set; }

    public string? OrokNyugalomHelyeOrszag { get; set; }

    public string? OrokNyugalomHelyeRegio { get; set; }

    public string? OrokNyugalomHelyeTelepules { get; set; }

    public virtual ICollection<Szemelyek> Szemelyeks { get; set; } = new List<Szemelyek>();
}

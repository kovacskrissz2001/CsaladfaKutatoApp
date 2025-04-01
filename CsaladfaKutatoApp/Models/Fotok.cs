using System;
using System.Collections.Generic;

namespace CsaladfaKutatoApp.Models;

public partial class Fotok
{
    public int FotoId { get; set; }

    public int SzemelyId { get; set; }

    public string FotoBase64 { get; set; } = null!;

    public string? Leiras { get; set; }

    public virtual Szemelyek Szemely { get; set; } = null!;

    public virtual ICollection<Tortenetek> Torteneteks { get; set; } = new List<Tortenetek>();
}

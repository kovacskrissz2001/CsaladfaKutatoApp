using System;
using System.Collections.Generic;

namespace CsaladfaKutatoApp.Models;

public partial class Forrasok
{
    public int ForrasId { get; set; }

    public int SzemelyId { get; set; }

    public string ForrasCime { get; set; } = null!;

    public string? Jegyzet { get; set; }

    public string? Leiras { get; set; }

    public string FajlEleresiUt { get; set; } = null!;

    public virtual Szemelyek Szemely { get; set; } = null!;
}

using System;
using System.Collections.Generic;

namespace CsaladfaKutatoApp.Models;

public partial class Tortenetek
{
    public int TortenetId { get; set; }

    public int SzemelyId { get; set; }

    public int? FotoId { get; set; }

    public string? Tortenet { get; set; }

    public virtual Fotok? Foto { get; set; }

    public virtual Szemelyek Szemely { get; set; } = null!;
}

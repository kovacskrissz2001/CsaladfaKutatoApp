using System;
using System.Collections.Generic;

namespace CsaladfaKutatoApp.Models;

public partial class Mellekletek
{
    public int MellekletId { get; set; }

    public int SzemelyId { get; set; }

    public string FajlEleresiUt { get; set; } = null!;

    public string? Leiras { get; set; }

    public DateTime FeltoltesDatum { get; set; }

    public virtual Szemelyek Szemely { get; set; } = null!;
}

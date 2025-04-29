using System;
using System.Collections.Generic;

namespace CsaladfaKutatoApp.Models;

public partial class Kapcsolatok
{
    public int KapcsolatId { get; set; }

    public int SzemelyId { get; set; }

    public int KapcsolodoSzemelyId { get; set; }

    public string KapcsolatTipusa { get; set; } = null!;

    public virtual Szemelyek KapcsolodoSzemely { get; set; } = null!;

    public virtual Szemelyek Szemely { get; set; } = null!;
}

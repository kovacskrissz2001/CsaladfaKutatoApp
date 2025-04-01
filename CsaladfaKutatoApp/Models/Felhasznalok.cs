using System;
using System.Collections.Generic;

namespace CsaladfaKutatoApp.Models;

public partial class Felhasznalok
{
    public int FelhasznaloId { get; set; }

    public string Felhasznalonev { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string JelszoHash { get; set; } = null!;

    public string JelszoSalt { get; set; } = null!;

    public string BejelentkezesiMod { get; set; } = null!;

    public virtual ICollection<Szemelyek> Szemelyeks { get; set; } = new List<Szemelyek>();
}

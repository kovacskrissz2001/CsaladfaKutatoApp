using System;
using System.Collections.Generic;

namespace CsaladfaKutatoApp.Models;

public partial class Szemelyek
{
    public int SzemelyId { get; set; }

    public string Keresztnev { get; set; } = null!;

    public string Vezeteknev { get; set; } = null!;

    public DateOnly? SzuletesiDatum { get; set; } = null!;

    public DateOnly? HalalozasiDatum { get; set; }

    public bool EloSzemely { get; set; } 

    public string? Neme { get; set; } = null!;

    public string? Tanulmanya { get; set; }

    public string? Foglalkozasa { get; set; }

    public string? Vallasa { get; set; }

    public int FelhasznaloId { get; set; }

    public int? HelyszinId { get; set; }

    public virtual Felhasznalok Felhasznalo { get; set; } = null!;

    public virtual ICollection<Forrasok> Forrasoks { get; set; } = new List<Forrasok>();

    public virtual ICollection<Fotok> Fotoks { get; set; } = new List<Fotok>();

    public virtual Helyszinek? Helyszin { get; set; }

    public virtual ICollection<Kapcsolatok> KapcsolatokKapcsolodoSzemelies { get; set; } = new List<Kapcsolatok>();

    public virtual ICollection<Kapcsolatok> KapcsolatokSzemelies { get; set; } = new List<Kapcsolatok>();

    public virtual ICollection<Mellekletek> Mellekleteks { get; set; } = new List<Mellekletek>();

    public virtual ICollection<Tortenetek> Torteneteks { get; set; } = new List<Tortenetek>();
}

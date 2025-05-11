using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace CsaladfaKutatoApp.Models;

public partial class CsaladfaAdatbazisContext : DbContext
{
    public CsaladfaAdatbazisContext()
    {
    }

    public CsaladfaAdatbazisContext(DbContextOptions<CsaladfaAdatbazisContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Felhasznalok> Felhasznaloks { get; set; }

    public virtual DbSet<Fotok> Fotoks { get; set; }

    public virtual DbSet<Helyszinek> Helyszineks { get; set; }

    public virtual DbSet<Kapcsolatok> Kapcsolatoks { get; set; }

    public virtual DbSet<Szemelyek> Szemelyeks { get; set; }

    public virtual DbSet<Tortenetek> Torteneteks { get; set; }



    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Felhasznalok>(entity =>
        {
            entity.HasKey(e => e.FelhasznaloId).HasName("PK__Felhaszn__6DCA410F3C5A57A5");

            entity.ToTable("Felhasznalok");

            entity.HasIndex(e => e.Email, "UQ__Felhaszn__A9D10534FD1B763B").IsUnique();

            entity.HasIndex(e => e.Felhasznalonev, "UQ__Felhaszn__EF32ED2DA0F0BF83").IsUnique();

            entity.Property(e => e.BejelentkezesiMod).HasMaxLength(100);
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.Felhasznalonev).HasMaxLength(50);
        });

        modelBuilder.Entity<Fotok>(entity =>
        {
            entity.HasKey(e => e.FotoId).HasName("PK__Fotok__4EA1C119D3403396");

            entity.ToTable("Fotok");

            entity.HasOne(d => d.Szemely).WithMany(p => p.Fotoks)
                .HasForeignKey(d => d.SzemelyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Fotok__SzemelyId__71D1E811");
        });

        modelBuilder.Entity<Helyszinek>(entity =>
        {
            entity.HasKey(e => e.HelyszinId).HasName("PK__Helyszin__B22451B1E7522CF5");

            entity.ToTable("Helyszinek");

            
            entity.Property(e => e.SzuletesiOrszag).HasMaxLength(255);
            entity.Property(e => e.SzuletesiRegio).HasMaxLength(255);
            entity.Property(e => e.SzuletesiTelepules).HasMaxLength(255);
            entity.Property(e => e.HalalozasiOrszag).HasMaxLength(255);
            entity.Property(e => e.HalalozasiRegio).HasMaxLength(255);
            entity.Property(e => e.HalalozasiTelepules).HasMaxLength(255);
            entity.Property(e => e.OrokNyugalomHelyeOrszag).HasMaxLength(255);
            entity.Property(e => e.OrokNyugalomHelyeRegio).HasMaxLength(255);
            entity.Property(e => e.OrokNyugalomHelyeTelepules).HasMaxLength(255);
        });

        modelBuilder.Entity<Kapcsolatok>(entity =>
        {
            entity.HasKey(e => e.KapcsolatId).HasName("PK__Kapcsola__1A8CCCDE07C7F119");

            entity.ToTable("Kapcsolatok");

            entity.Property(e => e.KapcsolatTipusa).HasMaxLength(50);
            

            entity.HasOne(d => d.KapcsolodoSzemely).WithMany(p => p.KapcsolatokKapcsolodoSzemelies)
                .HasForeignKey(d => d.KapcsolodoSzemelyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Kapcsolat__Kapcs__75A278F5");

            entity.HasOne(d => d.Szemely).WithMany(p => p.KapcsolatokSzemelies)
                .HasForeignKey(d => d.SzemelyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Kapcsolat__Szeme__74AE54BC");
        });

        

        modelBuilder.Entity<Szemelyek>(entity =>
        {
            entity.HasKey(e => e.SzemelyId).HasName("PK__Szemelye__1507328B1DEC4C45");

            entity.ToTable("Szemelyek");

            entity.Property(e => e.Foglalkozasa).HasMaxLength(100);
            entity.Property(e => e.Keresztnev).HasMaxLength(100);
            entity.Property(e => e.Neme).HasMaxLength(10);
            entity.Property(e => e.Tanulmanya).HasMaxLength(100);
            entity.Property(e => e.Vallasa).HasMaxLength(100);
            entity.Property(e => e.Vezeteknev).HasMaxLength(100);

            entity.HasOne(d => d.Felhasznalo).WithMany(p => p.Szemelyeks)
                .HasForeignKey(d => d.FelhasznaloId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Szemelyek__Felha__6E01572D");

            entity.HasOne(d => d.Helyszin).WithMany(p => p.Szemelyeks)
                .HasForeignKey(d => d.HelyszinId)
                .HasConstraintName("FK__Szemelyek__Helys__6EF57B66");
        });

        modelBuilder.Entity<Tortenetek>(entity =>
        {
            entity.HasKey(e => e.TortenetId).HasName("PK__Tortenet__99EFBE5D3FA2E1F5");

            entity.ToTable("Tortenetek");

            entity.HasOne(d => d.Foto).WithMany(p => p.Torteneteks)
                .HasForeignKey(d => d.FotoId)
                .HasConstraintName("FK__Tortenete__FotoI__7D439ABD");

            entity.HasOne(d => d.Szemely).WithMany(p => p.Torteneteks)
                .HasForeignKey(d => d.SzemelyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Tortenete__Szeme__7C4F7684");
        });

        

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

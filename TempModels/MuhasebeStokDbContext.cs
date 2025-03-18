using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace MuhasebeStokWebApp.TempModels;

public partial class MuhasebeStokDbContext : DbContext
{
    public MuhasebeStokDbContext()
    {
    }

    public MuhasebeStokDbContext(DbContextOptions<MuhasebeStokDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<DovizKurlari> DovizKurlaris { get; set; }

    public virtual DbSet<Dovizler> Dovizlers { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("Name=ConnectionStrings:DefaultConnection");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DovizKurlari>(entity =>
        {
            entity.HasKey(e => e.DovizKuruId);

            entity.ToTable("DovizKurlari");

            entity.Property(e => e.DovizKuruId)
                .ValueGeneratedNever()
                .HasColumnName("DovizKuruID");
            entity.Property(e => e.Aciklama)
                .HasMaxLength(500)
                .HasDefaultValue("");
            entity.Property(e => e.Aktif).HasDefaultValue(true);
            entity.Property(e => e.AlisFiyati).HasColumnType("decimal(18, 6)");
            entity.Property(e => e.BazParaBirimi)
                .HasMaxLength(10)
                .HasDefaultValue("");
            entity.Property(e => e.DovizAdi).HasMaxLength(100);
            entity.Property(e => e.DovizKodu)
                .HasMaxLength(10)
                .HasDefaultValue("");
            entity.Property(e => e.EfektifAlisFiyati).HasColumnType("decimal(18, 6)");
            entity.Property(e => e.EfektifSatisFiyati).HasColumnType("decimal(18, 6)");
            entity.Property(e => e.HedefParaBirimi)
                .HasMaxLength(3)
                .HasDefaultValue("");
            entity.Property(e => e.Kaynak)
                .HasMaxLength(100)
                .HasDefaultValue("");
            entity.Property(e => e.KaynakParaBirimi)
                .HasMaxLength(3)
                .HasDefaultValue("");
            entity.Property(e => e.Kur).HasColumnType("decimal(18, 6)");
            entity.Property(e => e.KurDegeri).HasColumnType("decimal(18, 6)");
            entity.Property(e => e.OlusturmaTarihi).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.ParaBirimi)
                .HasMaxLength(10)
                .HasDefaultValue("");
            entity.Property(e => e.SatisFiyati).HasColumnType("decimal(18, 6)");
        });

        modelBuilder.Entity<Dovizler>(entity =>
        {
            entity.HasKey(e => e.DovizId);

            entity.ToTable("Dovizler");

            entity.Property(e => e.DovizId).HasColumnName("DovizID");
            entity.Property(e => e.DovizAdi).HasMaxLength(50);
            entity.Property(e => e.DovizKodu).HasMaxLength(10);
            entity.Property(e => e.Sembol).HasMaxLength(10);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

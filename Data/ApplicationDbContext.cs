using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using MuhasebeStokWebApp.Data.Entities;
using MuhasebeStokWebApp.Models;

namespace MuhasebeStokWebApp.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Entities namespace'inden gelen modeller
        public virtual DbSet<Entities.Banka> Bankalar { get; set; }
        public virtual DbSet<Entities.BankaHareket> BankaHareketleri { get; set; }
        public virtual DbSet<Entities.Cari> Cariler { get; set; }
        public virtual DbSet<Entities.Fatura> Faturalar { get; set; }
        public virtual DbSet<Entities.FaturaDetay> FaturaDetaylari { get; set; }
        public virtual DbSet<Entities.FaturaTuru> FaturaTurleri { get; set; }
        public virtual DbSet<Entities.Menu> Menuler { get; set; }
        public virtual DbSet<Entities.MenuRol> MenuRoller { get; set; }
        public virtual DbSet<Entities.OdemeTuru> OdemeTurleri { get; set; }
        public virtual DbSet<Entities.StokHareket> StokHareketleri { get; set; }
        public virtual DbSet<Entities.ParaBirimi> ParaBirimleri { get; set; }
        public virtual DbSet<Entities.ParaBirimiIliski> ParaBirimiIliskileri { get; set; }
        public virtual DbSet<Entities.KurDegeri> KurDegerleri { get; set; }
        public virtual DbSet<Entities.SistemAyarlari> SistemAyarlari { get; set; }
        public virtual DbSet<Entities.SistemLog> SistemLoglar { get; set; }
        public virtual DbSet<Entities.Urun> Urunler { get; set; }
        public virtual DbSet<Entities.FiyatTipi> FiyatTipleri { get; set; }
        public virtual DbSet<Entities.UrunFiyat> UrunFiyatlari { get; set; }
        public virtual DbSet<Entities.UrunKategori> UrunKategorileri { get; set; }
        public virtual DbSet<Entities.StokFifo> StokFifo { get; set; }
        public virtual DbSet<Entities.Depo> Depolar { get; set; }
        public virtual DbSet<Entities.CariHareket> CariHareketler { get; set; }
        public virtual DbSet<Entities.FaturaOdeme> FaturaOdemeleri { get; set; }
        public virtual DbSet<Entities.Birim> Birimler { get; set; }
        public virtual DbSet<Entities.Kasa> Kasalar { get; set; }
        public virtual DbSet<Entities.KasaHareket> KasaHareketleri { get; set; }
        public virtual DbSet<Entities.DovizKuru> DovizKurlari { get; set; }
        public virtual DbSet<Entities.Irsaliye> Irsaliyeler { get; set; }
        public virtual DbSet<Entities.IrsaliyeDetay> IrsaliyeDetaylari { get; set; }
        public virtual DbSet<Entities.IrsaliyeTuru> IrsaliyeTurleri { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Burada Entity Framework ile ilgili özel konfigürasyonlar yapılabilir
            // Örneğin, cascade delete davranışlarını değiştirmek veya unique indeksler eklemek gibi
            
            // Urun
            modelBuilder.Entity<Entities.Urun>()
                .Property(u => u.StokMiktar)
                .HasDefaultValue(0);

            modelBuilder.Entity<Entities.Urun>()
                .Property(u => u.Aktif)
                .HasDefaultValue(true);

            modelBuilder.Entity<Entities.Urun>()
                .Property(u => u.SoftDelete)
                .HasDefaultValue(false);
                
            // UrunFiyat
            modelBuilder.Entity<Entities.UrunFiyat>()
                .ToTable("UrunFiyatlari");
                
            // StokFifo indeksleri
            modelBuilder.Entity<Entities.StokFifo>()
                .HasIndex(f => f.UrunID)
                .HasDatabaseName("IX_StokFifo_UrunID");
                
            modelBuilder.Entity<Entities.StokFifo>()
                .HasIndex(f => f.GirisTarihi)
                .HasDatabaseName("IX_StokFifo_GirisTarihi");
                
            modelBuilder.Entity<Entities.StokFifo>()
                .HasIndex(f => new { f.UrunID, f.KalanMiktar, f.Aktif, f.SoftDelete, f.Iptal })
                .HasDatabaseName("IX_StokFifo_StokSorgu");
                
            modelBuilder.Entity<Entities.StokFifo>()
                .HasIndex(f => new { f.ReferansID, f.ReferansTuru })
                .HasDatabaseName("IX_StokFifo_Referans");

            // UrunKategori
            modelBuilder.Entity<Entities.UrunKategori>()
                .Property(k => k.Aktif)
                .HasDefaultValue(true);

            modelBuilder.Entity<Entities.UrunKategori>()
                .Property(k => k.SoftDelete)
                .HasDefaultValue(false);

            // Depo
            modelBuilder.Entity<Entities.Depo>()
                .Property(d => d.Aktif)
                .HasDefaultValue(true);

            modelBuilder.Entity<Entities.Depo>()
                .Property(d => d.SoftDelete)
                .HasDefaultValue(false);

            // Cari
            modelBuilder.Entity<Entities.Cari>()
                .Property(c => c.Aktif)
                .HasDefaultValue(true);

            modelBuilder.Entity<Entities.Cari>()
                .Property(c => c.SoftDelete)
                .HasDefaultValue(false);

            // Fatura
            modelBuilder.Entity<Entities.Fatura>()
                .Property(f => f.Aktif)
                .HasDefaultValue(true);

            modelBuilder.Entity<Entities.Fatura>()
                .Property(f => f.SoftDelete)
                .HasDefaultValue(false);

            modelBuilder.Entity<Entities.Fatura>()
                .Property(f => f.Resmi)
                .HasDefaultValue(true);

            // Irsaliye
            modelBuilder.Entity<Entities.Irsaliye>()
                .Property(i => i.Aktif)
                .HasDefaultValue(true);

            modelBuilder.Entity<Entities.Irsaliye>()
                .Property(i => i.Resmi)
                .HasDefaultValue(true);
                
            // Kasa
            modelBuilder.Entity<Entities.Kasa>()
                .Property(k => k.Aktif)
                .HasDefaultValue(true);

            modelBuilder.Entity<Entities.Kasa>()
                .Property(k => k.SoftDelete)
                .HasDefaultValue(false);

            modelBuilder.Entity<Entities.Kasa>()
                .Property(k => k.AcilisBakiye)
                .HasDefaultValue(0);

            modelBuilder.Entity<Entities.Kasa>()
                .Property(k => k.GuncelBakiye)
                .HasDefaultValue(0);
                
            // Kasa ile KasaHareket arasındaki ilişkiler
            modelBuilder.Entity<Entities.Kasa>()
                .HasMany(k => k.KasaHareketleri)
                .WithOne(h => h.Kasa)
                .HasForeignKey(h => h.KasaID)
                .OnDelete(DeleteBehavior.NoAction);
            
            // Kasa'nın HedefKasa ilişkisi
            modelBuilder.Entity<Entities.KasaHareket>()
                .HasOne(h => h.HedefKasa)
                .WithMany()
                .HasForeignKey(h => h.HedefKasaID)
                .OnDelete(DeleteBehavior.NoAction);
                
            // KasaHareket
            modelBuilder.Entity<Entities.KasaHareket>()
                .Property(k => k.SoftDelete)
                .HasDefaultValue(false);
                
            // Banka
            modelBuilder.Entity<Entities.Banka>()
                .Property(b => b.Aktif)
                .HasDefaultValue(true);

            modelBuilder.Entity<Entities.Banka>()
                .Property(b => b.SoftDelete)
                .HasDefaultValue(false);

            modelBuilder.Entity<Entities.Banka>()
                .Property(b => b.AcilisBakiye)
                .HasDefaultValue(0);

            modelBuilder.Entity<Entities.Banka>()
                .Property(b => b.GuncelBakiye)
                .HasDefaultValue(0);
                
            modelBuilder.Entity<Entities.Banka>()
                .Property(b => b.ParaBirimi)
                .HasDefaultValue("TRY");
                
            // BankaHareket
            modelBuilder.Entity<Entities.BankaHareket>()
                .Property(b => b.SoftDelete)
                .HasDefaultValue(false);
                
            // DovizKuru
            modelBuilder.Entity<Entities.DovizKuru>()
                .Property(d => d.Aktif)
                .HasDefaultValue(true);
                
            modelBuilder.Entity<Entities.DovizKuru>()
                .Property(d => d.SoftDelete)
                .HasDefaultValue(false);

            // İrsaliye için global sorgu filtresi ekle
            modelBuilder.Entity<Entities.Irsaliye>().HasQueryFilter(i => i.Aktif);
                
            modelBuilder.Entity<Entities.Irsaliye>()
                .Property(i => i.Aktif)
                .HasDefaultValue(true);

            // ParaBirimiIliski ilişkilerini ayarla
            modelBuilder.Entity<Entities.ParaBirimiIliski>()
                .HasOne(p => p.KaynakParaBirimi)
                .WithMany()
                .HasForeignKey(p => p.KaynakParaBirimiID)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Entities.ParaBirimiIliski>()
                .HasOne(p => p.HedefParaBirimi)
                .WithMany()
                .HasForeignKey(p => p.HedefParaBirimiID)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
} 
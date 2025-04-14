using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using MuhasebeStokWebApp.Data.Entities;
using MuhasebeStokWebApp.Models;
using MuhasebeStokWebApp.Data.Entities.ParaBirimiModulu;
using Microsoft.Extensions.Configuration;
using MuhasebeStokWebApp.Enums;

namespace MuhasebeStokWebApp.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Temel entity'ler
        public virtual DbSet<Entities.Birim> Birimler { get; set; }
        public virtual DbSet<Entities.Cari> Cariler { get; set; }
        public virtual DbSet<Entities.Depo> Depolar { get; set; }
        public virtual DbSet<Entities.Fatura> Faturalar { get; set; }
        public virtual DbSet<Entities.FaturaDetay> FaturaDetaylari { get; set; }
        public virtual DbSet<Entities.FaturaOdeme> FaturaOdemeleri { get; set; }
        public virtual DbSet<Entities.FaturaTuru> FaturaTurleri { get; set; }
        public virtual DbSet<Entities.FiyatTipi> FiyatTipleri { get; set; }
        public virtual DbSet<Entities.Irsaliye> Irsaliyeler { get; set; }
        public virtual DbSet<Entities.IrsaliyeDetay> IrsaliyeDetaylari { get; set; }
        public virtual DbSet<Entities.IrsaliyeTuru> IrsaliyeTurleri { get; set; }
        public virtual DbSet<Entities.Sozlesme> Sozlesmeler { get; set; }
        
        // Para Birimi Modülü Entity'leri
        public virtual DbSet<Entities.ParaBirimiModulu.ParaBirimi> ParaBirimleri { get; set; }
        public virtual DbSet<Entities.ParaBirimiModulu.KurDegeri> KurDegerleri { get; set; }
        public virtual DbSet<Entities.ParaBirimiModulu.ParaBirimiIliski> ParaBirimiIliskileri { get; set; }
        public virtual DbSet<Entities.ParaBirimiModulu.KurMarj> KurMarjlari { get; set; }

        // Menu entity'leri
        public virtual DbSet<Entities.Menu> Menuler { get; set; }
        public virtual DbSet<Entities.MenuRol> MenuRoller { get; set; }
        
        // Banka ve Kasa entity'leri
        public virtual DbSet<Entities.Banka> Bankalar { get; set; }
        public virtual DbSet<Entities.BankaHareket> BankaHareketleri { get; set; }
        public virtual DbSet<Entities.Kasa> Kasalar { get; set; }
        public virtual DbSet<Entities.KasaHareket> KasaHareketleri { get; set; }

        // Ödeme türleri
        public virtual DbSet<Entities.OdemeTuru> OdemeTurleri { get; set; }

        // Stok entity'leri
        public virtual DbSet<Entities.StokFifo> StokFifoKayitlari { get; set; }
        public virtual DbSet<Entities.StokHareket> StokHareketleri { get; set; }
        public virtual DbSet<Entities.StokCikisDetay> StokCikisDetaylari { get; set; }
        public virtual DbSet<Entities.Urun> Urunler { get; set; }
        public virtual DbSet<Entities.UrunFiyat> UrunFiyatlari { get; set; }
        public virtual DbSet<Entities.UrunKategori> UrunKategorileri { get; set; }

        // Log entity'leri
        public virtual DbSet<Entities.SistemLog> SistemLoglar { get; set; }

        // Sistem Ayarları
        public virtual DbSet<Entities.SistemAyar> SistemAyarlari { get; set; }
        public virtual DbSet<Entities.SistemAyarlari> GenelSistemAyarlari { get; set; }

        public DbSet<CariHareket> CariHareketler { get; set; }

        // Banka hesapları
        public DbSet<BankaHesap> BankaHesaplari { get; set; }
        public DbSet<BankaHesapHareket> BankaHesapHareketleri { get; set; }
        
        // Merkezi aklama veritabanı nesneleri

        public virtual DbSet<FaturaAklamaKuyruk> FaturaAklamaKuyrugu { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Program.cs'te zaten yapılandırma yapıldı, burada sadece
            // henüz yapılandırılmadıysa basit bir yapılandırma ekleyelim
            if (!optionsBuilder.IsConfigured)
            {
                IConfigurationRoot configuration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json")
                    .Build();
                var connectionString = configuration.GetConnectionString("DefaultConnection");
                optionsBuilder.UseSqlServer(connectionString, options => options.CommandTimeout(120));
            }
            else
            {
                // Zaten yapılandırılmış olsa bile command timeout'u ayarla
                optionsBuilder.UseSqlServer(optionsBuilder.Options.FindExtension<Microsoft.EntityFrameworkCore.SqlServer.Infrastructure.Internal.SqlServerOptionsExtension>().ConnectionString, options => options.CommandTimeout(120));
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Identity ile ilgili tablolardaki Id alanları için SQL Server uyumluluğu
            modelBuilder.Entity<ApplicationUser>(entity => 
            {
                entity.Property(p => p.Id).HasColumnType("varchar(128)");
                
                // Adres, TelefonNo ve Silindi sütunlarını yapılandır
                entity.Property(p => p.Adres).HasMaxLength(200).IsRequired(false);
                entity.Property(p => p.TelefonNo).IsRequired(false);
                entity.Property(p => p.Silindi).HasDefaultValue(false);
            });

            modelBuilder.Entity<IdentityRole>().Property(p => p.Id).HasColumnType("varchar(128)");
            modelBuilder.Entity<IdentityUserRole<string>>().Property(p => p.UserId).HasColumnType("varchar(128)");
            modelBuilder.Entity<IdentityUserRole<string>>().Property(p => p.RoleId).HasColumnType("varchar(128)");
            modelBuilder.Entity<IdentityUserClaim<string>>().Property(p => p.UserId).HasColumnType("varchar(128)");
            modelBuilder.Entity<IdentityUserLogin<string>>().Property(p => p.UserId).HasColumnType("varchar(128)");
            modelBuilder.Entity<IdentityRoleClaim<string>>().Property(p => p.RoleId).HasColumnType("varchar(128)");
            modelBuilder.Entity<IdentityUserToken<string>>().Property(p => p.UserId).HasColumnType("varchar(128)");
            
            // Burada Entity Framework ile ilgili özel konfigürasyonlar yapılabilir
            // Örneğin, cascade delete davranışlarını değiştirmek veya unique indeksler eklemek gibi
            
            // KurDegeri ilişkilerini tanımla - döviz modülü yeniden tasarlandığı için kaldırıldı
            
            // Urun
            modelBuilder.Entity<Entities.Urun>()
                .Property(u => u.Aktif)
                .HasDefaultValue(true);

            modelBuilder.Entity<Entities.Urun>()
                .Property(u => u.Silindi)
                .HasDefaultValue(false);
                
            // UrunBirim ilişkisini Birim ile değiştir
            modelBuilder.Entity<Entities.Urun>()
                .HasOne(u => u.Birim)
                .WithMany(b => b.Urunler)
                .HasForeignKey(u => u.BirimID)
                .OnDelete(DeleteBehavior.SetNull);
                
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
                .HasIndex(f => new { f.UrunID, f.KalanMiktar, f.Aktif, f.Silindi, f.Iptal })
                .HasDatabaseName("IX_StokFifo_StokSorgu");
                
            modelBuilder.Entity<Entities.StokFifo>()
                .HasIndex(f => new { f.ReferansID, f.ReferansTuru })
                .HasDatabaseName("IX_StokFifo_Referans");

            // UrunKategori
            modelBuilder.Entity<Entities.UrunKategori>()
                .Property(k => k.Aktif)
                .HasDefaultValue(true);

            modelBuilder.Entity<Entities.UrunKategori>()
                .Property(k => k.Silindi)
                .HasDefaultValue(false);

            // Depo
            modelBuilder.Entity<Entities.Depo>()
                .Property(d => d.Aktif)
                .HasDefaultValue(true);

            modelBuilder.Entity<Entities.Depo>()
                .Property(d => d.Silindi)
                .HasDefaultValue(false);

            // Cari
            modelBuilder.Entity<Entities.Cari>()
                .Property(c => c.AktifMi)
                .HasDefaultValue(true);

            modelBuilder.Entity<Entities.Cari>()
                .Property(c => c.Silindi)
                .HasDefaultValue(false);

            // Fatura
            modelBuilder.Entity<Entities.Fatura>()
                .Property(f => f.Aktif)
                .HasDefaultValue(true);

            modelBuilder.Entity<Entities.Fatura>()
                .Property(f => f.Silindi)
                .HasDefaultValue(false);

            modelBuilder.Entity<Entities.Fatura>()
                .Property(f => f.ResmiMi)
                .HasDefaultValue(true);

            // Irsaliye
            modelBuilder.Entity<Entities.Irsaliye>()
                .Property(i => i.Aktif)
                .HasDefaultValue(true);

            // Kasa
            modelBuilder.Entity<Entities.Kasa>()
                .Property(k => k.Aktif)
                .HasDefaultValue(true);

            modelBuilder.Entity<Entities.Kasa>()
                .Property(k => k.Silindi)
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
                .Property(k => k.Silindi)
                .HasDefaultValue(false);
                
            // Banka
            modelBuilder.Entity<Entities.Banka>()
                .Property(b => b.Aktif)
                .HasDefaultValue(true);

            modelBuilder.Entity<Entities.Banka>()
                .Property(b => b.Silindi)
                .HasDefaultValue(false);

            // BankaHesap entity'si için konfigürasyon
            modelBuilder.Entity<Entities.BankaHesap>()
                .Property(b => b.AcilisBakiye)
                .HasDefaultValue(0);

            modelBuilder.Entity<Entities.BankaHesap>()
                .Property(b => b.GuncelBakiye)
                .HasDefaultValue(0);
                
            modelBuilder.Entity<Entities.BankaHesap>()
                .Property(b => b.ParaBirimi)
                .HasDefaultValue("TRY");
                
            // BankaHareket
            modelBuilder.Entity<Entities.BankaHareket>()
                .Property(b => b.Silindi)
                .HasDefaultValue(false);
                
            // İrsaliye için global sorgu filtresi ekle
            modelBuilder.Entity<Entities.Irsaliye>().HasQueryFilter(i => i.Aktif);
                
            // ParaBirimiIliski ilişkilerini ayarla - döviz modülü yeniden tasarlandığı için kaldırıldı

            // Döviz modülü konfigürasyonları
            
            // ParaBirimi tablosu konfigürasyonu
            modelBuilder.Entity<Entities.ParaBirimiModulu.ParaBirimi>()
                .HasIndex(p => p.Kod)
                .IsUnique()
                .HasDatabaseName("IX_ParaBirimi_Kod");
                
            modelBuilder.Entity<Entities.ParaBirimiModulu.ParaBirimi>()
                .Property(p => p.Aktif)
                .HasDefaultValue(true);
                
            modelBuilder.Entity<Entities.ParaBirimiModulu.ParaBirimi>()
                .Property(p => p.Silindi)
                .HasDefaultValue(false);
            
            // KurDegeri tablosu konfigürasyonu
            modelBuilder.Entity<Entities.ParaBirimiModulu.KurDegeri>(entity =>
            {
                entity.HasKey(e => e.KurDegeriID);
                entity.Property(e => e.KurDegeriID).ValueGeneratedOnAdd();
                entity.Property(e => e.Alis).HasColumnType("decimal(18,6)");
                entity.Property(e => e.Satis).HasColumnType("decimal(18,6)");
                
                entity.HasOne(d => d.ParaBirimi)
                    .WithMany(p => p.KurDegerleri)
                    .HasForeignKey(d => d.ParaBirimiID)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });
            
            // DovizIliski tablosu konfigürasyonu
            modelBuilder.Entity<Entities.ParaBirimiModulu.ParaBirimiIliski>()
                .HasIndex(d => new { d.KaynakParaBirimiID, d.HedefParaBirimiID })
                .IsUnique()
                .HasDatabaseName("IX_DovizIliski_KaynakParaBirimiID_HedefParaBirimiID");
                
            modelBuilder.Entity<Entities.ParaBirimiModulu.ParaBirimiIliski>()
                .Property(d => d.Aktif)
                .HasDefaultValue(true);
                
            modelBuilder.Entity<Entities.ParaBirimiModulu.ParaBirimiIliski>()
                .Property(d => d.Silindi)
                .HasDefaultValue(false);
                
            modelBuilder.Entity<Entities.ParaBirimiModulu.ParaBirimiIliski>()
                .HasOne(d => d.KaynakParaBirimi)
                .WithMany(p => p.KaynakParaBirimiIliskileri)
                .HasForeignKey(d => d.KaynakParaBirimiID)
                .OnDelete(DeleteBehavior.Restrict);
                
            modelBuilder.Entity<Entities.ParaBirimiModulu.ParaBirimiIliski>()
                .HasOne(d => d.HedefParaBirimi)
                .WithMany(p => p.HedefParaBirimiIliskileri)
                .HasForeignKey(d => d.HedefParaBirimiID)
                .OnDelete(DeleteBehavior.Restrict);
                
            // CheckConstraint ile farklı para birimleri kontrolü
            modelBuilder.Entity<Entities.ParaBirimiModulu.ParaBirimiIliski>()
                .HasCheckConstraint("CK_DovizIliski_DifferentCurrencies", "KaynakParaBirimiID <> HedefParaBirimiID");

            // Menu - MenuRol ilişkisi
            modelBuilder.Entity<MenuRol>()
                .HasKey(mr => new { mr.MenuId, mr.RolId });

            modelBuilder.Entity<MenuRol>()
                .HasOne(mr => mr.Menu)
                .WithMany(m => m.MenuRoller)
                .HasForeignKey(mr => mr.MenuId);

            // Cari entity konfigürasyonu
            modelBuilder.Entity<Cari>()
                .Property(c => c.BaslangicBakiye)
                .HasColumnType("decimal(18,2)");

            // Global query filtre uygulamasını kaldırıyoruz
            // Sadece yeni kayıt oluşturma veya listeleme ekranlarında filtreleme yapacağız
            // modelBuilder.Entity<Cari>().HasQueryFilter(x => !x.Silindi && x.AktifMi);

            modelBuilder.Entity<SistemAyar>().ToTable("SistemAyarlari");

            // Menuler tablosu konfigürasyonu
            modelBuilder.Entity<Entities.Menu>()
                .Property(m => m.Silindi)
                .HasDefaultValue(false);

            modelBuilder.Entity<Entities.Menu>()
                .Property(m => m.AktifMi)
                .HasDefaultValue(true);

            // Decimal türleri için doğru SQL veri tiplerini belirtelim
            // BankaHesap Entity
            modelBuilder.Entity<Entities.BankaHesap>()
                .Property(b => b.AcilisBakiye)
                .HasColumnType("decimal(18,2)");
                
            modelBuilder.Entity<Entities.BankaHesap>()
                .Property(b => b.GuncelBakiye)
                .HasColumnType("decimal(18,2)");
                
            // BankaHareket Entity
            modelBuilder.Entity<Entities.BankaHareket>()
                .Property(b => b.Tutar)
                .HasColumnType("decimal(18,2)");
                
            // CariHareket Entity
            modelBuilder.Entity<Entities.CariHareket>()
                .Property(c => c.Tutar)
                .HasColumnType("decimal(18,2)");
            modelBuilder.Entity<Entities.CariHareket>().HasQueryFilter(c => c.Silindi == false);
            modelBuilder.Entity<Entities.CariHareket>()
                .HasOne(ch => ch.Cari)
                .WithMany(c => c.CariHareketler)
                .HasForeignKey(ch => ch.CariID)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(true);
                
            // KurDegeri Entity
            modelBuilder.Entity<Entities.ParaBirimiModulu.KurDegeri>()
                .Property(k => k.Alis)
                .HasColumnType("decimal(18,6)");
                
            modelBuilder.Entity<Entities.ParaBirimiModulu.KurDegeri>()
                .Property(k => k.Satis)
                .HasColumnType("decimal(18,6)");
                
            // Kasa Entity
            modelBuilder.Entity<Entities.Kasa>()
                .Property(k => k.AcilisBakiye)
                .HasColumnType("decimal(18,2)");
                
            modelBuilder.Entity<Entities.Kasa>()
                .Property(k => k.GuncelBakiye)
                .HasColumnType("decimal(18,2)");
                
            // Stok Entity'leri
            modelBuilder.Entity<Entities.StokFifo>()
                .Property(s => s.Miktar)
                .HasColumnType("decimal(18,3)");
                
            modelBuilder.Entity<Entities.StokFifo>()
                .Property(s => s.KalanMiktar)
                .HasColumnType("decimal(18,3)");
                
            modelBuilder.Entity<Entities.StokFifo>()
                .Property(s => s.BirimFiyat)
                .HasColumnType("decimal(18,2)");
                
            modelBuilder.Entity<Entities.StokFifo>()
                .Property(s => s.TLBirimFiyat)
                .HasColumnType("decimal(18,2)");
                
            modelBuilder.Entity<Entities.StokFifo>()
                .Property(s => s.USDBirimFiyat)
                .HasColumnType("decimal(18,2)");
                
            modelBuilder.Entity<Entities.StokFifo>()
                .Property(s => s.UZSBirimFiyat)
                .HasColumnType("decimal(18,2)");
                
            modelBuilder.Entity<Entities.StokFifo>()
                .Property(s => s.DovizKuru)
                .HasColumnType("decimal(18,6)");
                
            // StokHareket Entity
            modelBuilder.Entity<Entities.StokHareket>()
                .Property(s => s.Miktar)
                .HasColumnType("decimal(18,3)");
                
            modelBuilder.Entity<Entities.StokHareket>()
                .Property(s => s.BirimFiyat)
                .HasColumnType("decimal(18,2)");
                
            // Urun - StokHareket ilişkisi için filtreler
            modelBuilder.Entity<Entities.Urun>().HasQueryFilter(u => u.Silindi == false && u.Aktif == true);
            
            // Urun decimal alanlarını yapılandır
            modelBuilder.Entity<Entities.Urun>()
                .Property(u => u.DovizliListeFiyati)
                .HasColumnType("decimal(18,2)");
                
            modelBuilder.Entity<Entities.Urun>()
                .Property(u => u.DovizliMaliyetFiyati)
                .HasColumnType("decimal(18,2)");
                
            modelBuilder.Entity<Entities.Urun>()
                .Property(u => u.DovizliSatisFiyati)
                .HasColumnType("decimal(18,2)");
                
            modelBuilder.Entity<Entities.StokHareket>().HasQueryFilter(sh => sh.Silindi == false);
                
            // UrunFiyat Entity
            modelBuilder.Entity<Entities.UrunFiyat>()
                .Property(u => u.Fiyat)
                .HasColumnType("decimal(18,2)");
                
            // FaturaDetay Entity
            modelBuilder.Entity<Entities.FaturaDetay>()
                .Property(f => f.BirimFiyat)
                .HasColumnType("decimal(18,2)");
                
            modelBuilder.Entity<Entities.FaturaDetay>()
                .Property(f => f.Miktar)
                .HasColumnType("decimal(18,3)");
                
            modelBuilder.Entity<Entities.FaturaDetay>()
                .Property(f => f.Tutar)
                .HasColumnType("decimal(18,2)");
                
            modelBuilder.Entity<Entities.FaturaDetay>()
                .Property(f => f.IndirimOrani)
                .HasColumnType("decimal(18,2)");
                
            modelBuilder.Entity<Entities.FaturaDetay>()
                .Property(f => f.IndirimTutari)
                .HasColumnType("decimal(18,2)");
                
            modelBuilder.Entity<Entities.FaturaDetay>()
                .Property(f => f.KdvOrani)
                .HasColumnType("decimal(18,2)");
                
            modelBuilder.Entity<Entities.FaturaDetay>()
                .Property(f => f.KdvTutari)
                .HasColumnType("decimal(18,2)");
                
            modelBuilder.Entity<Entities.FaturaDetay>()
                .Property(f => f.NetTutar)
                .HasColumnType("decimal(18,2)");
                
            modelBuilder.Entity<Entities.FaturaDetay>()
                .Property(f => f.SatirKdvToplam)
                .HasColumnType("decimal(18,2)");
                
            modelBuilder.Entity<Entities.FaturaDetay>()
                .Property(f => f.SatirToplam)
                .HasColumnType("decimal(18,2)");
                
            // IrsaliyeDetay Entity
            modelBuilder.Entity<Entities.IrsaliyeDetay>()
                .Property(i => i.Miktar)
                .HasColumnType("decimal(18,3)");
                
            modelBuilder.Entity<Entities.IrsaliyeDetay>()
                .Property(i => i.BirimFiyat)
                .HasColumnType("decimal(18,2)");
                
            modelBuilder.Entity<Entities.IrsaliyeDetay>()
                .Property(i => i.KdvOrani)
                .HasColumnType("decimal(18,2)");
                
            modelBuilder.Entity<Entities.IrsaliyeDetay>()
                .Property(i => i.IndirimOrani)
                .HasColumnType("decimal(18,2)");
                
            modelBuilder.Entity<Entities.IrsaliyeDetay>()
                .Property(i => i.SatirToplam)
                .HasColumnType("decimal(18,2)");
                
            modelBuilder.Entity<Entities.IrsaliyeDetay>()
                .Property(i => i.SatirKdvToplam)
                .HasColumnType("decimal(18,2)");

            // SistemLog tablosu için SQL Server uyumlu yapılandırma
            modelBuilder.Entity<Entities.SistemLog>(entity =>
            {
                entity.ToTable("SistemLoglar");
                // SQL Server için identity özelliğini ayarla
                entity.Property(e => e.Id)
                    .HasColumnType("int")
                    .UseIdentityColumn();
            });

            // Global query filtreleri - İlişkili entity'ler için uyumlu şekilde ayarlanıyor
            // Cari - CariHareket ilişkisi için filtreler
            modelBuilder.Entity<Entities.Cari>().HasQueryFilter(c => c.Silindi == false && c.AktifMi == true);

            // Fatura - FaturaDetay ilişkisi için filtreler
            modelBuilder.Entity<Entities.Fatura>().HasQueryFilter(f => f.Silindi == false && f.Aktif == true);
            modelBuilder.Entity<Entities.FaturaDetay>().HasQueryFilter(fd => fd.Silindi == false);

            // Fatura - FaturaOdeme ilişkisi için filtreler
            modelBuilder.Entity<Entities.FaturaOdeme>().HasQueryFilter(fo => fo.Silindi == false);

            // Irsaliye - IrsaliyeDetay ilişkisi için filtreler
            modelBuilder.Entity<Entities.Irsaliye>().HasQueryFilter(i => i.Silindi == false && i.Aktif == true);
            modelBuilder.Entity<Entities.IrsaliyeDetay>().HasQueryFilter(id => id.Silindi == false);

            // Menu - MenuRol ilişkisi için filtreler
            modelBuilder.Entity<Entities.Menu>().HasQueryFilter(m => m.Silindi == false && m.AktifMi == true);
            modelBuilder.Entity<Entities.MenuRol>().HasQueryFilter(mr => !mr.Menu.Silindi && mr.Menu.AktifMi);

            // Urun - StokHareket ilişkisi için filtreler
            modelBuilder.Entity<Entities.Urun>().HasQueryFilter(u => u.Silindi == false && u.Aktif == true);
            modelBuilder.Entity<Entities.StokHareket>().HasQueryFilter(sh => sh.Silindi == false);

            // Diğer entity'ler için filtreler
            modelBuilder.Entity<Entities.BankaHareket>().HasQueryFilter(b => b.Silindi == false);
            modelBuilder.Entity<Entities.Banka>().HasQueryFilter(b => b.Silindi == false && b.Aktif == true);
            modelBuilder.Entity<Entities.Depo>().HasQueryFilter(d => d.Silindi == false && d.Aktif == true);
            modelBuilder.Entity<Entities.Kasa>().HasQueryFilter(k => k.Silindi == false && k.Aktif == true);
            modelBuilder.Entity<Entities.KasaHareket>().HasQueryFilter(k => k.Silindi == false);
            modelBuilder.Entity<Entities.StokFifo>().HasQueryFilter(s => s.Silindi == false && s.Aktif == true && s.Iptal == false);
            modelBuilder.Entity<Entities.StokCikisDetay>().HasQueryFilter(s => !s.Iptal && (!s.StokFifoID.HasValue || (s.StokFifo != null && !s.StokFifo.Silindi && s.StokFifo.Aktif && !s.StokFifo.Iptal)));
            modelBuilder.Entity<Entities.UrunKategori>().HasQueryFilter(k => k.Silindi == false && k.Aktif == true);
            modelBuilder.Entity<Entities.UrunFiyat>().HasQueryFilter(uf => uf.Silindi == false);

            // UrunBirim
            modelBuilder.Entity<Entities.Birim>(entity =>
            {
                entity.Property(b => b.OlusturanKullaniciID)
                    .HasColumnType("nvarchar(450)")
                    .IsRequired();

                entity.HasQueryFilter(b => !b.Silindi && b.Aktif);
            });

            // Banka ile BankaHesap arasındaki ilişkiler
            modelBuilder.Entity<Entities.Banka>()
                .HasMany(b => b.BankaHesaplari)
                .WithOne(h => h.Banka)
                .HasForeignKey(h => h.BankaID)
                .OnDelete(DeleteBehavior.NoAction);

            // BankaHesap ile BankaHesapHareket arasındaki ilişkiler
            modelBuilder.Entity<Entities.BankaHesap>()
                .HasMany(b => b.BankaHesapHareketleri)
                .WithOne(h => h.BankaHesap)
                .HasForeignKey(h => h.BankaHesapID)
                .OnDelete(DeleteBehavior.NoAction);

            // BankaHesap entity konfigürasyonu
            modelBuilder.Entity<Entities.BankaHesap>()
                .Property(b => b.Aktif)
                .HasDefaultValue(true);

            modelBuilder.Entity<Entities.BankaHesap>()
                .Property(b => b.Silindi)
                .HasDefaultValue(false);

            modelBuilder.Entity<Entities.BankaHesap>()
                .Property(b => b.AcilisBakiye)
                .HasColumnType("decimal(18,2)")
                .HasDefaultValue(0);

            modelBuilder.Entity<Entities.BankaHesap>()
                .Property(b => b.GuncelBakiye)
                .HasColumnType("decimal(18,2)")
                .HasDefaultValue(0);

            modelBuilder.Entity<Entities.BankaHesap>()
                .Property(b => b.ParaBirimi)
                .HasDefaultValue("TRY");

            // Sorgu filtreleri
            modelBuilder.Entity<Entities.BankaHesap>()
                .HasQueryFilter(b => !b.Silindi);

            // BankaHesapHareket entity konfigürasyonu
            modelBuilder.Entity<Entities.BankaHesapHareket>()
                .Property(b => b.Silindi)
                .HasDefaultValue(false);

            modelBuilder.Entity<Entities.BankaHesapHareket>()
                .Property(b => b.Tutar)
                .HasColumnType("decimal(18,2)");

            // Sorgu filtreleri
            modelBuilder.Entity<Entities.BankaHesapHareket>()
                .HasQueryFilter(b => !b.Silindi);

            // Fatura - Sozlesme ilişkisini yapılandır - tek yönlü
            modelBuilder.Entity<Entities.Fatura>()
                .HasOne(f => f.Sozlesme)
                .WithMany(s => s.Faturalar)
                .HasForeignKey(f => f.SozlesmeID)
                .OnDelete(DeleteBehavior.SetNull);
                
            // FaturaAklamaKuyruk konfigürasyonu
            modelBuilder.Entity<FaturaAklamaKuyruk>()
                .HasOne(a => a.FaturaDetay)
                .WithMany()
                .HasForeignKey(a => a.FaturaKalemID)
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired(false);
                
            modelBuilder.Entity<FaturaAklamaKuyruk>()
                .HasOne(a => a.Urun)
                .WithMany()
                .HasForeignKey(a => a.UrunID)
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired(false);
                
            modelBuilder.Entity<FaturaAklamaKuyruk>()
                .HasOne(a => a.Sozlesme)
                .WithMany(s => s.AklamaKayitlari)
                .HasForeignKey(a => a.SozlesmeID)
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired(false);
                
            // FaturaAklamaKuyruk için diğer yapılandırmalar
            modelBuilder.Entity<FaturaAklamaKuyruk>()
                .Property(a => a.Silindi)
                .HasDefaultValue(false);
                
            modelBuilder.Entity<FaturaAklamaKuyruk>()
                .Property(a => a.Durum)
                .HasDefaultValue(AklamaDurumu.Bekliyor);
                
            modelBuilder.Entity<FaturaAklamaKuyruk>()
                .Property(a => a.DovizKuru)
                .HasDefaultValue(1);
                
            modelBuilder.Entity<FaturaAklamaKuyruk>()
                .Property(a => a.ParaBirimi)
                .HasDefaultValue("TL");
                
            // Cari - Sozlesme ilişkisi için IsRequired(false) ayarla
            modelBuilder.Entity<Entities.Sozlesme>()
                .HasOne(s => s.Cari)
                .WithMany()
                .HasForeignKey(s => s.CariID)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired(false);

            // StokCikisDetay - StokFifo ilişkisini opsiyonel hale getirme
            modelBuilder.Entity<Entities.StokCikisDetay>()
                .HasOne(s => s.StokFifo)
                .WithMany()
                .HasForeignKey(s => s.StokFifoID)
                .OnDelete(DeleteBehavior.SetNull)
                .IsRequired(false);
        }

        // GetDbContextOptions metodunu ekle
        public DbContextOptions<ApplicationDbContext> GetDbContextOptions()
        {
            return (DbContextOptions<ApplicationDbContext>)this.GetType()
                .GetProperty("ContextOptions", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.GetValue(this);
        }
    }
} 
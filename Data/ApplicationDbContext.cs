using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
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

        public DbSet<Urun> Urunler { get; set; }
        public DbSet<UrunFiyat> UrunFiyatlari { get; set; }
        public DbSet<FiyatTipi> FiyatTipleri { get; set; }
        public DbSet<UrunKategori> UrunKategorileri { get; set; }
        public DbSet<StokHareket> StokHareketleri { get; set; }
        public DbSet<StokFifo> StokFifo { get; set; }
        public DbSet<Depo> Depolar { get; set; }
        public DbSet<Cari> Cariler { get; set; }
        public DbSet<CariHareket> CariHareketler { get; set; }
        public DbSet<Fatura> Faturalar { get; set; }
        public DbSet<FaturaDetay> FaturaDetaylari { get; set; }
        public DbSet<FaturaOdeme> FaturaOdemeleri { get; set; }
        public DbSet<Birim> Birimler { get; set; }
        public DbSet<OdemeTuru> OdemeTurleri { get; set; }
        public DbSet<FaturaTuru> FaturaTurleri { get; set; }
        public DbSet<Irsaliye> Irsaliyeler { get; set; }
        public DbSet<IrsaliyeDetay> IrsaliyeDetaylari { get; set; }
        public DbSet<IrsaliyeTuru> IrsaliyeTurleri { get; set; }
        public DbSet<Kasa> Kasalar { get; set; }
        public DbSet<KasaHareket> KasaHareketleri { get; set; }
        public DbSet<Banka> Bankalar { get; set; }
        public DbSet<BankaHareket> BankaHareketleri { get; set; }
        public DbSet<MuhasebeStokWebApp.Data.Entities.SistemAyarlari> SistemAyarlari { get; set; }
        public DbSet<MuhasebeStokWebApp.Data.Entities.SistemLog> SistemLoglar { get; set; }
        public DbSet<ParaBirimi> ParaBirimleri { get; set; }
        public DbSet<KurDegeri> KurDegerleri { get; set; }
        public DbSet<DovizIliski> DovizIliskileri { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Entity - Tablo eşleştirmeleri
            modelBuilder.Entity<ParaBirimi>(entity =>
            {
                entity.ToTable("Dovizler");
                entity.Property(p => p.ParaBirimiID).HasColumnName("DovizID");
                entity.Property(p => p.Kod).HasColumnName("DovizKodu");
                entity.Property(p => p.Ad).HasColumnName("DovizAdi");
                
                // Varsayılan değerler
                entity.Property(p => p.Aktif).HasDefaultValue(true);
                entity.Property(p => p.SoftDelete).HasDefaultValue(false);
            });
            
            modelBuilder.Entity<KurDegeri>(entity =>
            {
                entity.ToTable("DovizKurlari");
                entity.Property(k => k.KurDegeriID).HasColumnName("DovizKuruID");
                entity.Property(k => k.AlisDegeri).HasColumnName("AlisFiyati").HasColumnType("decimal(18,6)");
                entity.Property(k => k.SatisDegeri).HasColumnName("SatisFiyati").HasColumnType("decimal(18,6)");
                
                // Varsayılan değerler
                entity.Property(k => k.Aktif).HasDefaultValue(true);
                entity.Property(k => k.SoftDelete).HasDefaultValue(false);
                entity.Property(k => k.Kaynak).HasDefaultValue("Manuel").IsRequired();
                
                // İlişkiler
                entity.HasOne(k => k.ParaBirimi)
                    .WithMany()
                    .HasForeignKey(k => k.ParaBirimiID)
                    .OnDelete(DeleteBehavior.Restrict);
            });
            
            // Döviz İlişkileri konfigürasyonu
            modelBuilder.Entity<DovizIliski>()
                .ToTable("DovizIliskileri")
                .HasOne(di => di.KaynakParaBirimi)
                .WithMany()
                .HasForeignKey(di => di.KaynakParaBirimiID)
                .OnDelete(DeleteBehavior.Restrict);
            
            modelBuilder.Entity<DovizIliski>()
                .HasOne(di => di.HedefParaBirimi)
                .WithMany()
                .HasForeignKey(di => di.HedefParaBirimiID)
                .OnDelete(DeleteBehavior.Restrict);
            
            // KurAyarlari için SistemAyarlari tablosunu kullanıyoruz
            // Ama SistemAyarlari ayrı bir entity olduğu için burada eşleştirme yapmıyoruz
            
            // Urun
            modelBuilder.Entity<Urun>()
                .Property(u => u.StokMiktar)
                .HasDefaultValue(0);

            modelBuilder.Entity<Urun>()
                .Property(u => u.Aktif)
                .HasDefaultValue(true);

            modelBuilder.Entity<Urun>()
                .Property(u => u.SoftDelete)
                .HasDefaultValue(false);
                
            // StokFifo indeksleri
            modelBuilder.Entity<StokFifo>()
                .HasIndex(f => f.UrunID)
                .HasDatabaseName("IX_StokFifo_UrunID");
                
            modelBuilder.Entity<StokFifo>()
                .HasIndex(f => f.GirisTarihi)
                .HasDatabaseName("IX_StokFifo_GirisTarihi");
                
            modelBuilder.Entity<StokFifo>()
                .HasIndex(f => new { f.UrunID, f.KalanMiktar, f.Aktif, f.SoftDelete, f.Iptal })
                .HasDatabaseName("IX_StokFifo_StokSorgu");
                
            modelBuilder.Entity<StokFifo>()
                .HasIndex(f => new { f.ReferansID, f.ReferansTuru })
                .HasDatabaseName("IX_StokFifo_Referans");
                
            // UrunKategori
            modelBuilder.Entity<UrunKategori>()
                .Property(k => k.Aktif)
                .HasDefaultValue(true);

            modelBuilder.Entity<UrunKategori>()
                .Property(k => k.SoftDelete)
                .HasDefaultValue(false);

            // Depo
            modelBuilder.Entity<Depo>()
                .Property(d => d.Aktif)
                .HasDefaultValue(true);

            modelBuilder.Entity<Depo>()
                .Property(d => d.SoftDelete)
                .HasDefaultValue(false);

            // Cari
            modelBuilder.Entity<Cari>()
                .Property(c => c.Aktif)
                .HasDefaultValue(true);

            modelBuilder.Entity<Cari>()
                .Property(c => c.SoftDelete)
                .HasDefaultValue(false);

            // Fatura
            modelBuilder.Entity<Fatura>()
                .Property(f => f.Aktif)
                .HasDefaultValue(true);

            modelBuilder.Entity<Fatura>()
                .Property(f => f.SoftDelete)
                .HasDefaultValue(false);

            modelBuilder.Entity<Fatura>()
                .Property(f => f.Resmi)
                .HasDefaultValue(true);

            // Irsaliye
            modelBuilder.Entity<Irsaliye>()
                .Property(i => i.SoftDelete)
                .HasDefaultValue(false);

            modelBuilder.Entity<Irsaliye>()
                .Property(i => i.Resmi)
                .HasDefaultValue(true);
                
            // Kasa
            modelBuilder.Entity<Kasa>()
                .Property(k => k.Aktif)
                .HasDefaultValue(true);

            modelBuilder.Entity<Kasa>()
                .Property(k => k.SoftDelete)
                .HasDefaultValue(false);

            modelBuilder.Entity<Kasa>()
                .Property(k => k.AcilisBakiye)
                .HasDefaultValue(0);

            modelBuilder.Entity<Kasa>()
                .Property(k => k.GuncelBakiye)
                .HasDefaultValue(0);
                
            // Kasa ile KasaHareket arasındaki ilişkiler
            modelBuilder.Entity<Kasa>()
                .HasMany(k => k.KasaHareketleri)
                .WithOne(h => h.Kasa)
                .HasForeignKey(h => h.KasaID)
                .OnDelete(DeleteBehavior.NoAction);
            
            // Kasa'nın HedefKasa ilişkisi
            modelBuilder.Entity<KasaHareket>()
                .HasOne(h => h.HedefKasa)
                .WithMany()
                .HasForeignKey(h => h.HedefKasaID)
                .OnDelete(DeleteBehavior.NoAction);
                
            // KasaHareket
            modelBuilder.Entity<KasaHareket>()
                .Property(k => k.SoftDelete)
                .HasDefaultValue(false);
                
            // Banka
            modelBuilder.Entity<Banka>()
                .Property(b => b.Aktif)
                .HasDefaultValue(true);

            modelBuilder.Entity<Banka>()
                .Property(b => b.SoftDelete)
                .HasDefaultValue(false);

            modelBuilder.Entity<Banka>()
                .Property(b => b.AcilisBakiye)
                .HasDefaultValue(0);

            modelBuilder.Entity<Banka>()
                .Property(b => b.GuncelBakiye)
                .HasDefaultValue(0);
                
            modelBuilder.Entity<Banka>()
                .Property(b => b.ParaBirimi)
                .HasDefaultValue("TRY");
                
            // BankaHareket
            modelBuilder.Entity<BankaHareket>()
                .Property(b => b.SoftDelete)
                .HasDefaultValue(false);

            // SistemAyarlari
            modelBuilder.Entity<MuhasebeStokWebApp.Data.Entities.SistemAyarlari>(entity => 
            {
                entity.Property(s => s.AnaDovizKodu).IsRequired().HasDefaultValue("USD");
                entity.Property(s => s.IkinciDovizKodu).HasDefaultValue("UZS");
                entity.Property(s => s.UcuncuDovizKodu).HasDefaultValue("TRY");
                entity.Property(s => s.SirketAdi).IsRequired().HasDefaultValue("Şirket");
                entity.Property(s => s.SirketAdresi).HasDefaultValue(string.Empty);
                entity.Property(s => s.SirketTelefon).HasDefaultValue(string.Empty);
                entity.Property(s => s.SirketEmail).HasDefaultValue(string.Empty);
                entity.Property(s => s.SirketVergiNo).HasDefaultValue(string.Empty);
                entity.Property(s => s.SirketVergiDairesi).HasDefaultValue(string.Empty);
                entity.Property(s => s.AktifParaBirimleri).HasDefaultValue("USD,EUR,TRY,UZS,GBP");
                entity.Property(s => s.Aktif).HasDefaultValue(true);
                entity.Property(s => s.SoftDelete).HasDefaultValue(false);

                // İlişkiler
                entity.HasOne(s => s.AnaParaBirimi)
                    .WithMany()
                    .HasForeignKey(s => s.AnaParaBirimiID)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(s => s.IkinciParaBirimi)
                    .WithMany()
                    .HasForeignKey(s => s.IkinciParaBirimiID)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(s => s.UcuncuParaBirimi)
                    .WithMany()
                    .HasForeignKey(s => s.UcuncuParaBirimiID)
                    .OnDelete(DeleteBehavior.NoAction);
            });
        }
    }
} 
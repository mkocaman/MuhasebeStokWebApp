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

    public virtual DbSet<AspNetRole> AspNetRoles { get; set; }

    public virtual DbSet<AspNetRoleClaim> AspNetRoleClaims { get; set; }

    public virtual DbSet<AspNetUser> AspNetUsers { get; set; }

    public virtual DbSet<AspNetUserClaim> AspNetUserClaims { get; set; }

    public virtual DbSet<AspNetUserLogin> AspNetUserLogins { get; set; }

    public virtual DbSet<AspNetUserToken> AspNetUserTokens { get; set; }

    public virtual DbSet<BankaHareketleri> BankaHareketleris { get; set; }

    public virtual DbSet<BankaHesapHareketleri> BankaHesapHareketleris { get; set; }

    public virtual DbSet<BankaHesaplari> BankaHesaplaris { get; set; }

    public virtual DbSet<Bankalar> Bankalars { get; set; }

    public virtual DbSet<Birimler> Birimlers { get; set; }

    public virtual DbSet<CariHareketler> CariHareketlers { get; set; }

    public virtual DbSet<Cariler> Carilers { get; set; }

    public virtual DbSet<Depolar> Depolars { get; set; }

    public virtual DbSet<FaturaAklamaKuyrugu> FaturaAklamaKuyrugus { get; set; }

    public virtual DbSet<FaturaDetaylari> FaturaDetaylaris { get; set; }

    public virtual DbSet<FaturaOdemeleri> FaturaOdemeleris { get; set; }

    public virtual DbSet<FaturaTurleri> FaturaTurleris { get; set; }

    public virtual DbSet<Faturalar> Faturalars { get; set; }

    public virtual DbSet<FiyatTipleri> FiyatTipleris { get; set; }

    public virtual DbSet<GenelSistemAyarlari> GenelSistemAyarlaris { get; set; }

    public virtual DbSet<IrsaliyeDetaylari> IrsaliyeDetaylaris { get; set; }

    public virtual DbSet<IrsaliyeTurleri> IrsaliyeTurleris { get; set; }

    public virtual DbSet<Irsaliyeler> Irsaliyelers { get; set; }

    public virtual DbSet<KasaHareketleri> KasaHareketleris { get; set; }

    public virtual DbSet<Kasalar> Kasalars { get; set; }

    public virtual DbSet<KurDegerleri> KurDegerleris { get; set; }

    public virtual DbSet<KurMarjlari> KurMarjlaris { get; set; }

    public virtual DbSet<MenuRoller> MenuRollers { get; set; }

    public virtual DbSet<Menuler> Menulers { get; set; }

    public virtual DbSet<OdemeTurleri> OdemeTurleris { get; set; }

    public virtual DbSet<ParaBirimiIliskileri> ParaBirimiIliskileris { get; set; }

    public virtual DbSet<ParaBirimleri> ParaBirimleris { get; set; }

    public virtual DbSet<SistemAyarlari> SistemAyarlaris { get; set; }

    public virtual DbSet<SistemLoglar> SistemLoglars { get; set; }

    public virtual DbSet<Sozlesmeler> Sozlesmelers { get; set; }

    public virtual DbSet<StokCikisDetaylari> StokCikisDetaylaris { get; set; }

    public virtual DbSet<StokFifoKayitlari> StokFifoKayitlaris { get; set; }

    public virtual DbSet<StokHareketleri> StokHareketleris { get; set; }

    public virtual DbSet<UrunFiyatlari> UrunFiyatlaris { get; set; }

    public virtual DbSet<UrunKategorileri> UrunKategorileris { get; set; }

    public virtual DbSet<Urunler> Urunlers { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=localhost;Database=MuhasebeStokDB;User Id=sa;Password=Test1234_;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AspNetRole>(entity =>
        {
            entity.HasIndex(e => e.NormalizedName, "RoleNameIndex")
                .IsUnique()
                .HasFilter("([NormalizedName] IS NOT NULL)");

            entity.Property(e => e.Id)
                .HasMaxLength(128)
                .IsUnicode(false);
            entity.Property(e => e.Name).HasMaxLength(256);
            entity.Property(e => e.NormalizedName).HasMaxLength(256);
        });

        modelBuilder.Entity<AspNetRoleClaim>(entity =>
        {
            entity.HasIndex(e => e.RoleId, "IX_AspNetRoleClaims_RoleId");

            entity.Property(e => e.RoleId)
                .HasMaxLength(128)
                .IsUnicode(false);

            entity.HasOne(d => d.Role).WithMany(p => p.AspNetRoleClaims).HasForeignKey(d => d.RoleId);
        });

        modelBuilder.Entity<AspNetUser>(entity =>
        {
            entity.HasIndex(e => e.NormalizedEmail, "EmailIndex");

            entity.HasIndex(e => e.NormalizedUserName, "UserNameIndex")
                .IsUnique()
                .HasFilter("([NormalizedUserName] IS NOT NULL)");

            entity.Property(e => e.Id)
                .HasMaxLength(128)
                .IsUnicode(false);
            entity.Property(e => e.Adres).HasMaxLength(200);
            entity.Property(e => e.Email).HasMaxLength(256);
            entity.Property(e => e.NormalizedEmail).HasMaxLength(256);
            entity.Property(e => e.NormalizedUserName).HasMaxLength(256);
            entity.Property(e => e.UserName).HasMaxLength(256);

            entity.HasMany(d => d.Roles).WithMany(p => p.Users)
                .UsingEntity<Dictionary<string, object>>(
                    "AspNetUserRole",
                    r => r.HasOne<AspNetRole>().WithMany().HasForeignKey("RoleId"),
                    l => l.HasOne<AspNetUser>().WithMany().HasForeignKey("UserId"),
                    j =>
                    {
                        j.HasKey("UserId", "RoleId");
                        j.ToTable("AspNetUserRoles");
                        j.HasIndex(new[] { "RoleId" }, "IX_AspNetUserRoles_RoleId");
                        j.IndexerProperty<string>("UserId")
                            .HasMaxLength(128)
                            .IsUnicode(false);
                        j.IndexerProperty<string>("RoleId")
                            .HasMaxLength(128)
                            .IsUnicode(false);
                    });
        });

        modelBuilder.Entity<AspNetUserClaim>(entity =>
        {
            entity.HasIndex(e => e.UserId, "IX_AspNetUserClaims_UserId");

            entity.Property(e => e.UserId)
                .HasMaxLength(128)
                .IsUnicode(false);

            entity.HasOne(d => d.User).WithMany(p => p.AspNetUserClaims).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<AspNetUserLogin>(entity =>
        {
            entity.HasKey(e => new { e.LoginProvider, e.ProviderKey });

            entity.HasIndex(e => e.UserId, "IX_AspNetUserLogins_UserId");

            entity.Property(e => e.UserId)
                .HasMaxLength(128)
                .IsUnicode(false);

            entity.HasOne(d => d.User).WithMany(p => p.AspNetUserLogins).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<AspNetUserToken>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.LoginProvider, e.Name });

            entity.Property(e => e.UserId)
                .HasMaxLength(128)
                .IsUnicode(false);

            entity.HasOne(d => d.User).WithMany(p => p.AspNetUserTokens).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<BankaHareketleri>(entity =>
        {
            entity.HasKey(e => e.BankaHareketId);

            entity.ToTable("BankaHareketleri");

            entity.HasIndex(e => e.BankaId, "IX_BankaHareketleri_BankaID");

            entity.HasIndex(e => e.CariId, "IX_BankaHareketleri_CariID");

            entity.HasIndex(e => e.HedefKasaId, "IX_BankaHareketleri_HedefKasaID");

            entity.HasIndex(e => e.KaynakKasaId, "IX_BankaHareketleri_KaynakKasaID");

            entity.Property(e => e.BankaHareketId)
                .ValueGeneratedNever()
                .HasColumnName("BankaHareketID");
            entity.Property(e => e.Aciklama)
                .HasMaxLength(500)
                .HasDefaultValue("");
            entity.Property(e => e.BankaId).HasColumnName("BankaID");
            entity.Property(e => e.CariId).HasColumnName("CariID");
            entity.Property(e => e.DekontNo)
                .HasMaxLength(50)
                .HasDefaultValue("");
            entity.Property(e => e.HareketTuru).HasMaxLength(50);
            entity.Property(e => e.HedefKasaId).HasColumnName("HedefKasaID");
            entity.Property(e => e.IslemYapanKullaniciId).HasColumnName("IslemYapanKullaniciID");
            entity.Property(e => e.KarsiBankaAdi)
                .HasMaxLength(50)
                .HasDefaultValue("");
            entity.Property(e => e.KarsiIban)
                .HasMaxLength(50)
                .HasDefaultValue("")
                .HasColumnName("KarsiIBAN");
            entity.Property(e => e.KarsiUnvan)
                .HasMaxLength(200)
                .HasDefaultValue("");
            entity.Property(e => e.KaynakKasaId).HasColumnName("KaynakKasaID");
            entity.Property(e => e.ReferansId).HasColumnName("ReferansID");
            entity.Property(e => e.ReferansNo)
                .HasMaxLength(50)
                .HasDefaultValue("");
            entity.Property(e => e.ReferansTuru)
                .HasMaxLength(50)
                .HasDefaultValue("");
            entity.Property(e => e.SonGuncelleyenKullaniciId).HasColumnName("SonGuncelleyenKullaniciID");
            entity.Property(e => e.TransferId).HasColumnName("TransferID");
            entity.Property(e => e.Tutar).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.Banka).WithMany(p => p.BankaHareketleris).HasForeignKey(d => d.BankaId);

            entity.HasOne(d => d.Cari).WithMany(p => p.BankaHareketleris).HasForeignKey(d => d.CariId);

            entity.HasOne(d => d.HedefKasa).WithMany(p => p.BankaHareketleriHedefKasas).HasForeignKey(d => d.HedefKasaId);

            entity.HasOne(d => d.KaynakKasa).WithMany(p => p.BankaHareketleriKaynakKasas).HasForeignKey(d => d.KaynakKasaId);
        });

        modelBuilder.Entity<BankaHesapHareketleri>(entity =>
        {
            entity.HasKey(e => e.BankaHesapHareketId);

            entity.ToTable("BankaHesapHareketleri");

            entity.HasIndex(e => e.BankaHesapId, "IX_BankaHesapHareketleri_BankaHesapID");

            entity.HasIndex(e => e.BankaId, "IX_BankaHesapHareketleri_BankaID");

            entity.HasIndex(e => e.CariId, "IX_BankaHesapHareketleri_CariID");

            entity.HasIndex(e => e.HedefKasaId, "IX_BankaHesapHareketleri_HedefKasaID");

            entity.HasIndex(e => e.KaynakKasaId, "IX_BankaHesapHareketleri_KaynakKasaID");

            entity.Property(e => e.BankaHesapHareketId)
                .ValueGeneratedNever()
                .HasColumnName("BankaHesapHareketID");
            entity.Property(e => e.Aciklama)
                .HasMaxLength(500)
                .HasDefaultValue("");
            entity.Property(e => e.BankaHesapId).HasColumnName("BankaHesapID");
            entity.Property(e => e.BankaId).HasColumnName("BankaID");
            entity.Property(e => e.CariId).HasColumnName("CariID");
            entity.Property(e => e.DekontNo)
                .HasMaxLength(50)
                .HasDefaultValue("");
            entity.Property(e => e.HareketTuru).HasMaxLength(50);
            entity.Property(e => e.HedefKasaId).HasColumnName("HedefKasaID");
            entity.Property(e => e.IslemYapanKullaniciId).HasColumnName("IslemYapanKullaniciID");
            entity.Property(e => e.KarsiBankaAdi).HasMaxLength(50);
            entity.Property(e => e.KarsiIban)
                .HasMaxLength(50)
                .HasColumnName("KarsiIBAN");
            entity.Property(e => e.KarsiParaBirimi)
                .HasMaxLength(10)
                .HasDefaultValue("");
            entity.Property(e => e.KarsiUnvan).HasMaxLength(200);
            entity.Property(e => e.KaynakKasaId).HasColumnName("KaynakKasaID");
            entity.Property(e => e.ReferansId).HasColumnName("ReferansID");
            entity.Property(e => e.ReferansNo)
                .HasMaxLength(50)
                .HasDefaultValue("");
            entity.Property(e => e.ReferansTuru)
                .HasMaxLength(50)
                .HasDefaultValue("");
            entity.Property(e => e.SonGuncelleyenKullaniciId).HasColumnName("SonGuncelleyenKullaniciID");
            entity.Property(e => e.TransferId).HasColumnName("TransferID");
            entity.Property(e => e.Tutar).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.BankaHesap).WithMany(p => p.BankaHesapHareketleris)
                .HasForeignKey(d => d.BankaHesapId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Banka).WithMany(p => p.BankaHesapHareketleris).HasForeignKey(d => d.BankaId);

            entity.HasOne(d => d.Cari).WithMany(p => p.BankaHesapHareketleris).HasForeignKey(d => d.CariId);

            entity.HasOne(d => d.HedefKasa).WithMany(p => p.BankaHesapHareketleriHedefKasas).HasForeignKey(d => d.HedefKasaId);

            entity.HasOne(d => d.KaynakKasa).WithMany(p => p.BankaHesapHareketleriKaynakKasas).HasForeignKey(d => d.KaynakKasaId);
        });

        modelBuilder.Entity<BankaHesaplari>(entity =>
        {
            entity.HasKey(e => e.BankaHesapId);

            entity.ToTable("BankaHesaplari");

            entity.HasIndex(e => e.BankaId, "IX_BankaHesaplari_BankaID");

            entity.Property(e => e.BankaHesapId)
                .ValueGeneratedNever()
                .HasColumnName("BankaHesapID");
            entity.Property(e => e.Aciklama)
                .HasMaxLength(500)
                .HasDefaultValue("");
            entity.Property(e => e.AcilisBakiye).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Aktif).HasDefaultValue(true);
            entity.Property(e => e.BankaId).HasColumnName("BankaID");
            entity.Property(e => e.GuncelBakiye).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.HesapAdi).HasMaxLength(100);
            entity.Property(e => e.HesapNo)
                .HasMaxLength(50)
                .HasDefaultValue("");
            entity.Property(e => e.Iban)
                .HasMaxLength(50)
                .HasDefaultValue("")
                .HasColumnName("IBAN");
            entity.Property(e => e.OlusturanKullaniciId).HasColumnName("OlusturanKullaniciID");
            entity.Property(e => e.ParaBirimi)
                .HasMaxLength(10)
                .HasDefaultValue("TRY");
            entity.Property(e => e.SonGuncelleyenKullaniciId).HasColumnName("SonGuncelleyenKullaniciID");
            entity.Property(e => e.SubeAdi)
                .HasMaxLength(100)
                .HasDefaultValue("");
            entity.Property(e => e.SubeKodu)
                .HasMaxLength(50)
                .HasDefaultValue("");
            entity.Property(e => e.YetkiliKullaniciId).HasColumnName("YetkiliKullaniciID");

            entity.HasOne(d => d.Banka).WithMany(p => p.BankaHesaplaris)
                .HasForeignKey(d => d.BankaId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Bankalar>(entity =>
        {
            entity.HasKey(e => e.BankaId);

            entity.ToTable("Bankalar");

            entity.Property(e => e.BankaId)
                .ValueGeneratedNever()
                .HasColumnName("BankaID");
            entity.Property(e => e.Aktif).HasDefaultValue(true);
            entity.Property(e => e.BankaAdi).HasMaxLength(100);
            entity.Property(e => e.OlusturanKullaniciId).HasColumnName("OlusturanKullaniciID");
            entity.Property(e => e.SonGuncelleyenKullaniciId).HasColumnName("SonGuncelleyenKullaniciID");
        });

        modelBuilder.Entity<Birimler>(entity =>
        {
            entity.HasKey(e => e.BirimId);

            entity.ToTable("Birimler");

            entity.Property(e => e.BirimId)
                .ValueGeneratedNever()
                .HasColumnName("BirimID");
            entity.Property(e => e.Aciklama)
                .HasMaxLength(200)
                .HasDefaultValue("");
            entity.Property(e => e.BirimAdi).HasMaxLength(50);
            entity.Property(e => e.BirimKodu).HasMaxLength(20);
            entity.Property(e => e.BirimSembol).HasMaxLength(10);
            entity.Property(e => e.OlusturanKullaniciId)
                .HasMaxLength(450)
                .HasColumnName("OlusturanKullaniciID");
            entity.Property(e => e.SirketId).HasColumnName("SirketID");
            entity.Property(e => e.SonGuncelleyenKullaniciId).HasColumnName("SonGuncelleyenKullaniciID");
        });

        modelBuilder.Entity<CariHareketler>(entity =>
        {
            entity.HasKey(e => e.CariHareketId);

            entity.ToTable("CariHareketler");

            entity.HasIndex(e => e.CariId, "IX_CariHareketler_CariId");

            entity.Property(e => e.CariHareketId).ValueGeneratedNever();
            entity.Property(e => e.Aciklama).HasMaxLength(500);
            entity.Property(e => e.Alacak).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Borc).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.HareketTuru).HasMaxLength(50);
            entity.Property(e => e.ReferansNo).HasMaxLength(50);
            entity.Property(e => e.ReferansTuru).HasMaxLength(50);
            entity.Property(e => e.Tutar).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.Cari).WithMany(p => p.CariHareketlers)
                .HasForeignKey(d => d.CariId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Cariler>(entity =>
        {
            entity.HasKey(e => e.CariId);

            entity.ToTable("Cariler");

            entity.HasIndex(e => e.VarsayilanParaBirimiId, "IX_Cariler_VarsayilanParaBirimiId");

            entity.Property(e => e.CariId)
                .ValueGeneratedNever()
                .HasColumnName("CariID");
            entity.Property(e => e.Aciklama).HasMaxLength(500);
            entity.Property(e => e.Ad).HasMaxLength(100);
            entity.Property(e => e.Adres).HasMaxLength(250);
            entity.Property(e => e.Aktif).HasDefaultValue(true);
            entity.Property(e => e.AktifMi).HasDefaultValue(true);
            entity.Property(e => e.BaslangicBakiye).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.CariKodu).HasMaxLength(20);
            entity.Property(e => e.CariTipi).HasMaxLength(50);
            entity.Property(e => e.CariUnvani).HasMaxLength(100);
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.GuncelleyenKullaniciId)
                .HasMaxLength(128)
                .HasColumnName("GuncelleyenKullaniciID");
            entity.Property(e => e.Il).HasMaxLength(50);
            entity.Property(e => e.Ilce).HasMaxLength(50);
            entity.Property(e => e.Notlar).HasMaxLength(1000);
            entity.Property(e => e.PostaKodu).HasMaxLength(10);
            entity.Property(e => e.Telefon).HasMaxLength(15);
            entity.Property(e => e.Ulke).HasMaxLength(50);
            entity.Property(e => e.VergiDairesi).HasMaxLength(50);
            entity.Property(e => e.VergiNo).HasMaxLength(20);
            entity.Property(e => e.WebSitesi).HasMaxLength(100);
            entity.Property(e => e.Yetkili).HasMaxLength(50);

            entity.HasOne(d => d.VarsayilanParaBirimi).WithMany(p => p.Carilers).HasForeignKey(d => d.VarsayilanParaBirimiId);
        });

        modelBuilder.Entity<Depolar>(entity =>
        {
            entity.HasKey(e => e.DepoId);

            entity.ToTable("Depolar");

            entity.Property(e => e.DepoId)
                .ValueGeneratedNever()
                .HasColumnName("DepoID");
            entity.Property(e => e.Adres)
                .HasMaxLength(200)
                .HasDefaultValue("");
            entity.Property(e => e.Aktif).HasDefaultValue(true);
            entity.Property(e => e.DepoAdi).HasMaxLength(100);
            entity.Property(e => e.OlusturanKullaniciId).HasColumnName("OlusturanKullaniciID");
            entity.Property(e => e.SonGuncelleyenKullaniciId).HasColumnName("SonGuncelleyenKullaniciID");
        });

        modelBuilder.Entity<FaturaAklamaKuyrugu>(entity =>
        {
            entity.HasKey(e => e.AklamaId);

            entity.ToTable("FaturaAklamaKuyrugu");

            entity.HasIndex(e => e.FaturaId, "IX_FaturaAklamaKuyrugu_FaturaID");

            entity.HasIndex(e => e.FaturaKalemId, "IX_FaturaAklamaKuyrugu_FaturaKalemID");

            entity.HasIndex(e => e.SozlesmeId, "IX_FaturaAklamaKuyrugu_SozlesmeID");

            entity.HasIndex(e => e.UrunId, "IX_FaturaAklamaKuyrugu_UrunID");

            entity.Property(e => e.AklamaId)
                .ValueGeneratedNever()
                .HasColumnName("AklamaID");
            entity.Property(e => e.AklananMiktar).HasColumnType("decimal(18, 3)");
            entity.Property(e => e.AklanmaNotu)
                .HasMaxLength(500)
                .HasDefaultValue("");
            entity.Property(e => e.BirimFiyat).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.DovizKuru)
                .HasDefaultValue(1.0m)
                .HasColumnType("decimal(18, 4)");
            entity.Property(e => e.FaturaId).HasColumnName("FaturaID");
            entity.Property(e => e.FaturaKalemId).HasColumnName("FaturaKalemID");
            entity.Property(e => e.GuncelleyenKullaniciId).HasColumnName("GuncelleyenKullaniciID");
            entity.Property(e => e.OlusturanKullaniciId).HasColumnName("OlusturanKullaniciID");
            entity.Property(e => e.ParaBirimi)
                .HasMaxLength(10)
                .HasDefaultValue("TL");
            entity.Property(e => e.ResmiFaturaKalemId).HasColumnName("ResmiFaturaKalemID");
            entity.Property(e => e.SozlesmeId).HasColumnName("SozlesmeID");
            entity.Property(e => e.UrunId).HasColumnName("UrunID");

            entity.HasOne(d => d.Fatura).WithMany(p => p.FaturaAklamaKuyrugus).HasForeignKey(d => d.FaturaId);

            entity.HasOne(d => d.FaturaKalem).WithMany(p => p.FaturaAklamaKuyrugus)
                .HasForeignKey(d => d.FaturaKalemId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Sozlesme).WithMany(p => p.FaturaAklamaKuyrugus)
                .HasForeignKey(d => d.SozlesmeId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Urun).WithMany(p => p.FaturaAklamaKuyrugus)
                .HasForeignKey(d => d.UrunId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<FaturaDetaylari>(entity =>
        {
            entity.HasKey(e => e.FaturaDetayId);

            entity.ToTable("FaturaDetaylari");

            entity.HasIndex(e => e.BirimId, "IX_FaturaDetaylari_BirimID");

            entity.HasIndex(e => e.FaturaId, "IX_FaturaDetaylari_FaturaID");

            entity.HasIndex(e => e.UrunId, "IX_FaturaDetaylari_UrunID");

            entity.Property(e => e.FaturaDetayId)
                .ValueGeneratedNever()
                .HasColumnName("FaturaDetayID");
            entity.Property(e => e.Aciklama).HasMaxLength(500);
            entity.Property(e => e.AklananMiktar).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Birim).HasMaxLength(50);
            entity.Property(e => e.BirimFiyat).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.BirimFiyatDoviz)
                .HasDefaultValue(0.0m)
                .HasColumnType("decimal(18, 2)");
            entity.Property(e => e.BirimId).HasColumnName("BirimID");
            entity.Property(e => e.FaturaId).HasColumnName("FaturaID");
            entity.Property(e => e.IndirimOrani).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.IndirimTutari).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.IndirimTutariDoviz).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.KdvOrani).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.KdvTutari).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.KdvTutariDoviz).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Miktar).HasColumnType("decimal(18, 3)");
            entity.Property(e => e.NetTutar).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.NetTutarDoviz).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.OlusturanKullaniciId).HasColumnName("OlusturanKullaniciID");
            entity.Property(e => e.SatirKdvToplam).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.SatirToplam).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.SonGuncelleyenKullaniciId).HasColumnName("SonGuncelleyenKullaniciID");
            entity.Property(e => e.Tutar).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TutarDoviz).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.UrunId).HasColumnName("UrunID");

            entity.HasOne(d => d.BirimNavigation).WithMany(p => p.FaturaDetaylaris).HasForeignKey(d => d.BirimId);

            entity.HasOne(d => d.Fatura).WithMany(p => p.FaturaDetaylaris).HasForeignKey(d => d.FaturaId);

            entity.HasOne(d => d.Urun).WithMany(p => p.FaturaDetaylaris).HasForeignKey(d => d.UrunId);
        });

        modelBuilder.Entity<FaturaOdemeleri>(entity =>
        {
            entity.HasKey(e => e.OdemeId);

            entity.ToTable("FaturaOdemeleri");

            entity.HasIndex(e => e.FaturaId, "IX_FaturaOdemeleri_FaturaID");

            entity.Property(e => e.OdemeId)
                .ValueGeneratedNever()
                .HasColumnName("OdemeID");
            entity.Property(e => e.Aciklama)
                .HasMaxLength(500)
                .HasDefaultValue("");
            entity.Property(e => e.FaturaId).HasColumnName("FaturaID");
            entity.Property(e => e.OdemeTuru).HasMaxLength(50);
            entity.Property(e => e.OdemeTutari).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.OlusturanKullaniciId).HasColumnName("OlusturanKullaniciID");
            entity.Property(e => e.SonGuncelleyenKullaniciId).HasColumnName("SonGuncelleyenKullaniciID");

            entity.HasOne(d => d.Fatura).WithMany(p => p.FaturaOdemeleris).HasForeignKey(d => d.FaturaId);
        });

        modelBuilder.Entity<FaturaTurleri>(entity =>
        {
            entity.HasKey(e => e.FaturaTuruId);

            entity.ToTable("FaturaTurleri");

            entity.Property(e => e.FaturaTuruId).HasColumnName("FaturaTuruID");
            entity.Property(e => e.FaturaTuruAdi).HasMaxLength(50);
            entity.Property(e => e.HareketTuru)
                .HasMaxLength(50)
                .HasDefaultValue("");
        });

        modelBuilder.Entity<Faturalar>(entity =>
        {
            entity.HasKey(e => e.FaturaId);

            entity.ToTable("Faturalar");

            entity.HasIndex(e => e.CariId, "IX_Faturalar_CariID");

            entity.HasIndex(e => e.FaturaTuruId, "IX_Faturalar_FaturaTuruID");

            entity.HasIndex(e => e.OdemeTuruId, "IX_Faturalar_OdemeTuruID");

            entity.HasIndex(e => e.SozlesmeId, "IX_Faturalar_SozlesmeID");

            entity.Property(e => e.FaturaId)
                .ValueGeneratedNever()
                .HasColumnName("FaturaID");
            entity.Property(e => e.AklanmaNotu)
                .HasMaxLength(500)
                .HasDefaultValue("");
            entity.Property(e => e.Aktif).HasDefaultValue(true);
            entity.Property(e => e.AraToplam).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.AraToplamDoviz).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.CariId).HasColumnName("CariID");
            entity.Property(e => e.DovizKuru).HasColumnType("decimal(18, 4)");
            entity.Property(e => e.DovizTuru)
                .HasMaxLength(10)
                .HasDefaultValue("");
            entity.Property(e => e.FaturaNotu)
                .HasMaxLength(500)
                .HasDefaultValue("");
            entity.Property(e => e.FaturaNumarasi).HasMaxLength(20);
            entity.Property(e => e.FaturaTuruId).HasColumnName("FaturaTuruID");
            entity.Property(e => e.GenelToplam).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.GenelToplamDoviz).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.GuncelleyenKullaniciId)
                .HasMaxLength(128)
                .HasColumnName("GuncelleyenKullaniciID");
            entity.Property(e => e.IndirimTutari).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.IndirimTutariDoviz).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Kdvtoplam)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("KDVToplam");
            entity.Property(e => e.KdvtoplamDoviz)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("KDVToplamDoviz");
            entity.Property(e => e.OdemeDurumu)
                .HasMaxLength(50)
                .HasDefaultValue("");
            entity.Property(e => e.OdemeTuruId).HasColumnName("OdemeTuruID");
            entity.Property(e => e.OdenenTutar).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.OlusturanKullaniciId).HasColumnName("OlusturanKullaniciID");
            entity.Property(e => e.ParaBirimi)
                .HasMaxLength(10)
                .HasDefaultValue("");
            entity.Property(e => e.ResmiMi).HasDefaultValue(true);
            entity.Property(e => e.SiparisNumarasi).HasMaxLength(20);
            entity.Property(e => e.SonGuncelleyenKullaniciId).HasColumnName("SonGuncelleyenKullaniciID");
            entity.Property(e => e.SozlesmeId).HasColumnName("SozlesmeID");

            entity.HasOne(d => d.Cari).WithMany(p => p.Faturalars).HasForeignKey(d => d.CariId);

            entity.HasOne(d => d.FaturaTuru).WithMany(p => p.Faturalars).HasForeignKey(d => d.FaturaTuruId);

            entity.HasOne(d => d.OdemeTuru).WithMany(p => p.Faturalars).HasForeignKey(d => d.OdemeTuruId);

            entity.HasOne(d => d.Sozlesme).WithMany(p => p.Faturalars)
                .HasForeignKey(d => d.SozlesmeId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<FiyatTipleri>(entity =>
        {
            entity.HasKey(e => e.FiyatTipiId);

            entity.ToTable("FiyatTipleri");

            entity.Property(e => e.FiyatTipiId).HasColumnName("FiyatTipiID");
            entity.Property(e => e.TipAdi).HasMaxLength(50);
        });

        modelBuilder.Entity<GenelSistemAyarlari>(entity =>
        {
            entity.HasKey(e => e.SistemAyarlariId);

            entity.ToTable("GenelSistemAyarlari");

            entity.Property(e => e.SistemAyarlariId).HasColumnName("SistemAyarlariID");
            entity.Property(e => e.AktifParaBirimleri).HasMaxLength(500);
            entity.Property(e => e.AnaDovizKodu).HasMaxLength(10);
            entity.Property(e => e.SirketAdi).HasMaxLength(50);
            entity.Property(e => e.SirketAdresi)
                .HasMaxLength(100)
                .HasDefaultValue("");
            entity.Property(e => e.SirketEmail)
                .HasMaxLength(100)
                .HasDefaultValue("");
            entity.Property(e => e.SirketTelefon)
                .HasMaxLength(20)
                .HasDefaultValue("");
            entity.Property(e => e.SirketVergiDairesi)
                .HasMaxLength(20)
                .HasDefaultValue("");
            entity.Property(e => e.SirketVergiNo)
                .HasMaxLength(20)
                .HasDefaultValue("");
        });

        modelBuilder.Entity<IrsaliyeDetaylari>(entity =>
        {
            entity.HasKey(e => e.IrsaliyeDetayId);

            entity.ToTable("IrsaliyeDetaylari");

            entity.HasIndex(e => e.BirimId, "IX_IrsaliyeDetaylari_BirimID");

            entity.HasIndex(e => e.IrsaliyeId, "IX_IrsaliyeDetaylari_IrsaliyeID");

            entity.HasIndex(e => e.UrunId, "IX_IrsaliyeDetaylari_UrunID");

            entity.Property(e => e.IrsaliyeDetayId)
                .ValueGeneratedNever()
                .HasColumnName("IrsaliyeDetayID");
            entity.Property(e => e.Aciklama)
                .HasMaxLength(200)
                .HasDefaultValue("");
            entity.Property(e => e.Birim)
                .HasMaxLength(50)
                .HasDefaultValue("");
            entity.Property(e => e.BirimFiyat).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.BirimId).HasColumnName("BirimID");
            entity.Property(e => e.IndirimOrani).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.IrsaliyeId).HasColumnName("IrsaliyeID");
            entity.Property(e => e.KdvOrani).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Miktar).HasColumnType("decimal(18, 3)");
            entity.Property(e => e.SatirKdvToplam).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.SatirToplam).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.UrunId).HasColumnName("UrunID");

            entity.HasOne(d => d.BirimNavigation).WithMany(p => p.IrsaliyeDetaylaris).HasForeignKey(d => d.BirimId);

            entity.HasOne(d => d.Irsaliye).WithMany(p => p.IrsaliyeDetaylaris).HasForeignKey(d => d.IrsaliyeId);

            entity.HasOne(d => d.Urun).WithMany(p => p.IrsaliyeDetaylaris).HasForeignKey(d => d.UrunId);
        });

        modelBuilder.Entity<IrsaliyeTurleri>(entity =>
        {
            entity.HasKey(e => e.IrsaliyeTuruId);

            entity.ToTable("IrsaliyeTurleri");

            entity.Property(e => e.IrsaliyeTuruId).HasColumnName("IrsaliyeTuruID");
            entity.Property(e => e.HareketTuru)
                .HasMaxLength(50)
                .HasDefaultValue("");
            entity.Property(e => e.IrsaliyeTuruAdi).HasMaxLength(50);
        });

        modelBuilder.Entity<Irsaliyeler>(entity =>
        {
            entity.HasKey(e => e.IrsaliyeId);

            entity.ToTable("Irsaliyeler");

            entity.HasIndex(e => e.CariId, "IX_Irsaliyeler_CariID");

            entity.HasIndex(e => e.FaturaId, "IX_Irsaliyeler_FaturaID");

            entity.HasIndex(e => e.IrsaliyeTuruId, "IX_Irsaliyeler_IrsaliyeTuruID");

            entity.Property(e => e.IrsaliyeId)
                .ValueGeneratedNever()
                .HasColumnName("IrsaliyeID");
            entity.Property(e => e.Aciklama)
                .HasMaxLength(500)
                .HasDefaultValue("");
            entity.Property(e => e.Aktif).HasDefaultValue(true);
            entity.Property(e => e.CariId).HasColumnName("CariID");
            entity.Property(e => e.DepoId).HasColumnName("DepoID");
            entity.Property(e => e.Durum)
                .HasMaxLength(20)
                .HasDefaultValue("");
            entity.Property(e => e.FaturaId).HasColumnName("FaturaID");
            entity.Property(e => e.GuncelleyenKullaniciId)
                .HasMaxLength(128)
                .HasColumnName("GuncelleyenKullaniciID");
            entity.Property(e => e.IrsaliyeNumarasi).HasMaxLength(20);
            entity.Property(e => e.IrsaliyeTuruId).HasColumnName("IrsaliyeTuruID");

            entity.HasOne(d => d.Cari).WithMany(p => p.Irsaliyelers).HasForeignKey(d => d.CariId);

            entity.HasOne(d => d.Fatura).WithMany(p => p.Irsaliyelers).HasForeignKey(d => d.FaturaId);

            entity.HasOne(d => d.IrsaliyeTuruNavigation).WithMany(p => p.Irsaliyelers).HasForeignKey(d => d.IrsaliyeTuruId);
        });

        modelBuilder.Entity<KasaHareketleri>(entity =>
        {
            entity.HasKey(e => e.KasaHareketId);

            entity.ToTable("KasaHareketleri");

            entity.HasIndex(e => e.CariId, "IX_KasaHareketleri_CariID");

            entity.HasIndex(e => e.HedefBankaId, "IX_KasaHareketleri_HedefBankaID");

            entity.HasIndex(e => e.HedefKasaId, "IX_KasaHareketleri_HedefKasaID");

            entity.HasIndex(e => e.KasaId, "IX_KasaHareketleri_KasaID");

            entity.HasIndex(e => e.KaynakBankaId, "IX_KasaHareketleri_KaynakBankaID");

            entity.Property(e => e.KasaHareketId)
                .ValueGeneratedNever()
                .HasColumnName("KasaHareketID");
            entity.Property(e => e.Aciklama)
                .HasMaxLength(500)
                .HasDefaultValue("");
            entity.Property(e => e.CariId).HasColumnName("CariID");
            entity.Property(e => e.DovizKuru).HasColumnType("decimal(18, 6)");
            entity.Property(e => e.HareketTuru).HasMaxLength(20);
            entity.Property(e => e.HedefBankaId).HasColumnName("HedefBankaID");
            entity.Property(e => e.HedefKasaId).HasColumnName("HedefKasaID");
            entity.Property(e => e.IslemTuru).HasMaxLength(20);
            entity.Property(e => e.IslemYapanKullaniciId).HasColumnName("IslemYapanKullaniciID");
            entity.Property(e => e.KarsiParaBirimi)
                .HasMaxLength(3)
                .HasDefaultValue("");
            entity.Property(e => e.KasaId).HasColumnName("KasaID");
            entity.Property(e => e.KaynakBankaId).HasColumnName("KaynakBankaID");
            entity.Property(e => e.ReferansId).HasColumnName("ReferansID");
            entity.Property(e => e.ReferansNo)
                .HasMaxLength(50)
                .HasDefaultValue("");
            entity.Property(e => e.ReferansTuru)
                .HasMaxLength(50)
                .HasDefaultValue("");
            entity.Property(e => e.SonGuncelleyenKullaniciId).HasColumnName("SonGuncelleyenKullaniciID");
            entity.Property(e => e.TransferId).HasColumnName("TransferID");
            entity.Property(e => e.Tutar).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.Cari).WithMany(p => p.KasaHareketleris).HasForeignKey(d => d.CariId);

            entity.HasOne(d => d.HedefBanka).WithMany(p => p.KasaHareketleriHedefBankas).HasForeignKey(d => d.HedefBankaId);

            entity.HasOne(d => d.HedefKasa).WithMany(p => p.KasaHareketleriHedefKasas).HasForeignKey(d => d.HedefKasaId);

            entity.HasOne(d => d.Kasa).WithMany(p => p.KasaHareketleriKasas)
                .HasForeignKey(d => d.KasaId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.KaynakBanka).WithMany(p => p.KasaHareketleriKaynakBankas).HasForeignKey(d => d.KaynakBankaId);
        });

        modelBuilder.Entity<Kasalar>(entity =>
        {
            entity.HasKey(e => e.KasaId);

            entity.ToTable("Kasalar");

            entity.Property(e => e.KasaId)
                .ValueGeneratedNever()
                .HasColumnName("KasaID");
            entity.Property(e => e.Aciklama)
                .HasMaxLength(500)
                .HasDefaultValue("");
            entity.Property(e => e.AcilisBakiye).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Aktif).HasDefaultValue(true);
            entity.Property(e => e.GuncelBakiye).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.KasaAdi).HasMaxLength(100);
            entity.Property(e => e.KasaTuru)
                .HasMaxLength(50)
                .HasDefaultValue("");
            entity.Property(e => e.OlusturanKullaniciId).HasColumnName("OlusturanKullaniciID");
            entity.Property(e => e.ParaBirimi).HasMaxLength(3);
            entity.Property(e => e.SonGuncelleyenKullaniciId).HasColumnName("SonGuncelleyenKullaniciID");
            entity.Property(e => e.SorumluKullaniciId).HasColumnName("SorumluKullaniciID");
        });

        modelBuilder.Entity<KurDegerleri>(entity =>
        {
            entity.HasKey(e => e.KurDegeriId);

            entity.ToTable("KurDegerleri");

            entity.HasIndex(e => e.ParaBirimiId, "IX_KurDegerleri_ParaBirimiID");

            entity.Property(e => e.KurDegeriId)
                .ValueGeneratedNever()
                .HasColumnName("KurDegeriID");
            entity.Property(e => e.Aciklama).HasMaxLength(500);
            entity.Property(e => e.Alis).HasColumnType("decimal(18, 6)");
            entity.Property(e => e.DekontNo).HasMaxLength(50);
            entity.Property(e => e.EfektifAlis).HasColumnType("decimal(18, 6)");
            entity.Property(e => e.EfektifSatis).HasColumnType("decimal(18, 6)");
            entity.Property(e => e.GuncelleyenKullaniciId)
                .HasMaxLength(128)
                .HasColumnName("GuncelleyenKullaniciID");
            entity.Property(e => e.OlusturanKullaniciId).HasColumnName("OlusturanKullaniciID");
            entity.Property(e => e.ParaBirimiId).HasColumnName("ParaBirimiID");
            entity.Property(e => e.Satis).HasColumnType("decimal(18, 6)");
            entity.Property(e => e.SonGuncelleyenKullaniciId).HasColumnName("SonGuncelleyenKullaniciID");

            entity.HasOne(d => d.ParaBirimi).WithMany(p => p.KurDegerleris)
                .HasForeignKey(d => d.ParaBirimiId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<KurMarjlari>(entity =>
        {
            entity.HasKey(e => e.KurMarjId);

            entity.ToTable("KurMarjlari");

            entity.Property(e => e.KurMarjId)
                .ValueGeneratedNever()
                .HasColumnName("KurMarjID");
            entity.Property(e => e.OlusturanKullaniciId).HasColumnName("OlusturanKullaniciID");
            entity.Property(e => e.SatisMarji).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.SonGuncelleyenKullaniciId).HasColumnName("SonGuncelleyenKullaniciID");
            entity.Property(e => e.Tanim).HasMaxLength(100);
        });

        modelBuilder.Entity<MenuRoller>(entity =>
        {
            entity.HasKey(e => new { e.MenuId, e.RolId });

            entity.ToTable("MenuRoller");

            entity.HasIndex(e => e.RolId, "IX_MenuRoller_RolId");

            entity.Property(e => e.RolId)
                .HasMaxLength(128)
                .IsUnicode(false);
            entity.Property(e => e.MenuRolId).HasColumnName("MenuRolID");

            entity.HasOne(d => d.Menu).WithMany(p => p.MenuRollers).HasForeignKey(d => d.MenuId);

            entity.HasOne(d => d.Rol).WithMany(p => p.MenuRollers).HasForeignKey(d => d.RolId);
        });

        modelBuilder.Entity<Menuler>(entity =>
        {
            entity.HasKey(e => e.MenuId);

            entity.ToTable("Menuler");

            entity.HasIndex(e => e.UstMenuId, "IX_Menuler_UstMenuID");

            entity.Property(e => e.MenuId)
                .ValueGeneratedNever()
                .HasColumnName("MenuID");
            entity.Property(e => e.Action)
                .HasMaxLength(100)
                .HasDefaultValue("");
            entity.Property(e => e.Ad).HasMaxLength(100);
            entity.Property(e => e.AktifMi).HasDefaultValue(true);
            entity.Property(e => e.Controller)
                .HasMaxLength(100)
                .HasDefaultValue("");
            entity.Property(e => e.Icon)
                .HasMaxLength(100)
                .HasDefaultValue("");
            entity.Property(e => e.OlusturanKullaniciId).HasColumnName("OlusturanKullaniciID");
            entity.Property(e => e.SonGuncelleyenKullaniciId).HasColumnName("SonGuncelleyenKullaniciID");
            entity.Property(e => e.Url)
                .HasMaxLength(255)
                .HasDefaultValue("");
            entity.Property(e => e.UstMenuId).HasColumnName("UstMenuID");

            entity.HasOne(d => d.UstMenu).WithMany(p => p.InverseUstMenu).HasForeignKey(d => d.UstMenuId);
        });

        modelBuilder.Entity<OdemeTurleri>(entity =>
        {
            entity.HasKey(e => e.OdemeTuruId);

            entity.ToTable("OdemeTurleri");

            entity.Property(e => e.OdemeTuruId).HasColumnName("OdemeTuruID");
            entity.Property(e => e.OdemeTuruAdi).HasMaxLength(50);
        });

        modelBuilder.Entity<ParaBirimiIliskileri>(entity =>
        {
            entity.HasKey(e => e.ParaBirimiIliskiId);

            entity.ToTable("ParaBirimiIliskileri");

            entity.HasIndex(e => new { e.KaynakParaBirimiId, e.HedefParaBirimiId }, "IX_DovizIliski_KaynakParaBirimiID_HedefParaBirimiID").IsUnique();

            entity.HasIndex(e => e.HedefParaBirimiId, "IX_ParaBirimiIliskileri_HedefParaBirimiID");

            entity.Property(e => e.ParaBirimiIliskiId)
                .ValueGeneratedNever()
                .HasColumnName("ParaBirimiIliskiID");
            entity.Property(e => e.Aciklama).HasMaxLength(500);
            entity.Property(e => e.Aktif).HasDefaultValue(true);
            entity.Property(e => e.GuncelleyenKullaniciId)
                .HasMaxLength(128)
                .HasColumnName("GuncelleyenKullaniciID");
            entity.Property(e => e.HedefParaBirimiId).HasColumnName("HedefParaBirimiID");
            entity.Property(e => e.KaynakParaBirimiId).HasColumnName("KaynakParaBirimiID");
            entity.Property(e => e.OlusturanKullaniciId).HasColumnName("OlusturanKullaniciID");
            entity.Property(e => e.SonGuncelleyenKullaniciId).HasColumnName("SonGuncelleyenKullaniciID");

            entity.HasOne(d => d.HedefParaBirimi).WithMany(p => p.ParaBirimiIliskileriHedefParaBirimis)
                .HasForeignKey(d => d.HedefParaBirimiId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.KaynakParaBirimi).WithMany(p => p.ParaBirimiIliskileriKaynakParaBirimis)
                .HasForeignKey(d => d.KaynakParaBirimiId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<ParaBirimleri>(entity =>
        {
            entity.HasKey(e => e.ParaBirimiId);

            entity.ToTable("ParaBirimleri");

            entity.HasIndex(e => e.Kod, "IX_ParaBirimi_Kod").IsUnique();

            entity.Property(e => e.ParaBirimiId)
                .ValueGeneratedNever()
                .HasColumnName("ParaBirimiID");
            entity.Property(e => e.Aciklama).HasMaxLength(500);
            entity.Property(e => e.Ad).HasMaxLength(50);
            entity.Property(e => e.Aktif).HasDefaultValue(true);
            entity.Property(e => e.BinlikAyraci).HasMaxLength(1);
            entity.Property(e => e.GuncelleyenKullaniciId)
                .HasMaxLength(128)
                .HasColumnName("GuncelleyenKullaniciID");
            entity.Property(e => e.Kod).HasMaxLength(10);
            entity.Property(e => e.OlusturanKullaniciId).HasColumnName("OlusturanKullaniciID");
            entity.Property(e => e.OndalikAyraci).HasMaxLength(1);
            entity.Property(e => e.Sembol).HasMaxLength(10);
            entity.Property(e => e.SonGuncelleyenKullaniciId).HasColumnName("SonGuncelleyenKullaniciID");
        });

        modelBuilder.Entity<SistemAyarlari>(entity =>
        {
            entity.ToTable("SistemAyarlari");

            entity.Property(e => e.Aciklama)
                .HasMaxLength(250)
                .HasDefaultValue("");
            entity.Property(e => e.Anahtar).HasMaxLength(100);
            entity.Property(e => e.Deger).HasMaxLength(500);
        });

        modelBuilder.Entity<SistemLoglar>(entity =>
        {
            entity.ToTable("SistemLoglar");

            entity.HasIndex(e => e.KullaniciId, "IX_SistemLoglar_KullaniciId");

            entity.Property(e => e.Ipadresi)
                .HasMaxLength(50)
                .HasDefaultValue("")
                .HasColumnName("IPAdresi");
            entity.Property(e => e.IslemTuru)
                .HasMaxLength(50)
                .HasDefaultValue("");
            entity.Property(e => e.KayitAdi).HasMaxLength(250);
            entity.Property(e => e.KayitId).HasColumnName("KayitID");
            entity.Property(e => e.KullaniciId)
                .HasMaxLength(128)
                .IsUnicode(false);
            entity.Property(e => e.LogId).HasColumnName("LogID");
            entity.Property(e => e.LogTuru).HasMaxLength(50);
            entity.Property(e => e.Mesaj).HasMaxLength(2000);
            entity.Property(e => e.Sayfa).HasMaxLength(250);
            entity.Property(e => e.TabloAdi).HasMaxLength(250);

            entity.HasOne(d => d.Kullanici).WithMany(p => p.SistemLoglars).HasForeignKey(d => d.KullaniciId);
        });

        modelBuilder.Entity<Sozlesmeler>(entity =>
        {
            entity.HasKey(e => e.SozlesmeId);

            entity.ToTable("Sozlesmeler");

            entity.HasIndex(e => e.CariId, "IX_Sozlesmeler_CariID");

            entity.Property(e => e.SozlesmeId)
                .ValueGeneratedNever()
                .HasColumnName("SozlesmeID");
            entity.Property(e => e.Aciklama).HasMaxLength(1000);
            entity.Property(e => e.Aktif).HasDefaultValue(true);
            entity.Property(e => e.AnaSozlesmeId).HasColumnName("AnaSozlesmeID");
            entity.Property(e => e.CariId).HasColumnName("CariID");
            entity.Property(e => e.GuncelleyenKullaniciId).HasColumnName("GuncelleyenKullaniciID");
            entity.Property(e => e.OlusturanKullaniciId).HasColumnName("OlusturanKullaniciID");
            entity.Property(e => e.SozlesmeDosyaYolu).HasMaxLength(500);
            entity.Property(e => e.SozlesmeDovizTuru).HasMaxLength(10);
            entity.Property(e => e.SozlesmeNo).HasMaxLength(100);
            entity.Property(e => e.SozlesmeTutari).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.VekaletnameDosyaYolu).HasMaxLength(500);

            entity.HasOne(d => d.Cari).WithMany(p => p.Sozlesmelers).HasForeignKey(d => d.CariId);
        });

        modelBuilder.Entity<StokCikisDetaylari>(entity =>
        {
            entity.HasKey(e => e.StokCikisDetayId);

            entity.ToTable("StokCikisDetaylari");

            entity.HasIndex(e => e.StokFifoId, "IX_StokCikisDetaylari_StokFifoID");

            entity.Property(e => e.StokCikisDetayId)
                .ValueGeneratedNever()
                .HasColumnName("StokCikisDetayID");
            entity.Property(e => e.Aciklama)
                .HasMaxLength(500)
                .HasDefaultValue("");
            entity.Property(e => e.BirimFiyat).HasColumnType("decimal(18, 6)");
            entity.Property(e => e.BirimFiyatTl)
                .HasColumnType("decimal(18, 6)")
                .HasColumnName("BirimFiyatTL");
            entity.Property(e => e.BirimFiyatUsd)
                .HasColumnType("decimal(18, 6)")
                .HasColumnName("BirimFiyatUSD");
            entity.Property(e => e.BirimFiyatUzs)
                .HasColumnType("decimal(18, 6)")
                .HasColumnName("BirimFiyatUZS");
            entity.Property(e => e.BirimMaliyet).HasColumnType("decimal(18, 6)");
            entity.Property(e => e.CikisMiktari).HasColumnType("decimal(18, 6)");
            entity.Property(e => e.HareketTipi)
                .HasMaxLength(50)
                .HasDefaultValue("");
            entity.Property(e => e.IptalAciklama)
                .HasMaxLength(500)
                .HasDefaultValue("");
            entity.Property(e => e.ParaBirimi)
                .HasMaxLength(10)
                .HasDefaultValue("");
            entity.Property(e => e.ReferansId).HasColumnName("ReferansID");
            entity.Property(e => e.ReferansNo)
                .HasMaxLength(50)
                .HasDefaultValue("");
            entity.Property(e => e.ReferansTuru)
                .HasMaxLength(50)
                .HasDefaultValue("");
            entity.Property(e => e.StokFifoId).HasColumnName("StokFifoID");
            entity.Property(e => e.ToplamMaliyet).HasColumnType("decimal(18, 6)");
            entity.Property(e => e.ToplamMaliyetUsd)
                .HasColumnType("decimal(18, 6)")
                .HasColumnName("ToplamMaliyetUSD");

            entity.HasOne(d => d.StokFifo).WithMany(p => p.StokCikisDetaylaris)
                .HasForeignKey(d => d.StokFifoId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<StokFifoKayitlari>(entity =>
        {
            entity.HasKey(e => e.StokFifoId);

            entity.ToTable("StokFifoKayitlari");

            entity.HasIndex(e => e.GirisTarihi, "IX_StokFifo_GirisTarihi");

            entity.HasIndex(e => new { e.ReferansId, e.ReferansTuru }, "IX_StokFifo_Referans");

            entity.HasIndex(e => new { e.UrunId, e.KalanMiktar, e.Aktif, e.Silindi, e.Iptal }, "IX_StokFifo_StokSorgu");

            entity.HasIndex(e => e.UrunId, "IX_StokFifo_UrunID");

            entity.Property(e => e.StokFifoId)
                .ValueGeneratedNever()
                .HasColumnName("StokFifoID");
            entity.Property(e => e.Aciklama).HasMaxLength(500);
            entity.Property(e => e.Birim).HasMaxLength(20);
            entity.Property(e => e.BirimFiyat).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.DovizKuru).HasColumnType("decimal(18, 6)");
            entity.Property(e => e.IptalAciklama).HasMaxLength(500);
            entity.Property(e => e.IptalEdenKullaniciId).HasColumnName("IptalEdenKullaniciID");
            entity.Property(e => e.KalanMiktar).HasColumnType("decimal(18, 3)");
            entity.Property(e => e.Miktar).HasColumnType("decimal(18, 3)");
            entity.Property(e => e.ParaBirimi)
                .HasMaxLength(3)
                .HasDefaultValue("");
            entity.Property(e => e.ReferansId).HasColumnName("ReferansID");
            entity.Property(e => e.ReferansNo).HasMaxLength(50);
            entity.Property(e => e.ReferansTuru).HasMaxLength(20);
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();
            entity.Property(e => e.TlbirimFiyat)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("TLBirimFiyat");
            entity.Property(e => e.UrunId).HasColumnName("UrunID");
            entity.Property(e => e.UsdbirimFiyat)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("USDBirimFiyat");
            entity.Property(e => e.UzsbirimFiyat)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("UZSBirimFiyat");

            entity.HasOne(d => d.Urun).WithMany(p => p.StokFifoKayitlaris).HasForeignKey(d => d.UrunId);
        });

        modelBuilder.Entity<StokHareketleri>(entity =>
        {
            entity.HasKey(e => e.StokHareketId);

            entity.ToTable("StokHareketleri");

            entity.HasIndex(e => e.DepoId, "IX_StokHareketleri_DepoID");

            entity.HasIndex(e => e.UrunId, "IX_StokHareketleri_UrunID");

            entity.Property(e => e.StokHareketId)
                .ValueGeneratedNever()
                .HasColumnName("StokHareketID");
            entity.Property(e => e.Aciklama).HasMaxLength(500);
            entity.Property(e => e.Birim).HasMaxLength(50);
            entity.Property(e => e.BirimFiyat).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.DepoId).HasColumnName("DepoID");
            entity.Property(e => e.FaturaId).HasColumnName("FaturaID");
            entity.Property(e => e.IrsaliyeId).HasColumnName("IrsaliyeID");
            entity.Property(e => e.IrsaliyeTuru).HasMaxLength(50);
            entity.Property(e => e.IslemYapanKullaniciId).HasColumnName("IslemYapanKullaniciID");
            entity.Property(e => e.Miktar).HasColumnType("decimal(18, 3)");
            entity.Property(e => e.ParaBirimi).HasMaxLength(10);
            entity.Property(e => e.ReferansId).HasColumnName("ReferansID");
            entity.Property(e => e.ReferansNo).HasMaxLength(50);
            entity.Property(e => e.ReferansTuru).HasMaxLength(50);
            entity.Property(e => e.SonGuncelleyenKullaniciId).HasColumnName("SonGuncelleyenKullaniciID");
            entity.Property(e => e.UrunId).HasColumnName("UrunID");

            entity.HasOne(d => d.Depo).WithMany(p => p.StokHareketleris).HasForeignKey(d => d.DepoId);

            entity.HasOne(d => d.Urun).WithMany(p => p.StokHareketleris).HasForeignKey(d => d.UrunId);
        });

        modelBuilder.Entity<UrunFiyatlari>(entity =>
        {
            entity.HasKey(e => e.FiyatId);

            entity.ToTable("UrunFiyatlari");

            entity.HasIndex(e => e.FiyatTipiId, "IX_UrunFiyatlari_FiyatTipiID");

            entity.HasIndex(e => e.UrunId, "IX_UrunFiyatlari_UrunID");

            entity.Property(e => e.FiyatId)
                .ValueGeneratedNever()
                .HasColumnName("FiyatID");
            entity.Property(e => e.Fiyat).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.FiyatTipiId).HasColumnName("FiyatTipiID");
            entity.Property(e => e.OlusturanKullaniciId).HasColumnName("OlusturanKullaniciID");
            entity.Property(e => e.SonGuncelleyenKullaniciId).HasColumnName("SonGuncelleyenKullaniciID");
            entity.Property(e => e.UrunId).HasColumnName("UrunID");

            entity.HasOne(d => d.FiyatTipi).WithMany(p => p.UrunFiyatlaris).HasForeignKey(d => d.FiyatTipiId);

            entity.HasOne(d => d.Urun).WithMany(p => p.UrunFiyatlaris).HasForeignKey(d => d.UrunId);
        });

        modelBuilder.Entity<UrunKategorileri>(entity =>
        {
            entity.HasKey(e => e.KategoriId);

            entity.ToTable("UrunKategorileri");

            entity.Property(e => e.KategoriId)
                .ValueGeneratedNever()
                .HasColumnName("KategoriID");
            entity.Property(e => e.Aciklama).HasMaxLength(500);
            entity.Property(e => e.Aktif).HasDefaultValue(true);
            entity.Property(e => e.KategoriAdi).HasMaxLength(100);
            entity.Property(e => e.OlusturanKullaniciId).HasColumnName("OlusturanKullaniciID");
            entity.Property(e => e.SonGuncelleyenKullaniciId).HasColumnName("SonGuncelleyenKullaniciID");
        });

        modelBuilder.Entity<Urunler>(entity =>
        {
            entity.HasKey(e => e.UrunId);

            entity.ToTable("Urunler");

            entity.HasIndex(e => e.BirimId, "IX_Urunler_BirimID");

            entity.HasIndex(e => e.KategoriId, "IX_Urunler_KategoriID");

            entity.Property(e => e.UrunId)
                .ValueGeneratedNever()
                .HasColumnName("UrunID");
            entity.Property(e => e.Aciklama)
                .HasMaxLength(500)
                .HasDefaultValue("");
            entity.Property(e => e.Aktif).HasDefaultValue(true);
            entity.Property(e => e.BirimId).HasColumnName("BirimID");
            entity.Property(e => e.DovizliListeFiyati).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.DovizliMaliyetFiyati).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.DovizliSatisFiyati).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.KategoriId).HasColumnName("KategoriID");
            entity.Property(e => e.Kdvorani).HasColumnName("KDVOrani");
            entity.Property(e => e.OlusturanKullaniciId).HasColumnName("OlusturanKullaniciID");
            entity.Property(e => e.SonGuncelleyenKullaniciId).HasColumnName("SonGuncelleyenKullaniciID");
            entity.Property(e => e.UrunAdi).HasMaxLength(200);
            entity.Property(e => e.UrunKodu).HasMaxLength(50);

            entity.HasOne(d => d.Birim).WithMany(p => p.Urunlers)
                .HasForeignKey(d => d.BirimId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(d => d.Kategori).WithMany(p => p.Urunlers).HasForeignKey(d => d.KategoriId);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

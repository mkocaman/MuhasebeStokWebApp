IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
CREATE TABLE [AspNetRoles] (
    [Id] nvarchar(450) NOT NULL,
    [Name] nvarchar(256) NULL,
    [NormalizedName] nvarchar(256) NULL,
    [ConcurrencyStamp] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
);

CREATE TABLE [AspNetUsers] (
    [Id] nvarchar(450) NOT NULL,
    [UserName] nvarchar(256) NULL,
    [NormalizedUserName] nvarchar(256) NULL,
    [Email] nvarchar(256) NULL,
    [NormalizedEmail] nvarchar(256) NULL,
    [EmailConfirmed] bit NOT NULL,
    [PasswordHash] nvarchar(max) NULL,
    [SecurityStamp] nvarchar(max) NULL,
    [ConcurrencyStamp] nvarchar(max) NULL,
    [PhoneNumber] nvarchar(max) NULL,
    [PhoneNumberConfirmed] bit NOT NULL,
    [TwoFactorEnabled] bit NOT NULL,
    [LockoutEnd] datetimeoffset NULL,
    [LockoutEnabled] bit NOT NULL,
    [AccessFailedCount] int NOT NULL,
    CONSTRAINT [PK_AspNetUsers] PRIMARY KEY ([Id])
);

CREATE TABLE [Bankalar] (
    [BankaID] uniqueidentifier NOT NULL,
    [BankaAdi] nvarchar(100) NOT NULL,
    [SubeAdi] nvarchar(100) NOT NULL,
    [SubeKodu] nvarchar(50) NOT NULL,
    [HesapNo] nvarchar(50) NOT NULL,
    [IBAN] nvarchar(50) NOT NULL,
    [ParaBirimi] nvarchar(10) NOT NULL DEFAULT N'TRY',
    [AcilisBakiye] decimal(18,2) NOT NULL DEFAULT 0.0,
    [GuncelBakiye] decimal(18,2) NOT NULL DEFAULT 0.0,
    [Aciklama] nvarchar(500) NOT NULL,
    [Aktif] bit NOT NULL DEFAULT CAST(1 AS bit),
    [YetkiliKullaniciID] uniqueidentifier NULL,
    [OlusturanKullaniciID] uniqueidentifier NULL,
    [SonGuncelleyenKullaniciID] uniqueidentifier NULL,
    [OlusturmaTarihi] datetime2 NOT NULL,
    [GuncellemeTarihi] datetime2 NULL,
    [SoftDelete] bit NOT NULL DEFAULT CAST(0 AS bit),
    CONSTRAINT [PK_Bankalar] PRIMARY KEY ([BankaID])
);

CREATE TABLE [Birimler] (
    [BirimID] uniqueidentifier NOT NULL,
    [BirimAdi] nvarchar(50) NOT NULL,
    [Aciklama] nvarchar(200) NOT NULL,
    [Aktif] bit NOT NULL,
    [SoftDelete] bit NOT NULL,
    CONSTRAINT [PK_Birimler] PRIMARY KEY ([BirimID])
);

CREATE TABLE [Cariler] (
    [CariID] uniqueidentifier NOT NULL,
    [CariAdi] nvarchar(200) NOT NULL,
    [VergiNo] nvarchar(50) NOT NULL,
    [Telefon] nvarchar(20) NOT NULL,
    [Email] nvarchar(100) NOT NULL,
    [Aktif] bit NOT NULL DEFAULT CAST(1 AS bit),
    [OlusturanKullaniciID] uniqueidentifier NULL,
    [SonGuncelleyenKullaniciID] uniqueidentifier NULL,
    [OlusturmaTarihi] datetime2 NULL,
    [GuncellemeTarihi] datetime2 NULL,
    [SoftDelete] bit NOT NULL DEFAULT CAST(0 AS bit),
    [Adres] nvarchar(250) NOT NULL,
    [Aciklama] nvarchar(500) NOT NULL,
    [Yetkili] nvarchar(50) NOT NULL,
    CONSTRAINT [PK_Cariler] PRIMARY KEY ([CariID])
);

CREATE TABLE [Depolar] (
    [DepoID] uniqueidentifier NOT NULL,
    [DepoAdi] nvarchar(100) NOT NULL,
    [Adres] nvarchar(200) NOT NULL,
    [Aktif] bit NOT NULL DEFAULT CAST(1 AS bit),
    [OlusturanKullaniciID] uniqueidentifier NULL,
    [SonGuncelleyenKullaniciID] uniqueidentifier NULL,
    [OlusturmaTarihi] datetime2 NULL,
    [GuncellemeTarihi] datetime2 NULL,
    [SoftDelete] bit NOT NULL DEFAULT CAST(0 AS bit),
    CONSTRAINT [PK_Depolar] PRIMARY KEY ([DepoID])
);

CREATE TABLE [Dovizler] (
    [DovizID] uniqueidentifier NOT NULL,
    [DovizKodu] nvarchar(10) NOT NULL,
    [DovizAdi] nvarchar(50) NOT NULL,
    [Sembol] nvarchar(10) NOT NULL,
    [Aktif] bit NOT NULL DEFAULT CAST(1 AS bit),
    [SoftDelete] bit NOT NULL DEFAULT CAST(0 AS bit),
    [OlusturmaTarihi] datetime2 NOT NULL,
    [GuncellemeTarihi] datetime2 NULL,
    CONSTRAINT [PK_Dovizler] PRIMARY KEY ([DovizID])
);

CREATE TABLE [FaturaTurleri] (
    [FaturaTuruID] int NOT NULL IDENTITY,
    [FaturaTuruAdi] nvarchar(50) NOT NULL,
    [HareketTuru] nvarchar(50) NOT NULL,
    CONSTRAINT [PK_FaturaTurleri] PRIMARY KEY ([FaturaTuruID])
);

CREATE TABLE [FiyatTipleri] (
    [FiyatTipiID] int NOT NULL IDENTITY,
    [TipAdi] nvarchar(50) NOT NULL,
    CONSTRAINT [PK_FiyatTipleri] PRIMARY KEY ([FiyatTipiID])
);

CREATE TABLE [IrsaliyeTurleri] (
    [IrsaliyeTuruID] int NOT NULL IDENTITY,
    [IrsaliyeTuruAdi] nvarchar(50) NOT NULL,
    [HareketTuru] nvarchar(50) NOT NULL,
    CONSTRAINT [PK_IrsaliyeTurleri] PRIMARY KEY ([IrsaliyeTuruID])
);

CREATE TABLE [Kasalar] (
    [KasaID] uniqueidentifier NOT NULL,
    [KasaAdi] nvarchar(100) NOT NULL,
    [KasaTuru] nvarchar(50) NOT NULL,
    [ParaBirimi] nvarchar(3) NOT NULL,
    [AcilisBakiye] decimal(18,2) NOT NULL DEFAULT 0.0,
    [GuncelBakiye] decimal(18,2) NOT NULL DEFAULT 0.0,
    [Aciklama] nvarchar(500) NOT NULL,
    [Aktif] bit NOT NULL DEFAULT CAST(1 AS bit),
    [SorumluKullaniciID] uniqueidentifier NULL,
    [OlusturanKullaniciID] uniqueidentifier NULL,
    [SonGuncelleyenKullaniciID] uniqueidentifier NULL,
    [OlusturmaTarihi] datetime2 NOT NULL,
    [GuncellemeTarihi] datetime2 NULL,
    [SoftDelete] bit NOT NULL DEFAULT CAST(0 AS bit),
    CONSTRAINT [PK_Kasalar] PRIMARY KEY ([KasaID])
);

CREATE TABLE [OdemeTurleri] (
    [OdemeTuruID] int NOT NULL IDENTITY,
    [OdemeTuruAdi] nvarchar(50) NOT NULL,
    CONSTRAINT [PK_OdemeTurleri] PRIMARY KEY ([OdemeTuruID])
);

CREATE TABLE [SistemLoglar] (
    [LogID] uniqueidentifier NOT NULL,
    [IslemTuru] nvarchar(50) NOT NULL,
    [KayitID] uniqueidentifier NULL,
    [TabloAdi] nvarchar(50) NOT NULL,
    [KayitAdi] nvarchar(100) NOT NULL,
    [IslemTarihi] datetime2 NOT NULL,
    [Aciklama] nvarchar(500) NOT NULL,
    [KullaniciID] nvarchar(max) NULL,
    [KullaniciAdi] nvarchar(50) NOT NULL,
    [IPAdresi] nvarchar(50) NOT NULL,
    [Tarayici] nvarchar(1024) NULL,
    [Basarili] bit NOT NULL,
    [HataMesaji] nvarchar(500) NULL,
    [IlgiliID] nvarchar(max) NULL,
    [SoftDelete] bit NOT NULL,
    CONSTRAINT [PK_SistemLoglar] PRIMARY KEY ([LogID])
);

CREATE TABLE [UrunKategorileri] (
    [KategoriID] uniqueidentifier NOT NULL,
    [KategoriAdi] nvarchar(100) NOT NULL,
    [Aciklama] nvarchar(500) NOT NULL,
    [Aktif] bit NOT NULL DEFAULT CAST(1 AS bit),
    [OlusturanKullaniciID] uniqueidentifier NULL,
    [SonGuncelleyenKullaniciID] uniqueidentifier NULL,
    [OlusturmaTarihi] datetime2 NULL,
    [GuncellemeTarihi] datetime2 NULL,
    [SoftDelete] bit NOT NULL DEFAULT CAST(0 AS bit),
    CONSTRAINT [PK_UrunKategorileri] PRIMARY KEY ([KategoriID])
);

CREATE TABLE [AspNetRoleClaims] (
    [Id] int NOT NULL IDENTITY,
    [RoleId] nvarchar(450) NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [AspNetUserClaims] (
    [Id] int NOT NULL IDENTITY,
    [UserId] nvarchar(450) NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [AspNetUserLogins] (
    [LoginProvider] nvarchar(450) NOT NULL,
    [ProviderKey] nvarchar(450) NOT NULL,
    [ProviderDisplayName] nvarchar(max) NULL,
    [UserId] nvarchar(450) NOT NULL,
    CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
    CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [AspNetUserRoles] (
    [UserId] nvarchar(450) NOT NULL,
    [RoleId] nvarchar(450) NOT NULL,
    CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
    CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [AspNetUserTokens] (
    [UserId] nvarchar(450) NOT NULL,
    [LoginProvider] nvarchar(450) NOT NULL,
    [Name] nvarchar(450) NOT NULL,
    [Value] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
    CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [CariHareketler] (
    [CariHareketID] uniqueidentifier NOT NULL,
    [CariID] uniqueidentifier NOT NULL,
    [Tutar] decimal(18,2) NOT NULL,
    [HareketTuru] nvarchar(50) NOT NULL,
    [Tarih] datetime2 NOT NULL,
    [ReferansNo] nvarchar(50) NULL,
    [ReferansTuru] nvarchar(50) NULL,
    [ReferansID] uniqueidentifier NULL,
    [Aciklama] nvarchar(500) NULL,
    [IslemYapanKullaniciID] uniqueidentifier NULL,
    [SonGuncelleyenKullaniciID] uniqueidentifier NULL,
    [OlusturmaTarihi] datetime2 NOT NULL,
    [GuncellemeTarihi] datetime2 NULL,
    [SoftDelete] bit NOT NULL,
    CONSTRAINT [PK_CariHareketler] PRIMARY KEY ([CariHareketID]),
    CONSTRAINT [FK_CariHareketler_Cariler_CariID] FOREIGN KEY ([CariID]) REFERENCES [Cariler] ([CariID]) ON DELETE CASCADE
);

CREATE TABLE [DovizKurlari] (
    [DovizKuruID] uniqueidentifier NOT NULL,
    [KaynakParaBirimi] nvarchar(3) NOT NULL DEFAULT N'',
    [KaynakParaBirimiID] uniqueidentifier NOT NULL,
    [HedefParaBirimi] nvarchar(3) NOT NULL DEFAULT N'',
    [HedefParaBirimiID] uniqueidentifier NOT NULL,
    [KurDegeri] decimal(18,6) NOT NULL,
    [AlisFiyati] decimal(18,6) NULL,
    [SatisFiyati] decimal(18,6) NULL,
    [Tarih] datetime2 NOT NULL,
    [Kaynak] nvarchar(100) NOT NULL DEFAULT N'Manuel',
    [Aciklama] nvarchar(500) NULL,
    [Aktif] bit NOT NULL DEFAULT CAST(1 AS bit),
    [SoftDelete] bit NOT NULL DEFAULT CAST(0 AS bit),
    [OlusturmaTarihi] datetime2 NOT NULL,
    [GuncellemeTarihi] datetime2 NULL,
    CONSTRAINT [PK_DovizKurlari] PRIMARY KEY ([DovizKuruID]),
    CONSTRAINT [FK_DovizKurlari_Dovizler_HedefParaBirimiID] FOREIGN KEY ([HedefParaBirimiID]) REFERENCES [Dovizler] ([DovizID]) ON DELETE NO ACTION,
    CONSTRAINT [FK_DovizKurlari_Dovizler_KaynakParaBirimiID] FOREIGN KEY ([KaynakParaBirimiID]) REFERENCES [Dovizler] ([DovizID]) ON DELETE NO ACTION
);

CREATE TABLE [SistemAyarlari] (
    [SistemAyarlariID] uniqueidentifier NOT NULL,
    [AnaDovizKodu] nvarchar(10) NOT NULL DEFAULT N'USD',
    [AnaParaBirimiID] uniqueidentifier NULL,
    [IkinciDovizKodu] nvarchar(10) NULL DEFAULT N'UZS',
    [IkinciParaBirimiID] uniqueidentifier NULL,
    [UcuncuDovizKodu] nvarchar(10) NULL DEFAULT N'TRY',
    [UcuncuParaBirimiID] uniqueidentifier NULL,
    [SirketAdi] nvarchar(100) NOT NULL DEFAULT N'Şirket',
    [SirketAdresi] nvarchar(250) NULL DEFAULT N'',
    [SirketTelefon] nvarchar(20) NULL DEFAULT N'',
    [SirketEmail] nvarchar(100) NULL DEFAULT N'',
    [SirketVergiNo] nvarchar(20) NULL DEFAULT N'',
    [SirketVergiDairesi] nvarchar(100) NULL DEFAULT N'',
    [OtomatikDovizGuncelleme] bit NOT NULL,
    [DovizGuncellemeSikligi] int NOT NULL,
    [SonDovizGuncellemeTarihi] datetime2 NULL,
    [AktifParaBirimleri] nvarchar(500) NULL DEFAULT N'USD,EUR,TRY,UZS,GBP',
    [Aktif] bit NOT NULL DEFAULT CAST(1 AS bit),
    [SoftDelete] bit NOT NULL DEFAULT CAST(0 AS bit),
    [OlusturmaTarihi] datetime2 NOT NULL,
    [GuncellemeTarihi] datetime2 NULL,
    CONSTRAINT [PK_SistemAyarlari] PRIMARY KEY ([SistemAyarlariID]),
    CONSTRAINT [FK_SistemAyarlari_Dovizler_AnaParaBirimiID] FOREIGN KEY ([AnaParaBirimiID]) REFERENCES [Dovizler] ([DovizID]),
    CONSTRAINT [FK_SistemAyarlari_Dovizler_IkinciParaBirimiID] FOREIGN KEY ([IkinciParaBirimiID]) REFERENCES [Dovizler] ([DovizID]),
    CONSTRAINT [FK_SistemAyarlari_Dovizler_UcuncuParaBirimiID] FOREIGN KEY ([UcuncuParaBirimiID]) REFERENCES [Dovizler] ([DovizID])
);

CREATE TABLE [BankaHareketleri] (
    [BankaHareketID] uniqueidentifier NOT NULL,
    [BankaID] uniqueidentifier NOT NULL,
    [KaynakKasaID] uniqueidentifier NULL,
    [HedefKasaID] uniqueidentifier NULL,
    [TransferID] uniqueidentifier NULL,
    [CariID] uniqueidentifier NULL,
    [Tutar] decimal(18,2) NOT NULL,
    [HareketTuru] nvarchar(50) NOT NULL,
    [Tarih] datetime2 NOT NULL,
    [ReferansNo] nvarchar(50) NOT NULL,
    [ReferansTuru] nvarchar(50) NOT NULL,
    [ReferansID] uniqueidentifier NULL,
    [Aciklama] nvarchar(500) NOT NULL,
    [DekontNo] nvarchar(50) NOT NULL,
    [IslemYapanKullaniciID] uniqueidentifier NULL,
    [SonGuncelleyenKullaniciID] uniqueidentifier NULL,
    [OlusturmaTarihi] datetime2 NOT NULL,
    [GuncellemeTarihi] datetime2 NULL,
    [SoftDelete] bit NOT NULL DEFAULT CAST(0 AS bit),
    [KarsiUnvan] nvarchar(200) NOT NULL,
    [KarsiBankaAdi] nvarchar(50) NOT NULL,
    [KarsiIBAN] nvarchar(50) NOT NULL,
    CONSTRAINT [PK_BankaHareketleri] PRIMARY KEY ([BankaHareketID]),
    CONSTRAINT [FK_BankaHareketleri_Bankalar_BankaID] FOREIGN KEY ([BankaID]) REFERENCES [Bankalar] ([BankaID]) ON DELETE CASCADE,
    CONSTRAINT [FK_BankaHareketleri_Cariler_CariID] FOREIGN KEY ([CariID]) REFERENCES [Cariler] ([CariID]),
    CONSTRAINT [FK_BankaHareketleri_Kasalar_HedefKasaID] FOREIGN KEY ([HedefKasaID]) REFERENCES [Kasalar] ([KasaID]),
    CONSTRAINT [FK_BankaHareketleri_Kasalar_KaynakKasaID] FOREIGN KEY ([KaynakKasaID]) REFERENCES [Kasalar] ([KasaID])
);

CREATE TABLE [KasaHareketleri] (
    [KasaHareketID] uniqueidentifier NOT NULL,
    [KasaID] uniqueidentifier NOT NULL,
    [CariID] uniqueidentifier NULL,
    [KaynakBankaID] uniqueidentifier NULL,
    [Tutar] decimal(18,2) NOT NULL,
    [HareketTuru] nvarchar(20) NOT NULL,
    [HedefKasaID] uniqueidentifier NULL,
    [HedefBankaID] uniqueidentifier NULL,
    [IslemTuru] nvarchar(20) NOT NULL,
    [DovizKuru] decimal(18,6) NOT NULL,
    [KarsiParaBirimi] nvarchar(3) NOT NULL,
    [Tarih] datetime2 NOT NULL,
    [ReferansNo] nvarchar(50) NOT NULL,
    [ReferansTuru] nvarchar(50) NOT NULL,
    [ReferansID] uniqueidentifier NULL,
    [Aciklama] nvarchar(500) NOT NULL,
    [IslemYapanKullaniciID] uniqueidentifier NULL,
    [SonGuncelleyenKullaniciID] uniqueidentifier NULL,
    [OlusturmaTarihi] datetime2 NOT NULL,
    [GuncellemeTarihi] datetime2 NULL,
    [SoftDelete] bit NOT NULL DEFAULT CAST(0 AS bit),
    [TransferID] uniqueidentifier NULL,
    CONSTRAINT [PK_KasaHareketleri] PRIMARY KEY ([KasaHareketID]),
    CONSTRAINT [FK_KasaHareketleri_Bankalar_HedefBankaID] FOREIGN KEY ([HedefBankaID]) REFERENCES [Bankalar] ([BankaID]),
    CONSTRAINT [FK_KasaHareketleri_Bankalar_KaynakBankaID] FOREIGN KEY ([KaynakBankaID]) REFERENCES [Bankalar] ([BankaID]),
    CONSTRAINT [FK_KasaHareketleri_Cariler_CariID] FOREIGN KEY ([CariID]) REFERENCES [Cariler] ([CariID]),
    CONSTRAINT [FK_KasaHareketleri_Kasalar_HedefKasaID] FOREIGN KEY ([HedefKasaID]) REFERENCES [Kasalar] ([KasaID]),
    CONSTRAINT [FK_KasaHareketleri_Kasalar_KasaID] FOREIGN KEY ([KasaID]) REFERENCES [Kasalar] ([KasaID])
);

CREATE TABLE [Faturalar] (
    [FaturaID] uniqueidentifier NOT NULL,
    [FaturaNumarasi] nvarchar(20) NOT NULL,
    [SiparisNumarasi] nvarchar(20) NOT NULL,
    [FaturaTarihi] datetime2 NULL,
    [VadeTarihi] datetime2 NULL,
    [CariID] uniqueidentifier NULL,
    [FaturaTuruID] int NULL,
    [AraToplam] decimal(18,2) NULL,
    [KDVToplam] decimal(18,2) NULL,
    [GenelToplam] decimal(18,2) NULL,
    [OdemeDurumu] nvarchar(50) NOT NULL,
    [FaturaNotu] nvarchar(500) NOT NULL,
    [Resmi] bit NULL DEFAULT CAST(1 AS bit),
    [DovizTuru] nvarchar(10) NOT NULL,
    [DovizKuru] decimal(18,4) NULL,
    [OlusturmaTarihi] datetime2 NULL,
    [GuncellemeTarihi] datetime2 NULL,
    [SoftDelete] bit NOT NULL DEFAULT CAST(0 AS bit),
    [Aktif] bit NULL DEFAULT CAST(1 AS bit),
    [OlusturanKullaniciID] uniqueidentifier NULL,
    [SonGuncelleyenKullaniciID] uniqueidentifier NULL,
    [OdemeTuruID] int NULL,
    CONSTRAINT [PK_Faturalar] PRIMARY KEY ([FaturaID]),
    CONSTRAINT [FK_Faturalar_Cariler_CariID] FOREIGN KEY ([CariID]) REFERENCES [Cariler] ([CariID]),
    CONSTRAINT [FK_Faturalar_FaturaTurleri_FaturaTuruID] FOREIGN KEY ([FaturaTuruID]) REFERENCES [FaturaTurleri] ([FaturaTuruID]),
    CONSTRAINT [FK_Faturalar_OdemeTurleri_OdemeTuruID] FOREIGN KEY ([OdemeTuruID]) REFERENCES [OdemeTurleri] ([OdemeTuruID])
);

CREATE TABLE [Urunler] (
    [UrunID] uniqueidentifier NOT NULL,
    [UrunKodu] nvarchar(50) NOT NULL,
    [UrunAdi] nvarchar(200) NOT NULL,
    [BirimID] uniqueidentifier NULL,
    [StokMiktar] decimal(18,2) NOT NULL DEFAULT 0.0,
    [Aktif] bit NOT NULL DEFAULT CAST(1 AS bit),
    [OlusturanKullaniciID] uniqueidentifier NULL,
    [SonGuncelleyenKullaniciID] uniqueidentifier NULL,
    [OlusturmaTarihi] datetime2 NULL,
    [GuncellemeTarihi] datetime2 NULL,
    [SoftDelete] bit NOT NULL DEFAULT CAST(0 AS bit),
    [KategoriID] uniqueidentifier NULL,
    CONSTRAINT [PK_Urunler] PRIMARY KEY ([UrunID]),
    CONSTRAINT [FK_Urunler_Birimler_BirimID] FOREIGN KEY ([BirimID]) REFERENCES [Birimler] ([BirimID]),
    CONSTRAINT [FK_Urunler_UrunKategorileri_KategoriID] FOREIGN KEY ([KategoriID]) REFERENCES [UrunKategorileri] ([KategoriID])
);

CREATE TABLE [FaturaOdemeleri] (
    [OdemeID] uniqueidentifier NOT NULL,
    [FaturaID] uniqueidentifier NOT NULL,
    [OdemeTarihi] datetime2 NOT NULL,
    [OdemeTutari] decimal(18,2) NOT NULL,
    [OdemeTuru] nvarchar(50) NOT NULL,
    [Aciklama] nvarchar(500) NOT NULL,
    [OlusturanKullaniciID] uniqueidentifier NULL,
    [SonGuncelleyenKullaniciID] uniqueidentifier NULL,
    [OlusturmaTarihi] datetime2 NULL,
    [GuncellemeTarihi] datetime2 NULL,
    [SoftDelete] bit NOT NULL,
    CONSTRAINT [PK_FaturaOdemeleri] PRIMARY KEY ([OdemeID]),
    CONSTRAINT [FK_FaturaOdemeleri_Faturalar_FaturaID] FOREIGN KEY ([FaturaID]) REFERENCES [Faturalar] ([FaturaID]) ON DELETE CASCADE
);

CREATE TABLE [Irsaliyeler] (
    [IrsaliyeID] uniqueidentifier NOT NULL,
    [FaturaID] uniqueidentifier NULL,
    [IrsaliyeTarihi] datetime2 NOT NULL,
    [SevkTarihi] datetime2 NOT NULL,
    [Aciklama] nvarchar(200) NOT NULL,
    [OlusturanKullaniciID] uniqueidentifier NULL,
    [SonGuncelleyenKullaniciID] uniqueidentifier NULL,
    [OlusturmaTarihi] datetime2 NOT NULL,
    [GuncellemeTarihi] datetime2 NULL,
    [SoftDelete] bit NOT NULL DEFAULT CAST(0 AS bit),
    [Resmi] bit NULL DEFAULT CAST(1 AS bit),
    [IrsaliyeNumarasi] nvarchar(50) NOT NULL,
    [CariID] uniqueidentifier NOT NULL,
    [IrsaliyeTuru] nvarchar(50) NOT NULL,
    [Durum] nvarchar(50) NOT NULL,
    [GenelToplam] decimal(18,2) NULL,
    [IrsaliyeTuruID] int NULL,
    CONSTRAINT [PK_Irsaliyeler] PRIMARY KEY ([IrsaliyeID]),
    CONSTRAINT [FK_Irsaliyeler_Cariler_CariID] FOREIGN KEY ([CariID]) REFERENCES [Cariler] ([CariID]) ON DELETE CASCADE,
    CONSTRAINT [FK_Irsaliyeler_Faturalar_FaturaID] FOREIGN KEY ([FaturaID]) REFERENCES [Faturalar] ([FaturaID]),
    CONSTRAINT [FK_Irsaliyeler_IrsaliyeTurleri_IrsaliyeTuruID] FOREIGN KEY ([IrsaliyeTuruID]) REFERENCES [IrsaliyeTurleri] ([IrsaliyeTuruID])
);

CREATE TABLE [FaturaDetaylari] (
    [FaturaDetayID] uniqueidentifier NOT NULL,
    [FaturaID] uniqueidentifier NOT NULL,
    [UrunID] uniqueidentifier NOT NULL,
    [Miktar] decimal(18,2) NOT NULL,
    [BirimFiyat] decimal(18,2) NOT NULL,
    [KdvOrani] decimal(18,2) NOT NULL,
    [IndirimOrani] decimal(18,2) NOT NULL,
    [SatirToplam] decimal(18,2) NOT NULL,
    [SatirKdvToplam] decimal(18,2) NOT NULL,
    [Birim] nvarchar(50) NULL,
    [Aciklama] nvarchar(200) NULL,
    [OlusturanKullaniciID] uniqueidentifier NULL,
    [SonGuncelleyenKullaniciID] uniqueidentifier NULL,
    [OlusturmaTarihi] datetime2 NOT NULL,
    [GuncellemeTarihi] datetime2 NULL,
    [SoftDelete] bit NOT NULL,
    [Tutar] decimal(18,2) NULL,
    [KdvTutari] decimal(18,2) NULL,
    [IndirimTutari] decimal(18,2) NULL,
    [NetTutar] decimal(18,2) NULL,
    [BirimID] uniqueidentifier NULL,
    CONSTRAINT [PK_FaturaDetaylari] PRIMARY KEY ([FaturaDetayID]),
    CONSTRAINT [FK_FaturaDetaylari_Birimler_BirimID] FOREIGN KEY ([BirimID]) REFERENCES [Birimler] ([BirimID]),
    CONSTRAINT [FK_FaturaDetaylari_Faturalar_FaturaID] FOREIGN KEY ([FaturaID]) REFERENCES [Faturalar] ([FaturaID]) ON DELETE CASCADE,
    CONSTRAINT [FK_FaturaDetaylari_Urunler_UrunID] FOREIGN KEY ([UrunID]) REFERENCES [Urunler] ([UrunID]) ON DELETE CASCADE
);

CREATE TABLE [StokFifo] (
    [StokFifoID] uniqueidentifier NOT NULL,
    [UrunID] uniqueidentifier NOT NULL,
    [Miktar] decimal(18,2) NOT NULL,
    [KalanMiktar] decimal(18,2) NOT NULL,
    [BirimFiyat] decimal(18,2) NOT NULL,
    [Birim] nvarchar(20) NOT NULL,
    [ParaBirimi] nvarchar(3) NOT NULL,
    [DovizKuru] decimal(18,2) NOT NULL,
    [USDBirimFiyat] decimal(18,2) NOT NULL,
    [TLBirimFiyat] decimal(18,2) NOT NULL,
    [UZSBirimFiyat] decimal(18,2) NOT NULL,
    [GirisTarihi] datetime2 NOT NULL,
    [SonCikisTarihi] datetime2 NULL,
    [ReferansNo] nvarchar(50) NOT NULL,
    [ReferansTuru] nvarchar(20) NOT NULL,
    [ReferansID] uniqueidentifier NOT NULL,
    [Aciklama] nvarchar(500) NOT NULL,
    [Aktif] bit NOT NULL,
    [Iptal] bit NOT NULL,
    [IptalTarihi] datetime2 NULL,
    [IptalAciklama] nvarchar(500) NULL,
    [IptalEdenKullaniciID] uniqueidentifier NULL,
    [OlusturmaTarihi] datetime2 NOT NULL,
    [GuncellemeTarihi] datetime2 NULL,
    [SoftDelete] bit NOT NULL,
    CONSTRAINT [PK_StokFifo] PRIMARY KEY ([StokFifoID]),
    CONSTRAINT [FK_StokFifo_Urunler_UrunID] FOREIGN KEY ([UrunID]) REFERENCES [Urunler] ([UrunID]) ON DELETE CASCADE
);

CREATE TABLE [StokHareketleri] (
    [StokHareketID] uniqueidentifier NOT NULL,
    [UrunID] uniqueidentifier NOT NULL,
    [DepoID] uniqueidentifier NULL,
    [Miktar] decimal(18,2) NOT NULL,
    [Birim] nvarchar(50) NOT NULL,
    [Tarih] datetime2 NOT NULL,
    [HareketTuru] nvarchar(50) NOT NULL,
    [ReferansNo] nvarchar(50) NOT NULL,
    [ReferansTuru] nvarchar(50) NOT NULL,
    [ReferansID] uniqueidentifier NULL,
    [Aciklama] nvarchar(500) NOT NULL,
    [BirimFiyat] decimal(18,2) NULL,
    [IslemYapanKullaniciID] uniqueidentifier NULL,
    [SonGuncelleyenKullaniciID] uniqueidentifier NULL,
    [OlusturmaTarihi] datetime2 NULL,
    [GuncellemeTarihi] datetime2 NULL,
    [SoftDelete] bit NOT NULL,
    [FaturaID] uniqueidentifier NULL,
    [IrsaliyeID] uniqueidentifier NULL,
    CONSTRAINT [PK_StokHareketleri] PRIMARY KEY ([StokHareketID]),
    CONSTRAINT [FK_StokHareketleri_Depolar_DepoID] FOREIGN KEY ([DepoID]) REFERENCES [Depolar] ([DepoID]),
    CONSTRAINT [FK_StokHareketleri_Urunler_UrunID] FOREIGN KEY ([UrunID]) REFERENCES [Urunler] ([UrunID]) ON DELETE CASCADE
);

CREATE TABLE [UrunFiyatlari] (
    [FiyatID] int NOT NULL IDENTITY,
    [UrunID] uniqueidentifier NULL,
    [Fiyat] decimal(18,2) NOT NULL,
    [GecerliTarih] datetime2 NOT NULL,
    [FiyatTipiID] int NULL,
    [OlusturanKullaniciID] uniqueidentifier NULL,
    [SonGuncelleyenKullaniciID] uniqueidentifier NULL,
    [OlusturmaTarihi] datetime2 NULL,
    [GuncellemeTarihi] datetime2 NULL,
    [SoftDelete] bit NOT NULL,
    CONSTRAINT [PK_UrunFiyatlari] PRIMARY KEY ([FiyatID]),
    CONSTRAINT [FK_UrunFiyatlari_FiyatTipleri_FiyatTipiID] FOREIGN KEY ([FiyatTipiID]) REFERENCES [FiyatTipleri] ([FiyatTipiID]),
    CONSTRAINT [FK_UrunFiyatlari_Urunler_UrunID] FOREIGN KEY ([UrunID]) REFERENCES [Urunler] ([UrunID])
);

CREATE TABLE [IrsaliyeDetaylari] (
    [IrsaliyeDetayID] uniqueidentifier NOT NULL,
    [IrsaliyeID] uniqueidentifier NOT NULL,
    [UrunID] uniqueidentifier NOT NULL,
    [Miktar] decimal(18,2) NOT NULL,
    [Birim] nvarchar(50) NOT NULL,
    [Aciklama] nvarchar(200) NOT NULL,
    [OlusturanKullaniciID] uniqueidentifier NULL,
    [SonGuncelleyenKullaniciID] uniqueidentifier NULL,
    [OlusturmaTarihi] datetime2 NULL,
    [GuncellemeTarihi] datetime2 NULL,
    [SoftDelete] bit NOT NULL,
    [SatirToplam] decimal(18,2) NOT NULL,
    [SatirKdvToplam] decimal(18,2) NOT NULL,
    [BirimID] uniqueidentifier NULL,
    CONSTRAINT [PK_IrsaliyeDetaylari] PRIMARY KEY ([IrsaliyeDetayID]),
    CONSTRAINT [FK_IrsaliyeDetaylari_Birimler_BirimID] FOREIGN KEY ([BirimID]) REFERENCES [Birimler] ([BirimID]),
    CONSTRAINT [FK_IrsaliyeDetaylari_Irsaliyeler_IrsaliyeID] FOREIGN KEY ([IrsaliyeID]) REFERENCES [Irsaliyeler] ([IrsaliyeID]) ON DELETE CASCADE,
    CONSTRAINT [FK_IrsaliyeDetaylari_Urunler_UrunID] FOREIGN KEY ([UrunID]) REFERENCES [Urunler] ([UrunID]) ON DELETE CASCADE
);

CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [AspNetRoleClaims] ([RoleId]);

CREATE UNIQUE INDEX [RoleNameIndex] ON [AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL;

CREATE INDEX [IX_AspNetUserClaims_UserId] ON [AspNetUserClaims] ([UserId]);

CREATE INDEX [IX_AspNetUserLogins_UserId] ON [AspNetUserLogins] ([UserId]);

CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [AspNetUserRoles] ([RoleId]);

CREATE INDEX [EmailIndex] ON [AspNetUsers] ([NormalizedEmail]);

CREATE UNIQUE INDEX [UserNameIndex] ON [AspNetUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL;

CREATE INDEX [IX_BankaHareketleri_BankaID] ON [BankaHareketleri] ([BankaID]);

CREATE INDEX [IX_BankaHareketleri_CariID] ON [BankaHareketleri] ([CariID]);

CREATE INDEX [IX_BankaHareketleri_HedefKasaID] ON [BankaHareketleri] ([HedefKasaID]);

CREATE INDEX [IX_BankaHareketleri_KaynakKasaID] ON [BankaHareketleri] ([KaynakKasaID]);

CREATE INDEX [IX_CariHareketler_CariID] ON [CariHareketler] ([CariID]);

CREATE INDEX [IX_DovizKurlari_HedefParaBirimiID] ON [DovizKurlari] ([HedefParaBirimiID]);

CREATE INDEX [IX_DovizKurlari_KaynakParaBirimiID] ON [DovizKurlari] ([KaynakParaBirimiID]);

CREATE INDEX [IX_FaturaDetaylari_BirimID] ON [FaturaDetaylari] ([BirimID]);

CREATE INDEX [IX_FaturaDetaylari_FaturaID] ON [FaturaDetaylari] ([FaturaID]);

CREATE INDEX [IX_FaturaDetaylari_UrunID] ON [FaturaDetaylari] ([UrunID]);

CREATE INDEX [IX_Faturalar_CariID] ON [Faturalar] ([CariID]);

CREATE INDEX [IX_Faturalar_FaturaTuruID] ON [Faturalar] ([FaturaTuruID]);

CREATE INDEX [IX_Faturalar_OdemeTuruID] ON [Faturalar] ([OdemeTuruID]);

CREATE INDEX [IX_FaturaOdemeleri_FaturaID] ON [FaturaOdemeleri] ([FaturaID]);

CREATE INDEX [IX_IrsaliyeDetaylari_BirimID] ON [IrsaliyeDetaylari] ([BirimID]);

CREATE INDEX [IX_IrsaliyeDetaylari_IrsaliyeID] ON [IrsaliyeDetaylari] ([IrsaliyeID]);

CREATE INDEX [IX_IrsaliyeDetaylari_UrunID] ON [IrsaliyeDetaylari] ([UrunID]);

CREATE INDEX [IX_Irsaliyeler_CariID] ON [Irsaliyeler] ([CariID]);

CREATE INDEX [IX_Irsaliyeler_FaturaID] ON [Irsaliyeler] ([FaturaID]);

CREATE INDEX [IX_Irsaliyeler_IrsaliyeTuruID] ON [Irsaliyeler] ([IrsaliyeTuruID]);

CREATE INDEX [IX_KasaHareketleri_CariID] ON [KasaHareketleri] ([CariID]);

CREATE INDEX [IX_KasaHareketleri_HedefBankaID] ON [KasaHareketleri] ([HedefBankaID]);

CREATE INDEX [IX_KasaHareketleri_HedefKasaID] ON [KasaHareketleri] ([HedefKasaID]);

CREATE INDEX [IX_KasaHareketleri_KasaID] ON [KasaHareketleri] ([KasaID]);

CREATE INDEX [IX_KasaHareketleri_KaynakBankaID] ON [KasaHareketleri] ([KaynakBankaID]);

CREATE INDEX [IX_SistemAyarlari_AnaParaBirimiID] ON [SistemAyarlari] ([AnaParaBirimiID]);

CREATE INDEX [IX_SistemAyarlari_IkinciParaBirimiID] ON [SistemAyarlari] ([IkinciParaBirimiID]);

CREATE INDEX [IX_SistemAyarlari_UcuncuParaBirimiID] ON [SistemAyarlari] ([UcuncuParaBirimiID]);

CREATE INDEX [IX_StokFifo_GirisTarihi] ON [StokFifo] ([GirisTarihi]);

CREATE INDEX [IX_StokFifo_Referans] ON [StokFifo] ([ReferansID], [ReferansTuru]);

CREATE INDEX [IX_StokFifo_StokSorgu] ON [StokFifo] ([UrunID], [KalanMiktar], [Aktif], [SoftDelete], [Iptal]);

CREATE INDEX [IX_StokFifo_UrunID] ON [StokFifo] ([UrunID]);

CREATE INDEX [IX_StokHareketleri_DepoID] ON [StokHareketleri] ([DepoID]);

CREATE INDEX [IX_StokHareketleri_UrunID] ON [StokHareketleri] ([UrunID]);

CREATE INDEX [IX_UrunFiyatlari_FiyatTipiID] ON [UrunFiyatlari] ([FiyatTipiID]);

CREATE INDEX [IX_UrunFiyatlari_UrunID] ON [UrunFiyatlari] ([UrunID]);

CREATE INDEX [IX_Urunler_BirimID] ON [Urunler] ([BirimID]);

CREATE INDEX [IX_Urunler_KategoriID] ON [Urunler] ([KategoriID]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250318131105_InitialCreate', N'9.0.3');

ALTER TABLE [DovizKurlari] DROP CONSTRAINT [FK_DovizKurlari_Dovizler_HedefParaBirimiID];

ALTER TABLE [DovizKurlari] DROP CONSTRAINT [FK_DovizKurlari_Dovizler_KaynakParaBirimiID];

DROP INDEX [IX_DovizKurlari_HedefParaBirimiID] ON [DovizKurlari];

DECLARE @var sysname;
SELECT @var = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[DovizKurlari]') AND [c].[name] = N'HedefParaBirimi');
IF @var IS NOT NULL EXEC(N'ALTER TABLE [DovizKurlari] DROP CONSTRAINT [' + @var + '];');
ALTER TABLE [DovizKurlari] DROP COLUMN [HedefParaBirimi];

DECLARE @var1 sysname;
SELECT @var1 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[DovizKurlari]') AND [c].[name] = N'HedefParaBirimiID');
IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [DovizKurlari] DROP CONSTRAINT [' + @var1 + '];');
ALTER TABLE [DovizKurlari] DROP COLUMN [HedefParaBirimiID];

DECLARE @var2 sysname;
SELECT @var2 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[DovizKurlari]') AND [c].[name] = N'KaynakParaBirimi');
IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [DovizKurlari] DROP CONSTRAINT [' + @var2 + '];');
ALTER TABLE [DovizKurlari] DROP COLUMN [KaynakParaBirimi];

EXEC sp_rename N'[DovizKurlari].[KaynakParaBirimiID]', N'ParaBirimiID', 'COLUMN';

EXEC sp_rename N'[DovizKurlari].[IX_DovizKurlari_KaynakParaBirimiID]', N'IX_DovizKurlari_ParaBirimiID', 'INDEX';

DECLARE @var3 sysname;
SELECT @var3 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Dovizler]') AND [c].[name] = N'Sembol');
IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [Dovizler] DROP CONSTRAINT [' + @var3 + '];');
ALTER TABLE [Dovizler] ALTER COLUMN [Sembol] nvarchar(10) NULL;

DECLARE @var4 sysname;
SELECT @var4 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Dovizler]') AND [c].[name] = N'DovizKodu');
IF @var4 IS NOT NULL EXEC(N'ALTER TABLE [Dovizler] DROP CONSTRAINT [' + @var4 + '];');
ALTER TABLE [Dovizler] ALTER COLUMN [DovizKodu] nvarchar(3) NOT NULL;

ALTER TABLE [DovizKurlari] ADD [DovizIliskiID] uniqueidentifier NULL;

CREATE TABLE [DovizIliskileri] (
    [DovizIliskiID] uniqueidentifier NOT NULL,
    [KaynakParaBirimiID] uniqueidentifier NOT NULL,
    [HedefParaBirimiID] uniqueidentifier NOT NULL,
    [Aktif] bit NOT NULL,
    [SoftDelete] bit NOT NULL,
    [OlusturmaTarihi] datetime2 NOT NULL,
    [GuncellemeTarihi] datetime2 NULL,
    [ParaBirimiID] uniqueidentifier NULL,
    [ParaBirimiID1] uniqueidentifier NULL,
    CONSTRAINT [PK_DovizIliskileri] PRIMARY KEY ([DovizIliskiID]),
    CONSTRAINT [FK_DovizIliskileri_Dovizler_HedefParaBirimiID] FOREIGN KEY ([HedefParaBirimiID]) REFERENCES [Dovizler] ([DovizID]) ON DELETE NO ACTION,
    CONSTRAINT [FK_DovizIliskileri_Dovizler_KaynakParaBirimiID] FOREIGN KEY ([KaynakParaBirimiID]) REFERENCES [Dovizler] ([DovizID]) ON DELETE NO ACTION,
    CONSTRAINT [FK_DovizIliskileri_Dovizler_ParaBirimiID] FOREIGN KEY ([ParaBirimiID]) REFERENCES [Dovizler] ([DovizID]),
    CONSTRAINT [FK_DovizIliskileri_Dovizler_ParaBirimiID1] FOREIGN KEY ([ParaBirimiID1]) REFERENCES [Dovizler] ([DovizID])
);

CREATE INDEX [IX_DovizKurlari_DovizIliskiID] ON [DovizKurlari] ([DovizIliskiID]);

CREATE INDEX [IX_DovizIliskileri_HedefParaBirimiID] ON [DovizIliskileri] ([HedefParaBirimiID]);

CREATE INDEX [IX_DovizIliskileri_KaynakParaBirimiID] ON [DovizIliskileri] ([KaynakParaBirimiID]);

CREATE INDEX [IX_DovizIliskileri_ParaBirimiID] ON [DovizIliskileri] ([ParaBirimiID]);

CREATE INDEX [IX_DovizIliskileri_ParaBirimiID1] ON [DovizIliskileri] ([ParaBirimiID1]);

ALTER TABLE [DovizKurlari] ADD CONSTRAINT [FK_DovizKurlari_DovizIliskileri_DovizIliskiID] FOREIGN KEY ([DovizIliskiID]) REFERENCES [DovizIliskileri] ([DovizIliskiID]);

ALTER TABLE [DovizKurlari] ADD CONSTRAINT [FK_DovizKurlari_Dovizler_ParaBirimiID] FOREIGN KEY ([ParaBirimiID]) REFERENCES [Dovizler] ([DovizID]) ON DELETE NO ACTION;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250318140724_RedesignCurrencyModule', N'9.0.3');

ALTER TABLE [BankaHareketleri] DROP CONSTRAINT [FK_BankaHareketleri_Bankalar_BankaID];

ALTER TABLE [BankaHareketleri] DROP CONSTRAINT [FK_BankaHareketleri_Cariler_CariID];

ALTER TABLE [DovizKurlari] DROP CONSTRAINT [FK_DovizKurlari_DovizIliskileri_DovizIliskiID];

ALTER TABLE [DovizKurlari] DROP CONSTRAINT [FK_DovizKurlari_Dovizler_ParaBirimiID];

ALTER TABLE [FaturaDetaylari] DROP CONSTRAINT [FK_FaturaDetaylari_Urunler_UrunID];

ALTER TABLE [Faturalar] DROP CONSTRAINT [FK_Faturalar_Cariler_CariID];

ALTER TABLE [Faturalar] DROP CONSTRAINT [FK_Faturalar_FaturaTurleri_FaturaTuruID];

ALTER TABLE [IrsaliyeDetaylari] DROP CONSTRAINT [FK_IrsaliyeDetaylari_Urunler_UrunID];

ALTER TABLE [Irsaliyeler] DROP CONSTRAINT [FK_Irsaliyeler_Cariler_CariID];

ALTER TABLE [Irsaliyeler] DROP CONSTRAINT [FK_Irsaliyeler_Faturalar_FaturaID];

ALTER TABLE [Irsaliyeler] DROP CONSTRAINT [FK_Irsaliyeler_IrsaliyeTurleri_IrsaliyeTuruID];

ALTER TABLE [KasaHareketleri] DROP CONSTRAINT [FK_KasaHareketleri_Bankalar_HedefBankaID];

ALTER TABLE [KasaHareketleri] DROP CONSTRAINT [FK_KasaHareketleri_Bankalar_KaynakBankaID];

ALTER TABLE [KasaHareketleri] DROP CONSTRAINT [FK_KasaHareketleri_Cariler_CariID];

ALTER TABLE [KasaHareketleri] DROP CONSTRAINT [FK_KasaHareketleri_Kasalar_HedefKasaID];

ALTER TABLE [KasaHareketleri] DROP CONSTRAINT [FK_KasaHareketleri_Kasalar_KasaID];

ALTER TABLE [SistemAyarlari] DROP CONSTRAINT [FK_SistemAyarlari_Dovizler_AnaParaBirimiID];

ALTER TABLE [SistemAyarlari] DROP CONSTRAINT [FK_SistemAyarlari_Dovizler_IkinciParaBirimiID];

ALTER TABLE [SistemAyarlari] DROP CONSTRAINT [FK_SistemAyarlari_Dovizler_UcuncuParaBirimiID];

ALTER TABLE [StokFifo] DROP CONSTRAINT [FK_StokFifo_Urunler_UrunID];

ALTER TABLE [StokHareketleri] DROP CONSTRAINT [FK_StokHareketleri_Depolar_DepoID];

ALTER TABLE [StokHareketleri] DROP CONSTRAINT [FK_StokHareketleri_Urunler_UrunID];

DROP TABLE [DovizIliskileri];

DROP INDEX [IX_StokFifo_GirisTarihi] ON [StokFifo];

DROP INDEX [IX_StokFifo_Referans] ON [StokFifo];

DROP INDEX [IX_StokFifo_StokSorgu] ON [StokFifo];

DROP INDEX [IX_SistemAyarlari_AnaParaBirimiID] ON [SistemAyarlari];

ALTER TABLE [Dovizler] DROP CONSTRAINT [PK_Dovizler];

ALTER TABLE [DovizKurlari] DROP CONSTRAINT [PK_DovizKurlari];

DROP INDEX [IX_DovizKurlari_DovizIliskiID] ON [DovizKurlari];

DECLARE @var5 sysname;
SELECT @var5 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[SistemAyarlari]') AND [c].[name] = N'AktifParaBirimleri');
IF @var5 IS NOT NULL EXEC(N'ALTER TABLE [SistemAyarlari] DROP CONSTRAINT [' + @var5 + '];');
ALTER TABLE [SistemAyarlari] DROP COLUMN [AktifParaBirimleri];

DECLARE @var6 sysname;
SELECT @var6 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[SistemAyarlari]') AND [c].[name] = N'AnaParaBirimiID');
IF @var6 IS NOT NULL EXEC(N'ALTER TABLE [SistemAyarlari] DROP CONSTRAINT [' + @var6 + '];');
ALTER TABLE [SistemAyarlari] DROP COLUMN [AnaParaBirimiID];

DECLARE @var7 sysname;
SELECT @var7 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[SistemAyarlari]') AND [c].[name] = N'SoftDelete');
IF @var7 IS NOT NULL EXEC(N'ALTER TABLE [SistemAyarlari] DROP CONSTRAINT [' + @var7 + '];');
ALTER TABLE [SistemAyarlari] DROP COLUMN [SoftDelete];

DECLARE @var8 sysname;
SELECT @var8 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Dovizler]') AND [c].[name] = N'SoftDelete');
IF @var8 IS NOT NULL EXEC(N'ALTER TABLE [Dovizler] DROP CONSTRAINT [' + @var8 + '];');
ALTER TABLE [Dovizler] DROP COLUMN [SoftDelete];

DECLARE @var9 sysname;
SELECT @var9 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[DovizKurlari]') AND [c].[name] = N'Aciklama');
IF @var9 IS NOT NULL EXEC(N'ALTER TABLE [DovizKurlari] DROP CONSTRAINT [' + @var9 + '];');
ALTER TABLE [DovizKurlari] DROP COLUMN [Aciklama];

DECLARE @var10 sysname;
SELECT @var10 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[DovizKurlari]') AND [c].[name] = N'DovizIliskiID');
IF @var10 IS NOT NULL EXEC(N'ALTER TABLE [DovizKurlari] DROP CONSTRAINT [' + @var10 + '];');
ALTER TABLE [DovizKurlari] DROP COLUMN [DovizIliskiID];

DECLARE @var11 sysname;
SELECT @var11 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[DovizKurlari]') AND [c].[name] = N'KurDegeri');
IF @var11 IS NOT NULL EXEC(N'ALTER TABLE [DovizKurlari] DROP CONSTRAINT [' + @var11 + '];');
ALTER TABLE [DovizKurlari] DROP COLUMN [KurDegeri];

DECLARE @var12 sysname;
SELECT @var12 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[DovizKurlari]') AND [c].[name] = N'SoftDelete');
IF @var12 IS NOT NULL EXEC(N'ALTER TABLE [DovizKurlari] DROP CONSTRAINT [' + @var12 + '];');
ALTER TABLE [DovizKurlari] DROP COLUMN [SoftDelete];

EXEC sp_rename N'[Dovizler]', N'ParaBirimi', 'OBJECT';

EXEC sp_rename N'[DovizKurlari]', N'KurDegeri', 'OBJECT';

EXEC sp_rename N'[SistemAyarlari].[UcuncuParaBirimiID]', N'UcuncuDovizID', 'COLUMN';

EXEC sp_rename N'[SistemAyarlari].[IkinciParaBirimiID]', N'IkinciDovizID', 'COLUMN';

EXEC sp_rename N'[SistemAyarlari].[IX_SistemAyarlari_UcuncuParaBirimiID]', N'IX_SistemAyarlari_UcuncuDovizID', 'INDEX';

EXEC sp_rename N'[SistemAyarlari].[IX_SistemAyarlari_IkinciParaBirimiID]', N'IX_SistemAyarlari_IkinciDovizID', 'INDEX';

EXEC sp_rename N'[ParaBirimi].[DovizID]', N'ParaBirimiID', 'COLUMN';

EXEC sp_rename N'[KurDegeri].[SatisFiyati]', N'SatisDegeri', 'COLUMN';

EXEC sp_rename N'[KurDegeri].[AlisFiyati]', N'AlisDegeri', 'COLUMN';

EXEC sp_rename N'[KurDegeri].[DovizKuruID]', N'KurDegeriID', 'COLUMN';

EXEC sp_rename N'[KurDegeri].[IX_DovizKurlari_ParaBirimiID]', N'IX_KurDegeri_ParaBirimiID', 'INDEX';

DECLARE @var13 sysname;
SELECT @var13 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Urunler]') AND [c].[name] = N'StokMiktar');
IF @var13 IS NOT NULL EXEC(N'ALTER TABLE [Urunler] DROP CONSTRAINT [' + @var13 + '];');

DECLARE @var14 sysname;
SELECT @var14 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Urunler]') AND [c].[name] = N'SoftDelete');
IF @var14 IS NOT NULL EXEC(N'ALTER TABLE [Urunler] DROP CONSTRAINT [' + @var14 + '];');

DECLARE @var15 sysname;
SELECT @var15 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Urunler]') AND [c].[name] = N'Aktif');
IF @var15 IS NOT NULL EXEC(N'ALTER TABLE [Urunler] DROP CONSTRAINT [' + @var15 + '];');

DECLARE @var16 sysname;
SELECT @var16 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[UrunKategorileri]') AND [c].[name] = N'SoftDelete');
IF @var16 IS NOT NULL EXEC(N'ALTER TABLE [UrunKategorileri] DROP CONSTRAINT [' + @var16 + '];');

DECLARE @var17 sysname;
SELECT @var17 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[UrunKategorileri]') AND [c].[name] = N'Aktif');
IF @var17 IS NOT NULL EXEC(N'ALTER TABLE [UrunKategorileri] DROP CONSTRAINT [' + @var17 + '];');

ALTER TABLE [StokHareketleri] ADD [DepoID1] uniqueidentifier NULL;

ALTER TABLE [StokHareketleri] ADD [UrunID1] uniqueidentifier NULL;

DECLARE @var18 sysname;
SELECT @var18 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[SistemAyarlari]') AND [c].[name] = N'UcuncuDovizKodu');
IF @var18 IS NOT NULL EXEC(N'ALTER TABLE [SistemAyarlari] DROP CONSTRAINT [' + @var18 + '];');
UPDATE [SistemAyarlari] SET [UcuncuDovizKodu] = N'' WHERE [UcuncuDovizKodu] IS NULL;
ALTER TABLE [SistemAyarlari] ALTER COLUMN [UcuncuDovizKodu] nvarchar(5) NOT NULL;
ALTER TABLE [SistemAyarlari] ADD DEFAULT N'' FOR [UcuncuDovizKodu];

DECLARE @var19 sysname;
SELECT @var19 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[SistemAyarlari]') AND [c].[name] = N'SonDovizGuncellemeTarihi');
IF @var19 IS NOT NULL EXEC(N'ALTER TABLE [SistemAyarlari] DROP CONSTRAINT [' + @var19 + '];');
UPDATE [SistemAyarlari] SET [SonDovizGuncellemeTarihi] = '0001-01-01T00:00:00.0000000' WHERE [SonDovizGuncellemeTarihi] IS NULL;
ALTER TABLE [SistemAyarlari] ALTER COLUMN [SonDovizGuncellemeTarihi] datetime2 NOT NULL;
ALTER TABLE [SistemAyarlari] ADD DEFAULT '0001-01-01T00:00:00.0000000' FOR [SonDovizGuncellemeTarihi];

DECLARE @var20 sysname;
SELECT @var20 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[SistemAyarlari]') AND [c].[name] = N'SirketVergiNo');
IF @var20 IS NOT NULL EXEC(N'ALTER TABLE [SistemAyarlari] DROP CONSTRAINT [' + @var20 + '];');
UPDATE [SistemAyarlari] SET [SirketVergiNo] = N'' WHERE [SirketVergiNo] IS NULL;
ALTER TABLE [SistemAyarlari] ALTER COLUMN [SirketVergiNo] nvarchar(20) NOT NULL;
ALTER TABLE [SistemAyarlari] ADD DEFAULT N'' FOR [SirketVergiNo];

DECLARE @var21 sysname;
SELECT @var21 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[SistemAyarlari]') AND [c].[name] = N'SirketVergiDairesi');
IF @var21 IS NOT NULL EXEC(N'ALTER TABLE [SistemAyarlari] DROP CONSTRAINT [' + @var21 + '];');
UPDATE [SistemAyarlari] SET [SirketVergiDairesi] = N'' WHERE [SirketVergiDairesi] IS NULL;
ALTER TABLE [SistemAyarlari] ALTER COLUMN [SirketVergiDairesi] nvarchar(100) NOT NULL;
ALTER TABLE [SistemAyarlari] ADD DEFAULT N'' FOR [SirketVergiDairesi];

DECLARE @var22 sysname;
SELECT @var22 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[SistemAyarlari]') AND [c].[name] = N'SirketTelefon');
IF @var22 IS NOT NULL EXEC(N'ALTER TABLE [SistemAyarlari] DROP CONSTRAINT [' + @var22 + '];');
UPDATE [SistemAyarlari] SET [SirketTelefon] = N'' WHERE [SirketTelefon] IS NULL;
ALTER TABLE [SistemAyarlari] ALTER COLUMN [SirketTelefon] nvarchar(20) NOT NULL;
ALTER TABLE [SistemAyarlari] ADD DEFAULT N'' FOR [SirketTelefon];

DECLARE @var23 sysname;
SELECT @var23 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[SistemAyarlari]') AND [c].[name] = N'SirketEmail');
IF @var23 IS NOT NULL EXEC(N'ALTER TABLE [SistemAyarlari] DROP CONSTRAINT [' + @var23 + '];');
UPDATE [SistemAyarlari] SET [SirketEmail] = N'' WHERE [SirketEmail] IS NULL;
ALTER TABLE [SistemAyarlari] ALTER COLUMN [SirketEmail] nvarchar(100) NOT NULL;
ALTER TABLE [SistemAyarlari] ADD DEFAULT N'' FOR [SirketEmail];

DECLARE @var24 sysname;
SELECT @var24 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[SistemAyarlari]') AND [c].[name] = N'SirketAdresi');
IF @var24 IS NOT NULL EXEC(N'ALTER TABLE [SistemAyarlari] DROP CONSTRAINT [' + @var24 + '];');
UPDATE [SistemAyarlari] SET [SirketAdresi] = N'' WHERE [SirketAdresi] IS NULL;
ALTER TABLE [SistemAyarlari] ALTER COLUMN [SirketAdresi] nvarchar(250) NOT NULL;
ALTER TABLE [SistemAyarlari] ADD DEFAULT N'' FOR [SirketAdresi];

DECLARE @var25 sysname;
SELECT @var25 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[SistemAyarlari]') AND [c].[name] = N'SirketAdi');
IF @var25 IS NOT NULL EXEC(N'ALTER TABLE [SistemAyarlari] DROP CONSTRAINT [' + @var25 + '];');

DECLARE @var26 sysname;
SELECT @var26 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[SistemAyarlari]') AND [c].[name] = N'IkinciDovizKodu');
IF @var26 IS NOT NULL EXEC(N'ALTER TABLE [SistemAyarlari] DROP CONSTRAINT [' + @var26 + '];');
UPDATE [SistemAyarlari] SET [IkinciDovizKodu] = N'' WHERE [IkinciDovizKodu] IS NULL;
ALTER TABLE [SistemAyarlari] ALTER COLUMN [IkinciDovizKodu] nvarchar(5) NOT NULL;
ALTER TABLE [SistemAyarlari] ADD DEFAULT N'' FOR [IkinciDovizKodu];

DECLARE @var27 sysname;
SELECT @var27 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[SistemAyarlari]') AND [c].[name] = N'GuncellemeTarihi');
IF @var27 IS NOT NULL EXEC(N'ALTER TABLE [SistemAyarlari] DROP CONSTRAINT [' + @var27 + '];');
UPDATE [SistemAyarlari] SET [GuncellemeTarihi] = '0001-01-01T00:00:00.0000000' WHERE [GuncellemeTarihi] IS NULL;
ALTER TABLE [SistemAyarlari] ALTER COLUMN [GuncellemeTarihi] datetime2 NOT NULL;
ALTER TABLE [SistemAyarlari] ADD DEFAULT '0001-01-01T00:00:00.0000000' FOR [GuncellemeTarihi];

DECLARE @var28 sysname;
SELECT @var28 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[SistemAyarlari]') AND [c].[name] = N'AnaDovizKodu');
IF @var28 IS NOT NULL EXEC(N'ALTER TABLE [SistemAyarlari] DROP CONSTRAINT [' + @var28 + '];');
ALTER TABLE [SistemAyarlari] ALTER COLUMN [AnaDovizKodu] nvarchar(5) NOT NULL;

DECLARE @var29 sysname;
SELECT @var29 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[SistemAyarlari]') AND [c].[name] = N'Aktif');
IF @var29 IS NOT NULL EXEC(N'ALTER TABLE [SistemAyarlari] DROP CONSTRAINT [' + @var29 + '];');

ALTER TABLE [SistemAyarlari] ADD [AnaDovizID] uniqueidentifier NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';

ALTER TABLE [SistemAyarlari] ADD [Silindi] bit NOT NULL DEFAULT CAST(0 AS bit);

DECLARE @var30 sysname;
SELECT @var30 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Kasalar]') AND [c].[name] = N'SoftDelete');
IF @var30 IS NOT NULL EXEC(N'ALTER TABLE [Kasalar] DROP CONSTRAINT [' + @var30 + '];');

DECLARE @var31 sysname;
SELECT @var31 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Kasalar]') AND [c].[name] = N'GuncelBakiye');
IF @var31 IS NOT NULL EXEC(N'ALTER TABLE [Kasalar] DROP CONSTRAINT [' + @var31 + '];');

DECLARE @var32 sysname;
SELECT @var32 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Kasalar]') AND [c].[name] = N'Aktif');
IF @var32 IS NOT NULL EXEC(N'ALTER TABLE [Kasalar] DROP CONSTRAINT [' + @var32 + '];');

DECLARE @var33 sysname;
SELECT @var33 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Kasalar]') AND [c].[name] = N'AcilisBakiye');
IF @var33 IS NOT NULL EXEC(N'ALTER TABLE [Kasalar] DROP CONSTRAINT [' + @var33 + '];');

DECLARE @var34 sysname;
SELECT @var34 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[KasaHareketleri]') AND [c].[name] = N'SoftDelete');
IF @var34 IS NOT NULL EXEC(N'ALTER TABLE [KasaHareketleri] DROP CONSTRAINT [' + @var34 + '];');

DECLARE @var35 sysname;
SELECT @var35 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Irsaliyeler]') AND [c].[name] = N'SoftDelete');
IF @var35 IS NOT NULL EXEC(N'ALTER TABLE [Irsaliyeler] DROP CONSTRAINT [' + @var35 + '];');

DECLARE @var36 sysname;
SELECT @var36 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Irsaliyeler]') AND [c].[name] = N'Resmi');
IF @var36 IS NOT NULL EXEC(N'ALTER TABLE [Irsaliyeler] DROP CONSTRAINT [' + @var36 + '];');

DECLARE @var37 sysname;
SELECT @var37 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Faturalar]') AND [c].[name] = N'SoftDelete');
IF @var37 IS NOT NULL EXEC(N'ALTER TABLE [Faturalar] DROP CONSTRAINT [' + @var37 + '];');

DECLARE @var38 sysname;
SELECT @var38 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Faturalar]') AND [c].[name] = N'Resmi');
IF @var38 IS NOT NULL EXEC(N'ALTER TABLE [Faturalar] DROP CONSTRAINT [' + @var38 + '];');

DECLARE @var39 sysname;
SELECT @var39 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Faturalar]') AND [c].[name] = N'Aktif');
IF @var39 IS NOT NULL EXEC(N'ALTER TABLE [Faturalar] DROP CONSTRAINT [' + @var39 + '];');

ALTER TABLE [FaturaDetaylari] ADD [UrunID1] uniqueidentifier NULL;

DECLARE @var40 sysname;
SELECT @var40 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Depolar]') AND [c].[name] = N'SoftDelete');
IF @var40 IS NOT NULL EXEC(N'ALTER TABLE [Depolar] DROP CONSTRAINT [' + @var40 + '];');

DECLARE @var41 sysname;
SELECT @var41 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Depolar]') AND [c].[name] = N'Aktif');
IF @var41 IS NOT NULL EXEC(N'ALTER TABLE [Depolar] DROP CONSTRAINT [' + @var41 + '];');

DECLARE @var42 sysname;
SELECT @var42 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Cariler]') AND [c].[name] = N'SoftDelete');
IF @var42 IS NOT NULL EXEC(N'ALTER TABLE [Cariler] DROP CONSTRAINT [' + @var42 + '];');

DECLARE @var43 sysname;
SELECT @var43 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Cariler]') AND [c].[name] = N'Aktif');
IF @var43 IS NOT NULL EXEC(N'ALTER TABLE [Cariler] DROP CONSTRAINT [' + @var43 + '];');

DECLARE @var44 sysname;
SELECT @var44 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Bankalar]') AND [c].[name] = N'SoftDelete');
IF @var44 IS NOT NULL EXEC(N'ALTER TABLE [Bankalar] DROP CONSTRAINT [' + @var44 + '];');

DECLARE @var45 sysname;
SELECT @var45 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Bankalar]') AND [c].[name] = N'ParaBirimi');
IF @var45 IS NOT NULL EXEC(N'ALTER TABLE [Bankalar] DROP CONSTRAINT [' + @var45 + '];');

DECLARE @var46 sysname;
SELECT @var46 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Bankalar]') AND [c].[name] = N'GuncelBakiye');
IF @var46 IS NOT NULL EXEC(N'ALTER TABLE [Bankalar] DROP CONSTRAINT [' + @var46 + '];');

DECLARE @var47 sysname;
SELECT @var47 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Bankalar]') AND [c].[name] = N'Aktif');
IF @var47 IS NOT NULL EXEC(N'ALTER TABLE [Bankalar] DROP CONSTRAINT [' + @var47 + '];');

DECLARE @var48 sysname;
SELECT @var48 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Bankalar]') AND [c].[name] = N'AcilisBakiye');
IF @var48 IS NOT NULL EXEC(N'ALTER TABLE [Bankalar] DROP CONSTRAINT [' + @var48 + '];');

DECLARE @var49 sysname;
SELECT @var49 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[BankaHareketleri]') AND [c].[name] = N'SoftDelete');
IF @var49 IS NOT NULL EXEC(N'ALTER TABLE [BankaHareketleri] DROP CONSTRAINT [' + @var49 + '];');

ALTER TABLE [BankaHareketleri] ADD [BankaID1] uniqueidentifier NULL;

DECLARE @var50 sysname;
SELECT @var50 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ParaBirimi]') AND [c].[name] = N'Sembol');
IF @var50 IS NOT NULL EXEC(N'ALTER TABLE [ParaBirimi] DROP CONSTRAINT [' + @var50 + '];');
UPDATE [ParaBirimi] SET [Sembol] = N'' WHERE [Sembol] IS NULL;
ALTER TABLE [ParaBirimi] ALTER COLUMN [Sembol] nvarchar(10) NOT NULL;
ALTER TABLE [ParaBirimi] ADD DEFAULT N'' FOR [Sembol];

DECLARE @var51 sysname;
SELECT @var51 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ParaBirimi]') AND [c].[name] = N'GuncellemeTarihi');
IF @var51 IS NOT NULL EXEC(N'ALTER TABLE [ParaBirimi] DROP CONSTRAINT [' + @var51 + '];');
UPDATE [ParaBirimi] SET [GuncellemeTarihi] = '0001-01-01T00:00:00.0000000' WHERE [GuncellemeTarihi] IS NULL;
ALTER TABLE [ParaBirimi] ALTER COLUMN [GuncellemeTarihi] datetime2 NOT NULL;
ALTER TABLE [ParaBirimi] ADD DEFAULT '0001-01-01T00:00:00.0000000' FOR [GuncellemeTarihi];

DECLARE @var52 sysname;
SELECT @var52 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ParaBirimi]') AND [c].[name] = N'DovizKodu');
IF @var52 IS NOT NULL EXEC(N'ALTER TABLE [ParaBirimi] DROP CONSTRAINT [' + @var52 + '];');
ALTER TABLE [ParaBirimi] ALTER COLUMN [DovizKodu] nvarchar(5) NOT NULL;

DECLARE @var53 sysname;
SELECT @var53 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ParaBirimi]') AND [c].[name] = N'Aktif');
IF @var53 IS NOT NULL EXEC(N'ALTER TABLE [ParaBirimi] DROP CONSTRAINT [' + @var53 + '];');

ALTER TABLE [ParaBirimi] ADD [Silindi] bit NOT NULL DEFAULT CAST(0 AS bit);

DECLARE @var54 sysname;
SELECT @var54 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[KurDegeri]') AND [c].[name] = N'Kaynak');
IF @var54 IS NOT NULL EXEC(N'ALTER TABLE [KurDegeri] DROP CONSTRAINT [' + @var54 + '];');
ALTER TABLE [KurDegeri] ALTER COLUMN [Kaynak] nvarchar(50) NOT NULL;

DECLARE @var55 sysname;
SELECT @var55 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[KurDegeri]') AND [c].[name] = N'GuncellemeTarihi');
IF @var55 IS NOT NULL EXEC(N'ALTER TABLE [KurDegeri] DROP CONSTRAINT [' + @var55 + '];');
UPDATE [KurDegeri] SET [GuncellemeTarihi] = '0001-01-01T00:00:00.0000000' WHERE [GuncellemeTarihi] IS NULL;
ALTER TABLE [KurDegeri] ALTER COLUMN [GuncellemeTarihi] datetime2 NOT NULL;
ALTER TABLE [KurDegeri] ADD DEFAULT '0001-01-01T00:00:00.0000000' FOR [GuncellemeTarihi];

DECLARE @var56 sysname;
SELECT @var56 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[KurDegeri]') AND [c].[name] = N'Aktif');
IF @var56 IS NOT NULL EXEC(N'ALTER TABLE [KurDegeri] DROP CONSTRAINT [' + @var56 + '];');

DECLARE @var57 sysname;
SELECT @var57 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[KurDegeri]') AND [c].[name] = N'SatisDegeri');
IF @var57 IS NOT NULL EXEC(N'ALTER TABLE [KurDegeri] DROP CONSTRAINT [' + @var57 + '];');
UPDATE [KurDegeri] SET [SatisDegeri] = 0.0 WHERE [SatisDegeri] IS NULL;
ALTER TABLE [KurDegeri] ALTER COLUMN [SatisDegeri] decimal(18,2) NOT NULL;
ALTER TABLE [KurDegeri] ADD DEFAULT 0.0 FOR [SatisDegeri];

DECLARE @var58 sysname;
SELECT @var58 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[KurDegeri]') AND [c].[name] = N'AlisDegeri');
IF @var58 IS NOT NULL EXEC(N'ALTER TABLE [KurDegeri] DROP CONSTRAINT [' + @var58 + '];');
UPDATE [KurDegeri] SET [AlisDegeri] = 0.0 WHERE [AlisDegeri] IS NULL;
ALTER TABLE [KurDegeri] ALTER COLUMN [AlisDegeri] decimal(18,2) NOT NULL;
ALTER TABLE [KurDegeri] ADD DEFAULT 0.0 FOR [AlisDegeri];

ALTER TABLE [KurDegeri] ADD [Silindi] bit NOT NULL DEFAULT CAST(0 AS bit);

ALTER TABLE [ParaBirimi] ADD CONSTRAINT [PK_ParaBirimi] PRIMARY KEY ([ParaBirimiID]);

ALTER TABLE [KurDegeri] ADD CONSTRAINT [PK_KurDegeri] PRIMARY KEY ([KurDegeriID]);

CREATE TABLE [ParaBirimiIliski] (
    [ParaBirimiIliskiID] uniqueidentifier NOT NULL,
    [KaynakParaBirimiID] uniqueidentifier NOT NULL,
    [HedefParaBirimiID] uniqueidentifier NOT NULL,
    [Carpan] decimal(18,2) NOT NULL,
    [Aktif] bit NOT NULL,
    [OlusturmaTarihi] datetime2 NOT NULL,
    [GuncellemeTarihi] datetime2 NOT NULL,
    [Silindi] bit NOT NULL,
    CONSTRAINT [PK_ParaBirimiIliski] PRIMARY KEY ([ParaBirimiIliskiID]),
    CONSTRAINT [FK_ParaBirimiIliski_ParaBirimi_HedefParaBirimiID] FOREIGN KEY ([HedefParaBirimiID]) REFERENCES [ParaBirimi] ([ParaBirimiID]) ON DELETE NO ACTION,
    CONSTRAINT [FK_ParaBirimiIliski_ParaBirimi_KaynakParaBirimiID] FOREIGN KEY ([KaynakParaBirimiID]) REFERENCES [ParaBirimi] ([ParaBirimiID]) ON DELETE NO ACTION
);

CREATE INDEX [IX_StokHareketleri_DepoID1] ON [StokHareketleri] ([DepoID1]);

CREATE INDEX [IX_StokHareketleri_UrunID1] ON [StokHareketleri] ([UrunID1]);

CREATE INDEX [IX_SistemAyarlari_AnaDovizID] ON [SistemAyarlari] ([AnaDovizID]);

CREATE INDEX [IX_FaturaDetaylari_UrunID1] ON [FaturaDetaylari] ([UrunID1]);

CREATE INDEX [IX_BankaHareketleri_BankaID1] ON [BankaHareketleri] ([BankaID1]);

CREATE INDEX [IX_ParaBirimiIliski_HedefParaBirimiID] ON [ParaBirimiIliski] ([HedefParaBirimiID]);

CREATE INDEX [IX_ParaBirimiIliski_KaynakParaBirimiID] ON [ParaBirimiIliski] ([KaynakParaBirimiID]);

ALTER TABLE [BankaHareketleri] ADD CONSTRAINT [FK_BankaHareketleri_Bankalar_BankaID] FOREIGN KEY ([BankaID]) REFERENCES [Bankalar] ([BankaID]) ON DELETE NO ACTION;

ALTER TABLE [BankaHareketleri] ADD CONSTRAINT [FK_BankaHareketleri_Bankalar_BankaID1] FOREIGN KEY ([BankaID1]) REFERENCES [Bankalar] ([BankaID]);

ALTER TABLE [BankaHareketleri] ADD CONSTRAINT [FK_BankaHareketleri_Cariler_CariID] FOREIGN KEY ([CariID]) REFERENCES [Cariler] ([CariID]) ON DELETE NO ACTION;

ALTER TABLE [FaturaDetaylari] ADD CONSTRAINT [FK_FaturaDetaylari_Urunler_UrunID] FOREIGN KEY ([UrunID]) REFERENCES [Urunler] ([UrunID]) ON DELETE NO ACTION;

ALTER TABLE [FaturaDetaylari] ADD CONSTRAINT [FK_FaturaDetaylari_Urunler_UrunID1] FOREIGN KEY ([UrunID1]) REFERENCES [Urunler] ([UrunID]);

ALTER TABLE [Faturalar] ADD CONSTRAINT [FK_Faturalar_Cariler_CariID] FOREIGN KEY ([CariID]) REFERENCES [Cariler] ([CariID]) ON DELETE NO ACTION;

ALTER TABLE [Faturalar] ADD CONSTRAINT [FK_Faturalar_FaturaTurleri_FaturaTuruID] FOREIGN KEY ([FaturaTuruID]) REFERENCES [FaturaTurleri] ([FaturaTuruID]) ON DELETE NO ACTION;

ALTER TABLE [IrsaliyeDetaylari] ADD CONSTRAINT [FK_IrsaliyeDetaylari_Urunler_UrunID] FOREIGN KEY ([UrunID]) REFERENCES [Urunler] ([UrunID]) ON DELETE NO ACTION;

ALTER TABLE [Irsaliyeler] ADD CONSTRAINT [FK_Irsaliyeler_Cariler_CariID] FOREIGN KEY ([CariID]) REFERENCES [Cariler] ([CariID]) ON DELETE NO ACTION;

ALTER TABLE [Irsaliyeler] ADD CONSTRAINT [FK_Irsaliyeler_Faturalar_FaturaID] FOREIGN KEY ([FaturaID]) REFERENCES [Faturalar] ([FaturaID]) ON DELETE NO ACTION;

ALTER TABLE [Irsaliyeler] ADD CONSTRAINT [FK_Irsaliyeler_IrsaliyeTurleri_IrsaliyeTuruID] FOREIGN KEY ([IrsaliyeTuruID]) REFERENCES [IrsaliyeTurleri] ([IrsaliyeTuruID]) ON DELETE NO ACTION;

ALTER TABLE [KasaHareketleri] ADD CONSTRAINT [FK_KasaHareketleri_Bankalar_HedefBankaID] FOREIGN KEY ([HedefBankaID]) REFERENCES [Bankalar] ([BankaID]) ON DELETE NO ACTION;

ALTER TABLE [KasaHareketleri] ADD CONSTRAINT [FK_KasaHareketleri_Bankalar_KaynakBankaID] FOREIGN KEY ([KaynakBankaID]) REFERENCES [Bankalar] ([BankaID]) ON DELETE NO ACTION;

ALTER TABLE [KasaHareketleri] ADD CONSTRAINT [FK_KasaHareketleri_Cariler_CariID] FOREIGN KEY ([CariID]) REFERENCES [Cariler] ([CariID]) ON DELETE NO ACTION;

ALTER TABLE [KasaHareketleri] ADD CONSTRAINT [FK_KasaHareketleri_Kasalar_HedefKasaID] FOREIGN KEY ([HedefKasaID]) REFERENCES [Kasalar] ([KasaID]) ON DELETE NO ACTION;

ALTER TABLE [KasaHareketleri] ADD CONSTRAINT [FK_KasaHareketleri_Kasalar_KasaID] FOREIGN KEY ([KasaID]) REFERENCES [Kasalar] ([KasaID]) ON DELETE NO ACTION;

ALTER TABLE [KurDegeri] ADD CONSTRAINT [FK_KurDegeri_ParaBirimi_ParaBirimiID] FOREIGN KEY ([ParaBirimiID]) REFERENCES [ParaBirimi] ([ParaBirimiID]) ON DELETE NO ACTION;

ALTER TABLE [SistemAyarlari] ADD CONSTRAINT [FK_SistemAyarlari_ParaBirimi_AnaDovizID] FOREIGN KEY ([AnaDovizID]) REFERENCES [ParaBirimi] ([ParaBirimiID]) ON DELETE CASCADE;

ALTER TABLE [SistemAyarlari] ADD CONSTRAINT [FK_SistemAyarlari_ParaBirimi_IkinciDovizID] FOREIGN KEY ([IkinciDovizID]) REFERENCES [ParaBirimi] ([ParaBirimiID]);

ALTER TABLE [SistemAyarlari] ADD CONSTRAINT [FK_SistemAyarlari_ParaBirimi_UcuncuDovizID] FOREIGN KEY ([UcuncuDovizID]) REFERENCES [ParaBirimi] ([ParaBirimiID]);

ALTER TABLE [StokFifo] ADD CONSTRAINT [FK_StokFifo_Urunler_UrunID] FOREIGN KEY ([UrunID]) REFERENCES [Urunler] ([UrunID]) ON DELETE NO ACTION;

ALTER TABLE [StokHareketleri] ADD CONSTRAINT [FK_StokHareketleri_Depolar_DepoID] FOREIGN KEY ([DepoID]) REFERENCES [Depolar] ([DepoID]) ON DELETE NO ACTION;

ALTER TABLE [StokHareketleri] ADD CONSTRAINT [FK_StokHareketleri_Depolar_DepoID1] FOREIGN KEY ([DepoID1]) REFERENCES [Depolar] ([DepoID]);

ALTER TABLE [StokHareketleri] ADD CONSTRAINT [FK_StokHareketleri_Urunler_UrunID] FOREIGN KEY ([UrunID]) REFERENCES [Urunler] ([UrunID]) ON DELETE NO ACTION;

ALTER TABLE [StokHareketleri] ADD CONSTRAINT [FK_StokHareketleri_Urunler_UrunID1] FOREIGN KEY ([UrunID1]) REFERENCES [Urunler] ([UrunID]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250319070020_FixModelRelationshipsAndForeignKeys', N'9.0.3');

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250319073852_FixParaBirimiTable', N'9.0.3');

ALTER TABLE [BankaHareketleri] DROP CONSTRAINT [FK_BankaHareketleri_Bankalar_BankaID];

ALTER TABLE [BankaHareketleri] DROP CONSTRAINT [FK_BankaHareketleri_Bankalar_BankaID1];

ALTER TABLE [BankaHareketleri] DROP CONSTRAINT [FK_BankaHareketleri_Cariler_CariID];

ALTER TABLE [FaturaDetaylari] DROP CONSTRAINT [FK_FaturaDetaylari_Urunler_UrunID];

ALTER TABLE [FaturaDetaylari] DROP CONSTRAINT [FK_FaturaDetaylari_Urunler_UrunID1];

ALTER TABLE [Faturalar] DROP CONSTRAINT [FK_Faturalar_Cariler_CariID];

ALTER TABLE [Faturalar] DROP CONSTRAINT [FK_Faturalar_FaturaTurleri_FaturaTuruID];

ALTER TABLE [IrsaliyeDetaylari] DROP CONSTRAINT [FK_IrsaliyeDetaylari_Urunler_UrunID];

ALTER TABLE [Irsaliyeler] DROP CONSTRAINT [FK_Irsaliyeler_Cariler_CariID];

ALTER TABLE [Irsaliyeler] DROP CONSTRAINT [FK_Irsaliyeler_Faturalar_FaturaID];

ALTER TABLE [Irsaliyeler] DROP CONSTRAINT [FK_Irsaliyeler_IrsaliyeTurleri_IrsaliyeTuruID];

ALTER TABLE [KasaHareketleri] DROP CONSTRAINT [FK_KasaHareketleri_Bankalar_HedefBankaID];

ALTER TABLE [KasaHareketleri] DROP CONSTRAINT [FK_KasaHareketleri_Bankalar_KaynakBankaID];

ALTER TABLE [KasaHareketleri] DROP CONSTRAINT [FK_KasaHareketleri_Cariler_CariID];

ALTER TABLE [KasaHareketleri] DROP CONSTRAINT [FK_KasaHareketleri_Kasalar_HedefKasaID];

ALTER TABLE [KasaHareketleri] DROP CONSTRAINT [FK_KasaHareketleri_Kasalar_KasaID];

ALTER TABLE [KurDegeri] DROP CONSTRAINT [FK_KurDegeri_ParaBirimi_ParaBirimiID];

ALTER TABLE [ParaBirimiIliski] DROP CONSTRAINT [FK_ParaBirimiIliski_ParaBirimi_HedefParaBirimiID];

ALTER TABLE [ParaBirimiIliski] DROP CONSTRAINT [FK_ParaBirimiIliski_ParaBirimi_KaynakParaBirimiID];

ALTER TABLE [SistemAyarlari] DROP CONSTRAINT [FK_SistemAyarlari_ParaBirimi_AnaDovizID];

ALTER TABLE [SistemAyarlari] DROP CONSTRAINT [FK_SistemAyarlari_ParaBirimi_IkinciDovizID];

ALTER TABLE [SistemAyarlari] DROP CONSTRAINT [FK_SistemAyarlari_ParaBirimi_UcuncuDovizID];

ALTER TABLE [StokFifo] DROP CONSTRAINT [FK_StokFifo_Urunler_UrunID];

ALTER TABLE [StokHareketleri] DROP CONSTRAINT [FK_StokHareketleri_Depolar_DepoID];

ALTER TABLE [StokHareketleri] DROP CONSTRAINT [FK_StokHareketleri_Depolar_DepoID1];

ALTER TABLE [StokHareketleri] DROP CONSTRAINT [FK_StokHareketleri_Urunler_UrunID];

ALTER TABLE [StokHareketleri] DROP CONSTRAINT [FK_StokHareketleri_Urunler_UrunID1];

DROP INDEX [IX_StokHareketleri_DepoID1] ON [StokHareketleri];

DROP INDEX [IX_StokHareketleri_UrunID1] ON [StokHareketleri];

DROP INDEX [IX_SistemAyarlari_AnaDovizID] ON [SistemAyarlari];

DROP INDEX [IX_SistemAyarlari_IkinciDovizID] ON [SistemAyarlari];

DROP INDEX [IX_SistemAyarlari_UcuncuDovizID] ON [SistemAyarlari];

DROP INDEX [IX_FaturaDetaylari_UrunID1] ON [FaturaDetaylari];

DROP INDEX [IX_BankaHareketleri_BankaID1] ON [BankaHareketleri];

DECLARE @var59 sysname;
SELECT @var59 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[StokHareketleri]') AND [c].[name] = N'DepoID1');
IF @var59 IS NOT NULL EXEC(N'ALTER TABLE [StokHareketleri] DROP CONSTRAINT [' + @var59 + '];');
ALTER TABLE [StokHareketleri] DROP COLUMN [DepoID1];

DECLARE @var60 sysname;
SELECT @var60 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[StokHareketleri]') AND [c].[name] = N'UrunID1');
IF @var60 IS NOT NULL EXEC(N'ALTER TABLE [StokHareketleri] DROP CONSTRAINT [' + @var60 + '];');
ALTER TABLE [StokHareketleri] DROP COLUMN [UrunID1];

DECLARE @var61 sysname;
SELECT @var61 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[SistemLoglar]') AND [c].[name] = N'IlgiliID');
IF @var61 IS NOT NULL EXEC(N'ALTER TABLE [SistemLoglar] DROP CONSTRAINT [' + @var61 + '];');
ALTER TABLE [SistemLoglar] DROP COLUMN [IlgiliID];

DECLARE @var62 sysname;
SELECT @var62 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[SistemLoglar]') AND [c].[name] = N'SoftDelete');
IF @var62 IS NOT NULL EXEC(N'ALTER TABLE [SistemLoglar] DROP CONSTRAINT [' + @var62 + '];');
ALTER TABLE [SistemLoglar] DROP COLUMN [SoftDelete];

DECLARE @var63 sysname;
SELECT @var63 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[SistemLoglar]') AND [c].[name] = N'Tarayici');
IF @var63 IS NOT NULL EXEC(N'ALTER TABLE [SistemLoglar] DROP CONSTRAINT [' + @var63 + '];');
ALTER TABLE [SistemLoglar] DROP COLUMN [Tarayici];

DECLARE @var64 sysname;
SELECT @var64 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[SistemAyarlari]') AND [c].[name] = N'AnaDovizID');
IF @var64 IS NOT NULL EXEC(N'ALTER TABLE [SistemAyarlari] DROP CONSTRAINT [' + @var64 + '];');
ALTER TABLE [SistemAyarlari] DROP COLUMN [AnaDovizID];

DECLARE @var65 sysname;
SELECT @var65 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[SistemAyarlari]') AND [c].[name] = N'IkinciDovizID');
IF @var65 IS NOT NULL EXEC(N'ALTER TABLE [SistemAyarlari] DROP CONSTRAINT [' + @var65 + '];');
ALTER TABLE [SistemAyarlari] DROP COLUMN [IkinciDovizID];

DECLARE @var66 sysname;
SELECT @var66 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[SistemAyarlari]') AND [c].[name] = N'IkinciDovizKodu');
IF @var66 IS NOT NULL EXEC(N'ALTER TABLE [SistemAyarlari] DROP CONSTRAINT [' + @var66 + '];');
ALTER TABLE [SistemAyarlari] DROP COLUMN [IkinciDovizKodu];

DECLARE @var67 sysname;
SELECT @var67 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[SistemAyarlari]') AND [c].[name] = N'UcuncuDovizID');
IF @var67 IS NOT NULL EXEC(N'ALTER TABLE [SistemAyarlari] DROP CONSTRAINT [' + @var67 + '];');
ALTER TABLE [SistemAyarlari] DROP COLUMN [UcuncuDovizID];

DECLARE @var68 sysname;
SELECT @var68 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[SistemAyarlari]') AND [c].[name] = N'UcuncuDovizKodu');
IF @var68 IS NOT NULL EXEC(N'ALTER TABLE [SistemAyarlari] DROP CONSTRAINT [' + @var68 + '];');
ALTER TABLE [SistemAyarlari] DROP COLUMN [UcuncuDovizKodu];

DECLARE @var69 sysname;
SELECT @var69 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[FaturaDetaylari]') AND [c].[name] = N'UrunID1');
IF @var69 IS NOT NULL EXEC(N'ALTER TABLE [FaturaDetaylari] DROP CONSTRAINT [' + @var69 + '];');
ALTER TABLE [FaturaDetaylari] DROP COLUMN [UrunID1];

DECLARE @var70 sysname;
SELECT @var70 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[BankaHareketleri]') AND [c].[name] = N'BankaID1');
IF @var70 IS NOT NULL EXEC(N'ALTER TABLE [BankaHareketleri] DROP CONSTRAINT [' + @var70 + '];');
ALTER TABLE [BankaHareketleri] DROP COLUMN [BankaID1];

EXEC sp_rename N'[SistemAyarlari].[Silindi]', N'SoftDelete', 'COLUMN';

DECLARE @var71 sysname;
SELECT @var71 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Urunler]') AND [c].[name] = N'StokMiktar');
IF @var71 IS NOT NULL EXEC(N'ALTER TABLE [Urunler] DROP CONSTRAINT [' + @var71 + '];');
ALTER TABLE [Urunler] ADD DEFAULT 0.0 FOR [StokMiktar];

DECLARE @var72 sysname;
SELECT @var72 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Urunler]') AND [c].[name] = N'SoftDelete');
IF @var72 IS NOT NULL EXEC(N'ALTER TABLE [Urunler] DROP CONSTRAINT [' + @var72 + '];');
ALTER TABLE [Urunler] ADD DEFAULT CAST(0 AS bit) FOR [SoftDelete];

DECLARE @var73 sysname;
SELECT @var73 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Urunler]') AND [c].[name] = N'Aktif');
IF @var73 IS NOT NULL EXEC(N'ALTER TABLE [Urunler] DROP CONSTRAINT [' + @var73 + '];');
ALTER TABLE [Urunler] ADD DEFAULT CAST(1 AS bit) FOR [Aktif];

DECLARE @var74 sysname;
SELECT @var74 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[UrunKategorileri]') AND [c].[name] = N'SoftDelete');
IF @var74 IS NOT NULL EXEC(N'ALTER TABLE [UrunKategorileri] DROP CONSTRAINT [' + @var74 + '];');
ALTER TABLE [UrunKategorileri] ADD DEFAULT CAST(0 AS bit) FOR [SoftDelete];

DECLARE @var75 sysname;
SELECT @var75 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[UrunKategorileri]') AND [c].[name] = N'Aktif');
IF @var75 IS NOT NULL EXEC(N'ALTER TABLE [UrunKategorileri] DROP CONSTRAINT [' + @var75 + '];');
ALTER TABLE [UrunKategorileri] ADD DEFAULT CAST(1 AS bit) FOR [Aktif];

DECLARE @var76 sysname;
SELECT @var76 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[SistemLoglar]') AND [c].[name] = N'TabloAdi');
IF @var76 IS NOT NULL EXEC(N'ALTER TABLE [SistemLoglar] DROP CONSTRAINT [' + @var76 + '];');
ALTER TABLE [SistemLoglar] ALTER COLUMN [TabloAdi] nvarchar(100) NOT NULL;

DECLARE @var77 sysname;
SELECT @var77 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[SistemLoglar]') AND [c].[name] = N'KullaniciID');
IF @var77 IS NOT NULL EXEC(N'ALTER TABLE [SistemLoglar] DROP CONSTRAINT [' + @var77 + '];');
ALTER TABLE [SistemLoglar] ALTER COLUMN [KullaniciID] uniqueidentifier NULL;

DECLARE @var78 sysname;
SELECT @var78 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[SistemLoglar]') AND [c].[name] = N'KullaniciAdi');
IF @var78 IS NOT NULL EXEC(N'ALTER TABLE [SistemLoglar] DROP CONSTRAINT [' + @var78 + '];');
ALTER TABLE [SistemLoglar] ALTER COLUMN [KullaniciAdi] nvarchar(100) NOT NULL;

DECLARE @var79 sysname;
SELECT @var79 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[SistemLoglar]') AND [c].[name] = N'KayitAdi');
IF @var79 IS NOT NULL EXEC(N'ALTER TABLE [SistemLoglar] DROP CONSTRAINT [' + @var79 + '];');
ALTER TABLE [SistemLoglar] ALTER COLUMN [KayitAdi] nvarchar(200) NOT NULL;

DECLARE @var80 sysname;
SELECT @var80 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[SistemLoglar]') AND [c].[name] = N'HataMesaji');
IF @var80 IS NOT NULL EXEC(N'ALTER TABLE [SistemLoglar] DROP CONSTRAINT [' + @var80 + '];');
UPDATE [SistemLoglar] SET [HataMesaji] = N'' WHERE [HataMesaji] IS NULL;
ALTER TABLE [SistemLoglar] ALTER COLUMN [HataMesaji] nvarchar(500) NOT NULL;
ALTER TABLE [SistemLoglar] ADD DEFAULT N'' FOR [HataMesaji];

ALTER TABLE [SistemLoglar] ADD [LogTuru] int NOT NULL DEFAULT 0;

DECLARE @var81 sysname;
SELECT @var81 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[SistemAyarlari]') AND [c].[name] = N'SirketVergiDairesi');
IF @var81 IS NOT NULL EXEC(N'ALTER TABLE [SistemAyarlari] DROP CONSTRAINT [' + @var81 + '];');
ALTER TABLE [SistemAyarlari] ALTER COLUMN [SirketVergiDairesi] nvarchar(20) NOT NULL;

DECLARE @var82 sysname;
SELECT @var82 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[SistemAyarlari]') AND [c].[name] = N'SirketAdresi');
IF @var82 IS NOT NULL EXEC(N'ALTER TABLE [SistemAyarlari] DROP CONSTRAINT [' + @var82 + '];');
ALTER TABLE [SistemAyarlari] ALTER COLUMN [SirketAdresi] nvarchar(100) NOT NULL;

DECLARE @var83 sysname;
SELECT @var83 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[SistemAyarlari]') AND [c].[name] = N'SirketAdi');
IF @var83 IS NOT NULL EXEC(N'ALTER TABLE [SistemAyarlari] DROP CONSTRAINT [' + @var83 + '];');
ALTER TABLE [SistemAyarlari] ALTER COLUMN [SirketAdi] nvarchar(50) NOT NULL;

DECLARE @var84 sysname;
SELECT @var84 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[SistemAyarlari]') AND [c].[name] = N'GuncellemeTarihi');
IF @var84 IS NOT NULL EXEC(N'ALTER TABLE [SistemAyarlari] DROP CONSTRAINT [' + @var84 + '];');
ALTER TABLE [SistemAyarlari] ALTER COLUMN [GuncellemeTarihi] datetime2 NULL;

DECLARE @var85 sysname;
SELECT @var85 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[SistemAyarlari]') AND [c].[name] = N'AnaDovizKodu');
IF @var85 IS NOT NULL EXEC(N'ALTER TABLE [SistemAyarlari] DROP CONSTRAINT [' + @var85 + '];');
ALTER TABLE [SistemAyarlari] ALTER COLUMN [AnaDovizKodu] nvarchar(10) NOT NULL;


                -- Geçici tablo oluştur
                CREATE TABLE #TempSistemAyarlari (
                    TempID INT IDENTITY(1,1) PRIMARY KEY,
                    AnaDovizKodu NVARCHAR(10) NOT NULL,
                    SirketAdi NVARCHAR(50) NOT NULL,
                    SirketAdresi NVARCHAR(100) NOT NULL,
                    SirketEmail NVARCHAR(100) NOT NULL,
                    SirketTelefon NVARCHAR(20) NOT NULL,
                    SirketVergiDairesi NVARCHAR(20) NOT NULL,
                    SirketVergiNo NVARCHAR(20) NOT NULL,
                    SonDovizGuncellemeTarihi DATETIME2 NOT NULL,
                    GuncellemeTarihi DATETIME2 NULL,
                    SoftDelete BIT NOT NULL
                );

                -- Mevcut verileri geçici tabloya kopyala
                INSERT INTO #TempSistemAyarlari (
                    AnaDovizKodu, SirketAdi, SirketAdresi, SirketEmail, 
                    SirketTelefon, SirketVergiDairesi, SirketVergiNo,
                    SonDovizGuncellemeTarihi, GuncellemeTarihi, SoftDelete
                )
                SELECT 
                    AnaDovizKodu, SirketAdi, SirketAdresi, SirketEmail, 
                    SirketTelefon, SirketVergiDairesi, SirketVergiNo,
                    SonDovizGuncellemeTarihi, GuncellemeTarihi, SoftDelete
                FROM SistemAyarlari;

                -- PK constraint'i kaldır
                ALTER TABLE SistemAyarlari DROP CONSTRAINT PK_SistemAyarlari;

                -- SistemAyarlariID kolonunu SistemAyarlari tablosundan sil
                ALTER TABLE SistemAyarlari DROP COLUMN SistemAyarlariID;

                -- Yeni IDENTITY özelliğine sahip kolon ekle
                ALTER TABLE SistemAyarlari ADD SistemAyarlariID INT IDENTITY(1,1) NOT NULL;

                -- PK constraint'i yeniden ekle
                ALTER TABLE SistemAyarlari ADD CONSTRAINT PK_SistemAyarlari PRIMARY KEY (SistemAyarlariID);

                -- Geçici tabloyu sil
                DROP TABLE #TempSistemAyarlari;
            

ALTER TABLE [SistemAyarlari] ADD [AktifParaBirimleri] nvarchar(500) NULL;

ALTER TABLE [ParaBirimi] ADD [Aciklama] nvarchar(250) NOT NULL DEFAULT N'';

ALTER TABLE [ParaBirimi] ADD [Format] nvarchar(50) NOT NULL DEFAULT N'';

ALTER TABLE [ParaBirimi] ADD [Sira] int NOT NULL DEFAULT 0;

DECLARE @var86 sysname;
SELECT @var86 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Kasalar]') AND [c].[name] = N'SoftDelete');
IF @var86 IS NOT NULL EXEC(N'ALTER TABLE [Kasalar] DROP CONSTRAINT [' + @var86 + '];');
ALTER TABLE [Kasalar] ADD DEFAULT CAST(0 AS bit) FOR [SoftDelete];

DECLARE @var87 sysname;
SELECT @var87 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Kasalar]') AND [c].[name] = N'GuncelBakiye');
IF @var87 IS NOT NULL EXEC(N'ALTER TABLE [Kasalar] DROP CONSTRAINT [' + @var87 + '];');
ALTER TABLE [Kasalar] ADD DEFAULT 0.0 FOR [GuncelBakiye];

DECLARE @var88 sysname;
SELECT @var88 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Kasalar]') AND [c].[name] = N'Aktif');
IF @var88 IS NOT NULL EXEC(N'ALTER TABLE [Kasalar] DROP CONSTRAINT [' + @var88 + '];');
ALTER TABLE [Kasalar] ADD DEFAULT CAST(1 AS bit) FOR [Aktif];

DECLARE @var89 sysname;
SELECT @var89 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Kasalar]') AND [c].[name] = N'AcilisBakiye');
IF @var89 IS NOT NULL EXEC(N'ALTER TABLE [Kasalar] DROP CONSTRAINT [' + @var89 + '];');
ALTER TABLE [Kasalar] ADD DEFAULT 0.0 FOR [AcilisBakiye];

DECLARE @var90 sysname;
SELECT @var90 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[KasaHareketleri]') AND [c].[name] = N'SoftDelete');
IF @var90 IS NOT NULL EXEC(N'ALTER TABLE [KasaHareketleri] DROP CONSTRAINT [' + @var90 + '];');
ALTER TABLE [KasaHareketleri] ADD DEFAULT CAST(0 AS bit) FOR [SoftDelete];

DECLARE @var91 sysname;
SELECT @var91 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Irsaliyeler]') AND [c].[name] = N'SoftDelete');
IF @var91 IS NOT NULL EXEC(N'ALTER TABLE [Irsaliyeler] DROP CONSTRAINT [' + @var91 + '];');
ALTER TABLE [Irsaliyeler] ADD DEFAULT CAST(0 AS bit) FOR [SoftDelete];

DECLARE @var92 sysname;
SELECT @var92 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Irsaliyeler]') AND [c].[name] = N'Resmi');
IF @var92 IS NOT NULL EXEC(N'ALTER TABLE [Irsaliyeler] DROP CONSTRAINT [' + @var92 + '];');
ALTER TABLE [Irsaliyeler] ADD DEFAULT CAST(1 AS bit) FOR [Resmi];

DECLARE @var93 sysname;
SELECT @var93 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Faturalar]') AND [c].[name] = N'SoftDelete');
IF @var93 IS NOT NULL EXEC(N'ALTER TABLE [Faturalar] DROP CONSTRAINT [' + @var93 + '];');
ALTER TABLE [Faturalar] ADD DEFAULT CAST(0 AS bit) FOR [SoftDelete];

DECLARE @var94 sysname;
SELECT @var94 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Faturalar]') AND [c].[name] = N'Resmi');
IF @var94 IS NOT NULL EXEC(N'ALTER TABLE [Faturalar] DROP CONSTRAINT [' + @var94 + '];');
ALTER TABLE [Faturalar] ADD DEFAULT CAST(1 AS bit) FOR [Resmi];

DECLARE @var95 sysname;
SELECT @var95 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Faturalar]') AND [c].[name] = N'Aktif');
IF @var95 IS NOT NULL EXEC(N'ALTER TABLE [Faturalar] DROP CONSTRAINT [' + @var95 + '];');
ALTER TABLE [Faturalar] ADD DEFAULT CAST(1 AS bit) FOR [Aktif];

DECLARE @var96 sysname;
SELECT @var96 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Depolar]') AND [c].[name] = N'SoftDelete');
IF @var96 IS NOT NULL EXEC(N'ALTER TABLE [Depolar] DROP CONSTRAINT [' + @var96 + '];');
ALTER TABLE [Depolar] ADD DEFAULT CAST(0 AS bit) FOR [SoftDelete];

DECLARE @var97 sysname;
SELECT @var97 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Depolar]') AND [c].[name] = N'Aktif');
IF @var97 IS NOT NULL EXEC(N'ALTER TABLE [Depolar] DROP CONSTRAINT [' + @var97 + '];');
ALTER TABLE [Depolar] ADD DEFAULT CAST(1 AS bit) FOR [Aktif];

DECLARE @var98 sysname;
SELECT @var98 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Cariler]') AND [c].[name] = N'SoftDelete');
IF @var98 IS NOT NULL EXEC(N'ALTER TABLE [Cariler] DROP CONSTRAINT [' + @var98 + '];');
ALTER TABLE [Cariler] ADD DEFAULT CAST(0 AS bit) FOR [SoftDelete];

DECLARE @var99 sysname;
SELECT @var99 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Cariler]') AND [c].[name] = N'Aktif');
IF @var99 IS NOT NULL EXEC(N'ALTER TABLE [Cariler] DROP CONSTRAINT [' + @var99 + '];');
ALTER TABLE [Cariler] ADD DEFAULT CAST(1 AS bit) FOR [Aktif];

DECLARE @var100 sysname;
SELECT @var100 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Bankalar]') AND [c].[name] = N'SoftDelete');
IF @var100 IS NOT NULL EXEC(N'ALTER TABLE [Bankalar] DROP CONSTRAINT [' + @var100 + '];');
ALTER TABLE [Bankalar] ADD DEFAULT CAST(0 AS bit) FOR [SoftDelete];

DECLARE @var101 sysname;
SELECT @var101 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Bankalar]') AND [c].[name] = N'ParaBirimi');
IF @var101 IS NOT NULL EXEC(N'ALTER TABLE [Bankalar] DROP CONSTRAINT [' + @var101 + '];');
ALTER TABLE [Bankalar] ADD DEFAULT N'TRY' FOR [ParaBirimi];

DECLARE @var102 sysname;
SELECT @var102 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Bankalar]') AND [c].[name] = N'GuncelBakiye');
IF @var102 IS NOT NULL EXEC(N'ALTER TABLE [Bankalar] DROP CONSTRAINT [' + @var102 + '];');
ALTER TABLE [Bankalar] ADD DEFAULT 0.0 FOR [GuncelBakiye];

DECLARE @var103 sysname;
SELECT @var103 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Bankalar]') AND [c].[name] = N'Aktif');
IF @var103 IS NOT NULL EXEC(N'ALTER TABLE [Bankalar] DROP CONSTRAINT [' + @var103 + '];');
ALTER TABLE [Bankalar] ADD DEFAULT CAST(1 AS bit) FOR [Aktif];

DECLARE @var104 sysname;
SELECT @var104 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Bankalar]') AND [c].[name] = N'AcilisBakiye');
IF @var104 IS NOT NULL EXEC(N'ALTER TABLE [Bankalar] DROP CONSTRAINT [' + @var104 + '];');
ALTER TABLE [Bankalar] ADD DEFAULT 0.0 FOR [AcilisBakiye];

DECLARE @var105 sysname;
SELECT @var105 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[BankaHareketleri]') AND [c].[name] = N'SoftDelete');
IF @var105 IS NOT NULL EXEC(N'ALTER TABLE [BankaHareketleri] DROP CONSTRAINT [' + @var105 + '];');
ALTER TABLE [BankaHareketleri] ADD DEFAULT CAST(0 AS bit) FOR [SoftDelete];

CREATE TABLE [DovizKurlari] (
    [ID] int NOT NULL IDENTITY,
    [ParaBirimi] nvarchar(10) NOT NULL,
    [BazParaBirimi] nvarchar(10) NOT NULL,
    [Alis] decimal(18,6) NOT NULL,
    [Satis] decimal(18,6) NOT NULL,
    [EfektifAlis] decimal(18,6) NOT NULL,
    [EfektifSatis] decimal(18,6) NOT NULL,
    [Tarih] datetime2 NOT NULL,
    [Kaynak] nvarchar(100) NOT NULL,
    [Aciklama] nvarchar(500) NOT NULL,
    [Aktif] bit NOT NULL DEFAULT CAST(1 AS bit),
    [SoftDelete] bit NOT NULL DEFAULT CAST(0 AS bit),
    [OlusturmaTarihi] datetime2 NOT NULL,
    [GuncellemeTarihi] datetime2 NULL,
    CONSTRAINT [PK_DovizKurlari] PRIMARY KEY ([ID])
);

CREATE INDEX [IX_StokFifo_GirisTarihi] ON [StokFifo] ([GirisTarihi]);

CREATE INDEX [IX_StokFifo_Referans] ON [StokFifo] ([ReferansID], [ReferansTuru]);

CREATE INDEX [IX_StokFifo_StokSorgu] ON [StokFifo] ([UrunID], [KalanMiktar], [Aktif], [SoftDelete], [Iptal]);

ALTER TABLE [BankaHareketleri] ADD CONSTRAINT [FK_BankaHareketleri_Bankalar_BankaID] FOREIGN KEY ([BankaID]) REFERENCES [Bankalar] ([BankaID]) ON DELETE CASCADE;

ALTER TABLE [BankaHareketleri] ADD CONSTRAINT [FK_BankaHareketleri_Cariler_CariID] FOREIGN KEY ([CariID]) REFERENCES [Cariler] ([CariID]);

ALTER TABLE [FaturaDetaylari] ADD CONSTRAINT [FK_FaturaDetaylari_Urunler_UrunID] FOREIGN KEY ([UrunID]) REFERENCES [Urunler] ([UrunID]) ON DELETE CASCADE;

ALTER TABLE [Faturalar] ADD CONSTRAINT [FK_Faturalar_Cariler_CariID] FOREIGN KEY ([CariID]) REFERENCES [Cariler] ([CariID]);

ALTER TABLE [Faturalar] ADD CONSTRAINT [FK_Faturalar_FaturaTurleri_FaturaTuruID] FOREIGN KEY ([FaturaTuruID]) REFERENCES [FaturaTurleri] ([FaturaTuruID]);

ALTER TABLE [IrsaliyeDetaylari] ADD CONSTRAINT [FK_IrsaliyeDetaylari_Urunler_UrunID] FOREIGN KEY ([UrunID]) REFERENCES [Urunler] ([UrunID]) ON DELETE CASCADE;

ALTER TABLE [Irsaliyeler] ADD CONSTRAINT [FK_Irsaliyeler_Cariler_CariID] FOREIGN KEY ([CariID]) REFERENCES [Cariler] ([CariID]) ON DELETE CASCADE;

ALTER TABLE [Irsaliyeler] ADD CONSTRAINT [FK_Irsaliyeler_Faturalar_FaturaID] FOREIGN KEY ([FaturaID]) REFERENCES [Faturalar] ([FaturaID]);

ALTER TABLE [Irsaliyeler] ADD CONSTRAINT [FK_Irsaliyeler_IrsaliyeTurleri_IrsaliyeTuruID] FOREIGN KEY ([IrsaliyeTuruID]) REFERENCES [IrsaliyeTurleri] ([IrsaliyeTuruID]);

ALTER TABLE [KasaHareketleri] ADD CONSTRAINT [FK_KasaHareketleri_Bankalar_HedefBankaID] FOREIGN KEY ([HedefBankaID]) REFERENCES [Bankalar] ([BankaID]);

ALTER TABLE [KasaHareketleri] ADD CONSTRAINT [FK_KasaHareketleri_Bankalar_KaynakBankaID] FOREIGN KEY ([KaynakBankaID]) REFERENCES [Bankalar] ([BankaID]);

ALTER TABLE [KasaHareketleri] ADD CONSTRAINT [FK_KasaHareketleri_Cariler_CariID] FOREIGN KEY ([CariID]) REFERENCES [Cariler] ([CariID]);

ALTER TABLE [KasaHareketleri] ADD CONSTRAINT [FK_KasaHareketleri_Kasalar_HedefKasaID] FOREIGN KEY ([HedefKasaID]) REFERENCES [Kasalar] ([KasaID]);

ALTER TABLE [KasaHareketleri] ADD CONSTRAINT [FK_KasaHareketleri_Kasalar_KasaID] FOREIGN KEY ([KasaID]) REFERENCES [Kasalar] ([KasaID]);

ALTER TABLE [KurDegeri] ADD CONSTRAINT [FK_KurDegeri_ParaBirimi_ParaBirimiID] FOREIGN KEY ([ParaBirimiID]) REFERENCES [ParaBirimi] ([ParaBirimiID]) ON DELETE CASCADE;

ALTER TABLE [ParaBirimiIliski] ADD CONSTRAINT [FK_ParaBirimiIliski_ParaBirimi_HedefParaBirimiID] FOREIGN KEY ([HedefParaBirimiID]) REFERENCES [ParaBirimi] ([ParaBirimiID]) ON DELETE CASCADE;

ALTER TABLE [ParaBirimiIliski] ADD CONSTRAINT [FK_ParaBirimiIliski_ParaBirimi_KaynakParaBirimiID] FOREIGN KEY ([KaynakParaBirimiID]) REFERENCES [ParaBirimi] ([ParaBirimiID]) ON DELETE CASCADE;

ALTER TABLE [StokFifo] ADD CONSTRAINT [FK_StokFifo_Urunler_UrunID] FOREIGN KEY ([UrunID]) REFERENCES [Urunler] ([UrunID]) ON DELETE CASCADE;

ALTER TABLE [StokHareketleri] ADD CONSTRAINT [FK_StokHareketleri_Depolar_DepoID] FOREIGN KEY ([DepoID]) REFERENCES [Depolar] ([DepoID]);

ALTER TABLE [StokHareketleri] ADD CONSTRAINT [FK_StokHareketleri_Urunler_UrunID] FOREIGN KEY ([UrunID]) REFERENCES [Urunler] ([UrunID]) ON DELETE CASCADE;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250319151547_AddMissingColumns', N'9.0.3');

DECLARE @var106 sysname;
SELECT @var106 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Irsaliyeler]') AND [c].[name] = N'GenelToplam');
IF @var106 IS NOT NULL EXEC(N'ALTER TABLE [Irsaliyeler] DROP CONSTRAINT [' + @var106 + '];');
ALTER TABLE [Irsaliyeler] DROP COLUMN [GenelToplam];

DECLARE @var107 sysname;
SELECT @var107 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Irsaliyeler]') AND [c].[name] = N'SevkTarihi');
IF @var107 IS NOT NULL EXEC(N'ALTER TABLE [Irsaliyeler] DROP CONSTRAINT [' + @var107 + '];');
ALTER TABLE [Irsaliyeler] DROP COLUMN [SevkTarihi];

DECLARE @var108 sysname;
SELECT @var108 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[CariHareketler]') AND [c].[name] = N'IslemYapanKullaniciID');
IF @var108 IS NOT NULL EXEC(N'ALTER TABLE [CariHareketler] DROP CONSTRAINT [' + @var108 + '];');
ALTER TABLE [CariHareketler] DROP COLUMN [IslemYapanKullaniciID];

DECLARE @var109 sysname;
SELECT @var109 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[CariHareketler]') AND [c].[name] = N'SonGuncelleyenKullaniciID');
IF @var109 IS NOT NULL EXEC(N'ALTER TABLE [CariHareketler] DROP CONSTRAINT [' + @var109 + '];');
ALTER TABLE [CariHareketler] DROP COLUMN [SonGuncelleyenKullaniciID];

DECLARE @var110 sysname;
SELECT @var110 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[UrunKategorileri]') AND [c].[name] = N'Aciklama');
IF @var110 IS NOT NULL EXEC(N'ALTER TABLE [UrunKategorileri] DROP CONSTRAINT [' + @var110 + '];');
ALTER TABLE [UrunKategorileri] ALTER COLUMN [Aciklama] nvarchar(500) NULL;

DECLARE @var111 sysname;
SELECT @var111 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[StokHareketleri]') AND [c].[name] = N'Aciklama');
IF @var111 IS NOT NULL EXEC(N'ALTER TABLE [StokHareketleri] DROP CONSTRAINT [' + @var111 + '];');
ALTER TABLE [StokHareketleri] ALTER COLUMN [Aciklama] nvarchar(500) NULL;

DECLARE @var112 sysname;
SELECT @var112 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[SistemLoglar]') AND [c].[name] = N'HataMesaji');
IF @var112 IS NOT NULL EXEC(N'ALTER TABLE [SistemLoglar] DROP CONSTRAINT [' + @var112 + '];');
ALTER TABLE [SistemLoglar] ALTER COLUMN [HataMesaji] nvarchar(500) NULL;

DECLARE @var113 sysname;
SELECT @var113 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[SistemLoglar]') AND [c].[name] = N'Aciklama');
IF @var113 IS NOT NULL EXEC(N'ALTER TABLE [SistemLoglar] DROP CONSTRAINT [' + @var113 + '];');
ALTER TABLE [SistemLoglar] ALTER COLUMN [Aciklama] nvarchar(500) NULL;

ALTER TABLE [KurDegeri] ADD [Aciklama] nvarchar(250) NOT NULL DEFAULT N'';

ALTER TABLE [KurDegeri] ADD [VeriKaynagi] nvarchar(50) NOT NULL DEFAULT N'';

DECLARE @var114 sysname;
SELECT @var114 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Irsaliyeler]') AND [c].[name] = N'SoftDelete');
IF @var114 IS NOT NULL EXEC(N'ALTER TABLE [Irsaliyeler] DROP CONSTRAINT [' + @var114 + '];');

ALTER TABLE [Irsaliyeler] ADD [Aktif] bit NOT NULL DEFAULT CAST(1 AS bit);

ALTER TABLE [IrsaliyeDetaylari] ADD [Aktif] bit NOT NULL DEFAULT CAST(0 AS bit);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250319182351_RepairDatabase', N'9.0.3');


                IF NOT EXISTS (
                    SELECT 1 FROM sys.columns 
                    WHERE Name = N'IslemTuru'
                    AND Object_ID = Object_ID(N'SistemLoglar')
                )
                BEGIN
                    ALTER TABLE SistemLoglar
                    ADD IslemTuru NVARCHAR(50) NOT NULL DEFAULT ''
                END
            

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250319182516_AddIslemTuruColumn', N'9.0.3');


                IF NOT EXISTS (
                    SELECT 1 FROM sys.columns 
                    WHERE Name = N'Aciklama'
                    AND Object_ID = Object_ID(N'KurDegeri')
                )
                BEGIN
                    ALTER TABLE KurDegeri
                    ADD Aciklama NVARCHAR(250) NULL
                END
            


                IF NOT EXISTS (
                    SELECT 1 FROM sys.columns 
                    WHERE Name = N'VeriKaynagi'
                    AND Object_ID = Object_ID(N'KurDegeri')
                )
                BEGIN
                    ALTER TABLE KurDegeri
                    ADD VeriKaynagi NVARCHAR(50) NOT NULL DEFAULT 'Kullanıcı'
                END
            

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250319183157_AddVeriKaynagiAndAciklamaToKurDegeri', N'9.0.3');


                IF NOT EXISTS (
                    SELECT 1 FROM sys.columns 
                    WHERE Name = N'IslemTuru'
                    AND Object_ID = Object_ID(N'SistemLoglar')
                )
                BEGIN
                    ALTER TABLE SistemLoglar
                    ADD IslemTuru NVARCHAR(50) NOT NULL DEFAULT ''
                END
            

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250319183319_AddIslemTuruToSistemLoglar', N'9.0.3');

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250319183749_AddIslemTuruToSistemLog', N'9.0.3');

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250319183955_AddIslemTuruToSistemLog2', N'9.0.3');


                -- Geçici isimde bir tablo oluştur
                IF OBJECT_ID('tempdb..#TempSistemAyarlari') IS NOT NULL
                    DROP TABLE #TempSistemAyarlari;

                SELECT * INTO #TempSistemAyarlari FROM SistemAyarlari;

                -- Primary Key constraint kaldır
                IF EXISTS (SELECT * FROM sys.key_constraints WHERE name = 'PK_SistemAyarlari')
                    ALTER TABLE SistemAyarlari DROP CONSTRAINT PK_SistemAyarlari;

                -- Mevcut SistemAyarlariID kolonunu kaldır
                IF EXISTS (SELECT * FROM sys.columns WHERE name = 'SistemAyarlariID' AND object_id = OBJECT_ID('SistemAyarlari'))
                    ALTER TABLE SistemAyarlari DROP COLUMN SistemAyarlariID;

                -- Yeni bir identity kolon ekle
                ALTER TABLE SistemAyarlari ADD SistemAyarlariID INT IDENTITY(1,1) NOT NULL;

                -- Primary Key constraint'i yeniden ekle
                ALTER TABLE SistemAyarlari ADD CONSTRAINT PK_SistemAyarlari PRIMARY KEY (SistemAyarlariID);

                -- Geçici tabloyu temizle
                DROP TABLE #TempSistemAyarlari;
            

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250319184547_FixIdentityColumn', N'9.0.3');

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250319184849_FixParaBirimiIliskiForeignKey', N'9.0.3');

ALTER TABLE [ParaBirimiIliski] DROP CONSTRAINT [FK_ParaBirimiIliski_ParaBirimi_KaynakParaBirimiID];

ALTER TABLE [ParaBirimiIliski] ADD CONSTRAINT [FK_ParaBirimiIliski_ParaBirimi_KaynakParaBirimiID] FOREIGN KEY ([KaynakParaBirimiID]) REFERENCES [ParaBirimi] ([ParaBirimiID]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250319185204_FixParaBirimiIliskiCascadeIssue', N'9.0.3');

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250319185420_AddMissingColumns2', N'9.0.3');

ALTER TABLE [ParaBirimiIliski] ADD CONSTRAINT [FK_ParaBirimiIliski_ParaBirimi_KaynakParaBirimiID] FOREIGN KEY ([KaynakParaBirimiID]) REFERENCES [ParaBirimi] ([ParaBirimiID]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250319185552_FixParaBirimiIliskiFK', N'9.0.3');

ALTER TABLE [ParaBirimiIliski] DROP CONSTRAINT [FK_ParaBirimiIliski_ParaBirimi_HedefParaBirimiID];

ALTER TABLE [ParaBirimiIliski] ADD CONSTRAINT [FK_ParaBirimiIliski_ParaBirimi_KaynakParaBirimiID] FOREIGN KEY ([KaynakParaBirimiID]) REFERENCES [ParaBirimi] ([ParaBirimiID]);

ALTER TABLE [ParaBirimiIliski] ADD CONSTRAINT [FK_ParaBirimiIliski_ParaBirimi_HedefParaBirimiID] FOREIGN KEY ([HedefParaBirimiID]) REFERENCES [ParaBirimi] ([ParaBirimiID]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250319190634_FixForeignKeyIssue', N'9.0.3');

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250319192559_AddIslemTuruToSistemLogFinal', N'9.0.3');

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250319192732_AddIslemTuruToSistemLogOnly', N'9.0.3');

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250320062356_Reinitialize', N'9.0.3');

ALTER TABLE [ParaBirimiIliski] DROP CONSTRAINT [FK_ParaBirimiIliski_ParaBirimi_HedefParaBirimiID];

ALTER TABLE [ParaBirimiIliski] DROP CONSTRAINT [FK_ParaBirimiIliski_ParaBirimi_KaynakParaBirimiID];

ALTER TABLE [ParaBirimiIliski] ADD CONSTRAINT [FK_ParaBirimiIliski_ParaBirimi_HedefParaBirimiID] FOREIGN KEY ([HedefParaBirimiID]) REFERENCES [ParaBirimi] ([ParaBirimiID]);

ALTER TABLE [ParaBirimiIliski] ADD CONSTRAINT [FK_ParaBirimiIliski_ParaBirimi_KaynakParaBirimiID] FOREIGN KEY ([KaynakParaBirimiID]) REFERENCES [ParaBirimi] ([ParaBirimiID]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250320073555_FixMissingColumns', N'9.0.3');

COMMIT;
GO


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
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250403075007_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetRoles] (
        [Id] varchar(128) NOT NULL,
        [Name] nvarchar(256) NULL,
        [NormalizedName] nvarchar(256) NULL,
        [ConcurrencyStamp] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250403075007_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUsers] (
        [Id] varchar(128) NOT NULL,
        [Ad] nvarchar(max) NULL,
        [Soyad] nvarchar(max) NULL,
        [TelefonNo] nvarchar(max) NULL,
        [Adres] nvarchar(200) NULL,
        [Aktif] bit NOT NULL,
        [Silindi] bit NOT NULL DEFAULT CAST(0 AS bit),
        [OlusturmaTarihi] datetime2 NOT NULL,
        [GuncellemeTarihi] datetime2 NULL,
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
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250403075007_InitialCreate'
)
BEGIN
    CREATE TABLE [Bankalar] (
        [BankaID] uniqueidentifier NOT NULL,
        [BankaAdi] nvarchar(100) NOT NULL,
        [SubeAdi] nvarchar(100) NULL,
        [SubeKodu] nvarchar(50) NULL,
        [HesapNo] nvarchar(50) NULL,
        [IBAN] nvarchar(50) NULL,
        [ParaBirimi] nvarchar(10) NULL DEFAULT N'TRY',
        [AcilisBakiye] decimal(18,2) NOT NULL DEFAULT 0.0,
        [GuncelBakiye] decimal(18,2) NOT NULL DEFAULT 0.0,
        [Aciklama] nvarchar(500) NULL,
        [Aktif] bit NOT NULL DEFAULT CAST(1 AS bit),
        [YetkiliKullaniciID] uniqueidentifier NULL,
        [OlusturanKullaniciID] uniqueidentifier NULL,
        [SonGuncelleyenKullaniciID] uniqueidentifier NULL,
        [OlusturmaTarihi] datetime2 NOT NULL,
        [GuncellemeTarihi] datetime2 NULL,
        [Silindi] bit NOT NULL DEFAULT CAST(0 AS bit),
        CONSTRAINT [PK_Bankalar] PRIMARY KEY ([BankaID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250403075007_InitialCreate'
)
BEGIN
    CREATE TABLE [Birimler] (
        [BirimID] uniqueidentifier NOT NULL,
        [BirimAdi] nvarchar(50) NOT NULL,
        [BirimKodu] nvarchar(20) NOT NULL,
        [BirimSembol] nvarchar(10) NOT NULL,
        [Aciklama] nvarchar(200) NULL,
        [Aktif] bit NOT NULL,
        [Silindi] bit NOT NULL,
        [OlusturmaTarihi] datetime2 NOT NULL,
        [GuncellemeTarihi] datetime2 NULL,
        [OlusturanKullaniciID] nvarchar(450) NOT NULL,
        [SonGuncelleyenKullaniciID] uniqueidentifier NULL,
        [SirketID] uniqueidentifier NULL,
        CONSTRAINT [PK_Birimler] PRIMARY KEY ([BirimID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250403075007_InitialCreate'
)
BEGIN
    CREATE TABLE [Depolar] (
        [DepoID] uniqueidentifier NOT NULL,
        [DepoAdi] nvarchar(100) NOT NULL,
        [Adres] nvarchar(200) NULL,
        [Aktif] bit NOT NULL DEFAULT CAST(1 AS bit),
        [OlusturanKullaniciID] uniqueidentifier NULL,
        [SonGuncelleyenKullaniciID] uniqueidentifier NULL,
        [OlusturmaTarihi] datetime2 NULL,
        [GuncellemeTarihi] datetime2 NULL,
        [Silindi] bit NOT NULL DEFAULT CAST(0 AS bit),
        CONSTRAINT [PK_Depolar] PRIMARY KEY ([DepoID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250403075007_InitialCreate'
)
BEGIN
    CREATE TABLE [FaturaTurleri] (
        [FaturaTuruID] int NOT NULL IDENTITY,
        [FaturaTuruAdi] nvarchar(50) NOT NULL,
        [HareketTuru] nvarchar(50) NULL,
        CONSTRAINT [PK_FaturaTurleri] PRIMARY KEY ([FaturaTuruID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250403075007_InitialCreate'
)
BEGIN
    CREATE TABLE [FiyatTipleri] (
        [FiyatTipiID] int NOT NULL IDENTITY,
        [TipAdi] nvarchar(50) NOT NULL,
        CONSTRAINT [PK_FiyatTipleri] PRIMARY KEY ([FiyatTipiID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250403075007_InitialCreate'
)
BEGIN
    CREATE TABLE [GenelSistemAyarlari] (
        [SistemAyarlariID] int NOT NULL IDENTITY,
        [AnaDovizKodu] nvarchar(10) NOT NULL,
        [SirketAdi] nvarchar(50) NOT NULL,
        [SirketAdresi] nvarchar(100) NULL,
        [SirketTelefon] nvarchar(20) NULL,
        [SirketEmail] nvarchar(100) NULL,
        [SirketVergiNo] nvarchar(20) NULL,
        [SirketVergiDairesi] nvarchar(20) NULL,
        [OtomatikDovizGuncelleme] bit NOT NULL,
        [DovizGuncellemeSikligi] int NOT NULL,
        [SonDovizGuncellemeTarihi] datetime2 NOT NULL,
        [AktifParaBirimleri] nvarchar(500) NULL,
        [Aktif] bit NOT NULL,
        [Silindi] bit NOT NULL,
        [OlusturmaTarihi] datetime2 NOT NULL,
        [GuncellemeTarihi] datetime2 NULL,
        CONSTRAINT [PK_GenelSistemAyarlari] PRIMARY KEY ([SistemAyarlariID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250403075007_InitialCreate'
)
BEGIN
    CREATE TABLE [IrsaliyeTurleri] (
        [IrsaliyeTuruID] int NOT NULL IDENTITY,
        [IrsaliyeTuruAdi] nvarchar(50) NOT NULL,
        [HareketTuru] nvarchar(50) NULL,
        CONSTRAINT [PK_IrsaliyeTurleri] PRIMARY KEY ([IrsaliyeTuruID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250403075007_InitialCreate'
)
BEGIN
    CREATE TABLE [Kasalar] (
        [KasaID] uniqueidentifier NOT NULL,
        [KasaAdi] nvarchar(100) NOT NULL,
        [KasaTuru] nvarchar(50) NULL,
        [ParaBirimi] nvarchar(3) NOT NULL,
        [AcilisBakiye] decimal(18,2) NOT NULL DEFAULT 0.0,
        [GuncelBakiye] decimal(18,2) NOT NULL DEFAULT 0.0,
        [Aciklama] nvarchar(500) NULL,
        [Aktif] bit NOT NULL DEFAULT CAST(1 AS bit),
        [SorumluKullaniciID] uniqueidentifier NULL,
        [OlusturanKullaniciID] uniqueidentifier NULL,
        [SonGuncelleyenKullaniciID] uniqueidentifier NULL,
        [OlusturmaTarihi] datetime2 NOT NULL,
        [GuncellemeTarihi] datetime2 NULL,
        [Silindi] bit NOT NULL DEFAULT CAST(0 AS bit),
        CONSTRAINT [PK_Kasalar] PRIMARY KEY ([KasaID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250403075007_InitialCreate'
)
BEGIN
    CREATE TABLE [Menuler] (
        [MenuID] uniqueidentifier NOT NULL,
        [Ad] nvarchar(100) NOT NULL,
        [Icon] nvarchar(100) NULL,
        [Controller] nvarchar(100) NULL,
        [Action] nvarchar(100) NULL,
        [Sira] int NOT NULL,
        [UstMenuID] uniqueidentifier NULL,
        [Url] nvarchar(255) NULL,
        [AktifMi] bit NOT NULL DEFAULT CAST(1 AS bit),
        [Silindi] bit NOT NULL DEFAULT CAST(0 AS bit),
        [OlusturmaTarihi] datetime2 NOT NULL,
        [GuncellemeTarihi] datetime2 NULL,
        [OlusturanKullaniciID] uniqueidentifier NULL,
        [SonGuncelleyenKullaniciID] uniqueidentifier NULL,
        CONSTRAINT [PK_Menuler] PRIMARY KEY ([MenuID]),
        CONSTRAINT [FK_Menuler_Menuler_UstMenuID] FOREIGN KEY ([UstMenuID]) REFERENCES [Menuler] ([MenuID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250403075007_InitialCreate'
)
BEGIN
    CREATE TABLE [OdemeTurleri] (
        [OdemeTuruID] int NOT NULL IDENTITY,
        [OdemeTuruAdi] nvarchar(50) NOT NULL,
        CONSTRAINT [PK_OdemeTurleri] PRIMARY KEY ([OdemeTuruID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250403075007_InitialCreate'
)
BEGIN
    CREATE TABLE [ParaBirimleri] (
        [ParaBirimiID] uniqueidentifier NOT NULL,
        [Ad] nvarchar(50) NOT NULL,
        [Kod] nvarchar(10) NOT NULL,
        [Sembol] nvarchar(10) NOT NULL,
        [OndalikAyraci] nvarchar(1) NOT NULL,
        [BinlikAyraci] nvarchar(1) NOT NULL,
        [OndalikHassasiyet] int NOT NULL,
        [AnaParaBirimiMi] bit NOT NULL,
        [Sira] int NOT NULL,
        [Aciklama] nvarchar(500) NOT NULL,
        [Aktif] bit NOT NULL DEFAULT CAST(1 AS bit),
        [Silindi] bit NOT NULL DEFAULT CAST(0 AS bit),
        [OlusturmaTarihi] datetime2 NOT NULL,
        [GuncellemeTarihi] datetime2 NULL,
        [OlusturanKullaniciID] nvarchar(max) NOT NULL,
        [SonGuncelleyenKullaniciID] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_ParaBirimleri] PRIMARY KEY ([ParaBirimiID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250403075007_InitialCreate'
)
BEGIN
    CREATE TABLE [SistemAyarlari] (
        [Id] int NOT NULL IDENTITY,
        [Anahtar] nvarchar(100) NOT NULL,
        [Deger] nvarchar(500) NOT NULL,
        [Aciklama] nvarchar(250) NULL,
        [OlusturmaTarihi] datetime2 NOT NULL,
        [GuncellemeTarihi] datetime2 NOT NULL,
        [Silindi] bit NOT NULL,
        CONSTRAINT [PK_SistemAyarlari] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250403075007_InitialCreate'
)
BEGIN
    CREATE TABLE [UrunKategorileri] (
        [KategoriID] uniqueidentifier NOT NULL,
        [KategoriAdi] nvarchar(100) NOT NULL,
        [Aciklama] nvarchar(500) NULL,
        [Aktif] bit NOT NULL DEFAULT CAST(1 AS bit),
        [OlusturanKullaniciID] uniqueidentifier NULL,
        [SonGuncelleyenKullaniciID] uniqueidentifier NULL,
        [OlusturmaTarihi] datetime2 NULL,
        [GuncellemeTarihi] datetime2 NULL,
        [Silindi] bit NOT NULL DEFAULT CAST(0 AS bit),
        CONSTRAINT [PK_UrunKategorileri] PRIMARY KEY ([KategoriID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250403075007_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetRoleClaims] (
        [Id] int NOT NULL IDENTITY,
        [RoleId] varchar(128) NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250403075007_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUserClaims] (
        [Id] int NOT NULL IDENTITY,
        [UserId] varchar(128) NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250403075007_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUserLogins] (
        [LoginProvider] nvarchar(450) NOT NULL,
        [ProviderKey] nvarchar(450) NOT NULL,
        [ProviderDisplayName] nvarchar(max) NULL,
        [UserId] varchar(128) NOT NULL,
        CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
        CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250403075007_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUserRoles] (
        [UserId] varchar(128) NOT NULL,
        [RoleId] varchar(128) NOT NULL,
        CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
        CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250403075007_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUserTokens] (
        [UserId] varchar(128) NOT NULL,
        [LoginProvider] nvarchar(450) NOT NULL,
        [Name] nvarchar(450) NOT NULL,
        [Value] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
        CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250403075007_InitialCreate'
)
BEGIN
    CREATE TABLE [SistemLoglar] (
        [Id] int NOT NULL IDENTITY,
        [LogID] uniqueidentifier NOT NULL,
        [LogTuru] nvarchar(50) NOT NULL,
        [Mesaj] nvarchar(500) NOT NULL,
        [Sayfa] nvarchar(255) NULL,
        [OlusturmaTarihi] datetime2 NOT NULL,
        [IslemTuru] nvarchar(50) NULL,
        [LogTuruInt] int NULL,
        [Aciklama] nvarchar(500) NULL,
        [HataMesaji] nvarchar(500) NULL,
        [KullaniciAdi] nvarchar(100) NULL,
        [IPAdresi] nvarchar(50) NULL,
        [IslemTarihi] datetime2 NOT NULL,
        [Basarili] bit NOT NULL,
        [TabloAdi] nvarchar(100) NULL,
        [KayitAdi] nvarchar(100) NULL,
        [KayitID] uniqueidentifier NULL,
        [KullaniciID] uniqueidentifier NULL,
        [ApplicationUserId] varchar(128) NULL,
        CONSTRAINT [PK_SistemLoglar] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_SistemLoglar_AspNetUsers_ApplicationUserId] FOREIGN KEY ([ApplicationUserId]) REFERENCES [AspNetUsers] ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250403075007_InitialCreate'
)
BEGIN
    CREATE TABLE [BankaHesaplari] (
        [BankaHesapID] uniqueidentifier NOT NULL,
        [BankaID] uniqueidentifier NOT NULL,
        [HesapAdi] nvarchar(100) NOT NULL,
        [HesapNo] nvarchar(50) NULL,
        [IBAN] nvarchar(50) NULL,
        [SubeAdi] nvarchar(100) NULL,
        [SubeKodu] nvarchar(50) NULL,
        [ParaBirimi] nvarchar(10) NULL DEFAULT N'TRY',
        [AcilisBakiye] decimal(18,2) NOT NULL DEFAULT 0.0,
        [GuncelBakiye] decimal(18,2) NOT NULL DEFAULT 0.0,
        [Aciklama] nvarchar(500) NULL,
        [Aktif] bit NOT NULL DEFAULT CAST(1 AS bit),
        [YetkiliKullaniciID] uniqueidentifier NULL,
        [OlusturanKullaniciID] uniqueidentifier NULL,
        [SonGuncelleyenKullaniciID] uniqueidentifier NULL,
        [OlusturmaTarihi] datetime2 NOT NULL,
        [GuncellemeTarihi] datetime2 NULL,
        [Silindi] bit NOT NULL DEFAULT CAST(0 AS bit),
        CONSTRAINT [PK_BankaHesaplari] PRIMARY KEY ([BankaHesapID]),
        CONSTRAINT [FK_BankaHesaplari_Bankalar_BankaID] FOREIGN KEY ([BankaID]) REFERENCES [Bankalar] ([BankaID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250403075007_InitialCreate'
)
BEGIN
    CREATE TABLE [MenuRoller] (
        [MenuId] uniqueidentifier NOT NULL,
        [RolId] varchar(128) NOT NULL,
        [MenuRolID] uniqueidentifier NOT NULL,
        [OlusturmaTarihi] datetime2 NOT NULL,
        CONSTRAINT [PK_MenuRoller] PRIMARY KEY ([MenuId], [RolId]),
        CONSTRAINT [FK_MenuRoller_AspNetRoles_RolId] FOREIGN KEY ([RolId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_MenuRoller_Menuler_MenuId] FOREIGN KEY ([MenuId]) REFERENCES [Menuler] ([MenuID]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250403075007_InitialCreate'
)
BEGIN
    CREATE TABLE [Cariler] (
        [CariID] uniqueidentifier NOT NULL,
        [Ad] nvarchar(100) NOT NULL,
        [CariUnvani] nvarchar(100) NOT NULL,
        [CariKodu] nvarchar(50) NOT NULL,
        [CariTipi] nvarchar(50) NOT NULL,
        [VergiNo] nvarchar(11) NOT NULL,
        [VergiDairesi] nvarchar(50) NOT NULL,
        [Telefon] nvarchar(15) NOT NULL,
        [Email] nvarchar(100) NOT NULL,
        [Yetkili] nvarchar(50) NOT NULL,
        [BaslangicBakiye] decimal(18,2) NOT NULL,
        [Adres] nvarchar(250) NOT NULL,
        [Aciklama] nvarchar(500) NOT NULL,
        [Il] nvarchar(50) NOT NULL,
        [Ilce] nvarchar(50) NOT NULL,
        [PostaKodu] nvarchar(10) NOT NULL,
        [Ulke] nvarchar(50) NOT NULL,
        [WebSitesi] nvarchar(100) NOT NULL,
        [Notlar] nvarchar(1000) NOT NULL,
        [VarsayilanParaBirimiId] uniqueidentifier NULL,
        [VarsayilanKurKullan] bit NOT NULL,
        [AktifMi] bit NOT NULL DEFAULT CAST(1 AS bit),
        [OlusturanKullaniciId] uniqueidentifier NULL,
        [SonGuncelleyenKullaniciId] uniqueidentifier NULL,
        [OlusturmaTarihi] datetime2 NOT NULL,
        [GuncellemeTarihi] datetime2 NULL,
        [Silindi] bit NOT NULL DEFAULT CAST(0 AS bit),
        CONSTRAINT [PK_Cariler] PRIMARY KEY ([CariID]),
        CONSTRAINT [FK_Cariler_ParaBirimleri_VarsayilanParaBirimiId] FOREIGN KEY ([VarsayilanParaBirimiId]) REFERENCES [ParaBirimleri] ([ParaBirimiID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250403075007_InitialCreate'
)
BEGIN
    CREATE TABLE [KurDegerleri] (
        [KurDegeriID] uniqueidentifier NOT NULL,
        [ParaBirimiID] uniqueidentifier NOT NULL,
        [Tarih] datetime2 NOT NULL,
        [Alis] decimal(18,6) NOT NULL,
        [Satis] decimal(18,6) NOT NULL,
        [Efektif_Alis] decimal(18,6) NOT NULL,
        [Efektif_Satis] decimal(18,6) NOT NULL,
        [Aktif] bit NOT NULL DEFAULT CAST(1 AS bit),
        [Silindi] bit NOT NULL,
        [Aciklama] nvarchar(500) NULL,
        [OlusturmaTarihi] datetime2 NOT NULL,
        [GuncellemeTarihi] datetime2 NULL,
        [OlusturanKullaniciID] nvarchar(max) NULL,
        [SonGuncelleyenKullaniciID] nvarchar(max) NULL,
        CONSTRAINT [PK_KurDegerleri] PRIMARY KEY ([KurDegeriID]),
        CONSTRAINT [FK_KurDegerleri_ParaBirimleri_ParaBirimiID] FOREIGN KEY ([ParaBirimiID]) REFERENCES [ParaBirimleri] ([ParaBirimiID]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250403075007_InitialCreate'
)
BEGIN
    CREATE TABLE [ParaBirimiIliskileri] (
        [ParaBirimiIliskiID] uniqueidentifier NOT NULL,
        [KaynakParaBirimiID] uniqueidentifier NOT NULL,
        [HedefParaBirimiID] uniqueidentifier NOT NULL,
        [Aktif] bit NOT NULL DEFAULT CAST(1 AS bit),
        [Silindi] bit NOT NULL DEFAULT CAST(0 AS bit),
        [Aciklama] nvarchar(500) NOT NULL,
        [OlusturmaTarihi] datetime2 NOT NULL,
        [GuncellemeTarihi] datetime2 NULL,
        [OlusturanKullaniciID] nvarchar(max) NOT NULL,
        [SonGuncelleyenKullaniciID] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_ParaBirimiIliskileri] PRIMARY KEY ([ParaBirimiIliskiID]),
        CONSTRAINT [CK_DovizIliski_DifferentCurrencies] CHECK (KaynakParaBirimiID <> HedefParaBirimiID),
        CONSTRAINT [FK_ParaBirimiIliskileri_ParaBirimleri_HedefParaBirimiID] FOREIGN KEY ([HedefParaBirimiID]) REFERENCES [ParaBirimleri] ([ParaBirimiID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ParaBirimiIliskileri_ParaBirimleri_KaynakParaBirimiID] FOREIGN KEY ([KaynakParaBirimiID]) REFERENCES [ParaBirimleri] ([ParaBirimiID]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250403075007_InitialCreate'
)
BEGIN
    CREATE TABLE [Urunler] (
        [UrunID] uniqueidentifier NOT NULL,
        [UrunKodu] nvarchar(50) NOT NULL,
        [UrunAdi] nvarchar(200) NOT NULL,
        [BirimID] uniqueidentifier NULL,
        [StokMiktar] decimal(18,3) NOT NULL DEFAULT 0.0,
        [Aktif] bit NOT NULL DEFAULT CAST(1 AS bit),
        [KDVOrani] int NOT NULL,
        [OlusturanKullaniciID] uniqueidentifier NULL,
        [SonGuncelleyenKullaniciID] uniqueidentifier NULL,
        [OlusturmaTarihi] datetime2 NULL,
        [GuncellemeTarihi] datetime2 NULL,
        [Silindi] bit NOT NULL DEFAULT CAST(0 AS bit),
        [KategoriID] uniqueidentifier NULL,
        CONSTRAINT [PK_Urunler] PRIMARY KEY ([UrunID]),
        CONSTRAINT [FK_Urunler_Birimler_BirimID] FOREIGN KEY ([BirimID]) REFERENCES [Birimler] ([BirimID]) ON DELETE SET NULL,
        CONSTRAINT [FK_Urunler_UrunKategorileri_KategoriID] FOREIGN KEY ([KategoriID]) REFERENCES [UrunKategorileri] ([KategoriID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250403075007_InitialCreate'
)
BEGIN
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
        [ReferansNo] nvarchar(50) NULL,
        [ReferansTuru] nvarchar(50) NULL,
        [ReferansID] uniqueidentifier NULL,
        [Aciklama] nvarchar(500) NULL,
        [DekontNo] nvarchar(50) NULL,
        [IslemYapanKullaniciID] uniqueidentifier NULL,
        [SonGuncelleyenKullaniciID] uniqueidentifier NULL,
        [OlusturmaTarihi] datetime2 NOT NULL,
        [GuncellemeTarihi] datetime2 NULL,
        [Silindi] bit NOT NULL DEFAULT CAST(0 AS bit),
        [KarsiUnvan] nvarchar(200) NULL,
        [KarsiBankaAdi] nvarchar(50) NULL,
        [KarsiIBAN] nvarchar(50) NULL,
        CONSTRAINT [PK_BankaHareketleri] PRIMARY KEY ([BankaHareketID]),
        CONSTRAINT [FK_BankaHareketleri_Bankalar_BankaID] FOREIGN KEY ([BankaID]) REFERENCES [Bankalar] ([BankaID]) ON DELETE CASCADE,
        CONSTRAINT [FK_BankaHareketleri_Cariler_CariID] FOREIGN KEY ([CariID]) REFERENCES [Cariler] ([CariID]),
        CONSTRAINT [FK_BankaHareketleri_Kasalar_HedefKasaID] FOREIGN KEY ([HedefKasaID]) REFERENCES [Kasalar] ([KasaID]),
        CONSTRAINT [FK_BankaHareketleri_Kasalar_KaynakKasaID] FOREIGN KEY ([KaynakKasaID]) REFERENCES [Kasalar] ([KasaID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250403075007_InitialCreate'
)
BEGIN
    CREATE TABLE [BankaHesapHareketleri] (
        [BankaHesapHareketID] uniqueidentifier NOT NULL,
        [BankaHesapID] uniqueidentifier NOT NULL,
        [BankaID] uniqueidentifier NOT NULL,
        [KaynakKasaID] uniqueidentifier NULL,
        [HedefKasaID] uniqueidentifier NULL,
        [TransferID] uniqueidentifier NULL,
        [CariID] uniqueidentifier NULL,
        [Tutar] decimal(18,2) NOT NULL,
        [HareketTuru] nvarchar(50) NOT NULL,
        [Tarih] datetime2 NOT NULL,
        [ReferansNo] nvarchar(50) NULL,
        [ReferansTuru] nvarchar(50) NULL,
        [ReferansID] uniqueidentifier NULL,
        [Aciklama] nvarchar(500) NULL,
        [DekontNo] nvarchar(50) NULL,
        [IslemYapanKullaniciID] uniqueidentifier NULL,
        [SonGuncelleyenKullaniciID] uniqueidentifier NULL,
        [OlusturmaTarihi] datetime2 NOT NULL,
        [GuncellemeTarihi] datetime2 NULL,
        [Silindi] bit NOT NULL DEFAULT CAST(0 AS bit),
        [KarsiUnvan] nvarchar(200) NULL,
        [KarsiBankaAdi] nvarchar(50) NULL,
        [KarsiIBAN] nvarchar(50) NULL,
        CONSTRAINT [PK_BankaHesapHareketleri] PRIMARY KEY ([BankaHesapHareketID]),
        CONSTRAINT [FK_BankaHesapHareketleri_BankaHesaplari_BankaHesapID] FOREIGN KEY ([BankaHesapID]) REFERENCES [BankaHesaplari] ([BankaHesapID]),
        CONSTRAINT [FK_BankaHesapHareketleri_Bankalar_BankaID] FOREIGN KEY ([BankaID]) REFERENCES [Bankalar] ([BankaID]) ON DELETE CASCADE,
        CONSTRAINT [FK_BankaHesapHareketleri_Cariler_CariID] FOREIGN KEY ([CariID]) REFERENCES [Cariler] ([CariID]),
        CONSTRAINT [FK_BankaHesapHareketleri_Kasalar_HedefKasaID] FOREIGN KEY ([HedefKasaID]) REFERENCES [Kasalar] ([KasaID]),
        CONSTRAINT [FK_BankaHesapHareketleri_Kasalar_KaynakKasaID] FOREIGN KEY ([KaynakKasaID]) REFERENCES [Kasalar] ([KasaID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250403075007_InitialCreate'
)
BEGIN
    CREATE TABLE [CariHareketler] (
        [CariHareketId] uniqueidentifier NOT NULL,
        [CariId] uniqueidentifier NOT NULL,
        [HareketTuru] nvarchar(50) NOT NULL,
        [Tutar] decimal(18,2) NOT NULL,
        [Tarih] datetime2 NOT NULL,
        [VadeTarihi] datetime2 NULL,
        [ReferansNo] nvarchar(50) NOT NULL,
        [ReferansTuru] nvarchar(50) NOT NULL,
        [ReferansId] uniqueidentifier NULL,
        [Aciklama] nvarchar(500) NOT NULL,
        [Borc] decimal(18,2) NOT NULL,
        [Alacak] decimal(18,2) NOT NULL,
        [OlusturmaTarihi] datetime2 NOT NULL,
        [OlusturanKullaniciId] uniqueidentifier NULL,
        [Silindi] bit NOT NULL,
        [GuncellemeTarihi] datetime2 NULL,
        CONSTRAINT [PK_CariHareketler] PRIMARY KEY ([CariHareketId]),
        CONSTRAINT [FK_CariHareketler_Cariler_CariId] FOREIGN KEY ([CariId]) REFERENCES [Cariler] ([CariID]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250403075007_InitialCreate'
)
BEGIN
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
        [KarsiParaBirimi] nvarchar(3) NULL,
        [Tarih] datetime2 NOT NULL,
        [ReferansNo] nvarchar(50) NULL,
        [ReferansTuru] nvarchar(50) NULL,
        [ReferansID] uniqueidentifier NULL,
        [Aciklama] nvarchar(500) NULL,
        [IslemYapanKullaniciID] uniqueidentifier NULL,
        [SonGuncelleyenKullaniciID] uniqueidentifier NULL,
        [OlusturmaTarihi] datetime2 NOT NULL,
        [GuncellemeTarihi] datetime2 NULL,
        [Silindi] bit NOT NULL DEFAULT CAST(0 AS bit),
        [TransferID] uniqueidentifier NULL,
        CONSTRAINT [PK_KasaHareketleri] PRIMARY KEY ([KasaHareketID]),
        CONSTRAINT [FK_KasaHareketleri_Bankalar_HedefBankaID] FOREIGN KEY ([HedefBankaID]) REFERENCES [Bankalar] ([BankaID]),
        CONSTRAINT [FK_KasaHareketleri_Bankalar_KaynakBankaID] FOREIGN KEY ([KaynakBankaID]) REFERENCES [Bankalar] ([BankaID]),
        CONSTRAINT [FK_KasaHareketleri_Cariler_CariID] FOREIGN KEY ([CariID]) REFERENCES [Cariler] ([CariID]),
        CONSTRAINT [FK_KasaHareketleri_Kasalar_HedefKasaID] FOREIGN KEY ([HedefKasaID]) REFERENCES [Kasalar] ([KasaID]),
        CONSTRAINT [FK_KasaHareketleri_Kasalar_KasaID] FOREIGN KEY ([KasaID]) REFERENCES [Kasalar] ([KasaID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250403075007_InitialCreate'
)
BEGIN
    CREATE TABLE [Sozlesmeler] (
        [SozlesmeID] uniqueidentifier NOT NULL,
        [SozlesmeNo] nvarchar(100) NOT NULL,
        [SozlesmeTarihi] datetime2 NOT NULL,
        [BitisTarihi] datetime2 NULL,
        [CariID] uniqueidentifier NOT NULL,
        [VekaletGeldiMi] bit NOT NULL,
        [ResmiFaturaKesildiMi] bit NOT NULL,
        [SozlesmeDosyaYolu] nvarchar(500) NULL,
        [VekaletnameDosyaYolu] nvarchar(500) NULL,
        [SozlesmeTutari] decimal(18,2) NOT NULL,
        [SozlesmeDovizTuru] nvarchar(10) NULL,
        [Aciklama] nvarchar(1000) NULL,
        [AktifMi] bit NOT NULL,
        [OlusturmaTarihi] datetime2 NOT NULL,
        [GuncellemeTarihi] datetime2 NULL,
        [OlusturanKullaniciID] uniqueidentifier NULL,
        [GuncelleyenKullaniciID] uniqueidentifier NULL,
        [Silindi] bit NOT NULL,
        [AnaSozlesmeID] uniqueidentifier NULL,
        CONSTRAINT [PK_Sozlesmeler] PRIMARY KEY ([SozlesmeID]),
        CONSTRAINT [FK_Sozlesmeler_Cariler_CariID] FOREIGN KEY ([CariID]) REFERENCES [Cariler] ([CariID]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250403075007_InitialCreate'
)
BEGIN
    CREATE TABLE [StokFifoKayitlari] (
        [StokFifoID] uniqueidentifier NOT NULL,
        [UrunID] uniqueidentifier NOT NULL,
        [Miktar] decimal(18,3) NOT NULL,
        [KalanMiktar] decimal(18,3) NOT NULL,
        [BirimFiyat] decimal(18,2) NOT NULL,
        [Birim] nvarchar(20) NOT NULL,
        [ParaBirimi] nvarchar(3) NULL,
        [DovizKuru] decimal(18,6) NOT NULL,
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
        [Silindi] bit NOT NULL,
        [RowVersion] rowversion NULL,
        CONSTRAINT [PK_StokFifoKayitlari] PRIMARY KEY ([StokFifoID]),
        CONSTRAINT [FK_StokFifoKayitlari_Urunler_UrunID] FOREIGN KEY ([UrunID]) REFERENCES [Urunler] ([UrunID]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250403075007_InitialCreate'
)
BEGIN
    CREATE TABLE [StokHareketleri] (
        [StokHareketID] uniqueidentifier NOT NULL,
        [UrunID] uniqueidentifier NOT NULL,
        [DepoID] uniqueidentifier NULL,
        [Miktar] decimal(18,3) NOT NULL,
        [Birim] nvarchar(50) NOT NULL,
        [Tarih] datetime2 NOT NULL,
        [HareketTuru] int NOT NULL,
        [ReferansNo] nvarchar(50) NOT NULL,
        [ReferansTuru] nvarchar(50) NOT NULL,
        [ReferansID] uniqueidentifier NULL,
        [Aciklama] nvarchar(500) NULL,
        [BirimFiyat] decimal(18,2) NULL,
        [IslemYapanKullaniciID] uniqueidentifier NULL,
        [SonGuncelleyenKullaniciID] uniqueidentifier NULL,
        [OlusturmaTarihi] datetime2 NULL,
        [GuncellemeTarihi] datetime2 NULL,
        [Silindi] bit NOT NULL,
        [FaturaID] uniqueidentifier NULL,
        [IrsaliyeID] uniqueidentifier NULL,
        [IrsaliyeTuru] nvarchar(50) NULL,
        CONSTRAINT [PK_StokHareketleri] PRIMARY KEY ([StokHareketID]),
        CONSTRAINT [FK_StokHareketleri_Depolar_DepoID] FOREIGN KEY ([DepoID]) REFERENCES [Depolar] ([DepoID]),
        CONSTRAINT [FK_StokHareketleri_Urunler_UrunID] FOREIGN KEY ([UrunID]) REFERENCES [Urunler] ([UrunID]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250403075007_InitialCreate'
)
BEGIN
    CREATE TABLE [UrunFiyatlari] (
        [FiyatID] uniqueidentifier NOT NULL,
        [UrunID] uniqueidentifier NULL,
        [Fiyat] decimal(18,2) NOT NULL,
        [GecerliTarih] datetime2 NOT NULL,
        [FiyatTipiID] int NULL,
        [OlusturanKullaniciID] uniqueidentifier NULL,
        [SonGuncelleyenKullaniciID] uniqueidentifier NULL,
        [OlusturmaTarihi] datetime2 NULL,
        [GuncellemeTarihi] datetime2 NULL,
        [Silindi] bit NOT NULL,
        CONSTRAINT [PK_UrunFiyatlari] PRIMARY KEY ([FiyatID]),
        CONSTRAINT [FK_UrunFiyatlari_FiyatTipleri_FiyatTipiID] FOREIGN KEY ([FiyatTipiID]) REFERENCES [FiyatTipleri] ([FiyatTipiID]),
        CONSTRAINT [FK_UrunFiyatlari_Urunler_UrunID] FOREIGN KEY ([UrunID]) REFERENCES [Urunler] ([UrunID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250403075007_InitialCreate'
)
BEGIN
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
        [OdemeDurumu] nvarchar(50) NULL,
        [FaturaNotu] nvarchar(500) NULL,
        [ResmiMi] bit NOT NULL DEFAULT CAST(1 AS bit),
        [SozlesmeID] uniqueidentifier NULL,
        [DovizTuru] nvarchar(10) NULL,
        [DovizKuru] decimal(18,4) NULL,
        [OlusturmaTarihi] datetime2 NULL,
        [GuncellemeTarihi] datetime2 NULL,
        [Silindi] bit NOT NULL DEFAULT CAST(0 AS bit),
        [Aktif] bit NOT NULL DEFAULT CAST(1 AS bit),
        [OlusturanKullaniciID] uniqueidentifier NULL,
        [SonGuncelleyenKullaniciID] uniqueidentifier NULL,
        [OdemeTuruID] int NULL,
        [AklanmaTarihi] datetime2 NULL,
        [AklanmaNotu] nvarchar(500) NULL,
        CONSTRAINT [PK_Faturalar] PRIMARY KEY ([FaturaID]),
        CONSTRAINT [FK_Faturalar_Cariler_CariID] FOREIGN KEY ([CariID]) REFERENCES [Cariler] ([CariID]),
        CONSTRAINT [FK_Faturalar_FaturaTurleri_FaturaTuruID] FOREIGN KEY ([FaturaTuruID]) REFERENCES [FaturaTurleri] ([FaturaTuruID]),
        CONSTRAINT [FK_Faturalar_OdemeTurleri_OdemeTuruID] FOREIGN KEY ([OdemeTuruID]) REFERENCES [OdemeTurleri] ([OdemeTuruID]),
        CONSTRAINT [FK_Faturalar_Sozlesmeler_SozlesmeID] FOREIGN KEY ([SozlesmeID]) REFERENCES [Sozlesmeler] ([SozlesmeID]) ON DELETE SET NULL
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250403075007_InitialCreate'
)
BEGIN
    CREATE TABLE [FaturaDetaylari] (
        [FaturaDetayID] uniqueidentifier NOT NULL,
        [FaturaID] uniqueidentifier NOT NULL,
        [UrunID] uniqueidentifier NOT NULL,
        [Miktar] decimal(18,3) NOT NULL,
        [BirimFiyat] decimal(18,2) NOT NULL,
        [KdvOrani] decimal(18,2) NOT NULL,
        [IndirimOrani] decimal(18,2) NOT NULL,
        [SatirToplam] decimal(18,2) NOT NULL,
        [SatirKdvToplam] decimal(18,2) NOT NULL,
        [Birim] nvarchar(50) NULL,
        [Aciklama] nvarchar(500) NULL,
        [OlusturmaTarihi] datetime2 NULL,
        [GuncellemeTarihi] datetime2 NULL,
        [Silindi] bit NOT NULL,
        [Aktif] bit NULL,
        [OlusturanKullaniciID] uniqueidentifier NULL,
        [SonGuncelleyenKullaniciID] uniqueidentifier NULL,
        [Tutar] decimal(18,2) NULL,
        [KdvTutari] decimal(18,2) NULL,
        [IndirimTutari] decimal(18,2) NULL,
        [NetTutar] decimal(18,2) NULL,
        [AklananMiktar] decimal(18,2) NULL,
        [AklanmaTamamlandi] bit NOT NULL,
        [BirimID] uniqueidentifier NULL,
        CONSTRAINT [PK_FaturaDetaylari] PRIMARY KEY ([FaturaDetayID]),
        CONSTRAINT [FK_FaturaDetaylari_Birimler_BirimID] FOREIGN KEY ([BirimID]) REFERENCES [Birimler] ([BirimID]),
        CONSTRAINT [FK_FaturaDetaylari_Faturalar_FaturaID] FOREIGN KEY ([FaturaID]) REFERENCES [Faturalar] ([FaturaID]) ON DELETE CASCADE,
        CONSTRAINT [FK_FaturaDetaylari_Urunler_UrunID] FOREIGN KEY ([UrunID]) REFERENCES [Urunler] ([UrunID]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250403075007_InitialCreate'
)
BEGIN
    CREATE TABLE [FaturaOdemeleri] (
        [OdemeID] uniqueidentifier NOT NULL,
        [FaturaID] uniqueidentifier NOT NULL,
        [OdemeTarihi] datetime2 NOT NULL,
        [OdemeTutari] decimal(18,2) NOT NULL,
        [OdemeTuru] nvarchar(50) NOT NULL,
        [Aciklama] nvarchar(500) NULL,
        [OlusturanKullaniciID] uniqueidentifier NULL,
        [SonGuncelleyenKullaniciID] uniqueidentifier NULL,
        [OlusturmaTarihi] datetime2 NULL,
        [GuncellemeTarihi] datetime2 NULL,
        [Silindi] bit NOT NULL,
        [Aktif] bit NOT NULL,
        CONSTRAINT [PK_FaturaOdemeleri] PRIMARY KEY ([OdemeID]),
        CONSTRAINT [FK_FaturaOdemeleri_Faturalar_FaturaID] FOREIGN KEY ([FaturaID]) REFERENCES [Faturalar] ([FaturaID]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250403075007_InitialCreate'
)
BEGIN
    CREATE TABLE [Irsaliyeler] (
        [IrsaliyeID] uniqueidentifier NOT NULL,
        [IrsaliyeNumarasi] nvarchar(20) NOT NULL,
        [IrsaliyeTarihi] datetime2 NOT NULL,
        [CariID] uniqueidentifier NOT NULL,
        [FaturaID] uniqueidentifier NULL,
        [Aciklama] nvarchar(500) NULL,
        [IrsaliyeTuru] nvarchar(max) NOT NULL,
        [Aktif] bit NOT NULL DEFAULT CAST(1 AS bit),
        [OlusturanKullaniciId] uniqueidentifier NOT NULL,
        [SonGuncelleyenKullaniciId] uniqueidentifier NULL,
        [Silindi] bit NOT NULL,
        [Durum] nvarchar(20) NULL,
        [IrsaliyeTuruID] int NULL,
        [OlusturmaTarihi] datetime2 NOT NULL,
        [GuncellemeTarihi] datetime2 NULL,
        CONSTRAINT [PK_Irsaliyeler] PRIMARY KEY ([IrsaliyeID]),
        CONSTRAINT [FK_Irsaliyeler_Cariler_CariID] FOREIGN KEY ([CariID]) REFERENCES [Cariler] ([CariID]) ON DELETE CASCADE,
        CONSTRAINT [FK_Irsaliyeler_Faturalar_FaturaID] FOREIGN KEY ([FaturaID]) REFERENCES [Faturalar] ([FaturaID]),
        CONSTRAINT [FK_Irsaliyeler_IrsaliyeTurleri_IrsaliyeTuruID] FOREIGN KEY ([IrsaliyeTuruID]) REFERENCES [IrsaliyeTurleri] ([IrsaliyeTuruID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250403075007_InitialCreate'
)
BEGIN
    CREATE TABLE [FaturaAklamaKuyrugu] (
        [AklamaID] uniqueidentifier NOT NULL,
        [FaturaKalemID] uniqueidentifier NOT NULL,
        [UrunID] uniqueidentifier NOT NULL,
        [AklananMiktar] decimal(18,3) NOT NULL,
        [OlusturmaTarihi] datetime2 NOT NULL,
        [AklanmaTarihi] datetime2 NULL,
        [AklanmaNotu] nvarchar(500) NULL,
        [SozlesmeID] uniqueidentifier NOT NULL,
        [Durum] int NOT NULL DEFAULT 0,
        [BirimFiyat] decimal(18,2) NOT NULL,
        [ParaBirimi] nvarchar(10) NOT NULL DEFAULT N'TL',
        [DovizKuru] decimal(18,4) NOT NULL DEFAULT 1.0,
        [OlusturanKullaniciID] uniqueidentifier NULL,
        [GuncelleyenKullaniciID] uniqueidentifier NULL,
        [GuncellemeTarihi] datetime2 NULL,
        [Silindi] bit NOT NULL DEFAULT CAST(0 AS bit),
        [ResmiFaturaKalemID] uniqueidentifier NULL,
        [FaturaID] uniqueidentifier NULL,
        CONSTRAINT [PK_FaturaAklamaKuyrugu] PRIMARY KEY ([AklamaID]),
        CONSTRAINT [FK_FaturaAklamaKuyrugu_FaturaDetaylari_FaturaKalemID] FOREIGN KEY ([FaturaKalemID]) REFERENCES [FaturaDetaylari] ([FaturaDetayID]),
        CONSTRAINT [FK_FaturaAklamaKuyrugu_Faturalar_FaturaID] FOREIGN KEY ([FaturaID]) REFERENCES [Faturalar] ([FaturaID]),
        CONSTRAINT [FK_FaturaAklamaKuyrugu_Sozlesmeler_SozlesmeID] FOREIGN KEY ([SozlesmeID]) REFERENCES [Sozlesmeler] ([SozlesmeID]),
        CONSTRAINT [FK_FaturaAklamaKuyrugu_Urunler_UrunID] FOREIGN KEY ([UrunID]) REFERENCES [Urunler] ([UrunID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250403075007_InitialCreate'
)
BEGIN
    CREATE TABLE [IrsaliyeDetaylari] (
        [IrsaliyeDetayID] uniqueidentifier NOT NULL,
        [IrsaliyeID] uniqueidentifier NOT NULL,
        [UrunID] uniqueidentifier NOT NULL,
        [Miktar] decimal(18,3) NOT NULL,
        [BirimFiyat] decimal(18,2) NOT NULL,
        [KdvOrani] decimal(18,2) NOT NULL,
        [IndirimOrani] decimal(18,2) NOT NULL,
        [Birim] nvarchar(50) NULL,
        [Aciklama] nvarchar(200) NULL,
        [OlusturanKullaniciId] uniqueidentifier NULL,
        [SonGuncelleyenKullaniciId] uniqueidentifier NULL,
        [OlusturmaTarihi] datetime2 NULL,
        [GuncellemeTarihi] datetime2 NULL,
        [Aktif] bit NOT NULL,
        [Silindi] bit NOT NULL,
        [SatirToplam] decimal(18,2) NOT NULL,
        [SatirKdvToplam] decimal(18,2) NOT NULL,
        [BirimID] uniqueidentifier NULL,
        CONSTRAINT [PK_IrsaliyeDetaylari] PRIMARY KEY ([IrsaliyeDetayID]),
        CONSTRAINT [FK_IrsaliyeDetaylari_Birimler_BirimID] FOREIGN KEY ([BirimID]) REFERENCES [Birimler] ([BirimID]),
        CONSTRAINT [FK_IrsaliyeDetaylari_Irsaliyeler_IrsaliyeID] FOREIGN KEY ([IrsaliyeID]) REFERENCES [Irsaliyeler] ([IrsaliyeID]) ON DELETE CASCADE,
        CONSTRAINT [FK_IrsaliyeDetaylari_Urunler_UrunID] FOREIGN KEY ([UrunID]) REFERENCES [Urunler] ([UrunID]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250403075007_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [AspNetRoleClaims] ([RoleId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250403075007_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [RoleNameIndex] ON [AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250403075007_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AspNetUserClaims_UserId] ON [AspNetUserClaims] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250403075007_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AspNetUserLogins_UserId] ON [AspNetUserLogins] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250403075007_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [AspNetUserRoles] ([RoleId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250403075007_InitialCreate'
)
BEGIN
    CREATE INDEX [EmailIndex] ON [AspNetUsers] ([NormalizedEmail]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250403075007_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [UserNameIndex] ON [AspNetUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250403075007_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_BankaHareketleri_BankaID] ON [BankaHareketleri] ([BankaID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250403075007_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_BankaHareketleri_CariID] ON [BankaHareketleri] ([CariID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250403075007_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_BankaHareketleri_HedefKasaID] ON [BankaHareketleri] ([HedefKasaID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250403075007_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_BankaHareketleri_KaynakKasaID] ON [BankaHareketleri] ([KaynakKasaID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250403075007_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_BankaHesapHareketleri_BankaHesapID] ON [BankaHesapHareketleri] ([BankaHesapID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250403075007_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_BankaHesapHareketleri_BankaID] ON [BankaHesapHareketleri] ([BankaID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250403075007_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_BankaHesapHareketleri_CariID] ON [BankaHesapHareketleri] ([CariID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250403075007_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_BankaHesapHareketleri_HedefKasaID] ON [BankaHesapHareketleri] ([HedefKasaID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250403075007_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_BankaHesapHareketleri_KaynakKasaID] ON [BankaHesapHareketleri] ([KaynakKasaID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250403075007_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_BankaHesaplari_BankaID] ON [BankaHesaplari] ([BankaID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250403075007_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_CariHareketler_CariId] ON [CariHareketler] ([CariId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250403075007_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Cariler_VarsayilanParaBirimiId] ON [Cariler] ([VarsayilanParaBirimiId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250403075007_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_FaturaAklamaKuyrugu_FaturaID] ON [FaturaAklamaKuyrugu] ([FaturaID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250403075007_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_FaturaAklamaKuyrugu_FaturaKalemID] ON [FaturaAklamaKuyrugu] ([FaturaKalemID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250403075007_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_FaturaAklamaKuyrugu_SozlesmeID] ON [FaturaAklamaKuyrugu] ([SozlesmeID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250403075007_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_FaturaAklamaKuyrugu_UrunID] ON [FaturaAklamaKuyrugu] ([UrunID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250403075007_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_FaturaDetaylari_BirimID] ON [FaturaDetaylari] ([BirimID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250403075007_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_FaturaDetaylari_FaturaID] ON [FaturaDetaylari] ([FaturaID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250403075007_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_FaturaDetaylari_UrunID] ON [FaturaDetaylari] ([UrunID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250403075007_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Faturalar_CariID] ON [Faturalar] ([CariID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250403075007_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Faturalar_FaturaTuruID] ON [Faturalar] ([FaturaTuruID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250403075007_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Faturalar_OdemeTuruID] ON [Faturalar] ([OdemeTuruID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250403075007_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Faturalar_SozlesmeID] ON [Faturalar] ([SozlesmeID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250403075007_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_FaturaOdemeleri_FaturaID] ON [FaturaOdemeleri] ([FaturaID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250403075007_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_IrsaliyeDetaylari_BirimID] ON [IrsaliyeDetaylari] ([BirimID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250403075007_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_IrsaliyeDetaylari_IrsaliyeID] ON [IrsaliyeDetaylari] ([IrsaliyeID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250403075007_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_IrsaliyeDetaylari_UrunID] ON [IrsaliyeDetaylari] ([UrunID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250403075007_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Irsaliyeler_CariID] ON [Irsaliyeler] ([CariID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250403075007_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Irsaliyeler_FaturaID] ON [Irsaliyeler] ([FaturaID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250403075007_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Irsaliyeler_IrsaliyeTuruID] ON [Irsaliyeler] ([IrsaliyeTuruID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250403075007_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_KasaHareketleri_CariID] ON [KasaHareketleri] ([CariID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250403075007_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_KasaHareketleri_HedefBankaID] ON [KasaHareketleri] ([HedefBankaID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250403075007_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_KasaHareketleri_HedefKasaID] ON [KasaHareketleri] ([HedefKasaID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250403075007_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_KasaHareketleri_KasaID] ON [KasaHareketleri] ([KasaID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250403075007_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_KasaHareketleri_KaynakBankaID] ON [KasaHareketleri] ([KaynakBankaID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250403075007_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_KurDegeri_ParaBirimiID_Tarih] ON [KurDegerleri] ([ParaBirimiID], [Tarih]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250403075007_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Menuler_UstMenuID] ON [Menuler] ([UstMenuID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250403075007_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_MenuRoller_RolId] ON [MenuRoller] ([RolId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250403075007_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_DovizIliski_KaynakParaBirimiID_HedefParaBirimiID] ON [ParaBirimiIliskileri] ([KaynakParaBirimiID], [HedefParaBirimiID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250403075007_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ParaBirimiIliskileri_HedefParaBirimiID] ON [ParaBirimiIliskileri] ([HedefParaBirimiID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250403075007_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ParaBirimi_Kod] ON [ParaBirimleri] ([Kod]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250403075007_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_SistemLoglar_ApplicationUserId] ON [SistemLoglar] ([ApplicationUserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250403075007_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Sozlesmeler_CariID] ON [Sozlesmeler] ([CariID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250403075007_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_StokFifo_GirisTarihi] ON [StokFifoKayitlari] ([GirisTarihi]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250403075007_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_StokFifo_Referans] ON [StokFifoKayitlari] ([ReferansID], [ReferansTuru]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250403075007_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_StokFifo_StokSorgu] ON [StokFifoKayitlari] ([UrunID], [KalanMiktar], [Aktif], [Silindi], [Iptal]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250403075007_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_StokFifo_UrunID] ON [StokFifoKayitlari] ([UrunID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250403075007_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_StokHareketleri_DepoID] ON [StokHareketleri] ([DepoID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250403075007_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_StokHareketleri_UrunID] ON [StokHareketleri] ([UrunID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250403075007_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_UrunFiyatlari_FiyatTipiID] ON [UrunFiyatlari] ([FiyatTipiID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250403075007_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_UrunFiyatlari_UrunID] ON [UrunFiyatlari] ([UrunID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250403075007_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Urunler_BirimID] ON [Urunler] ([BirimID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250403075007_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Urunler_KategoriID] ON [Urunler] ([KategoriID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250403075007_InitialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250403075007_InitialCreate', N'9.0.4');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250403143013_PendingChanges'
)
BEGIN
    DECLARE @var sysname;
    SELECT @var = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Bankalar]') AND [c].[name] = N'Aciklama');
    IF @var IS NOT NULL EXEC(N'ALTER TABLE [Bankalar] DROP CONSTRAINT [' + @var + '];');
    ALTER TABLE [Bankalar] DROP COLUMN [Aciklama];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250403143013_PendingChanges'
)
BEGIN
    DECLARE @var1 sysname;
    SELECT @var1 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Bankalar]') AND [c].[name] = N'AcilisBakiye');
    IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [Bankalar] DROP CONSTRAINT [' + @var1 + '];');
    ALTER TABLE [Bankalar] DROP COLUMN [AcilisBakiye];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250403143013_PendingChanges'
)
BEGIN
    DECLARE @var2 sysname;
    SELECT @var2 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Bankalar]') AND [c].[name] = N'GuncelBakiye');
    IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [Bankalar] DROP CONSTRAINT [' + @var2 + '];');
    ALTER TABLE [Bankalar] DROP COLUMN [GuncelBakiye];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250403143013_PendingChanges'
)
BEGIN
    DECLARE @var3 sysname;
    SELECT @var3 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Bankalar]') AND [c].[name] = N'HesapNo');
    IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [Bankalar] DROP CONSTRAINT [' + @var3 + '];');
    ALTER TABLE [Bankalar] DROP COLUMN [HesapNo];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250403143013_PendingChanges'
)
BEGIN
    DECLARE @var4 sysname;
    SELECT @var4 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Bankalar]') AND [c].[name] = N'IBAN');
    IF @var4 IS NOT NULL EXEC(N'ALTER TABLE [Bankalar] DROP CONSTRAINT [' + @var4 + '];');
    ALTER TABLE [Bankalar] DROP COLUMN [IBAN];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250403143013_PendingChanges'
)
BEGIN
    DECLARE @var5 sysname;
    SELECT @var5 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Bankalar]') AND [c].[name] = N'ParaBirimi');
    IF @var5 IS NOT NULL EXEC(N'ALTER TABLE [Bankalar] DROP CONSTRAINT [' + @var5 + '];');
    ALTER TABLE [Bankalar] DROP COLUMN [ParaBirimi];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250403143013_PendingChanges'
)
BEGIN
    DECLARE @var6 sysname;
    SELECT @var6 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Bankalar]') AND [c].[name] = N'SubeAdi');
    IF @var6 IS NOT NULL EXEC(N'ALTER TABLE [Bankalar] DROP CONSTRAINT [' + @var6 + '];');
    ALTER TABLE [Bankalar] DROP COLUMN [SubeAdi];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250403143013_PendingChanges'
)
BEGIN
    DECLARE @var7 sysname;
    SELECT @var7 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Bankalar]') AND [c].[name] = N'SubeKodu');
    IF @var7 IS NOT NULL EXEC(N'ALTER TABLE [Bankalar] DROP CONSTRAINT [' + @var7 + '];');
    ALTER TABLE [Bankalar] DROP COLUMN [SubeKodu];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250403143013_PendingChanges'
)
BEGIN
    DECLARE @var8 sysname;
    SELECT @var8 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Bankalar]') AND [c].[name] = N'YetkiliKullaniciID');
    IF @var8 IS NOT NULL EXEC(N'ALTER TABLE [Bankalar] DROP CONSTRAINT [' + @var8 + '];');
    ALTER TABLE [Bankalar] DROP COLUMN [YetkiliKullaniciID];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250403143013_PendingChanges'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250403143013_PendingChanges', N'9.0.4');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250403143432_FixPendingModelChanges'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250403143432_FixPendingModelChanges', N'9.0.4');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250403182734_FixRelationships'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250403182734_FixRelationships', N'9.0.4');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250403182830_FixRemainingRelationships'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250403182830_FixRemainingRelationships', N'9.0.4');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250403205133_MakeCariUnvaniNullable'
)
BEGIN
    DECLARE @var9 sysname;
    SELECT @var9 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Cariler]') AND [c].[name] = N'CariUnvani');
    IF @var9 IS NOT NULL EXEC(N'ALTER TABLE [Cariler] DROP CONSTRAINT [' + @var9 + '];');
    ALTER TABLE [Cariler] ALTER COLUMN [CariUnvani] nvarchar(100) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250403205133_MakeCariUnvaniNullable'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250403205133_MakeCariUnvaniNullable', N'9.0.4');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250405181911_AddStokFifoAndStokCikisDetay'
)
BEGIN
    CREATE TABLE [StokCikisDetaylari] (
        [StokCikisDetayID] uniqueidentifier NOT NULL,
        [StokFifoID] uniqueidentifier NOT NULL,
        [CikisMiktari] decimal(18,6) NOT NULL,
        [BirimFiyat] decimal(18,6) NOT NULL,
        [BirimFiyatUSD] decimal(18,6) NOT NULL,
        [BirimFiyatTL] decimal(18,6) NOT NULL,
        [BirimFiyatUZS] decimal(18,6) NOT NULL,
        [ToplamMaliyetUSD] decimal(18,6) NOT NULL,
        [ReferansNo] nvarchar(50) NULL,
        [ReferansTuru] nvarchar(50) NULL,
        [ReferansID] uniqueidentifier NOT NULL,
        [CikisTarihi] datetime2 NOT NULL,
        [Aciklama] nvarchar(500) NULL,
        [OlusturmaTarihi] datetime2 NOT NULL,
        [Iptal] bit NOT NULL,
        [IptalTarihi] datetime2 NULL,
        [IptalAciklama] nvarchar(500) NULL,
        CONSTRAINT [PK_StokCikisDetaylari] PRIMARY KEY ([StokCikisDetayID]),
        CONSTRAINT [FK_StokCikisDetaylari_StokFifoKayitlari_StokFifoID] FOREIGN KEY ([StokFifoID]) REFERENCES [StokFifoKayitlari] ([StokFifoID]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250405181911_AddStokFifoAndStokCikisDetay'
)
BEGIN
    CREATE INDEX [IX_StokCikisDetaylari_StokFifoID] ON [StokCikisDetaylari] ([StokFifoID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250405181911_AddStokFifoAndStokCikisDetay'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250405181911_AddStokFifoAndStokCikisDetay', N'9.0.4');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250405182331_UpdateStokCikisDetayRelationToOptional'
)
BEGIN
    ALTER TABLE [StokCikisDetaylari] DROP CONSTRAINT [FK_StokCikisDetaylari_StokFifoKayitlari_StokFifoID];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250405182331_UpdateStokCikisDetayRelationToOptional'
)
BEGIN
    DECLARE @var10 sysname;
    SELECT @var10 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[StokCikisDetaylari]') AND [c].[name] = N'StokFifoID');
    IF @var10 IS NOT NULL EXEC(N'ALTER TABLE [StokCikisDetaylari] DROP CONSTRAINT [' + @var10 + '];');
    ALTER TABLE [StokCikisDetaylari] ALTER COLUMN [StokFifoID] uniqueidentifier NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250405182331_UpdateStokCikisDetayRelationToOptional'
)
BEGIN
    ALTER TABLE [StokCikisDetaylari] ADD CONSTRAINT [FK_StokCikisDetaylari_StokFifoKayitlari_StokFifoID] FOREIGN KEY ([StokFifoID]) REFERENCES [StokFifoKayitlari] ([StokFifoID]) ON DELETE SET NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250405182331_UpdateStokCikisDetayRelationToOptional'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250405182331_UpdateStokCikisDetayRelationToOptional', N'9.0.4');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250405184437_UpdateNullableTypes'
)
BEGIN
    DECLARE @var11 sysname;
    SELECT @var11 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[StokFifoKayitlari]') AND [c].[name] = N'ParaBirimi');
    IF @var11 IS NOT NULL EXEC(N'ALTER TABLE [StokFifoKayitlari] DROP CONSTRAINT [' + @var11 + '];');
    EXEC(N'UPDATE [StokFifoKayitlari] SET [ParaBirimi] = N'''' WHERE [ParaBirimi] IS NULL');
    ALTER TABLE [StokFifoKayitlari] ALTER COLUMN [ParaBirimi] nvarchar(3) NOT NULL;
    ALTER TABLE [StokFifoKayitlari] ADD DEFAULT N'' FOR [ParaBirimi];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250405184437_UpdateNullableTypes'
)
BEGIN
    DECLARE @var12 sysname;
    SELECT @var12 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[StokCikisDetaylari]') AND [c].[name] = N'ReferansTuru');
    IF @var12 IS NOT NULL EXEC(N'ALTER TABLE [StokCikisDetaylari] DROP CONSTRAINT [' + @var12 + '];');
    EXEC(N'UPDATE [StokCikisDetaylari] SET [ReferansTuru] = N'''' WHERE [ReferansTuru] IS NULL');
    ALTER TABLE [StokCikisDetaylari] ALTER COLUMN [ReferansTuru] nvarchar(50) NOT NULL;
    ALTER TABLE [StokCikisDetaylari] ADD DEFAULT N'' FOR [ReferansTuru];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250405184437_UpdateNullableTypes'
)
BEGIN
    DECLARE @var13 sysname;
    SELECT @var13 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[StokCikisDetaylari]') AND [c].[name] = N'ReferansNo');
    IF @var13 IS NOT NULL EXEC(N'ALTER TABLE [StokCikisDetaylari] DROP CONSTRAINT [' + @var13 + '];');
    EXEC(N'UPDATE [StokCikisDetaylari] SET [ReferansNo] = N'''' WHERE [ReferansNo] IS NULL');
    ALTER TABLE [StokCikisDetaylari] ALTER COLUMN [ReferansNo] nvarchar(50) NOT NULL;
    ALTER TABLE [StokCikisDetaylari] ADD DEFAULT N'' FOR [ReferansNo];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250405184437_UpdateNullableTypes'
)
BEGIN
    DECLARE @var14 sysname;
    SELECT @var14 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[StokCikisDetaylari]') AND [c].[name] = N'IptalAciklama');
    IF @var14 IS NOT NULL EXEC(N'ALTER TABLE [StokCikisDetaylari] DROP CONSTRAINT [' + @var14 + '];');
    EXEC(N'UPDATE [StokCikisDetaylari] SET [IptalAciklama] = N'''' WHERE [IptalAciklama] IS NULL');
    ALTER TABLE [StokCikisDetaylari] ALTER COLUMN [IptalAciklama] nvarchar(500) NOT NULL;
    ALTER TABLE [StokCikisDetaylari] ADD DEFAULT N'' FOR [IptalAciklama];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250405184437_UpdateNullableTypes'
)
BEGIN
    DECLARE @var15 sysname;
    SELECT @var15 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[StokCikisDetaylari]') AND [c].[name] = N'Aciklama');
    IF @var15 IS NOT NULL EXEC(N'ALTER TABLE [StokCikisDetaylari] DROP CONSTRAINT [' + @var15 + '];');
    EXEC(N'UPDATE [StokCikisDetaylari] SET [Aciklama] = N'''' WHERE [Aciklama] IS NULL');
    ALTER TABLE [StokCikisDetaylari] ALTER COLUMN [Aciklama] nvarchar(500) NOT NULL;
    ALTER TABLE [StokCikisDetaylari] ADD DEFAULT N'' FOR [Aciklama];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250405184437_UpdateNullableTypes'
)
BEGIN
    DECLARE @var16 sysname;
    SELECT @var16 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[SistemLoglar]') AND [c].[name] = N'TabloAdi');
    IF @var16 IS NOT NULL EXEC(N'ALTER TABLE [SistemLoglar] DROP CONSTRAINT [' + @var16 + '];');
    EXEC(N'UPDATE [SistemLoglar] SET [TabloAdi] = N'''' WHERE [TabloAdi] IS NULL');
    ALTER TABLE [SistemLoglar] ALTER COLUMN [TabloAdi] nvarchar(100) NOT NULL;
    ALTER TABLE [SistemLoglar] ADD DEFAULT N'' FOR [TabloAdi];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250405184437_UpdateNullableTypes'
)
BEGIN
    DECLARE @var17 sysname;
    SELECT @var17 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[SistemLoglar]') AND [c].[name] = N'Sayfa');
    IF @var17 IS NOT NULL EXEC(N'ALTER TABLE [SistemLoglar] DROP CONSTRAINT [' + @var17 + '];');
    EXEC(N'UPDATE [SistemLoglar] SET [Sayfa] = N'''' WHERE [Sayfa] IS NULL');
    ALTER TABLE [SistemLoglar] ALTER COLUMN [Sayfa] nvarchar(255) NOT NULL;
    ALTER TABLE [SistemLoglar] ADD DEFAULT N'' FOR [Sayfa];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250405184437_UpdateNullableTypes'
)
BEGIN
    DECLARE @var18 sysname;
    SELECT @var18 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[SistemLoglar]') AND [c].[name] = N'KullaniciAdi');
    IF @var18 IS NOT NULL EXEC(N'ALTER TABLE [SistemLoglar] DROP CONSTRAINT [' + @var18 + '];');
    EXEC(N'UPDATE [SistemLoglar] SET [KullaniciAdi] = N'''' WHERE [KullaniciAdi] IS NULL');
    ALTER TABLE [SistemLoglar] ALTER COLUMN [KullaniciAdi] nvarchar(100) NOT NULL;
    ALTER TABLE [SistemLoglar] ADD DEFAULT N'' FOR [KullaniciAdi];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250405184437_UpdateNullableTypes'
)
BEGIN
    DECLARE @var19 sysname;
    SELECT @var19 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[SistemLoglar]') AND [c].[name] = N'KayitAdi');
    IF @var19 IS NOT NULL EXEC(N'ALTER TABLE [SistemLoglar] DROP CONSTRAINT [' + @var19 + '];');
    EXEC(N'UPDATE [SistemLoglar] SET [KayitAdi] = N'''' WHERE [KayitAdi] IS NULL');
    ALTER TABLE [SistemLoglar] ALTER COLUMN [KayitAdi] nvarchar(100) NOT NULL;
    ALTER TABLE [SistemLoglar] ADD DEFAULT N'' FOR [KayitAdi];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250405184437_UpdateNullableTypes'
)
BEGIN
    DECLARE @var20 sysname;
    SELECT @var20 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[SistemLoglar]') AND [c].[name] = N'IslemTuru');
    IF @var20 IS NOT NULL EXEC(N'ALTER TABLE [SistemLoglar] DROP CONSTRAINT [' + @var20 + '];');
    EXEC(N'UPDATE [SistemLoglar] SET [IslemTuru] = N'''' WHERE [IslemTuru] IS NULL');
    ALTER TABLE [SistemLoglar] ALTER COLUMN [IslemTuru] nvarchar(50) NOT NULL;
    ALTER TABLE [SistemLoglar] ADD DEFAULT N'' FOR [IslemTuru];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250405184437_UpdateNullableTypes'
)
BEGIN
    DECLARE @var21 sysname;
    SELECT @var21 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[SistemLoglar]') AND [c].[name] = N'IPAdresi');
    IF @var21 IS NOT NULL EXEC(N'ALTER TABLE [SistemLoglar] DROP CONSTRAINT [' + @var21 + '];');
    EXEC(N'UPDATE [SistemLoglar] SET [IPAdresi] = N'''' WHERE [IPAdresi] IS NULL');
    ALTER TABLE [SistemLoglar] ALTER COLUMN [IPAdresi] nvarchar(50) NOT NULL;
    ALTER TABLE [SistemLoglar] ADD DEFAULT N'' FOR [IPAdresi];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250405184437_UpdateNullableTypes'
)
BEGIN
    DECLARE @var22 sysname;
    SELECT @var22 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[SistemLoglar]') AND [c].[name] = N'HataMesaji');
    IF @var22 IS NOT NULL EXEC(N'ALTER TABLE [SistemLoglar] DROP CONSTRAINT [' + @var22 + '];');
    EXEC(N'UPDATE [SistemLoglar] SET [HataMesaji] = N'''' WHERE [HataMesaji] IS NULL');
    ALTER TABLE [SistemLoglar] ALTER COLUMN [HataMesaji] nvarchar(500) NOT NULL;
    ALTER TABLE [SistemLoglar] ADD DEFAULT N'' FOR [HataMesaji];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250405184437_UpdateNullableTypes'
)
BEGIN
    DECLARE @var23 sysname;
    SELECT @var23 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[SistemLoglar]') AND [c].[name] = N'Aciklama');
    IF @var23 IS NOT NULL EXEC(N'ALTER TABLE [SistemLoglar] DROP CONSTRAINT [' + @var23 + '];');
    EXEC(N'UPDATE [SistemLoglar] SET [Aciklama] = N'''' WHERE [Aciklama] IS NULL');
    ALTER TABLE [SistemLoglar] ALTER COLUMN [Aciklama] nvarchar(500) NOT NULL;
    ALTER TABLE [SistemLoglar] ADD DEFAULT N'' FOR [Aciklama];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250405184437_UpdateNullableTypes'
)
BEGIN
    DECLARE @var24 sysname;
    SELECT @var24 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[SistemAyarlari]') AND [c].[name] = N'Aciklama');
    IF @var24 IS NOT NULL EXEC(N'ALTER TABLE [SistemAyarlari] DROP CONSTRAINT [' + @var24 + '];');
    EXEC(N'UPDATE [SistemAyarlari] SET [Aciklama] = N'''' WHERE [Aciklama] IS NULL');
    ALTER TABLE [SistemAyarlari] ALTER COLUMN [Aciklama] nvarchar(250) NOT NULL;
    ALTER TABLE [SistemAyarlari] ADD DEFAULT N'' FOR [Aciklama];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250405184437_UpdateNullableTypes'
)
BEGIN
    DECLARE @var25 sysname;
    SELECT @var25 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Menuler]') AND [c].[name] = N'Url');
    IF @var25 IS NOT NULL EXEC(N'ALTER TABLE [Menuler] DROP CONSTRAINT [' + @var25 + '];');
    EXEC(N'UPDATE [Menuler] SET [Url] = N'''' WHERE [Url] IS NULL');
    ALTER TABLE [Menuler] ALTER COLUMN [Url] nvarchar(255) NOT NULL;
    ALTER TABLE [Menuler] ADD DEFAULT N'' FOR [Url];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250405184437_UpdateNullableTypes'
)
BEGIN
    DECLARE @var26 sysname;
    SELECT @var26 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Menuler]') AND [c].[name] = N'Icon');
    IF @var26 IS NOT NULL EXEC(N'ALTER TABLE [Menuler] DROP CONSTRAINT [' + @var26 + '];');
    EXEC(N'UPDATE [Menuler] SET [Icon] = N'''' WHERE [Icon] IS NULL');
    ALTER TABLE [Menuler] ALTER COLUMN [Icon] nvarchar(100) NOT NULL;
    ALTER TABLE [Menuler] ADD DEFAULT N'' FOR [Icon];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250405184437_UpdateNullableTypes'
)
BEGIN
    DECLARE @var27 sysname;
    SELECT @var27 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Menuler]') AND [c].[name] = N'Controller');
    IF @var27 IS NOT NULL EXEC(N'ALTER TABLE [Menuler] DROP CONSTRAINT [' + @var27 + '];');
    EXEC(N'UPDATE [Menuler] SET [Controller] = N'''' WHERE [Controller] IS NULL');
    ALTER TABLE [Menuler] ALTER COLUMN [Controller] nvarchar(100) NOT NULL;
    ALTER TABLE [Menuler] ADD DEFAULT N'' FOR [Controller];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250405184437_UpdateNullableTypes'
)
BEGIN
    DECLARE @var28 sysname;
    SELECT @var28 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Menuler]') AND [c].[name] = N'Action');
    IF @var28 IS NOT NULL EXEC(N'ALTER TABLE [Menuler] DROP CONSTRAINT [' + @var28 + '];');
    EXEC(N'UPDATE [Menuler] SET [Action] = N'''' WHERE [Action] IS NULL');
    ALTER TABLE [Menuler] ALTER COLUMN [Action] nvarchar(100) NOT NULL;
    ALTER TABLE [Menuler] ADD DEFAULT N'' FOR [Action];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250405184437_UpdateNullableTypes'
)
BEGIN
    DECLARE @var29 sysname;
    SELECT @var29 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Kasalar]') AND [c].[name] = N'KasaTuru');
    IF @var29 IS NOT NULL EXEC(N'ALTER TABLE [Kasalar] DROP CONSTRAINT [' + @var29 + '];');
    EXEC(N'UPDATE [Kasalar] SET [KasaTuru] = N'''' WHERE [KasaTuru] IS NULL');
    ALTER TABLE [Kasalar] ALTER COLUMN [KasaTuru] nvarchar(50) NOT NULL;
    ALTER TABLE [Kasalar] ADD DEFAULT N'' FOR [KasaTuru];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250405184437_UpdateNullableTypes'
)
BEGIN
    DECLARE @var30 sysname;
    SELECT @var30 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Kasalar]') AND [c].[name] = N'Aciklama');
    IF @var30 IS NOT NULL EXEC(N'ALTER TABLE [Kasalar] DROP CONSTRAINT [' + @var30 + '];');
    EXEC(N'UPDATE [Kasalar] SET [Aciklama] = N'''' WHERE [Aciklama] IS NULL');
    ALTER TABLE [Kasalar] ALTER COLUMN [Aciklama] nvarchar(500) NOT NULL;
    ALTER TABLE [Kasalar] ADD DEFAULT N'' FOR [Aciklama];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250405184437_UpdateNullableTypes'
)
BEGIN
    DECLARE @var31 sysname;
    SELECT @var31 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[KasaHareketleri]') AND [c].[name] = N'ReferansTuru');
    IF @var31 IS NOT NULL EXEC(N'ALTER TABLE [KasaHareketleri] DROP CONSTRAINT [' + @var31 + '];');
    EXEC(N'UPDATE [KasaHareketleri] SET [ReferansTuru] = N'''' WHERE [ReferansTuru] IS NULL');
    ALTER TABLE [KasaHareketleri] ALTER COLUMN [ReferansTuru] nvarchar(50) NOT NULL;
    ALTER TABLE [KasaHareketleri] ADD DEFAULT N'' FOR [ReferansTuru];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250405184437_UpdateNullableTypes'
)
BEGIN
    DECLARE @var32 sysname;
    SELECT @var32 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[KasaHareketleri]') AND [c].[name] = N'ReferansNo');
    IF @var32 IS NOT NULL EXEC(N'ALTER TABLE [KasaHareketleri] DROP CONSTRAINT [' + @var32 + '];');
    EXEC(N'UPDATE [KasaHareketleri] SET [ReferansNo] = N'''' WHERE [ReferansNo] IS NULL');
    ALTER TABLE [KasaHareketleri] ALTER COLUMN [ReferansNo] nvarchar(50) NOT NULL;
    ALTER TABLE [KasaHareketleri] ADD DEFAULT N'' FOR [ReferansNo];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250405184437_UpdateNullableTypes'
)
BEGIN
    DECLARE @var33 sysname;
    SELECT @var33 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[KasaHareketleri]') AND [c].[name] = N'KarsiParaBirimi');
    IF @var33 IS NOT NULL EXEC(N'ALTER TABLE [KasaHareketleri] DROP CONSTRAINT [' + @var33 + '];');
    EXEC(N'UPDATE [KasaHareketleri] SET [KarsiParaBirimi] = N'''' WHERE [KarsiParaBirimi] IS NULL');
    ALTER TABLE [KasaHareketleri] ALTER COLUMN [KarsiParaBirimi] nvarchar(3) NOT NULL;
    ALTER TABLE [KasaHareketleri] ADD DEFAULT N'' FOR [KarsiParaBirimi];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250405184437_UpdateNullableTypes'
)
BEGIN
    DECLARE @var34 sysname;
    SELECT @var34 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[KasaHareketleri]') AND [c].[name] = N'Aciklama');
    IF @var34 IS NOT NULL EXEC(N'ALTER TABLE [KasaHareketleri] DROP CONSTRAINT [' + @var34 + '];');
    EXEC(N'UPDATE [KasaHareketleri] SET [Aciklama] = N'''' WHERE [Aciklama] IS NULL');
    ALTER TABLE [KasaHareketleri] ALTER COLUMN [Aciklama] nvarchar(500) NOT NULL;
    ALTER TABLE [KasaHareketleri] ADD DEFAULT N'' FOR [Aciklama];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250405184437_UpdateNullableTypes'
)
BEGIN
    DECLARE @var35 sysname;
    SELECT @var35 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[IrsaliyeTurleri]') AND [c].[name] = N'HareketTuru');
    IF @var35 IS NOT NULL EXEC(N'ALTER TABLE [IrsaliyeTurleri] DROP CONSTRAINT [' + @var35 + '];');
    EXEC(N'UPDATE [IrsaliyeTurleri] SET [HareketTuru] = N'''' WHERE [HareketTuru] IS NULL');
    ALTER TABLE [IrsaliyeTurleri] ALTER COLUMN [HareketTuru] nvarchar(50) NOT NULL;
    ALTER TABLE [IrsaliyeTurleri] ADD DEFAULT N'' FOR [HareketTuru];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250405184437_UpdateNullableTypes'
)
BEGIN
    DECLARE @var36 sysname;
    SELECT @var36 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Irsaliyeler]') AND [c].[name] = N'Durum');
    IF @var36 IS NOT NULL EXEC(N'ALTER TABLE [Irsaliyeler] DROP CONSTRAINT [' + @var36 + '];');
    EXEC(N'UPDATE [Irsaliyeler] SET [Durum] = N'''' WHERE [Durum] IS NULL');
    ALTER TABLE [Irsaliyeler] ALTER COLUMN [Durum] nvarchar(20) NOT NULL;
    ALTER TABLE [Irsaliyeler] ADD DEFAULT N'' FOR [Durum];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250405184437_UpdateNullableTypes'
)
BEGIN
    DECLARE @var37 sysname;
    SELECT @var37 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Irsaliyeler]') AND [c].[name] = N'Aciklama');
    IF @var37 IS NOT NULL EXEC(N'ALTER TABLE [Irsaliyeler] DROP CONSTRAINT [' + @var37 + '];');
    EXEC(N'UPDATE [Irsaliyeler] SET [Aciklama] = N'''' WHERE [Aciklama] IS NULL');
    ALTER TABLE [Irsaliyeler] ALTER COLUMN [Aciklama] nvarchar(500) NOT NULL;
    ALTER TABLE [Irsaliyeler] ADD DEFAULT N'' FOR [Aciklama];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250405184437_UpdateNullableTypes'
)
BEGIN
    DECLARE @var38 sysname;
    SELECT @var38 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[IrsaliyeDetaylari]') AND [c].[name] = N'Birim');
    IF @var38 IS NOT NULL EXEC(N'ALTER TABLE [IrsaliyeDetaylari] DROP CONSTRAINT [' + @var38 + '];');
    EXEC(N'UPDATE [IrsaliyeDetaylari] SET [Birim] = N'''' WHERE [Birim] IS NULL');
    ALTER TABLE [IrsaliyeDetaylari] ALTER COLUMN [Birim] nvarchar(50) NOT NULL;
    ALTER TABLE [IrsaliyeDetaylari] ADD DEFAULT N'' FOR [Birim];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250405184437_UpdateNullableTypes'
)
BEGIN
    DECLARE @var39 sysname;
    SELECT @var39 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[IrsaliyeDetaylari]') AND [c].[name] = N'Aciklama');
    IF @var39 IS NOT NULL EXEC(N'ALTER TABLE [IrsaliyeDetaylari] DROP CONSTRAINT [' + @var39 + '];');
    EXEC(N'UPDATE [IrsaliyeDetaylari] SET [Aciklama] = N'''' WHERE [Aciklama] IS NULL');
    ALTER TABLE [IrsaliyeDetaylari] ALTER COLUMN [Aciklama] nvarchar(200) NOT NULL;
    ALTER TABLE [IrsaliyeDetaylari] ADD DEFAULT N'' FOR [Aciklama];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250405184437_UpdateNullableTypes'
)
BEGIN
    DECLARE @var40 sysname;
    SELECT @var40 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[GenelSistemAyarlari]') AND [c].[name] = N'SirketVergiNo');
    IF @var40 IS NOT NULL EXEC(N'ALTER TABLE [GenelSistemAyarlari] DROP CONSTRAINT [' + @var40 + '];');
    EXEC(N'UPDATE [GenelSistemAyarlari] SET [SirketVergiNo] = N'''' WHERE [SirketVergiNo] IS NULL');
    ALTER TABLE [GenelSistemAyarlari] ALTER COLUMN [SirketVergiNo] nvarchar(20) NOT NULL;
    ALTER TABLE [GenelSistemAyarlari] ADD DEFAULT N'' FOR [SirketVergiNo];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250405184437_UpdateNullableTypes'
)
BEGIN
    DECLARE @var41 sysname;
    SELECT @var41 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[GenelSistemAyarlari]') AND [c].[name] = N'SirketVergiDairesi');
    IF @var41 IS NOT NULL EXEC(N'ALTER TABLE [GenelSistemAyarlari] DROP CONSTRAINT [' + @var41 + '];');
    EXEC(N'UPDATE [GenelSistemAyarlari] SET [SirketVergiDairesi] = N'''' WHERE [SirketVergiDairesi] IS NULL');
    ALTER TABLE [GenelSistemAyarlari] ALTER COLUMN [SirketVergiDairesi] nvarchar(20) NOT NULL;
    ALTER TABLE [GenelSistemAyarlari] ADD DEFAULT N'' FOR [SirketVergiDairesi];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250405184437_UpdateNullableTypes'
)
BEGIN
    DECLARE @var42 sysname;
    SELECT @var42 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[GenelSistemAyarlari]') AND [c].[name] = N'SirketTelefon');
    IF @var42 IS NOT NULL EXEC(N'ALTER TABLE [GenelSistemAyarlari] DROP CONSTRAINT [' + @var42 + '];');
    EXEC(N'UPDATE [GenelSistemAyarlari] SET [SirketTelefon] = N'''' WHERE [SirketTelefon] IS NULL');
    ALTER TABLE [GenelSistemAyarlari] ALTER COLUMN [SirketTelefon] nvarchar(20) NOT NULL;
    ALTER TABLE [GenelSistemAyarlari] ADD DEFAULT N'' FOR [SirketTelefon];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250405184437_UpdateNullableTypes'
)
BEGIN
    DECLARE @var43 sysname;
    SELECT @var43 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[GenelSistemAyarlari]') AND [c].[name] = N'SirketEmail');
    IF @var43 IS NOT NULL EXEC(N'ALTER TABLE [GenelSistemAyarlari] DROP CONSTRAINT [' + @var43 + '];');
    EXEC(N'UPDATE [GenelSistemAyarlari] SET [SirketEmail] = N'''' WHERE [SirketEmail] IS NULL');
    ALTER TABLE [GenelSistemAyarlari] ALTER COLUMN [SirketEmail] nvarchar(100) NOT NULL;
    ALTER TABLE [GenelSistemAyarlari] ADD DEFAULT N'' FOR [SirketEmail];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250405184437_UpdateNullableTypes'
)
BEGIN
    DECLARE @var44 sysname;
    SELECT @var44 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[GenelSistemAyarlari]') AND [c].[name] = N'SirketAdresi');
    IF @var44 IS NOT NULL EXEC(N'ALTER TABLE [GenelSistemAyarlari] DROP CONSTRAINT [' + @var44 + '];');
    EXEC(N'UPDATE [GenelSistemAyarlari] SET [SirketAdresi] = N'''' WHERE [SirketAdresi] IS NULL');
    ALTER TABLE [GenelSistemAyarlari] ALTER COLUMN [SirketAdresi] nvarchar(100) NOT NULL;
    ALTER TABLE [GenelSistemAyarlari] ADD DEFAULT N'' FOR [SirketAdresi];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250405184437_UpdateNullableTypes'
)
BEGIN
    DECLARE @var45 sysname;
    SELECT @var45 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[FaturaTurleri]') AND [c].[name] = N'HareketTuru');
    IF @var45 IS NOT NULL EXEC(N'ALTER TABLE [FaturaTurleri] DROP CONSTRAINT [' + @var45 + '];');
    EXEC(N'UPDATE [FaturaTurleri] SET [HareketTuru] = N'''' WHERE [HareketTuru] IS NULL');
    ALTER TABLE [FaturaTurleri] ALTER COLUMN [HareketTuru] nvarchar(50) NOT NULL;
    ALTER TABLE [FaturaTurleri] ADD DEFAULT N'' FOR [HareketTuru];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250405184437_UpdateNullableTypes'
)
BEGIN
    DECLARE @var46 sysname;
    SELECT @var46 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[FaturaOdemeleri]') AND [c].[name] = N'Aciklama');
    IF @var46 IS NOT NULL EXEC(N'ALTER TABLE [FaturaOdemeleri] DROP CONSTRAINT [' + @var46 + '];');
    EXEC(N'UPDATE [FaturaOdemeleri] SET [Aciklama] = N'''' WHERE [Aciklama] IS NULL');
    ALTER TABLE [FaturaOdemeleri] ALTER COLUMN [Aciklama] nvarchar(500) NOT NULL;
    ALTER TABLE [FaturaOdemeleri] ADD DEFAULT N'' FOR [Aciklama];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250405184437_UpdateNullableTypes'
)
BEGIN
    DECLARE @var47 sysname;
    SELECT @var47 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Faturalar]') AND [c].[name] = N'OdemeDurumu');
    IF @var47 IS NOT NULL EXEC(N'ALTER TABLE [Faturalar] DROP CONSTRAINT [' + @var47 + '];');
    EXEC(N'UPDATE [Faturalar] SET [OdemeDurumu] = N'''' WHERE [OdemeDurumu] IS NULL');
    ALTER TABLE [Faturalar] ALTER COLUMN [OdemeDurumu] nvarchar(50) NOT NULL;
    ALTER TABLE [Faturalar] ADD DEFAULT N'' FOR [OdemeDurumu];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250405184437_UpdateNullableTypes'
)
BEGIN
    DECLARE @var48 sysname;
    SELECT @var48 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Faturalar]') AND [c].[name] = N'FaturaNotu');
    IF @var48 IS NOT NULL EXEC(N'ALTER TABLE [Faturalar] DROP CONSTRAINT [' + @var48 + '];');
    EXEC(N'UPDATE [Faturalar] SET [FaturaNotu] = N'''' WHERE [FaturaNotu] IS NULL');
    ALTER TABLE [Faturalar] ALTER COLUMN [FaturaNotu] nvarchar(500) NOT NULL;
    ALTER TABLE [Faturalar] ADD DEFAULT N'' FOR [FaturaNotu];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250405184437_UpdateNullableTypes'
)
BEGIN
    DECLARE @var49 sysname;
    SELECT @var49 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Faturalar]') AND [c].[name] = N'DovizTuru');
    IF @var49 IS NOT NULL EXEC(N'ALTER TABLE [Faturalar] DROP CONSTRAINT [' + @var49 + '];');
    EXEC(N'UPDATE [Faturalar] SET [DovizTuru] = N'''' WHERE [DovizTuru] IS NULL');
    ALTER TABLE [Faturalar] ALTER COLUMN [DovizTuru] nvarchar(10) NOT NULL;
    ALTER TABLE [Faturalar] ADD DEFAULT N'' FOR [DovizTuru];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250405184437_UpdateNullableTypes'
)
BEGIN
    DECLARE @var50 sysname;
    SELECT @var50 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Faturalar]') AND [c].[name] = N'AklanmaNotu');
    IF @var50 IS NOT NULL EXEC(N'ALTER TABLE [Faturalar] DROP CONSTRAINT [' + @var50 + '];');
    EXEC(N'UPDATE [Faturalar] SET [AklanmaNotu] = N'''' WHERE [AklanmaNotu] IS NULL');
    ALTER TABLE [Faturalar] ALTER COLUMN [AklanmaNotu] nvarchar(500) NOT NULL;
    ALTER TABLE [Faturalar] ADD DEFAULT N'' FOR [AklanmaNotu];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250405184437_UpdateNullableTypes'
)
BEGIN
    DECLARE @var51 sysname;
    SELECT @var51 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[FaturaAklamaKuyrugu]') AND [c].[name] = N'AklanmaNotu');
    IF @var51 IS NOT NULL EXEC(N'ALTER TABLE [FaturaAklamaKuyrugu] DROP CONSTRAINT [' + @var51 + '];');
    EXEC(N'UPDATE [FaturaAklamaKuyrugu] SET [AklanmaNotu] = N'''' WHERE [AklanmaNotu] IS NULL');
    ALTER TABLE [FaturaAklamaKuyrugu] ALTER COLUMN [AklanmaNotu] nvarchar(500) NOT NULL;
    ALTER TABLE [FaturaAklamaKuyrugu] ADD DEFAULT N'' FOR [AklanmaNotu];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250405184437_UpdateNullableTypes'
)
BEGIN
    DECLARE @var52 sysname;
    SELECT @var52 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Depolar]') AND [c].[name] = N'Adres');
    IF @var52 IS NOT NULL EXEC(N'ALTER TABLE [Depolar] DROP CONSTRAINT [' + @var52 + '];');
    EXEC(N'UPDATE [Depolar] SET [Adres] = N'''' WHERE [Adres] IS NULL');
    ALTER TABLE [Depolar] ALTER COLUMN [Adres] nvarchar(200) NOT NULL;
    ALTER TABLE [Depolar] ADD DEFAULT N'' FOR [Adres];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250405184437_UpdateNullableTypes'
)
BEGIN
    DECLARE @var53 sysname;
    SELECT @var53 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Birimler]') AND [c].[name] = N'Aciklama');
    IF @var53 IS NOT NULL EXEC(N'ALTER TABLE [Birimler] DROP CONSTRAINT [' + @var53 + '];');
    EXEC(N'UPDATE [Birimler] SET [Aciklama] = N'''' WHERE [Aciklama] IS NULL');
    ALTER TABLE [Birimler] ALTER COLUMN [Aciklama] nvarchar(200) NOT NULL;
    ALTER TABLE [Birimler] ADD DEFAULT N'' FOR [Aciklama];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250405184437_UpdateNullableTypes'
)
BEGIN
    DECLARE @var54 sysname;
    SELECT @var54 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[BankaHesaplari]') AND [c].[name] = N'SubeKodu');
    IF @var54 IS NOT NULL EXEC(N'ALTER TABLE [BankaHesaplari] DROP CONSTRAINT [' + @var54 + '];');
    EXEC(N'UPDATE [BankaHesaplari] SET [SubeKodu] = N'''' WHERE [SubeKodu] IS NULL');
    ALTER TABLE [BankaHesaplari] ALTER COLUMN [SubeKodu] nvarchar(50) NOT NULL;
    ALTER TABLE [BankaHesaplari] ADD DEFAULT N'' FOR [SubeKodu];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250405184437_UpdateNullableTypes'
)
BEGIN
    DECLARE @var55 sysname;
    SELECT @var55 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[BankaHesaplari]') AND [c].[name] = N'SubeAdi');
    IF @var55 IS NOT NULL EXEC(N'ALTER TABLE [BankaHesaplari] DROP CONSTRAINT [' + @var55 + '];');
    EXEC(N'UPDATE [BankaHesaplari] SET [SubeAdi] = N'''' WHERE [SubeAdi] IS NULL');
    ALTER TABLE [BankaHesaplari] ALTER COLUMN [SubeAdi] nvarchar(100) NOT NULL;
    ALTER TABLE [BankaHesaplari] ADD DEFAULT N'' FOR [SubeAdi];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250405184437_UpdateNullableTypes'
)
BEGIN
    DECLARE @var56 sysname;
    SELECT @var56 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[BankaHesaplari]') AND [c].[name] = N'ParaBirimi');
    IF @var56 IS NOT NULL EXEC(N'ALTER TABLE [BankaHesaplari] DROP CONSTRAINT [' + @var56 + '];');
    EXEC(N'UPDATE [BankaHesaplari] SET [ParaBirimi] = N''TRY'' WHERE [ParaBirimi] IS NULL');
    ALTER TABLE [BankaHesaplari] ALTER COLUMN [ParaBirimi] nvarchar(10) NOT NULL;
    ALTER TABLE [BankaHesaplari] ADD DEFAULT N'TRY' FOR [ParaBirimi];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250405184437_UpdateNullableTypes'
)
BEGIN
    DECLARE @var57 sysname;
    SELECT @var57 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[BankaHesaplari]') AND [c].[name] = N'IBAN');
    IF @var57 IS NOT NULL EXEC(N'ALTER TABLE [BankaHesaplari] DROP CONSTRAINT [' + @var57 + '];');
    EXEC(N'UPDATE [BankaHesaplari] SET [IBAN] = N'''' WHERE [IBAN] IS NULL');
    ALTER TABLE [BankaHesaplari] ALTER COLUMN [IBAN] nvarchar(50) NOT NULL;
    ALTER TABLE [BankaHesaplari] ADD DEFAULT N'' FOR [IBAN];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250405184437_UpdateNullableTypes'
)
BEGIN
    DECLARE @var58 sysname;
    SELECT @var58 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[BankaHesaplari]') AND [c].[name] = N'HesapNo');
    IF @var58 IS NOT NULL EXEC(N'ALTER TABLE [BankaHesaplari] DROP CONSTRAINT [' + @var58 + '];');
    EXEC(N'UPDATE [BankaHesaplari] SET [HesapNo] = N'''' WHERE [HesapNo] IS NULL');
    ALTER TABLE [BankaHesaplari] ALTER COLUMN [HesapNo] nvarchar(50) NOT NULL;
    ALTER TABLE [BankaHesaplari] ADD DEFAULT N'' FOR [HesapNo];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250405184437_UpdateNullableTypes'
)
BEGIN
    DECLARE @var59 sysname;
    SELECT @var59 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[BankaHesaplari]') AND [c].[name] = N'Aciklama');
    IF @var59 IS NOT NULL EXEC(N'ALTER TABLE [BankaHesaplari] DROP CONSTRAINT [' + @var59 + '];');
    EXEC(N'UPDATE [BankaHesaplari] SET [Aciklama] = N'''' WHERE [Aciklama] IS NULL');
    ALTER TABLE [BankaHesaplari] ALTER COLUMN [Aciklama] nvarchar(500) NOT NULL;
    ALTER TABLE [BankaHesaplari] ADD DEFAULT N'' FOR [Aciklama];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250405184437_UpdateNullableTypes'
)
BEGIN
    DECLARE @var60 sysname;
    SELECT @var60 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[BankaHesapHareketleri]') AND [c].[name] = N'ReferansTuru');
    IF @var60 IS NOT NULL EXEC(N'ALTER TABLE [BankaHesapHareketleri] DROP CONSTRAINT [' + @var60 + '];');
    EXEC(N'UPDATE [BankaHesapHareketleri] SET [ReferansTuru] = N'''' WHERE [ReferansTuru] IS NULL');
    ALTER TABLE [BankaHesapHareketleri] ALTER COLUMN [ReferansTuru] nvarchar(50) NOT NULL;
    ALTER TABLE [BankaHesapHareketleri] ADD DEFAULT N'' FOR [ReferansTuru];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250405184437_UpdateNullableTypes'
)
BEGIN
    DECLARE @var61 sysname;
    SELECT @var61 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[BankaHesapHareketleri]') AND [c].[name] = N'ReferansNo');
    IF @var61 IS NOT NULL EXEC(N'ALTER TABLE [BankaHesapHareketleri] DROP CONSTRAINT [' + @var61 + '];');
    EXEC(N'UPDATE [BankaHesapHareketleri] SET [ReferansNo] = N'''' WHERE [ReferansNo] IS NULL');
    ALTER TABLE [BankaHesapHareketleri] ALTER COLUMN [ReferansNo] nvarchar(50) NOT NULL;
    ALTER TABLE [BankaHesapHareketleri] ADD DEFAULT N'' FOR [ReferansNo];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250405184437_UpdateNullableTypes'
)
BEGIN
    DECLARE @var62 sysname;
    SELECT @var62 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[BankaHesapHareketleri]') AND [c].[name] = N'KarsiUnvan');
    IF @var62 IS NOT NULL EXEC(N'ALTER TABLE [BankaHesapHareketleri] DROP CONSTRAINT [' + @var62 + '];');
    EXEC(N'UPDATE [BankaHesapHareketleri] SET [KarsiUnvan] = N'''' WHERE [KarsiUnvan] IS NULL');
    ALTER TABLE [BankaHesapHareketleri] ALTER COLUMN [KarsiUnvan] nvarchar(200) NOT NULL;
    ALTER TABLE [BankaHesapHareketleri] ADD DEFAULT N'' FOR [KarsiUnvan];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250405184437_UpdateNullableTypes'
)
BEGIN
    DECLARE @var63 sysname;
    SELECT @var63 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[BankaHesapHareketleri]') AND [c].[name] = N'KarsiIBAN');
    IF @var63 IS NOT NULL EXEC(N'ALTER TABLE [BankaHesapHareketleri] DROP CONSTRAINT [' + @var63 + '];');
    EXEC(N'UPDATE [BankaHesapHareketleri] SET [KarsiIBAN] = N'''' WHERE [KarsiIBAN] IS NULL');
    ALTER TABLE [BankaHesapHareketleri] ALTER COLUMN [KarsiIBAN] nvarchar(50) NOT NULL;
    ALTER TABLE [BankaHesapHareketleri] ADD DEFAULT N'' FOR [KarsiIBAN];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250405184437_UpdateNullableTypes'
)
BEGIN
    DECLARE @var64 sysname;
    SELECT @var64 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[BankaHesapHareketleri]') AND [c].[name] = N'KarsiBankaAdi');
    IF @var64 IS NOT NULL EXEC(N'ALTER TABLE [BankaHesapHareketleri] DROP CONSTRAINT [' + @var64 + '];');
    EXEC(N'UPDATE [BankaHesapHareketleri] SET [KarsiBankaAdi] = N'''' WHERE [KarsiBankaAdi] IS NULL');
    ALTER TABLE [BankaHesapHareketleri] ALTER COLUMN [KarsiBankaAdi] nvarchar(50) NOT NULL;
    ALTER TABLE [BankaHesapHareketleri] ADD DEFAULT N'' FOR [KarsiBankaAdi];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250405184437_UpdateNullableTypes'
)
BEGIN
    DECLARE @var65 sysname;
    SELECT @var65 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[BankaHesapHareketleri]') AND [c].[name] = N'DekontNo');
    IF @var65 IS NOT NULL EXEC(N'ALTER TABLE [BankaHesapHareketleri] DROP CONSTRAINT [' + @var65 + '];');
    EXEC(N'UPDATE [BankaHesapHareketleri] SET [DekontNo] = N'''' WHERE [DekontNo] IS NULL');
    ALTER TABLE [BankaHesapHareketleri] ALTER COLUMN [DekontNo] nvarchar(50) NOT NULL;
    ALTER TABLE [BankaHesapHareketleri] ADD DEFAULT N'' FOR [DekontNo];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250405184437_UpdateNullableTypes'
)
BEGIN
    DECLARE @var66 sysname;
    SELECT @var66 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[BankaHesapHareketleri]') AND [c].[name] = N'Aciklama');
    IF @var66 IS NOT NULL EXEC(N'ALTER TABLE [BankaHesapHareketleri] DROP CONSTRAINT [' + @var66 + '];');
    EXEC(N'UPDATE [BankaHesapHareketleri] SET [Aciklama] = N'''' WHERE [Aciklama] IS NULL');
    ALTER TABLE [BankaHesapHareketleri] ALTER COLUMN [Aciklama] nvarchar(500) NOT NULL;
    ALTER TABLE [BankaHesapHareketleri] ADD DEFAULT N'' FOR [Aciklama];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250405184437_UpdateNullableTypes'
)
BEGIN
    DECLARE @var67 sysname;
    SELECT @var67 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[BankaHareketleri]') AND [c].[name] = N'ReferansTuru');
    IF @var67 IS NOT NULL EXEC(N'ALTER TABLE [BankaHareketleri] DROP CONSTRAINT [' + @var67 + '];');
    EXEC(N'UPDATE [BankaHareketleri] SET [ReferansTuru] = N'''' WHERE [ReferansTuru] IS NULL');
    ALTER TABLE [BankaHareketleri] ALTER COLUMN [ReferansTuru] nvarchar(50) NOT NULL;
    ALTER TABLE [BankaHareketleri] ADD DEFAULT N'' FOR [ReferansTuru];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250405184437_UpdateNullableTypes'
)
BEGIN
    DECLARE @var68 sysname;
    SELECT @var68 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[BankaHareketleri]') AND [c].[name] = N'ReferansNo');
    IF @var68 IS NOT NULL EXEC(N'ALTER TABLE [BankaHareketleri] DROP CONSTRAINT [' + @var68 + '];');
    EXEC(N'UPDATE [BankaHareketleri] SET [ReferansNo] = N'''' WHERE [ReferansNo] IS NULL');
    ALTER TABLE [BankaHareketleri] ALTER COLUMN [ReferansNo] nvarchar(50) NOT NULL;
    ALTER TABLE [BankaHareketleri] ADD DEFAULT N'' FOR [ReferansNo];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250405184437_UpdateNullableTypes'
)
BEGIN
    DECLARE @var69 sysname;
    SELECT @var69 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[BankaHareketleri]') AND [c].[name] = N'KarsiUnvan');
    IF @var69 IS NOT NULL EXEC(N'ALTER TABLE [BankaHareketleri] DROP CONSTRAINT [' + @var69 + '];');
    EXEC(N'UPDATE [BankaHareketleri] SET [KarsiUnvan] = N'''' WHERE [KarsiUnvan] IS NULL');
    ALTER TABLE [BankaHareketleri] ALTER COLUMN [KarsiUnvan] nvarchar(200) NOT NULL;
    ALTER TABLE [BankaHareketleri] ADD DEFAULT N'' FOR [KarsiUnvan];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250405184437_UpdateNullableTypes'
)
BEGIN
    DECLARE @var70 sysname;
    SELECT @var70 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[BankaHareketleri]') AND [c].[name] = N'KarsiIBAN');
    IF @var70 IS NOT NULL EXEC(N'ALTER TABLE [BankaHareketleri] DROP CONSTRAINT [' + @var70 + '];');
    EXEC(N'UPDATE [BankaHareketleri] SET [KarsiIBAN] = N'''' WHERE [KarsiIBAN] IS NULL');
    ALTER TABLE [BankaHareketleri] ALTER COLUMN [KarsiIBAN] nvarchar(50) NOT NULL;
    ALTER TABLE [BankaHareketleri] ADD DEFAULT N'' FOR [KarsiIBAN];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250405184437_UpdateNullableTypes'
)
BEGIN
    DECLARE @var71 sysname;
    SELECT @var71 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[BankaHareketleri]') AND [c].[name] = N'KarsiBankaAdi');
    IF @var71 IS NOT NULL EXEC(N'ALTER TABLE [BankaHareketleri] DROP CONSTRAINT [' + @var71 + '];');
    EXEC(N'UPDATE [BankaHareketleri] SET [KarsiBankaAdi] = N'''' WHERE [KarsiBankaAdi] IS NULL');
    ALTER TABLE [BankaHareketleri] ALTER COLUMN [KarsiBankaAdi] nvarchar(50) NOT NULL;
    ALTER TABLE [BankaHareketleri] ADD DEFAULT N'' FOR [KarsiBankaAdi];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250405184437_UpdateNullableTypes'
)
BEGIN
    DECLARE @var72 sysname;
    SELECT @var72 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[BankaHareketleri]') AND [c].[name] = N'DekontNo');
    IF @var72 IS NOT NULL EXEC(N'ALTER TABLE [BankaHareketleri] DROP CONSTRAINT [' + @var72 + '];');
    EXEC(N'UPDATE [BankaHareketleri] SET [DekontNo] = N'''' WHERE [DekontNo] IS NULL');
    ALTER TABLE [BankaHareketleri] ALTER COLUMN [DekontNo] nvarchar(50) NOT NULL;
    ALTER TABLE [BankaHareketleri] ADD DEFAULT N'' FOR [DekontNo];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250405184437_UpdateNullableTypes'
)
BEGIN
    DECLARE @var73 sysname;
    SELECT @var73 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[BankaHareketleri]') AND [c].[name] = N'Aciklama');
    IF @var73 IS NOT NULL EXEC(N'ALTER TABLE [BankaHareketleri] DROP CONSTRAINT [' + @var73 + '];');
    EXEC(N'UPDATE [BankaHareketleri] SET [Aciklama] = N'''' WHERE [Aciklama] IS NULL');
    ALTER TABLE [BankaHareketleri] ALTER COLUMN [Aciklama] nvarchar(500) NOT NULL;
    ALTER TABLE [BankaHareketleri] ADD DEFAULT N'' FOR [Aciklama];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250405184437_UpdateNullableTypes'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250405184437_UpdateNullableTypes', N'9.0.4');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250405192713_FixParaBirimiController'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250405192713_FixParaBirimiController', N'9.0.4');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250406081455_FixFaturaTuruIDType'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250406081455_FixFaturaTuruIDType', N'9.0.4');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250406082948_FaturaNullableUpdate'
)
BEGIN
    DECLARE @var74 sysname;
    SELECT @var74 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Faturalar]') AND [c].[name] = N'FaturaNotu');
    IF @var74 IS NOT NULL EXEC(N'ALTER TABLE [Faturalar] DROP CONSTRAINT [' + @var74 + '];');
    ALTER TABLE [Faturalar] ALTER COLUMN [FaturaNotu] nvarchar(500) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250406082948_FaturaNullableUpdate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250406082948_FaturaNullableUpdate', N'9.0.4');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250406090612_UpdateFaturaIrsaliyeRelations'
)
BEGIN
    DECLARE @var75 sysname;
    SELECT @var75 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Faturalar]') AND [c].[name] = N'FaturaNotu');
    IF @var75 IS NOT NULL EXEC(N'ALTER TABLE [Faturalar] DROP CONSTRAINT [' + @var75 + '];');
    EXEC(N'UPDATE [Faturalar] SET [FaturaNotu] = N'''' WHERE [FaturaNotu] IS NULL');
    ALTER TABLE [Faturalar] ALTER COLUMN [FaturaNotu] nvarchar(500) NOT NULL;
    ALTER TABLE [Faturalar] ADD DEFAULT N'' FOR [FaturaNotu];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250406090612_UpdateFaturaIrsaliyeRelations'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250406090612_UpdateFaturaIrsaliyeRelations', N'9.0.4');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250406090823_FaturaIrsaliyeAndStokUpdate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250406090823_FaturaIrsaliyeAndStokUpdate', N'9.0.4');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250406152325_FaturaDeleteiFix'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250406152325_FaturaDeleteiFix', N'9.0.4');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250407114623_FixCariHareketJsonPropertyNames'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250407114623_FixCariHareketJsonPropertyNames', N'9.0.4');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250407184127_FixParaBirimiAdiProperty'
)
BEGIN
    ALTER TABLE [ParaBirimleri] ADD [ParaBirimiAdi] nvarchar(max) NOT NULL DEFAULT N'';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250407184127_FixParaBirimiAdiProperty'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250407184127_FixParaBirimiAdiProperty', N'9.0.4');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250407185215_FixParaBirimiAdiIssue'
)
BEGIN
    DECLARE @var76 sysname;
    SELECT @var76 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ParaBirimleri]') AND [c].[name] = N'ParaBirimiAdi');
    IF @var76 IS NOT NULL EXEC(N'ALTER TABLE [ParaBirimleri] DROP CONSTRAINT [' + @var76 + '];');
    ALTER TABLE [ParaBirimleri] DROP COLUMN [ParaBirimiAdi];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250407185215_FixParaBirimiAdiIssue'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250407185215_FixParaBirimiAdiIssue', N'9.0.4');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250407220114_FixNullableCariIDConversion'
)
BEGIN
    DECLARE @var77 sysname;
    SELECT @var77 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[BankaHesapHareketleri]') AND [c].[name] = N'KarsiUnvan');
    IF @var77 IS NOT NULL EXEC(N'ALTER TABLE [BankaHesapHareketleri] DROP CONSTRAINT [' + @var77 + '];');
    ALTER TABLE [BankaHesapHareketleri] ALTER COLUMN [KarsiUnvan] nvarchar(200) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250407220114_FixNullableCariIDConversion'
)
BEGIN
    DECLARE @var78 sysname;
    SELECT @var78 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[BankaHesapHareketleri]') AND [c].[name] = N'KarsiIBAN');
    IF @var78 IS NOT NULL EXEC(N'ALTER TABLE [BankaHesapHareketleri] DROP CONSTRAINT [' + @var78 + '];');
    ALTER TABLE [BankaHesapHareketleri] ALTER COLUMN [KarsiIBAN] nvarchar(50) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250407220114_FixNullableCariIDConversion'
)
BEGIN
    DECLARE @var79 sysname;
    SELECT @var79 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[BankaHesapHareketleri]') AND [c].[name] = N'KarsiBankaAdi');
    IF @var79 IS NOT NULL EXEC(N'ALTER TABLE [BankaHesapHareketleri] DROP CONSTRAINT [' + @var79 + '];');
    ALTER TABLE [BankaHesapHareketleri] ALTER COLUMN [KarsiBankaAdi] nvarchar(50) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250407220114_FixNullableCariIDConversion'
)
BEGIN
    ALTER TABLE [BankaHesapHareketleri] ADD [KarsiParaBirimi] nvarchar(10) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250407220114_FixNullableCariIDConversion'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250407220114_FixNullableCariIDConversion', N'9.0.4');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250407220930_FixFaturaForeignKeyConstraints'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250407220930_FixFaturaForeignKeyConstraints', N'9.0.4');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250407221605_FixSozlesmeIdNullability'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250407221605_FixSozlesmeIdNullability', N'9.0.4');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250407221756_FixKarsiParaBirimi'
)
BEGIN
    DECLARE @var80 sysname;
    SELECT @var80 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[BankaHesapHareketleri]') AND [c].[name] = N'KarsiParaBirimi');
    IF @var80 IS NOT NULL EXEC(N'ALTER TABLE [BankaHesapHareketleri] DROP CONSTRAINT [' + @var80 + '];');
    EXEC(N'UPDATE [BankaHesapHareketleri] SET [KarsiParaBirimi] = N'''' WHERE [KarsiParaBirimi] IS NULL');
    ALTER TABLE [BankaHesapHareketleri] ALTER COLUMN [KarsiParaBirimi] nvarchar(10) NOT NULL;
    ALTER TABLE [BankaHesapHareketleri] ADD DEFAULT N'' FOR [KarsiParaBirimi];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250407221756_FixKarsiParaBirimi'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250407221756_FixKarsiParaBirimi', N'9.0.4');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250407222844_FixFirstOrDefaultWithOrderBy'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250407222844_FixFirstOrDefaultWithOrderBy', N'9.0.4');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250408120223_RemoveEffectiveRatesFromKurDegeri'
)
BEGIN
    ALTER TABLE [KurDegerleri] DROP CONSTRAINT [FK_KurDegerleri_ParaBirimleri_ParaBirimiID];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250408120223_RemoveEffectiveRatesFromKurDegeri'
)
BEGIN
    DROP INDEX [IX_KurDegeri_ParaBirimiID_Tarih] ON [KurDegerleri];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250408120223_RemoveEffectiveRatesFromKurDegeri'
)
BEGIN
    DECLARE @var81 sysname;
    SELECT @var81 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[KurDegerleri]') AND [c].[name] = N'Efektif_Alis');
    IF @var81 IS NOT NULL EXEC(N'ALTER TABLE [KurDegerleri] DROP CONSTRAINT [' + @var81 + '];');
    ALTER TABLE [KurDegerleri] DROP COLUMN [Efektif_Alis];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250408120223_RemoveEffectiveRatesFromKurDegeri'
)
BEGIN
    DECLARE @var82 sysname;
    SELECT @var82 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[KurDegerleri]') AND [c].[name] = N'Efektif_Satis');
    IF @var82 IS NOT NULL EXEC(N'ALTER TABLE [KurDegerleri] DROP CONSTRAINT [' + @var82 + '];');
    ALTER TABLE [KurDegerleri] DROP COLUMN [Efektif_Satis];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250408120223_RemoveEffectiveRatesFromKurDegeri'
)
BEGIN
    DECLARE @var83 sysname;
    SELECT @var83 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[KurDegerleri]') AND [c].[name] = N'Aktif');
    IF @var83 IS NOT NULL EXEC(N'ALTER TABLE [KurDegerleri] DROP CONSTRAINT [' + @var83 + '];');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250408120223_RemoveEffectiveRatesFromKurDegeri'
)
BEGIN
    CREATE TABLE [KurMarjlari] (
        [KurMarjID] uniqueidentifier NOT NULL,
        [SatisMarji] decimal(5,2) NOT NULL,
        [Varsayilan] bit NOT NULL,
        [Tanim] nvarchar(100) NOT NULL,
        [Aktif] bit NOT NULL,
        [Silindi] bit NOT NULL,
        [OlusturmaTarihi] datetime2 NOT NULL,
        [GuncellemeTarihi] datetime2 NULL,
        [OlusturanKullaniciID] nvarchar(max) NULL,
        [SonGuncelleyenKullaniciID] nvarchar(max) NULL,
        CONSTRAINT [PK_KurMarjlari] PRIMARY KEY ([KurMarjID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250408120223_RemoveEffectiveRatesFromKurDegeri'
)
BEGIN
    CREATE INDEX [IX_KurDegerleri_ParaBirimiID] ON [KurDegerleri] ([ParaBirimiID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250408120223_RemoveEffectiveRatesFromKurDegeri'
)
BEGIN
    ALTER TABLE [KurDegerleri] ADD CONSTRAINT [FK_KurDegerleri_ParaBirimleri_ParaBirimiID] FOREIGN KEY ([ParaBirimiID]) REFERENCES [ParaBirimleri] ([ParaBirimiID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250408120223_RemoveEffectiveRatesFromKurDegeri'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250408120223_RemoveEffectiveRatesFromKurDegeri', N'9.0.4');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250408125520_UpdateKurDegeriViewModel'
)
BEGIN
    ALTER TABLE [KurDegerleri] ADD [DekontNo] nvarchar(50) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250408125520_UpdateKurDegeriViewModel'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250408125520_UpdateKurDegeriViewModel', N'9.0.4');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250408135551_RemoveCariGlobalQueryFilter'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250408135551_RemoveCariGlobalQueryFilter', N'9.0.4');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250408193706_CariNullableFields'
)
BEGIN
    DECLARE @var84 sysname;
    SELECT @var84 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Cariler]') AND [c].[name] = N'WebSitesi');
    IF @var84 IS NOT NULL EXEC(N'ALTER TABLE [Cariler] DROP CONSTRAINT [' + @var84 + '];');
    ALTER TABLE [Cariler] ALTER COLUMN [WebSitesi] nvarchar(100) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250408193706_CariNullableFields'
)
BEGIN
    DECLARE @var85 sysname;
    SELECT @var85 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Cariler]') AND [c].[name] = N'VergiNo');
    IF @var85 IS NOT NULL EXEC(N'ALTER TABLE [Cariler] DROP CONSTRAINT [' + @var85 + '];');
    ALTER TABLE [Cariler] ALTER COLUMN [VergiNo] nvarchar(11) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250408193706_CariNullableFields'
)
BEGIN
    DECLARE @var86 sysname;
    SELECT @var86 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Cariler]') AND [c].[name] = N'VergiDairesi');
    IF @var86 IS NOT NULL EXEC(N'ALTER TABLE [Cariler] DROP CONSTRAINT [' + @var86 + '];');
    ALTER TABLE [Cariler] ALTER COLUMN [VergiDairesi] nvarchar(50) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250408193706_CariNullableFields'
)
BEGIN
    DECLARE @var87 sysname;
    SELECT @var87 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Cariler]') AND [c].[name] = N'Aciklama');
    IF @var87 IS NOT NULL EXEC(N'ALTER TABLE [Cariler] DROP CONSTRAINT [' + @var87 + '];');
    ALTER TABLE [Cariler] ALTER COLUMN [Aciklama] nvarchar(500) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250408193706_CariNullableFields'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250408193706_CariNullableFields', N'9.0.4');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250409122816_AddSilindiToFaturaTuru'
)
BEGIN
    ALTER TABLE [FaturaTurleri] ADD [Silindi] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250409122816_AddSilindiToFaturaTuru'
)
BEGIN
    ALTER TABLE [Faturalar] ADD [IndirimTutari] decimal(18,2) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250409122816_AddSilindiToFaturaTuru'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250409122816_AddSilindiToFaturaTuru', N'9.0.4');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250409180150_StokMiktarFix'
)
BEGIN
    DECLARE @var88 sysname;
    SELECT @var88 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Urunler]') AND [c].[name] = N'StokMiktar');
    IF @var88 IS NOT NULL EXEC(N'ALTER TABLE [Urunler] DROP CONSTRAINT [' + @var88 + '];');
    ALTER TABLE [Urunler] DROP COLUMN [StokMiktar];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250409180150_StokMiktarFix'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250409180150_StokMiktarFix', N'9.0.4');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250409191433_KDVFormatFix'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250409191433_KDVFormatFix', N'9.0.4');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250409193910_FixKDVOrani'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250409193910_FixKDVOrani', N'9.0.4');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250409194810_DataTablesFixAndExcelExport'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250409194810_DataTablesFixAndExcelExport', N'9.0.4');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250410092309_FormattingAndDisplayFixMigration'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250410092309_FormattingAndDisplayFixMigration', N'9.0.4');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250410092808_FixCategoryDropdownIssue'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250410092808_FixCategoryDropdownIssue', N'9.0.4');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250410094413_ExcelImportExportFunctionality'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250410094413_ExcelImportExportFunctionality', N'9.0.4');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250410163754_SetDeleteAndRestoreDeletedMethods'
)
BEGIN
    ALTER TABLE [Urunler] ADD [DovizliListeFiyati] decimal(18,2) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250410163754_SetDeleteAndRestoreDeletedMethods'
)
BEGIN
    ALTER TABLE [Urunler] ADD [DovizliMaliyetFiyati] decimal(18,2) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250410163754_SetDeleteAndRestoreDeletedMethods'
)
BEGIN
    ALTER TABLE [Urunler] ADD [DovizliSatisFiyati] decimal(18,2) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250410163754_SetDeleteAndRestoreDeletedMethods'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250410163754_SetDeleteAndRestoreDeletedMethods', N'9.0.4');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250410164115_FixDecimalColumnTypes'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250410164115_FixDecimalColumnTypes', N'9.0.4');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250410194128_UpdateDecimalColumnTypes'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250410194128_UpdateDecimalColumnTypes', N'9.0.4');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250411095322_AddUrunAciklamaColumn'
)
BEGIN
    ALTER TABLE [Urunler] ADD [Aciklama] nvarchar(500) NOT NULL DEFAULT N'';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250411095322_AddUrunAciklamaColumn'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250411095322_AddUrunAciklamaColumn', N'9.0.4');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250411115047_AddParaBirimiToStokHareket'
)
BEGIN
    ALTER TABLE [StokHareketleri] ADD [ParaBirimi] nvarchar(10) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250411115047_AddParaBirimiToStokHareket'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250411115047_AddParaBirimiToStokHareket', N'9.0.4');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250411171017_AddDovizFields'
)
BEGIN
    ALTER TABLE [Faturalar] ADD [AraToplamDoviz] decimal(18,2) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250411171017_AddDovizFields'
)
BEGIN
    ALTER TABLE [Faturalar] ADD [GenelToplamDoviz] decimal(18,2) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250411171017_AddDovizFields'
)
BEGIN
    ALTER TABLE [Faturalar] ADD [IndirimTutariDoviz] decimal(18,2) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250411171017_AddDovizFields'
)
BEGIN
    ALTER TABLE [Faturalar] ADD [KDVToplamDoviz] decimal(18,2) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250411171017_AddDovizFields'
)
BEGIN
    ALTER TABLE [FaturaDetaylari] ADD [BirimFiyatDoviz] decimal(18,2) NOT NULL DEFAULT 0.0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250411171017_AddDovizFields'
)
BEGIN
    ALTER TABLE [FaturaDetaylari] ADD [IndirimTutariDoviz] decimal(18,2) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250411171017_AddDovizFields'
)
BEGIN
    ALTER TABLE [FaturaDetaylari] ADD [KdvTutariDoviz] decimal(18,2) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250411171017_AddDovizFields'
)
BEGIN
    ALTER TABLE [FaturaDetaylari] ADD [NetTutarDoviz] decimal(18,2) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250411171017_AddDovizFields'
)
BEGIN
    ALTER TABLE [FaturaDetaylari] ADD [TutarDoviz] decimal(18,2) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250411171017_AddDovizFields'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250411171017_AddDovizFields', N'9.0.4');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250414113155_AddMissingColumns'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250414113155_AddMissingColumns', N'9.0.4');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250414113845_EfektifKurlarEklendi'
)
BEGIN
    ALTER TABLE [KurDegerleri] ADD [EfektifAlis] decimal(18,6) NOT NULL DEFAULT 0.0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250414113845_EfektifKurlarEklendi'
)
BEGIN
    ALTER TABLE [KurDegerleri] ADD [EfektifSatis] decimal(18,6) NOT NULL DEFAULT 0.0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250414113845_EfektifKurlarEklendi'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250414113845_EfektifKurlarEklendi', N'9.0.4');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250414114127_InitialMigrationFix'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250414114127_InitialMigrationFix', N'9.0.4');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250414192501_UpdateSistemLogWithKullaniciIDRelation'
)
BEGIN
    ALTER TABLE [CariHareketler] DROP CONSTRAINT [FK_CariHareketler_Cariler_CariId];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250414192501_UpdateSistemLogWithKullaniciIDRelation'
)
BEGIN
    ALTER TABLE [SistemLoglar] DROP CONSTRAINT [FK_SistemLoglar_AspNetUsers_ApplicationUserId];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250414192501_UpdateSistemLogWithKullaniciIDRelation'
)
BEGIN
    DROP INDEX [IX_SistemLoglar_ApplicationUserId] ON [SistemLoglar];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250414192501_UpdateSistemLogWithKullaniciIDRelation'
)
BEGIN
    DECLARE @var89 sysname;
    SELECT @var89 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[SistemLoglar]') AND [c].[name] = N'ApplicationUserId');
    IF @var89 IS NOT NULL EXEC(N'ALTER TABLE [SistemLoglar] DROP CONSTRAINT [' + @var89 + '];');
    ALTER TABLE [SistemLoglar] DROP COLUMN [ApplicationUserId];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250414192501_UpdateSistemLogWithKullaniciIDRelation'
)
BEGIN
    EXEC sp_rename N'[SistemLoglar].[KullaniciID]', N'KullaniciId', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250414192501_UpdateSistemLogWithKullaniciIDRelation'
)
BEGIN
    DECLARE @var90 sysname;
    SELECT @var90 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[SistemLoglar]') AND [c].[name] = N'TabloAdi');
    IF @var90 IS NOT NULL EXEC(N'ALTER TABLE [SistemLoglar] DROP CONSTRAINT [' + @var90 + '];');
    ALTER TABLE [SistemLoglar] ALTER COLUMN [TabloAdi] nvarchar(250) NOT NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250414192501_UpdateSistemLogWithKullaniciIDRelation'
)
BEGIN
    DECLARE @var91 sysname;
    SELECT @var91 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[SistemLoglar]') AND [c].[name] = N'Sayfa');
    IF @var91 IS NOT NULL EXEC(N'ALTER TABLE [SistemLoglar] DROP CONSTRAINT [' + @var91 + '];');
    ALTER TABLE [SistemLoglar] ALTER COLUMN [Sayfa] nvarchar(250) NOT NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250414192501_UpdateSistemLogWithKullaniciIDRelation'
)
BEGIN
    DECLARE @var92 sysname;
    SELECT @var92 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[SistemLoglar]') AND [c].[name] = N'Mesaj');
    IF @var92 IS NOT NULL EXEC(N'ALTER TABLE [SistemLoglar] DROP CONSTRAINT [' + @var92 + '];');
    ALTER TABLE [SistemLoglar] ALTER COLUMN [Mesaj] nvarchar(2000) NOT NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250414192501_UpdateSistemLogWithKullaniciIDRelation'
)
BEGIN
    DECLARE @var93 sysname;
    SELECT @var93 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[SistemLoglar]') AND [c].[name] = N'KullaniciId');
    IF @var93 IS NOT NULL EXEC(N'ALTER TABLE [SistemLoglar] DROP CONSTRAINT [' + @var93 + '];');
    ALTER TABLE [SistemLoglar] ALTER COLUMN [KullaniciId] varchar(128) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250414192501_UpdateSistemLogWithKullaniciIDRelation'
)
BEGIN
    DECLARE @var94 sysname;
    SELECT @var94 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[SistemLoglar]') AND [c].[name] = N'KullaniciAdi');
    IF @var94 IS NOT NULL EXEC(N'ALTER TABLE [SistemLoglar] DROP CONSTRAINT [' + @var94 + '];');
    ALTER TABLE [SistemLoglar] ALTER COLUMN [KullaniciAdi] nvarchar(max) NOT NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250414192501_UpdateSistemLogWithKullaniciIDRelation'
)
BEGIN
    DECLARE @var95 sysname;
    SELECT @var95 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[SistemLoglar]') AND [c].[name] = N'KayitAdi');
    IF @var95 IS NOT NULL EXEC(N'ALTER TABLE [SistemLoglar] DROP CONSTRAINT [' + @var95 + '];');
    ALTER TABLE [SistemLoglar] ALTER COLUMN [KayitAdi] nvarchar(250) NOT NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250414192501_UpdateSistemLogWithKullaniciIDRelation'
)
BEGIN
    DECLARE @var96 sysname;
    SELECT @var96 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[SistemLoglar]') AND [c].[name] = N'HataMesaji');
    IF @var96 IS NOT NULL EXEC(N'ALTER TABLE [SistemLoglar] DROP CONSTRAINT [' + @var96 + '];');
    ALTER TABLE [SistemLoglar] ALTER COLUMN [HataMesaji] nvarchar(max) NOT NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250414192501_UpdateSistemLogWithKullaniciIDRelation'
)
BEGIN
    DECLARE @var97 sysname;
    SELECT @var97 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[SistemLoglar]') AND [c].[name] = N'Aciklama');
    IF @var97 IS NOT NULL EXEC(N'ALTER TABLE [SistemLoglar] DROP CONSTRAINT [' + @var97 + '];');
    ALTER TABLE [SistemLoglar] ALTER COLUMN [Aciklama] nvarchar(max) NOT NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250414192501_UpdateSistemLogWithKullaniciIDRelation'
)
BEGIN
    DECLARE @var98 sysname;
    SELECT @var98 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Cariler]') AND [c].[name] = N'VergiNo');
    IF @var98 IS NOT NULL EXEC(N'ALTER TABLE [Cariler] DROP CONSTRAINT [' + @var98 + '];');
    ALTER TABLE [Cariler] ALTER COLUMN [VergiNo] nvarchar(20) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250414192501_UpdateSistemLogWithKullaniciIDRelation'
)
BEGIN
    DECLARE @var99 sysname;
    SELECT @var99 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Cariler]') AND [c].[name] = N'CariKodu');
    IF @var99 IS NOT NULL EXEC(N'ALTER TABLE [Cariler] DROP CONSTRAINT [' + @var99 + '];');
    ALTER TABLE [Cariler] ALTER COLUMN [CariKodu] nvarchar(20) NOT NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250414192501_UpdateSistemLogWithKullaniciIDRelation'
)
BEGIN
    ALTER TABLE [CariHareketler] ADD [CariID1] uniqueidentifier NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250414192501_UpdateSistemLogWithKullaniciIDRelation'
)
BEGIN
    CREATE INDEX [IX_SistemLoglar_KullaniciId] ON [SistemLoglar] ([KullaniciId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250414192501_UpdateSistemLogWithKullaniciIDRelation'
)
BEGIN
    CREATE INDEX [IX_CariHareketler_CariID1] ON [CariHareketler] ([CariID1]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250414192501_UpdateSistemLogWithKullaniciIDRelation'
)
BEGIN
    ALTER TABLE [CariHareketler] ADD CONSTRAINT [FK_CariHareketler_Cariler_CariID1] FOREIGN KEY ([CariID1]) REFERENCES [Cariler] ([CariID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250414192501_UpdateSistemLogWithKullaniciIDRelation'
)
BEGIN
    ALTER TABLE [CariHareketler] ADD CONSTRAINT [FK_CariHareketler_Cariler_CariId] FOREIGN KEY ([CariId]) REFERENCES [Cariler] ([CariID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250414192501_UpdateSistemLogWithKullaniciIDRelation'
)
BEGIN
    ALTER TABLE [SistemLoglar] ADD CONSTRAINT [FK_SistemLoglar_AspNetUsers_KullaniciId] FOREIGN KEY ([KullaniciId]) REFERENCES [AspNetUsers] ([Id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250414192501_UpdateSistemLogWithKullaniciIDRelation'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250414192501_UpdateSistemLogWithKullaniciIDRelation', N'9.0.4');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250414195705_AddOdenenTutarToFatura'
)
BEGIN
    ALTER TABLE [Faturalar] ADD [OdenenTutar] decimal(18,2) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250414195705_AddOdenenTutarToFatura'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250414195705_AddOdenenTutarToFatura', N'9.0.4');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250414203128_AddParaBirimiToFatura'
)
BEGIN
    ALTER TABLE [CariHareketler] DROP CONSTRAINT [FK_CariHareketler_Cariler_CariID1];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250414203128_AddParaBirimiToFatura'
)
BEGIN
    ALTER TABLE [CariHareketler] DROP CONSTRAINT [FK_CariHareketler_Cariler_CariId];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250414203128_AddParaBirimiToFatura'
)
BEGIN
    DROP INDEX [IX_CariHareketler_CariID1] ON [CariHareketler];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250414203128_AddParaBirimiToFatura'
)
BEGIN
    DECLARE @var100 sysname;
    SELECT @var100 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[CariHareketler]') AND [c].[name] = N'CariID1');
    IF @var100 IS NOT NULL EXEC(N'ALTER TABLE [CariHareketler] DROP CONSTRAINT [' + @var100 + '];');
    ALTER TABLE [CariHareketler] DROP COLUMN [CariID1];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250414203128_AddParaBirimiToFatura'
)
BEGIN
    ALTER TABLE [Faturalar] ADD [ParaBirimi] nvarchar(10) NOT NULL DEFAULT N'';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250414203128_AddParaBirimiToFatura'
)
BEGIN
    ALTER TABLE [CariHareketler] ADD CONSTRAINT [FK_CariHareketler_Cariler_CariId] FOREIGN KEY ([CariId]) REFERENCES [Cariler] ([CariID]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250414203128_AddParaBirimiToFatura'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250414203128_AddParaBirimiToFatura', N'9.0.4');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250415080113_AddPropertyForStokCikisDetay'
)
BEGIN
    ALTER TABLE [StokCikisDetaylari] ADD [BirimMaliyet] decimal(18,6) NOT NULL DEFAULT 0.0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250415080113_AddPropertyForStokCikisDetay'
)
BEGIN
    ALTER TABLE [StokCikisDetaylari] ADD [HareketTipi] nvarchar(50) NOT NULL DEFAULT N'';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250415080113_AddPropertyForStokCikisDetay'
)
BEGIN
    ALTER TABLE [StokCikisDetaylari] ADD [ParaBirimi] nvarchar(10) NOT NULL DEFAULT N'';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250415080113_AddPropertyForStokCikisDetay'
)
BEGIN
    ALTER TABLE [StokCikisDetaylari] ADD [ToplamMaliyet] decimal(18,6) NOT NULL DEFAULT 0.0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250415080113_AddPropertyForStokCikisDetay'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250415080113_AddPropertyForStokCikisDetay', N'9.0.4');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250415101737_FixSistemLogKullaniciIdColumnIssue'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250415101737_FixSistemLogKullaniciIdColumnIssue', N'9.0.4');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250415162845_NormalizeFaturaParaAlanlari'
)
BEGIN

                    IF OBJECT_ID('FaturaAklamaKuyrugu', 'U') IS NOT NULL
                        DELETE FROM FaturaAklamaKuyrugu;
                    IF OBJECT_ID('FaturaOdemeleri', 'U') IS NOT NULL
                        DELETE FROM FaturaOdemeleri;
                    IF OBJECT_ID('FaturaDetaylari', 'U') IS NOT NULL
                        DELETE FROM FaturaDetaylari;
                    IF OBJECT_ID('Faturalar', 'U') IS NOT NULL
                        DELETE FROM Faturalar;
                
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250415162845_NormalizeFaturaParaAlanlari'
)
BEGIN
    DECLARE @var101 sysname;
    SELECT @var101 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Faturalar]') AND [c].[name] = N'AraToplam');
    IF @var101 IS NOT NULL EXEC(N'ALTER TABLE [Faturalar] DROP CONSTRAINT [' + @var101 + '];');
    ALTER TABLE [Faturalar] ALTER COLUMN [AraToplam] decimal(18,2) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250415162845_NormalizeFaturaParaAlanlari'
)
BEGIN
    DECLARE @var102 sysname;
    SELECT @var102 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Faturalar]') AND [c].[name] = N'KDVToplam');
    IF @var102 IS NOT NULL EXEC(N'ALTER TABLE [Faturalar] DROP CONSTRAINT [' + @var102 + '];');
    ALTER TABLE [Faturalar] ALTER COLUMN [KDVToplam] decimal(18,2) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250415162845_NormalizeFaturaParaAlanlari'
)
BEGIN
    DECLARE @var103 sysname;
    SELECT @var103 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Faturalar]') AND [c].[name] = N'IndirimTutari');
    IF @var103 IS NOT NULL EXEC(N'ALTER TABLE [Faturalar] DROP CONSTRAINT [' + @var103 + '];');
    ALTER TABLE [Faturalar] ALTER COLUMN [IndirimTutari] decimal(18,2) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250415162845_NormalizeFaturaParaAlanlari'
)
BEGIN
    DECLARE @var104 sysname;
    SELECT @var104 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Faturalar]') AND [c].[name] = N'GenelToplam');
    IF @var104 IS NOT NULL EXEC(N'ALTER TABLE [Faturalar] DROP CONSTRAINT [' + @var104 + '];');
    ALTER TABLE [Faturalar] ALTER COLUMN [GenelToplam] decimal(18,2) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250415162845_NormalizeFaturaParaAlanlari'
)
BEGIN
    DECLARE @var105 sysname;
    SELECT @var105 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Faturalar]') AND [c].[name] = N'AraToplamDoviz');
    IF @var105 IS NOT NULL EXEC(N'ALTER TABLE [Faturalar] DROP CONSTRAINT [' + @var105 + '];');
    ALTER TABLE [Faturalar] ALTER COLUMN [AraToplamDoviz] decimal(18,2) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250415162845_NormalizeFaturaParaAlanlari'
)
BEGIN
    DECLARE @var106 sysname;
    SELECT @var106 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Faturalar]') AND [c].[name] = N'KDVToplamDoviz');
    IF @var106 IS NOT NULL EXEC(N'ALTER TABLE [Faturalar] DROP CONSTRAINT [' + @var106 + '];');
    ALTER TABLE [Faturalar] ALTER COLUMN [KDVToplamDoviz] decimal(18,2) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250415162845_NormalizeFaturaParaAlanlari'
)
BEGIN
    DECLARE @var107 sysname;
    SELECT @var107 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Faturalar]') AND [c].[name] = N'IndirimTutariDoviz');
    IF @var107 IS NOT NULL EXEC(N'ALTER TABLE [Faturalar] DROP CONSTRAINT [' + @var107 + '];');
    ALTER TABLE [Faturalar] ALTER COLUMN [IndirimTutariDoviz] decimal(18,2) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250415162845_NormalizeFaturaParaAlanlari'
)
BEGIN
    DECLARE @var108 sysname;
    SELECT @var108 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Faturalar]') AND [c].[name] = N'GenelToplamDoviz');
    IF @var108 IS NOT NULL EXEC(N'ALTER TABLE [Faturalar] DROP CONSTRAINT [' + @var108 + '];');
    ALTER TABLE [Faturalar] ALTER COLUMN [GenelToplamDoviz] decimal(18,2) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250415162845_NormalizeFaturaParaAlanlari'
)
BEGIN
    DECLARE @var109 sysname;
    SELECT @var109 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Faturalar]') AND [c].[name] = N'OdenenTutar');
    IF @var109 IS NOT NULL EXEC(N'ALTER TABLE [Faturalar] DROP CONSTRAINT [' + @var109 + '];');
    ALTER TABLE [Faturalar] ALTER COLUMN [OdenenTutar] decimal(18,2) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250415162845_NormalizeFaturaParaAlanlari'
)
BEGIN

                    IF OBJECT_ID('FaturaDetaylari', 'U') IS NOT NULL
                    BEGIN
                        IF COL_LENGTH('FaturaDetaylari', 'BirimFiyat') IS NOT NULL
                            ALTER TABLE FaturaDetaylari ALTER COLUMN BirimFiyat decimal(18,2);
                        
                        IF COL_LENGTH('FaturaDetaylari', 'BirimFiyatDoviz') IS NOT NULL
                            ALTER TABLE FaturaDetaylari ALTER COLUMN BirimFiyatDoviz decimal(18,2);
                        
                        IF COL_LENGTH('FaturaDetaylari', 'Tutar') IS NOT NULL
                            ALTER TABLE FaturaDetaylari ALTER COLUMN Tutar decimal(18,2);
                        
                        IF COL_LENGTH('FaturaDetaylari', 'NetTutar') IS NOT NULL
                            ALTER TABLE FaturaDetaylari ALTER COLUMN NetTutar decimal(18,2);
                    END
                
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250415162845_NormalizeFaturaParaAlanlari'
)
BEGIN

                    IF OBJECT_ID('FaturaOdemeleri', 'U') IS NOT NULL
                    BEGIN
                        IF COL_LENGTH('FaturaOdemeleri', 'OdemeTutari') IS NOT NULL
                            ALTER TABLE FaturaOdemeleri ALTER COLUMN OdemeTutari decimal(18,2);
                    END
                
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250415162845_NormalizeFaturaParaAlanlari'
)
BEGIN

                    IF OBJECT_ID('FaturaAklamaKuyrugu', 'U') IS NOT NULL
                    BEGIN
                        IF COL_LENGTH('FaturaAklamaKuyrugu', 'AklananTutar') IS NOT NULL
                            ALTER TABLE FaturaAklamaKuyrugu ALTER COLUMN AklananTutar decimal(18,2);
                        
                        IF COL_LENGTH('FaturaAklamaKuyrugu', 'AklananTutarDoviz') IS NOT NULL
                            ALTER TABLE FaturaAklamaKuyrugu ALTER COLUMN AklananTutarDoviz decimal(18,2);
                    END
                
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250415162845_NormalizeFaturaParaAlanlari'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250415162845_NormalizeFaturaParaAlanlari', N'9.0.4');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250415165508_MenuTablosuDuzenleme'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250415165508_MenuTablosuDuzenleme', N'9.0.4');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250415170704_FixSistemLogKullaniciIdColumn'
)
BEGIN

                    -- Önce sistemin hangi veritabanı sürümünde olduğunu kontrol edelim
                    DECLARE @IsSQLServer2016OrHigher BIT = 0;
                    IF SERVERPROPERTY('ProductMajorVersion') >= 13
                        SET @IsSQLServer2016OrHigher = 1;

                    -- SistemLoglar tablosunda KullaniciId sütununun var olup olmadığını kontrol et
                    IF EXISTS (
                        SELECT 1 FROM sys.columns 
                        WHERE object_id = OBJECT_ID('SistemLoglar') 
                        AND name = 'KullaniciId'
                    )
                    BEGIN
                        -- Foreign key varsa kaldır
                        IF EXISTS (
                            SELECT 1 FROM sys.foreign_keys 
                            WHERE parent_object_id = OBJECT_ID('SistemLoglar') 
                            AND name = 'FK_SistemLoglar_AspNetUsers_KullaniciId'
                        )
                        BEGIN
                            ALTER TABLE SistemLoglar DROP CONSTRAINT FK_SistemLoglar_AspNetUsers_KullaniciId;
                        END

                        -- Index varsa kaldır
                        IF EXISTS (
                            SELECT 1 FROM sys.indexes 
                            WHERE object_id = OBJECT_ID('SistemLoglar') 
                            AND name = 'IX_SistemLoglar_KullaniciId'
                        )
                        BEGIN
                            DROP INDEX IX_SistemLoglar_KullaniciId ON SistemLoglar;
                        END

                        -- Geçici bir tablo oluştur
                        SELECT * INTO #TempSistemLog FROM SistemLoglar WHERE 1=0;
                        
                        -- Geçici tabloya sütun ekle
                        ALTER TABLE #TempSistemLog ADD TempKullaniciId uniqueidentifier NULL;
                        
                        -- Mevcut verileri geçici tabloya kopyala, KullaniciID değerini al
                        IF (@IsSQLServer2016OrHigher = 1)
                        BEGIN
                            -- SQL Server 2016 ve üzeri için
                            INSERT INTO #TempSistemLog (
                                Id, LogID, LogTuru, Mesaj, Sayfa, OlusturmaTarihi, IslemTuru, 
                                LogTuruInt, Aciklama, HataMesaji, KullaniciAdi, IPAdresi, 
                                IslemTarihi, Basarili, TabloAdi, KayitAdi, KayitID, TempKullaniciId
                            )
                            SELECT 
                                Id, LogID, LogTuru, Mesaj, Sayfa, OlusturmaTarihi, IslemTuru, 
                                LogTuruInt, Aciklama, HataMesaji, KullaniciAdi, IPAdresi, 
                                IslemTarihi, Basarili, TabloAdi, KayitAdi, KayitID, KullaniciID
                            FROM SistemLoglar;
                        END
                        ELSE
                        BEGIN
                            -- Daha eski SQL Server sürümleri için
                            INSERT INTO #TempSistemLog (
                                Id, LogID, LogTuru, Mesaj, Sayfa, OlusturmaTarihi, IslemTuru, 
                                LogTuruInt, Aciklama, HataMesaji, KullaniciAdi, IPAdresi, 
                                IslemTarihi, Basarili, TabloAdi, KayitAdi, KayitID, TempKullaniciId
                            )
                            SELECT 
                                Id, LogID, LogTuru, Mesaj, Sayfa, OlusturmaTarihi, IslemTuru, 
                                LogTuruInt, Aciklama, HataMesaji, KullaniciAdi, IPAdresi, 
                                IslemTarihi, Basarili, TabloAdi, KayitAdi, KayitID, KullaniciID
                            FROM SistemLoglar;
                        END
                        
                        -- Mevcut tabloyu temizle
                        TRUNCATE TABLE SistemLoglar;
                        
                        -- Tablodan KullaniciId ve KullaniciID sütunlarını kaldır (varsa)
                        IF EXISTS (
                            SELECT 1 FROM sys.columns 
                            WHERE object_id = OBJECT_ID('SistemLoglar') 
                            AND name = 'KullaniciId'
                        )
                        BEGIN
                            ALTER TABLE SistemLoglar DROP COLUMN KullaniciId;
                        END

                        IF EXISTS (
                            SELECT 1 FROM sys.columns 
                            WHERE object_id = OBJECT_ID('SistemLoglar') 
                            AND name = 'KullaniciID'
                        )
                        BEGIN
                            ALTER TABLE SistemLoglar DROP COLUMN KullaniciID;
                        END
                        
                        -- Yeni KullaniciId sütununu ekle
                        ALTER TABLE SistemLoglar ADD KullaniciId uniqueidentifier NULL;
                        
                        -- Geçici tablodan verileri geri kopyala
                        INSERT INTO SistemLoglar (
                            Id, LogID, LogTuru, Mesaj, Sayfa, OlusturmaTarihi, IslemTuru, 
                            LogTuruInt, Aciklama, HataMesaji, KullaniciAdi, IPAdresi, 
                            IslemTarihi, Basarili, TabloAdi, KayitAdi, KayitID, KullaniciId
                        )
                        SELECT 
                            Id, LogID, LogTuru, Mesaj, Sayfa, OlusturmaTarihi, IslemTuru, 
                            LogTuruInt, Aciklama, HataMesaji, KullaniciAdi, IPAdresi, 
                            IslemTarihi, Basarili, TabloAdi, KayitAdi, KayitID, TempKullaniciId
                        FROM #TempSistemLog;
                        
                        -- Geçici tabloyu temizle
                        DROP TABLE #TempSistemLog;
                        
                        -- Foreign key ve indeksi tekrar oluştur
                        CREATE INDEX IX_SistemLoglar_KullaniciId ON SistemLoglar(KullaniciId);
                        
                        ALTER TABLE SistemLoglar ADD CONSTRAINT FK_SistemLoglar_AspNetUsers_KullaniciId 
                        FOREIGN KEY (KullaniciId) REFERENCES AspNetUsers(Id);
                    END
                
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250415170704_FixSistemLogKullaniciIdColumn'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250415170704_FixSistemLogKullaniciIdColumn', N'9.0.4');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250415173639_FixDatabaseModels'
)
BEGIN
    DECLARE @var110 sysname;
    SELECT @var110 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[SistemLoglar]') AND [c].[name] = N'KullaniciID');
    IF @var110 IS NOT NULL EXEC(N'ALTER TABLE [SistemLoglar] DROP CONSTRAINT [' + @var110 + '];');
    ALTER TABLE [SistemLoglar] DROP COLUMN [KullaniciID];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250415173639_FixDatabaseModels'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250415173639_FixDatabaseModels', N'9.0.4');
END;

COMMIT;
GO


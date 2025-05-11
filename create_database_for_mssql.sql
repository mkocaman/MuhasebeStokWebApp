-- Önce veritabanı var mı kontrol et, varsa sil
USE [master]
GO

IF EXISTS(SELECT 1 FROM sys.databases WHERE name = 'MuhasebeStokDB')
BEGIN
    ALTER DATABASE [MuhasebeStokDB] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE [MuhasebeStokDB];
END
GO

-- Veritabanını oluştur (farklı lokasyonda)
CREATE DATABASE [MuhasebeStokDB]
ON PRIMARY 
(
    NAME = N'MuhasebeStokDB', 
    FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL16.MSSQLSERVER\MSSQL\DATA\MuhasebeStokDB_Primary.mdf',
    SIZE = 8MB,
    MAXSIZE = UNLIMITED,
    FILEGROWTH = 65536KB
)
LOG ON 
(
    NAME = N'MuhasebeStokDB_log', 
    FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL16.MSSQLSERVER\MSSQL\DATA\MuhasebeStokDB_Primary_log.ldf',
    SIZE = 8MB,
    MAXSIZE = 2048GB,
    FILEGROWTH = 65536KB
)
GO

-- Veritabanını kullan
USE [MuhasebeStokDB]
GO

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
    [Id] varchar(128) NOT NULL,
    [Name] nvarchar(256) NULL,
    [NormalizedName] nvarchar(256) NULL,
    [ConcurrencyStamp] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
);

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

CREATE TABLE [Bankalar] (
    [BankaID] uniqueidentifier NOT NULL,
    [BankaAdi] nvarchar(100) NOT NULL,
    [Aktif] bit NOT NULL DEFAULT CAST(1 AS bit),
    [OlusturanKullaniciID] uniqueidentifier NULL,
    [SonGuncelleyenKullaniciID] uniqueidentifier NULL,
    [OlusturmaTarihi] datetime2 NOT NULL,
    [GuncellemeTarihi] datetime2 NULL,
    [Silindi] bit NOT NULL DEFAULT CAST(0 AS bit),
    CONSTRAINT [PK_Bankalar] PRIMARY KEY ([BankaID])
);

CREATE TABLE [Birimler] (
    [BirimID] uniqueidentifier NOT NULL,
    [BirimAdi] nvarchar(50) NOT NULL,
    [BirimKodu] nvarchar(20) NOT NULL,
    [BirimSembol] nvarchar(10) NOT NULL,
    [Aciklama] nvarchar(200) NOT NULL,
    [Aktif] bit NOT NULL,
    [Silindi] bit NOT NULL,
    [OlusturmaTarihi] datetime2 NOT NULL,
    [GuncellemeTarihi] datetime2 NULL,
    [OlusturanKullaniciID] nvarchar(450) NOT NULL,
    [SonGuncelleyenKullaniciID] uniqueidentifier NULL,
    [SirketID] uniqueidentifier NULL,
    CONSTRAINT [PK_Birimler] PRIMARY KEY ([BirimID])
);

CREATE TABLE [BirlesikModulParaBirimleri] (
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
    [Aktif] bit NOT NULL,
    [Silindi] bit NOT NULL,
    [OlusturmaTarihi] datetime2 NOT NULL,
    [GuncellemeTarihi] datetime2 NULL,
    [OlusturanKullaniciID] nvarchar(max) NOT NULL,
    [SonGuncelleyenKullaniciID] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_BirlesikModulParaBirimleri] PRIMARY KEY ([ParaBirimiID])
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
    [Silindi] bit NOT NULL DEFAULT CAST(0 AS bit),
    CONSTRAINT [PK_Depolar] PRIMARY KEY ([DepoID])
);

CREATE TABLE [FaturaTurleri] (
    [FaturaTuruID] int NOT NULL IDENTITY,
    [FaturaTuruAdi] nvarchar(50) NOT NULL,
    [HareketTuru] nvarchar(50) NOT NULL,
    [Silindi] bit NOT NULL,
    CONSTRAINT [PK_FaturaTurleri] PRIMARY KEY ([FaturaTuruID])
);

CREATE TABLE [FiyatTipleri] (
    [FiyatTipiID] int NOT NULL IDENTITY,
    [TipAdi] nvarchar(50) NOT NULL,
    CONSTRAINT [PK_FiyatTipleri] PRIMARY KEY ([FiyatTipiID])
);

CREATE TABLE [GenelSistemAyarlari] (
    [SistemAyarlariID] int NOT NULL IDENTITY,
    [AnaDovizKodu] nvarchar(10) NOT NULL,
    [SirketAdi] nvarchar(50) NOT NULL,
    [SirketAdresi] nvarchar(100) NOT NULL,
    [SirketTelefon] nvarchar(20) NOT NULL,
    [SirketEmail] nvarchar(100) NOT NULL,
    [SirketVergiNo] nvarchar(20) NOT NULL,
    [SirketVergiDairesi] nvarchar(20) NOT NULL,
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
    [Silindi] bit NOT NULL DEFAULT CAST(0 AS bit),
    CONSTRAINT [PK_Kasalar] PRIMARY KEY ([KasaID])
);

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

CREATE TABLE [Menuler] (
    [MenuID] uniqueidentifier NOT NULL,
    [Ad] nvarchar(100) NOT NULL,
    [Icon] nvarchar(100) NOT NULL,
    [Controller] nvarchar(100) NOT NULL,
    [Action] nvarchar(100) NOT NULL,
    [Sira] int NOT NULL,
    [UstMenuID] uniqueidentifier NULL,
    [Url] nvarchar(255) NOT NULL,
    [AktifMi] bit NOT NULL DEFAULT CAST(1 AS bit),
    [Silindi] bit NOT NULL DEFAULT CAST(0 AS bit),
    [OlusturmaTarihi] datetime2 NOT NULL,
    [GuncellemeTarihi] datetime2 NULL,
    [OlusturanKullaniciID] uniqueidentifier NULL,
    [SonGuncelleyenKullaniciID] uniqueidentifier NULL,
    CONSTRAINT [PK_Menuler] PRIMARY KEY ([MenuID]),
    CONSTRAINT [FK_Menuler_Menuler_UstMenuID] FOREIGN KEY ([UstMenuID]) REFERENCES [Menuler] ([MenuID])
);

CREATE TABLE [OdemeTurleri] (
    [OdemeTuruID] int NOT NULL IDENTITY,
    [OdemeTuruAdi] nvarchar(50) NOT NULL,
    CONSTRAINT [PK_OdemeTurleri] PRIMARY KEY ([OdemeTuruID])
);

CREATE TABLE [ParaBirimiModuluParaBirimleri] (
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
    CONSTRAINT [PK_ParaBirimiModuluParaBirimleri] PRIMARY KEY ([ParaBirimiID])
);

CREATE TABLE [SistemAyarlari] (
    [Id] int NOT NULL IDENTITY,
    [Anahtar] nvarchar(100) NOT NULL,
    [Deger] nvarchar(500) NOT NULL,
    [Aciklama] nvarchar(250) NOT NULL,
    [OlusturmaTarihi] datetime2 NOT NULL,
    [GuncellemeTarihi] datetime2 NOT NULL,
    [Silindi] bit NOT NULL,
    CONSTRAINT [PK_SistemAyarlari] PRIMARY KEY ([Id])
);

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

CREATE TABLE [AspNetRoleClaims] (
    [Id] int NOT NULL IDENTITY,
    [RoleId] varchar(128) NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [AspNetUserClaims] (
    [Id] int NOT NULL IDENTITY,
    [UserId] varchar(128) NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [AspNetUserLogins] (
    [LoginProvider] nvarchar(450) NOT NULL,
    [ProviderKey] nvarchar(450) NOT NULL,
    [ProviderDisplayName] nvarchar(max) NULL,
    [UserId] varchar(128) NOT NULL,
    CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
    CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [AspNetUserRoles] (
    [UserId] varchar(128) NOT NULL,
    [RoleId] varchar(128) NOT NULL,
    CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
    CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [AspNetUserTokens] (
    [UserId] varchar(128) NOT NULL,
    [LoginProvider] nvarchar(450) NOT NULL,
    [Name] nvarchar(450) NOT NULL,
    [Value] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
    CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [SistemLoglar] (
    [Id] int NOT NULL IDENTITY,
    [LogID] nvarchar(max) NOT NULL,
    [LogTuru] nvarchar(50) NOT NULL,
    [Mesaj] nvarchar(2000) NOT NULL,
    [Sayfa] nvarchar(250) NOT NULL,
    [OlusturmaTarihi] datetime2 NOT NULL,
    [IslemTuru] nvarchar(50) NOT NULL,
    [LogTuruInt] int NULL,
    [Aciklama] nvarchar(max) NOT NULL,
    [HataMesaji] nvarchar(max) NOT NULL,
    [KullaniciAdi] nvarchar(max) NOT NULL,
    [IPAdresi] nvarchar(50) NOT NULL,
    [IslemTarihi] datetime2 NOT NULL,
    [Basarili] bit NOT NULL,
    [TabloAdi] nvarchar(250) NOT NULL,
    [KayitAdi] nvarchar(250) NULL,
    [KayitID] nvarchar(max) NULL,
    [KullaniciId] varchar(128) NULL,
    [KullaniciGuid] uniqueidentifier NULL,
    CONSTRAINT [PK_SistemLoglar] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_SistemLoglar_AspNetUsers_KullaniciId] FOREIGN KEY ([KullaniciId]) REFERENCES [AspNetUsers] ([Id])
);

CREATE TABLE [BankaHesaplari] (
    [BankaHesapID] uniqueidentifier NOT NULL,
    [BankaID] uniqueidentifier NOT NULL,
    [HesapAdi] nvarchar(100) NOT NULL,
    [HesapNo] nvarchar(50) NOT NULL,
    [IBAN] nvarchar(50) NOT NULL,
    [SubeAdi] nvarchar(100) NOT NULL,
    [SubeKodu] nvarchar(50) NOT NULL,
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
    [Silindi] bit NOT NULL DEFAULT CAST(0 AS bit),
    CONSTRAINT [PK_BankaHesaplari] PRIMARY KEY ([BankaHesapID]),
    CONSTRAINT [FK_BankaHesaplari_Bankalar_BankaID] FOREIGN KEY ([BankaID]) REFERENCES [Bankalar] ([BankaID])
);

CREATE TABLE [BirlesikModulKurDegerleri] (
    [KurDegeriID] uniqueidentifier NOT NULL,
    [ParaBirimiID] uniqueidentifier NOT NULL,
    [Alis] decimal(18,6) NOT NULL,
    [Satis] decimal(18,6) NOT NULL,
    [EfektifAlis] decimal(18,6) NOT NULL,
    [EfektifSatis] decimal(18,6) NOT NULL,
    [Tarih] datetime2 NOT NULL,
    [Aktif] bit NOT NULL,
    [Silindi] bit NOT NULL,
    [VeriKaynagi] nvarchar(50) NOT NULL,
    [OlusturmaTarihi] datetime2 NOT NULL,
    [GuncellemeTarihi] datetime2 NULL,
    [OlusturanKullaniciID] nvarchar(max) NOT NULL,
    [SonGuncelleyenKullaniciID] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_BirlesikModulKurDegerleri] PRIMARY KEY ([KurDegeriID]),
    CONSTRAINT [FK_BirlesikModulKurDegerleri_BirlesikModulParaBirimleri_ParaBirimiID] FOREIGN KEY ([ParaBirimiID]) REFERENCES [BirlesikModulParaBirimleri] ([ParaBirimiID]) ON DELETE CASCADE
);

CREATE TABLE [BirlesikModulParaBirimiIliskileri] (
    [ParaBirimiIliskiID] uniqueidentifier NOT NULL,
    [KaynakParaBirimiID] uniqueidentifier NOT NULL,
    [HedefParaBirimiID] uniqueidentifier NOT NULL,
    [Aktif] bit NOT NULL,
    [Silindi] bit NOT NULL,
    [Aciklama] nvarchar(500) NOT NULL,
    [OlusturmaTarihi] datetime2 NOT NULL,
    [GuncellemeTarihi] datetime2 NULL,
    [OlusturanKullaniciID] nvarchar(max) NOT NULL,
    [SonGuncelleyenKullaniciID] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_BirlesikModulParaBirimiIliskileri] PRIMARY KEY ([ParaBirimiIliskiID]),
    CONSTRAINT [CK_BirlesikModulDovizIliski_DifferentCurrencies] CHECK (KaynakParaBirimiID <> HedefParaBirimiID),
    CONSTRAINT [FK_BirlesikModulParaBirimiIliskileri_BirlesikModulParaBirimleri_HedefParaBirimiID] FOREIGN KEY ([HedefParaBirimiID]) REFERENCES [BirlesikModulParaBirimleri] ([ParaBirimiID]),
    CONSTRAINT [FK_BirlesikModulParaBirimiIliskileri_BirlesikModulParaBirimleri_KaynakParaBirimiID] FOREIGN KEY ([KaynakParaBirimiID]) REFERENCES [BirlesikModulParaBirimleri] ([ParaBirimiID])
);

CREATE TABLE [MenuRoller] (
    [MenuId] uniqueidentifier NOT NULL,
    [RolId] varchar(128) NOT NULL,
    [MenuRolID] uniqueidentifier NOT NULL,
    [OlusturmaTarihi] datetime2 NOT NULL,
    CONSTRAINT [PK_MenuRoller] PRIMARY KEY ([MenuId], [RolId]),
    CONSTRAINT [FK_MenuRoller_AspNetRoles_RolId] FOREIGN KEY ([RolId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_MenuRoller_Menuler_MenuId] FOREIGN KEY ([MenuId]) REFERENCES [Menuler] ([MenuID]) ON DELETE CASCADE
);

CREATE TABLE [Cariler] (
    [CariID] uniqueidentifier NOT NULL,
    [Ad] nvarchar(100) NOT NULL,
    [CariUnvani] nvarchar(100) NULL,
    [CariKodu] nvarchar(20) NOT NULL,
    [CariTipi] nvarchar(50) NOT NULL,
    [VergiNo] nvarchar(20) NULL,
    [VergiDairesi] nvarchar(50) NULL,
    [Telefon] nvarchar(15) NOT NULL,
    [Email] nvarchar(100) NOT NULL,
    [Yetkili] nvarchar(50) NOT NULL,
    [BaslangicBakiye] decimal(18,2) NOT NULL,
    [Adres] nvarchar(250) NOT NULL,
    [Aciklama] nvarchar(500) NULL,
    [Il] nvarchar(50) NOT NULL,
    [Ilce] nvarchar(50) NOT NULL,
    [PostaKodu] nvarchar(10) NOT NULL,
    [Ulke] nvarchar(50) NOT NULL,
    [WebSitesi] nvarchar(100) NULL,
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
    CONSTRAINT [FK_Cariler_ParaBirimiModuluParaBirimleri_VarsayilanParaBirimiId] FOREIGN KEY ([VarsayilanParaBirimiId]) REFERENCES [ParaBirimiModuluParaBirimleri] ([ParaBirimiID])
);

CREATE TABLE [ParaBirimiModuluKurDegerleri] (
    [KurDegeriID] uniqueidentifier NOT NULL,
    [ParaBirimiID] uniqueidentifier NOT NULL,
    [Tarih] datetime2 NOT NULL,
    [Alis] decimal(18,6) NOT NULL,
    [Satis] decimal(18,6) NOT NULL,
    [EfektifAlis] decimal(18,6) NOT NULL,
    [EfektifSatis] decimal(18,6) NOT NULL,
    [Aktif] bit NOT NULL,
    [Silindi] bit NOT NULL,
    [Aciklama] nvarchar(500) NULL,
    [OlusturmaTarihi] datetime2 NOT NULL,
    [GuncellemeTarihi] datetime2 NULL,
    [OlusturanKullaniciID] nvarchar(max) NULL,
    [SonGuncelleyenKullaniciID] nvarchar(max) NULL,
    [DekontNo] nvarchar(50) NULL,
    CONSTRAINT [PK_ParaBirimiModuluKurDegerleri] PRIMARY KEY ([KurDegeriID]),
    CONSTRAINT [FK_ParaBirimiModuluKurDegerleri_ParaBirimiModuluParaBirimleri_ParaBirimiID] FOREIGN KEY ([ParaBirimiID]) REFERENCES [ParaBirimiModuluParaBirimleri] ([ParaBirimiID])
);

CREATE TABLE [ParaBirimiModuluParaBirimiIliskileri] (
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
    CONSTRAINT [PK_ParaBirimiModuluParaBirimiIliskileri] PRIMARY KEY ([ParaBirimiIliskiID]),
    CONSTRAINT [CK_DovizIliski_DifferentCurrencies] CHECK (KaynakParaBirimiID <> HedefParaBirimiID),
    CONSTRAINT [FK_ParaBirimiModuluParaBirimiIliskileri_ParaBirimiModuluParaBirimleri_HedefParaBirimiID] FOREIGN KEY ([HedefParaBirimiID]) REFERENCES [ParaBirimiModuluParaBirimleri] ([ParaBirimiID]) ON DELETE NO ACTION,
    CONSTRAINT [FK_ParaBirimiModuluParaBirimiIliskileri_ParaBirimiModuluParaBirimleri_KaynakParaBirimiID] FOREIGN KEY ([KaynakParaBirimiID]) REFERENCES [ParaBirimiModuluParaBirimleri] ([ParaBirimiID]) ON DELETE NO ACTION
);

CREATE TABLE [Urunler] (
    [UrunID] uniqueidentifier NOT NULL,
    [UrunKodu] nvarchar(50) NOT NULL,
    [UrunAdi] nvarchar(200) NOT NULL,
    [Aciklama] nvarchar(500) NOT NULL,
    [BirimID] uniqueidentifier NULL,
    [Aktif] bit NOT NULL DEFAULT CAST(1 AS bit),
    [KDVOrani] int NOT NULL,
    [KritikStokSeviyesi] decimal(18,2) NOT NULL,
    [DovizliListeFiyati] decimal(18,2) NULL,
    [DovizliMaliyetFiyati] decimal(18,2) NULL,
    [DovizliSatisFiyati] decimal(18,2) NULL,
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
    [Silindi] bit NOT NULL DEFAULT CAST(0 AS bit),
    [KarsiUnvan] nvarchar(200) NOT NULL,
    [KarsiBankaAdi] nvarchar(50) NOT NULL,
    [KarsiIBAN] nvarchar(50) NOT NULL,
    CONSTRAINT [PK_BankaHareketleri] PRIMARY KEY ([BankaHareketID]),
    CONSTRAINT [FK_BankaHareketleri_Bankalar_BankaID] FOREIGN KEY ([BankaID]) REFERENCES [Bankalar] ([BankaID]) ON DELETE CASCADE,
    CONSTRAINT [FK_BankaHareketleri_Cariler_CariID] FOREIGN KEY ([CariID]) REFERENCES [Cariler] ([CariID]),
    CONSTRAINT [FK_BankaHareketleri_Kasalar_HedefKasaID] FOREIGN KEY ([HedefKasaID]) REFERENCES [Kasalar] ([KasaID]),
    CONSTRAINT [FK_BankaHareketleri_Kasalar_KaynakKasaID] FOREIGN KEY ([KaynakKasaID]) REFERENCES [Kasalar] ([KasaID])
);

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
    [ReferansNo] nvarchar(50) NOT NULL,
    [ReferansTuru] nvarchar(50) NOT NULL,
    [ReferansID] uniqueidentifier NULL,
    [Aciklama] nvarchar(500) NOT NULL,
    [DekontNo] nvarchar(50) NOT NULL,
    [IslemYapanKullaniciID] uniqueidentifier NULL,
    [SonGuncelleyenKullaniciID] uniqueidentifier NULL,
    [OlusturmaTarihi] datetime2 NOT NULL,
    [GuncellemeTarihi] datetime2 NULL,
    [Silindi] bit NOT NULL DEFAULT CAST(0 AS bit),
    [KarsiUnvan] nvarchar(200) NULL,
    [KarsiBankaAdi] nvarchar(50) NULL,
    [KarsiIBAN] nvarchar(50) NULL,
    [KarsiParaBirimi] nvarchar(10) NOT NULL,
    CONSTRAINT [PK_BankaHesapHareketleri] PRIMARY KEY ([BankaHesapHareketID]),
    CONSTRAINT [FK_BankaHesapHareketleri_BankaHesaplari_BankaHesapID] FOREIGN KEY ([BankaHesapID]) REFERENCES [BankaHesaplari] ([BankaHesapID]),
    CONSTRAINT [FK_BankaHesapHareketleri_Bankalar_BankaID] FOREIGN KEY ([BankaID]) REFERENCES [Bankalar] ([BankaID]) ON DELETE CASCADE,
    CONSTRAINT [FK_BankaHesapHareketleri_Cariler_CariID] FOREIGN KEY ([CariID]) REFERENCES [Cariler] ([CariID]),
    CONSTRAINT [FK_BankaHesapHareketleri_Kasalar_HedefKasaID] FOREIGN KEY ([HedefKasaID]) REFERENCES [Kasalar] ([KasaID]),
    CONSTRAINT [FK_BankaHesapHareketleri_Kasalar_KaynakKasaID] FOREIGN KEY ([KaynakKasaID]) REFERENCES [Kasalar] ([KasaID])
);

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
    CONSTRAINT [FK_CariHareketler_Cariler_CariId] FOREIGN KEY ([CariId]) REFERENCES [Cariler] ([CariID]) ON DELETE NO ACTION
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
    [Silindi] bit NOT NULL DEFAULT CAST(0 AS bit),
    [TransferID] uniqueidentifier NULL,
    CONSTRAINT [PK_KasaHareketleri] PRIMARY KEY ([KasaHareketID]),
    CONSTRAINT [FK_KasaHareketleri_Bankalar_HedefBankaID] FOREIGN KEY ([HedefBankaID]) REFERENCES [Bankalar] ([BankaID]),
    CONSTRAINT [FK_KasaHareketleri_Bankalar_KaynakBankaID] FOREIGN KEY ([KaynakBankaID]) REFERENCES [Bankalar] ([BankaID]),
    CONSTRAINT [FK_KasaHareketleri_Cariler_CariID] FOREIGN KEY ([CariID]) REFERENCES [Cariler] ([CariID]),
    CONSTRAINT [FK_KasaHareketleri_Kasalar_HedefKasaID] FOREIGN KEY ([HedefKasaID]) REFERENCES [Kasalar] ([KasaID]),
    CONSTRAINT [FK_KasaHareketleri_Kasalar_KasaID] FOREIGN KEY ([KasaID]) REFERENCES [Kasalar] ([KasaID])
);

CREATE TABLE [Sozlesmeler] (
    [SozlesmeID] uniqueidentifier NOT NULL,
    [SozlesmeNo] nvarchar(100) NOT NULL,
    [SozlesmeTarihi] datetime2 NOT NULL,
    [BitisTarihi] datetime2 NULL,
    [CariID] uniqueidentifier NULL,
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
    CONSTRAINT [FK_Sozlesmeler_Cariler_CariID] FOREIGN KEY ([CariID]) REFERENCES [Cariler] ([CariID]) ON DELETE SET NULL
);

CREATE TABLE [StokFifoKayitlari] (
    [StokFifoID] uniqueidentifier NOT NULL,
    [UrunID] uniqueidentifier NOT NULL,
    [Miktar] decimal(18,3) NOT NULL,
    [KalanMiktar] decimal(18,3) NOT NULL,
    [BirimFiyat] decimal(18,2) NOT NULL,
    [Birim] nvarchar(20) NOT NULL,
    [ParaBirimi] nvarchar(10) NOT NULL,
    [DovizKuru] decimal(18,6) NOT NULL,
    [USDBirimFiyat] decimal(18,2) NOT NULL,
    [TLBirimFiyat] decimal(18,2) NOT NULL,
    [UZSBirimFiyat] decimal(18,2) NOT NULL,
    [GirisTarihi] datetime2 NOT NULL,
    [SonCikisTarihi] datetime2 NULL,
    [ReferansNo] nvarchar(50) NOT NULL,
    [ReferansTuru] nvarchar(20) NOT NULL,
    [ReferansID] uniqueidentifier NOT NULL,
    [Aciklama] nvarchar(500) NULL,
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
    [ParaBirimi] nvarchar(10) NULL,
    CONSTRAINT [PK_StokHareketleri] PRIMARY KEY ([StokHareketID]),
    CONSTRAINT [FK_StokHareketleri_Depolar_DepoID] FOREIGN KEY ([DepoID]) REFERENCES [Depolar] ([DepoID]),
    CONSTRAINT [FK_StokHareketleri_Urunler_UrunID] FOREIGN KEY ([UrunID]) REFERENCES [Urunler] ([UrunID]) ON DELETE CASCADE
);

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
    [IndirimTutari] decimal(18,2) NULL,
    [GenelToplam] decimal(18,2) NULL,
    [OdenenTutar] decimal(18,2) NULL,
    [AraToplamDoviz] decimal(18,2) NULL,
    [KDVToplamDoviz] decimal(18,2) NULL,
    [IndirimTutariDoviz] decimal(18,2) NULL,
    [GenelToplamDoviz] decimal(18,2) NULL,
    [OdemeDurumu] nvarchar(50) NOT NULL,
    [FaturaNotu] nvarchar(500) NOT NULL,
    [ResmiMi] bit NOT NULL DEFAULT CAST(1 AS bit),
    [SozlesmeID] uniqueidentifier NULL,
    [DovizTuru] nvarchar(10) NOT NULL,
    [ParaBirimi] nvarchar(10) NOT NULL,
    [DovizKuru] decimal(18,4) NULL,
    [OlusturmaTarihi] datetime2 NULL,
    [GuncellemeTarihi] datetime2 NULL,
    [Silindi] bit NOT NULL DEFAULT CAST(0 AS bit),
    [Aktif] bit NOT NULL DEFAULT CAST(1 AS bit),
    [OlusturanKullaniciID] uniqueidentifier NULL,
    [SonGuncelleyenKullaniciID] uniqueidentifier NULL,
    [OdemeTuruID] int NULL,
    [AklanmaTarihi] datetime2 NULL,
    [AklanmaNotu] nvarchar(500) NOT NULL,
    CONSTRAINT [PK_Faturalar] PRIMARY KEY ([FaturaID]),
    CONSTRAINT [FK_Faturalar_Cariler_CariID] FOREIGN KEY ([CariID]) REFERENCES [Cariler] ([CariID]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Faturalar_FaturaTurleri_FaturaTuruID] FOREIGN KEY ([FaturaTuruID]) REFERENCES [FaturaTurleri] ([FaturaTuruID]),
    CONSTRAINT [FK_Faturalar_OdemeTurleri_OdemeTuruID] FOREIGN KEY ([OdemeTuruID]) REFERENCES [OdemeTurleri] ([OdemeTuruID]),
    CONSTRAINT [FK_Faturalar_Sozlesmeler_SozlesmeID] FOREIGN KEY ([SozlesmeID]) REFERENCES [Sozlesmeler] ([SozlesmeID]) ON DELETE SET NULL
);

CREATE TABLE [StokCikisDetaylari] (
    [StokCikisDetayID] uniqueidentifier NOT NULL,
    [StokFifoID] uniqueidentifier NULL,
    [CikisMiktari] decimal(18,6) NOT NULL,
    [BirimFiyat] decimal(18,6) NOT NULL,
    [BirimFiyatUSD] decimal(18,6) NOT NULL,
    [BirimFiyatTL] decimal(18,6) NOT NULL,
    [BirimFiyatUZS] decimal(18,6) NOT NULL,
    [ToplamMaliyetUSD] decimal(18,6) NOT NULL,
    [HareketTipi] nvarchar(50) NOT NULL,
    [BirimMaliyet] decimal(18,6) NOT NULL,
    [ToplamMaliyet] decimal(18,6) NOT NULL,
    [ParaBirimi] nvarchar(10) NOT NULL,
    [ReferansNo] nvarchar(50) NOT NULL,
    [ReferansTuru] nvarchar(50) NOT NULL,
    [ReferansID] uniqueidentifier NOT NULL,
    [CikisTarihi] datetime2 NOT NULL,
    [Aciklama] nvarchar(500) NOT NULL,
    [OlusturmaTarihi] datetime2 NOT NULL,
    [Iptal] bit NOT NULL,
    [IptalTarihi] datetime2 NULL,
    [IptalAciklama] nvarchar(500) NOT NULL,
    CONSTRAINT [PK_StokCikisDetaylari] PRIMARY KEY ([StokCikisDetayID]),
    CONSTRAINT [FK_StokCikisDetaylari_StokFifoKayitlari_StokFifoID] FOREIGN KEY ([StokFifoID]) REFERENCES [StokFifoKayitlari] ([StokFifoID]) ON DELETE SET NULL
);

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
    [BirimFiyatDoviz] decimal(18,2) NOT NULL,
    [TutarDoviz] decimal(18,2) NULL,
    [KdvTutariDoviz] decimal(18,2) NULL,
    [IndirimTutariDoviz] decimal(18,2) NULL,
    [NetTutarDoviz] decimal(18,2) NULL,
    [AklananMiktar] decimal(18,2) NULL,
    [AklanmaTamamlandi] bit NOT NULL,
    [BirimID] uniqueidentifier NULL,
    CONSTRAINT [PK_FaturaDetaylari] PRIMARY KEY ([FaturaDetayID]),
    CONSTRAINT [FK_FaturaDetaylari_Birimler_BirimID] FOREIGN KEY ([BirimID]) REFERENCES [Birimler] ([BirimID]),
    CONSTRAINT [FK_FaturaDetaylari_Faturalar_FaturaID] FOREIGN KEY ([FaturaID]) REFERENCES [Faturalar] ([FaturaID]) ON DELETE CASCADE,
    CONSTRAINT [FK_FaturaDetaylari_Urunler_UrunID] FOREIGN KEY ([UrunID]) REFERENCES [Urunler] ([UrunID]) ON DELETE CASCADE
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
    [Silindi] bit NOT NULL,
    [Aktif] bit NOT NULL,
    CONSTRAINT [PK_FaturaOdemeleri] PRIMARY KEY ([OdemeID]),
    CONSTRAINT [FK_FaturaOdemeleri_Faturalar_FaturaID] FOREIGN KEY ([FaturaID]) REFERENCES [Faturalar] ([FaturaID]) ON DELETE CASCADE
);

CREATE TABLE [Irsaliyeler] (
    [IrsaliyeID] uniqueidentifier NOT NULL,
    [IrsaliyeNumarasi] nvarchar(20) NOT NULL,
    [IrsaliyeTarihi] datetime2 NOT NULL,
    [CariID] uniqueidentifier NOT NULL,
    [FaturaID] uniqueidentifier NULL,
    [DepoID] uniqueidentifier NULL,
    [Aciklama] nvarchar(500) NOT NULL,
    [IrsaliyeTuru] nvarchar(max) NOT NULL,
    [Aktif] bit NOT NULL DEFAULT CAST(1 AS bit),
    [OlusturanKullaniciId] uniqueidentifier NOT NULL,
    [SonGuncelleyenKullaniciId] uniqueidentifier NULL,
    [Silindi] bit NOT NULL,
    [Durum] nvarchar(20) NOT NULL,
    [IrsaliyeTuruID] int NULL,
    [OlusturmaTarihi] datetime2 NOT NULL,
    [GuncellemeTarihi] datetime2 NULL,
    CONSTRAINT [PK_Irsaliyeler] PRIMARY KEY ([IrsaliyeID]),
    CONSTRAINT [FK_Irsaliyeler_Cariler_CariID] FOREIGN KEY ([CariID]) REFERENCES [Cariler] ([CariID]) ON DELETE CASCADE,
    CONSTRAINT [FK_Irsaliyeler_Depolar_DepoID] FOREIGN KEY ([DepoID]) REFERENCES [Depolar] ([DepoID]),
    CONSTRAINT [FK_Irsaliyeler_Faturalar_FaturaID] FOREIGN KEY ([FaturaID]) REFERENCES [Faturalar] ([FaturaID]),
    CONSTRAINT [FK_Irsaliyeler_IrsaliyeTurleri_IrsaliyeTuruID] FOREIGN KEY ([IrsaliyeTuruID]) REFERENCES [IrsaliyeTurleri] ([IrsaliyeTuruID])
);

CREATE TABLE [FaturaAklamaKuyrugu] (
    [AklamaID] uniqueidentifier NOT NULL,
    [FaturaKalemID] uniqueidentifier NOT NULL,
    [UrunID] uniqueidentifier NOT NULL,
    [AklananMiktar] decimal(18,3) NOT NULL,
    [OlusturmaTarihi] datetime2 NOT NULL,
    [AklanmaTarihi] datetime2 NULL,
    [AklanmaNotu] nvarchar(500) NOT NULL,
    [SozlesmeID] uniqueidentifier NULL,
    [Durum] int NOT NULL DEFAULT 0,
    [BirimFiyat] decimal(18,2) NOT NULL,
    [ParaBirimi] nvarchar(10) NOT NULL DEFAULT N'TL',
    [DovizKuru] decimal(18,4) NOT NULL DEFAULT 1.0,
    [OlusturanKullaniciID] uniqueidentifier NULL,
    [GuncelleyenKullaniciID] uniqueidentifier NULL,
    [GuncellemeTarihi] datetime2 NULL,
    [Silindi] bit NOT NULL DEFAULT CAST(0 AS bit),
    [SilmeTarihi] datetime2 NULL,
    [ManuelKayit] bit NOT NULL,
    [ResmiFaturaKalemID] uniqueidentifier NULL,
    [FaturaID] uniqueidentifier NULL,
    CONSTRAINT [PK_FaturaAklamaKuyrugu] PRIMARY KEY ([AklamaID]),
    CONSTRAINT [FK_FaturaAklamaKuyrugu_FaturaDetaylari_FaturaKalemID] FOREIGN KEY ([FaturaKalemID]) REFERENCES [FaturaDetaylari] ([FaturaDetayID]),
    CONSTRAINT [FK_FaturaAklamaKuyrugu_Faturalar_FaturaID] FOREIGN KEY ([FaturaID]) REFERENCES [Faturalar] ([FaturaID]),
    CONSTRAINT [FK_FaturaAklamaKuyrugu_Sozlesmeler_SozlesmeID] FOREIGN KEY ([SozlesmeID]) REFERENCES [Sozlesmeler] ([SozlesmeID]),
    CONSTRAINT [FK_FaturaAklamaKuyrugu_Urunler_UrunID] FOREIGN KEY ([UrunID]) REFERENCES [Urunler] ([UrunID])
);

CREATE TABLE [IrsaliyeDetaylari] (
    [IrsaliyeDetayID] uniqueidentifier NOT NULL,
    [IrsaliyeID] uniqueidentifier NOT NULL,
    [UrunID] uniqueidentifier NOT NULL,
    [DepoID] uniqueidentifier NULL,
    [Miktar] decimal(18,3) NOT NULL,
    [BirimFiyat] decimal(18,2) NOT NULL,
    [KdvOrani] decimal(18,2) NOT NULL,
    [IndirimOrani] decimal(18,2) NOT NULL,
    [Birim] nvarchar(50) NOT NULL,
    [Aciklama] nvarchar(200) NOT NULL,
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
    CONSTRAINT [FK_IrsaliyeDetaylari_Depolar_DepoID] FOREIGN KEY ([DepoID]) REFERENCES [Depolar] ([DepoID]),
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

CREATE INDEX [IX_BankaHesapHareketleri_BankaHesapID] ON [BankaHesapHareketleri] ([BankaHesapID]);

CREATE INDEX [IX_BankaHesapHareketleri_BankaID] ON [BankaHesapHareketleri] ([BankaID]);

CREATE INDEX [IX_BankaHesapHareketleri_CariID] ON [BankaHesapHareketleri] ([CariID]);

CREATE INDEX [IX_BankaHesapHareketleri_HedefKasaID] ON [BankaHesapHareketleri] ([HedefKasaID]);

CREATE INDEX [IX_BankaHesapHareketleri_KaynakKasaID] ON [BankaHesapHareketleri] ([KaynakKasaID]);

CREATE INDEX [IX_BankaHesaplari_BankaID] ON [BankaHesaplari] ([BankaID]);

CREATE INDEX [IX_BirlesikModulKurDegerleri_ParaBirimiID] ON [BirlesikModulKurDegerleri] ([ParaBirimiID]);

CREATE INDEX [IX_BirlesikModulParaBirimiIliskileri_HedefParaBirimiID] ON [BirlesikModulParaBirimiIliskileri] ([HedefParaBirimiID]);

CREATE INDEX [IX_BirlesikModulParaBirimiIliskileri_KaynakParaBirimiID] ON [BirlesikModulParaBirimiIliskileri] ([KaynakParaBirimiID]);

CREATE INDEX [IX_CariHareketler_CariId] ON [CariHareketler] ([CariId]);

CREATE INDEX [IX_Cariler_VarsayilanParaBirimiId] ON [Cariler] ([VarsayilanParaBirimiId]);

CREATE INDEX [IX_FaturaAklamaKuyrugu_FaturaID] ON [FaturaAklamaKuyrugu] ([FaturaID]);

CREATE INDEX [IX_FaturaAklamaKuyrugu_FaturaKalemID] ON [FaturaAklamaKuyrugu] ([FaturaKalemID]);

CREATE INDEX [IX_FaturaAklamaKuyrugu_SozlesmeID] ON [FaturaAklamaKuyrugu] ([SozlesmeID]);

CREATE INDEX [IX_FaturaAklamaKuyrugu_UrunID] ON [FaturaAklamaKuyrugu] ([UrunID]);

CREATE INDEX [IX_FaturaDetaylari_BirimID] ON [FaturaDetaylari] ([BirimID]);

CREATE INDEX [IX_FaturaDetaylari_FaturaID] ON [FaturaDetaylari] ([FaturaID]);

CREATE INDEX [IX_FaturaDetaylari_UrunID] ON [FaturaDetaylari] ([UrunID]);

CREATE INDEX [IX_Faturalar_CariID] ON [Faturalar] ([CariID]);

CREATE INDEX [IX_Faturalar_FaturaTuruID] ON [Faturalar] ([FaturaTuruID]);

CREATE INDEX [IX_Faturalar_OdemeTuruID] ON [Faturalar] ([OdemeTuruID]);

CREATE INDEX [IX_Faturalar_SozlesmeID] ON [Faturalar] ([SozlesmeID]);

CREATE INDEX [IX_FaturaOdemeleri_FaturaID] ON [FaturaOdemeleri] ([FaturaID]);

CREATE INDEX [IX_IrsaliyeDetaylari_BirimID] ON [IrsaliyeDetaylari] ([BirimID]);

CREATE INDEX [IX_IrsaliyeDetaylari_DepoID] ON [IrsaliyeDetaylari] ([DepoID]);

CREATE INDEX [IX_IrsaliyeDetaylari_IrsaliyeID] ON [IrsaliyeDetaylari] ([IrsaliyeID]);

CREATE INDEX [IX_IrsaliyeDetaylari_UrunID] ON [IrsaliyeDetaylari] ([UrunID]);

CREATE INDEX [IX_Irsaliyeler_CariID] ON [Irsaliyeler] ([CariID]);

CREATE INDEX [IX_Irsaliyeler_DepoID] ON [Irsaliyeler] ([DepoID]);

CREATE INDEX [IX_Irsaliyeler_FaturaID] ON [Irsaliyeler] ([FaturaID]);

CREATE INDEX [IX_Irsaliyeler_IrsaliyeTuruID] ON [Irsaliyeler] ([IrsaliyeTuruID]);

CREATE INDEX [IX_KasaHareketleri_CariID] ON [KasaHareketleri] ([CariID]);

CREATE INDEX [IX_KasaHareketleri_HedefBankaID] ON [KasaHareketleri] ([HedefBankaID]);

CREATE INDEX [IX_KasaHareketleri_HedefKasaID] ON [KasaHareketleri] ([HedefKasaID]);

CREATE INDEX [IX_KasaHareketleri_KasaID] ON [KasaHareketleri] ([KasaID]);

CREATE INDEX [IX_KasaHareketleri_KaynakBankaID] ON [KasaHareketleri] ([KaynakBankaID]);

CREATE INDEX [IX_Menuler_UstMenuID] ON [Menuler] ([UstMenuID]);

CREATE INDEX [IX_MenuRoller_RolId] ON [MenuRoller] ([RolId]);

CREATE INDEX [IX_ParaBirimiModuluKurDegerleri_ParaBirimiID] ON [ParaBirimiModuluKurDegerleri] ([ParaBirimiID]);

CREATE UNIQUE INDEX [IX_DovizIliski_KaynakParaBirimiID_HedefParaBirimiID] ON [ParaBirimiModuluParaBirimiIliskileri] ([KaynakParaBirimiID], [HedefParaBirimiID]);

CREATE INDEX [IX_ParaBirimiModuluParaBirimiIliskileri_HedefParaBirimiID] ON [ParaBirimiModuluParaBirimiIliskileri] ([HedefParaBirimiID]);

CREATE UNIQUE INDEX [IX_ParaBirimi_Kod] ON [ParaBirimiModuluParaBirimleri] ([Kod]);

CREATE INDEX [IX_SistemLoglar_KullaniciId] ON [SistemLoglar] ([KullaniciId]);

CREATE INDEX [IX_Sozlesmeler_CariID] ON [Sozlesmeler] ([CariID]);

CREATE INDEX [IX_StokCikisDetaylari_StokFifoID] ON [StokCikisDetaylari] ([StokFifoID]);

CREATE INDEX [IX_StokFifo_GirisTarihi] ON [StokFifoKayitlari] ([GirisTarihi]);

CREATE INDEX [IX_StokFifo_Referans] ON [StokFifoKayitlari] ([ReferansID], [ReferansTuru]);

CREATE INDEX [IX_StokFifo_StokSorgu] ON [StokFifoKayitlari] ([UrunID], [KalanMiktar], [Aktif], [Silindi], [Iptal]);

CREATE INDEX [IX_StokFifo_UrunID] ON [StokFifoKayitlari] ([UrunID]);

CREATE INDEX [IX_StokHareketleri_DepoID] ON [StokHareketleri] ([DepoID]);

CREATE INDEX [IX_StokHareketleri_UrunID] ON [StokHareketleri] ([UrunID]);

CREATE INDEX [IX_UrunFiyatlari_FiyatTipiID] ON [UrunFiyatlari] ([FiyatTipiID]);

CREATE INDEX [IX_UrunFiyatlari_UrunID] ON [UrunFiyatlari] ([UrunID]);

CREATE INDEX [IX_Urunler_BirimID] ON [Urunler] ([BirimID]);

CREATE INDEX [IX_Urunler_KategoriID] ON [Urunler] ([KategoriID]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250503130549_InitialCreate', N'9.0.4');

DECLARE @var sysname;
SELECT @var = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Cariler]') AND [c].[name] = N'BaslangicBakiye');
IF @var IS NOT NULL EXEC(N'ALTER TABLE [Cariler] DROP CONSTRAINT [' + @var + '];');
ALTER TABLE [Cariler] DROP COLUMN [BaslangicBakiye];

DECLARE @var1 sysname;
SELECT @var1 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Cariler]') AND [c].[name] = N'CariUnvani');
IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [Cariler] DROP CONSTRAINT [' + @var1 + '];');
ALTER TABLE [Cariler] DROP COLUMN [CariUnvani];

DECLARE @var2 sysname;
SELECT @var2 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Cariler]') AND [c].[name] = N'Notlar');
IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [Cariler] DROP CONSTRAINT [' + @var2 + '];');
ALTER TABLE [Cariler] DROP COLUMN [Notlar];

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250504191106_RemoveBaslangicBakiyeAndCariUnvani', N'9.0.4');

DECLARE @var3 sysname;
SELECT @var3 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Faturalar]') AND [c].[name] = N'FaturaNotu');
IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [Faturalar] DROP CONSTRAINT [' + @var3 + '];');
ALTER TABLE [Faturalar] ALTER COLUMN [FaturaNotu] nvarchar(500) NULL;

DECLARE @var4 sysname;
SELECT @var4 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Faturalar]') AND [c].[name] = N'DovizTuru');
IF @var4 IS NOT NULL EXEC(N'ALTER TABLE [Faturalar] DROP CONSTRAINT [' + @var4 + '];');
ALTER TABLE [Faturalar] ALTER COLUMN [DovizTuru] nvarchar(10) NULL;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250505145845_NullableFaturaNotu', N'9.0.4');

DECLARE @var5 sysname;
SELECT @var5 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[StokCikisDetaylari]') AND [c].[name] = N'ReferansTuru');
IF @var5 IS NOT NULL EXEC(N'ALTER TABLE [StokCikisDetaylari] DROP CONSTRAINT [' + @var5 + '];');
ALTER TABLE [StokCikisDetaylari] ALTER COLUMN [ReferansTuru] nvarchar(50) NULL;

DECLARE @var6 sysname;
SELECT @var6 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[StokCikisDetaylari]') AND [c].[name] = N'ReferansNo');
IF @var6 IS NOT NULL EXEC(N'ALTER TABLE [StokCikisDetaylari] DROP CONSTRAINT [' + @var6 + '];');
ALTER TABLE [StokCikisDetaylari] ALTER COLUMN [ReferansNo] nvarchar(50) NULL;

DECLARE @var7 sysname;
SELECT @var7 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[StokCikisDetaylari]') AND [c].[name] = N'IptalAciklama');
IF @var7 IS NOT NULL EXEC(N'ALTER TABLE [StokCikisDetaylari] DROP CONSTRAINT [' + @var7 + '];');
ALTER TABLE [StokCikisDetaylari] ALTER COLUMN [IptalAciklama] nvarchar(500) NULL;

DECLARE @var8 sysname;
SELECT @var8 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[StokCikisDetaylari]') AND [c].[name] = N'Aciklama');
IF @var8 IS NOT NULL EXEC(N'ALTER TABLE [StokCikisDetaylari] DROP CONSTRAINT [' + @var8 + '];');
ALTER TABLE [StokCikisDetaylari] ALTER COLUMN [Aciklama] nvarchar(500) NULL;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250506071239_StokCikisDetayNullableFields', N'9.0.4');

DECLARE @var9 sysname;
SELECT @var9 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[StokFifoKayitlari]') AND [c].[name] = N'Birim');
IF @var9 IS NOT NULL EXEC(N'ALTER TABLE [StokFifoKayitlari] DROP CONSTRAINT [' + @var9 + '];');
ALTER TABLE [StokFifoKayitlari] ALTER COLUMN [Birim] nvarchar(50) NOT NULL;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250506075404_UpdateStokFifoFields', N'9.0.4');

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250506085742_FixTransactionAndAddMissingFields', N'9.0.4');

ALTER TABLE [StokHareketleri] ADD [BirimFiyatUSD] decimal(18,6) NULL;

ALTER TABLE [StokHareketleri] ADD [BirimFiyatUZS] decimal(18,6) NULL;

ALTER TABLE [CariHareketler] ADD [ParaBirimi] nvarchar(10) NOT NULL DEFAULT N'';

ALTER TABLE [CariHareketler] ADD [TutarDoviz] decimal(18,2) NOT NULL DEFAULT 0.0;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250506095544_FixCariHareketAndStokHareket', N'9.0.4');

ALTER TABLE [CariHareketler] ADD [AlacakDoviz] decimal(18,2) NOT NULL DEFAULT 0.0;

ALTER TABLE [CariHareketler] ADD [BorcDoviz] decimal(18,2) NOT NULL DEFAULT 0.0;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250506105508_AddDovizFields', N'9.0.4');

DECLARE @var10 sysname;
SELECT @var10 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[StokFifoKayitlari]') AND [c].[name] = N'IptalAciklama');
IF @var10 IS NOT NULL EXEC(N'ALTER TABLE [StokFifoKayitlari] DROP CONSTRAINT [' + @var10 + '];');
ALTER TABLE [StokFifoKayitlari] DROP COLUMN [IptalAciklama];

DECLARE @var11 sysname;
SELECT @var11 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[StokFifoKayitlari]') AND [c].[name] = N'IptalEdenKullaniciID');
IF @var11 IS NOT NULL EXEC(N'ALTER TABLE [StokFifoKayitlari] DROP CONSTRAINT [' + @var11 + '];');
ALTER TABLE [StokFifoKayitlari] DROP COLUMN [IptalEdenKullaniciID];

DECLARE @var12 sysname;
SELECT @var12 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[StokFifoKayitlari]') AND [c].[name] = N'IptalTarihi');
IF @var12 IS NOT NULL EXEC(N'ALTER TABLE [StokFifoKayitlari] DROP CONSTRAINT [' + @var12 + '];');
ALTER TABLE [StokFifoKayitlari] DROP COLUMN [IptalTarihi];

DECLARE @var13 sysname;
SELECT @var13 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[StokFifoKayitlari]') AND [c].[name] = N'RowVersion');
IF @var13 IS NOT NULL EXEC(N'ALTER TABLE [StokFifoKayitlari] DROP CONSTRAINT [' + @var13 + '];');
ALTER TABLE [StokFifoKayitlari] DROP COLUMN [RowVersion];

DECLARE @var14 sysname;
SELECT @var14 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[StokFifoKayitlari]') AND [c].[name] = N'TLBirimFiyat');
IF @var14 IS NOT NULL EXEC(N'ALTER TABLE [StokFifoKayitlari] DROP CONSTRAINT [' + @var14 + '];');
ALTER TABLE [StokFifoKayitlari] DROP COLUMN [TLBirimFiyat];

EXEC sp_rename N'[StokFifoKayitlari].[UZSBirimFiyat]', N'BirimFiyatUZS', 'COLUMN';

EXEC sp_rename N'[StokFifoKayitlari].[USDBirimFiyat]', N'BirimFiyatUSD', 'COLUMN';

DECLARE @var15 sysname;
SELECT @var15 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[StokFifoKayitlari]') AND [c].[name] = N'ReferansID');
IF @var15 IS NOT NULL EXEC(N'ALTER TABLE [StokFifoKayitlari] DROP CONSTRAINT [' + @var15 + '];');
ALTER TABLE [StokFifoKayitlari] ALTER COLUMN [ReferansID] uniqueidentifier NULL;

DECLARE @var16 sysname;
SELECT @var16 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[StokFifoKayitlari]') AND [c].[name] = N'Aciklama');
IF @var16 IS NOT NULL EXEC(N'ALTER TABLE [StokFifoKayitlari] DROP CONSTRAINT [' + @var16 + '];');
UPDATE [StokFifoKayitlari] SET [Aciklama] = N'' WHERE [Aciklama] IS NULL;
ALTER TABLE [StokFifoKayitlari] ALTER COLUMN [Aciklama] nvarchar(500) NOT NULL;
ALTER TABLE [StokFifoKayitlari] ADD DEFAULT N'' FOR [Aciklama];

ALTER TABLE [StokCikisDetaylari] ADD [StokFifoID1] uniqueidentifier NULL;

CREATE TABLE [StokFifoCikislar] (
    [StokFifoCikisID] uniqueidentifier NOT NULL,
    [StokFifoID] uniqueidentifier NOT NULL,
    [ReferansID] uniqueidentifier NULL,
    [DetayID] uniqueidentifier NULL,
    [ReferansNo] nvarchar(max) NULL,
    [ReferansTuru] nvarchar(max) NULL,
    [CikisMiktar] decimal(18,2) NOT NULL,
    [CikisTarihi] datetime2 NOT NULL,
    [USDBirimFiyat] decimal(18,2) NOT NULL,
    [UZSBirimFiyat] decimal(18,2) NOT NULL,
    [ParaBirimi] nvarchar(max) NULL,
    [DovizKuru] decimal(18,2) NULL,
    [OlusturmaTarihi] datetime2 NOT NULL,
    [OlusturanKullaniciId] uniqueidentifier NULL,
    [Aktif] bit NOT NULL,
    CONSTRAINT [PK_StokFifoCikislar] PRIMARY KEY ([StokFifoCikisID]),
    CONSTRAINT [FK_StokFifoCikislar_StokFifoKayitlari_StokFifoID] FOREIGN KEY ([StokFifoID]) REFERENCES [StokFifoKayitlari] ([StokFifoID]) ON DELETE CASCADE
);

CREATE INDEX [IX_StokCikisDetaylari_StokFifoID1] ON [StokCikisDetaylari] ([StokFifoID1]);

CREATE INDEX [IX_StokFifoCikislar_StokFifoID] ON [StokFifoCikislar] ([StokFifoID]);

ALTER TABLE [StokCikisDetaylari] ADD CONSTRAINT [FK_StokCikisDetaylari_StokFifoKayitlari_StokFifoID1] FOREIGN KEY ([StokFifoID1]) REFERENCES [StokFifoKayitlari] ([StokFifoID]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250507082840_StokFifoPropertyUpdates', N'9.0.4');

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250507090849_FaturaSilmeIslemDuzeltme', N'9.0.4');

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250507091422_IrsaliyeControllerFix', N'9.0.4');

ALTER TABLE [StokCikisDetaylari] DROP CONSTRAINT [FK_StokCikisDetaylari_StokFifoKayitlari_StokFifoID1];

ALTER TABLE [StokFifoCikislar] DROP CONSTRAINT [FK_StokFifoCikislar_StokFifoKayitlari_StokFifoID];

DROP INDEX [IX_StokCikisDetaylari_StokFifoID1] ON [StokCikisDetaylari];

DECLARE @var17 sysname;
SELECT @var17 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[StokCikisDetaylari]') AND [c].[name] = N'StokFifoID1');
IF @var17 IS NOT NULL EXEC(N'ALTER TABLE [StokCikisDetaylari] DROP CONSTRAINT [' + @var17 + '];');
ALTER TABLE [StokCikisDetaylari] DROP COLUMN [StokFifoID1];

EXEC sp_rename N'[StokFifoCikislar].[UZSBirimFiyat]', N'BirimFiyatUZS', 'COLUMN';

EXEC sp_rename N'[StokFifoCikislar].[USDBirimFiyat]', N'BirimFiyatUSD', 'COLUMN';

DECLARE @var18 sysname;
SELECT @var18 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[StokFifoCikislar]') AND [c].[name] = N'StokFifoID');
IF @var18 IS NOT NULL EXEC(N'ALTER TABLE [StokFifoCikislar] DROP CONSTRAINT [' + @var18 + '];');
ALTER TABLE [StokFifoCikislar] ALTER COLUMN [StokFifoID] uniqueidentifier NULL;

DECLARE @var19 sysname;
SELECT @var19 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[StokFifoCikislar]') AND [c].[name] = N'DovizKuru');
IF @var19 IS NOT NULL EXEC(N'ALTER TABLE [StokFifoCikislar] DROP CONSTRAINT [' + @var19 + '];');
ALTER TABLE [StokFifoCikislar] ALTER COLUMN [DovizKuru] decimal(18,6) NULL;

ALTER TABLE [StokFifoCikislar] ADD CONSTRAINT [FK_StokFifoCikislar_StokFifoKayitlari_StokFifoID] FOREIGN KEY ([StokFifoID]) REFERENCES [StokFifoKayitlari] ([StokFifoID]) ON DELETE SET NULL;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250507114838_ResetDatabase', N'9.0.4');

ALTER TABLE [AspNetUsers] ADD [Bio] nvarchar(500) NULL;

ALTER TABLE [AspNetUsers] ADD [Department] nvarchar(100) NULL;

ALTER TABLE [AspNetUsers] ADD [FullName] nvarchar(100) NULL;

ALTER TABLE [AspNetUsers] ADD [JobTitle] nvarchar(100) NULL;

ALTER TABLE [AspNetUsers] ADD [ProfileImage] nvarchar(255) NULL;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250507155057_ProfilePageProperties', N'9.0.4');

DECLARE @var20 sysname;
SELECT @var20 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[AspNetUsers]') AND [c].[name] = N'Department');
IF @var20 IS NOT NULL EXEC(N'ALTER TABLE [AspNetUsers] DROP CONSTRAINT [' + @var20 + '];');
ALTER TABLE [AspNetUsers] DROP COLUMN [Department];

DECLARE @var21 sysname;
SELECT @var21 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[AspNetUsers]') AND [c].[name] = N'JobTitle');
IF @var21 IS NOT NULL EXEC(N'ALTER TABLE [AspNetUsers] DROP CONSTRAINT [' + @var21 + '];');
ALTER TABLE [AspNetUsers] DROP COLUMN [JobTitle];

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250507161007_RemoveJobTitleAndDepartment', N'9.0.4');

DECLARE @var22 sysname;
SELECT @var22 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[AspNetUsers]') AND [c].[name] = N'ProfileImage');
IF @var22 IS NOT NULL EXEC(N'ALTER TABLE [AspNetUsers] DROP CONSTRAINT [' + @var22 + '];');
ALTER TABLE [AspNetUsers] ALTER COLUMN [ProfileImage] nvarchar(200) NULL;

CREATE TABLE [TodoItems] (
    [Id] int NOT NULL IDENTITY,
    [Title] nvarchar(200) NOT NULL,
    [Description] nvarchar(1000) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [Deadline] datetime2 NULL,
    [IsCompleted] bit NOT NULL,
    [AssignedToUserId] varchar(128) NULL,
    CONSTRAINT [PK_TodoItems] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_TodoItems_AspNetUsers_AssignedToUserId] FOREIGN KEY ([AssignedToUserId]) REFERENCES [AspNetUsers] ([Id])
);

CREATE INDEX [IX_TodoItems_AssignedToUserId] ON [TodoItems] ([AssignedToUserId]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250507190346_AddTodoItemsTable', N'9.0.4');

ALTER TABLE [TodoItems] ADD [TaskCategory] nvarchar(50) NULL;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250507191307_AddTaskCategoryToTodoItem', N'9.0.4');

ALTER TABLE [TodoItems] ADD [PriorityLevel] int NOT NULL DEFAULT 0;

CREATE TABLE [TodoComments] (
    [Id] int NOT NULL IDENTITY,
    [TodoItemId] int NOT NULL,
    [AppUserId] varchar(128) NOT NULL,
    [Content] nvarchar(1000) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_TodoComments] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_TodoComments_AspNetUsers_AppUserId] FOREIGN KEY ([AppUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_TodoComments_TodoItems_TodoItemId] FOREIGN KEY ([TodoItemId]) REFERENCES [TodoItems] ([Id]) ON DELETE CASCADE
);

CREATE INDEX [IX_TodoComments_AppUserId] ON [TodoComments] ([AppUserId]);

CREATE INDEX [IX_TodoComments_TodoItemId] ON [TodoComments] ([TodoItemId]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250507195854_AddTodoCommentsAndPriority', N'9.0.4');

ALTER TABLE [TodoItems] ADD [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250508082924_AddIsDeletedToTodoItem', N'9.0.4');

ALTER TABLE [TodoItems] ADD [IsReminderSent] bit NOT NULL DEFAULT CAST(0 AS bit);

ALTER TABLE [TodoItems] ADD [ReminderAt] datetime2 NULL;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250508090844_AddTodoItemReminder', N'9.0.4');

ALTER TABLE [TodoItems] ADD [IsArchived] bit NOT NULL DEFAULT CAST(0 AS bit);

ALTER TABLE [TodoItems] ADD [Status] int NOT NULL DEFAULT 0;

ALTER TABLE [TodoItems] ADD [Tags] nvarchar(200) NULL;

CREATE TABLE [Notifications] (
    [Id] int NOT NULL IDENTITY,
    [UserId] varchar(128) NOT NULL,
    [Content] nvarchar(500) NOT NULL,
    [Type] nvarchar(50) NOT NULL,
    [RelatedEntityId] int NULL,
    [CreatedAt] datetime2 NOT NULL,
    [IsRead] bit NOT NULL,
    CONSTRAINT [PK_Notifications] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Notifications_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);

CREATE INDEX [IX_Notifications_UserId] ON [Notifications] ([UserId]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250508163139_AddTodoItemNewFields', N'9.0.4');

ALTER TABLE [StokCikisDetaylari] ADD [DovizKuru] decimal(18,6) NULL;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250510102755_StokCikisDetayDovizKuru', N'9.0.4');

DECLARE @var23 sysname;
SELECT @var23 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[FaturaTurleri]') AND [c].[name] = N'Silindi');
IF @var23 IS NOT NULL EXEC(N'ALTER TABLE [FaturaTurleri] DROP CONSTRAINT [' + @var23 + '];');
ALTER TABLE [FaturaTurleri] ADD DEFAULT CAST(0 AS bit) FOR [Silindi];


                SET IDENTITY_INSERT FaturaTurleri ON;

                IF NOT EXISTS(SELECT 1 FROM FaturaTurleri WHERE FaturaTuruID = 1)
                BEGIN
                    INSERT INTO FaturaTurleri (FaturaTuruID, FaturaTuruAdi, HareketTuru, Silindi)
                    VALUES (1, N'Satış Faturası', N'Çıkış', 0)
                END
                ELSE
                BEGIN
                    UPDATE FaturaTurleri
                    SET FaturaTuruAdi = N'Satış Faturası', HareketTuru = N'Çıkış', Silindi = 0
                    WHERE FaturaTuruID = 1
                END

                IF NOT EXISTS(SELECT 1 FROM FaturaTurleri WHERE FaturaTuruID = 2)
                BEGIN
                    INSERT INTO FaturaTurleri (FaturaTuruID, FaturaTuruAdi, HareketTuru, Silindi)
                    VALUES (2, N'Alış Faturası', N'Giriş', 0)
                END
                ELSE
                BEGIN
                    UPDATE FaturaTurleri
                    SET FaturaTuruAdi = N'Alış Faturası', HareketTuru = N'Giriş', Silindi = 0
                    WHERE FaturaTuruID = 2
                END

                SET IDENTITY_INSERT FaturaTurleri OFF;
            

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250511121547_AddFaturaTuruSeedData', N'9.0.4');


                -- FaturaTurleri tablosundaki verileri kontrol et
                PRINT 'FaturaTurleri tablosundaki kayıtlar:';
                SELECT * FROM FaturaTurleri;
            

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250511122311_CheckFaturaTuruSeed', N'9.0.4');

COMMIT;
GO


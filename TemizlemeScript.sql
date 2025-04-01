-- Kullanıcı verileri hariç tüm verileri temizleme

-- Referans bütünlüğünü geçici olarak devre dışı bırak
EXEC sp_MSforeachtable 'ALTER TABLE ? NOCHECK CONSTRAINT ALL'

-- AspNetUsers ve ilgili tablolar hariç tüm tabloları temizle
DELETE FROM BankaHesapHareketleri;
DELETE FROM BankaHesaplari;
DELETE FROM BankaHareketleri;
DELETE FROM Bankalar;
DELETE FROM CariHareketler;
DELETE FROM Cariler;
DELETE FROM FaturaDetaylari;
DELETE FROM FaturaOdemeleri;
DELETE FROM Faturalar;
DELETE FROM IrsaliyeDetaylari;
DELETE FROM Irsaliyeler;
DELETE FROM KasaHareketleri;
DELETE FROM Kasalar;
DELETE FROM KurDegerleri;
DELETE FROM ParaBirimiIliskileri;
DELETE FROM StokFifoKayitlari;
DELETE FROM StokHareketleri;
DELETE FROM UrunFiyatlari;
DELETE FROM UrunKategorileri;
DELETE FROM Urunler;
DELETE FROM Depolar;
DELETE FROM Birimler;
DELETE FROM SistemLoglar;

-- Referans bütünlüğünü tekrar etkinleştir
EXEC sp_MSforeachtable 'ALTER TABLE ? CHECK CONSTRAINT ALL'

-- Identity sıfırlama (varsa)
DBCC CHECKIDENT ('tablonuz', RESEED, 0)

CREATE TABLE [AspNetRoles] (
    [Id] varchar(128) NOT NULL,
    [Name] nvarchar(256) NULL,
    [NormalizedName] nvarchar(256) NULL,
    [ConcurrencyStamp] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
);
GO


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
GO


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
GO


CREATE TABLE [Birimler] (
    [BirimID] uniqueidentifier NOT NULL,
    [BirimAdi] nvarchar(50) NOT NULL,
    [BirimKodu] nvarchar(20) NULL,
    [BirimSembol] nvarchar(10) NULL,
    [Aciklama] nvarchar(200) NULL,
    [Aktif] bit NOT NULL,
    [Silindi] bit NOT NULL,
    [OlusturmaTarihi] datetime2 NULL,
    [GuncellemeTarihi] datetime2 NULL,
    [OlusturanKullaniciID] nvarchar(450) NOT NULL,
    [SonGuncelleyenKullaniciID] uniqueidentifier NULL,
    [SirketID] uniqueidentifier NULL,
    CONSTRAINT [PK_Birimler] PRIMARY KEY ([BirimID])
);
GO


CREATE TABLE [Cariler] (
    [CariID] uniqueidentifier NOT NULL,
    [Ad] nvarchar(100) NOT NULL,
    [CariKodu] nvarchar(20) NOT NULL,
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
    [AktifMi] bit NOT NULL DEFAULT CAST(1 AS bit),
    [OlusturanKullaniciId] uniqueidentifier NULL,
    [SonGuncelleyenKullaniciId] uniqueidentifier NULL,
    [OlusturmaTarihi] datetime2 NOT NULL,
    [GuncellemeTarihi] datetime2 NULL,
    [Silindi] bit NOT NULL DEFAULT CAST(0 AS bit),
    CONSTRAINT [PK_Cariler] PRIMARY KEY ([CariID])
);
GO


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
GO


CREATE TABLE [FaturaTurleri] (
    [FaturaTuruID] int NOT NULL IDENTITY,
    [FaturaTuruAdi] nvarchar(50) NOT NULL,
    [HareketTuru] nvarchar(50) NULL,
    CONSTRAINT [PK_FaturaTurleri] PRIMARY KEY ([FaturaTuruID])
);
GO


CREATE TABLE [FiyatTipleri] (
    [FiyatTipiID] int NOT NULL IDENTITY,
    [TipAdi] nvarchar(50) NOT NULL,
    CONSTRAINT [PK_FiyatTipleri] PRIMARY KEY ([FiyatTipiID])
);
GO


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
GO


CREATE TABLE [IrsaliyeTurleri] (
    [IrsaliyeTuruID] int NOT NULL IDENTITY,
    [IrsaliyeTuruAdi] nvarchar(50) NOT NULL,
    [HareketTuru] nvarchar(50) NULL,
    CONSTRAINT [PK_IrsaliyeTurleri] PRIMARY KEY ([IrsaliyeTuruID])
);
GO


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
GO


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
GO


CREATE TABLE [OdemeTurleri] (
    [OdemeTuruID] int NOT NULL IDENTITY,
    [OdemeTuruAdi] nvarchar(50) NOT NULL,
    CONSTRAINT [PK_OdemeTurleri] PRIMARY KEY ([OdemeTuruID])
);
GO


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
GO


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
GO


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
GO


CREATE TABLE [AspNetRoleClaims] (
    [Id] int NOT NULL IDENTITY,
    [RoleId] varchar(128) NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [AspNetUserClaims] (
    [Id] int NOT NULL IDENTITY,
    [UserId] varchar(128) NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [AspNetUserLogins] (
    [LoginProvider] nvarchar(450) NOT NULL,
    [ProviderKey] nvarchar(450) NOT NULL,
    [ProviderDisplayName] nvarchar(max) NULL,
    [UserId] varchar(128) NOT NULL,
    CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
    CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [AspNetUserRoles] (
    [UserId] varchar(128) NOT NULL,
    [RoleId] varchar(128) NOT NULL,
    CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
    CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [AspNetUserTokens] (
    [UserId] varchar(128) NOT NULL,
    [LoginProvider] nvarchar(450) NOT NULL,
    [Name] nvarchar(450) NOT NULL,
    [Value] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
    CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO


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
GO


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
GO


CREATE TABLE [CariHareketler] (
    [CariHareketID] uniqueidentifier NOT NULL,
    [CariId] uniqueidentifier NOT NULL,
    [HareketTuru] nvarchar(max) NOT NULL,
    [Tutar] decimal(18,2) NOT NULL,
    [Tarih] datetime2 NOT NULL,
    [VadeTarihi] datetime2 NULL,
    [ReferansNo] nvarchar(50) NOT NULL,
    [ReferansTuru] nvarchar(50) NOT NULL,
    [ReferansID] uniqueidentifier NULL,
    [Aciklama] nvarchar(500) NOT NULL,
    [OlusturmaTarihi] datetime2 NOT NULL,
    [OlusturanKullaniciId] uniqueidentifier NULL,
    [Silindi] bit NOT NULL,
    [GuncellemeTarihi] datetime2 NULL,
    CONSTRAINT [PK_CariHareketler] PRIMARY KEY ([CariHareketID]),
    CONSTRAINT [FK_CariHareketler_Cariler_CariId] FOREIGN KEY ([CariId]) REFERENCES [Cariler] ([CariID]) ON DELETE CASCADE
);
GO


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
GO


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
GO


CREATE TABLE [MenuRoller] (
    [MenuId] uniqueidentifier NOT NULL,
    [RolId] varchar(128) NOT NULL,
    [MenuRolID] uniqueidentifier NOT NULL,
    [OlusturmaTarihi] datetime2 NOT NULL,
    CONSTRAINT [PK_MenuRoller] PRIMARY KEY ([MenuId], [RolId]),
    CONSTRAINT [FK_MenuRoller_AspNetRoles_RolId] FOREIGN KEY ([RolId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_MenuRoller_Menuler_MenuId] FOREIGN KEY ([MenuId]) REFERENCES [Menuler] ([MenuID]) ON DELETE CASCADE
);
GO


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
    [Resmi] bit NULL DEFAULT CAST(1 AS bit),
    [DovizTuru] nvarchar(10) NULL,
    [DovizKuru] decimal(18,4) NULL,
    [OlusturmaTarihi] datetime2 NULL,
    [GuncellemeTarihi] datetime2 NULL,
    [Silindi] bit NOT NULL DEFAULT CAST(0 AS bit),
    [Aktif] bit NULL DEFAULT CAST(1 AS bit),
    [OlusturanKullaniciID] uniqueidentifier NULL,
    [SonGuncelleyenKullaniciID] uniqueidentifier NULL,
    [OdemeTuruID] int NULL,
    CONSTRAINT [PK_Faturalar] PRIMARY KEY ([FaturaID]),
    CONSTRAINT [FK_Faturalar_Cariler_CariID] FOREIGN KEY ([CariID]) REFERENCES [Cariler] ([CariID]),
    CONSTRAINT [FK_Faturalar_FaturaTurleri_FaturaTuruID] FOREIGN KEY ([FaturaTuruID]) REFERENCES [FaturaTurleri] ([FaturaTuruID]),
    CONSTRAINT [FK_Faturalar_OdemeTurleri_OdemeTuruID] FOREIGN KEY ([OdemeTuruID]) REFERENCES [OdemeTurleri] ([OdemeTuruID])
);
GO


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
GO


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
GO


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
GO


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
GO


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
GO


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
GO


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
    [Aciklama] nvarchar(200) NULL,
    [OlusturanKullaniciID] uniqueidentifier NULL,
    [SonGuncelleyenKullaniciID] uniqueidentifier NULL,
    [OlusturmaTarihi] datetime2 NOT NULL,
    [GuncellemeTarihi] datetime2 NULL,
    [Silindi] bit NOT NULL,
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
GO


CREATE TABLE [StokFifo] (
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
    CONSTRAINT [PK_StokFifo] PRIMARY KEY ([StokFifoID]),
    CONSTRAINT [FK_StokFifo_Urunler_UrunID] FOREIGN KEY ([UrunID]) REFERENCES [Urunler] ([UrunID]) ON DELETE CASCADE
);
GO


CREATE TABLE [StokHareketleri] (
    [StokHareketID] uniqueidentifier NOT NULL,
    [UrunID] uniqueidentifier NOT NULL,
    [DepoID] uniqueidentifier NULL,
    [Miktar] decimal(18,3) NOT NULL,
    [Birim] nvarchar(50) NULL,
    [Tarih] datetime2 NOT NULL,
    [HareketTuru] nvarchar(50) NOT NULL,
    [ReferansNo] nvarchar(50) NULL,
    [ReferansTuru] nvarchar(50) NULL,
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
    CONSTRAINT [PK_StokHareketleri] PRIMARY KEY ([StokHareketID]),
    CONSTRAINT [FK_StokHareketleri_Depolar_DepoID] FOREIGN KEY ([DepoID]) REFERENCES [Depolar] ([DepoID]),
    CONSTRAINT [FK_StokHareketleri_Urunler_UrunID] FOREIGN KEY ([UrunID]) REFERENCES [Urunler] ([UrunID]) ON DELETE CASCADE
);
GO


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
GO


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
GO


CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [AspNetRoleClaims] ([RoleId]);
GO


CREATE UNIQUE INDEX [RoleNameIndex] ON [AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL;
GO


CREATE INDEX [IX_AspNetUserClaims_UserId] ON [AspNetUserClaims] ([UserId]);
GO


CREATE INDEX [IX_AspNetUserLogins_UserId] ON [AspNetUserLogins] ([UserId]);
GO


CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [AspNetUserRoles] ([RoleId]);
GO


CREATE INDEX [EmailIndex] ON [AspNetUsers] ([NormalizedEmail]);
GO


CREATE UNIQUE INDEX [UserNameIndex] ON [AspNetUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL;
GO


CREATE INDEX [IX_BankaHareketleri_BankaID] ON [BankaHareketleri] ([BankaID]);
GO


CREATE INDEX [IX_BankaHareketleri_CariID] ON [BankaHareketleri] ([CariID]);
GO


CREATE INDEX [IX_BankaHareketleri_HedefKasaID] ON [BankaHareketleri] ([HedefKasaID]);
GO


CREATE INDEX [IX_BankaHareketleri_KaynakKasaID] ON [BankaHareketleri] ([KaynakKasaID]);
GO


CREATE INDEX [IX_BankaHesapHareketleri_BankaHesapID] ON [BankaHesapHareketleri] ([BankaHesapID]);
GO


CREATE INDEX [IX_BankaHesapHareketleri_BankaID] ON [BankaHesapHareketleri] ([BankaID]);
GO


CREATE INDEX [IX_BankaHesapHareketleri_CariID] ON [BankaHesapHareketleri] ([CariID]);
GO


CREATE INDEX [IX_BankaHesapHareketleri_HedefKasaID] ON [BankaHesapHareketleri] ([HedefKasaID]);
GO


CREATE INDEX [IX_BankaHesapHareketleri_KaynakKasaID] ON [BankaHesapHareketleri] ([KaynakKasaID]);
GO


CREATE INDEX [IX_BankaHesaplari_BankaID] ON [BankaHesaplari] ([BankaID]);
GO


CREATE INDEX [IX_CariHareketler_CariId] ON [CariHareketler] ([CariId]);
GO


CREATE INDEX [IX_FaturaDetaylari_BirimID] ON [FaturaDetaylari] ([BirimID]);
GO


CREATE INDEX [IX_FaturaDetaylari_FaturaID] ON [FaturaDetaylari] ([FaturaID]);
GO


CREATE INDEX [IX_FaturaDetaylari_UrunID] ON [FaturaDetaylari] ([UrunID]);
GO


CREATE INDEX [IX_Faturalar_CariID] ON [Faturalar] ([CariID]);
GO


CREATE INDEX [IX_Faturalar_FaturaTuruID] ON [Faturalar] ([FaturaTuruID]);
GO


CREATE INDEX [IX_Faturalar_OdemeTuruID] ON [Faturalar] ([OdemeTuruID]);
GO


CREATE INDEX [IX_FaturaOdemeleri_FaturaID] ON [FaturaOdemeleri] ([FaturaID]);
GO


CREATE INDEX [IX_IrsaliyeDetaylari_BirimID] ON [IrsaliyeDetaylari] ([BirimID]);
GO


CREATE INDEX [IX_IrsaliyeDetaylari_IrsaliyeID] ON [IrsaliyeDetaylari] ([IrsaliyeID]);
GO


CREATE INDEX [IX_IrsaliyeDetaylari_UrunID] ON [IrsaliyeDetaylari] ([UrunID]);
GO


CREATE INDEX [IX_Irsaliyeler_CariID] ON [Irsaliyeler] ([CariID]);
GO


CREATE INDEX [IX_Irsaliyeler_FaturaID] ON [Irsaliyeler] ([FaturaID]);
GO


CREATE INDEX [IX_Irsaliyeler_IrsaliyeTuruID] ON [Irsaliyeler] ([IrsaliyeTuruID]);
GO


CREATE INDEX [IX_KasaHareketleri_CariID] ON [KasaHareketleri] ([CariID]);
GO


CREATE INDEX [IX_KasaHareketleri_HedefBankaID] ON [KasaHareketleri] ([HedefBankaID]);
GO


CREATE INDEX [IX_KasaHareketleri_HedefKasaID] ON [KasaHareketleri] ([HedefKasaID]);
GO


CREATE INDEX [IX_KasaHareketleri_KasaID] ON [KasaHareketleri] ([KasaID]);
GO


CREATE INDEX [IX_KasaHareketleri_KaynakBankaID] ON [KasaHareketleri] ([KaynakBankaID]);
GO


CREATE INDEX [IX_KurDegeri_ParaBirimiID_Tarih] ON [KurDegerleri] ([ParaBirimiID], [Tarih]);
GO


CREATE INDEX [IX_Menuler_UstMenuID] ON [Menuler] ([UstMenuID]);
GO


CREATE INDEX [IX_MenuRoller_RolId] ON [MenuRoller] ([RolId]);
GO


CREATE UNIQUE INDEX [IX_DovizIliski_KaynakParaBirimiID_HedefParaBirimiID] ON [ParaBirimiIliskileri] ([KaynakParaBirimiID], [HedefParaBirimiID]);
GO


CREATE INDEX [IX_ParaBirimiIliskileri_HedefParaBirimiID] ON [ParaBirimiIliskileri] ([HedefParaBirimiID]);
GO


CREATE UNIQUE INDEX [IX_ParaBirimi_Kod] ON [ParaBirimleri] ([Kod]);
GO


CREATE INDEX [IX_SistemLoglar_ApplicationUserId] ON [SistemLoglar] ([ApplicationUserId]);
GO


CREATE INDEX [IX_StokFifo_GirisTarihi] ON [StokFifo] ([GirisTarihi]);
GO


CREATE INDEX [IX_StokFifo_Referans] ON [StokFifo] ([ReferansID], [ReferansTuru]);
GO


CREATE INDEX [IX_StokFifo_StokSorgu] ON [StokFifo] ([UrunID], [KalanMiktar], [Aktif], [Silindi], [Iptal]);
GO


CREATE INDEX [IX_StokFifo_UrunID] ON [StokFifo] ([UrunID]);
GO


CREATE INDEX [IX_StokHareketleri_DepoID] ON [StokHareketleri] ([DepoID]);
GO


CREATE INDEX [IX_StokHareketleri_UrunID] ON [StokHareketleri] ([UrunID]);
GO


CREATE INDEX [IX_UrunFiyatlari_FiyatTipiID] ON [UrunFiyatlari] ([FiyatTipiID]);
GO


CREATE INDEX [IX_UrunFiyatlari_UrunID] ON [UrunFiyatlari] ([UrunID]);
GO


CREATE INDEX [IX_Urunler_BirimID] ON [Urunler] ([BirimID]);
GO


CREATE INDEX [IX_Urunler_KategoriID] ON [Urunler] ([KategoriID]);
GO



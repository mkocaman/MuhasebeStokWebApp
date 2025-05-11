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
    [FullName] nvarchar(100) NULL,
    [Bio] nvarchar(500) NULL,
    [ProfileImage] nvarchar(200) NULL,
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
    [Aktif] bit NOT NULL DEFAULT CAST(1 AS bit),
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
GO


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
GO


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
GO


CREATE TABLE [FaturaTurleri] (
    [FaturaTuruID] int NOT NULL IDENTITY,
    [FaturaTuruAdi] nvarchar(50) NOT NULL,
    [HareketTuru] nvarchar(50) NOT NULL,
    [Silindi] bit NOT NULL DEFAULT CAST(0 AS bit),
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
GO


CREATE TABLE [IrsaliyeTurleri] (
    [IrsaliyeTuruID] int NOT NULL IDENTITY,
    [IrsaliyeTuruAdi] nvarchar(50) NOT NULL,
    [HareketTuru] nvarchar(50) NOT NULL,
    CONSTRAINT [PK_IrsaliyeTurleri] PRIMARY KEY ([IrsaliyeTuruID])
);
GO


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
GO


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
GO


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
GO


CREATE TABLE [OdemeTurleri] (
    [OdemeTuruID] int NOT NULL IDENTITY,
    [OdemeTuruAdi] nvarchar(50) NOT NULL,
    CONSTRAINT [PK_OdemeTurleri] PRIMARY KEY ([OdemeTuruID])
);
GO


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
GO


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
GO


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
GO


CREATE TABLE [TodoItems] (
    [Id] int NOT NULL IDENTITY,
    [Title] nvarchar(200) NOT NULL,
    [Description] nvarchar(1000) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [Deadline] datetime2 NULL,
    [IsCompleted] bit NOT NULL,
    [AssignedToUserId] varchar(128) NULL,
    [TaskCategory] nvarchar(50) NULL,
    [PriorityLevel] int NOT NULL,
    [Status] int NOT NULL,
    [Tags] nvarchar(200) NULL,
    [IsArchived] bit NOT NULL,
    [IsDeleted] bit NOT NULL,
    [ReminderAt] datetime2 NULL,
    [IsReminderSent] bit NOT NULL,
    CONSTRAINT [PK_TodoItems] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_TodoItems_AspNetUsers_AssignedToUserId] FOREIGN KEY ([AssignedToUserId]) REFERENCES [AspNetUsers] ([Id])
);
GO


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
GO


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
GO


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


CREATE TABLE [Cariler] (
    [CariID] uniqueidentifier NOT NULL,
    [Ad] nvarchar(100) NOT NULL,
    [CariKodu] nvarchar(20) NOT NULL,
    [CariTipi] nvarchar(50) NOT NULL,
    [VergiNo] nvarchar(20) NULL,
    [VergiDairesi] nvarchar(50) NULL,
    [Telefon] nvarchar(15) NOT NULL,
    [Email] nvarchar(100) NOT NULL,
    [Yetkili] nvarchar(50) NOT NULL,
    [Adres] nvarchar(250) NOT NULL,
    [Aciklama] nvarchar(500) NULL,
    [Il] nvarchar(50) NOT NULL,
    [Ilce] nvarchar(50) NOT NULL,
    [PostaKodu] nvarchar(10) NOT NULL,
    [Ulke] nvarchar(50) NOT NULL,
    [WebSitesi] nvarchar(100) NULL,
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
GO


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
GO


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
GO


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
GO


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
GO


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
    [BorcDoviz] decimal(18,2) NOT NULL,
    [AlacakDoviz] decimal(18,2) NOT NULL,
    [ParaBirimi] nvarchar(10) NOT NULL,
    [TutarDoviz] decimal(18,2) NOT NULL,
    [OlusturmaTarihi] datetime2 NOT NULL,
    [OlusturanKullaniciId] uniqueidentifier NULL,
    [Silindi] bit NOT NULL,
    [GuncellemeTarihi] datetime2 NULL,
    CONSTRAINT [PK_CariHareketler] PRIMARY KEY ([CariHareketId]),
    CONSTRAINT [FK_CariHareketler_Cariler_CariId] FOREIGN KEY ([CariId]) REFERENCES [Cariler] ([CariID]) ON DELETE NO ACTION
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
GO


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
GO


CREATE TABLE [StokFifoKayitlari] (
    [StokFifoID] uniqueidentifier NOT NULL,
    [UrunID] uniqueidentifier NOT NULL,
    [Miktar] decimal(18,3) NOT NULL,
    [KalanMiktar] decimal(18,3) NOT NULL,
    [BirimFiyat] decimal(18,2) NOT NULL,
    [Birim] nvarchar(50) NOT NULL,
    [ParaBirimi] nvarchar(10) NOT NULL,
    [DovizKuru] decimal(18,6) NOT NULL,
    [BirimFiyatUSD] decimal(18,2) NOT NULL,
    [BirimFiyatUZS] decimal(18,2) NOT NULL,
    [GirisTarihi] datetime2 NOT NULL,
    [SonCikisTarihi] datetime2 NULL,
    [ReferansNo] nvarchar(50) NOT NULL,
    [ReferansTuru] nvarchar(20) NOT NULL,
    [ReferansID] uniqueidentifier NULL,
    [Aciklama] nvarchar(500) NOT NULL,
    [OlusturmaTarihi] datetime2 NOT NULL,
    [GuncellemeTarihi] datetime2 NULL,
    [Aktif] bit NOT NULL,
    [Iptal] bit NOT NULL,
    [Silindi] bit NOT NULL,
    CONSTRAINT [PK_StokFifoKayitlari] PRIMARY KEY ([StokFifoID]),
    CONSTRAINT [FK_StokFifoKayitlari_Urunler_UrunID] FOREIGN KEY ([UrunID]) REFERENCES [Urunler] ([UrunID]) ON DELETE CASCADE
);
GO


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
    [BirimFiyatUSD] decimal(18,6) NULL,
    [BirimFiyatUZS] decimal(18,6) NULL,
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
    [FaturaNotu] nvarchar(500) NULL,
    [ResmiMi] bit NOT NULL DEFAULT CAST(1 AS bit),
    [SozlesmeID] uniqueidentifier NULL,
    [DovizTuru] nvarchar(10) NULL,
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
GO


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
    [DovizKuru] decimal(18,6) NULL,
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
    CONSTRAINT [FK_StokCikisDetaylari_StokFifoKayitlari_StokFifoID] FOREIGN KEY ([StokFifoID]) REFERENCES [StokFifoKayitlari] ([StokFifoID]) ON DELETE SET NULL
);
GO


CREATE TABLE [StokFifoCikislar] (
    [StokFifoCikisID] uniqueidentifier NOT NULL,
    [StokFifoID] uniqueidentifier NULL,
    [ReferansID] uniqueidentifier NULL,
    [DetayID] uniqueidentifier NULL,
    [ReferansNo] nvarchar(max) NULL,
    [ReferansTuru] nvarchar(max) NULL,
    [CikisMiktar] decimal(18,2) NOT NULL,
    [CikisTarihi] datetime2 NOT NULL,
    [BirimFiyatUSD] decimal(18,2) NOT NULL,
    [BirimFiyatUZS] decimal(18,2) NOT NULL,
    [ParaBirimi] nvarchar(max) NULL,
    [DovizKuru] decimal(18,6) NULL,
    [OlusturmaTarihi] datetime2 NOT NULL,
    [OlusturanKullaniciId] uniqueidentifier NULL,
    [Aktif] bit NOT NULL,
    CONSTRAINT [PK_StokFifoCikislar] PRIMARY KEY ([StokFifoCikisID]),
    CONSTRAINT [FK_StokFifoCikislar_StokFifoKayitlari_StokFifoID] FOREIGN KEY ([StokFifoID]) REFERENCES [StokFifoKayitlari] ([StokFifoID]) ON DELETE SET NULL
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
GO


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
GO


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
GO


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
GO


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
GO


IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'FaturaTuruID', N'FaturaTuruAdi', N'HareketTuru') AND [object_id] = OBJECT_ID(N'[FaturaTurleri]'))
    SET IDENTITY_INSERT [FaturaTurleri] ON;
INSERT INTO [FaturaTurleri] ([FaturaTuruID], [FaturaTuruAdi], [HareketTuru])
VALUES (1, N'Satış Faturası', N'Çıkış'),
(2, N'Alış Faturası', N'Giriş');
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'FaturaTuruID', N'FaturaTuruAdi', N'HareketTuru') AND [object_id] = OBJECT_ID(N'[FaturaTurleri]'))
    SET IDENTITY_INSERT [FaturaTurleri] OFF;
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


CREATE INDEX [IX_BirlesikModulKurDegerleri_ParaBirimiID] ON [BirlesikModulKurDegerleri] ([ParaBirimiID]);
GO


CREATE INDEX [IX_BirlesikModulParaBirimiIliskileri_HedefParaBirimiID] ON [BirlesikModulParaBirimiIliskileri] ([HedefParaBirimiID]);
GO


CREATE INDEX [IX_BirlesikModulParaBirimiIliskileri_KaynakParaBirimiID] ON [BirlesikModulParaBirimiIliskileri] ([KaynakParaBirimiID]);
GO


CREATE INDEX [IX_CariHareketler_CariId] ON [CariHareketler] ([CariId]);
GO


CREATE INDEX [IX_Cariler_VarsayilanParaBirimiId] ON [Cariler] ([VarsayilanParaBirimiId]);
GO


CREATE INDEX [IX_FaturaAklamaKuyrugu_FaturaID] ON [FaturaAklamaKuyrugu] ([FaturaID]);
GO


CREATE INDEX [IX_FaturaAklamaKuyrugu_FaturaKalemID] ON [FaturaAklamaKuyrugu] ([FaturaKalemID]);
GO


CREATE INDEX [IX_FaturaAklamaKuyrugu_SozlesmeID] ON [FaturaAklamaKuyrugu] ([SozlesmeID]);
GO


CREATE INDEX [IX_FaturaAklamaKuyrugu_UrunID] ON [FaturaAklamaKuyrugu] ([UrunID]);
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


CREATE INDEX [IX_Faturalar_SozlesmeID] ON [Faturalar] ([SozlesmeID]);
GO


CREATE INDEX [IX_FaturaOdemeleri_FaturaID] ON [FaturaOdemeleri] ([FaturaID]);
GO


CREATE INDEX [IX_IrsaliyeDetaylari_BirimID] ON [IrsaliyeDetaylari] ([BirimID]);
GO


CREATE INDEX [IX_IrsaliyeDetaylari_DepoID] ON [IrsaliyeDetaylari] ([DepoID]);
GO


CREATE INDEX [IX_IrsaliyeDetaylari_IrsaliyeID] ON [IrsaliyeDetaylari] ([IrsaliyeID]);
GO


CREATE INDEX [IX_IrsaliyeDetaylari_UrunID] ON [IrsaliyeDetaylari] ([UrunID]);
GO


CREATE INDEX [IX_Irsaliyeler_CariID] ON [Irsaliyeler] ([CariID]);
GO


CREATE INDEX [IX_Irsaliyeler_DepoID] ON [Irsaliyeler] ([DepoID]);
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


CREATE INDEX [IX_Menuler_UstMenuID] ON [Menuler] ([UstMenuID]);
GO


CREATE INDEX [IX_MenuRoller_RolId] ON [MenuRoller] ([RolId]);
GO


CREATE INDEX [IX_Notifications_UserId] ON [Notifications] ([UserId]);
GO


CREATE INDEX [IX_ParaBirimiModuluKurDegerleri_ParaBirimiID] ON [ParaBirimiModuluKurDegerleri] ([ParaBirimiID]);
GO


CREATE UNIQUE INDEX [IX_DovizIliski_KaynakParaBirimiID_HedefParaBirimiID] ON [ParaBirimiModuluParaBirimiIliskileri] ([KaynakParaBirimiID], [HedefParaBirimiID]);
GO


CREATE INDEX [IX_ParaBirimiModuluParaBirimiIliskileri_HedefParaBirimiID] ON [ParaBirimiModuluParaBirimiIliskileri] ([HedefParaBirimiID]);
GO


CREATE UNIQUE INDEX [IX_ParaBirimi_Kod] ON [ParaBirimiModuluParaBirimleri] ([Kod]);
GO


CREATE INDEX [IX_SistemLoglar_KullaniciId] ON [SistemLoglar] ([KullaniciId]);
GO


CREATE INDEX [IX_Sozlesmeler_CariID] ON [Sozlesmeler] ([CariID]);
GO


CREATE INDEX [IX_StokCikisDetaylari_StokFifoID] ON [StokCikisDetaylari] ([StokFifoID]);
GO


CREATE INDEX [IX_StokFifoCikislar_StokFifoID] ON [StokFifoCikislar] ([StokFifoID]);
GO


CREATE INDEX [IX_StokFifo_GirisTarihi] ON [StokFifoKayitlari] ([GirisTarihi]);
GO


CREATE INDEX [IX_StokFifo_Referans] ON [StokFifoKayitlari] ([ReferansID], [ReferansTuru]);
GO


CREATE INDEX [IX_StokFifo_StokSorgu] ON [StokFifoKayitlari] ([UrunID], [KalanMiktar], [Aktif], [Silindi], [Iptal]);
GO


CREATE INDEX [IX_StokFifo_UrunID] ON [StokFifoKayitlari] ([UrunID]);
GO


CREATE INDEX [IX_StokHareketleri_DepoID] ON [StokHareketleri] ([DepoID]);
GO


CREATE INDEX [IX_StokHareketleri_UrunID] ON [StokHareketleri] ([UrunID]);
GO


CREATE INDEX [IX_TodoComments_AppUserId] ON [TodoComments] ([AppUserId]);
GO


CREATE INDEX [IX_TodoComments_TodoItemId] ON [TodoComments] ([TodoItemId]);
GO


CREATE INDEX [IX_TodoItems_AssignedToUserId] ON [TodoItems] ([AssignedToUserId]);
GO


CREATE INDEX [IX_UrunFiyatlari_FiyatTipiID] ON [UrunFiyatlari] ([FiyatTipiID]);
GO


CREATE INDEX [IX_UrunFiyatlari_UrunID] ON [UrunFiyatlari] ([UrunID]);
GO


CREATE INDEX [IX_Urunler_BirimID] ON [Urunler] ([BirimID]);
GO


CREATE INDEX [IX_Urunler_KategoriID] ON [Urunler] ([KategoriID]);
GO



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

CREATE TABLE [Birimler] (
    [BirimID] uniqueidentifier NOT NULL,
    [BirimAdi] nvarchar(50) NOT NULL,
    [Aciklama] nvarchar(200) NOT NULL,
    [Aktif] bit NOT NULL,
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

CREATE TABLE [OdemeTurleri] (
    [OdemeTuruID] int NOT NULL IDENTITY,
    [OdemeTuruAdi] nvarchar(50) NOT NULL,
    CONSTRAINT [PK_OdemeTurleri] PRIMARY KEY ([OdemeTuruID])
);

CREATE TABLE [Urunler] (
    [UrunID] uniqueidentifier NOT NULL,
    [UrunKodu] nvarchar(50) NOT NULL,
    [UrunAdi] nvarchar(200) NOT NULL,
    [Birim] nvarchar(50) NOT NULL,
    [StokMiktar] decimal(18,2) NOT NULL DEFAULT 0.0,
    [Aktif] bit NOT NULL DEFAULT CAST(1 AS bit),
    [OlusturanKullaniciID] uniqueidentifier NULL,
    [SonGuncelleyenKullaniciID] uniqueidentifier NULL,
    [OlusturmaTarihi] datetime2 NULL,
    [GuncellemeTarihi] datetime2 NULL,
    [SoftDelete] bit NOT NULL DEFAULT CAST(0 AS bit),
    CONSTRAINT [PK_Urunler] PRIMARY KEY ([UrunID])
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

CREATE TABLE [Faturalar] (
    [FaturaID] uniqueidentifier NOT NULL,
    [CariID] uniqueidentifier NULL,
    [FaturaTarihi] datetime2 NOT NULL,
    [Aktif] bit NOT NULL DEFAULT CAST(1 AS bit),
    [OlusturanKullaniciID] uniqueidentifier NULL,
    [SonGuncelleyenKullaniciID] uniqueidentifier NULL,
    [OlusturmaTarihi] datetime2 NULL,
    [GuncellemeTarihi] datetime2 NULL,
    [SoftDelete] bit NOT NULL DEFAULT CAST(0 AS bit),
    [Resmi] bit NULL DEFAULT CAST(1 AS bit),
    [VadeTarihi] datetime2 NULL,
    [OdemeTuruID] int NULL,
    [FaturaNotu] nvarchar(max) NOT NULL,
    [SiparisNumarasi] nvarchar(50) NOT NULL,
    [DovizTuru] nvarchar(10) NOT NULL,
    [DovizKuru] decimal(18,2) NULL,
    [KdvOrani] decimal(18,2) NULL,
    [GenelToplam] decimal(18,2) NULL,
    [KDVToplam] decimal(18,2) NULL,
    [FaturaNumarasi] nvarchar(50) NOT NULL,
    [FaturaTuruID] int NULL,
    [AraToplam] decimal(18,2) NULL,
    CONSTRAINT [PK_Faturalar] PRIMARY KEY ([FaturaID]),
    CONSTRAINT [FK_Faturalar_Cariler_CariID] FOREIGN KEY ([CariID]) REFERENCES [Cariler] ([CariID]),
    CONSTRAINT [FK_Faturalar_FaturaTurleri_FaturaTuruID] FOREIGN KEY ([FaturaTuruID]) REFERENCES [FaturaTurleri] ([FaturaTuruID]),
    CONSTRAINT [FK_Faturalar_OdemeTurleri_OdemeTuruID] FOREIGN KEY ([OdemeTuruID]) REFERENCES [OdemeTurleri] ([OdemeTuruID])
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
    [BirimID] uniqueidentifier NULL,
    CONSTRAINT [PK_FaturaDetaylari] PRIMARY KEY ([FaturaDetayID]),
    CONSTRAINT [FK_FaturaDetaylari_Birimler_BirimID] FOREIGN KEY ([BirimID]) REFERENCES [Birimler] ([BirimID]),
    CONSTRAINT [FK_FaturaDetaylari_Faturalar_FaturaID] FOREIGN KEY ([FaturaID]) REFERENCES [Faturalar] ([FaturaID]) ON DELETE CASCADE,
    CONSTRAINT [FK_FaturaDetaylari_Urunler_UrunID] FOREIGN KEY ([UrunID]) REFERENCES [Urunler] ([UrunID]) ON DELETE CASCADE
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

CREATE INDEX [IX_CariHareketler_CariID] ON [CariHareketler] ([CariID]);

CREATE INDEX [IX_FaturaDetaylari_BirimID] ON [FaturaDetaylari] ([BirimID]);

CREATE INDEX [IX_FaturaDetaylari_FaturaID] ON [FaturaDetaylari] ([FaturaID]);

CREATE INDEX [IX_FaturaDetaylari_UrunID] ON [FaturaDetaylari] ([UrunID]);

CREATE INDEX [IX_Faturalar_CariID] ON [Faturalar] ([CariID]);

CREATE INDEX [IX_Faturalar_FaturaTuruID] ON [Faturalar] ([FaturaTuruID]);

CREATE INDEX [IX_Faturalar_OdemeTuruID] ON [Faturalar] ([OdemeTuruID]);

CREATE INDEX [IX_IrsaliyeDetaylari_BirimID] ON [IrsaliyeDetaylari] ([BirimID]);

CREATE INDEX [IX_IrsaliyeDetaylari_IrsaliyeID] ON [IrsaliyeDetaylari] ([IrsaliyeID]);

CREATE INDEX [IX_IrsaliyeDetaylari_UrunID] ON [IrsaliyeDetaylari] ([UrunID]);

CREATE INDEX [IX_Irsaliyeler_CariID] ON [Irsaliyeler] ([CariID]);

CREATE INDEX [IX_Irsaliyeler_FaturaID] ON [Irsaliyeler] ([FaturaID]);

CREATE INDEX [IX_Irsaliyeler_IrsaliyeTuruID] ON [Irsaliyeler] ([IrsaliyeTuruID]);

CREATE INDEX [IX_StokHareketleri_DepoID] ON [StokHareketleri] ([DepoID]);

CREATE INDEX [IX_StokHareketleri_UrunID] ON [StokHareketleri] ([UrunID]);

CREATE INDEX [IX_UrunFiyatlari_FiyatTipiID] ON [UrunFiyatlari] ([FiyatTipiID]);

CREATE INDEX [IX_UrunFiyatlari_UrunID] ON [UrunFiyatlari] ([UrunID]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250310143537_InitialCreate', N'9.0.2');

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

CREATE TABLE [Kasalar] (
    [KasaID] uniqueidentifier NOT NULL,
    [KasaAdi] nvarchar(100) NOT NULL,
    [KasaTuru] nvarchar(50) NOT NULL,
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

CREATE TABLE [BankaHareketleri] (
    [BankaHareketID] uniqueidentifier NOT NULL,
    [BankaID] uniqueidentifier NOT NULL,
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
    CONSTRAINT [FK_BankaHareketleri_Bankalar_BankaID] FOREIGN KEY ([BankaID]) REFERENCES [Bankalar] ([BankaID]) ON DELETE CASCADE
);

CREATE TABLE [KasaHareketleri] (
    [KasaHareketID] uniqueidentifier NOT NULL,
    [KasaID] uniqueidentifier NOT NULL,
    [Tutar] decimal(18,2) NOT NULL,
    [HareketTuru] nvarchar(50) NOT NULL,
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
    CONSTRAINT [PK_KasaHareketleri] PRIMARY KEY ([KasaHareketID]),
    CONSTRAINT [FK_KasaHareketleri_Kasalar_KasaID] FOREIGN KEY ([KasaID]) REFERENCES [Kasalar] ([KasaID]) ON DELETE CASCADE
);

CREATE INDEX [IX_BankaHareketleri_BankaID] ON [BankaHareketleri] ([BankaID]);

CREATE INDEX [IX_KasaHareketleri_KasaID] ON [KasaHareketleri] ([KasaID]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250310190502_FixDecimalPrecisions', N'9.0.2');

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'KasaID', N'KasaAdi', N'KasaTuru', N'AcilisBakiye', N'GuncelBakiye', N'Aciklama', N'Aktif', N'OlusturmaTarihi', N'SoftDelete') AND [object_id] = OBJECT_ID(N'[Kasalar]'))
    SET IDENTITY_INSERT [Kasalar] ON;
INSERT INTO [Kasalar] ([KasaID], [KasaAdi], [KasaTuru], [AcilisBakiye], [GuncelBakiye], [Aciklama], [Aktif], [OlusturmaTarihi], [SoftDelete])
VALUES ('fce55b17-749c-48a1-bb21-0ccbab7fcdf8', N'Ana Kasa', N'Merkez', 1000.0, 1000.0, N'Merkez ofis ana kasası', CAST(1 AS bit), '2025-03-18T10:14:15.1233000+05:00', CAST(0 AS bit));
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'KasaID', N'KasaAdi', N'KasaTuru', N'AcilisBakiye', N'GuncelBakiye', N'Aciklama', N'Aktif', N'OlusturmaTarihi', N'SoftDelete') AND [object_id] = OBJECT_ID(N'[Kasalar]'))
    SET IDENTITY_INSERT [Kasalar] OFF;

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'BankaID', N'BankaAdi', N'SubeAdi', N'SubeKodu', N'HesapNo', N'IBAN', N'ParaBirimi', N'AcilisBakiye', N'GuncelBakiye', N'Aciklama', N'Aktif', N'OlusturmaTarihi', N'SoftDelete') AND [object_id] = OBJECT_ID(N'[Bankalar]'))
    SET IDENTITY_INSERT [Bankalar] ON;
INSERT INTO [Bankalar] ([BankaID], [BankaAdi], [SubeAdi], [SubeKodu], [HesapNo], [IBAN], [ParaBirimi], [AcilisBakiye], [GuncelBakiye], [Aciklama], [Aktif], [OlusturmaTarihi], [SoftDelete])
VALUES ('3b46b791-1495-4d4e-baf4-4cf3526092dd', N'Test Bankası', N'Test Şubesi', N'100', N'123456789', N'TR123456789012345678901234', N'TL', 5000.0, 5000.0, N'Test banka hesabı', CAST(1 AS bit), '2025-03-18T10:14:15.1406480+05:00', CAST(0 AS bit));
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'BankaID', N'BankaAdi', N'SubeAdi', N'SubeKodu', N'HesapNo', N'IBAN', N'ParaBirimi', N'AcilisBakiye', N'GuncelBakiye', N'Aciklama', N'Aktif', N'OlusturmaTarihi', N'SoftDelete') AND [object_id] = OBJECT_ID(N'[Bankalar]'))
    SET IDENTITY_INSERT [Bankalar] OFF;

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'BankaHareketID', N'BankaID', N'Tutar', N'HareketTuru', N'Tarih', N'ReferansNo', N'ReferansTuru', N'Aciklama', N'DekontNo', N'OlusturmaTarihi', N'SoftDelete', N'KarsiUnvan', N'KarsiBankaAdi', N'KarsiIBAN') AND [object_id] = OBJECT_ID(N'[BankaHareketleri]'))
    SET IDENTITY_INSERT [BankaHareketleri] ON;
INSERT INTO [BankaHareketleri] ([BankaHareketID], [BankaID], [Tutar], [HareketTuru], [Tarih], [ReferansNo], [ReferansTuru], [Aciklama], [DekontNo], [OlusturmaTarihi], [SoftDelete], [KarsiUnvan], [KarsiBankaAdi], [KarsiIBAN])
VALUES ('ce89119e-da49-4635-a945-e8fffb11965e', '3b46b791-1495-4d4e-baf4-4cf3526092dd', 1000.0, N'Giriş', '2025-03-18T10:14:15.1406510+05:00', N'REF001', N'Havale', N'Test banka hareketi', N'DK001', '2025-03-18T10:14:15.1406510+05:00', CAST(0 AS bit), N'Test Müşteri', N'Test Karşı Banka', N'TR9876543210');
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'BankaHareketID', N'BankaID', N'Tutar', N'HareketTuru', N'Tarih', N'ReferansNo', N'ReferansTuru', N'Aciklama', N'DekontNo', N'OlusturmaTarihi', N'SoftDelete', N'KarsiUnvan', N'KarsiBankaAdi', N'KarsiIBAN') AND [object_id] = OBJECT_ID(N'[BankaHareketleri]'))
    SET IDENTITY_INSERT [BankaHareketleri] OFF;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250311052830_AddTestData', N'9.0.2');

ALTER TABLE [Faturalar] ADD [OdemeDurumu] nvarchar(50) NOT NULL DEFAULT N'';

ALTER TABLE [FaturaDetaylari] ADD [IndirimTutari] decimal(18,2) NULL;

ALTER TABLE [FaturaDetaylari] ADD [KdvTutari] decimal(18,2) NULL;

ALTER TABLE [FaturaDetaylari] ADD [NetTutar] decimal(18,2) NULL;

ALTER TABLE [FaturaDetaylari] ADD [Tutar] decimal(18,2) NULL;

CREATE TABLE [DovizKurlari] (
    [DovizKuruID] uniqueidentifier NOT NULL,
    [DovizKodu] nvarchar(10) NOT NULL,
    [DovizAdi] nvarchar(100) NOT NULL,
    [AlisFiyati] decimal(18,2) NOT NULL,
    [SatisFiyati] decimal(18,2) NOT NULL,
    [EfektifAlisFiyati] decimal(18,2) NOT NULL,
    [EfektifSatisFiyati] decimal(18,2) NOT NULL,
    [Tarih] datetime2 NOT NULL,
    [GuncellemeTarihi] datetime2 NOT NULL,
    [Aktif] bit NOT NULL,
    CONSTRAINT [PK_DovizKurlari] PRIMARY KEY ([DovizKuruID])
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

CREATE TABLE [SistemAyarlari] (
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
    [GuncellemeTarihi] datetime2 NOT NULL,
    CONSTRAINT [PK_SistemAyarlari] PRIMARY KEY ([SistemAyarlariID])
);

CREATE INDEX [IX_FaturaOdemeleri_FaturaID] ON [FaturaOdemeleri] ([FaturaID]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250311073434_AddDovizKuruEntity', N'9.0.2');

ALTER TABLE [KasaHareketleri] DROP CONSTRAINT [FK_KasaHareketleri_Kasalar_KasaID];

ALTER TABLE [Kasalar] ADD [ParaBirimi] nvarchar(3) NOT NULL DEFAULT N'';

ALTER TABLE [KasaHareketleri] ADD [CariID] uniqueidentifier NULL;

ALTER TABLE [KasaHareketleri] ADD [DovizKuru] decimal(18,2) NOT NULL DEFAULT 0.0;

ALTER TABLE [KasaHareketleri] ADD [HedefBankaID] uniqueidentifier NULL;

ALTER TABLE [KasaHareketleri] ADD [HedefKasaID] uniqueidentifier NULL;

ALTER TABLE [KasaHareketleri] ADD [IslemTuru] nvarchar(max) NOT NULL DEFAULT N'';

ALTER TABLE [KasaHareketleri] ADD [KarsiParaBirimi] nvarchar(3) NOT NULL DEFAULT N'';

CREATE INDEX [IX_KasaHareketleri_CariID] ON [KasaHareketleri] ([CariID]);

CREATE INDEX [IX_KasaHareketleri_HedefBankaID] ON [KasaHareketleri] ([HedefBankaID]);

CREATE INDEX [IX_KasaHareketleri_HedefKasaID] ON [KasaHareketleri] ([HedefKasaID]);

ALTER TABLE [KasaHareketleri] ADD CONSTRAINT [FK_KasaHareketleri_Bankalar_HedefBankaID] FOREIGN KEY ([HedefBankaID]) REFERENCES [Bankalar] ([BankaID]);

ALTER TABLE [KasaHareketleri] ADD CONSTRAINT [FK_KasaHareketleri_Cariler_CariID] FOREIGN KEY ([CariID]) REFERENCES [Cariler] ([CariID]);

ALTER TABLE [KasaHareketleri] ADD CONSTRAINT [FK_KasaHareketleri_Kasalar_HedefKasaID] FOREIGN KEY ([HedefKasaID]) REFERENCES [Kasalar] ([KasaID]);

ALTER TABLE [KasaHareketleri] ADD CONSTRAINT [FK_KasaHareketleri_Kasalar_KasaID] FOREIGN KEY ([KasaID]) REFERENCES [Kasalar] ([KasaID]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250311172915_AddParaBirimiToKasa', N'9.0.2');

DECLARE @var sysname;
SELECT @var = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[KasaHareketleri]') AND [c].[name] = N'IslemTuru');
IF @var IS NOT NULL EXEC(N'ALTER TABLE [KasaHareketleri] DROP CONSTRAINT [' + @var + '];');
ALTER TABLE [KasaHareketleri] ALTER COLUMN [IslemTuru] nvarchar(20) NOT NULL;

DECLARE @var1 sysname;
SELECT @var1 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[KasaHareketleri]') AND [c].[name] = N'HareketTuru');
IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [KasaHareketleri] DROP CONSTRAINT [' + @var1 + '];');
ALTER TABLE [KasaHareketleri] ALTER COLUMN [HareketTuru] nvarchar(20) NOT NULL;

DECLARE @var2 sysname;
SELECT @var2 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[KasaHareketleri]') AND [c].[name] = N'DovizKuru');
IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [KasaHareketleri] DROP CONSTRAINT [' + @var2 + '];');
ALTER TABLE [KasaHareketleri] ALTER COLUMN [DovizKuru] decimal(18,6) NOT NULL;

ALTER TABLE [KasaHareketleri] ADD [KaynakBankaID] uniqueidentifier NULL;

ALTER TABLE [KasaHareketleri] ADD [TransferID] uniqueidentifier NULL;

DECLARE @var3 sysname;
SELECT @var3 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[DovizKurlari]') AND [c].[name] = N'SatisFiyati');
IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [DovizKurlari] DROP CONSTRAINT [' + @var3 + '];');
ALTER TABLE [DovizKurlari] ALTER COLUMN [SatisFiyati] decimal(18,6) NOT NULL;

DECLARE @var4 sysname;
SELECT @var4 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[DovizKurlari]') AND [c].[name] = N'EfektifSatisFiyati');
IF @var4 IS NOT NULL EXEC(N'ALTER TABLE [DovizKurlari] DROP CONSTRAINT [' + @var4 + '];');
ALTER TABLE [DovizKurlari] ALTER COLUMN [EfektifSatisFiyati] decimal(18,6) NOT NULL;

DECLARE @var5 sysname;
SELECT @var5 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[DovizKurlari]') AND [c].[name] = N'EfektifAlisFiyati');
IF @var5 IS NOT NULL EXEC(N'ALTER TABLE [DovizKurlari] DROP CONSTRAINT [' + @var5 + '];');
ALTER TABLE [DovizKurlari] ALTER COLUMN [EfektifAlisFiyati] decimal(18,6) NOT NULL;

DECLARE @var6 sysname;
SELECT @var6 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[DovizKurlari]') AND [c].[name] = N'AlisFiyati');
IF @var6 IS NOT NULL EXEC(N'ALTER TABLE [DovizKurlari] DROP CONSTRAINT [' + @var6 + '];');
ALTER TABLE [DovizKurlari] ALTER COLUMN [AlisFiyati] decimal(18,6) NOT NULL;

ALTER TABLE [DovizKurlari] ADD [HedefParaBirimi] nvarchar(3) NOT NULL DEFAULT N'';

ALTER TABLE [DovizKurlari] ADD [KaynakParaBirimi] nvarchar(3) NOT NULL DEFAULT N'';

ALTER TABLE [DovizKurlari] ADD [KurDegeri] decimal(18,6) NOT NULL DEFAULT 0.0;

ALTER TABLE [BankaHareketleri] ADD [HedefKasaID] uniqueidentifier NULL;

ALTER TABLE [BankaHareketleri] ADD [KaynakKasaID] uniqueidentifier NULL;

ALTER TABLE [BankaHareketleri] ADD [TransferID] uniqueidentifier NULL;

CREATE INDEX [IX_KasaHareketleri_KaynakBankaID] ON [KasaHareketleri] ([KaynakBankaID]);

CREATE INDEX [IX_BankaHareketleri_HedefKasaID] ON [BankaHareketleri] ([HedefKasaID]);

CREATE INDEX [IX_BankaHareketleri_KaynakKasaID] ON [BankaHareketleri] ([KaynakKasaID]);

ALTER TABLE [BankaHareketleri] ADD CONSTRAINT [FK_BankaHareketleri_Kasalar_HedefKasaID] FOREIGN KEY ([HedefKasaID]) REFERENCES [Kasalar] ([KasaID]);

ALTER TABLE [BankaHareketleri] ADD CONSTRAINT [FK_BankaHareketleri_Kasalar_KaynakKasaID] FOREIGN KEY ([KaynakKasaID]) REFERENCES [Kasalar] ([KasaID]);

ALTER TABLE [KasaHareketleri] ADD CONSTRAINT [FK_KasaHareketleri_Bankalar_KaynakBankaID] FOREIGN KEY ([KaynakBankaID]) REFERENCES [Bankalar] ([BankaID]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250311191745_KasaBankaTransferEklendi', N'9.0.2');

ALTER TABLE [BankaHareketleri] ADD [CariID] uniqueidentifier NULL;

CREATE INDEX [IX_BankaHareketleri_CariID] ON [BankaHareketleri] ([CariID]);

ALTER TABLE [BankaHareketleri] ADD CONSTRAINT [FK_BankaHareketleri_Cariler_CariID] FOREIGN KEY ([CariID]) REFERENCES [Cariler] ([CariID]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250311193739_BankaHareketCariIliskisi', N'9.0.2');

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250311194021_BankaHareketCariIliskisiGuncelleme', N'9.0.2');

ALTER TABLE [Birimler] ADD [SoftDelete] bit NOT NULL DEFAULT CAST(0 AS bit);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250312072019_AddSoftDeleteToBirim', N'9.0.2');

ALTER TABLE [Urunler] ADD [KategoriID] uniqueidentifier NULL;

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

CREATE INDEX [IX_Urunler_KategoriID] ON [Urunler] ([KategoriID]);

ALTER TABLE [Urunler] ADD CONSTRAINT [FK_Urunler_UrunKategorileri_KategoriID] FOREIGN KEY ([KategoriID]) REFERENCES [UrunKategorileri] ([KategoriID]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250312131856_AddKategoriIDToUrun', N'9.0.2');

DECLARE @var7 sysname;
SELECT @var7 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Faturalar]') AND [c].[name] = N'KdvOrani');
IF @var7 IS NOT NULL EXEC(N'ALTER TABLE [Faturalar] DROP CONSTRAINT [' + @var7 + '];');
ALTER TABLE [Faturalar] DROP COLUMN [KdvOrani];

DECLARE @var8 sysname;
SELECT @var8 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Urunler]') AND [c].[name] = N'Birim');
IF @var8 IS NOT NULL EXEC(N'ALTER TABLE [Urunler] DROP CONSTRAINT [' + @var8 + '];');
ALTER TABLE [Urunler] ALTER COLUMN [Birim] nvarchar(50) NULL;

DECLARE @var9 sysname;
SELECT @var9 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[UrunKategorileri]') AND [c].[name] = N'Aciklama');
IF @var9 IS NOT NULL EXEC(N'ALTER TABLE [UrunKategorileri] DROP CONSTRAINT [' + @var9 + '];');
ALTER TABLE [UrunKategorileri] ALTER COLUMN [Aciklama] nvarchar(500) NULL;

DECLARE @var10 sysname;
SELECT @var10 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Faturalar]') AND [c].[name] = N'SiparisNumarasi');
IF @var10 IS NOT NULL EXEC(N'ALTER TABLE [Faturalar] DROP CONSTRAINT [' + @var10 + '];');
ALTER TABLE [Faturalar] ALTER COLUMN [SiparisNumarasi] nvarchar(20) NOT NULL;

DECLARE @var11 sysname;
SELECT @var11 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Faturalar]') AND [c].[name] = N'FaturaTarihi');
IF @var11 IS NOT NULL EXEC(N'ALTER TABLE [Faturalar] DROP CONSTRAINT [' + @var11 + '];');
ALTER TABLE [Faturalar] ALTER COLUMN [FaturaTarihi] datetime2 NULL;

DECLARE @var12 sysname;
SELECT @var12 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Faturalar]') AND [c].[name] = N'FaturaNumarasi');
IF @var12 IS NOT NULL EXEC(N'ALTER TABLE [Faturalar] DROP CONSTRAINT [' + @var12 + '];');
ALTER TABLE [Faturalar] ALTER COLUMN [FaturaNumarasi] nvarchar(20) NOT NULL;

DECLARE @var13 sysname;
SELECT @var13 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Faturalar]') AND [c].[name] = N'FaturaNotu');
IF @var13 IS NOT NULL EXEC(N'ALTER TABLE [Faturalar] DROP CONSTRAINT [' + @var13 + '];');
ALTER TABLE [Faturalar] ALTER COLUMN [FaturaNotu] nvarchar(500) NOT NULL;

DECLARE @var14 sysname;
SELECT @var14 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Faturalar]') AND [c].[name] = N'DovizKuru');
IF @var14 IS NOT NULL EXEC(N'ALTER TABLE [Faturalar] DROP CONSTRAINT [' + @var14 + '];');
ALTER TABLE [Faturalar] ALTER COLUMN [DovizKuru] decimal(18,4) NULL;

DECLARE @var15 sysname;
SELECT @var15 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Faturalar]') AND [c].[name] = N'Aktif');
IF @var15 IS NOT NULL EXEC(N'ALTER TABLE [Faturalar] DROP CONSTRAINT [' + @var15 + '];');
ALTER TABLE [Faturalar] ALTER COLUMN [Aktif] bit NULL;
ALTER TABLE [Faturalar] ADD DEFAULT CAST(1 AS bit) FOR [Aktif];

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250313043602_UpdateUrunAndUrunKategoriNullability', N'9.0.2');

DECLARE @var16 sysname;
SELECT @var16 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[UrunKategorileri]') AND [c].[name] = N'Aciklama');
IF @var16 IS NOT NULL EXEC(N'ALTER TABLE [UrunKategorileri] DROP CONSTRAINT [' + @var16 + '];');
UPDATE [UrunKategorileri] SET [Aciklama] = N'' WHERE [Aciklama] IS NULL;
ALTER TABLE [UrunKategorileri] ALTER COLUMN [Aciklama] nvarchar(500) NOT NULL;
ALTER TABLE [UrunKategorileri] ADD DEFAULT N'' FOR [Aciklama];

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250314063223_FixUrunKategoriPrimaryKey', N'9.0.2');

CREATE TABLE [SistemLoglar] (
    [LogID] uniqueidentifier NOT NULL,
    [IslemTuru] nvarchar(50) NOT NULL,
    [KayitID] uniqueidentifier NULL,
    [TabloAdi] nvarchar(100) NOT NULL,
    [KayitAdi] nvarchar(200) NOT NULL,
    [IslemTarihi] datetime2 NOT NULL,
    [Aciklama] nvarchar(500) NOT NULL,
    [KullaniciID] uniqueidentifier NULL,
    [KullaniciAdi] nvarchar(100) NOT NULL,
    [IPAdresi] nvarchar(50) NOT NULL,
    [Basarili] bit NOT NULL,
    [HataMesaji] nvarchar(500) NOT NULL,
    CONSTRAINT [PK_SistemLoglar] PRIMARY KEY ([LogID])
);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250314190911_AddSistemLogTable', N'9.0.2');

ALTER TABLE [Urunler] ADD [BirimID] uniqueidentifier NULL;

CREATE INDEX [IX_Urunler_BirimID] ON [Urunler] ([BirimID]);

ALTER TABLE [Urunler] ADD CONSTRAINT [FK_Urunler_Birimler_BirimID] FOREIGN KEY ([BirimID]) REFERENCES [Birimler] ([BirimID]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250315072950_UpdateEntitiesForSoftDelete', N'9.0.2');

DECLARE @var17 sysname;
SELECT @var17 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Urunler]') AND [c].[name] = N'Birim');
IF @var17 IS NOT NULL EXEC(N'ALTER TABLE [Urunler] DROP CONSTRAINT [' + @var17 + '];');
ALTER TABLE [Urunler] DROP COLUMN [Birim];

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250315091600_UpdateDeleteBehaviors', N'9.0.2');

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250315091901_RemoveBirimRelationships', N'9.0.2');

DECLARE @var18 sysname;
SELECT @var18 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[SistemLoglar]') AND [c].[name] = N'TabloAdi');
IF @var18 IS NOT NULL EXEC(N'ALTER TABLE [SistemLoglar] DROP CONSTRAINT [' + @var18 + '];');
ALTER TABLE [SistemLoglar] ALTER COLUMN [TabloAdi] nvarchar(50) NOT NULL;

DECLARE @var19 sysname;
SELECT @var19 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[SistemLoglar]') AND [c].[name] = N'KullaniciID');
IF @var19 IS NOT NULL EXEC(N'ALTER TABLE [SistemLoglar] DROP CONSTRAINT [' + @var19 + '];');
ALTER TABLE [SistemLoglar] ALTER COLUMN [KullaniciID] nvarchar(max) NULL;

DECLARE @var20 sysname;
SELECT @var20 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[SistemLoglar]') AND [c].[name] = N'KayitAdi');
IF @var20 IS NOT NULL EXEC(N'ALTER TABLE [SistemLoglar] DROP CONSTRAINT [' + @var20 + '];');
ALTER TABLE [SistemLoglar] ALTER COLUMN [KayitAdi] nvarchar(100) NOT NULL;

ALTER TABLE [SistemLoglar] ADD [IlgiliID] nvarchar(100) NULL;

ALTER TABLE [SistemLoglar] ADD [SoftDelete] bit NOT NULL DEFAULT CAST(0 AS bit);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250317085152_AddMissingSistemLogColumns', N'9.0.2');

DECLARE @var21 sysname;
SELECT @var21 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[SistemLoglar]') AND [c].[name] = N'KullaniciAdi');
IF @var21 IS NOT NULL EXEC(N'ALTER TABLE [SistemLoglar] DROP CONSTRAINT [' + @var21 + '];');
ALTER TABLE [SistemLoglar] ALTER COLUMN [KullaniciAdi] nvarchar(50) NOT NULL;

DECLARE @var22 sysname;
SELECT @var22 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[SistemLoglar]') AND [c].[name] = N'IlgiliID');
IF @var22 IS NOT NULL EXEC(N'ALTER TABLE [SistemLoglar] DROP CONSTRAINT [' + @var22 + '];');
ALTER TABLE [SistemLoglar] ALTER COLUMN [IlgiliID] nvarchar(max) NULL;

DECLARE @var23 sysname;
SELECT @var23 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[SistemLoglar]') AND [c].[name] = N'HataMesaji');
IF @var23 IS NOT NULL EXEC(N'ALTER TABLE [SistemLoglar] DROP CONSTRAINT [' + @var23 + '];');
ALTER TABLE [SistemLoglar] ALTER COLUMN [HataMesaji] nvarchar(500) NULL;

ALTER TABLE [SistemLoglar] ADD [Tarayici] nvarchar(100) NULL;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250317091050_CreateStokFifoTableNew', N'9.0.2');

ALTER TABLE [SistemAyarlari] ADD [IkinciDovizKodu] nvarchar(10) NULL;

ALTER TABLE [SistemAyarlari] ADD [UcuncuDovizKodu] nvarchar(10) NULL;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250317124912_AddMultiCurrencySupport', N'9.0.2');


                IF EXISTS (
                    SELECT 1 
                    FROM sys.columns 
                    WHERE object_id = OBJECT_ID(N'DovizKurlari') 
                    AND name = 'ParaBirimi'
                )
                BEGIN
                    ALTER TABLE DovizKurlari DROP COLUMN ParaBirimi;
                END
            


                DECLARE @var sysname;
                SELECT @var = [d].[name]
                FROM [sys].[default_constraints] [d]
                INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
                WHERE ([d].[parent_object_id] = OBJECT_ID(N'[DovizKurlari]') AND [c].[name] = N'KaynakParaBirimi');
                IF @var IS NOT NULL EXEC(N'ALTER TABLE [DovizKurlari] DROP CONSTRAINT [' + @var + '];');
                ALTER TABLE [DovizKurlari] ADD DEFAULT N'' FOR [KaynakParaBirimi];
            


                DECLARE @var sysname;
                SELECT @var = [d].[name]
                FROM [sys].[default_constraints] [d]
                INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
                WHERE ([d].[parent_object_id] = OBJECT_ID(N'[DovizKurlari]') AND [c].[name] = N'HedefParaBirimi');
                IF @var IS NOT NULL EXEC(N'ALTER TABLE [DovizKurlari] DROP CONSTRAINT [' + @var + '];');
                ALTER TABLE [DovizKurlari] ADD DEFAULT N'' FOR [HedefParaBirimi];
            


                DECLARE @var sysname;
                SELECT @var = [d].[name]
                FROM [sys].[default_constraints] [d]
                INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
                WHERE ([d].[parent_object_id] = OBJECT_ID(N'[DovizKurlari]') AND [c].[name] = N'Aciklama');
                IF @var IS NOT NULL EXEC(N'ALTER TABLE [DovizKurlari] DROP CONSTRAINT [' + @var + '];');
                ALTER TABLE [DovizKurlari] ALTER COLUMN [Aciklama] nvarchar(500) NULL;
            

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250317134044_FixDovizKuruNullValues', N'9.0.2');

DECLARE @var24 sysname;
SELECT @var24 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[SistemAyarlari]') AND [c].[name] = N'UcuncuDovizKodu');
IF @var24 IS NOT NULL EXEC(N'ALTER TABLE [SistemAyarlari] DROP CONSTRAINT [' + @var24 + '];');
ALTER TABLE [SistemAyarlari] ADD DEFAULT N'TRY' FOR [UcuncuDovizKodu];

DECLARE @var25 sysname;
SELECT @var25 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[SistemAyarlari]') AND [c].[name] = N'SirketVergiNo');
IF @var25 IS NOT NULL EXEC(N'ALTER TABLE [SistemAyarlari] DROP CONSTRAINT [' + @var25 + '];');
ALTER TABLE [SistemAyarlari] ADD DEFAULT N'' FOR [SirketVergiNo];

DECLARE @var26 sysname;
SELECT @var26 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[SistemAyarlari]') AND [c].[name] = N'SirketVergiDairesi');
IF @var26 IS NOT NULL EXEC(N'ALTER TABLE [SistemAyarlari] DROP CONSTRAINT [' + @var26 + '];');
ALTER TABLE [SistemAyarlari] ADD DEFAULT N'' FOR [SirketVergiDairesi];

DECLARE @var27 sysname;
SELECT @var27 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[SistemAyarlari]') AND [c].[name] = N'SirketTelefon');
IF @var27 IS NOT NULL EXEC(N'ALTER TABLE [SistemAyarlari] DROP CONSTRAINT [' + @var27 + '];');
ALTER TABLE [SistemAyarlari] ADD DEFAULT N'' FOR [SirketTelefon];

DECLARE @var28 sysname;
SELECT @var28 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[SistemAyarlari]') AND [c].[name] = N'SirketEmail');
IF @var28 IS NOT NULL EXEC(N'ALTER TABLE [SistemAyarlari] DROP CONSTRAINT [' + @var28 + '];');
ALTER TABLE [SistemAyarlari] ADD DEFAULT N'' FOR [SirketEmail];

DECLARE @var29 sysname;
SELECT @var29 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[SistemAyarlari]') AND [c].[name] = N'SirketAdresi');
IF @var29 IS NOT NULL EXEC(N'ALTER TABLE [SistemAyarlari] DROP CONSTRAINT [' + @var29 + '];');
ALTER TABLE [SistemAyarlari] ADD DEFAULT N'' FOR [SirketAdresi];

DECLARE @var30 sysname;
SELECT @var30 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[SistemAyarlari]') AND [c].[name] = N'SirketAdi');
IF @var30 IS NOT NULL EXEC(N'ALTER TABLE [SistemAyarlari] DROP CONSTRAINT [' + @var30 + '];');
ALTER TABLE [SistemAyarlari] ADD DEFAULT N'Şirket' FOR [SirketAdi];

DECLARE @var31 sysname;
SELECT @var31 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[SistemAyarlari]') AND [c].[name] = N'IkinciDovizKodu');
IF @var31 IS NOT NULL EXEC(N'ALTER TABLE [SistemAyarlari] DROP CONSTRAINT [' + @var31 + '];');
ALTER TABLE [SistemAyarlari] ADD DEFAULT N'UZS' FOR [IkinciDovizKodu];

DECLARE @var32 sysname;
SELECT @var32 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[SistemAyarlari]') AND [c].[name] = N'AnaDovizKodu');
IF @var32 IS NOT NULL EXEC(N'ALTER TABLE [SistemAyarlari] DROP CONSTRAINT [' + @var32 + '];');
ALTER TABLE [SistemAyarlari] ADD DEFAULT N'USD' FOR [AnaDovizKodu];

DECLARE @var33 sysname;
SELECT @var33 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[SistemAyarlari]') AND [c].[name] = N'AktifParaBirimleri');
IF @var33 IS NOT NULL EXEC(N'ALTER TABLE [SistemAyarlari] DROP CONSTRAINT [' + @var33 + '];');
ALTER TABLE [SistemAyarlari] ADD DEFAULT N'USD,EUR,TRY,UZS,GBP' FOR [AktifParaBirimleri];

DECLARE @var34 sysname;
SELECT @var34 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[DovizKurlari]') AND [c].[name] = N'SoftDelete');
IF @var34 IS NOT NULL EXEC(N'ALTER TABLE [DovizKurlari] DROP CONSTRAINT [' + @var34 + '];');
ALTER TABLE [DovizKurlari] ADD DEFAULT CAST(0 AS bit) FOR [SoftDelete];

UPDATE DovizKurlari SET Kaynak = '' WHERE Kaynak IS NULL

DECLARE @var35 sysname;
SELECT @var35 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[DovizKurlari]') AND [c].[name] = N'Kaynak');
IF @var35 IS NOT NULL EXEC(N'ALTER TABLE [DovizKurlari] DROP CONSTRAINT [' + @var35 + '];');
UPDATE [DovizKurlari] SET [Kaynak] = N'' WHERE [Kaynak] IS NULL;
ALTER TABLE [DovizKurlari] ALTER COLUMN [Kaynak] nvarchar(100) NOT NULL;
ALTER TABLE [DovizKurlari] ADD DEFAULT N'' FOR [Kaynak];

DECLARE @var36 sysname;
SELECT @var36 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[DovizKurlari]') AND [c].[name] = N'Aktif');
IF @var36 IS NOT NULL EXEC(N'ALTER TABLE [DovizKurlari] DROP CONSTRAINT [' + @var36 + '];');
ALTER TABLE [DovizKurlari] ADD DEFAULT CAST(1 AS bit) FOR [Aktif];

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250317143045_FixNullStringValues', N'9.0.2');

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250317162937_AddDovizAdiToNotMapped', N'9.0.2');

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250317184317_InitKurDegeriRelationships', N'9.0.2');

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250317185323_FixParaBirimiTableName', N'9.0.2');

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250317185421_FixKurDegeriTableName', N'9.0.2');

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250317185533_FixKurAyarlariTableName', N'9.0.2');

ALTER TABLE [KurDegerleri] DROP CONSTRAINT [FK_KurDegerleri_ParaBirimleri_HedefParaBirimiID];

ALTER TABLE [KurDegerleri] DROP CONSTRAINT [FK_KurDegerleri_ParaBirimleri_KaynakParaBirimiID];

DROP TABLE [KurAyarlari];

ALTER TABLE [ParaBirimleri] DROP CONSTRAINT [PK_ParaBirimleri];

ALTER TABLE [KurDegerleri] DROP CONSTRAINT [PK_KurDegerleri];

EXEC sp_rename N'[ParaBirimleri]', N'Dovizler', 'OBJECT';

EXEC sp_rename N'[KurDegerleri]', N'DovizKurlari', 'OBJECT';

EXEC sp_rename N'[Dovizler].[Kod]', N'DovizKodu', 'COLUMN';

EXEC sp_rename N'[Dovizler].[Ad]', N'DovizAdi', 'COLUMN';

EXEC sp_rename N'[Dovizler].[ParaBirimiID]', N'DovizID', 'COLUMN';

EXEC sp_rename N'[DovizKurlari].[SatisDegeri]', N'SatisFiyati', 'COLUMN';

EXEC sp_rename N'[DovizKurlari].[Deger]', N'KurDegeri', 'COLUMN';

EXEC sp_rename N'[DovizKurlari].[AlisDegeri]', N'AlisFiyati', 'COLUMN';

EXEC sp_rename N'[DovizKurlari].[KurDegeriID]', N'DovizKuruID', 'COLUMN';

EXEC sp_rename N'[DovizKurlari].[IX_KurDegerleri_KaynakParaBirimiID]', N'IX_DovizKurlari_KaynakParaBirimiID', 'INDEX';

EXEC sp_rename N'[DovizKurlari].[IX_KurDegerleri_HedefParaBirimiID]', N'IX_DovizKurlari_HedefParaBirimiID', 'INDEX';

DECLARE @var37 sysname;
SELECT @var37 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Dovizler]') AND [c].[name] = N'DovizAdi');
IF @var37 IS NOT NULL EXEC(N'ALTER TABLE [Dovizler] DROP CONSTRAINT [' + @var37 + '];');
ALTER TABLE [Dovizler] ALTER COLUMN [DovizAdi] nvarchar(50) NOT NULL;

ALTER TABLE [DovizKurlari] ADD [HedefParaBirimi] nvarchar(3) NOT NULL DEFAULT N'';

ALTER TABLE [DovizKurlari] ADD [KaynakParaBirimi] nvarchar(3) NOT NULL DEFAULT N'';

ALTER TABLE [Dovizler] ADD CONSTRAINT [PK_Dovizler] PRIMARY KEY ([DovizID]);

ALTER TABLE [DovizKurlari] ADD CONSTRAINT [PK_DovizKurlari] PRIMARY KEY ([DovizKuruID]);

ALTER TABLE [DovizKurlari] ADD CONSTRAINT [FK_DovizKurlari_Dovizler_HedefParaBirimiID] FOREIGN KEY ([HedefParaBirimiID]) REFERENCES [Dovizler] ([DovizID]) ON DELETE NO ACTION;

ALTER TABLE [DovizKurlari] ADD CONSTRAINT [FK_DovizKurlari_Dovizler_KaynakParaBirimiID] FOREIGN KEY ([KaynakParaBirimiID]) REFERENCES [Dovizler] ([DovizID]) ON DELETE NO ACTION;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250317231538_DecimalPrecisionFix', N'9.0.2');

DECLARE @var38 sysname;
SELECT @var38 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[SistemLoglar]') AND [c].[name] = N'KayitID');
IF @var38 IS NOT NULL EXEC(N'ALTER TABLE [SistemLoglar] DROP CONSTRAINT [' + @var38 + '];');
ALTER TABLE [SistemLoglar] ALTER COLUMN [KayitID] int NULL;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250318040052_SistemLogKayitIDIntification', N'9.0.2');

DECLARE @var39 sysname;
SELECT @var39 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[SistemLoglar]') AND [c].[name] = N'Tarayici');
IF @var39 IS NOT NULL EXEC(N'ALTER TABLE [SistemLoglar] DROP CONSTRAINT [' + @var39 + '];');
ALTER TABLE [SistemLoglar] ALTER COLUMN [Tarayici] nvarchar(1024) NULL;

DECLARE @var40 sysname;
SELECT @var40 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[SistemLoglar]') AND [c].[name] = N'KayitID');
IF @var40 IS NOT NULL EXEC(N'ALTER TABLE [SistemLoglar] DROP CONSTRAINT [' + @var40 + '];');
ALTER TABLE [SistemLoglar] ALTER COLUMN [KayitID] uniqueidentifier NULL;

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
    [Aktif] bit NOT NULL,
    [SoftDelete] bit NOT NULL,
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

CREATE TABLE [SistemAyarlari] (
    [SistemAyarlariID] uniqueidentifier NOT NULL,
    [AnaDovizKodu] nvarchar(10) NOT NULL DEFAULT N'USD',
    [IkinciDovizKodu] nvarchar(10) NULL DEFAULT N'UZS',
    [UcuncuDovizKodu] nvarchar(10) NULL DEFAULT N'TRY',
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
    [Aktif] bit NOT NULL,
    [SoftDelete] bit NOT NULL,
    [OlusturmaTarihi] datetime2 NOT NULL,
    [GuncellemeTarihi] datetime2 NULL,
    CONSTRAINT [PK_SistemAyarlari] PRIMARY KEY ([SistemAyarlariID])
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
    [KaynakParaBirimi] nvarchar(3) NOT NULL,
    [KaynakParaBirimiID] uniqueidentifier NOT NULL,
    [HedefParaBirimi] nvarchar(3) NOT NULL,
    [HedefParaBirimiID] uniqueidentifier NOT NULL,
    [KurDegeri] decimal(18,6) NOT NULL,
    [AlisFiyati] decimal(18,6) NULL,
    [SatisFiyati] decimal(18,6) NULL,
    [Tarih] datetime2 NOT NULL,
    [Kaynak] nvarchar(100) NOT NULL,
    [Aciklama] nvarchar(500) NULL,
    [Aktif] bit NOT NULL,
    [SoftDelete] bit NOT NULL,
    [OlusturmaTarihi] datetime2 NOT NULL,
    [GuncellemeTarihi] datetime2 NULL,
    CONSTRAINT [PK_DovizKurlari] PRIMARY KEY ([DovizKuruID]),
    CONSTRAINT [FK_DovizKurlari_Dovizler_HedefParaBirimiID] FOREIGN KEY ([HedefParaBirimiID]) REFERENCES [Dovizler] ([DovizID]) ON DELETE NO ACTION,
    CONSTRAINT [FK_DovizKurlari_Dovizler_KaynakParaBirimiID] FOREIGN KEY ([KaynakParaBirimiID]) REFERENCES [Dovizler] ([DovizID]) ON DELETE NO ACTION
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
VALUES (N'20250318041919_FixDovizKurlariColumnNames', N'9.0.2');

DECLARE @var41 sysname;
SELECT @var41 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[SistemLoglar]') AND [c].[name] = N'Tarayici');
IF @var41 IS NOT NULL EXEC(N'ALTER TABLE [SistemLoglar] DROP CONSTRAINT [' + @var41 + '];');
ALTER TABLE [SistemLoglar] ALTER COLUMN [Tarayici] nvarchar(1024) NULL;

DECLARE @var42 sysname;
SELECT @var42 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[SistemLoglar]') AND [c].[name] = N'KayitID');
IF @var42 IS NOT NULL EXEC(N'ALTER TABLE [SistemLoglar] DROP CONSTRAINT [' + @var42 + '];');
ALTER TABLE [SistemLoglar] ALTER COLUMN [KayitID] uniqueidentifier NULL;

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
    [Aktif] bit NOT NULL,
    [SoftDelete] bit NOT NULL,
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

CREATE TABLE [SistemAyarlari] (
    [SistemAyarlariID] uniqueidentifier NOT NULL,
    [AnaDovizKodu] nvarchar(10) NOT NULL DEFAULT N'USD',
    [IkinciDovizKodu] nvarchar(10) NULL DEFAULT N'UZS',
    [UcuncuDovizKodu] nvarchar(10) NULL DEFAULT N'TRY',
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
    [Aktif] bit NOT NULL,
    [SoftDelete] bit NOT NULL,
    [OlusturmaTarihi] datetime2 NOT NULL,
    [GuncellemeTarihi] datetime2 NULL,
    CONSTRAINT [PK_SistemAyarlari] PRIMARY KEY ([SistemAyarlariID])
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
    [KaynakParaBirimi] nvarchar(3) NOT NULL,
    [KaynakParaBirimiID] uniqueidentifier NOT NULL,
    [HedefParaBirimi] nvarchar(3) NOT NULL,
    [HedefParaBirimiID] uniqueidentifier NOT NULL,
    [KurDegeri] decimal(18,6) NOT NULL,
    [AlisFiyati] decimal(18,6) NULL,
    [SatisFiyati] decimal(18,6) NULL,
    [Tarih] datetime2 NOT NULL,
    [Kaynak] nvarchar(100) NOT NULL,
    [Aciklama] nvarchar(500) NULL,
    [Aktif] bit NOT NULL,
    [SoftDelete] bit NOT NULL,
    [OlusturmaTarihi] datetime2 NOT NULL,
    [GuncellemeTarihi] datetime2 NULL,
    CONSTRAINT [PK_DovizKurlari] PRIMARY KEY ([DovizKuruID]),
    CONSTRAINT [FK_DovizKurlari_Dovizler_HedefParaBirimiID] FOREIGN KEY ([HedefParaBirimiID]) REFERENCES [Dovizler] ([DovizID]) ON DELETE NO ACTION,
    CONSTRAINT [FK_DovizKurlari_Dovizler_KaynakParaBirimiID] FOREIGN KEY ([KaynakParaBirimiID]) REFERENCES [Dovizler] ([DovizID]) ON DELETE NO ACTION
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
VALUES (N'20250318042611_CombineAllMigrations', N'9.0.2');


                IF EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'ParaBirimi' AND Object_ID = Object_ID(N'DovizKurlari')) 
                BEGIN
                    ALTER TABLE DovizKurlari DROP COLUMN ParaBirimi;
                END
            

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250318042907_FixParaBirimiProperty', N'9.0.2');


                IF EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'ParaBirimi' AND Object_ID = Object_ID(N'DovizKurlari')) 
                BEGIN
                    ALTER TABLE DovizKurlari DROP COLUMN ParaBirimi;
                END
            

DECLARE @var43 sysname;
SELECT @var43 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[SistemLoglar]') AND [c].[name] = N'Tarayici');
IF @var43 IS NOT NULL EXEC(N'ALTER TABLE [SistemLoglar] DROP CONSTRAINT [' + @var43 + '];');
ALTER TABLE [SistemLoglar] ALTER COLUMN [Tarayici] nvarchar(1024) NULL;

DECLARE @var44 sysname;
SELECT @var44 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[SistemLoglar]') AND [c].[name] = N'KayitID');
IF @var44 IS NOT NULL EXEC(N'ALTER TABLE [SistemLoglar] DROP CONSTRAINT [' + @var44 + '];');
ALTER TABLE [SistemLoglar] ALTER COLUMN [KayitID] uniqueidentifier NULL;

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
    [Aktif] bit NOT NULL,
    [SoftDelete] bit NOT NULL,
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

CREATE TABLE [SistemAyarlari] (
    [SistemAyarlariID] uniqueidentifier NOT NULL,
    [AnaDovizKodu] nvarchar(10) NOT NULL DEFAULT N'USD',
    [IkinciDovizKodu] nvarchar(10) NULL DEFAULT N'UZS',
    [UcuncuDovizKodu] nvarchar(10) NULL DEFAULT N'TRY',
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
    [Aktif] bit NOT NULL,
    [SoftDelete] bit NOT NULL,
    [OlusturmaTarihi] datetime2 NOT NULL,
    [GuncellemeTarihi] datetime2 NULL,
    CONSTRAINT [PK_SistemAyarlari] PRIMARY KEY ([SistemAyarlariID])
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
    [KaynakParaBirimi] nvarchar(3) NOT NULL,
    [KaynakParaBirimiID] uniqueidentifier NOT NULL,
    [HedefParaBirimi] nvarchar(3) NOT NULL,
    [HedefParaBirimiID] uniqueidentifier NOT NULL,
    [KurDegeri] decimal(18,6) NOT NULL,
    [AlisFiyati] decimal(18,6) NULL,
    [SatisFiyati] decimal(18,6) NULL,
    [Tarih] datetime2 NOT NULL,
    [Kaynak] nvarchar(100) NOT NULL,
    [Aciklama] nvarchar(500) NULL,
    [Aktif] bit NOT NULL,
    [SoftDelete] bit NOT NULL,
    [OlusturmaTarihi] datetime2 NOT NULL,
    [GuncellemeTarihi] datetime2 NULL,
    CONSTRAINT [PK_DovizKurlari] PRIMARY KEY ([DovizKuruID]),
    CONSTRAINT [FK_DovizKurlari_Dovizler_HedefParaBirimiID] FOREIGN KEY ([HedefParaBirimiID]) REFERENCES [Dovizler] ([DovizID]) ON DELETE NO ACTION,
    CONSTRAINT [FK_DovizKurlari_Dovizler_KaynakParaBirimiID] FOREIGN KEY ([KaynakParaBirimiID]) REFERENCES [Dovizler] ([DovizID]) ON DELETE NO ACTION
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
VALUES (N'20250318043156_RemoveParaBirimiColumn', N'9.0.2');

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250318043334_InitialMigration', N'9.0.2');


                IF EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'ParaBirimi' AND Object_ID = Object_ID(N'DovizKurlari')) 
                BEGIN
                    ALTER TABLE DovizKurlari DROP COLUMN ParaBirimi;
                END
            

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250318043412_RemoveParaBirimiFromDovizKurlari', N'9.0.2');


                IF COL_LENGTH('DovizKurlari', 'ParaBirimi') IS NOT NULL
                BEGIN
                    ALTER TABLE DovizKurlari DROP COLUMN ParaBirimi;
                END
            

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250318043854_FixDovizKurlariEntity', N'9.0.2');

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250318044150_RemoveParaBirimiFromEntity', N'9.0.2');

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250318044308_FixDovizKurlariParaBirimi', N'9.0.2');

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250318044453_FixParaBirimiColumn', N'9.0.2');


                IF COL_LENGTH('DovizKurlari', 'ParaBirimi') IS NOT NULL
                BEGIN
                    ALTER TABLE DovizKurlari DROP COLUMN ParaBirimi;
                END
            

DECLARE @var45 sysname;
SELECT @var45 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[SistemLoglar]') AND [c].[name] = N'Tarayici');
IF @var45 IS NOT NULL EXEC(N'ALTER TABLE [SistemLoglar] DROP CONSTRAINT [' + @var45 + '];');
ALTER TABLE [SistemLoglar] ALTER COLUMN [Tarayici] nvarchar(1024) NULL;

DECLARE @var46 sysname;
SELECT @var46 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[SistemLoglar]') AND [c].[name] = N'KayitID');
IF @var46 IS NOT NULL EXEC(N'ALTER TABLE [SistemLoglar] DROP CONSTRAINT [' + @var46 + '];');
ALTER TABLE [SistemLoglar] ALTER COLUMN [KayitID] uniqueidentifier NULL;

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
    [Aktif] bit NOT NULL,
    [SoftDelete] bit NOT NULL,
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

CREATE TABLE [SistemAyarlari] (
    [SistemAyarlariID] uniqueidentifier NOT NULL,
    [AnaDovizKodu] nvarchar(10) NOT NULL DEFAULT N'USD',
    [IkinciDovizKodu] nvarchar(10) NULL DEFAULT N'UZS',
    [UcuncuDovizKodu] nvarchar(10) NULL DEFAULT N'TRY',
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
    [Aktif] bit NOT NULL,
    [SoftDelete] bit NOT NULL,
    [OlusturmaTarihi] datetime2 NOT NULL,
    [GuncellemeTarihi] datetime2 NULL,
    CONSTRAINT [PK_SistemAyarlari] PRIMARY KEY ([SistemAyarlariID])
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
    [KaynakParaBirimi] nvarchar(3) NOT NULL,
    [KaynakParaBirimiID] uniqueidentifier NOT NULL,
    [HedefParaBirimi] nvarchar(3) NOT NULL,
    [HedefParaBirimiID] uniqueidentifier NOT NULL,
    [KurDegeri] decimal(18,6) NOT NULL,
    [AlisFiyati] decimal(18,6) NULL,
    [SatisFiyati] decimal(18,6) NULL,
    [Tarih] datetime2 NOT NULL,
    [Kaynak] nvarchar(100) NOT NULL,
    [Aciklama] nvarchar(500) NULL,
    [Aktif] bit NOT NULL,
    [SoftDelete] bit NOT NULL,
    [OlusturmaTarihi] datetime2 NOT NULL,
    [GuncellemeTarihi] datetime2 NULL,
    CONSTRAINT [PK_DovizKurlari] PRIMARY KEY ([DovizKuruID]),
    CONSTRAINT [FK_DovizKurlari_Dovizler_HedefParaBirimiID] FOREIGN KEY ([HedefParaBirimiID]) REFERENCES [Dovizler] ([DovizID]) ON DELETE NO ACTION,
    CONSTRAINT [FK_DovizKurlari_Dovizler_KaynakParaBirimiID] FOREIGN KEY ([KaynakParaBirimiID]) REFERENCES [Dovizler] ([DovizID]) ON DELETE NO ACTION
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
VALUES (N'20250318044632_RemoveParaBirimiFromKurDegeri', N'9.0.2');


                IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'DovizKurlari') AND name = 'ParaBirimi')
                BEGIN
                    ALTER TABLE DovizKurlari DROP COLUMN ParaBirimi;
                END
            

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250318044800_FixParaBirimiFromKurDegeri', N'9.0.2');


                IF EXISTS (
                    SELECT 1 
                    FROM sys.columns 
                    WHERE object_id = OBJECT_ID(N'DovizKurlari') 
                    AND name = 'ParaBirimi'
                )
                BEGIN
                    ALTER TABLE DovizKurlari DROP COLUMN ParaBirimi;
                END
            

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250318044913_DirectRemoveParaBirimi', N'9.0.2');


                IF EXISTS (
                    SELECT 1 
                    FROM sys.columns 
                    WHERE object_id = OBJECT_ID(N'DovizKurlari') 
                    AND name = 'ParaBirimi'
                )
                BEGIN
                    ALTER TABLE DovizKurlari DROP COLUMN ParaBirimi;
                END
            

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250318045126_RemoveExtraParaBirimiFromKurDegeri', N'9.0.2');


                IF EXISTS (
                    SELECT 1 
                    FROM sys.columns 
                    WHERE object_id = OBJECT_ID(N'DovizKurlari') 
                    AND name = 'ParaBirimi'
                )
                BEGIN
                    ALTER TABLE DovizKurlari DROP COLUMN ParaBirimi;
                END
            

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250318045412_ManualDropParaBirimiColumn', N'9.0.2');


                IF EXISTS (
                    SELECT 1 
                    FROM sys.columns 
                    WHERE object_id = OBJECT_ID(N'DovizKurlari') 
                    AND name = 'ParaBirimi'
                )
                BEGIN
                    ALTER TABLE DovizKurlari DROP COLUMN ParaBirimi;
                END
            


                IF NOT EXISTS (
                    SELECT 1 
                    FROM sys.columns 
                    WHERE object_id = OBJECT_ID(N'DovizKurlari') 
                    AND name = 'Kaynak'
                )
                BEGIN
                    ALTER TABLE DovizKurlari ADD Kaynak nvarchar(100) NOT NULL DEFAULT 'Manuel';
                END
            

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250318045517_FixKaynak', N'9.0.2');


                IF EXISTS (
                    SELECT 1 
                    FROM sys.columns 
                    WHERE object_id = OBJECT_ID(N'DovizKurlari') 
                    AND name = 'ParaBirimi'
                )
                BEGIN
                    ALTER TABLE DovizKurlari DROP COLUMN ParaBirimi;
                END
            


                IF NOT EXISTS (
                    SELECT 1 
                    FROM sys.columns 
                    WHERE object_id = OBJECT_ID(N'DovizKurlari') 
                    AND name = 'Kaynak'
                )
                BEGIN
                    ALTER TABLE DovizKurlari ADD Kaynak nvarchar(100) NOT NULL DEFAULT 'Manuel';
                END
                ELSE
                BEGIN
                    -- Eğer Kaynak sütunu varsa, varsayılan değerini güncelle
                    DECLARE @ConstraintName nvarchar(200)
                    SELECT @ConstraintName = name
                    FROM sys.default_constraints
                    WHERE parent_object_id = OBJECT_ID(N'DovizKurlari')
                    AND parent_column_id = (
                        SELECT column_id
                        FROM sys.columns
                        WHERE object_id = OBJECT_ID(N'DovizKurlari')
                        AND name = 'Kaynak'
                    )
                    
                    IF @ConstraintName IS NOT NULL
                    BEGIN
                        EXEC('ALTER TABLE DovizKurlari DROP CONSTRAINT ' + @ConstraintName)
                    END
                    
                    ALTER TABLE DovizKurlari ALTER COLUMN Kaynak nvarchar(100) NOT NULL;
                    ALTER TABLE DovizKurlari ADD CONSTRAINT DF_DovizKurlari_Kaynak DEFAULT 'Manuel' FOR Kaynak;
                END
            

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250318045700_DirectSQLDropParaBirimi', N'9.0.2');


                IF EXISTS (
                    SELECT 1 
                    FROM sys.columns 
                    WHERE object_id = OBJECT_ID(N'DovizKurlari') 
                    AND name = 'ParaBirimi'
                )
                BEGIN
                    ALTER TABLE DovizKurlari DROP COLUMN ParaBirimi;
                END
            


                IF EXISTS (
                    SELECT 1 
                    FROM sys.columns 
                    WHERE object_id = OBJECT_ID(N'DovizKurlari') 
                    AND name = 'Kaynak'
                )
                BEGIN
                    -- Kaynak sütununu NULL olarak güncelle
                    DECLARE @constraint_name NVARCHAR(128)
                    SELECT @constraint_name = name 
                    FROM sys.default_constraints 
                    WHERE parent_object_id = OBJECT_ID('DovizKurlari') 
                    AND parent_column_id = (
                        SELECT column_id 
                        FROM sys.columns 
                        WHERE object_id = OBJECT_ID('DovizKurlari') 
                        AND name = 'Kaynak'
                    )

                    IF @constraint_name IS NOT NULL
                    BEGIN
                        EXEC('ALTER TABLE DovizKurlari DROP CONSTRAINT ' + @constraint_name)
                    END

                    -- Sütunu NULL olarak değiştir
                    EXEC('ALTER TABLE DovizKurlari ALTER COLUMN Kaynak NVARCHAR(100) NULL')
                END
                ELSE
                BEGIN
                    -- Sütun yoksa NULL olarak ekle
                    EXEC('ALTER TABLE DovizKurlari ADD Kaynak NVARCHAR(100) NULL')
                END
            


                UPDATE DovizKurlari SET Kaynak = 'Manuel' WHERE Kaynak IS NULL
            

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250318050024_FixKaynakColumnRequired', N'9.0.2');


                -- Önce herhangi bir default constraint'i kaldır
                DECLARE @constraint_name NVARCHAR(128)
                SELECT @constraint_name = name 
                FROM sys.default_constraints 
                WHERE parent_object_id = OBJECT_ID('DovizKurlari') 
                AND parent_column_id = (
                    SELECT column_id 
                    FROM sys.columns 
                    WHERE object_id = OBJECT_ID('DovizKurlari') 
                    AND name = 'Kaynak'
                )

                IF @constraint_name IS NOT NULL
                BEGIN
                    EXEC('ALTER TABLE DovizKurlari DROP CONSTRAINT ' + @constraint_name)
                END

                -- Sütunu NULL olarak değiştir
                IF EXISTS (
                    SELECT 1 
                    FROM sys.columns 
                    WHERE object_id = OBJECT_ID(N'DovizKurlari') 
                    AND name = 'Kaynak'
                    AND is_nullable = 0  -- Şu anda NULL değil
                )
                BEGIN
                    ALTER TABLE DovizKurlari ALTER COLUMN Kaynak NVARCHAR(100) NULL
                END

                -- NULL değerleri 'Manuel' ile güncelle
                UPDATE DovizKurlari SET Kaynak = 'Manuel' WHERE Kaynak IS NULL
            

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250318050424_FixKaynakColumnRequired2', N'9.0.2');


                IF EXISTS (
                    SELECT 1 
                    FROM sys.columns 
                    WHERE object_id = OBJECT_ID(N'DovizKurlari') 
                    AND name = 'Kaynak'
                )
                BEGIN
                    -- Varsa constraint'i kaldır
                    DECLARE @constraint_name NVARCHAR(128)
                    SELECT @constraint_name = name 
                    FROM sys.default_constraints 
                    WHERE parent_object_id = OBJECT_ID('DovizKurlari') 
                    AND parent_column_id = (
                        SELECT column_id 
                        FROM sys.columns 
                        WHERE object_id = OBJECT_ID('DovizKurlari') 
                        AND name = 'Kaynak'
                    )

                    IF @constraint_name IS NOT NULL
                    BEGIN
                        EXEC('ALTER TABLE DovizKurlari DROP CONSTRAINT ' + @constraint_name)
                    END

                    -- Sütunu NULL olarak değiştir
                    ALTER TABLE DovizKurlari ALTER COLUMN Kaynak NVARCHAR(100) NULL
                END
            

UPDATE DovizKurlari SET Kaynak = 'Manuel' WHERE Kaynak IS NULL

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250318050651_FixKaynakColumnSchema', N'9.0.2');


                IF EXISTS (
                    SELECT 1 
                    FROM sys.columns 
                    WHERE object_id = OBJECT_ID(N'DovizKurlari') 
                    AND name = 'Kaynak'
                )
                BEGIN
                    -- Varsa constraint'i kaldır
                    DECLARE @constraint_name NVARCHAR(128)
                    SELECT @constraint_name = name 
                    FROM sys.default_constraints 
                    WHERE parent_object_id = OBJECT_ID('DovizKurlari') 
                    AND parent_column_id = (
                        SELECT column_id 
                        FROM sys.columns 
                        WHERE object_id = OBJECT_ID('DovizKurlari') 
                        AND name = 'Kaynak'
                    )

                    IF @constraint_name IS NOT NULL
                    BEGIN
                        EXEC('ALTER TABLE DovizKurlari DROP CONSTRAINT ' + @constraint_name)
                    END

                    -- Sütunu NULL olarak değiştir
                    ALTER TABLE DovizKurlari ALTER COLUMN Kaynak NVARCHAR(100) NULL
                END
            

UPDATE DovizKurlari SET Kaynak = 'Manuel' WHERE Kaynak IS NULL

DECLARE @var47 sysname;
SELECT @var47 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[SistemLoglar]') AND [c].[name] = N'Tarayici');
IF @var47 IS NOT NULL EXEC(N'ALTER TABLE [SistemLoglar] DROP CONSTRAINT [' + @var47 + '];');
ALTER TABLE [SistemLoglar] ALTER COLUMN [Tarayici] nvarchar(1024) NULL;

DECLARE @var48 sysname;
SELECT @var48 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[SistemLoglar]') AND [c].[name] = N'KayitID');
IF @var48 IS NOT NULL EXEC(N'ALTER TABLE [SistemLoglar] DROP CONSTRAINT [' + @var48 + '];');
ALTER TABLE [SistemLoglar] ALTER COLUMN [KayitID] uniqueidentifier NULL;

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
    [Aktif] bit NOT NULL,
    [SoftDelete] bit NOT NULL,
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

CREATE TABLE [SistemAyarlari] (
    [SistemAyarlariID] uniqueidentifier NOT NULL,
    [AnaDovizKodu] nvarchar(10) NOT NULL DEFAULT N'USD',
    [IkinciDovizKodu] nvarchar(10) NULL DEFAULT N'UZS',
    [UcuncuDovizKodu] nvarchar(10) NULL DEFAULT N'TRY',
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
    [Aktif] bit NOT NULL,
    [SoftDelete] bit NOT NULL,
    [OlusturmaTarihi] datetime2 NOT NULL,
    [GuncellemeTarihi] datetime2 NULL,
    CONSTRAINT [PK_SistemAyarlari] PRIMARY KEY ([SistemAyarlariID])
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
    [KaynakParaBirimi] nvarchar(3) NOT NULL,
    [KaynakParaBirimiID] uniqueidentifier NOT NULL,
    [HedefParaBirimi] nvarchar(3) NOT NULL,
    [HedefParaBirimiID] uniqueidentifier NOT NULL,
    [KurDegeri] decimal(18,6) NOT NULL,
    [AlisFiyati] decimal(18,6) NULL,
    [SatisFiyati] decimal(18,6) NULL,
    [Tarih] datetime2 NOT NULL,
    [Kaynak] nvarchar(100) NOT NULL,
    [Aciklama] nvarchar(500) NULL,
    [Aktif] bit NOT NULL,
    [SoftDelete] bit NOT NULL,
    [OlusturmaTarihi] datetime2 NOT NULL,
    [GuncellemeTarihi] datetime2 NULL,
    CONSTRAINT [PK_DovizKurlari] PRIMARY KEY ([DovizKuruID]),
    CONSTRAINT [FK_DovizKurlari_Dovizler_HedefParaBirimiID] FOREIGN KEY ([HedefParaBirimiID]) REFERENCES [Dovizler] ([DovizID]) ON DELETE NO ACTION,
    CONSTRAINT [FK_DovizKurlari_Dovizler_KaynakParaBirimiID] FOREIGN KEY ([KaynakParaBirimiID]) REFERENCES [Dovizler] ([DovizID]) ON DELETE NO ACTION
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
VALUES (N'20250318050832_FixKaynakColumnManual', N'9.0.2');


                IF EXISTS (
                    SELECT 1 
                    FROM sys.columns 
                    WHERE object_id = OBJECT_ID(N'DovizKurlari') 
                    AND name = 'Kaynak'
                )
                BEGIN
                    -- Varsa constraint'i kaldır
                    DECLARE @constraint_name NVARCHAR(128)
                    SELECT @constraint_name = name 
                    FROM sys.default_constraints 
                    WHERE parent_object_id = OBJECT_ID('DovizKurlari') 
                    AND parent_column_id = (
                        SELECT column_id 
                        FROM sys.columns 
                        WHERE object_id = OBJECT_ID('DovizKurlari') 
                        AND name = 'Kaynak'
                    )

                    IF @constraint_name IS NOT NULL
                    BEGIN
                        EXEC('ALTER TABLE DovizKurlari DROP CONSTRAINT ' + @constraint_name)
                    END

                    -- Sütunu NULL olarak değiştir
                    ALTER TABLE DovizKurlari ALTER COLUMN Kaynak NVARCHAR(100) NULL
                END
            

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250318051040_MakeKaynakNullable', N'9.0.2');

DECLARE @var49 sysname;
SELECT @var49 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[SistemLoglar]') AND [c].[name] = N'Tarayici');
IF @var49 IS NOT NULL EXEC(N'ALTER TABLE [SistemLoglar] DROP CONSTRAINT [' + @var49 + '];');
ALTER TABLE [SistemLoglar] ALTER COLUMN [Tarayici] nvarchar(1024) NULL;

DECLARE @var50 sysname;
SELECT @var50 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[SistemLoglar]') AND [c].[name] = N'KayitID');
IF @var50 IS NOT NULL EXEC(N'ALTER TABLE [SistemLoglar] DROP CONSTRAINT [' + @var50 + '];');
ALTER TABLE [SistemLoglar] ALTER COLUMN [KayitID] uniqueidentifier NULL;

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
    [Aktif] bit NOT NULL,
    [SoftDelete] bit NOT NULL,
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

CREATE TABLE [SistemAyarlari] (
    [SistemAyarlariID] uniqueidentifier NOT NULL,
    [AnaDovizKodu] nvarchar(10) NOT NULL DEFAULT N'USD',
    [IkinciDovizKodu] nvarchar(10) NULL DEFAULT N'UZS',
    [UcuncuDovizKodu] nvarchar(10) NULL DEFAULT N'TRY',
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
    [Aktif] bit NOT NULL,
    [SoftDelete] bit NOT NULL,
    [OlusturmaTarihi] datetime2 NOT NULL,
    [GuncellemeTarihi] datetime2 NULL,
    CONSTRAINT [PK_SistemAyarlari] PRIMARY KEY ([SistemAyarlariID])
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
    [KaynakParaBirimi] nvarchar(3) NOT NULL,
    [KaynakParaBirimiID] uniqueidentifier NOT NULL,
    [HedefParaBirimi] nvarchar(3) NOT NULL,
    [HedefParaBirimiID] uniqueidentifier NOT NULL,
    [KurDegeri] decimal(18,6) NOT NULL,
    [AlisFiyati] decimal(18,6) NULL,
    [SatisFiyati] decimal(18,6) NULL,
    [Tarih] datetime2 NOT NULL,
    [Kaynak] nvarchar(100) NULL,
    [Aciklama] nvarchar(500) NULL,
    [Aktif] bit NOT NULL,
    [SoftDelete] bit NOT NULL,
    [OlusturmaTarihi] datetime2 NOT NULL,
    [GuncellemeTarihi] datetime2 NULL,
    CONSTRAINT [PK_DovizKurlari] PRIMARY KEY ([DovizKuruID]),
    CONSTRAINT [FK_DovizKurlari_Dovizler_HedefParaBirimiID] FOREIGN KEY ([HedefParaBirimiID]) REFERENCES [Dovizler] ([DovizID]) ON DELETE NO ACTION,
    CONSTRAINT [FK_DovizKurlari_Dovizler_KaynakParaBirimiID] FOREIGN KEY ([KaynakParaBirimiID]) REFERENCES [Dovizler] ([DovizID]) ON DELETE NO ACTION
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
VALUES (N'20250318051239_UpdateDovizKuruKaynakToNullable', N'9.0.2');

COMMIT;
GO


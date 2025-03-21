-- =============================================
-- YENİ OPTİMİZE EDİLMİŞ MIGRATION SCRIPT
-- =============================================
-- Bu script, #TempSistemAyarlari geçici tablo sorununu çözmek için optimize edilmiştir.
-- =============================================

USE MuhasebeStokWebDB;
GO

-- Önce tempdb içindeki geçici tabloları temizle
IF OBJECT_ID('tempdb..#TempSistemAyarlari') IS NOT NULL 
BEGIN
    DROP TABLE #TempSistemAyarlari;
    PRINT 'Varolan #TempSistemAyarlari tablosu silindi.';
END
GO

IF OBJECT_ID('tempdb..#TempSistemAyarlari2') IS NOT NULL
BEGIN
    DROP TABLE #TempSistemAyarlari2;
    PRINT 'Varolan #TempSistemAyarlari2 tablosu silindi.';
END
GO

-- __EFMigrationsHistory tablosunu kontrol et ve oluştur
IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
    PRINT '__EFMigrationsHistory tablosu oluşturuldu.';
END;
GO

-- Tüm migration işlemlerini tek bir transaction içinde yap
BEGIN TRANSACTION;
GO

-- ========== TABLOLARI OLUŞTURMA BÖLÜMÜ ==========

-- AspNetRoles
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AspNetRoles')
BEGIN
    CREATE TABLE [AspNetRoles] (
        [Id] nvarchar(450) NOT NULL,
        [Name] nvarchar(256) NULL,
        [NormalizedName] nvarchar(256) NULL,
        [ConcurrencyStamp] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
    );
    PRINT 'AspNetRoles tablosu oluşturuldu.';
END
GO

-- AspNetUsers
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AspNetUsers')
BEGIN
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
    PRINT 'AspNetUsers tablosu oluşturuldu.';
END
GO

-- Bankalar
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Bankalar')
BEGIN
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
    PRINT 'Bankalar tablosu oluşturuldu.';
END
GO

-- Birimler
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Birimler')
BEGIN
    CREATE TABLE [Birimler] (
        [BirimID] uniqueidentifier NOT NULL,
        [BirimAdi] nvarchar(50) NOT NULL,
        [Aciklama] nvarchar(200) NOT NULL,
        [Aktif] bit NOT NULL,
        [SoftDelete] bit NOT NULL,
        CONSTRAINT [PK_Birimler] PRIMARY KEY ([BirimID])
    );
    PRINT 'Birimler tablosu oluşturuldu.';
END
GO

-- Cariler
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Cariler')
BEGIN
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
    PRINT 'Cariler tablosu oluşturuldu.';
END
GO

-- Depolar
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Depolar')
BEGIN
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
    PRINT 'Depolar tablosu oluşturuldu.';
END
GO

-- Dovizler
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Dovizler')
BEGIN
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
    PRINT 'Dovizler tablosu oluşturuldu.';
END
GO

-- ParaBirimi
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ParaBirimi')
BEGIN
    CREATE TABLE [ParaBirimi] (
        [ParaBirimiID] uniqueidentifier NOT NULL,
        [DovizKodu] nvarchar(5) NOT NULL,
        [DovizAdi] nvarchar(50) NOT NULL,
        [Sembol] nvarchar(10) NOT NULL,
        [Aktif] bit NOT NULL DEFAULT CAST(1 AS bit),
        [OlusturmaTarihi] datetime2 NOT NULL,
        [GuncellemeTarihi] datetime2 NOT NULL,
        [Silindi] bit NOT NULL DEFAULT CAST(0 AS bit),
        [Sira] int NOT NULL DEFAULT 0,
        [Format] nvarchar(50) NOT NULL DEFAULT N'#,##0.00',
        [Aciklama] nvarchar(250) NOT NULL DEFAULT N'',
        CONSTRAINT [PK_ParaBirimi] PRIMARY KEY ([ParaBirimiID])
    );
    PRINT 'ParaBirimi tablosu oluşturuldu.';
END
GO

-- ParaBirimiIliski
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ParaBirimiIliski')
BEGIN
    CREATE TABLE [ParaBirimiIliski] (
        [ParaBirimiIliskiID] uniqueidentifier NOT NULL,
        [KaynakParaBirimiID] uniqueidentifier NOT NULL,
        [HedefParaBirimiID] uniqueidentifier NOT NULL,
        [Carpan] decimal(18,6) NOT NULL DEFAULT 1.0,
        [Aktif] bit NOT NULL DEFAULT CAST(1 AS bit),
        [OlusturmaTarihi] datetime2 NOT NULL,
        [GuncellemeTarihi] datetime2 NOT NULL,
        [Silindi] bit NOT NULL DEFAULT CAST(0 AS bit),
        CONSTRAINT [PK_ParaBirimiIliski] PRIMARY KEY ([ParaBirimiIliskiID]),
        CONSTRAINT [FK_ParaBirimiIliski_ParaBirimi_KaynakParaBirimiID] 
            FOREIGN KEY ([KaynakParaBirimiID]) REFERENCES [ParaBirimi] ([ParaBirimiID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ParaBirimiIliski_ParaBirimi_HedefParaBirimiID] 
            FOREIGN KEY ([HedefParaBirimiID]) REFERENCES [ParaBirimi] ([ParaBirimiID]) ON DELETE NO ACTION
    );
    PRINT 'ParaBirimiIliski tablosu oluşturuldu.';
END
GO

-- FaturaTurleri
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'FaturaTurleri')
BEGIN
    CREATE TABLE [FaturaTurleri] (
        [FaturaTuruID] int NOT NULL IDENTITY,
        [FaturaTuruAdi] nvarchar(50) NOT NULL,
        [HareketTuru] nvarchar(50) NOT NULL,
        CONSTRAINT [PK_FaturaTurleri] PRIMARY KEY ([FaturaTuruID])
    );
    PRINT 'FaturaTurleri tablosu oluşturuldu.';
END
GO

-- FiyatTipleri
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'FiyatTipleri')
BEGIN
    CREATE TABLE [FiyatTipleri] (
        [FiyatTipiID] int NOT NULL IDENTITY,
        [TipAdi] nvarchar(50) NOT NULL,
        CONSTRAINT [PK_FiyatTipleri] PRIMARY KEY ([FiyatTipiID])
    );
    PRINT 'FiyatTipleri tablosu oluşturuldu.';
END
GO

-- IrsaliyeTurleri
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'IrsaliyeTurleri')
BEGIN
    CREATE TABLE [IrsaliyeTurleri] (
        [IrsaliyeTuruID] int NOT NULL IDENTITY,
        [IrsaliyeTuruAdi] nvarchar(50) NOT NULL,
        [HareketTuru] nvarchar(50) NOT NULL,
        CONSTRAINT [PK_IrsaliyeTurleri] PRIMARY KEY ([IrsaliyeTuruID])
    );
    PRINT 'IrsaliyeTurleri tablosu oluşturuldu.';
END
GO

-- Kasalar
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Kasalar')
BEGIN
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
    PRINT 'Kasalar tablosu oluşturuldu.';
END
GO

-- OdemeTurleri
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'OdemeTurleri')
BEGIN
    CREATE TABLE [OdemeTurleri] (
        [OdemeTuruID] int NOT NULL IDENTITY,
        [OdemeTuruAdi] nvarchar(50) NOT NULL,
        CONSTRAINT [PK_OdemeTurleri] PRIMARY KEY ([OdemeTuruID])
    );
    PRINT 'OdemeTurleri tablosu oluşturuldu.';
END
GO

-- SistemLoglar
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'SistemLoglar')
BEGIN
    CREATE TABLE [SistemLoglar] (
        [LogID] uniqueidentifier NOT NULL,
        [IslemTuru] nvarchar(50) NOT NULL,
        [LogTuru] nvarchar(50) NOT NULL DEFAULT 'Bilgi',
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
    PRINT 'SistemLoglar tablosu oluşturuldu.';
END
GO

-- UrunKategorileri
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'UrunKategorileri')
BEGIN
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
    PRINT 'UrunKategorileri tablosu oluşturuldu.';
END
GO

-- SistemAyarlari
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'SistemAyarlari')
BEGIN
    -- Doğrudan ve güvenli bir şekilde SistemAyarlari tablosunu oluştur
    -- Geçici tablo kullanmadan
    CREATE TABLE [SistemAyarlari] (
        [SistemAyarlariID] int IDENTITY(1,1) NOT NULL,
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
        [OtomatikDovizGuncelleme] bit NOT NULL DEFAULT CAST(0 AS bit),
        [DovizGuncellemeSikligi] int NOT NULL DEFAULT 24,
        [SonDovizGuncellemeTarihi] datetime2 NULL,
        [AktifParaBirimleri] nvarchar(500) NULL DEFAULT N'USD,EUR,TRY,UZS,GBP',
        [Aktif] bit NOT NULL DEFAULT CAST(1 AS bit),
        [SoftDelete] bit NOT NULL DEFAULT CAST(0 AS bit),
        [OlusturmaTarihi] datetime2 NOT NULL DEFAULT GETDATE(),
        [GuncellemeTarihi] datetime2 NULL,
        CONSTRAINT [PK_SistemAyarlari] PRIMARY KEY ([SistemAyarlariID])
    );
    PRINT 'SistemAyarlari tablosu oluşturuldu - Geçici tablo kullanılmadan.';
END
GO

-- --- Diğer tablo oluşturma işlemleri ---

-- Urunler
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Urunler')
BEGIN
    CREATE TABLE [Urunler] (
        [UrunID] uniqueidentifier NOT NULL,
        [UrunKodu] nvarchar(50) NOT NULL,
        [UrunAdi] nvarchar(200) NOT NULL,
        [BirimID] uniqueidentifier NOT NULL,
        [KategoriID] uniqueidentifier NULL,
        [Barkod] nvarchar(50) NULL,
        [KDVOrani] int NOT NULL,
        [Aktif] bit NOT NULL DEFAULT CAST(1 AS bit),
        [OlusturanKullaniciID] uniqueidentifier NULL,
        [SonGuncelleyenKullaniciID] uniqueidentifier NULL,
        [OlusturmaTarihi] datetime2 NULL,
        [GuncellemeTarihi] datetime2 NULL,
        [SoftDelete] bit NOT NULL DEFAULT CAST(0 AS bit),
        CONSTRAINT [PK_Urunler] PRIMARY KEY ([UrunID]),
        CONSTRAINT [FK_Urunler_Birimler_BirimID] FOREIGN KEY ([BirimID]) REFERENCES [Birimler] ([BirimID]),
        CONSTRAINT [FK_Urunler_UrunKategorileri_KategoriID] FOREIGN KEY ([KategoriID]) REFERENCES [UrunKategorileri] ([KategoriID])
    );
    PRINT 'Urunler tablosu oluşturuldu.';
END
GO

-- -- Foreign Key bağlantılarını ekleyelim --

-- SistemAyarlari için Foreign Key ekle (eğer Dovizler tablosu varsa)
IF OBJECT_ID('Dovizler', 'U') IS NOT NULL AND OBJECT_ID('SistemAyarlari', 'U') IS NOT NULL
BEGIN
    -- AnaParaBirimiID için foreign key
    IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_SistemAyarlari_Dovizler_AnaParaBirimiID')
    BEGIN
        ALTER TABLE [SistemAyarlari] ADD CONSTRAINT [FK_SistemAyarlari_Dovizler_AnaParaBirimiID] 
        FOREIGN KEY ([AnaParaBirimiID]) REFERENCES [Dovizler] ([DovizID]);
    END

    -- IkinciParaBirimiID için foreign key
    IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_SistemAyarlari_Dovizler_IkinciParaBirimiID')
    BEGIN
        ALTER TABLE [SistemAyarlari] ADD CONSTRAINT [FK_SistemAyarlari_Dovizler_IkinciParaBirimiID] 
        FOREIGN KEY ([IkinciParaBirimiID]) REFERENCES [Dovizler] ([DovizID]);
    END

    -- UcuncuParaBirimiID için foreign key
    IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_SistemAyarlari_Dovizler_UcuncuParaBirimiID')
    BEGIN
        ALTER TABLE [SistemAyarlari] ADD CONSTRAINT [FK_SistemAyarlari_Dovizler_UcuncuParaBirimiID] 
        FOREIGN KEY ([UcuncuParaBirimiID]) REFERENCES [Dovizler] ([DovizID]);
    END
    
    PRINT 'SistemAyarlari Foreign Key bağlantıları eklendi.';
END
GO

-- Migration bilgilerini ekleyelim
-- NOT: Migration geçmişiniz bu tabloda kayıtlıysa, aynı migration ID'lerini tekrar eklemeyin
INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250311052830_AddTestData', N'8.0.3');

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250318131105_InitialCreate', N'8.0.3');

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250319182351_RepairDatabase', N'8.0.3');

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250320062356_Reinitialize', N'8.0.3');

-- Transactionı commit et
COMMIT TRANSACTION;
GO

PRINT 'Migration başarıyla tamamlandı. Temel tablolar oluşturuldu ve Foreign Key bağlantıları yapıldı.';
PRINT '#TempSistemAyarlari geçici tablosu kullanılmadı, tüm işlemler doğrudan tablolar üzerinde yapıldı.';
GO 
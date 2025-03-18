-- Önce mevcut tabloları temizleyelim (Foreign Key kısıtlamaları nedeniyle önce DovizKurlari tablosunu silmeliyiz)
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DovizKurlari]') AND type in (N'U'))
BEGIN
    DROP TABLE [DovizKurlari];
    PRINT 'DovizKurlari tablosu silindi.';
END

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Dovizler]') AND type in (N'U'))
BEGIN
    DROP TABLE [Dovizler];
    PRINT 'Dovizler tablosu silindi.';
END

-- ADIM 1: Dovizler tablosunu oluştur
CREATE TABLE [Dovizler] (
    [DovizID] uniqueidentifier NOT NULL,
    [DovizKodu] nvarchar(10) NOT NULL,
    [DovizAdi] nvarchar(50) NOT NULL,
    [Sembol] nvarchar(10) NOT NULL,
    [Aktif] bit NOT NULL DEFAULT 1,
    [SoftDelete] bit NOT NULL DEFAULT 0,
    [OlusturmaTarihi] datetime2 NOT NULL DEFAULT GETDATE(),
    [GuncellemeTarihi] datetime2 NULL,
    CONSTRAINT [PK_Dovizler] PRIMARY KEY ([DovizID])
);
PRINT 'Dovizler tablosu oluşturuldu.';

-- ADIM 2: Temel döviz kayıtlarını ekle
INSERT INTO [Dovizler] ([DovizID], [DovizKodu], [DovizAdi], [Sembol], [Aktif], [SoftDelete], [OlusturmaTarihi])
VALUES 
    (NEWID(), N'TRY', N'Türk Lirası', N'₺', 1, 0, GETDATE()),
    (NEWID(), N'USD', N'Amerikan Doları', N'$', 1, 0, GETDATE()),
    (NEWID(), N'EUR', N'Euro', N'€', 1, 0, GETDATE()),
    (NEWID(), N'GBP', N'İngiliz Sterlini', N'£', 1, 0, GETDATE()),
    (NEWID(), N'UZS', N'Özbekistan Somu', N'SO''M', 1, 0, GETDATE());
PRINT 'Temel döviz kayıtları eklendi.';

-- ADIM 3: DovizKurlari tablosunu oluştur
CREATE TABLE [DovizKurlari] (
    [DovizKuruID] uniqueidentifier NOT NULL,
    [KaynakParaBirimi] nvarchar(3) NOT NULL,
    [KaynakParaBirimiID] uniqueidentifier NOT NULL,
    [HedefParaBirimi] nvarchar(3) NOT NULL,
    [HedefParaBirimiID] uniqueidentifier NOT NULL,
    [KurDegeri] decimal(18,6) NOT NULL,
    [AlisFiyati] decimal(18,6) NULL,
    [SatisFiyati] decimal(18,6) NULL,
    [Tarih] datetime2 NOT NULL DEFAULT GETDATE(),
    [Kaynak] nvarchar(100) NOT NULL DEFAULT 'Manuel',
    [Aciklama] nvarchar(500) NULL,
    [Aktif] bit NOT NULL DEFAULT 1,
    [SoftDelete] bit NOT NULL DEFAULT 0,
    [OlusturmaTarihi] datetime2 NOT NULL DEFAULT GETDATE(),
    [GuncellemeTarihi] datetime2 NULL,
    CONSTRAINT [PK_DovizKurlari] PRIMARY KEY ([DovizKuruID])
);
PRINT 'DovizKurlari tablosu oluşturuldu.';

-- ADIM 4: Foreign key ilişkilerini ekle
ALTER TABLE [DovizKurlari] ADD CONSTRAINT [FK_DovizKurlari_Dovizler_KaynakParaBirimiID] 
    FOREIGN KEY ([KaynakParaBirimiID]) REFERENCES [Dovizler] ([DovizID]);
    
ALTER TABLE [DovizKurlari] ADD CONSTRAINT [FK_DovizKurlari_Dovizler_HedefParaBirimiID] 
    FOREIGN KEY ([HedefParaBirimiID]) REFERENCES [Dovizler] ([DovizID]);
PRINT 'Foreign key ilişkileri eklendi.';

-- ADIM 5: Temel döviz kurlarını ekle
DECLARE @TRY_ID uniqueidentifier;
DECLARE @USD_ID uniqueidentifier;
DECLARE @EUR_ID uniqueidentifier;
DECLARE @GBP_ID uniqueidentifier;
DECLARE @UZS_ID uniqueidentifier;

SELECT @TRY_ID = DovizID FROM Dovizler WHERE DovizKodu = 'TRY';
SELECT @USD_ID = DovizID FROM Dovizler WHERE DovizKodu = 'USD';
SELECT @EUR_ID = DovizID FROM Dovizler WHERE DovizKodu = 'EUR';
SELECT @GBP_ID = DovizID FROM Dovizler WHERE DovizKodu = 'GBP';
SELECT @UZS_ID = DovizID FROM Dovizler WHERE DovizKodu = 'UZS';

-- USD/TRY
INSERT INTO DovizKurlari (
    DovizKuruID, 
    KaynakParaBirimi, 
    KaynakParaBirimiID, 
    HedefParaBirimi, 
    HedefParaBirimiID, 
    KurDegeri, 
    AlisFiyati, 
    SatisFiyati, 
    Tarih, 
    Kaynak, 
    Aciklama, 
    Aktif, 
    SoftDelete, 
    OlusturmaTarihi
)
VALUES (
    NEWID(),
    'USD',
    @USD_ID,
    'TRY',
    @TRY_ID,
    32.5,
    32.4,
    32.6,
    GETDATE(),
    'Manuel',
    'Manuel olarak eklendi',
    1,
    0,
    GETDATE()
);

-- EUR/TRY
INSERT INTO DovizKurlari (
    DovizKuruID, 
    KaynakParaBirimi, 
    KaynakParaBirimiID, 
    HedefParaBirimi, 
    HedefParaBirimiID, 
    KurDegeri, 
    AlisFiyati, 
    SatisFiyati, 
    Tarih, 
    Kaynak, 
    Aciklama, 
    Aktif, 
    SoftDelete, 
    OlusturmaTarihi
)
VALUES (
    NEWID(),
    'EUR',
    @EUR_ID,
    'TRY',
    @TRY_ID,
    35.1,
    35.0,
    35.2,
    GETDATE(),
    'Manuel',
    'Manuel olarak eklendi',
    1,
    0,
    GETDATE()
);

-- GBP/TRY
INSERT INTO DovizKurlari (
    DovizKuruID, 
    KaynakParaBirimi, 
    KaynakParaBirimiID, 
    HedefParaBirimi, 
    HedefParaBirimiID, 
    KurDegeri, 
    AlisFiyati, 
    SatisFiyati, 
    Tarih, 
    Kaynak, 
    Aciklama, 
    Aktif, 
    SoftDelete, 
    OlusturmaTarihi
)
VALUES (
    NEWID(),
    'GBP',
    @GBP_ID,
    'TRY',
    @TRY_ID,
    41.6,
    41.5,
    41.7,
    GETDATE(),
    'Manuel',
    'Manuel olarak eklendi',
    1,
    0,
    GETDATE()
);

-- UZS/TRY
INSERT INTO DovizKurlari (
    DovizKuruID, 
    KaynakParaBirimi, 
    KaynakParaBirimiID, 
    HedefParaBirimi, 
    HedefParaBirimiID, 
    KurDegeri, 
    AlisFiyati, 
    SatisFiyati, 
    Tarih, 
    Kaynak, 
    Aciklama, 
    Aktif, 
    SoftDelete, 
    OlusturmaTarihi
)
VALUES (
    NEWID(),
    'UZS',
    @UZS_ID,
    'TRY',
    @TRY_ID,
    0.00264,
    0.00262,
    0.00266,
    GETDATE(),
    'Manuel',
    'Manuel olarak eklendi',
    1,
    0,
    GETDATE()
);

PRINT 'Temel döviz kurları eklendi.';

-- ADIM 6: SistemLoglar tablosundaki Tarayici sütununu genişlet
IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'SistemLoglar')
BEGIN
    IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('SistemLoglar') AND name = 'Tarayici' AND max_length < 4000)
    BEGIN
        ALTER TABLE SistemLoglar ALTER COLUMN Tarayici nvarchar(4000) NULL;
        PRINT 'SistemLoglar tablosundaki Tarayici sütunu nvarchar(4000) olarak güncellendi.';
    END
END

PRINT 'Tüm işlemler başarıyla tamamlandı!'; 
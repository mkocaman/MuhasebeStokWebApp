-- Dovizler tablosunu oluştur
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Dovizler]') AND type in (N'U'))
BEGIN
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
    
    -- Temel döviz kodlarını ekle
    INSERT INTO [Dovizler] ([DovizID], [DovizKodu], [DovizAdi], [Sembol], [Aktif], [SoftDelete], [OlusturmaTarihi])
    VALUES 
        (NEWID(), N'TRY', N'Türk Lirası', N'₺', 1, 0, GETDATE()),
        (NEWID(), N'USD', N'Amerikan Doları', N'$', 1, 0, GETDATE()),
        (NEWID(), N'EUR', N'Euro', N'€', 1, 0, GETDATE()),
        (NEWID(), N'GBP', N'İngiliz Sterlini', N'£', 1, 0, GETDATE()),
        (NEWID(), N'UZS', N'Özbekistan Somu', N'SO''M', 1, 0, GETDATE());
    
    PRINT 'Dovizler tablosu oluşturuldu ve temel para birimleri eklendi.';
END
ELSE
BEGIN
    -- SoftDelete sütunu yoksa ekle
    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Dovizler') AND name = 'SoftDelete')
    BEGIN
        ALTER TABLE Dovizler ADD SoftDelete bit NOT NULL DEFAULT 0;
        PRINT 'Dovizler tablosuna SoftDelete sütunu eklendi.';
    END
END

-- DovizKurlari tablosunu oluştur
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DovizKurlari]') AND type in (N'U'))
BEGIN
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
        [Aktif] bit NOT NULL DEFAULT 1,
        [SoftDelete] bit NOT NULL DEFAULT 0,
        [OlusturmaTarihi] datetime2 NOT NULL DEFAULT GETDATE(),
        [GuncellemeTarihi] datetime2 NULL,
        CONSTRAINT [PK_DovizKurlari] PRIMARY KEY ([DovizKuruID])
    );
    
    PRINT 'DovizKurlari tablosu oluşturuldu.';
    
    -- Foreign key'leri ekle
    ALTER TABLE [DovizKurlari] ADD CONSTRAINT [FK_DovizKurlari_Dovizler_KaynakParaBirimiID] 
        FOREIGN KEY ([KaynakParaBirimiID]) REFERENCES [Dovizler] ([DovizID]);
        
    ALTER TABLE [DovizKurlari] ADD CONSTRAINT [FK_DovizKurlari_Dovizler_HedefParaBirimiID] 
        FOREIGN KEY ([HedefParaBirimiID]) REFERENCES [Dovizler] ([DovizID]);
    
    PRINT 'DovizKurlari tablosuna foreign key ilişkileri eklendi.';
END
ELSE
BEGIN
    -- SoftDelete sütunu yoksa ekle
    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('DovizKurlari') AND name = 'SoftDelete')
    BEGIN
        ALTER TABLE DovizKurlari ADD SoftDelete bit NOT NULL DEFAULT 0;
        PRINT 'DovizKurlari tablosuna SoftDelete sütunu eklendi.';
    END
    
    -- Kaynak sütunu yoksa ekle
    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('DovizKurlari') AND name = 'Kaynak')
    BEGIN
        ALTER TABLE DovizKurlari ADD Kaynak nvarchar(100) NOT NULL DEFAULT 'Manuel';
        PRINT 'DovizKurlari tablosuna Kaynak sütunu eklendi.';
    END
    
    -- Aciklama sütunu yoksa ekle
    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('DovizKurlari') AND name = 'Aciklama')
    BEGIN
        ALTER TABLE DovizKurlari ADD Aciklama nvarchar(500) NULL;
        PRINT 'DovizKurlari tablosuna Aciklama sütunu eklendi.';
    END
END

-- SistemLoglar tablosundaki Tarayici sütununu genişlet
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('SistemLoglar') AND name = 'Tarayici' AND max_length < 2000)
BEGIN
    ALTER TABLE SistemLoglar ALTER COLUMN Tarayici nvarchar(2000) NULL;
    PRINT 'SistemLoglar tablosundaki Tarayici sütunu nvarchar(2000) olarak güncellendi.';
END

-- Temel döviz kayıtlarını ekle
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

IF @TRY_ID IS NOT NULL AND @USD_ID IS NOT NULL AND @EUR_ID IS NOT NULL AND @GBP_ID IS NOT NULL AND @UZS_ID IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM DovizKurlari WHERE Aktif = 1)
    BEGIN
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

        PRINT 'DovizKurlari tablosuna örnek kayıtlar eklendi.';
    END
    ELSE
    BEGIN
        PRINT 'DovizKurlari tablosunda zaten aktif kayıtlar bulunmaktadır.';
    END
END
ELSE
BEGIN
    PRINT 'Dovizler tablosunda gerekli kayıtlar bulunamadı.';
END 
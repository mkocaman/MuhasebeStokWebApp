-- Tabloları temizle
IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_DovizKurlari_Dovizler_KaynakParaBirimiID')
    ALTER TABLE DovizKurlari DROP CONSTRAINT FK_DovizKurlari_Dovizler_KaynakParaBirimiID;

IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_DovizKurlari_Dovizler_HedefParaBirimiID')
    ALTER TABLE DovizKurlari DROP CONSTRAINT FK_DovizKurlari_Dovizler_HedefParaBirimiID;

IF OBJECT_ID('DovizKurlari', 'U') IS NOT NULL
    DROP TABLE DovizKurlari;

IF OBJECT_ID('Dovizler', 'U') IS NOT NULL
    DROP TABLE Dovizler;

-- Dovizler tablosunu oluştur
CREATE TABLE Dovizler (
    DovizID uniqueidentifier PRIMARY KEY,
    DovizKodu nvarchar(10) NOT NULL,
    DovizAdi nvarchar(50) NOT NULL,
    Sembol nvarchar(10) NOT NULL,
    Aktif bit DEFAULT 1,
    SoftDelete bit DEFAULT 0,
    OlusturmaTarihi datetime2 DEFAULT GETDATE(),
    GuncellemeTarihi datetime2 NULL
);

-- Döviz kayıtlarını ekle
INSERT INTO Dovizler (DovizID, DovizKodu, DovizAdi, Sembol)
VALUES 
    (NEWID(), 'TRY', 'Türk Lirası', '₺'),
    (NEWID(), 'USD', 'Amerikan Doları', '$'),
    (NEWID(), 'EUR', 'Euro', '€'),
    (NEWID(), 'GBP', 'İngiliz Sterlini', '£'),
    (NEWID(), 'UZS', 'Özbekistan Somu', 'SO''M');

-- DovizKurlari tablosunu oluştur 
CREATE TABLE DovizKurlari (
    DovizKuruID uniqueidentifier PRIMARY KEY,
    KaynakParaBirimi nvarchar(10),
    KaynakParaBirimiID uniqueidentifier,
    HedefParaBirimi nvarchar(10),
    HedefParaBirimiID uniqueidentifier,
    KurDegeri decimal(18,6),
    AlisFiyati decimal(18,6),
    SatisFiyati decimal(18,6),
    Tarih datetime2 DEFAULT GETDATE(),
    Kaynak nvarchar(100),
    Aciklama nvarchar(500),
    Aktif bit DEFAULT 1,
    SoftDelete bit DEFAULT 0,
    OlusturmaTarihi datetime2 DEFAULT GETDATE(),
    GuncellemeTarihi datetime2 NULL
);

-- Foreign key'leri ekle
ALTER TABLE DovizKurlari 
ADD CONSTRAINT FK_DovizKurlari_Dovizler_KaynakParaBirimiID 
FOREIGN KEY (KaynakParaBirimiID) REFERENCES Dovizler(DovizID);

ALTER TABLE DovizKurlari 
ADD CONSTRAINT FK_DovizKurlari_Dovizler_HedefParaBirimiID 
FOREIGN KEY (HedefParaBirimiID) REFERENCES Dovizler(DovizID);

-- SistemLoglar tablosunu kontrol et ve düzelt
IF OBJECT_ID('SistemLoglar', 'U') IS NOT NULL
BEGIN
    IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('SistemLoglar') AND name = 'Tarayici')
    BEGIN
        ALTER TABLE SistemLoglar ALTER COLUMN Tarayici nvarchar(MAX);
        PRINT 'SistemLoglar tablosundaki Tarayici sütunu nvarchar(MAX) olarak güncellendi';
    END
END

PRINT 'Döviz tabloları başarıyla oluşturuldu!'; 
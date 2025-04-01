-- AspNetUsers tablosunda SoftDelete varsa Silindi olarak değiştir, yoksa Silindi sütununu ekle
-- SoftDelete ve Silindi sütunlarının durumunu kontrol et
IF EXISTS (SELECT 1 FROM sys.columns WHERE Name = 'SoftDelete' AND Object_ID = Object_ID('AspNetUsers'))
   AND NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = 'Silindi' AND Object_ID = Object_ID('AspNetUsers'))
BEGIN
    EXEC sp_rename 'AspNetUsers.SoftDelete', 'Silindi', 'COLUMN';
    PRINT 'AspNetUsers.SoftDelete -> Silindi olarak değiştirildi';
END
ELSE IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = 'Silindi' AND Object_ID = Object_ID('AspNetUsers'))
BEGIN
    ALTER TABLE AspNetUsers ADD Silindi bit NOT NULL DEFAULT(0);
    PRINT 'AspNetUsers tablosuna Silindi sütunu eklendi';
END

-- AspNetUsers tablosuna TelefonNo ve Adres sütunlarını ekle
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = 'TelefonNo' AND Object_ID = Object_ID('AspNetUsers'))
BEGIN
    ALTER TABLE AspNetUsers ADD TelefonNo nvarchar(50) NULL;
    PRINT 'AspNetUsers tablosuna TelefonNo sütunu eklendi';
END

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = 'Adres' AND Object_ID = Object_ID('AspNetUsers'))
BEGIN
    ALTER TABLE AspNetUsers ADD Adres nvarchar(200) NULL;
    PRINT 'AspNetUsers tablosuna Adres sütunu eklendi';
END

-- SistemLoglar tablosuna ApplicationUserId sütununu ekle
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = 'ApplicationUserId' AND Object_ID = Object_ID('SistemLoglar'))
BEGIN
    ALTER TABLE SistemLoglar ADD ApplicationUserId varchar(128) NULL;
    PRINT 'SistemLoglar tablosuna ApplicationUserId sütunu eklendi';
END

-- Eğer ParaBirimiIliskileri tablosu yoksa oluştur
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE Name = 'ParaBirimiIliskileri')
BEGIN
    CREATE TABLE ParaBirimiIliskileri (
        ParaBirimiIliskiID uniqueidentifier PRIMARY KEY NOT NULL,
        KaynakParaBirimiID uniqueidentifier NOT NULL,
        HedefParaBirimiID uniqueidentifier NOT NULL,
        Aktif bit NOT NULL DEFAULT(1),
        Silindi bit NOT NULL DEFAULT(0),
        Aciklama nvarchar(500) NULL,
        OlusturmaTarihi datetime2 NOT NULL DEFAULT(GETDATE()),
        GuncellemeTarihi datetime2 NULL,
        OlusturanKullaniciID nvarchar(128) NULL,
        SonGuncelleyenKullaniciID nvarchar(128) NULL
    );
    
    -- İlişkileri tanımla
    ALTER TABLE ParaBirimiIliskileri
    ADD CONSTRAINT FK_ParaBirimiIliskileri_KaynakParaBirimi
    FOREIGN KEY (KaynakParaBirimiID) REFERENCES ParaBirimleri(ParaBirimiID);
    
    ALTER TABLE ParaBirimiIliskileri
    ADD CONSTRAINT FK_ParaBirimiIliskileri_HedefParaBirimi
    FOREIGN KEY (HedefParaBirimiID) REFERENCES ParaBirimleri(ParaBirimiID);
    
    -- Benzersiz indeks ekle
    CREATE UNIQUE INDEX IX_ParaBirimiIliskileri_KaynakHedef
    ON ParaBirimiIliskileri(KaynakParaBirimiID, HedefParaBirimiID);
    
    PRINT 'ParaBirimiIliskileri tablosu oluşturuldu';
END

-- SoftDelete -> Silindi renaming işlemini tüm tablolar için yap
DECLARE @tables TABLE (tableName nvarchar(128));
INSERT INTO @tables VALUES 
('Bankalar'), ('BankaHareketleri'), ('Birimler'), ('Cariler'), ('CariHareketler'),
('Depolar'), ('Faturalar'), ('FaturaDetaylari'), ('FaturaOdemeleri'), ('GenelSistemAyarlari'),
('Irsaliyeler'), ('IrsaliyeDetaylari'), ('Kasalar'), ('KasaHareketleri'),
('StokFifo'), ('StokHareketleri'), ('UrunFiyatlari'), ('UrunKategorileri'), ('Urunler');

DECLARE @tableName nvarchar(128);
DECLARE @sql nvarchar(max);

DECLARE tableCursor CURSOR FOR SELECT tableName FROM @tables;
OPEN tableCursor;
FETCH NEXT FROM tableCursor INTO @tableName;

WHILE @@FETCH_STATUS = 0
BEGIN
    -- Check if the table exists
    IF EXISTS (SELECT 1 FROM sys.tables WHERE name = @tableName)
    BEGIN
        -- Check if SoftDelete exists but Silindi doesn't
        IF EXISTS (SELECT 1 FROM sys.columns WHERE Name = 'SoftDelete' AND Object_ID = Object_ID(@tableName))
           AND NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = 'Silindi' AND Object_ID = Object_ID(@tableName))
        BEGIN
            SET @sql = 'EXEC sp_rename ''' + @tableName + '.SoftDelete'', ''Silindi'', ''COLUMN''';
            EXEC sp_executesql @sql;
            PRINT @tableName + '.SoftDelete -> Silindi olarak değiştirildi';
        END
        -- Check if Silindi doesn't exist
        ELSE IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = 'Silindi' AND Object_ID = Object_ID(@tableName))
        BEGIN
            SET @sql = 'ALTER TABLE ' + @tableName + ' ADD Silindi bit NOT NULL DEFAULT(0)';
            EXEC sp_executesql @sql;
            PRINT @tableName + ' tablosuna Silindi sütunu eklendi';
        END
    END
    
    FETCH NEXT FROM tableCursor INTO @tableName;
END

CLOSE tableCursor;
DEALLOCATE tableCursor;

PRINT 'Tüm tablolarda SoftDelete -> Silindi dönüşümü tamamlandı'; 
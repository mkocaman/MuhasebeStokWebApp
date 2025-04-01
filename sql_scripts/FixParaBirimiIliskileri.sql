-- ParaBirimiIliskileri tablosunun var olup olmadığını kontrol et
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE Name = 'ParaBirimiIliskileri')
BEGIN
    PRINT 'ParaBirimiIliskileri tablosu oluşturuluyor...'
    
    -- Tabloyu oluştur
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
    
    PRINT 'ParaBirimiIliskileri tablosu oluşturuldu'
    
    -- ParaBirimleri tablosunun var olup olmadığını kontrol et
    IF EXISTS (SELECT 1 FROM sys.tables WHERE Name = 'ParaBirimleri')
    BEGIN
        PRINT 'ParaBirimleri tablosu mevcut, ilişkiler oluşturuluyor...'
        
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
        
        PRINT 'ParaBirimiIliskileri için ilişkiler ve indeksler oluşturuldu'
    END
    ELSE
    BEGIN
        PRINT 'UYARI: ParaBirimleri tablosu bulunamadı, ilişkiler oluşturulamadı!'
    END
END
ELSE
BEGIN
    PRINT 'ParaBirimiIliskileri tablosu zaten mevcut'
    
    -- Gerekli kolonlar var mı kontrol et ve eksik olanları ekle
    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = 'Silindi' AND OBJECT_ID = OBJECT_ID('ParaBirimiIliskileri'))
    BEGIN
        ALTER TABLE ParaBirimiIliskileri ADD Silindi bit NOT NULL DEFAULT(0);
        PRINT 'ParaBirimiIliskileri tablosuna Silindi kolonu eklendi'
    END
    
    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = 'Aktif' AND OBJECT_ID = OBJECT_ID('ParaBirimiIliskileri'))
    BEGIN
        ALTER TABLE ParaBirimiIliskileri ADD Aktif bit NOT NULL DEFAULT(1);
        PRINT 'ParaBirimiIliskileri tablosuna Aktif kolonu eklendi'
    END
    
    -- SoftDelete kolonu varsa Silindi olarak değiştir
    IF EXISTS (SELECT 1 FROM sys.columns WHERE Name = 'SoftDelete' AND OBJECT_ID = OBJECT_ID('ParaBirimiIliskileri'))
       AND EXISTS (SELECT 1 FROM sys.columns WHERE Name = 'Silindi' AND OBJECT_ID = OBJECT_ID('ParaBirimiIliskileri'))
    BEGIN
        -- Veriyi taşı
        EXEC('UPDATE ParaBirimiIliskileri SET Silindi = SoftDelete');
        
        -- SoftDelete kolonunu kaldır
        DECLARE @DropConstraintSQL NVARCHAR(MAX)
        SELECT @DropConstraintSQL = 
            'ALTER TABLE ParaBirimiIliskileri DROP CONSTRAINT ' + name
        FROM sys.default_constraints
        WHERE parent_object_id = OBJECT_ID('ParaBirimiIliskileri')
        AND parent_column_id = (
            SELECT column_id FROM sys.columns
            WHERE object_id = OBJECT_ID('ParaBirimiIliskileri')
            AND name = 'SoftDelete'
        )
        
        IF @DropConstraintSQL IS NOT NULL
            EXEC sp_executesql @DropConstraintSQL
            
        ALTER TABLE ParaBirimiIliskileri DROP COLUMN SoftDelete;
        PRINT 'SoftDelete kolonu Silindi kolonuna dönüştürüldü'
    END
    ELSE IF EXISTS (SELECT 1 FROM sys.columns WHERE Name = 'SoftDelete' AND OBJECT_ID = OBJECT_ID('ParaBirimiIliskileri'))
       AND NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = 'Silindi' AND OBJECT_ID = OBJECT_ID('ParaBirimiIliskileri'))
    BEGIN
        EXEC sp_rename 'ParaBirimiIliskileri.SoftDelete', 'Silindi', 'COLUMN';
        PRINT 'SoftDelete kolonu Silindi olarak yeniden adlandırıldı'
    END
END

PRINT 'ParaBirimiIliskileri tablosu kontrolleri tamamlandı' 
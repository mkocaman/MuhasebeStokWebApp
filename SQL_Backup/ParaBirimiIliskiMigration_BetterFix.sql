-- Geliştirilmiş SQL Script to fix ParaBirimiIliskiMigration.sql

-- 1. İlk olarak, #TempSistemAyarlari geçici tablo hatasını çözelim
-- Oturum bilgisini görüntüle
SELECT @@SPID AS [Current Session ID];
GO

-- Tempdb'deki tüm geçici tabloları temizleyelim
PRINT 'TempDB temizleme işlemi başlıyor...';
GO

-- Eğer varsa #TempSistemAyarlari tablosunu sil
IF OBJECT_ID('tempdb..#TempSistemAyarlari') IS NOT NULL
BEGIN
    PRINT '#TempSistemAyarlari mevcut, siliniyor...';
    DROP TABLE #TempSistemAyarlari;
END
GO

-- 2. Şimdi ParaBirimiIliski için Foreign Key düzeltmesi
PRINT 'ParaBirimiIliski Foreign Key düzeltmesi başlıyor...';
GO

-- Eğer ParaBirimiIliski tablosu varsa Foreign Key'leri kontrol edelim
IF OBJECT_ID('ParaBirimiIliski', 'U') IS NOT NULL
BEGIN
    -- Var olan Foreign Key'leri silelim (eğer varsa)
    IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_ParaBirimiIliski_ParaBirimi_KaynakParaBirimiID')
    BEGIN
        ALTER TABLE [ParaBirimiIliski] DROP CONSTRAINT [FK_ParaBirimiIliski_ParaBirimi_KaynakParaBirimiID];
        PRINT 'FK_ParaBirimiIliski_ParaBirimi_KaynakParaBirimiID silindi.';
    END

    IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_ParaBirimiIliski_ParaBirimi_HedefParaBirimiID')
    BEGIN
        ALTER TABLE [ParaBirimiIliski] DROP CONSTRAINT [FK_ParaBirimiIliski_ParaBirimi_HedefParaBirimiID];
        PRINT 'FK_ParaBirimiIliski_ParaBirimi_HedefParaBirimiID silindi.';
    END

    -- Foreign Key'leri NO ACTION ile yeniden oluşturalım
    ALTER TABLE [ParaBirimiIliski] ADD CONSTRAINT [FK_ParaBirimiIliski_ParaBirimi_KaynakParaBirimiID] 
    FOREIGN KEY ([KaynakParaBirimiID]) REFERENCES [ParaBirimi] ([ParaBirimiID]) ON DELETE NO ACTION;
    PRINT 'FK_ParaBirimiIliski_ParaBirimi_KaynakParaBirimiID NO ACTION ile yeniden oluşturuldu.';

    ALTER TABLE [ParaBirimiIliski] ADD CONSTRAINT [FK_ParaBirimiIliski_ParaBirimi_HedefParaBirimiID] 
    FOREIGN KEY ([HedefParaBirimiID]) REFERENCES [ParaBirimi] ([ParaBirimiID]) ON DELETE NO ACTION;
    PRINT 'FK_ParaBirimiIliski_ParaBirimi_HedefParaBirimiID NO ACTION ile yeniden oluşturuldu.';
END
ELSE
BEGIN
    PRINT 'ParaBirimiIliski tablosu mevcut değil, Foreign Key düzeltmesi atlanıyor.';
END
GO

-- SistemAyarlari tablosunun Identity özelliğini düzeltmek için güvenli bir yaklaşım
PRINT 'SistemAyarlari tablosu için Identity düzeltmesi kontrol ediliyor...';
GO

-- SistemAyarlari tablosu var mı kontrol et
IF OBJECT_ID('SistemAyarlari', 'U') IS NOT NULL
BEGIN
    -- Geçici tablo oluşturulup oluşturulmadığını izlemek için bir flag
    DECLARE @TempTableCreated BIT = 0;
    
    -- Eğer geçici tablo zaten varsa, temizle
    IF OBJECT_ID('tempdb..#TempSistemAyarlari') IS NOT NULL
    BEGIN
        DROP TABLE #TempSistemAyarlari;
        PRINT 'Mevcut #TempSistemAyarlari tablosu silindi.';
    END
    
    BEGIN TRY
        -- Verileri geçici tabloya kopyala
        SELECT * INTO #TempSistemAyarlari FROM SistemAyarlari;
        SET @TempTableCreated = 1;
        PRINT 'Veriler başarıyla geçici tabloya kopyalandı.';
        
        -- SistemAyarlariID'nin identity olup olmadığını kontrol et
        IF NOT EXISTS (
            SELECT 1 FROM sys.identity_columns 
            WHERE object_id = OBJECT_ID('SistemAyarlari') 
            AND name = 'SistemAyarlariID'
        )
        BEGIN
            -- Primary Key constraint kaldır
            IF EXISTS (SELECT * FROM sys.key_constraints WHERE name = 'PK_SistemAyarlari')
            BEGIN
                ALTER TABLE SistemAyarlari DROP CONSTRAINT PK_SistemAyarlari;
                PRINT 'PK_SistemAyarlari constraint kaldırıldı.';
            END
            
            -- Mevcut SistemAyarlariID kolonunu kaldır
            IF EXISTS (SELECT * FROM sys.columns WHERE name = 'SistemAyarlariID' AND object_id = OBJECT_ID('SistemAyarlari'))
            BEGIN
                ALTER TABLE SistemAyarlari DROP COLUMN SistemAyarlariID;
                PRINT 'SistemAyarlariID kolonu kaldırıldı.';
            END
            
            -- Yeni bir identity kolon ekle
            ALTER TABLE SistemAyarlari ADD SistemAyarlariID INT IDENTITY(1,1) NOT NULL;
            PRINT 'SistemAyarlariID IDENTITY kolonu eklendi.';
            
            -- Primary Key constraint'i yeniden ekle
            ALTER TABLE SistemAyarlari ADD CONSTRAINT PK_SistemAyarlari PRIMARY KEY (SistemAyarlariID);
            PRINT 'PK_SistemAyarlari constraint yeniden eklendi.';
            
            PRINT 'SistemAyarlari tablosu için Identity düzeltmesi tamamlandı.';
        END
        ELSE
        BEGIN
            PRINT 'SistemAyarlariID zaten IDENTITY özelliğine sahip, düzeltme gerekmiyor.';
        END
        
        -- Geçici tabloyu temizle
        IF @TempTableCreated = 1
        BEGIN
            DROP TABLE #TempSistemAyarlari;
            PRINT '#TempSistemAyarlari tablosu temizlendi.';
        END
    END TRY
    BEGIN CATCH
        PRINT 'Hata: ' + ERROR_MESSAGE();
        
        -- Hata durumunda da geçici tabloyu temizle
        IF @TempTableCreated = 1 AND OBJECT_ID('tempdb..#TempSistemAyarlari') IS NOT NULL
        BEGIN
            DROP TABLE #TempSistemAyarlari;
            PRINT 'Hata sonrası #TempSistemAyarlari tablosu temizlendi.';
        END
    END CATCH
END
ELSE
BEGIN
    PRINT 'SistemAyarlari tablosu mevcut değil, Identity düzeltmesi atlanıyor.';
END
GO

PRINT 'Tüm düzeltmeler tamamlandı.';
GO 
-- SistemLoglar tablosunun varlığını kontrol et
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'SistemLoglar')
BEGIN
    -- IslemTuru sütununun varlığını kontrol et
    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'SistemLoglar' AND COLUMN_NAME = 'IslemTuru')
    BEGIN
        -- IslemTuru sütununu ekle
        ALTER TABLE SistemLoglar
        ADD IslemTuru NVARCHAR(50) NOT NULL DEFAULT ''
        
        PRINT 'IslemTuru sütunu başarıyla eklendi.'
    END
    ELSE
    BEGIN
        PRINT 'IslemTuru sütunu zaten mevcut.'
    END
END
ELSE
BEGIN
    PRINT 'SistemLoglar tablosu bulunamadı.'
END 
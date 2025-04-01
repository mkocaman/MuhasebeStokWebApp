-- CariHareketler tablosuna eksik kolonları ekleyen SQL sorgusu

-- Alacak sütununu ekle
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('CariHareketler') AND name = 'Alacak')
BEGIN
    ALTER TABLE CariHareketler ADD Alacak decimal(18,2) NOT NULL DEFAULT 0;
    PRINT 'Alacak kolonu eklendi.';
END
ELSE 
BEGIN
    PRINT 'Alacak kolonu zaten mevcut.';
END

-- Borc sütununu ekle
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('CariHareketler') AND name = 'Borc')
BEGIN
    ALTER TABLE CariHareketler ADD Borc decimal(18,2) NOT NULL DEFAULT 0;
    PRINT 'Borc kolonu eklendi.';
END
ELSE 
BEGIN
    PRINT 'Borc kolonu zaten mevcut.';
END

-- Varolan kayıtlar için değerleri güncelle
UPDATE CariHareketler 
SET Alacak = CASE WHEN HareketTuru = 'Tahsilat' THEN Tutar ELSE 0 END,
    Borc = CASE WHEN HareketTuru = 'Odeme' THEN Tutar ELSE 0 END
WHERE EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('CariHareketler') AND name = 'Alacak')
  AND EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('CariHareketler') AND name = 'Borc')
  AND (Alacak = 0 AND Borc = 0);

PRINT 'CariHareketler tablosu başarıyla güncellendi!'; 
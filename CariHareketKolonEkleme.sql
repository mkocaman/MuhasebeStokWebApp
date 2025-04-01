-- CariHareket tablosuna eksik kolonları ekleyen SQL sorgusu

-- Alacak sütununu ekle
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('CariHareketler') AND name = 'Alacak')
BEGIN
    ALTER TABLE CariHareketler ADD Alacak decimal(18,2) NOT NULL DEFAULT 0;
END

-- Borc sütununu ekle
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('CariHareketler') AND name = 'Borc')
BEGIN
    ALTER TABLE CariHareketler ADD Borc decimal(18,2) NOT NULL DEFAULT 0;
END

-- IslemTarihi sütununu ekle
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('CariHareketler') AND name = 'IslemTarihi')
BEGIN
    ALTER TABLE CariHareketler ADD IslemTarihi datetime2 NOT NULL DEFAULT GETDATE();
END

-- Varolan kayıtlar için değerleri güncelle
UPDATE CariHareketler 
SET IslemTarihi = Tarih,
    Alacak = CASE WHEN HareketTuru = 'Tahsilat' THEN Tutar ELSE 0 END,
    Borc = CASE WHEN HareketTuru = 'Odeme' THEN Tutar ELSE 0 END
WHERE (IslemTarihi IS NULL OR IslemTarihi = '0001-01-01 00:00:00') 
   OR (Alacak = 0 AND Borc = 0); 
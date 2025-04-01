-- CariHareket tablosunu kontrol et ve eksik sütunları ekle

-- Önce mevcut kolonların durumunu kontrol et
-- SQL Server'da kolonun var olup olmadığını kontrol et ve varsa hiçbir şey yapma

-- Alacak sütununu ekle (eğer yoksa)
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('CariHareket') AND name = 'Alacak')
BEGIN
    ALTER TABLE CariHareket ADD Alacak decimal(18,2) NOT NULL DEFAULT 0;
END

-- Borc sütununu ekle (eğer yoksa)
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('CariHareket') AND name = 'Borc')
BEGIN
    ALTER TABLE CariHareket ADD Borc decimal(18,2) NOT NULL DEFAULT 0;
END

-- Varolan kayıtlar için hesapla
-- Mevcut kayıtlarda Alacak ve Borc kolonlarını Tutar değerine göre doldur
UPDATE CariHareket 
SET Alacak = CASE WHEN HareketTuru = 'Tahsilat' THEN Tutar ELSE 0 END,
    Borc = CASE WHEN HareketTuru = 'Odeme' THEN Tutar ELSE 0 END
WHERE (Alacak = 0 AND Borc = 0);

PRINT 'CariHareket tablosu başarıyla güncellendi!'; 
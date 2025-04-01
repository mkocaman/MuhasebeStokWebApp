-- Her tablodaki SoftDelete sütununu Silindi olarak değiştirmek için SQL script
-- Her tabloyu kontrol edip, yalnızca SoftDelete sütunu varsa ve Silindi sütunu yoksa değişiklik yapıyor

-- AspNetUsers tablosu için kontrol
IF EXISTS (SELECT 1 FROM sys.columns WHERE Name = 'SoftDelete' AND Object_ID = Object_ID('AspNetUsers')) 
AND NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = 'Silindi' AND Object_ID = Object_ID('AspNetUsers'))
BEGIN
    EXEC sp_rename 'AspNetUsers.SoftDelete', 'Silindi', 'COLUMN';
END

-- Bankalar tablosu için kontrol
IF EXISTS (SELECT 1 FROM sys.columns WHERE Name = 'SoftDelete' AND Object_ID = Object_ID('Bankalar')) 
AND NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = 'Silindi' AND Object_ID = Object_ID('Bankalar'))
BEGIN
    EXEC sp_rename 'Bankalar.SoftDelete', 'Silindi', 'COLUMN';
END

-- BankaHareketleri tablosu için kontrol
IF EXISTS (SELECT 1 FROM sys.columns WHERE Name = 'SoftDelete' AND Object_ID = Object_ID('BankaHareketleri')) 
AND NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = 'Silindi' AND Object_ID = Object_ID('BankaHareketleri'))
BEGIN
    EXEC sp_rename 'BankaHareketleri.SoftDelete', 'Silindi', 'COLUMN';
END

-- Birimler tablosu için kontrol
IF EXISTS (SELECT 1 FROM sys.columns WHERE Name = 'SoftDelete' AND Object_ID = Object_ID('Birimler')) 
AND NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = 'Silindi' AND Object_ID = Object_ID('Birimler'))
BEGIN
    EXEC sp_rename 'Birimler.SoftDelete', 'Silindi', 'COLUMN';
END

-- CariHareketler tablosu için kontrol
IF EXISTS (SELECT 1 FROM sys.columns WHERE Name = 'SoftDelete' AND Object_ID = Object_ID('CariHareketler')) 
AND NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = 'Silindi' AND Object_ID = Object_ID('CariHareketler'))
BEGIN
    EXEC sp_rename 'CariHareketler.SoftDelete', 'Silindi', 'COLUMN';
END

-- Cariler tablosu için kontrol
IF EXISTS (SELECT 1 FROM sys.columns WHERE Name = 'SoftDelete' AND Object_ID = Object_ID('Cariler')) 
AND NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = 'Silindi' AND Object_ID = Object_ID('Cariler'))
BEGIN
    EXEC sp_rename 'Cariler.SoftDelete', 'Silindi', 'COLUMN';
END

-- Depolar tablosu için kontrol
IF EXISTS (SELECT 1 FROM sys.columns WHERE Name = 'SoftDelete' AND Object_ID = Object_ID('Depolar')) 
AND NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = 'Silindi' AND Object_ID = Object_ID('Depolar'))
BEGIN
    EXEC sp_rename 'Depolar.SoftDelete', 'Silindi', 'COLUMN';
END

-- FaturaDetaylari tablosu için kontrol
IF EXISTS (SELECT 1 FROM sys.columns WHERE Name = 'SoftDelete' AND Object_ID = Object_ID('FaturaDetaylari')) 
AND NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = 'Silindi' AND Object_ID = Object_ID('FaturaDetaylari'))
BEGIN
    EXEC sp_rename 'FaturaDetaylari.SoftDelete', 'Silindi', 'COLUMN';
END

-- Faturalar tablosu için kontrol
IF EXISTS (SELECT 1 FROM sys.columns WHERE Name = 'SoftDelete' AND Object_ID = Object_ID('Faturalar')) 
AND NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = 'Silindi' AND Object_ID = Object_ID('Faturalar'))
BEGIN
    EXEC sp_rename 'Faturalar.SoftDelete', 'Silindi', 'COLUMN';
END

-- FaturaOdemeleri tablosu için kontrol
IF EXISTS (SELECT 1 FROM sys.columns WHERE Name = 'SoftDelete' AND Object_ID = Object_ID('FaturaOdemeleri')) 
AND NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = 'Silindi' AND Object_ID = Object_ID('FaturaOdemeleri'))
BEGIN
    EXEC sp_rename 'FaturaOdemeleri.SoftDelete', 'Silindi', 'COLUMN';
END

-- GenelSistemAyarlari tablosu için kontrol
IF EXISTS (SELECT 1 FROM sys.columns WHERE Name = 'SoftDelete' AND Object_ID = Object_ID('GenelSistemAyarlari')) 
AND NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = 'Silindi' AND Object_ID = Object_ID('GenelSistemAyarlari'))
BEGIN
    EXEC sp_rename 'GenelSistemAyarlari.SoftDelete', 'Silindi', 'COLUMN';
END

-- IrsaliyeDetaylari tablosu için kontrol
IF EXISTS (SELECT 1 FROM sys.columns WHERE Name = 'SoftDelete' AND Object_ID = Object_ID('IrsaliyeDetaylari')) 
AND NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = 'Silindi' AND Object_ID = Object_ID('IrsaliyeDetaylari'))
BEGIN
    EXEC sp_rename 'IrsaliyeDetaylari.SoftDelete', 'Silindi', 'COLUMN';
END

-- Irsaliyeler tablosu için kontrol
IF EXISTS (SELECT 1 FROM sys.columns WHERE Name = 'SoftDelete' AND Object_ID = Object_ID('Irsaliyeler')) 
AND NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = 'Silindi' AND Object_ID = Object_ID('Irsaliyeler'))
BEGIN
    EXEC sp_rename 'Irsaliyeler.SoftDelete', 'Silindi', 'COLUMN';
END

-- KasaHareketleri tablosu için kontrol
IF EXISTS (SELECT 1 FROM sys.columns WHERE Name = 'SoftDelete' AND Object_ID = Object_ID('KasaHareketleri')) 
AND NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = 'Silindi' AND Object_ID = Object_ID('KasaHareketleri'))
BEGIN
    EXEC sp_rename 'KasaHareketleri.SoftDelete', 'Silindi', 'COLUMN';
END

-- Kasalar tablosu için kontrol
IF EXISTS (SELECT 1 FROM sys.columns WHERE Name = 'SoftDelete' AND Object_ID = Object_ID('Kasalar')) 
AND NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = 'Silindi' AND Object_ID = Object_ID('Kasalar'))
BEGIN
    EXEC sp_rename 'Kasalar.SoftDelete', 'Silindi', 'COLUMN';
END

-- StokFifo tablosu için kontrol
IF EXISTS (SELECT 1 FROM sys.columns WHERE Name = 'SoftDelete' AND Object_ID = Object_ID('StokFifo')) 
AND NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = 'Silindi' AND Object_ID = Object_ID('StokFifo'))
BEGIN
    EXEC sp_rename 'StokFifo.SoftDelete', 'Silindi', 'COLUMN';
END

-- StokHareketleri tablosu için kontrol
IF EXISTS (SELECT 1 FROM sys.columns WHERE Name = 'SoftDelete' AND Object_ID = Object_ID('StokHareketleri')) 
AND NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = 'Silindi' AND Object_ID = Object_ID('StokHareketleri'))
BEGIN
    EXEC sp_rename 'StokHareketleri.SoftDelete', 'Silindi', 'COLUMN';
END

-- UrunFiyatlari tablosu için kontrol
IF EXISTS (SELECT 1 FROM sys.columns WHERE Name = 'SoftDelete' AND Object_ID = Object_ID('UrunFiyatlari')) 
AND NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = 'Silindi' AND Object_ID = Object_ID('UrunFiyatlari'))
BEGIN
    EXEC sp_rename 'UrunFiyatlari.SoftDelete', 'Silindi', 'COLUMN';
END

-- UrunKategorileri tablosu için kontrol
IF EXISTS (SELECT 1 FROM sys.columns WHERE Name = 'SoftDelete' AND Object_ID = Object_ID('UrunKategorileri')) 
AND NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = 'Silindi' AND Object_ID = Object_ID('UrunKategorileri'))
BEGIN
    EXEC sp_rename 'UrunKategorileri.SoftDelete', 'Silindi', 'COLUMN';
END

-- Urunler tablosu için kontrol
IF EXISTS (SELECT 1 FROM sys.columns WHERE Name = 'SoftDelete' AND Object_ID = Object_ID('Urunler')) 
AND NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = 'Silindi' AND Object_ID = Object_ID('Urunler'))
BEGIN
    EXEC sp_rename 'Urunler.SoftDelete', 'Silindi', 'COLUMN';
END

-- Menuler tablosu için kontrol
IF EXISTS (SELECT 1 FROM sys.columns WHERE Name = 'SoftDelete' AND Object_ID = Object_ID('Menuler')) 
AND NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = 'Silindi' AND Object_ID = Object_ID('Menuler'))
BEGIN
    EXEC sp_rename 'Menuler.SoftDelete', 'Silindi', 'COLUMN';
END

-- ParaBirimleri tablosu için kontrol
IF EXISTS (SELECT 1 FROM sys.columns WHERE Name = 'SoftDelete' AND Object_ID = Object_ID('ParaBirimleri')) 
AND NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = 'Silindi' AND Object_ID = Object_ID('ParaBirimleri'))
BEGIN
    EXEC sp_rename 'ParaBirimleri.SoftDelete', 'Silindi', 'COLUMN';
END

-- DovizKurlari tablosu için kontrol
IF EXISTS (SELECT 1 FROM sys.columns WHERE Name = 'SoftDelete' AND Object_ID = Object_ID('DovizKurlari')) 
AND NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = 'Silindi' AND Object_ID = Object_ID('DovizKurlari'))
BEGIN
    EXEC sp_rename 'DovizKurlari.SoftDelete', 'Silindi', 'COLUMN';
END

PRINT 'SoftDelete sütunları başarıyla Silindi olarak değiştirildi'; 
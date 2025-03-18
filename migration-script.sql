-- SistemAyarlari tablosundaki eksik sütunları kontrol et ve ekle
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.SistemAyarlari') AND name = 'Aktif')
BEGIN
    ALTER TABLE dbo.SistemAyarlari ADD Aktif bit NOT NULL DEFAULT 1;
    PRINT 'Aktif sütunu SistemAyarlari tablosuna eklendi.';
END

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.SistemAyarlari') AND name = 'SoftDelete')
BEGIN
    ALTER TABLE dbo.SistemAyarlari ADD SoftDelete bit NOT NULL DEFAULT 0;
    PRINT 'SoftDelete sütunu SistemAyarlari tablosuna eklendi.';
END

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.SistemAyarlari') AND name = 'OlusturmaTarihi')
BEGIN
    ALTER TABLE dbo.SistemAyarlari ADD OlusturmaTarihi datetime2 NOT NULL DEFAULT GETDATE();
    PRINT 'OlusturmaTarihi sütunu SistemAyarlari tablosuna eklendi.';
END

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.SistemAyarlari') AND name = 'AktifParaBirimleri')
BEGIN
    ALTER TABLE dbo.SistemAyarlari ADD AktifParaBirimleri nvarchar(MAX) NULL;
    PRINT 'AktifParaBirimleri sütunu SistemAyarlari tablosuna eklendi.';
END

-- DovizKurlari tablosundaki eksik sütunları kontrol et ve ekle
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.DovizKurlari') AND name = 'Aciklama')
BEGIN
    ALTER TABLE dbo.DovizKurlari ADD Aciklama nvarchar(500) NULL;
    PRINT 'Aciklama sütunu DovizKurlari tablosuna eklendi.';
END

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.DovizKurlari') AND name = 'BazParaBirimi')
BEGIN
    ALTER TABLE dbo.DovizKurlari ADD BazParaBirimi nvarchar(10) NOT NULL DEFAULT 'TRY';
    PRINT 'BazParaBirimi sütunu DovizKurlari tablosuna eklendi.';
END

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.DovizKurlari') AND name = 'Kaynak')
BEGIN
    ALTER TABLE dbo.DovizKurlari ADD Kaynak nvarchar(100) NULL;
    PRINT 'Kaynak sütunu DovizKurlari tablosuna eklendi.';
END

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.DovizKurlari') AND name = 'Kur')
BEGIN
    ALTER TABLE dbo.DovizKurlari ADD Kur decimal(18, 6) NOT NULL DEFAULT 0;
    PRINT 'Kur sütunu DovizKurlari tablosuna eklendi.';
END

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.DovizKurlari') AND name = 'OlusturmaTarihi')
BEGIN
    ALTER TABLE dbo.DovizKurlari ADD OlusturmaTarihi datetime2 NOT NULL DEFAULT GETDATE();
    PRINT 'OlusturmaTarihi sütunu DovizKurlari tablosuna eklendi.';
END

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.DovizKurlari') AND name = 'ParaBirimi')
BEGIN
    ALTER TABLE dbo.DovizKurlari ADD ParaBirimi nvarchar(10) NOT NULL DEFAULT 'USD';
    PRINT 'ParaBirimi sütunu DovizKurlari tablosuna eklendi.';
END

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.DovizKurlari') AND name = 'SoftDelete')
BEGIN
    ALTER TABLE dbo.DovizKurlari ADD SoftDelete bit NOT NULL DEFAULT 0;
    PRINT 'SoftDelete sütunu DovizKurlari tablosuna eklendi.';
END

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.DovizKurlari') AND name = 'DovizKodu')
BEGIN
    ALTER TABLE dbo.DovizKurlari ADD DovizKodu nvarchar(10) NULL;
    PRINT 'DovizKodu sütunu DovizKurlari tablosuna eklendi.';
END

PRINT 'Veritabanı şema güncellemeleri tamamlandı.'; 
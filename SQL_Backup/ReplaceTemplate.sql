-- ===============================================
-- GEÇİCİ TABLO ADINI SCRIPT İÇİNDE DEĞİŞTİRME ŞABLONU
-- ===============================================
-- Bu şablonu kullanarak migration scriptinizdeki geçici tablo adlarını değiştirebilirsiniz
-- Bu değişiklik, aynı isimle geçici tablo oluşturmayı engelleyecektir
-- ===============================================

-- 1. Script dosyanızı SQL Server Management Studio'da metin olarak açın (sağ tıklayıp "Edit" seçin, F4 tuşunu kullanın)
-- 2. "Edit" menüsünden "Find and Replace" (Ctrl+H) penceresini açın
-- 3. Aşağıdaki değişiklikleri uygulayın:

-- ÖNEMLİ NOT: Düzenlemeler metin editöründe yapılmalıdır, "EDIT TOP 1000 ROWS" kullanarak değil

-- ------------------------------------
-- DEĞİŞTİRİLECEK METİNLER:
-- ------------------------------------
-- #TempSistemAyarlari -> #TempSistemAyarlari_RandomID
-- CREATE TABLE #TempSistemAyarlari -> CREATE TABLE #TempSistemAyarlari_RandomID
-- INSERT INTO #TempSistemAyarlari -> INSERT INTO #TempSistemAyarlari_RandomID
-- SELECT * FROM #TempSistemAyarlari -> SELECT * FROM #TempSistemAyarlari_RandomID
-- DROP TABLE #TempSistemAyarlari -> DROP TABLE #TempSistemAyarlari_RandomID
-- TRUNCATE TABLE #TempSistemAyarlari -> TRUNCATE TABLE #TempSistemAyarlari_RandomID
-- UPDATE #TempSistemAyarlari -> UPDATE #TempSistemAyarlari_RandomID
-- DELETE FROM #TempSistemAyarlari -> DELETE FROM #TempSistemAyarlari_RandomID
-- ------------------------------------

-- Script dosyanızın başına eklenecek yeni güvenlik kontrolü:
-- Bu kodu yeni scriptinizin en başına ekleyin

-- ************** BAŞLANGIÇ KODU **************
USE tempdb;
GO

-- Benzersiz bir isim oluştur - oturum ID ile
DECLARE @YeniTempTabloAdi NVARCHAR(100) = '#TempSistemAyarlari_' + CAST(@@SPID AS NVARCHAR(10));
PRINT 'Bu oturum için benzersiz geçici tablo adı: ' + @YeniTempTabloAdi;

-- Eski geçici tabloyu temizle (eğer varsa)
DECLARE @sql NVARCHAR(MAX) = N'
IF OBJECT_ID(''tempdb..#TempSistemAyarlari'') IS NOT NULL
BEGIN
    PRINT ''Eski #TempSistemAyarlari tablosu siliniyor...'';
    DROP TABLE #TempSistemAyarlari;
    PRINT ''Eski tablo silindi.'';
END
ELSE
BEGIN
    PRINT ''Eski #TempSistemAyarlari tablosu bulunamadı.'';
END';

-- Dinamik SQL'i çalıştır
BEGIN TRY
    EXEC sp_executesql @sql;
END TRY
BEGIN CATCH
    PRINT 'Eski geçici tabloyu silme hatası: ' + ERROR_MESSAGE();
END CATCH;

-- Yeni geçici tablo adı için sihirli kodlar
PRINT 'İPUCU: Migration scriptinizdeki tüm #TempSistemAyarlari ifadelerini ' + @YeniTempTabloAdi + ' ile değiştirin!';
PRINT 'Değiştirme işlemi için "Find and Replace" (Ctrl+H) kullanın.';
GO
-- ************** BİTİŞ KODU **************

-- ΨΨΨ VEYA ΨΨΨ
-- Migration script içindeki geçici tabloları komple kaldırmak için:

/*
Migration scriptinizdeki şu satırları:

1) CREATE TABLE #TempSistemAyarlari
2) SELECT * INTO #TempSistemAyarlari FROM SistemAyarlari;
3) Her türlü #TempSistemAyarlari ile yaptığınız işlemler (SİLİN)

Yerine doğrudan şöyle değiştirin (örnek kod):

-- Doğrudan SistemAyarlari tablosunda işlem yap
ALTER TABLE [SistemAyarlari] DROP CONSTRAINT [FK_SistemAyarlari_HedefSistemAyarlariID];
ALTER TABLE [SistemAyarlari] DROP CONSTRAINT [FK_SistemAyarlari_KaynakSistemAyarlariID];

-- Bu tablo için identity değerini değiştirme
DBCC CHECKIDENT ('[SistemAyarlari]', RESEED, 0);

-- Sonra gerekli değişiklikleri yap
ALTER TABLE [SistemAyarlari] ADD CONSTRAINT [FK_SistemAyarlari_HedefSistemAyarlariID] 
  FOREIGN KEY ([HedefSistemAyarlariID]) REFERENCES [SistemAyarlari] ([SistemAyarlariID]) ON DELETE NO ACTION;
ALTER TABLE [SistemAyarlari] ADD CONSTRAINT [FK_SistemAyarlari_KaynakSistemAyarlariID] 
  FOREIGN KEY ([KaynakSistemAyarlariID]) REFERENCES [SistemAyarlari] ([SistemAyarlariID]) ON DELETE NO ACTION;
*/

-- SON ÇAREMİZ: Tamamen farklı bir yaklaşımla düzeltme
-- ===============================================
PRINT '---- GERÇEK ÇÖZÜM ŞU ----';
PRINT '';
PRINT '1. SQL Server Management Studio''yu tamamen kapatın';
PRINT '2. Yeniden açın ve migration scriptinizi metin olarak düzenleyin';
PRINT '3. Aşağıdaki değişiklikleri uygulayın:';
PRINT '';
PRINT 'Değişiklik 1: Migration scriptinizin başına aşağıdaki kodu ekleyin:';
PRINT '';
PRINT 'USE MuhasebeStokWebDB;
GO
-- Eğer geçici tablo varsa temizle
IF OBJECT_ID(''tempdb..#TempSistemAyarlari'') IS NOT NULL DROP TABLE #TempSistemAyarlari;
GO';
PRINT '';
PRINT '4. Ardından Ctrl+H ile Find ve Replace kullanarak:';
PRINT '   #TempSistemAyarlari => #TempSistemAyarlari_' + CAST(@@SPID AS NVARCHAR(10));
PRINT '';
PRINT '5. Değişikliklerinizi kaydedin ve çalıştırın';
PRINT '';
PRINT 'İyi şanslar!';
GO 
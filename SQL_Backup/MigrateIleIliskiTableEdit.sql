-- Migration script içindeki #TempSistemAyarlari ismini değiştirme
-- Bu script, migration betiğinizde sadece geçici tablo adını değiştirmek için kullanılabilir
-- Sorunun kalıcı olarak çözülmesine yardımcı olur

-- Yeni script oluşturalım
PRINT 'Migration script içindeki geçici tablo adını değiştirme işlemi başlıyor...';
GO

-- Oturum bilgisini görüntüle
SELECT @@SPID AS [Current Session ID];
GO

-- Eski scripti yedekleyelim (eğer migration_app_script.sql dosyası varsa)
IF OBJECT_ID('dbo.sp_MigrationTableNameChange', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_MigrationTableNameChange;
GO

CREATE PROCEDURE dbo.sp_MigrationTableNameChange
AS
BEGIN
    PRINT 'Bu prosedür, migration scriptinizdeki #TempSistemAyarlari adını #TempSistemAyarlari2 olarak değiştirmenizi sağlar.';
    PRINT 'Değişiklik yapmadan önce lütfen migration scriptinizin yedeğini alın!';
    PRINT '';
    PRINT 'Örnek kullanım (SQL Server Management Studio''da):';
    PRINT '1. Migration scriptinizdeki tüm "#TempSistemAyarlari" ifadelerini "Bul-Değiştir" kullanarak "#TempSistemAyarlari2" olarak değiştirin';
    PRINT '2. Veya elle manuel olarak aşağıdaki değişiklikleri yapın:';
    PRINT '';
    PRINT '- "CREATE TABLE #TempSistemAyarlari (" → "CREATE TABLE #TempSistemAyarlari2 ("';
    PRINT '- "INSERT INTO #TempSistemAyarlari (" → "INSERT INTO #TempSistemAyarlari2 ("';
    PRINT '- "DROP TABLE #TempSistemAyarlari;" → "DROP TABLE #TempSistemAyarlari2;"';
    PRINT '- "SELECT * INTO #TempSistemAyarlari FROM SistemAyarlari;" → "SELECT * INTO #TempSistemAyarlari2 FROM SistemAyarlari;"';
    PRINT '';
    PRINT 'Değişikliklerden sonra migration scriptinizi tekrar çalıştırın.';
END;
GO

-- Test için prosedürü çalıştıralım
EXEC dbo.sp_MigrationTableNameChange;
GO

-- Geçici tablo kullanımı ile ilgili en iyi pratikler
PRINT '==== Geçici Tablo Kullanımı İçin En İyi Pratikler ====';
PRINT '1. Geçici tabloları kullanmadan önce her zaman var olup olmadıklarını kontrol edin:';
PRINT '   IF OBJECT_ID(''tempdb..#TempTable'') IS NOT NULL DROP TABLE #TempTable;';
PRINT '';
PRINT '2. Geçici tablolar kullanıldıktan sonra her zaman silinmelidir:';
PRINT '   DROP TABLE #TempTable;';
PRINT '';
PRINT '3. Migration scriptleri için global geçici tablolar (##) yerine lokal geçici tablolar (#) kullanın.';
PRINT '';
PRINT '4. Lütfen geçici tabloları TRY/CATCH bloğu içinde kullanın ve CATCH bloğunda da silin.';
PRINT '';
PRINT '5. ParaBirimiIliskiMigration.sql ve benzeri scriptleri farklı isimli geçici tablolarla güncelleyin.';
GO

-- Kullanıcıya bilgi ver
PRINT '===============================================';
PRINT 'Çözüm önerileri:';
PRINT '1. SuperTempTableFix.sql betiğini çalıştırın (geçici tabloları temizlemek için)';
PRINT '2. Migration scriptinizdeki tüm #TempSistemAyarlari ifadelerini #TempSistemAyarlari2 olarak değiştirin';
PRINT '3. SQL Server Management Studio''yu kapatıp yeniden açın';
PRINT '4. Değiştirilmiş migration scriptinizi çalıştırın';
PRINT '===============================================';
GO 
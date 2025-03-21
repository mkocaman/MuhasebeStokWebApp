-- Süper Geliştirilmiş Geçici Tablo Temizleme Betiği
-- Bu betik, tempdb'deki tüm #TempSistemAyarlari benzeri geçici tabloları bulup siler
-- Ekran görüntülerinden anlaşıldığı üzere birden fazla oturum ve farklı ID'lerle geçici tablolar oluşmuş

SET NOCOUNT ON;
GO

-- İlk olarak mevcut bağlantı ID'mizi görelim
PRINT 'Mevcut oturum ID: ' + CAST(@@SPID AS NVARCHAR(10));
GO

-- Tempdb'deki tüm geçici tabloları göster 
-- (Ekran görüntülerinde görülen ve 'TempSistemAyarlari' içeren tablolara odaklanarak)
PRINT 'TempDB''deki "TempSistemAyarlari" benzeri tüm geçici tablolar listeleniyor...';

-- Tüm kullanıcı geçici tablolarını listele
SELECT 
    OBJECT_NAME(object_id) AS name,
    object_id,
    OBJECT_SCHEMA_NAME(object_id) AS schema_name,
    create_date
FROM tempdb.sys.objects 
WHERE name LIKE '%TempSistemAyarlari%' OR name LIKE '#%'
ORDER BY create_date DESC;
GO

-- Eğer varsa normal geçici tabloyu sil
IF OBJECT_ID('tempdb..#TempSistemAyarlari') IS NOT NULL
BEGIN
    PRINT '#TempSistemAyarlari geçici tablosu bulundu, siliniyor...';
    DROP TABLE #TempSistemAyarlari;
    PRINT '#TempSistemAyarlari geçici tablosu silindi.';
END
ELSE
BEGIN
    PRINT 'Bu oturumda #TempSistemAyarlari tablosu bulunamadı.';
END
GO

-- Diğer oturumlardaki geçici tabloları silmek için bir prosedür
-- Bu, başka oturumlarda oluşturulmuş geçici tabloları silmek için özel bir yaklaşımdır
PRINT 'Diğer oturumlarda oluşturulan benzer geçici tablolar aranıyor...';
GO

-- SQL Server 2016 ve üzeri için kullanılabilir
-- Tüm benzer adlı geçici tabloları bul
DECLARE @sqlKill NVARCHAR(MAX) = N'';

SELECT @sqlKill = @sqlKill + 
    N'DROP TABLE ' + QUOTENAME(name) + N'; PRINT ''' + name + N' silindi.''; ' 
FROM tempdb.sys.objects
WHERE (name LIKE '%Sistem%' OR name LIKE '%TempSistem%') AND type = 'U';

-- Yedek olarak adı # ile başlayan ve "temp" benzeri tüm tabloları da kontrol et
SELECT @sqlKill = @sqlKill + 
    N'DROP TABLE ' + QUOTENAME(name) + N'; PRINT ''' + name + N' silindi.''; ' 
FROM tempdb.sys.objects
WHERE name LIKE '#Temp%' AND type = 'U';

-- Eğer silinecek tablolar varsa, sil
IF LEN(@sqlKill) > 0
BEGIN
    PRINT 'Bulunan geçici tablolar siliniyor...';
    BEGIN TRY
        EXEC sp_executesql @sqlKill;
        PRINT 'Benzer adlı tüm geçici tablolar silindi.';
    END TRY
    BEGIN CATCH
        PRINT 'Hata: ' + ERROR_MESSAGE();
        PRINT 'Diğer oturumlara ait geçici tabloları silmek için yeterli izinler olmayabilir.';
        PRINT 'Yönetici olarak yeniden deneyin veya SQL Server''ı yeniden başlatın.';
    END CATCH
END
ELSE
BEGIN
    PRINT 'Silinecek ek geçici tablo bulunamadı.';
END
GO

-- Tempdb'de kalıntıları temizlemek için daha radikal bir yaklaşım
PRINT 'Ek tempdb temizleme işlemleri kontrol ediliyor...';
GO

-- TempSistemAyarlari kalıntılarını dinamik SQL ile temizle
DECLARE @tableName NVARCHAR(128);
DECLARE @dynSql NVARCHAR(MAX);
DECLARE @sessionID INT;

-- Farklı oturumlara ait geçici tablolar için
DECLARE temp_cursor CURSOR FOR
SELECT name FROM tempdb.sys.tables 
WHERE name LIKE '#%TempSistemAyarlari%' OR name LIKE '#%Sistem%';

OPEN temp_cursor;
FETCH NEXT FROM temp_cursor INTO @tableName;

WHILE @@FETCH_STATUS = 0
BEGIN
    SET @dynSql = N'IF OBJECT_ID(''tempdb..' + @tableName + ''') IS NOT NULL 
                    BEGIN 
                        DROP TABLE ' + @tableName + ';
                        PRINT ''' + @tableName + ' silindi.'';
                    END;';
    
    BEGIN TRY
        EXEC sp_executesql @dynSql;
    END TRY
    BEGIN CATCH
        PRINT 'Tablo silinemedi: ' + @tableName + ' - ' + ERROR_MESSAGE();
    END CATCH
    
    FETCH NEXT FROM temp_cursor INTO @tableName;
END

CLOSE temp_cursor;
DEALLOCATE temp_cursor;
GO

-- Kullanıcı tarafından oluşturulan tüm geçici tabloları temizle
PRINT 'Tüm kullanıcı geçici tabloları temizleniyor...';
GO

-- En son yapılan işlemleri kontrol et
PRINT 'TempDB''deki tüm geçici tablolar (temizlik sonrası):';
SELECT name, object_id, create_date 
FROM tempdb.sys.tables 
WHERE name LIKE '%Temp%' OR name LIKE '#%';
GO

-- Başarı mesajı
PRINT '=======================================';
PRINT 'TempDB temizleme işlemi tamamlandı.';
PRINT 'Şimdi asıl migration script çalıştırılabilir.';
PRINT '=======================================';
GO

-- Uygulamanız gereken önemli not
PRINT 'ÖNEMLİ: Eğer bu betik çalıştıktan sonra hala aynı hatayı alıyorsanız:';
PRINT '1. SQL Server Management Studio''yu tamamen kapatıp yeniden açın';
PRINT '2. Veya SQL Server servisini yeniden başlatın';
PRINT '3. Veya migration scriptindeki "#TempSistemAyarlari" isimlerini "#TempSistemAyarlari2" olarak değiştirin';
GO 
-- Geliştirilmiş SQL Script to fix '#TempSistemAyarlari' already exists error

-- SQL Server'da PRINT mesajlarını kullanarak nerede olduğumuzu görelim
PRINT 'Başlangıç: Geçici tablo kontrolü yapılıyor...'

-- Oturum bilgisini görüntüle
SELECT @@SPID AS [Current Session ID]

-- Tempdb'deki tüm geçici tabloları göster
PRINT 'TempDB''deki geçici tablolar:'
SELECT name, object_id, create_date 
FROM tempdb.sys.tables 
WHERE name LIKE '#%'

-- Özel olarak #TempSistemAyarlari tablosunu ara
PRINT '#TempSistemAyarlari tablosu kontrol ediliyor...'
IF OBJECT_ID('tempdb..#TempSistemAyarlari') IS NOT NULL
BEGIN
    PRINT '#TempSistemAyarlari tablosu bulundu ve siliniyor...'
    DROP TABLE #TempSistemAyarlari
    PRINT '#TempSistemAyarlari tablosu silindi.'
END
ELSE
BEGIN
    PRINT '#TempSistemAyarlari tablosu bulunamadı.'
END
GO

-- Tüm geçici nesneleri temizle (ihtiyaç olursa)
PRINT 'TempDB kaynaklarını temizleme işlemi...'
DECLARE @sql NVARCHAR(MAX) = N''

SELECT @sql = @sql + N'DROP TABLE ' + QUOTENAME(name) + N'; '
FROM tempdb.sys.tables
WHERE name LIKE '#TempSistemAyarlari%'

IF LEN(@sql) > 0
BEGIN
    PRINT 'Bulunan diğer benzer geçici tablolar siliniyor...'
    EXEC sp_executesql @sql
    PRINT 'Temizleme tamamlandı.'
END
ELSE
BEGIN
    PRINT 'Silinecek başka geçici tablo bulunamadı.'
END
GO

PRINT 'Geçici tablo temizliği tamamlandı. Script çalıştırılabilir.'
GO

-- Not: Bu betiği SQL Server Management Studio'da, asıl betikten önce çalıştırın
-- Bu betik hem mevcut #TempSistemAyarlari tablosunu hem de benzer adlara sahip 
-- diğer tüm geçici tabloları temizleyecektir 
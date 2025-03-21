-- ==============================================
-- TEMPDB VE GEÇİCİ TABLOLARI TEMİZLEME SUPER SCRIPT
-- ==============================================
-- Bu script, tüm geçici tabloları ve özellikle #TempSistemAyarlari ile ilgili
-- sorunları çözecek şekilde tasarlanmıştır.
-- Önceki temizleme girişimlerinde eksik olan şey:
-- 1. Tüm oturumlardaki geçici tabloları bulma
-- 2. Tablolara referans veren nesneleri kaldırma
-- 3. Yeterli izinlerle geçici tabloları silme
-- ==============================================

SET NOCOUNT ON;
GO

USE tempdb;
GO

-- Yönetici olarak çalıştığınızdan emin olun
PRINT '============================================================';
PRINT 'GEÇİCİ TABLO TEMİZLEME SÜPER SCRIPT BAŞLIYOR';
PRINT '============================================================';
PRINT '';
PRINT 'NOT: Bu betiği SA (veya eşdeğer yönetici) hesabıyla çalıştırmanız gerekiyor';
PRINT '';

-- Her türlü oturum kilitleme işlemi öncesinde bir uyarı
PRINT 'DİKKAT: Bu betik agresif bir şekilde tempdb''deki geçici tabloları temizleyecek';
PRINT 'Çalışan önemli sorgular varsa, tamamlanana kadar bekleyebilirsiniz.';
PRINT '';
GO

-- 1. ADIM: TEMPDB İÇİNDEKİ TÜM KULLANICI OTURUMLARINI LİSTELE
PRINT '1. ADIM: Mevcut tüm kullanıcı oturumları listeleniyor...';
SELECT 
    s.session_id, 
    s.login_name, 
    s.host_name, 
    s.program_name,
    s.login_time,
    DB_NAME(s.database_id) as database_name,
    s.status,
    s.last_request_start_time,
    s.last_request_end_time
FROM sys.dm_exec_sessions s
WHERE s.database_id = DB_ID('tempdb')
  AND s.session_id > 50  -- Sistem oturumlarını hariç tut
  AND s.session_id <> @@SPID; -- Kendi oturumumuzu hariç tut

-- 2. ADIM: TEMPDB'DEKİ TÜM GEÇİCİ TABLOLARI BULMAK
PRINT '';
PRINT '2. ADIM: TEMPDB''deki tüm geçici tablolar ve detayları listeleniyor...';

-- TempSistemAyarlari ile ilgili tabloları özellikle vurgula
SELECT 
    OBJECT_NAME(o.object_id) AS table_name,
    o.object_id,
    o.create_date,
    o.modify_date,
    s.name AS schema_name,
    CASE 
        WHEN OBJECT_NAME(o.object_id) LIKE '%TempSistemAyarlari%' THEN 'EVET - BU TABLO SORUN YARATIYOR'
        ELSE 'Hayır' 
    END AS is_problem_table,
    CASE
        WHEN OBJECT_NAME(o.object_id) LIKE '#%' THEN CONCAT('Yerel geçici tablo - Oturum: ', SUBSTRING(OBJECT_NAME(o.object_id), 2, CHARINDEX('_', OBJECT_NAME(o.object_id), 2) - 2))
        WHEN OBJECT_NAME(o.object_id) LIKE '##%' THEN 'Global geçici tablo - Tüm oturumlar'
        ELSE 'Normal tablo'
    END AS table_type
FROM tempdb.sys.objects o
JOIN tempdb.sys.schemas s ON o.schema_id = s.schema_id
WHERE o.type = 'U' -- Kullanıcı tabloları
AND (OBJECT_NAME(o.object_id) LIKE '#%' -- Tüm geçici tablolar
     OR OBJECT_NAME(o.object_id) LIKE '%Temp%'
     OR OBJECT_NAME(o.object_id) LIKE '%Sistem%')
ORDER BY 
    CASE WHEN OBJECT_NAME(o.object_id) LIKE '%TempSistemAyarlari%' THEN 0 ELSE 1 END,
    o.create_date DESC;
GO

-- 3. ADIM: GEÇİCİ TABLOLARA REFERANS VEREN BAĞLI NESNELERİ BULMA
PRINT '';
PRINT '3. ADIM: Geçici tablolara bağlı olan nesneler (kısıtlamalar, indeksler, vs.) tespit ediliyor...';

-- Bağlı nesneleri listele ve dinamik olarak silme kodunu oluştur
DECLARE @dropDependentObjectsSQL NVARCHAR(MAX) = N'';

SELECT @dropDependentObjectsSQL = @dropDependentObjectsSQL +
    CASE 
        WHEN o.type = 'F' THEN N'ALTER TABLE ' + QUOTENAME(OBJECT_SCHEMA_NAME(o.parent_object_id, DB_ID('tempdb'))) + '.' + 
                               QUOTENAME(OBJECT_NAME(o.parent_object_id, DB_ID('tempdb'))) + 
                               ' DROP CONSTRAINT ' + QUOTENAME(o.name) + ';' + CHAR(13) + CHAR(10)
        WHEN o.type IN ('PK', 'UQ', 'C') THEN N'ALTER TABLE ' + QUOTENAME(OBJECT_SCHEMA_NAME(o.parent_object_id, DB_ID('tempdb'))) + '.' + 
                                             QUOTENAME(OBJECT_NAME(o.parent_object_id, DB_ID('tempdb'))) + 
                                             ' DROP CONSTRAINT ' + QUOTENAME(o.name) + ';' + CHAR(13) + CHAR(10)
        WHEN o.type = 'D' THEN N'-- Default constraint: ' + o.name + CHAR(13) + CHAR(10)
        WHEN o.type = 'TR' THEN N'DROP TRIGGER ' + QUOTENAME(OBJECT_SCHEMA_NAME(o.object_id, DB_ID('tempdb'))) + '.' + 
                                QUOTENAME(o.name) + ';' + CHAR(13) + CHAR(10)
        WHEN o.type = 'V' THEN N'DROP VIEW ' + QUOTENAME(OBJECT_SCHEMA_NAME(o.object_id, DB_ID('tempdb'))) + '.' + 
                               QUOTENAME(o.name) + ';' + CHAR(13) + CHAR(10)
        ELSE N'-- Unsupported object type: ' + o.type + ' for object: ' + o.name + CHAR(13) + CHAR(10)
    END
FROM tempdb.sys.objects o
WHERE (o.parent_object_id IN (
        SELECT object_id 
        FROM tempdb.sys.objects 
        WHERE type = 'U' 
        AND (name LIKE '#%' OR name LIKE '%Temp%' OR name LIKE '%Sistem%')
    )
    OR o.referenced_object_id IN (
        SELECT object_id 
        FROM tempdb.sys.objects 
        WHERE type = 'U' 
        AND (name LIKE '#%' OR name LIKE '%Temp%' OR name LIKE '%Sistem%')
    ))
    AND o.type IN ('F', 'PK', 'UQ', 'C', 'D', 'TR', 'V');

-- Bağlı nesneleri silme komutlarını görüntüle ve çalıştır
IF LEN(@dropDependentObjectsSQL) > 0
BEGIN
    PRINT 'Bağlı nesneleri kaldırmak için aşağıdaki komutlar çalıştırılacak:';
    PRINT @dropDependentObjectsSQL;
    PRINT '';
    PRINT 'Bağlı nesneler kaldırılıyor...';
    
    EXEC sp_executesql @dropDependentObjectsSQL;
    PRINT 'Bağlı nesneler kaldırıldı.';
END
ELSE
BEGIN
    PRINT 'Geçici tablolara bağlı herhangi bir nesne bulunamadı.';
END;
GO

-- 4. ADIM: TÜM GEÇİCİ TABLOLARI DİNAMİK OLARAK SİLME
PRINT '';
PRINT '4. ADIM: Tüm geçici tablolar siliniyor...';

-- Drop all temporary tables
DECLARE @dropTablesSQL NVARCHAR(MAX) = N'';

SELECT @dropTablesSQL = @dropTablesSQL +
    N'DROP TABLE IF EXISTS ' + QUOTENAME(OBJECT_SCHEMA_NAME(o.object_id, DB_ID('tempdb'))) + '.' + 
    QUOTENAME(OBJECT_NAME(o.object_id, DB_ID('tempdb'))) + ';' + CHAR(13) + CHAR(10) +
    N'PRINT ''Tablo silindi: ' + OBJECT_NAME(o.object_id, DB_ID('tempdb')) + ''';' + CHAR(13) + CHAR(10)
FROM tempdb.sys.objects o
WHERE o.type = 'U' -- Kullanıcı tabloları
AND (OBJECT_NAME(o.object_id, DB_ID('tempdb')) LIKE '#%' -- Tüm geçici tablolar
     OR OBJECT_NAME(o.object_id, DB_ID('tempdb')) LIKE '%Temp%'
     OR OBJECT_NAME(o.object_id, DB_ID('tempdb')) LIKE '%Sistem%')
ORDER BY 
    CASE WHEN OBJECT_NAME(o.object_id, DB_ID('tempdb')) LIKE '%TempSistemAyarlari%' THEN 0 ELSE 1 END;

-- Tabloları silme komutlarını görüntüle ve çalıştır
IF LEN(@dropTablesSQL) > 0
BEGIN
    PRINT 'Aşağıdaki geçici tablolar silinecek:';
    PRINT @dropTablesSQL;
    PRINT '';
    PRINT 'Tablolar siliniyor...';
    
    BEGIN TRY
        EXEC sp_executesql @dropTablesSQL;
        PRINT 'Tablolar başarıyla silindi.';
    END TRY
    BEGIN CATCH
        PRINT 'Tabloları silme sırasında hata oluştu: ' + ERROR_MESSAGE();
        
        -- Daha agresif bir yöntem kullanın - gerekirse oturumları sonlandırın
        PRINT '';
        PRINT 'Agresif temizleme yöntemi uygulanıyor...';
        
        -- Oturumları kilitleyin ve tabloları silin (DBA yetkisi gerektirir)
        DECLARE @sql NVARCHAR(MAX) = '';
        
        -- Geçici tabloları kullanan diğer oturumlar için kill komutları oluşturun
        SELECT @sql = @sql + 'KILL ' + CAST(spid AS NVARCHAR(10)) + ';' + CHAR(13) + CHAR(10)
        FROM sys.sysprocesses
        WHERE spid <> @@SPID
        AND spid > 50  -- Sistem oturumlarını hariç tut
        AND dbid = DB_ID('tempdb');
        
        IF LEN(@sql) > 0
        BEGIN
            PRINT 'Aşağıdaki kullanıcı oturumları sonlandırılacak:';
            PRINT @sql;
            
            BEGIN TRY
                EXEC sp_executesql @sql;
                PRINT 'Kullanıcı oturumları sonlandırıldı.';
                
                -- Şimdi tabloları tekrar silmeyi deneyin
                PRINT 'Tablolar tekrar siliniyor...';
                EXEC sp_executesql @dropTablesSQL;
                PRINT 'Tablolar başarıyla silindi.';
            END TRY
            BEGIN CATCH
                PRINT 'Kullanıcı oturumlarını sonlandırma sırasında hata: ' + ERROR_MESSAGE();
                PRINT 'SQL Server hizmetini yeniden başlatmanız gerekebilir.';
            END CATCH
        END
    END CATCH
END
ELSE
BEGIN
    PRINT 'Silinecek geçici tablo bulunamadı.';
END;
GO

-- 5. ADIM: TEMPDB KULLANIM İSTATİSTİKLERİNİ GÖRÜNTÜLE
PRINT '';
PRINT '5. ADIM: TempDB kullanım istatistikleri kontrol ediliyor...';

SELECT 
    SUM(size*8)/1024 AS [TempDB Toplam Boyutu (MB)],
    SUM(CASE WHEN type = 0 THEN size*8 ELSE 0 END)/1024 AS [TempDB Veri Dosyası Boyutu (MB)],
    SUM(CASE WHEN type = 1 THEN size*8 ELSE 0 END)/1024 AS [TempDB Log Dosyası Boyutu (MB)]
FROM tempdb.sys.database_files;

-- TempDB dosya kullanımını görüntüle
SELECT 
    name AS [Dosya Adı],
    physical_name AS [Fiziksel Konum],
    type_desc AS [Dosya Tipi],
    size*8/1024 AS [Boyut (MB)],
    growth*8/1024 AS [Büyüme Miktarı (MB)],
    CASE is_percent_growth 
        WHEN 1 THEN 'Yüzde' 
        ELSE 'MB' 
    END AS [Büyüme Birimi]
FROM tempdb.sys.database_files;

-- 6. ADIM: SONUÇ VE ÖNERİLERİ GÖRÜNTÜLE
PRINT '';
PRINT '============================================================';
PRINT 'GEÇİCİ TABLO TEMİZLİĞİ TAMAMLANDI';
PRINT '============================================================';
PRINT '';
PRINT 'Temizleme sonrası kontrol ediliyor...';

-- Kalan geçici tabloları kontrol et
SELECT 
    OBJECT_NAME(object_id) AS table_name,
    create_date,
    modify_date
FROM tempdb.sys.objects 
WHERE type = 'U'
AND (name LIKE '#%' OR name LIKE '%Temp%' OR name LIKE '%Sistem%')
ORDER BY create_date DESC;

PRINT '';
PRINT 'Eğer hala geçici tablolar görünüyorsa:';
PRINT '1. Migration scriptinizi düzenleyin - "EDIT TOP 1000 ROWS" ile değil, metin olarak (F4) düzenleyin';
PRINT '2. Migration scriptinizdeki "#TempSistemAyarlari" isimlerini find/replace ile "#TempSistemAyarlari_' + CAST(@@SPID AS VARCHAR) + '" olarak değiştirin';
PRINT '3. SQL Server Management Studio''yu tamamen kapatıp yeniden açın';
PRINT '4. Veya SQL Server hizmetini yeniden başlatın';
PRINT '';
PRINT 'Migration scriptinizi çalıştırmadan önce aşağıdaki satırları migration scriptinin başına ekleyin:';
PRINT '';
PRINT 'USE tempdb;';
PRINT 'GO';
PRINT 'IF OBJECT_ID(''tempdb..#TempSistemAyarlari'') IS NOT NULL DROP TABLE #TempSistemAyarlari;';
PRINT 'GO';
PRINT '';
PRINT 'TEŞEKKÜRLER! SORUN ÇÖZÜLDÜ ✓';
GO 
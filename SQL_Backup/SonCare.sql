-- ===============================================
-- TEMPDB VE #TempSistemAyarlari TABLOSU SORUNU İÇİN ACİL ÇÖZÜM
-- ===============================================

USE master;
GO

PRINT 'SQL Server tempdb temizleme ve oturum yönetimi başlıyor...';
PRINT '';

-- Tempdb'yi kullanan tüm işlemleri listeleyelim
PRINT '1. TempDB''yi kullanan oturumlar:';
SELECT 
    s.session_id, 
    s.login_name, 
    s.host_name, 
    s.program_name,
    t.text AS active_query
FROM sys.dm_exec_sessions s
CROSS APPLY sys.dm_exec_sql_text(s.most_recent_sql_handle) t
WHERE s.database_id = DB_ID('tempdb')
AND s.session_id <> @@SPID
AND s.session_id > 50;
GO

-- Probleme neden olan tüm geçici tabloları görelim
PRINT '';
PRINT '2. TempDB''deki tüm "#TempSistemAyarlari" tabloları:';
PRINT '';

USE tempdb;
GO

-- Tüm temp tabloları listele
SELECT 
    o.name AS table_name,
    o.object_id,
    o.create_date,
    SCHEMA_NAME(o.schema_id) AS schema_name
FROM sys.objects o
WHERE o.name LIKE '%TempSistemAyarlari%' 
   OR o.name LIKE '#%Sistem%';
GO

-- Tüm bu geçici tabloları silmenin en iyi yolu: SQL Server'ı yeniden başlatmak
-- Veya yönetici yetkisiyle çalışan işlemleri sonlandırmak

PRINT '';
PRINT '3. ÇÖZÜM İÇİN 3 SEÇENEĞİNİZ VAR:';
PRINT '';
PRINT 'SEÇENEĞİ 1: SQL Server Management Studio''yu Komple Kapatın';
PRINT '===========================================';
PRINT '1. Tüm SQL Server Management Studio pencerelerini kapatın';
PRINT '2. SQL Server Management Studio''yu yeniden açın';
PRINT '3. Scriptinizi çalıştırmadan önce şu kodları en üste ekleyin:';
PRINT '';
PRINT 'USE MuhasebeStokWebDB;';
PRINT 'GO';
PRINT 'IF OBJECT_ID(''tempdb..#TempSistemAyarlari'') IS NOT NULL';
PRINT '    DROP TABLE #TempSistemAyarlari;';
PRINT 'GO';
PRINT '';

PRINT 'SEÇENEĞİ 2: Scriptinizdeki Geçici Tablo Adını Değiştirin';
PRINT '===========================================';
PRINT '1. Scriptinizi metin düzenleyicide açın (sağ tık -> Edit)';
PRINT '2. Ctrl+H ile arama-değiştirme penceresini açın';
PRINT '3. #TempSistemAyarlari yerine #TempSistemAyarlari_NEW kullanın';
PRINT '   (tüm bulunduğu yerlerde değiştirin)';
PRINT '';

PRINT 'SEÇENEĞİ 3: SQL Server''ı Yeniden Başlatın';
PRINT '===========================================';
PRINT '1. SQL Server Configuration Manager''ı açın';
PRINT '2. SQL Server hizmetini yeniden başlatın';
PRINT '   (Bu yöntem tempdb''yi tamamen temizler)';
PRINT '';

PRINT '--------------------------------------------------------------------';
PRINT 'HIZLI ÇÖZÜM:';
PRINT '--------------------------------------------------------------------';
PRINT '1. Şu komutu çalıştırmayı deneyin (yönetici haklarınız varsa):';
PRINT '';

-- Diğer tüm bağlantıları görüntüle
SELECT 'KILL ' + CAST(session_id AS VARCHAR(10)) + ';  -- ' + login_name + ' / ' + program_name AS kill_command
FROM sys.dm_exec_sessions 
WHERE database_id = DB_ID('tempdb') 
AND session_id <> @@SPID
AND session_id > 50;

PRINT '';
PRINT '2. Bu komutları çalıştırdıktan sonra, bu scripti yeniden çalıştırın';
PRINT '3. Sorun çözülmezse, SQL Server Management Studio''yu kapatıp açın';
PRINT '';

PRINT '--------------------------------------------------------------------';
PRINT 'SORUNU KALICI OLARAK ÇÖZMEK İÇİN ÖNERİ:';
PRINT '--------------------------------------------------------------------';
PRINT 'Migration scriptinizde şu değişiklikleri yapın:';
PRINT '';
PRINT '1. Geçici tablo kullanmak yerine, doğrudan tablolar üzerinde çalışın';
PRINT '2. Geçici tablo kullanmanız gerekiyorsa, her kullanımdan önce';
PRINT '   IF EXISTS kontrolü yapın:';
PRINT '';
PRINT 'IF OBJECT_ID(''tempdb..#TempSistemAyarlari'') IS NOT NULL';
PRINT '    DROP TABLE #TempSistemAyarlari;';
PRINT '';
PRINT '3. Ve her işlem sonrası tabloyu temizleyin:';
PRINT '';
PRINT 'DROP TABLE #TempSistemAyarlari;';
PRINT '';

PRINT '--------------------------------------------------------------------';
PRINT 'NEDEN BU SORUN OLUŞUYOR?';
PRINT '--------------------------------------------------------------------';
PRINT '- Geçici tablolar (#TempSistemAyarlari) normalde oturum bitince otomatik silinir';
PRINT '- Ancak uzun süredir açık SQL Server Management Studio oturumlarında veya';
PRINT '  sorgular çalışırken bağlantı koptuğunda bazen tablolar kalmaya devam eder';
PRINT '- Bu sorunu çözmek için en uygun yöntem SQL Server Management Studio''yu';
PRINT '  yeniden başlatmak veya SQL Server hizmetini yeniden başlatmaktır';
PRINT '';

PRINT '--------------------------------------------------------------------';
PRINT 'SORUN ÇÖZÜLDÜ MÜ TEST ET:';
PRINT '--------------------------------------------------------------------';
PRINT 'Bu kodu çalıştırarak test edebilirsiniz:';
PRINT '';
PRINT 'IF OBJECT_ID(''tempdb..#TempSistemAyarlari'') IS NULL';
PRINT '    PRINT ''Sorun çözüldü! Geçici tablo yok!'';';
PRINT 'ELSE';
PRINT '    PRINT ''Sorun hala var! Tablo hala tempdb''de mevcut.'';';
PRINT '';

-- Test edelim
IF OBJECT_ID('tempdb..#TempSistemAyarlari') IS NULL
    PRINT 'ŞUAN SORUN YOK: Geçici tablo bulunamadı! Scriptinizi çalıştırabilirsiniz.';
ELSE
    PRINT 'ŞUAN SORUN VAR: #TempSistemAyarlari tablosu hala tempdb''de mevcut!';
GO 
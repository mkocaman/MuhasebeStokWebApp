-- SQL Script to fix '#TempSistemAyarlari' already exists error

-- First, check if the temporary table exists and drop it
IF OBJECT_ID('tempdb..#TempSistemAyarlari') IS NOT NULL
BEGIN
    DROP TABLE #TempSistemAyarlari
END
GO

-- Now your original script can create the temporary table
-- Bu satırdan sonra orijinal script devam edebilir

-- Not: Bu betiği SQL Server Management Studio'da, asıl betikten önce çalıştırın
-- Geçici tablolar oturum sonunda zaten silinir, ancak bir hata durumunda veya 
-- oturum düzgün kapanmadıysa, geçici tablolar bazen kalabilir 
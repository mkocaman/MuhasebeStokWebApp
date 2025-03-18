-- DovizKurlari tablosunu kontrol et, yoksa oluştur
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DovizKurlari]') AND type in (N'U'))
BEGIN
    CREATE TABLE [DovizKurlari] (
        [DovizKuruID] uniqueidentifier NOT NULL,
        [KaynakParaBirimi] nvarchar(3) NOT NULL,
        [KaynakParaBirimiID] uniqueidentifier NOT NULL,
        [HedefParaBirimi] nvarchar(3) NOT NULL,
        [HedefParaBirimiID] uniqueidentifier NOT NULL,
        [KurDegeri] decimal(18,6) NOT NULL,
        [AlisFiyati] decimal(18,6) NULL,
        [SatisFiyati] decimal(18,6) NULL,
        [Tarih] datetime2 NOT NULL,
        [Kaynak] nvarchar(100) NOT NULL,
        [Aciklama] nvarchar(500) NULL,
        [Aktif] bit NOT NULL DEFAULT CAST(1 AS bit),
        [SoftDelete] bit NOT NULL DEFAULT CAST(0 AS bit),
        [OlusturmaTarihi] datetime2 NOT NULL DEFAULT (GETDATE()),
        [GuncellemeTarihi] datetime2 NULL,
        CONSTRAINT [PK_DovizKurlari] PRIMARY KEY ([DovizKuruID])
    );
    
    PRINT 'DovizKurlari tablosu oluşturuldu.';
    
    -- Foreign key'leri ekle (eğer Dovizler tablosu varsa)
    IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Dovizler]') AND type in (N'U'))
    BEGIN
        ALTER TABLE [DovizKurlari] ADD CONSTRAINT [FK_DovizKurlari_Dovizler_KaynakParaBirimiID] 
            FOREIGN KEY ([KaynakParaBirimiID]) REFERENCES [Dovizler] ([DovizID]) ON DELETE NO ACTION;
            
        ALTER TABLE [DovizKurlari] ADD CONSTRAINT [FK_DovizKurlari_Dovizler_HedefParaBirimiID] 
            FOREIGN KEY ([HedefParaBirimiID]) REFERENCES [Dovizler] ([DovizID]) ON DELETE NO ACTION;
            
        PRINT 'DovizKurlari tablosuna Dovizler tablosu ile foreign key ilişkileri eklendi.';
    END
    ELSE
    BEGIN
        PRINT 'Dovizler tablosu bulunamadığı için foreign key ilişkileri eklenemedi.';
    END
END
ELSE
BEGIN
    PRINT 'DovizKurlari tablosu zaten var.';
    
    -- Eğer tablo var ama foreign key ilişkileri yoksa ekleyelim
    IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'FK_DovizKurlari_Dovizler_KaynakParaBirimiID'))
       AND EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Dovizler]') AND type in (N'U'))
    BEGIN
        -- KaynakParaBirimiID sütunu yoksa ekle
        IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[DovizKurlari]') AND name = 'KaynakParaBirimiID')
        BEGIN
            ALTER TABLE [DovizKurlari] ADD [KaynakParaBirimiID] uniqueidentifier NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';
            
            DECLARE @defaultConstraintName1 nvarchar(200);
            SELECT @defaultConstraintName1 = name FROM sys.default_constraints 
            WHERE parent_object_id = OBJECT_ID(N'[dbo].[DovizKurlari]') 
            AND parent_column_id = (SELECT column_id FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[DovizKurlari]') AND name = 'KaynakParaBirimiID');
            
            IF @defaultConstraintName1 IS NOT NULL
                EXEC('ALTER TABLE [DovizKurlari] DROP CONSTRAINT ' + @defaultConstraintName1);
        END
        
        ALTER TABLE [DovizKurlari] ADD CONSTRAINT [FK_DovizKurlari_Dovizler_KaynakParaBirimiID] 
            FOREIGN KEY ([KaynakParaBirimiID]) REFERENCES [Dovizler] ([DovizID]) ON DELETE NO ACTION;
            
        PRINT 'KaynakParaBirimiID foreign key ilişkisi eklendi.';
    END
    
    IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'FK_DovizKurlari_Dovizler_HedefParaBirimiID'))
       AND EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Dovizler]') AND type in (N'U'))
    BEGIN
        -- HedefParaBirimiID sütunu yoksa ekle
        IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[DovizKurlari]') AND name = 'HedefParaBirimiID')
        BEGIN
            ALTER TABLE [DovizKurlari] ADD [HedefParaBirimiID] uniqueidentifier NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';
            
            DECLARE @defaultConstraintName2 nvarchar(200);
            SELECT @defaultConstraintName2 = name FROM sys.default_constraints 
            WHERE parent_object_id = OBJECT_ID(N'[dbo].[DovizKurlari]') 
            AND parent_column_id = (SELECT column_id FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[DovizKurlari]') AND name = 'HedefParaBirimiID');
            
            IF @defaultConstraintName2 IS NOT NULL
                EXEC('ALTER TABLE [DovizKurlari] DROP CONSTRAINT ' + @defaultConstraintName2);
        END
        
        ALTER TABLE [DovizKurlari] ADD CONSTRAINT [FK_DovizKurlari_Dovizler_HedefParaBirimiID] 
            FOREIGN KEY ([HedefParaBirimiID]) REFERENCES [Dovizler] ([DovizID]) ON DELETE NO ACTION;
            
        PRINT 'HedefParaBirimiID foreign key ilişkisi eklendi.';
    END
END 
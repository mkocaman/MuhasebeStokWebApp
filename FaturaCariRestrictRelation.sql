-- Mevcut foreign key kısıtlamasını bul
IF EXISTS (
    SELECT * FROM sys.foreign_keys 
    WHERE object_id = OBJECT_ID(N'[FK_Faturalar_Cariler_CariID]') 
    AND parent_object_id = OBJECT_ID(N'[Faturalar]')
)
BEGIN
    -- Mevcut foreign key kısıtlamasını kaldır
    ALTER TABLE [Faturalar] DROP CONSTRAINT [FK_Faturalar_Cariler_CariID];
END

-- Restrict davranışı ile yeni foreign key kısıtlaması ekle
ALTER TABLE [Faturalar] ADD CONSTRAINT [FK_Faturalar_Cariler_CariID]
    FOREIGN KEY ([CariID]) REFERENCES [Cariler] ([CariID])
    ON DELETE NO ACTION; -- SQL Server'da NO ACTION, EF Core'daki Restrict'e karşılık gelir 
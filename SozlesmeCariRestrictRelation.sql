-- Mevcut foreign key kısıtlamasını kaldır
ALTER TABLE [Sozlesmeler] DROP CONSTRAINT [FK_Sozlesmeler_Cariler_CariID];

-- Restrict davranışı ile yeni foreign key kısıtlaması ekle
ALTER TABLE [Sozlesmeler] ADD CONSTRAINT [FK_Sozlesmeler_Cariler_CariID]
    FOREIGN KEY ([CariID]) REFERENCES [Cariler] ([CariID])
    ON DELETE NO ACTION; -- SQL Server'da NO ACTION, EF Core'daki Restrict'e karşılık gelir 
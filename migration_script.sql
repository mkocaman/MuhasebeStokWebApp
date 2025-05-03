BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250424174026_FixSistemLogsAndCurrencyUpdates'
)
BEGIN
    DECLARE @var sysname;
    SELECT @var = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[SistemLoglar]') AND [c].[name] = N'KullaniciID');
    IF @var IS NOT NULL EXEC(N'ALTER TABLE [SistemLoglar] DROP CONSTRAINT [' + @var + '];');
    ALTER TABLE [SistemLoglar] DROP COLUMN [KullaniciID];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250424174026_FixSistemLogsAndCurrencyUpdates'
)
BEGIN 
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250424174026_FixSistemLogsAndCurrencyUpdates', N'9.0.4');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250429064427_FixSistemLogKullaniciIdType'
)
BEGIN
    -- DROP INDEX [IX_SistemLoglar_KullaniciId] ON [SistemLoglar]; -- Removed/Commented out as it causes errors
    DECLARE @var1 sysname;
    SELECT @var1 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[SistemLoglar]') AND [c].[name] = N'KullaniciId');
    IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [SistemLoglar] DROP CONSTRAINT [' + @var1 + '];');
    ALTER TABLE [SistemLoglar] ALTER COLUMN [KullaniciId] nvarchar(450) NULL;
    -- CREATE INDEX [IX_SistemLoglar_KullaniciId] ON [SistemLoglar] ([KullaniciId]); -- Removed/Commented out as it corresponds to the problematic DROP
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250429064427_FixSistemLogKullaniciIdType'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250429064427_FixSistemLogKullaniciIdType', N'9.0.4');
END;

COMMIT;

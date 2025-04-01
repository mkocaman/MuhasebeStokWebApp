using System;
using System.Threading.Tasks;

namespace MuhasebeStokWebApp.Services.Interfaces
{
    public interface IDbMigrationService
    {
        Task<bool> ApplyMigrationsAsync();
        Task<bool> RunSqlScriptAsync(string scriptPath);
        Task<bool> BackupDatabaseAsync(string backupPath);
        Task<bool> RestoreDatabaseAsync(string backupPath);
    }
} 
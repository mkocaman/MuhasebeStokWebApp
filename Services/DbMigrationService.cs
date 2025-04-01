using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MuhasebeStokWebApp.Data;
using MuhasebeStokWebApp.Services.Interfaces;

namespace MuhasebeStokWebApp.Services
{
    public class DbMigrationService : IDbMigrationService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<DbMigrationService> _logger;
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogService _logService;

        public DbMigrationService(
            IConfiguration configuration,
            ILogger<DbMigrationService> logger,
            ApplicationDbContext dbContext,
            ILogService logService)
        {
            _configuration = configuration;
            _logger = logger;
            _dbContext = dbContext;
            _logService = logService;
        }

        public async Task<bool> ApplyMigrationsAsync()
        {
            try
            {
                await _dbContext.Database.MigrateAsync();
                await _logService.LogInfoAsync("Veritabanı migrasyonları başarıyla uygulandı.", "DbMigrationService/ApplyMigrationsAsync");
                return true;
            }
            catch (Exception ex)
            {
                await _logService.LogErrorAsync("Veritabanı migrasyonları uygulanırken hata oluştu.", ex);
                return false;
            }
        }

        public async Task<bool> RunSqlScriptAsync(string scriptPath)
        {
            if (!File.Exists(scriptPath))
            {
                await _logService.LogWarningAsync($"SQL script dosyası bulunamadı: {scriptPath}", "DbMigrationService/RunSqlScriptAsync");
                return false;
            }

            try
            {
                var connectionString = _configuration.GetConnectionString("DefaultConnection");
                var script = File.ReadAllText(scriptPath);

                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    
                    // Her bir SQL komutunu ayrı ayrı çalıştır
                    var commandTexts = script.Split(new[] { "GO", "go" }, StringSplitOptions.RemoveEmptyEntries);
                    
                    foreach (var commandText in commandTexts)
                    {
                        if (string.IsNullOrWhiteSpace(commandText))
                            continue;

                        using (var command = new SqlCommand(commandText, connection))
                        {
                            await command.ExecuteNonQueryAsync();
                        }
                    }
                }

                await _logService.LogInfoAsync($"SQL script başarıyla çalıştırıldı: {scriptPath}", "DbMigrationService/RunSqlScriptAsync");
                return true;
            }
            catch (Exception ex)
            {
                await _logService.LogErrorAsync($"SQL script çalıştırılırken hata oluştu: {scriptPath}", ex);
                return false;
            }
        }

        public async Task<bool> BackupDatabaseAsync(string backupPath)
        {
            try
            {
                var connectionString = _configuration.GetConnectionString("DefaultConnection");
                var databaseName = _dbContext.Database.GetDbConnection().Database;
                
                if (string.IsNullOrEmpty(backupPath))
                {
                    backupPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Backups", $"{databaseName}_{DateTime.Now:yyyyMMdd_HHmmss}.bak");
                    
                    // Dizin yoksa oluştur
                    Directory.CreateDirectory(Path.GetDirectoryName(backupPath));
                }

                var backupCommand = $"BACKUP DATABASE [{databaseName}] TO DISK = '{backupPath}' WITH FORMAT, INIT, NAME = '{databaseName}-Full Database Backup', SKIP, NOREWIND, NOUNLOAD, STATS = 10";

                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    
                    using (var command = new SqlCommand(backupCommand, connection))
                    {
                        await command.ExecuteNonQueryAsync();
                    }
                }

                await _logService.LogInfoAsync($"Veritabanı yedeği başarıyla oluşturuldu: {backupPath}", "DbMigrationService/BackupDatabaseAsync");
                return true;
            }
            catch (Exception ex)
            {
                await _logService.LogErrorAsync("Veritabanı yedeği oluşturulurken hata oluştu.", ex);
                return false;
            }
        }

        public async Task<bool> RestoreDatabaseAsync(string backupPath)
        {
            if (!File.Exists(backupPath))
            {
                await _logService.LogWarningAsync($"Yedek dosyası bulunamadı: {backupPath}", "DbMigrationService/RestoreDatabaseAsync");
                return false;
            }

            try
            {
                var connectionString = _configuration.GetConnectionString("DefaultConnection");
                var databaseName = _dbContext.Database.GetDbConnection().Database;
                
                // Önce veritabanı bağlantılarını kapat (single user mode)
                var singleUserCommand = $"ALTER DATABASE [{databaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE";
                
                // Veritabanı geri yükleme komutu
                var restoreCommand = $"RESTORE DATABASE [{databaseName}] FROM DISK = '{backupPath}' WITH REPLACE, RECOVERY";
                
                // Geri yükleme sonrası veritabanını multi user mode'a al
                var multiUserCommand = $"ALTER DATABASE [{databaseName}] SET MULTI_USER";

                using (var connection = new SqlConnection(connectionString.Replace(databaseName, "master")))
                {
                    await connection.OpenAsync();
                    
                    // Single user mode
                    using (var command = new SqlCommand(singleUserCommand, connection))
                    {
                        await command.ExecuteNonQueryAsync();
                    }
                    
                    // Restore
                    using (var command = new SqlCommand(restoreCommand, connection))
                    {
                        await command.ExecuteNonQueryAsync();
                    }
                    
                    // Multi user mode
                    using (var command = new SqlCommand(multiUserCommand, connection))
                    {
                        await command.ExecuteNonQueryAsync();
                    }
                }

                await _logService.LogInfoAsync($"Veritabanı yedeği başarıyla geri yüklendi: {backupPath}", "DbMigrationService/RestoreDatabaseAsync");
                return true;
            }
            catch (Exception ex)
            {
                await _logService.LogErrorAsync("Veritabanı yedeği geri yüklenirken hata oluştu.", ex);
                return false;
            }
        }

        /// <summary>
        /// Para birimleri oluşturur
        /// </summary>
        private async Task SeedParaBirimleriAsync(ApplicationDbContext context)
        {
            if (!context.ParaBirimleri.Any())
            {
                _logger.LogInformation("Para birimleri oluşturuluyor...");
                
                // Özbekistan Somu (UZS) - Ana para birimi olarak ekle
                var uzs = Data.Entities.ParaBirimiModulu.ParaBirimi.CreateUzbekistanSomu();
                context.ParaBirimleri.Add(uzs);
                
                // ABD Doları (USD) ekle
                var usd = Data.Entities.ParaBirimiModulu.ParaBirimi.CreateUSDolar();
                context.ParaBirimleri.Add(usd);
                
                // TL para birimini ekle
                context.ParaBirimleri.Add(new Data.Entities.ParaBirimiModulu.ParaBirimi
                {
                    ParaBirimiID = Guid.NewGuid(),
                    Kod = "TRY",
                    Ad = "Türk Lirası",
                    Sembol = "₺",
                    Aciklama = "Türkiye Cumhuriyeti'nin resmi para birimi",
                    AnaParaBirimiMi = false,
                    Aktif = true,
                    Silindi = false,
                    Sira = 3,
                    OlusturmaTarihi = DateTime.Now,
                    OlusturanKullaniciID = "Sistem",
                    SonGuncelleyenKullaniciID = "Sistem"
                });
                
                // Euro para birimini ekle
                context.ParaBirimleri.Add(new Data.Entities.ParaBirimiModulu.ParaBirimi
                {
                    ParaBirimiID = Guid.NewGuid(),
                    Kod = "EUR",
                    Ad = "Euro",
                    Sembol = "€",
                    Aciklama = "Avrupa Birliği'nin ortak para birimi",
                    AnaParaBirimiMi = false,
                    Aktif = true,
                    Silindi = false,
                    Sira = 4,
                    OlusturmaTarihi = DateTime.Now,
                    OlusturanKullaniciID = "Sistem",
                    SonGuncelleyenKullaniciID = "Sistem"
                });
                
                // İngiliz Sterlini para birimini ekle
                context.ParaBirimleri.Add(new Data.Entities.ParaBirimiModulu.ParaBirimi
                {
                    ParaBirimiID = Guid.NewGuid(),
                    Kod = "GBP",
                    Ad = "İngiliz Sterlini",
                    Sembol = "£",
                    Aciklama = "Birleşik Krallık'ın resmi para birimi",
                    AnaParaBirimiMi = false,
                    Aktif = true,
                    Silindi = false,
                    Sira = 5,
                    OlusturmaTarihi = DateTime.Now,
                    OlusturanKullaniciID = "Sistem",
                    SonGuncelleyenKullaniciID = "Sistem"
                });

                await context.SaveChangesAsync();
                
                // Para birimleri arasındaki ilişkileri oluştur
                // USD <-> UZS
                context.ParaBirimiIliskileri.Add(new Data.Entities.ParaBirimiModulu.ParaBirimiIliski
                {
                    ParaBirimiIliskiID = Guid.NewGuid(),
                    KaynakParaBirimiID = usd.ParaBirimiID,
                    HedefParaBirimiID = uzs.ParaBirimiID,
                    Aktif = true,
                    Silindi = false,
                    Aciklama = "USD -> UZS dönüşümü",
                    OlusturmaTarihi = DateTime.Now,
                    OlusturanKullaniciID = "Sistem",
                    SonGuncelleyenKullaniciID = "Sistem"
                });
                
                // UZS <-> USD
                context.ParaBirimiIliskileri.Add(new Data.Entities.ParaBirimiModulu.ParaBirimiIliski
                {
                    ParaBirimiIliskiID = Guid.NewGuid(),
                    KaynakParaBirimiID = uzs.ParaBirimiID,
                    HedefParaBirimiID = usd.ParaBirimiID,
                    Aktif = true,
                    Silindi = false,
                    Aciklama = "UZS -> USD dönüşümü",
                    OlusturmaTarihi = DateTime.Now,
                    OlusturanKullaniciID = "Sistem",
                    SonGuncelleyenKullaniciID = "Sistem"
                });
                
                await context.SaveChangesAsync();
                _logger.LogInformation("Para birimleri başarıyla oluşturuldu.");
            }
        }

        /// <summary>
        /// Veritabanını başlatır ve gerekli seed verilerini oluşturur
        /// </summary>
        public async Task InitializeAsync()
        {
            try
            {
                _logger.LogInformation("Veritabanı başlatılıyor...");
                
                // Migrasyonları uygula
                await ApplyMigrationsAsync();
                
                // Seed işlemlerini yap
                await _logService.LogInfoAsync("Seed işlemleri başlatılıyor...", "DbMigrationService/InitializeAsync");
                
                // Para birimleri oluştur
                await SeedParaBirimleriAsync(_dbContext);
                
                await _logService.LogInfoAsync("Seed işlemleri tamamlandı.", "DbMigrationService/InitializeAsync");
            }
            catch (Exception ex)
            {
                await _logService.LogErrorAsync("Veritabanı başlatılırken bir hata oluştu.", ex);
                throw;
            }
        }
    }
} 
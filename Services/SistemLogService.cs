using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MuhasebeStokWebApp.Data;
using MuhasebeStokWebApp.Data.Entities;
using MuhasebeStokWebApp.Services.CustomExceptions;

namespace MuhasebeStokWebApp.Services
{
    /// <summary>
    /// Sistem log işlemlerini yöneten servis sınıfı.
    /// </summary>
    public class SistemLogService : ISistemLogService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<SistemLogService> _logger;
        
        /// <summary>
        /// Log seviyesi
        /// </summary>
        public enum LogLevel
        {
            /// <summary>
            /// Bilgi seviyesindeki loglar
            /// </summary>
            Information = 0,
            
            /// <summary>
            /// Uyarı seviyesindeki loglar
            /// </summary>
            Warning = 1,
            
            /// <summary>
            /// Hata seviyesindeki loglar
            /// </summary>
            Error = 2,
            
            /// <summary>
            /// Kritik hata seviyesindeki loglar
            /// </summary>
            Critical = 3
        }
        
        public SistemLogService(
            ApplicationDbContext context, 
            IHttpContextAccessor httpContextAccessor, 
            ILogger<SistemLogService> logger)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }
        
        /// <summary>
        /// Yeni bir sistem log kaydı ekler
        /// </summary>
        /// <param name="islemTuru">İşlem Türü</param>
        /// <param name="kayitID">İlgili kaydın ID'si</param>
        /// <param name="aciklama">Açıklama</param>
        /// <param name="basarili">İşlem başarılı mı?</param>
        /// <returns>true: Başarılı, false: Başarısız</returns>
        public async Task<bool> LogEkleAsync(string islemTuru, object kayitID, string aciklama, bool basarili = true)
        {
            return await InternalLogEkleAsync(islemTuru, kayitID, aciklama, basarili, null, 
                basarili ? LogLevel.Information : LogLevel.Warning);
        }
        
        /// <summary>
        /// Yeni bir sistem log kaydı ekler (hata ile birlikte)
        /// </summary>
        /// <param name="islemTuru">İşlem Türü</param>
        /// <param name="kayitID">İlgili kaydın ID'si</param>
        /// <param name="aciklama">Açıklama</param>
        /// <param name="hataMesaji">Hata mesajı</param>
        /// <returns>true: Başarılı, false: Başarısız</returns>
        public async Task<bool> LogHataEkleAsync(string islemTuru, object kayitID, string aciklama, string hataMesaji)
        {
            return await InternalLogEkleAsync(islemTuru, kayitID, aciklama, false, hataMesaji, LogLevel.Error);
        }
        
        /// <summary>
        /// Yeni bir sistem log kaydı ekler (exception ile birlikte)
        /// </summary>
        /// <param name="islemTuru">İşlem Türü</param>
        /// <param name="kayitID">İlgili kaydın ID'si</param>
        /// <param name="aciklama">Açıklama</param>
        /// <param name="ex">Exception</param>
        /// <returns>true: Başarılı, false: Başarısız</returns>
        public async Task<bool> LogExceptionEkleAsync(string islemTuru, object kayitID, string aciklama, Exception ex)
        {
            // Exception tipine göre log seviyesini belirle
            LogLevel logLevel = LogLevel.Error;
            
            if (ex is BusinessException busEx)
            {
                // BusinessException için severity bilgisine göre log seviyesini belirle
                logLevel = busEx.Severity switch
                {
                    ErrorSeverity.Info => LogLevel.Information,
                    ErrorSeverity.Warning => LogLevel.Warning,
                    ErrorSeverity.Error => LogLevel.Error,
                    ErrorSeverity.Critical => LogLevel.Critical,
                    _ => LogLevel.Error
                };
            }
            else if (ex is ValidationException)
            {
                // Validation hataları genellikle Warning seviyesindedir
                logLevel = LogLevel.Warning;
            }
            else if (ex is DataException dataEx)
            {
                // Veritabanı hataları genellikle Error seviyesindedir
                logLevel = LogLevel.Error;
                
                // Bağlantı hataları ve diğer kritik veritabanı hataları için Critical seviyesi
                if (dataEx.ErrorType == DataErrorType.Connection || 
                    dataEx.ErrorType == DataErrorType.Transaction)
                {
                    logLevel = LogLevel.Critical;
                }
            }
            
            string hataMesaji = ex.Message;
            if (ex.InnerException != null)
            {
                hataMesaji += $" (Inner: {ex.InnerException.Message})";
            }
            
            // Stack trace'i loga ekle (çok uzunsa kısalt)
            string stackTrace = ex.StackTrace;
            if (!string.IsNullOrEmpty(stackTrace) && stackTrace.Length > 500)
            {
                stackTrace = stackTrace.Substring(0, 500) + "...";
            }
            
            if (!string.IsNullOrEmpty(stackTrace))
            {
                hataMesaji += $" | Stack: {stackTrace}";
            }
            
            return await InternalLogEkleAsync(islemTuru, kayitID, aciklama, false, hataMesaji, logLevel);
        }
        
        /// <summary>
        /// Yeni bir kritik hata log kaydı ekler
        /// </summary>
        /// <param name="islemTuru">İşlem Türü</param>
        /// <param name="kayitID">İlgili kaydın ID'si</param>
        /// <param name="aciklama">Açıklama</param>
        /// <param name="hataMesaji">Hata mesajı</param>
        /// <returns>true: Başarılı, false: Başarısız</returns>
        public async Task<bool> LogKritikHataEkleAsync(string islemTuru, object kayitID, string aciklama, string hataMesaji)
        {
            return await InternalLogEkleAsync(islemTuru, kayitID, aciklama, false, hataMesaji, LogLevel.Critical);
        }
        
        /// <summary>
        /// Yeni bir uyarı log kaydı ekler
        /// </summary>
        /// <param name="islemTuru">İşlem Türü</param>
        /// <param name="kayitID">İlgili kaydın ID'si</param>
        /// <param name="aciklama">Açıklama</param>
        /// <returns>true: Başarılı, false: Başarısız</returns>
        public async Task<bool> LogUyariEkleAsync(string islemTuru, object kayitID, string aciklama)
        {
            return await InternalLogEkleAsync(islemTuru, kayitID, aciklama, true, null, LogLevel.Warning);
        }
        
        /// <summary>
        /// Sistem loglarını listeler
        /// </summary>
        /// <param name="filtreBasarili">Başarılı olanları filtrele (null: tümü)</param>
        /// <param name="islemTuru">İşlem türü filtresi (null: tümü)</param>
        /// <param name="baslangicTarihi">Başlangıç tarihi</param>
        /// <param name="bitisTarihi">Bitiş tarihi</param>
        /// <param name="kayitID">Kayıt ID filtresi (null: tümü)</param>
        /// <returns>SistemLog listesi</returns>
        public async Task<List<SistemLog>> GetLogsAsync(bool? filtreBasarili = null, string islemTuru = null, 
            DateTime? baslangicTarihi = null, DateTime? bitisTarihi = null, object kayitID = null)
        {
            try
            {
                var query = _context.SistemLoglar.AsQueryable();
                
                // Filtreleri uygula
                if (filtreBasarili.HasValue)
                {
                    query = query.Where(l => l.Basarili == filtreBasarili.Value);
                }
                
                if (!string.IsNullOrEmpty(islemTuru))
                {
                    query = query.Where(l => l.IslemTuru == islemTuru);
                }
                
                if (baslangicTarihi.HasValue)
                {
                    query = query.Where(l => l.IslemTarihi >= baslangicTarihi.Value);
                }
                
                if (bitisTarihi.HasValue)
                {
                    query = query.Where(l => l.IslemTarihi <= bitisTarihi.Value);
                }
                
                if (kayitID != null)
                {
                    // KayitID Guid ise
                    if (kayitID is Guid guidId)
                    {
                        query = query.Where(l => l.KayitID == guidId);
                    }
                    // KayitID string ise
                    else if (kayitID is string strId)
                    {
                        // Guid olarak çevrilebilir mi?
                        if (Guid.TryParse(strId, out Guid parsedId))
                        {
                            query = query.Where(l => l.KayitID == parsedId);
                        }
                    }
                }
                
                // Tarihe göre azalan sırayla getir
                return await query.OrderByDescending(l => l.IslemTarihi).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sistem logları getirilirken hata oluştu");
                return new List<SistemLog>();
            }
        }
        
        /// <summary>
        /// Log kaydı eklemek için iç metot
        /// </summary>
        private async Task<bool> InternalLogEkleAsync(string islemTuru, object kayitID, string aciklama, 
            bool basarili, string hataMesaji, LogLevel logLevel)
        {
            try
            {
                // Kullanıcı ID'sini al
                string kullaniciId = GetCurrentUserId();
                
                // KayitID'yi çözümle
                Guid? kayitGuid = null;
                
                if (kayitID != null)
                {
                    if (kayitID is Guid guid)
                    {
                        kayitGuid = guid;
                    }
                    else if (kayitID is string stringId && Guid.TryParse(stringId, out Guid parsedGuid))
                    {
                        kayitGuid = parsedGuid;
                    }
                }
                
                // Tarayıcı ve IP bilgisi
                string tarayici = _httpContextAccessor.HttpContext?.Request.Headers["User-Agent"].ToString() ?? "Bilinmiyor";
                string ipAdresi = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "Bilinmiyor";
                
                // Log seviyesine göre loglama yap
                switch (logLevel)
                {
                    case LogLevel.Information:
                        _logger.LogInformation(
                            $"[{islemTuru}] Kullanıcı: {kullaniciId}, Kayıt: {kayitID}, Açıklama: {aciklama}");
                        break;
                    case LogLevel.Warning:
                        _logger.LogWarning(
                            $"[{islemTuru}] Kullanıcı: {kullaniciId}, Kayıt: {kayitID}, Açıklama: {aciklama}, Hata: {hataMesaji}");
                        break;
                    case LogLevel.Error:
                    case LogLevel.Critical:
                        _logger.LogError(
                            $"[{islemTuru}] Kullanıcı: {kullaniciId}, Kayıt: {kayitID}, Açıklama: {aciklama}, Hata: {hataMesaji}");
                        break;
                }
                
                // Yeni log nesnesi oluştur
                var yeniLog = new SistemLog
                {
                    ID = Guid.NewGuid(),
                    IslemTuru = islemTuru,
                    KayitID = kayitGuid,
                    Aciklama = aciklama,
                    IslemTarihi = DateTime.Now,
                    KullaniciId = kullaniciId,
                    IP = ipAdresi,
                    Tarayici = tarayici,
                    Basarili = basarili,
                    HataMesaji = hataMesaji
                };
                
                // Veritabanına ekle
                await _context.SistemLoglar.AddAsync(yeniLog);
                await _context.SaveChangesAsync();
                
                return true;
            }
            catch (Exception ex)
            {
                // Burada log servisi çağrılamaz (sonsuz döngüye girer)
                _logger.LogError(ex, "Log kaydı eklenirken hata oluştu: {Message}", ex.Message);
                return false;
            }
        }
        
        /// <summary>
        /// Mevcut kullanıcının ID'sini alır
        /// </summary>
        private string GetCurrentUserId()
        {
            if (_httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated == true)
            {
                var userId = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                return userId ?? "Anonim";
            }
            
            return "Anonim";
        }
    }
    
    /// <summary>
    /// Sistem log servisi için interface
    /// </summary>
    public interface ISistemLogService
    {
        /// <summary>
        /// Yeni bir sistem log kaydı ekler
        /// </summary>
        Task<bool> LogEkleAsync(string islemTuru, object kayitID, string aciklama, bool basarili = true);
        
        /// <summary>
        /// Yeni bir sistem log kaydı ekler (hata ile birlikte)
        /// </summary>
        Task<bool> LogHataEkleAsync(string islemTuru, object kayitID, string aciklama, string hataMesaji);
        
        /// <summary>
        /// Yeni bir sistem log kaydı ekler (exception ile birlikte)
        /// </summary>
        Task<bool> LogExceptionEkleAsync(string islemTuru, object kayitID, string aciklama, Exception ex);
        
        /// <summary>
        /// Yeni bir kritik hata log kaydı ekler
        /// </summary>
        Task<bool> LogKritikHataEkleAsync(string islemTuru, object kayitID, string aciklama, string hataMesaji);
        
        /// <summary>
        /// Yeni bir uyarı log kaydı ekler
        /// </summary>
        Task<bool> LogUyariEkleAsync(string islemTuru, object kayitID, string aciklama);
        
        /// <summary>
        /// Sistem loglarını listeler
        /// </summary>
        Task<List<SistemLog>> GetLogsAsync(bool? filtreBasarili = null, string islemTuru = null,
            DateTime? baslangicTarihi = null, DateTime? bitisTarihi = null, object kayitID = null);
    }
} 
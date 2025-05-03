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
        /// <param name="kayitID">İlgili kaydın ID'si (string formatında)</param>
        /// <param name="aciklama">Açıklama</param>
        /// <param name="basarili">İşlem başarılı mı?</param>
        /// <returns>true: Başarılı, false: Başarısız</returns>
        public async Task<bool> LogEkleAsync(string islemTuru, string? kayitID, string aciklama, bool basarili = true)
        {
            return await InternalLogEkleAsync(islemTuru, kayitID, aciklama, basarili, null,
                basarili ? LogLevel.Information : LogLevel.Warning);
        }
        
        /// <summary>
        /// Yeni bir sistem log kaydı ekler (hata ile birlikte)
        /// </summary>
        /// <param name="islemTuru">İşlem Türü</param>
        /// <param name="kayitID">İlgili kaydın ID'si (string formatında)</param>
        /// <param name="aciklama">Açıklama</param>
        /// <param name="hataMesaji">Hata mesajı</param>
        /// <returns>true: Başarılı, false: Başarısız</returns>
        public async Task<bool> LogHataEkleAsync(string islemTuru, string? kayitID, string aciklama, string hataMesaji)
        {
            return await InternalLogEkleAsync(islemTuru, kayitID, aciklama, false, hataMesaji, LogLevel.Error);
        }
        
        /// <summary>
        /// Yeni bir sistem log kaydı ekler (exception ile birlikte)
        /// </summary>
        /// <param name="islemTuru">İşlem Türü</param>
        /// <param name="kayitID">İlgili kaydın ID'si (string formatında)</param>
        /// <param name="aciklama">Açıklama</param>
        /// <param name="ex">Exception</param>
        /// <returns>true: Başarılı, false: Başarısız</returns>
        public async Task<bool> LogExceptionEkleAsync(string islemTuru, string? kayitID, string aciklama, Exception ex)
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
        /// <param name="kayitID">İlgili kaydın ID'si (string formatında)</param>
        /// <param name="aciklama">Açıklama</param>
        /// <param name="hataMesaji">Hata mesajı</param>
        /// <returns>true: Başarılı, false: Başarısız</returns>
        public async Task<bool> LogKritikHataEkleAsync(string islemTuru, string? kayitID, string aciklama, string hataMesaji)
        {
            return await InternalLogEkleAsync(islemTuru, kayitID, aciklama, false, hataMesaji, LogLevel.Critical);
        }
        
        /// <summary>
        /// Yeni bir uyarı log kaydı ekler
        /// </summary>
        /// <param name="islemTuru">İşlem Türü</param>
        /// <param name="kayitID">İlgili kaydın ID'si (string formatında)</param>
        /// <param name="aciklama">Açıklama</param>
        /// <returns>true: Başarılı, false: Başarısız</returns>
        public async Task<bool> LogUyariEkleAsync(string islemTuru, string? kayitID, string aciklama)
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
        /// <param name="kayitID">Kayıt ID filtresi (string, null: tümü)</param>
        /// <returns>SistemLog listesi</returns>
        public async Task<List<SistemLog>> GetLogsAsync(bool? filtreBasarili = null, string? islemTuru = null,
            DateTime? baslangicTarihi = null, DateTime? bitisTarihi = null, string? kayitID = null)
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
                
                if (!string.IsNullOrEmpty(kayitID))
                {
                    query = query.Where(l => l.KayitID == kayitID);
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
        private async Task<bool> InternalLogEkleAsync(string islemTuru, string? kayitID, string aciklama, 
            bool basarili, string? hataMesaji, LogLevel logLevel)
        {
            try
            {
                // Kullanıcı ID'sini al
                string? kullaniciId = GetCurrentUserId();
                string? kullaniciAdi = GetCurrentUserName();
                Guid? kullaniciGuid = GetCurrentUserGuid();

                string? kayitAdi = await GetKayitAdiAsync(islemTuru, kayitID); // Kayıt adını bulmak için string ID kullan
                string? tabloAdi = GetTabloAdi(islemTuru);

                var log = new SistemLog
                {
                    LogID = Guid.NewGuid().ToString(),
                    LogTuru = logLevel.ToString(),
                    Mesaj = aciklama, 
                    Sayfa = _httpContextAccessor.HttpContext?.Request.Path ?? "Bilinmiyor",
                    OlusturmaTarihi = DateTime.Now,
                    IslemTuru = islemTuru,
                    LogTuruInt = (int)logLevel,
                    Aciklama = aciklama,
                    HataMesaji = hataMesaji,
                    KullaniciAdi = kullaniciAdi,
                    IPAdresi = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "Bilinmiyor",
                    IslemTarihi = DateTime.Now,
                    Basarili = basarili,
                    TabloAdi = tabloAdi,
                    KayitAdi = kayitAdi ?? kayitID, // Kayıt adı bulunamazsa ID'yi kullan
                    KayitID = kayitID, // String olarak ata
                    KullaniciId = kullaniciId,
                    KullaniciGuid = kullaniciGuid
                };
                
                // Veritabanına ekle
                await _context.SistemLoglar.AddAsync(log);
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
        private string? GetCurrentUserId()
        {
            if (_httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated == true)
            {
                var userId = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                return userId ?? "Anonim";
            }
            
            return "Anonim";
        }

        private string? GetCurrentUserName()
        {
            return _httpContextAccessor.HttpContext?.User?.Identity?.Name;
        }

        private Guid? GetCurrentUserGuid()
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (Guid.TryParse(userIdClaim, out Guid userGuid))
            {
                return userGuid;
            }
            return null;
        }

        private async Task<string?> GetKayitAdiAsync(string islemTuru, string? kayitID)
        {
            if (string.IsNullOrEmpty(kayitID))
                return null;

            try
            {
                Guid parsedGuid; // Guid denemesi için değişken
                bool isGuid = Guid.TryParse(kayitID, out parsedGuid);

                switch (GetTabloAdi(islemTuru))
                {
                    case "Urun":
                        if (isGuid)
                        {
                            var urun = await _context.Urunler.FindAsync(parsedGuid);
                            return urun?.UrunAdi;
                        }
                        break;
                    case "Cari":
                         if (isGuid)
                        {
                            var cari = await _context.Cariler.FindAsync(parsedGuid);
                            return cari?.Ad;
                        }
                        break;
                    case "Fatura":
                         if (isGuid)
                        {
                            var fatura = await _context.Faturalar.FindAsync(parsedGuid);
                            return fatura?.FaturaNumarasi;
                        }
                        break;
                    case "BankaHesap":
                         if (isGuid)
                         {
                             var bankaHesap = await _context.BankaHesaplari.FindAsync(parsedGuid);
                             return bankaHesap?.HesapAdi;
                         }
                         break;
                    case "Kasa":
                        if (isGuid)
                        {
                             var kasa = await _context.Kasalar.FindAsync(parsedGuid);
                             return kasa?.KasaAdi;
                        }
                         break;
                    // Diğer tablolar için case'ler eklenebilir...
                }
            }
            catch (Exception ex)
            { 
                _logger.LogError(ex, $"Kayıt adı getirilirken hata: Tablo={GetTabloAdi(islemTuru)}, KayitID={kayitID}");
            }

            return kayitID; // Bulunamazsa ID'yi döndür
        }

        private string? GetTabloAdi(string islemTuru)
        {
            // İşlem türüne göre tablo adını belirle (Örnek implementasyon)
            if (islemTuru.Contains("Urun")) return "Urun";
            if (islemTuru.Contains("Cari")) return "Cari";
            if (islemTuru.Contains("Fatura")) return "Fatura";
            if (islemTuru.Contains("BankaHesap")) return "BankaHesap";
            if (islemTuru.Contains("Kasa")) return "Kasa";
            if (islemTuru.Contains("Birim")) return "Birim";
            if (islemTuru.Contains("Kategori")) return "UrunKategori";
            if (islemTuru.Contains("Depo")) return "Depo";
             if (islemTuru.Contains("Banka")) return "Banka";
            // ... Diğer işlem türleri
            return null;
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
        Task<bool> LogEkleAsync(string islemTuru, string? kayitID, string aciklama, bool basarili = true);
        
        /// <summary>
        /// Yeni bir sistem log kaydı ekler (hata ile birlikte)
        /// </summary>
        Task<bool> LogHataEkleAsync(string islemTuru, string? kayitID, string aciklama, string hataMesaji);
        
        /// <summary>
        /// Yeni bir sistem log kaydı ekler (exception ile birlikte)
        /// </summary>
        Task<bool> LogExceptionEkleAsync(string islemTuru, string? kayitID, string aciklama, Exception ex);
        
        /// <summary>
        /// Yeni bir kritik hata log kaydı ekler
        /// </summary>
        Task<bool> LogKritikHataEkleAsync(string islemTuru, string? kayitID, string aciklama, string hataMesaji);
        
        /// <summary>
        /// Yeni bir uyarı log kaydı ekler
        /// </summary>
        Task<bool> LogUyariEkleAsync(string islemTuru, string? kayitID, string aciklama);
        
        /// <summary>
        /// Sistem loglarını listeler
        /// </summary>
        Task<List<SistemLog>> GetLogsAsync(bool? filtreBasarili = null, string? islemTuru = null,
            DateTime? baslangicTarihi = null, DateTime? bitisTarihi = null, string? kayitID = null);
    }
} 
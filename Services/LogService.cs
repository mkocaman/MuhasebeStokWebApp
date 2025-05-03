using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using MuhasebeStokWebApp.Data;
using MuhasebeStokWebApp.Data.Entities;
using MuhasebeStokWebApp.Models;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.Extensions.Logging;
using ESistemLog = MuhasebeStokWebApp.Data.Entities.SistemLog;
using MSistemLog = MuhasebeStokWebApp.Models.SistemLog;
using MuhasebeStokWebApp.Enums;
using MuhasebeStokWebApp.Services.Interfaces;

namespace MuhasebeStokWebApp.Services
{
    public class LogService : ILogService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<LogService> _logger;

        public LogService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor, ILogger<LogService> logger)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task<bool> LogEkleAsync(string mesaj, MuhasebeStokWebApp.Enums.LogTuru logTuru, string detay = null)
        {
            try
            {
                var httpContext = _httpContextAccessor.HttpContext;
                var kullaniciAdi = httpContext != null ? httpContext.User?.Identity?.Name ?? "Anonim" : "Sistem";
                var ipAdresi = httpContext != null ? httpContext.Connection?.RemoteIpAddress?.ToString() ?? "0.0.0.0" : "0.0.0.0";
                var sayfaAdresi = httpContext != null ? httpContext.Request?.Path.ToString() ?? "/" : "/";
                var kullaniciIDString = httpContext != null && httpContext.User?.Identity?.IsAuthenticated == true ?
                    httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value : null;
                
                // Artık KullaniciId string tipinde olduğu için doğrudan atama yapıyoruz
                string? kullaniciId = kullaniciIDString;

                var log = new ESistemLog
                {
                    LogID = Guid.NewGuid(),
                    LogTuru = logTuru.ToString(),
                    LogTuruInt = (int)logTuru,
                    IslemTuru = logTuru.ToString(),
                    Aciklama = mesaj ?? "Sistem log kaydı", // Null kontrolü eklendi
                    HataMesaji = detay ?? "İşlem başarılı", // Null kontrolü eklendi
                    KullaniciAdi = kullaniciAdi, // Bu zaten null değil
                    IPAdresi = ipAdresi,
                    IslemTarihi = DateTime.Now,
                    TabloAdi = string.Empty,
                    KayitAdi = string.Empty,
                    Mesaj = mesaj ?? "Sistem log kaydı", // Null kontrolü eklendi
                    Sayfa = sayfaAdresi,
                    KullaniciId = kullaniciId,
                    KullaniciGuid = kullaniciId != null ? Guid.TryParse(kullaniciId, out Guid guidResult) ? guidResult : (Guid?)null : null
                };

                _context.SistemLoglar.Add(log);
                await _context.SaveChangesAsync();
                
                return true;
            }
            catch (Exception ex)
            {
                // Loglama mekanizmasının çalışmaması uygulamayı etkilemesin
                _logger.LogError(ex, $"Log ekleme hatası: {mesaj} - {ex.Message}");
                return false;
            }
        }

        public async Task<List<MSistemLog>> GetAllLogsAsync()
        {
            var logs = await _context.SistemLoglar
                .OrderByDescending(l => l.IslemTarihi)
                .ToListAsync();
            
            return logs.Select(l => new MSistemLog
            {
                SistemLogID = l.LogID.GetHashCode(),
                IslemTuru = l.IslemTuru,
                Aciklama = l.Aciklama,
                KullaniciAdi = l.KullaniciAdi,
                IPAdresi = l.IPAdresi,
                IslemTarihi = l.IslemTarihi,
                Tarayici = l.HataMesaji // En yakın eşleşme
            }).ToList();
        }

        public async Task<List<ESistemLog>> GetLogsAsync(
            DateTime? startDate = null,
            DateTime? endDate = null,
            string searchTerm = null,
            string logTuru = null,
            int page = 1,
            int pageSize = 20)
        {
            var query = _context.SistemLoglar.AsQueryable();

            // Filtreleri uygula
            if (startDate.HasValue)
            {
                query = query.Where(l => l.IslemTarihi >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(l => l.IslemTarihi <= endDate.Value);
            }

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(l => 
                    l.IslemTuru.Contains(searchTerm) || 
                    l.Aciklama.Contains(searchTerm) || 
                    l.KullaniciAdi.Contains(searchTerm) || 
                    l.TabloAdi.Contains(searchTerm) || 
                    l.KayitAdi.Contains(searchTerm));
            }

            if (!string.IsNullOrEmpty(logTuru))
            {
                if (Enum.TryParse<MuhasebeStokWebApp.Enums.LogTuru>(logTuru, out var logTuruEnum))
                {
                    query = query.Where(l => l.LogTuruInt == (int)logTuruEnum);
                }
            }

            // Sıralama ve sayfalama
            var logs = await query
                .OrderByDescending(l => l.IslemTarihi)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return logs;
        }

        // Tarayıcı bilgisini çıkartan yardımcı metod
        public string GetBrowserInfo(string userAgent)
        {
            if (string.IsNullOrEmpty(userAgent))
                return "Bilinmiyor";

            // Basit bir tarayıcı tespiti
            if (userAgent.Contains("Chrome") && !userAgent.Contains("Edg"))
                return "Chrome";
            else if (userAgent.Contains("Firefox"))
                return "Firefox";
            else if (userAgent.Contains("Edg"))
                return "Edge";
            else if (userAgent.Contains("Safari") && !userAgent.Contains("Chrome"))
                return "Safari";
            else if (userAgent.Contains("MSIE") || userAgent.Contains("Trident"))
                return "Internet Explorer";
            else
                return "Diğer";
        }

        // İşletim sistemi bilgisini çıkartan yardımcı metod
        public string GetOSInfo(string userAgent)
        {
            if (string.IsNullOrEmpty(userAgent))
                return "Bilinmiyor";

            // Basit bir işletim sistemi tespiti
            if (userAgent.Contains("Windows"))
                return "Windows";
            else if (userAgent.Contains("Mac"))
                return "MacOS";
            else if (userAgent.Contains("Linux"))
                return "Linux";
            else if (userAgent.Contains("Android"))
                return "Android";
            else if (userAgent.Contains("iPhone") || userAgent.Contains("iPad"))
                return "iOS";
            else
                return "Diğer";
        }

        // LogOlustur metodunda
        public async Task<bool> LogOlustur(string mesaj, MuhasebeStokWebApp.Enums.LogTuru logTuru, string tabloAdi, string kayitAdi, Guid? kayitID, string kullaniciAdi = null, bool basarili = true, string kategori = null)
        {
            try
            {
                var log = new ESistemLog
                {
                    LogID = Guid.NewGuid(),
                    LogTuru = logTuru.ToString(),
                    LogTuruInt = (int)logTuru,
                    IslemTuru = logTuru.ToString(),
                    Aciklama = mesaj,
                    KullaniciAdi = kullaniciAdi ?? "Sistem",
                    IslemTarihi = DateTime.Now,
                    Basarili = basarili,
                    TabloAdi = tabloAdi,
                    KayitAdi = kayitAdi,
                    KayitID = kayitID,
                    Mesaj = mesaj,
                    HataMesaji = string.Empty,
                    KullaniciId = kayitID?.ToString(),
                    KullaniciGuid = kayitID
                };

                _context.SistemLoglar.Add(log);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                // Loglama hatası
                _logger.LogError(ex, "Log oluşturma hatası: {Message}", ex.Message);
                return false;
            }
        }
        
        // Tablo adına göre kategori belirleyen yardımcı metod
        private string DetermineCategory(string tabloAdi)
        {
            if (string.IsNullOrEmpty(tabloAdi))
                return "Genel";
        
            switch (tabloAdi.ToLower())
            {
                case "cari":
                case "carihareket":
                case "cariler":
                    return "Müşteri";
            
                case "fatura":
                case "faturalar":
                case "faturadetay":
                    return "Finansal";
            
                case "urun":
                case "urunler":
                case "stokhareket":
                case "depo":
                    return "Stok";
            
                case "kullanici":
                case "rol":
                case "rolkullanici":
                case "applicationuser":
                    return "Kullanıcı";
            
                case "parabirimi":
                case "kurları":
                case "kurlar":
                case "doviz":
                    return "Döviz";
            
                case "sistemloglar":
                case "log":
                    return "Sistem";
            
                default:
                    return tabloAdi;
            }
        }

        // OturumAcmaLogEkle metodundaki string to Guid? hatalarını düzeltelim
        public async Task OturumAcmaLogEkle(string kullaniciID, string kullaniciAdi)
        {
            try
            {
                var ipAdresi = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "Bilinmiyor";
                var userAgent = _httpContextAccessor.HttpContext?.Request?.Headers["User-Agent"].ToString() ?? "Bilinmiyor";
                var tarayici = GetBrowserInfo(userAgent);
                var isletimSistemi = GetOSInfo(userAgent);

                var log = new ESistemLog
                {
                    LogID = Guid.NewGuid(),
                    IslemTuru = "Oturum Açma",
                    LogTuruInt = (int)MuhasebeStokWebApp.Enums.LogTuru.Giris,
                    TabloAdi = "AspNetUsers",
                    KayitAdi = kullaniciAdi,
                    KayitID = Guid.TryParse(kullaniciID, out Guid guidResult) ? guidResult : (Guid?)null,
                    Aciklama = $"{kullaniciAdi} kullanıcısı sisteme giriş yaptı.",
                    HataMesaji = "İşlem başarılı", // HataMesaji alanını ekliyoruz
                    KullaniciAdi = kullaniciAdi,
                    IPAdresi = ipAdresi,
                    IslemTarihi = DateTime.Now,
                    Basarili = true,
                    Sayfa = string.Empty, // Sayfa alanını boş string ile dolduruyoruz
                    KullaniciId = kullaniciID,
                    KullaniciGuid = Guid.TryParse(kullaniciID, out Guid guidValue) ? guidValue : (Guid?)null
                };

                _context.SistemLoglar.Add(log);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Oturum açma logu oluşturulurken hata: {Message}", ex.Message);
            }
        }
        
        public async Task<List<MSistemLog>> GetLogsByKullaniciAsync(string kullaniciAdi, DateTime? baslangicTarihi = null, DateTime? bitisTarihi = null)
        {
            var query = _context.SistemLoglar.AsQueryable();

            if (!string.IsNullOrEmpty(kullaniciAdi))
                query = query.Where(l => l.KullaniciAdi == kullaniciAdi);

                if (baslangicTarihi.HasValue)
                    query = query.Where(l => l.IslemTarihi >= baslangicTarihi.Value);

                if (bitisTarihi.HasValue)
                query = query.Where(l => l.IslemTarihi <= bitisTarihi.Value);

            var logs = await query.OrderByDescending(l => l.IslemTarihi).ToListAsync();
            
            return logs.Select(l => new MSistemLog
            {
                SistemLogID = l.LogID.GetHashCode(),
                        IslemTuru = l.IslemTuru,
                        Aciklama = l.Aciklama,
                        KullaniciAdi = l.KullaniciAdi,
                        IPAdresi = l.IPAdresi,
                        IslemTarihi = l.IslemTarihi,
                Tarayici = l.HataMesaji // En yakın eşleşme
            }).ToList();
        }
        
        public async Task<List<MSistemLog>> GetLogsByTurAsync(MuhasebeStokWebApp.Enums.LogTuru logTuru, DateTime? baslangicTarihi = null, DateTime? bitisTarihi = null)
            {
                var query = _context.SistemLoglar.AsQueryable();

            query = query.Where(l => l.IslemTuru == logTuru.ToString());

                if (baslangicTarihi.HasValue)
                    query = query.Where(l => l.IslemTarihi >= baslangicTarihi.Value);

                if (bitisTarihi.HasValue)
                query = query.Where(l => l.IslemTarihi <= bitisTarihi.Value);

            var logs = await query.OrderByDescending(l => l.IslemTarihi).ToListAsync();
            
            return logs.Select(l => new MSistemLog
            {
                SistemLogID = l.LogID.GetHashCode(),
                        IslemTuru = l.IslemTuru,
                        Aciklama = l.Aciklama,
                        KullaniciAdi = l.KullaniciAdi,
                        IPAdresi = l.IPAdresi,
                        IslemTarihi = l.IslemTarihi,
                Tarayici = l.HataMesaji // En yakın eşleşme
            }).ToList();
        }
        
        public async Task<List<MSistemLog>> GetCariLogsAsync(Guid cariId)
        {
            var query = _context.SistemLoglar.AsQueryable();

            // Cari ID'sine göre filtreleme - string yerine doğrudan GUID ile karşılaştırma
            query = query.Where(l => l.KayitID == cariId);

            var logs = await query.OrderByDescending(l => l.IslemTarihi).ToListAsync();

            return logs.Select(l => new MSistemLog
                    {
                        LogID = l.LogID,
                SistemLogID = l.LogID.GetHashCode(),
                        IslemTuru = l.IslemTuru,
                        Aciklama = l.Aciklama,
                        KullaniciAdi = l.KullaniciAdi,
                        IPAdresi = l.IPAdresi,
                        IslemTarihi = l.IslemTarihi,
                        TabloAdi = l.TabloAdi,
                        KayitAdi = l.KayitAdi,
                IlgiliID = l.KayitID // KayitID alanını kullanıyoruz
            }).ToList();
        }

        public async Task ClearLogsAsync()
        {
            _context.SistemLoglar.RemoveRange(_context.SistemLoglar);
            await _context.SaveChangesAsync();
        }
        
        public async Task<bool> LogSilAsync(int id)
        {
            var entityLog = await _context.SistemLoglar.FirstOrDefaultAsync(l => l.LogID.GetHashCode() == id);
            if (entityLog != null)
            {
                _context.SistemLoglar.Remove(entityLog);
                await _context.SaveChangesAsync();
                return true;
            }
                return false;
        }
        
        public async Task CariOlusturmaLogOlustur(Guid cariID, string ad, string aciklama)
        {
            try 
            {
                string kullaniciAdi = _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "Sistem";
                string ipAdresi = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "0.0.0.0";
                string? kullaniciId = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);

                var log = new ESistemLog
                {
                    LogID = Guid.NewGuid(),
                    LogTuru = "Bilgi",
                    IslemTuru = "Cari Oluşturma",
                    LogTuruInt = (int)MuhasebeStokWebApp.Enums.LogTuru.Bilgi,
                    Aciklama = $"{ad} isimli cari kaydı oluşturuldu. {aciklama}",
                    KullaniciAdi = kullaniciAdi,
                    IPAdresi = ipAdresi,
                    IslemTarihi = DateTime.Now,
                    Basarili = true,
                    TabloAdi = "Cariler",
                    KayitAdi = ad,
                    KayitID = cariID,
                    Mesaj = $"Cari Kaydı Eklendi: {ad}",
                    HataMesaji = string.Empty,
                    KullaniciId = kullaniciId,
                    KullaniciGuid = kullaniciId != null ? Guid.TryParse(kullaniciId, out Guid guidResult) ? guidResult : (Guid?)null : null
                };

                _context.SistemLoglar.Add(log);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                await LogErrorAsync("CariOlusturmaLogOlustur", ex.StackTrace, ex);
            }
        }
        
        public async Task CariGuncellemeLogOlustur(Guid cariID, string ad, string aciklama)
        {
            try 
            {
                string kullaniciAdi = _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "Sistem";
                string ipAdresi = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "0.0.0.0";
                string? kullaniciId = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);

                var log = new ESistemLog
                {
                    LogID = Guid.NewGuid(),
                    LogTuru = "Bilgi",
                    IslemTuru = "Cari Güncelleme",
                    LogTuruInt = (int)MuhasebeStokWebApp.Enums.LogTuru.Bilgi,
                    Aciklama = $"{ad} isimli cari kaydı güncellendi. {aciklama}",
                    KullaniciAdi = kullaniciAdi,
                    IPAdresi = ipAdresi,
                    IslemTarihi = DateTime.Now,
                    Basarili = true,
                    TabloAdi = "Cariler",
                    KayitAdi = ad,
                    KayitID = cariID,
                    Mesaj = $"Cari Kaydı Güncellendi: {ad}",
                    HataMesaji = string.Empty,
                    KullaniciId = kullaniciId,
                    KullaniciGuid = kullaniciId != null ? Guid.TryParse(kullaniciId, out Guid guidResult) ? guidResult : (Guid?)null : null
                };

                _context.SistemLoglar.Add(log);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                await LogErrorAsync("CariGuncellemeLogOlustur", ex.StackTrace, ex);
            }
        }
        
        public async Task CariSilmeLogOlustur(Guid cariID, string ad, string aciklama)
        {
            try
            {
                string kullaniciAdi = _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "Sistem";
                string ipAdresi = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "0.0.0.0";
                string? kullaniciId = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);

                var log = new ESistemLog
                {
                    LogID = Guid.NewGuid(),
                    LogTuru = "Bilgi",
                    IslemTuru = "Cari Silme",
                    LogTuruInt = (int)MuhasebeStokWebApp.Enums.LogTuru.Bilgi,
                    Aciklama = $"{ad} isimli cari kaydı silindi. {aciklama}",
                    KullaniciAdi = kullaniciAdi,
                    IPAdresi = ipAdresi,
                    IslemTarihi = DateTime.Now,
                    Basarili = true,
                    TabloAdi = "Cariler",
                    KayitAdi = ad,
                    KayitID = cariID,
                    Mesaj = $"Cari Kaydı Silindi: {ad}",
                    HataMesaji = string.Empty,
                    KullaniciId = kullaniciId,
                    KullaniciGuid = kullaniciId != null ? Guid.TryParse(kullaniciId, out Guid guidResult) ? guidResult : (Guid?)null : null
                };

                _context.SistemLoglar.Add(log);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                await LogErrorAsync("CariSilmeLogOlustur", ex.StackTrace, ex);
            }
        }
        
        public async Task CariPasifleştirmeLogOlustur(Guid cariID, string ad, string aciklama)
        {
            try
            {
                var httpContext = _httpContextAccessor.HttpContext;
                var kullaniciAdi = httpContext != null ? httpContext.User?.Identity?.Name ?? "Sistem" : "Sistem";
                var kullaniciId = httpContext != null && httpContext.User?.Identity?.IsAuthenticated == true ?
                    httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value : null;

                var log = new ESistemLog
                {
                    LogID = Guid.NewGuid(),
                    LogTuru = LogTuru.Bilgi.ToString(),
                    LogTuruInt = (int)LogTuru.Bilgi,
                    IslemTuru = "Cari Pasifleştirme",
                    Aciklama = $"{ad} adlı cari pasif duruma getirildi: {aciklama}",
                    KullaniciAdi = kullaniciAdi,
                    IslemTarihi = DateTime.Now,
                    Basarili = true,
                    TabloAdi = "Cari",
                    KayitAdi = ad,
                    KayitID = cariID,
                    KullaniciId = kullaniciId,
                    KullaniciGuid = kullaniciId != null ? Guid.TryParse(kullaniciId, out Guid guidResult) ? guidResult : (Guid?)null : null
                };

                _context.SistemLoglar.Add(log);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Cari pasife alma log oluşturma hatası: {Mesaj}", ex.Message);
            }
        }
        
        public async Task CariAktifleştirmeLogOlustur(Guid cariID, string ad, string aciklama)
        {
            try
            {
                var httpContext = _httpContextAccessor.HttpContext;
                var kullaniciAdi = httpContext != null ? httpContext.User?.Identity?.Name ?? "Sistem" : "Sistem";
                var kullaniciId = httpContext != null && httpContext.User?.Identity?.IsAuthenticated == true ?
                    httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value : null;

                var log = new ESistemLog
                {
                    LogID = Guid.NewGuid(),
                    LogTuru = LogTuru.Bilgi.ToString(),
                    LogTuruInt = (int)LogTuru.Bilgi,
                    IslemTuru = "Cari Aktifleştirme",
                    Aciklama = $"{ad} adlı cari aktifleştirildi: {aciklama}",
                    KullaniciAdi = kullaniciAdi,
                    IslemTarihi = DateTime.Now,
                    Basarili = true,
                    TabloAdi = "Cari",
                    KayitAdi = ad,
                    KayitID = cariID,
                    KullaniciId = kullaniciId,
                    KullaniciGuid = kullaniciId != null ? Guid.TryParse(kullaniciId, out Guid guidResult) ? guidResult : (Guid?)null : null
                };

                _context.SistemLoglar.Add(log);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Cari aktifleştirme log oluşturma hatası: {Mesaj}", ex.Message);
            }
        }

        public async Task CariHareketEklemeLogOlustur(Guid cariID, string ad, string hareketTuru, decimal tutar, string aciklama)
        {
            try
            {
                string kullaniciAdi = _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "Sistem";
                string ipAdresi = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "0.0.0.0";
                string? kullaniciId = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);

                var log = new ESistemLog
                {
                    LogID = Guid.NewGuid(),
                    LogTuru = "Bilgi",
                    IslemTuru = "Cari Hareket Ekleme",
                    LogTuruInt = (int)MuhasebeStokWebApp.Enums.LogTuru.Bilgi,
                    Aciklama = $"{ad} isimli cariye {tutar} TL tutarında {hareketTuru} hareketi eklendi. {aciklama}",
                    KullaniciAdi = kullaniciAdi,
                    IPAdresi = ipAdresi,
                    IslemTarihi = DateTime.Now,
                    Basarili = true,
                    TabloAdi = "CariHareketler",
                    KayitAdi = ad,
                    KayitID = cariID,
                    Mesaj = $"Cari Hareket Eklendi: {ad} - {tutar} TL {hareketTuru}",
                    HataMesaji = string.Empty,
                    KullaniciId = kullaniciId,
                    KullaniciGuid = kullaniciId != null ? Guid.TryParse(kullaniciId, out Guid guidResult) ? guidResult : (Guid?)null : null
                };

                _context.SistemLoglar.Add(log);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                await LogErrorAsync("CariHareketEklemeLogOlustur", ex.StackTrace, ex);
            }
        }
        
        public async Task UrunOlusturmaLogOlustur(Guid urunID, string urunAdi, string aciklama)
        {
            try
            {
                var kullaniciAdi = _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "Sistem";
                var ipAdresi = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "Bilinmiyor";
                
                // Kullanıcı ID'sini al
                string? kullaniciId = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);

                var log = new ESistemLog
                {
                    LogID = Guid.NewGuid(),
                    IslemTuru = "Ürün Oluşturma",
                    Aciklama = aciklama,
                    HataMesaji = "İşlem başarılı", // HataMesaji alanını ekliyoruz
                    KullaniciAdi = kullaniciAdi,
                    IPAdresi = ipAdresi,
                    IslemTarihi = DateTime.Now,
                    TabloAdi = "Urunler",
                    KayitAdi = urunAdi,
                    KayitID = urunID, // Doğrudan GUID kullanımı
                    Sayfa = string.Empty, // Sayfa alanını boş string ile dolduruyoruz
                    KullaniciId = kullaniciId,
                    KullaniciGuid = kullaniciId != null ? Guid.TryParse(kullaniciId, out Guid guidResult) ? guidResult : (Guid?)null : null
                };

                _context.SistemLoglar.Add(log);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Loglama hatası durumunda sessizce devam et
                _logger.LogError(ex, "Ürün oluşturma logu oluşturulurken hata: {Message}", ex.Message);
            }
        }
        
        public async Task UrunGuncellemeLogOlustur(Guid urunID, string urunAdi, string aciklama)
        {
            try
            {
                var kullaniciAdi = _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "Sistem";
                var ipAdresi = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "Bilinmiyor";
                
                // Kullanıcı ID'sini al
                string? kullaniciId = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);

                var log = new ESistemLog
                {
                    LogID = Guid.NewGuid(),
                    IslemTuru = "Ürün Güncelleme",
                    Aciklama = aciklama,
                    HataMesaji = "İşlem başarılı", // HataMesaji alanını ekliyoruz
                    KullaniciAdi = kullaniciAdi,
                    IPAdresi = ipAdresi,
                    IslemTarihi = DateTime.Now,
                    TabloAdi = "Urunler",
                    KayitAdi = urunAdi,
                    KayitID = urunID, // Doğrudan GUID kullanımı
                    Sayfa = string.Empty, // Sayfa alanını boş string ile dolduruyoruz
                    KullaniciId = kullaniciId,
                    KullaniciGuid = kullaniciId != null ? Guid.TryParse(kullaniciId, out Guid guidResult) ? guidResult : (Guid?)null : null
                };

                _context.SistemLoglar.Add(log);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Loglama hatası durumunda sessizce devam et
                _logger.LogError(ex, "Ürün güncelleme logu oluşturulurken hata: {Message}", ex.Message);
            }
        }
        
        public async Task UrunSilmeLogOlustur(Guid urunID, string urunAdi, string aciklama)
        {
            try
            {
                var kullaniciAdi = _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "Sistem";
                var ipAdresi = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "Bilinmiyor";
                
                // Kullanıcı ID'sini al
                string? kullaniciId = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);

                var log = new ESistemLog
                {
                    LogID = Guid.NewGuid(),
                    IslemTuru = "Ürün Silme",
                    Aciklama = aciklama,
                    HataMesaji = "İşlem başarılı", // HataMesaji alanını ekliyoruz
                    KullaniciAdi = kullaniciAdi,
                    IPAdresi = ipAdresi,
                    IslemTarihi = DateTime.Now,
                    TabloAdi = "Urunler",
                    KayitAdi = urunAdi,
                    KayitID = urunID, // Doğrudan GUID kullanımı
                    Sayfa = string.Empty, // Sayfa alanını boş string ile dolduruyoruz
                    KullaniciId = kullaniciId,
                    KullaniciGuid = kullaniciId != null ? Guid.TryParse(kullaniciId, out Guid guidResult) ? guidResult : (Guid?)null : null
                };

                _context.SistemLoglar.Add(log);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Loglama hatası durumunda sessizce devam et
                _logger.LogError(ex, "Ürün silme logu oluşturulurken hata: {Message}", ex.Message);
            }
        }
        
        public async Task StokGirisLogOlustur(Guid stokHareketID, Guid urunID, string urunAdi, decimal miktar, string aciklama)
        {
            try
            {
                var kullaniciAdi = _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "Sistem";
                var ipAdresi = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "Bilinmiyor";
                
                // Kullanıcı ID'sini al
                string? kullaniciId = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);

                // Log oluştur
                var log = new ESistemLog
                {
                    LogID = Guid.NewGuid(),
                    IslemTuru = "Stok Giriş",
                    LogTuruInt = (int)MuhasebeStokWebApp.Enums.LogTuru.Bilgi,
                    KayitID = urunID, // urunID doğrudan Guid olarak kullan, ToString() yapmadan
                    TabloAdi = "StokHareketler",
                    KayitAdi = urunAdi,
                    Aciklama = $"{urunAdi} ürününe {miktar} adet giriş yapıldı. Açıklama: {aciklama}",
                    HataMesaji = "İşlem başarılı",
                    KullaniciAdi = kullaniciAdi,
                    IPAdresi = ipAdresi,
                    IslemTarihi = DateTime.Now,
                    Basarili = true,
                    Sayfa = string.Empty, // Sayfa alanını boş string ile dolduruyoruz
                    KullaniciId = kullaniciId,
                    KullaniciGuid = kullaniciId != null ? Guid.TryParse(kullaniciId, out Guid guidResult) ? guidResult : (Guid?)null : null
                };
                
                // Veritabanına kaydet
                _context.SistemLoglar.Add(log);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Stok giriş logu oluşturulurken hata: {Message}", ex.Message);
            }
        }
        
        public async Task StokCikisLogOlustur(Guid stokHareketID, Guid urunID, string urunAdi, decimal miktar, string aciklama)
        {
            try
            {
                var kullaniciAdi = _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "Sistem";
                var ipAdresi = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "Bilinmiyor";
                
                // Kullanıcı ID'sini al
                string? kullaniciId = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);

                // Log oluştur
                var log = new ESistemLog
                {
                    LogID = Guid.NewGuid(),
                    IslemTuru = "Stok Çıkış",
                    LogTuruInt = (int)MuhasebeStokWebApp.Enums.LogTuru.Bilgi,
                    KayitID = urunID, // urunID doğrudan Guid olarak kullan, ToString() yapmadan
                    TabloAdi = "StokHareketler",
                    KayitAdi = urunAdi,
                    Aciklama = $"{urunAdi} ürününden {miktar} adet çıkış yapıldı. Açıklama: {aciklama}",
                    HataMesaji = "İşlem başarılı",
                    KullaniciAdi = kullaniciAdi,
                    IPAdresi = ipAdresi,
                    IslemTarihi = DateTime.Now,
                    Basarili = true,
                    Sayfa = string.Empty, // Sayfa alanını boş string ile dolduruyoruz
                    KullaniciId = kullaniciId,
                    KullaniciGuid = kullaniciId != null ? Guid.TryParse(kullaniciId, out Guid guidResult) ? guidResult : (Guid?)null : null
                };
                
                // Veritabanına kaydet
                _context.SistemLoglar.Add(log);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Stok çıkış logu oluşturulurken hata: {Message}", ex.Message);
            }
        }
        
        public async Task<bool> LogInfoAsync(string message, string detail = null)
        {
            try
            {
                return await LogEkleAsync(message, MuhasebeStokWebApp.Enums.LogTuru.Bilgi, detail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Bilgi logu eklenirken hata oluştu: {Message}", message);
                return false;
            }
        }
        
        public async Task<bool> LogWarningAsync(string message, string detail = null)
        {
            try
            {
                return await LogEkleAsync(message, MuhasebeStokWebApp.Enums.LogTuru.Uyari, detail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Uyarı logu eklenirken hata oluştu: {Message}", message);
                return false;
            }
        }
        
        public async Task<bool> LogErrorAsync(string message, Exception ex)
        {
            try
            {
                string exceptionDetail = ex.ToString();
                _logger.LogError(ex, "{Message}: {ExceptionDetail}", message, exceptionDetail);
                return await LogEkleAsync(message, MuhasebeStokWebApp.Enums.LogTuru.Hata, exceptionDetail);
            }
            catch (Exception innerEx)
            {
                _logger.LogError(innerEx, "LogErrorAsync metodu çalışırken hata oluştu");
                return false;
            }
        }
        
        public async Task<bool> LogErrorAsync(string operation, string message)
        {
            return await LogEkleAsync(operation + ": " + message, MuhasebeStokWebApp.Enums.LogTuru.Hata);
        }

        public async Task LogErrorAsync(string operation, string? stackTrace, Exception ex)
        {
            // stackTrace null olabilir, kontrol edelim
            stackTrace = stackTrace ?? "Stack trace not available";
            await LogEkleAsync(operation + ": " + ex.Message, MuhasebeStokWebApp.Enums.LogTuru.Hata, stackTrace);
        }

        public async Task FaturaOlusturmaLogOlustur(Guid faturaID, string faturaNo, string aciklama)
        {
            try
            {
                var kullaniciAdi = _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "Sistem";
                var ipAdresi = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "Bilinmiyor";
                
                // Kullanıcı ID'sini al
                string? kullaniciId = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);

                var log = new ESistemLog
                {
                    LogID = Guid.NewGuid(),
                    IslemTuru = "Fatura Oluşturma",
                    Aciklama = aciklama,
                    HataMesaji = "İşlem başarılı", // HataMesaji alanını ekliyoruz
                    KullaniciAdi = kullaniciAdi,
                    IPAdresi = ipAdresi,
                    IslemTarihi = DateTime.Now,
                    TabloAdi = "Faturalar",
                    KayitAdi = faturaNo,
                    KayitID = faturaID, // Doğrudan GUID kullanımı
                    Sayfa = string.Empty, // Sayfa alanını boş string ile dolduruyoruz
                    KullaniciId = kullaniciId,
                    KullaniciGuid = kullaniciId != null ? Guid.TryParse(kullaniciId, out Guid guidResult) ? guidResult : (Guid?)null : null
                };

                _context.SistemLoglar.Add(log);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Loglama hatası durumunda sessizce devam et
                _logger.LogError(ex, "Fatura oluşturma logu oluşturulurken hata: {Message}", ex.Message);
            }
        }
        
        public async Task<List<MSistemLog>> GetLogsByTarihAsync(DateTime baslangicTarihi, DateTime bitisTarihi)
        {
            var query = _context.SistemLoglar.AsQueryable()
                .Where(l => l.IslemTarihi >= baslangicTarihi && l.IslemTarihi <= bitisTarihi);
                
            var logs = await query
                .OrderByDescending(l => l.IslemTarihi)
                .ToListAsync();
                
            return logs.Select(l => new MSistemLog
            {
                SistemLogID = l.LogID.GetHashCode(),
                IslemTuru = l.IslemTuru,
                Aciklama = l.Aciklama,
                KullaniciAdi = l.KullaniciAdi,
                IPAdresi = l.IPAdresi,
                IslemTarihi = l.IslemTarihi,
                Tarayici = l.HataMesaji
            }).ToList();
        }
        
        public async Task<List<MSistemLog>> GetLogsByIslemTuruAsync(string islemTuru)
        {
            var query = _context.SistemLoglar.AsQueryable()
                .Where(l => l.IslemTuru == islemTuru);
                
            var logs = await query
                .OrderByDescending(l => l.IslemTarihi)
                .ToListAsync();
                
            return logs.Select(l => new MSistemLog
            {
                SistemLogID = l.LogID.GetHashCode(),
                IslemTuru = l.IslemTuru,
                Aciklama = l.Aciklama,
                KullaniciAdi = l.KullaniciAdi,
                IPAdresi = l.IPAdresi,
                IslemTarihi = l.IslemTarihi,
                Tarayici = l.HataMesaji
            }).ToList();
        }
        
        public async Task<List<MSistemLog>> GetLogsByIslemTuruVeTarihAsync(string islemTuru, DateTime baslangicTarihi, DateTime bitisTarihi)
        {
            var query = _context.SistemLoglar.AsQueryable()
                .Where(l => l.IslemTuru == islemTuru && l.IslemTarihi >= baslangicTarihi && l.IslemTarihi <= bitisTarihi);
                
            var logs = await query
                .OrderByDescending(l => l.IslemTarihi)
                .ToListAsync();
                
            return logs.Select(l => new MSistemLog
            {
                SistemLogID = l.LogID.GetHashCode(),
                IslemTuru = l.IslemTuru,
                Aciklama = l.Aciklama,
                KullaniciAdi = l.KullaniciAdi,
                IPAdresi = l.IPAdresi,
                IslemTarihi = l.IslemTarihi,
                Tarayici = l.HataMesaji
            }).ToList();
        }
        
        public async Task<List<MSistemLog>> GetLogsByKullaniciVeTarihAsync(string kullaniciAdi, DateTime baslangicTarihi, DateTime bitisTarihi)
        {
            var query = _context.SistemLoglar.AsQueryable()
                .Where(l => l.KullaniciAdi == kullaniciAdi && l.IslemTarihi >= baslangicTarihi && l.IslemTarihi <= bitisTarihi);
                
            var logs = await query
                .OrderByDescending(l => l.IslemTarihi)
                .ToListAsync();
                
            return logs.Select(l => new MSistemLog
            {
                SistemLogID = l.LogID.GetHashCode(),
                IslemTuru = l.IslemTuru,
                Aciklama = l.Aciklama,
                KullaniciAdi = l.KullaniciAdi,
                IPAdresi = l.IPAdresi,
                IslemTarihi = l.IslemTarihi,
                Tarayici = l.HataMesaji
            }).ToList();
        }
        
        public async Task<bool> DeleteLogAsync(int logId)
        {
            var entityLog = await _context.SistemLoglar.FirstOrDefaultAsync(l => l.LogID.GetHashCode() == logId);
            if (entityLog != null)
            {
                _context.SistemLoglar.Remove(entityLog);
                await _context.SaveChangesAsync();
                return true;
            }
                return false;
        }
        
        public async Task FaturaGuncellemeLogOlustur(Guid faturaID, string faturaNo, string aciklama)
        {
            try
            {
                var httpContext = _httpContextAccessor.HttpContext;
                var kullaniciAdi = httpContext != null ? httpContext.User?.Identity?.Name ?? "Sistem" : "Sistem";
                var kullaniciId = httpContext != null && httpContext.User?.Identity?.IsAuthenticated == true ?
                    httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value : null;

                var log = new ESistemLog
                {
                    LogID = Guid.NewGuid(),
                    LogTuru = LogTuru.Bilgi.ToString(),
                    LogTuruInt = (int)LogTuru.Bilgi,
                    IslemTuru = "Fatura Güncelleme",
                    Aciklama = $"{faturaNo} numaralı fatura güncellendi: {aciklama}",
                    KullaniciAdi = kullaniciAdi,
                    IslemTarihi = DateTime.Now,
                    Basarili = true,
                    TabloAdi = "Fatura",
                    KayitAdi = faturaNo,
                    KayitID = faturaID,
                    KullaniciId = kullaniciId,
                    KullaniciGuid = kullaniciId != null ? Guid.TryParse(kullaniciId, out Guid guidResult) ? guidResult : (Guid?)null : null
                };

                _context.SistemLoglar.Add(log);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fatura güncelleme log oluşturma hatası: {Mesaj}", ex.Message);
            }
        }

        public async Task FaturaSilmeLogOlustur(Guid faturaID, string faturaNo, string aciklama)
        {
            try
            {
                var kullaniciAdi = _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "Sistem";
                var ipAdresi = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "Bilinmiyor";
                
                // Kullanıcı ID'sini al
                string? kullaniciId = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);

                var log = new ESistemLog
                {
                    LogID = Guid.NewGuid(),
                    IslemTuru = "Fatura Silme",
                    Aciklama = aciklama,
                    HataMesaji = "İşlem başarılı", // HataMesaji alanını ekliyoruz
                    KullaniciAdi = kullaniciAdi,
                    IPAdresi = ipAdresi,
                    IslemTarihi = DateTime.Now,
                    TabloAdi = "Faturalar",
                    KayitAdi = faturaNo,
                    KayitID = faturaID, // Doğrudan GUID kullanımı
                    Sayfa = string.Empty, // Sayfa alanını boş string ile dolduruyoruz
                    KullaniciId = kullaniciId,
                    KullaniciGuid = kullaniciId != null ? Guid.TryParse(kullaniciId, out Guid guidResult) ? guidResult : (Guid?)null : null
                };

                _context.SistemLoglar.Add(log);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Loglama hatası durumunda sessizce devam et
                _logger.LogError(ex, "Fatura silme logu oluşturulurken hata: {Message}", ex.Message);
            }
        }

        public async Task<ESistemLog> DetayliLogEkleAsync(string mesaj, MuhasebeStokWebApp.Enums.LogTuru logTuru, string tabloAdi, string kayitAdi, Guid? kayitID, string detay = null)
        {
            try
            {
                var httpContext = _httpContextAccessor.HttpContext;
                var kullaniciAdi = httpContext != null ? httpContext.User?.Identity?.Name ?? "Anonim" : "Sistem";
                var ipAdresi = httpContext != null ? httpContext.Connection?.RemoteIpAddress?.ToString() ?? "0.0.0.0" : "0.0.0.0";
                var kullaniciIDString = httpContext != null && httpContext.User?.Identity?.IsAuthenticated == true ?
                    httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value : null;

                var log = new ESistemLog
                {
                    LogID = Guid.NewGuid(),
                    LogTuru = logTuru.ToString(),
                    LogTuruInt = (int)logTuru,
                    IslemTuru = logTuru.ToString(),
                    Aciklama = mesaj,
                    KullaniciAdi = kullaniciAdi,
                    IPAdresi = ipAdresi,
                    IslemTarihi = DateTime.Now,
                    Basarili = true,
                    TabloAdi = tabloAdi,
                    KayitAdi = kayitAdi,
                    KayitID = kayitID,
                    Mesaj = mesaj,
                    HataMesaji = detay ?? string.Empty,
                    KullaniciId = kullaniciIDString,
                    KullaniciGuid = kayitID
                };

                _context.SistemLoglar.Add(log);
                await _context.SaveChangesAsync();
                return log;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Log ekleme hatası: {Message}", ex.Message);
                return null;
            }
        }

        public async Task<bool> AddLogAsync(string tableName, string operation, string details, string entityId = null)
        {
            try
            {
                MuhasebeStokWebApp.Enums.LogTuru logTuru;
                string entityIdString = entityId;
                
                // İşlem türünü belirle
                switch (operation.ToLower())
                {
                    case "add":
                    case "create":
                    case "insert":
                    case "oluştur":
                    case "ekle":
                        logTuru = MuhasebeStokWebApp.Enums.LogTuru.Bilgi;
                        break;
                    case "update":
                    case "edit":
                    case "güncelle":
                    case "düzenle":
                        logTuru = MuhasebeStokWebApp.Enums.LogTuru.Bilgi;
                        break;
                    case "delete":
                    case "remove":
                    case "sil":
                    case "kaldır":
                        logTuru = MuhasebeStokWebApp.Enums.LogTuru.Uyari;
                        break;
                    case "error":
                    case "exception":
                    case "hata":
                        logTuru = MuhasebeStokWebApp.Enums.LogTuru.Hata;
                        break;
                    default:
                        logTuru = MuhasebeStokWebApp.Enums.LogTuru.Bilgi;
                        break;
                }

                // Kullanıcı bilgilerini al
                var httpContext = _httpContextAccessor.HttpContext;
                var kullaniciAdi = httpContext?.User?.Identity?.Name ?? "Sistem";
                
                var log = new ESistemLog
                {
                    LogID = Guid.NewGuid(),
                    LogTuru = logTuru.ToString(),
                    LogTuruInt = (int)logTuru,
                    Aciklama = details,
                    IslemTuru = operation,
                    TabloAdi = tableName,
                    KayitAdi = details.Length > 50 ? details.Substring(0, 50) + "..." : details,
                    Mesaj = $"{operation} - {tableName}",
                    KullaniciAdi = kullaniciAdi,
                    IPAdresi = httpContext?.Connection?.RemoteIpAddress?.ToString() ?? "0.0.0.0",
                    IslemTarihi = DateTime.Now,
                    Basarili = true,
                    HataMesaji = string.Empty,
                    KullaniciId = entityIdString,
                    KullaniciGuid = entityIdString != null ? Guid.TryParse(entityIdString, out Guid guidResult) ? guidResult : (Guid?)null : null
                };

                _context.SistemLoglar.Add(log);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Log eklenirken hata oluştu: {Message}", ex.Message);
                return false;
            }
        }

        public async Task<bool> Log(string mesaj, MuhasebeStokWebApp.Enums.LogTuru logTuru, string detay = null)
        {
            try
            {
                var httpContext = _httpContextAccessor.HttpContext;
                var kullaniciAdi = httpContext?.User?.Identity?.Name ?? "Sistem";
                string? kullaniciId = null;
                
                if (httpContext?.User?.Identity?.IsAuthenticated == true)
                {
                    kullaniciId = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
                }
                
                var ipAdresi = httpContext?.Connection?.RemoteIpAddress?.ToString() ?? "0.0.0.0";
                var sayfa = httpContext?.Request?.Path.ToString() ?? "/";
                
                var log = new ESistemLog
                {
                    LogID = Guid.NewGuid(),
                    LogTuru = logTuru.ToString(),
                    LogTuruInt = (int)logTuru,
                    IslemTuru = logTuru.ToString(),
                    Aciklama = mesaj,
                    KullaniciAdi = kullaniciAdi,
                    IPAdresi = ipAdresi,
                    IslemTarihi = DateTime.Now,
                    Basarili = true,
                    TabloAdi = "",
                    KayitAdi = "",
                    Mesaj = mesaj,
                    HataMesaji = detay ?? "",
                    Sayfa = sayfa,
                    KullaniciId = kullaniciId,
                    KullaniciGuid = kullaniciId != null ? Guid.TryParse(kullaniciId, out Guid guidResult) ? guidResult : (Guid?)null : null
                };

                _context.SistemLoglar.Add(log);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Log eklenirken hata oluştu: {Message}", ex.Message);
                return false;
            }
        }

        public async Task AddSystemLogAsync(string logTuru, string mesaj, string sayfa)
        {
            try
            {
                var log = new ESistemLog
                {
                    LogID = Guid.NewGuid(),
                    LogTuru = logTuru,
                    IslemTuru = logTuru,
                    Mesaj = mesaj,
                    Aciklama = mesaj,
                    HataMesaji = "",
                    KullaniciAdi = "Sistem",
                    IPAdresi = "127.0.0.1",
                    IslemTarihi = DateTime.Now,
                    TabloAdi = "",
                    KayitAdi = "",
                    Sayfa = sayfa,
                    Basarili = true,
                    KullaniciId = null,
                    KullaniciGuid = null
                };

                _context.SistemLoglar.Add(log);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AddSystemLogAsync metodu çalışırken hata oluştu");
            }
        }

        public void LogBilgi(string baslik, string detay, string kullanici)
        {
            try
            {
                var log = new ESistemLog
                {
                    LogID = Guid.NewGuid(),
                    LogTuru = "Bilgi",
                    IslemTuru = "Bilgi",
                    LogTuruInt = (int)MuhasebeStokWebApp.Enums.LogTuru.Bilgi,
                    Aciklama = baslik,
                    HataMesaji = detay,
                    KullaniciAdi = kullanici,
                    IPAdresi = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "0.0.0.0",
                    IslemTarihi = DateTime.Now,
                    TabloAdi = "",
                    KayitAdi = "",
                    Mesaj = baslik,
                    Sayfa = _httpContextAccessor.HttpContext?.Request?.Path.ToString() ?? "",
                    Basarili = true,
                    KullaniciId = null,
                    KullaniciGuid = null
                };

                _context.SistemLoglar.Add(log);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                // Log eklerken oluşan hatayı sadece logla, uygulamanın çalışmasını etkilemesin
                _logger.LogError(ex, $"Log ekleme hatası: {baslik} - {ex.Message}");
            }
        }

        public void LogUyari(string baslik, string detay, string kullanici)
        {
            try
            {
                var log = new ESistemLog
                {
                    LogID = Guid.NewGuid(),
                    LogTuru = "Uyarı",
                    IslemTuru = "Uyarı",
                    LogTuruInt = (int)MuhasebeStokWebApp.Enums.LogTuru.Uyari,
                    Aciklama = baslik,
                    HataMesaji = detay,
                    KullaniciAdi = kullanici,
                    IPAdresi = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "0.0.0.0",
                    IslemTarihi = DateTime.Now,
                    TabloAdi = "",
                    KayitAdi = "",
                    Mesaj = baslik,
                    Sayfa = _httpContextAccessor.HttpContext?.Request?.Path.ToString() ?? "",
                    Basarili = true,
                    KullaniciId = null,
                    KullaniciGuid = null
                };

                _context.SistemLoglar.Add(log);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                // Log eklerken oluşan hatayı sadece logla, uygulamanın çalışmasını etkilemesin
                _logger.LogError(ex, $"Log ekleme hatası: {baslik} - {ex.Message}");
            }
        }

        public void LogHata(string baslik, string detay, string kullanici)
        {
            try
            {
                var log = new ESistemLog
                {
                    LogID = Guid.NewGuid(),
                    LogTuru = "Hata",
                    IslemTuru = "Hata",
                    LogTuruInt = (int)MuhasebeStokWebApp.Enums.LogTuru.Hata,
                    Aciklama = baslik,
                    HataMesaji = detay,
                    KullaniciAdi = kullanici,
                    IPAdresi = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "0.0.0.0",
                    IslemTarihi = DateTime.Now,
                    TabloAdi = "",
                    KayitAdi = "",
                    Mesaj = baslik,
                    Sayfa = _httpContextAccessor.HttpContext?.Request?.Path.ToString() ?? "",
                    Basarili = false,
                    KullaniciId = null,
                    KullaniciGuid = null
                };

                _context.SistemLoglar.Add(log);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                // Log eklerken oluşan hatayı sadece logla, uygulamanın çalışmasını etkilemesin
                _logger.LogError(ex, $"Log ekleme hatası: {baslik} - {ex.Message}");
            }
        }

        public async Task<bool> AddLogAsync(string logLevel, string message, string source)
        {
            try
            {
                MuhasebeStokWebApp.Enums.LogTuru logTuru;
                
                // LogLevel'i belirle
                switch (logLevel.ToLower())
                {
                    case "information":
                    case "info":
                    case "bilgi":
                        logTuru = MuhasebeStokWebApp.Enums.LogTuru.Bilgi;
                        break;
                    case "warning":
                    case "warn":
                    case "uyarı":
                    case "uyari":
                        logTuru = MuhasebeStokWebApp.Enums.LogTuru.Uyari;
                        break;
                    case "error":
                    case "hata":
                        logTuru = MuhasebeStokWebApp.Enums.LogTuru.Hata;
                        break;
                    case "critical":
                    case "kritik":
                        logTuru = MuhasebeStokWebApp.Enums.LogTuru.Kritik;
                        break;
                    default:
                        logTuru = MuhasebeStokWebApp.Enums.LogTuru.Bilgi;
                        break;
                }
                
                var log = new ESistemLog
                {
                    LogID = Guid.NewGuid(),
                    LogTuru = logTuru.ToString(),
                    LogTuruInt = (int)logTuru,
                    IslemTuru = logTuru.ToString(),
                    Aciklama = message,
                    HataMesaji = source,
                    KullaniciAdi = "Sistem",
                    IPAdresi = "0.0.0.0",
                    IslemTarihi = DateTime.Now,
                    TabloAdi = "",
                    KayitAdi = "",
                    Mesaj = message,
                    Sayfa = source,
                    Basarili = logTuru != MuhasebeStokWebApp.Enums.LogTuru.Hata && logTuru != MuhasebeStokWebApp.Enums.LogTuru.Kritik,
                    KullaniciId = null,
                    KullaniciGuid = null
                };

                _context.SistemLoglar.Add(log);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AddLogAsync metodu çalışırken hata oluştu");
                return false;
            }
        }

        // DbContext'i yeni bir scope'a taşımak için yardımcı sınıf
        private class DbContextScope : IDisposable
        {
            private readonly bool _disposeContext;
            public readonly ApplicationDbContext Context;
            
            public DbContextScope(ApplicationDbContext existingContext)
            {
                Context = existingContext;
                _disposeContext = false;
            }
            
            public DbContextScope(string connectionString)
            {
                var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                    .UseSqlServer(connectionString)
                    .Options;
                
                Context = new ApplicationDbContext(options);
                _disposeContext = true;
            }
            
            public void Dispose()
            {
                if (_disposeContext)
                {
                    Context.Dispose();
                }
            }
        }
    }
} 
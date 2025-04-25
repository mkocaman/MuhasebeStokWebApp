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
                var kullaniciID = httpContext != null && httpContext.User?.Identity?.IsAuthenticated == true ?
                    httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value : null;

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
                    KullaniciId = kullaniciID?.ToString()
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
                    KullaniciId = kayitID != null ? kayitID.ToString() : null
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
                    KullaniciId = kullaniciID?.ToString()
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
                var kullaniciID = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
                
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
                    KullaniciId = kullaniciID?.ToString()
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
                string kullaniciIDString = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
                Guid? kullaniciID = kullaniciIDString != null ? Guid.Parse(kullaniciIDString) : (Guid?)null;
                
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
                    KullaniciId = kullaniciID?.ToString()
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
                string kullaniciIDString = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
                Guid? kullaniciID = kullaniciIDString != null ? Guid.Parse(kullaniciIDString) : (Guid?)null;
                
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
                    KullaniciId = kullaniciID?.ToString()
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
                string kullaniciAdi = _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "Sistem";
                string ipAdresi = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "0.0.0.0";
                string kullaniciIDString = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
                Guid? kullaniciID = kullaniciIDString != null ? Guid.Parse(kullaniciIDString) : (Guid?)null;
                
                var log = new ESistemLog
                {
                    LogID = Guid.NewGuid(),
                    LogTuru = "Bilgi",
                    IslemTuru = "Cari Pasifleştirme",
                    LogTuruInt = (int)MuhasebeStokWebApp.Enums.LogTuru.Bilgi,
                    Aciklama = $"{ad} isimli cari kaydı pasif duruma getirildi. {aciklama}",
                    KullaniciAdi = kullaniciAdi,
                    IPAdresi = ipAdresi,
                    IslemTarihi = DateTime.Now,
                    Basarili = true,
                    TabloAdi = "Cariler",
                    KayitAdi = ad,
                    KayitID = cariID,
                    Mesaj = $"Cari Kaydı Pasifleştirildi: {ad}",
                    HataMesaji = string.Empty,
                    KullaniciId = kullaniciID?.ToString()
                };
                
                _context.SistemLoglar.Add(log);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                await LogErrorAsync("CariPasifleştirmeLogOlustur", ex.StackTrace, ex);
            }
        }
        
        public async Task CariAktifleştirmeLogOlustur(Guid cariID, string ad, string aciklama)
        {
            try
            {
                string kullaniciAdi = _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "Sistem";
                string ipAdresi = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "0.0.0.0";
                string kullaniciIDString = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
                Guid? kullaniciID = kullaniciIDString != null ? Guid.Parse(kullaniciIDString) : (Guid?)null;
                
                var log = new ESistemLog
                {
                    LogID = Guid.NewGuid(),
                    LogTuru = "Bilgi",
                    IslemTuru = "Cari Aktifleştirme",
                    LogTuruInt = (int)MuhasebeStokWebApp.Enums.LogTuru.Bilgi,
                    Aciklama = $"{ad} isimli cari kaydı aktif duruma getirildi. {aciklama}",
                    KullaniciAdi = kullaniciAdi,
                    IPAdresi = ipAdresi,
                    IslemTarihi = DateTime.Now,
                    Basarili = true,
                    TabloAdi = "Cariler",
                    KayitAdi = ad,
                    KayitID = cariID,
                    Mesaj = $"Cari Kaydı Aktifleştirildi: {ad}",
                    HataMesaji = string.Empty,
                    KullaniciId = kullaniciID?.ToString()
                };
                
                _context.SistemLoglar.Add(log);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                await LogErrorAsync("CariAktifleştirmeLogOlustur", ex.StackTrace, ex);
            }
        }
        
        public async Task CariHareketEklemeLogOlustur(Guid cariID, string ad, string hareketTuru, decimal tutar, string aciklama)
        {
            try
            {
                string kullaniciAdi = _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "Sistem";
                string ipAdresi = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "0.0.0.0";
                string kullaniciIDString = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
                Guid? kullaniciID = kullaniciIDString != null ? Guid.Parse(kullaniciIDString) : (Guid?)null;
                
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
                    KullaniciId = kullaniciID?.ToString()
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
                    KullaniciId = urunID.ToString()
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
                    KullaniciId = urunID.ToString()
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
                    KullaniciId = urunID.ToString()
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
                    KullaniciId = urunID.ToString()
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
                    KullaniciId = urunID.ToString()
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
                    KullaniciId = faturaID.ToString()
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
                var kullaniciAdi = _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "Sistem";
                var ipAdresi = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "Bilinmiyor";

                var log = new ESistemLog
                {
                    LogID = Guid.NewGuid(),
                    IslemTuru = "Fatura Güncelleme",
                    Aciklama = aciklama,
                    HataMesaji = "İşlem başarılı", // HataMesaji alanını ekliyoruz
                    KullaniciAdi = kullaniciAdi,
                    IPAdresi = ipAdresi,
                    IslemTarihi = DateTime.Now,
                    TabloAdi = "Faturalar",
                    KayitAdi = faturaNo,
                    KayitID = faturaID, // Doğrudan GUID kullanımı
                    Sayfa = string.Empty, // Sayfa alanını boş string ile dolduruyoruz
                    KullaniciId = faturaID.ToString()
                };

                _context.SistemLoglar.Add(log);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Loglama hatası durumunda sessizce devam et
                _logger.LogError(ex, "Fatura güncelleme logu oluşturulurken hata: {Message}", ex.Message);
            }
        }
        
        public async Task FaturaSilmeLogOlustur(Guid faturaID, string faturaNo, string aciklama)
        {
            try
            {
                var kullaniciAdi = _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "Sistem";
                var ipAdresi = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "Bilinmiyor";

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
                    KullaniciId = faturaID.ToString()
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
                var sayfaAdresi = httpContext != null ? httpContext.Request?.Path.ToString() ?? "/" : "/";
                var kullaniciID = httpContext != null && httpContext.User?.Identity?.IsAuthenticated == true ?
                    httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value : null;

                var log = new ESistemLog
                {
                    LogID = Guid.NewGuid(),
                    LogTuru = logTuru.ToString(),
                    IslemTuru = tabloAdi + " - " + logTuru.ToString(),
                    LogTuruInt = (int)logTuru,
                    Aciklama = mesaj,
                    HataMesaji = detay ?? "",
                    KullaniciAdi = kullaniciAdi,
                    IPAdresi = ipAdresi,
                    IslemTarihi = DateTime.Now,
                    TabloAdi = tabloAdi,
                    KayitAdi = kayitAdi,
                    KayitID = kayitID,
                    Mesaj = mesaj,
                    Sayfa = sayfaAdresi,
                    KullaniciId = kullaniciID?.ToString()
                };
                
                _context.SistemLoglar.Add(log);
                await _context.SaveChangesAsync();
                
                return log;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Detaylı log oluşturulurken hata: {Message}", ex.Message);
                return null;
            }
        }

        public async Task<bool> AddLogAsync(string tableName, string operation, string details, string entityId = null)
        {
            try
            {
                var logTuru = MuhasebeStokWebApp.Enums.LogTuru.Bilgi;
                
                // İşlem türüne göre log türünü belirle
                if (operation.Contains("Silme") || operation.Contains("Sil"))
                    logTuru = MuhasebeStokWebApp.Enums.LogTuru.Silme;
                else if (operation.Contains("Ekleme") || operation.Contains("Ekle") || operation.Contains("Oluştur"))
                    logTuru = MuhasebeStokWebApp.Enums.LogTuru.Ekleme;
                else if (operation.Contains("Güncelleme") || operation.Contains("Güncelle") || operation.Contains("Düzenle"))
                    logTuru = MuhasebeStokWebApp.Enums.LogTuru.Guncelleme;
                else if (operation.Contains("Hata") || operation.Contains("Error"))
                    logTuru = MuhasebeStokWebApp.Enums.LogTuru.Hata;
                
                var kullaniciAdi = _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "Sistem";
                var ipAdresi = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "Bilinmiyor";
                
                // details null ise boş string ata
                details = details ?? "Sistem işlemi";
                
                var log = new ESistemLog
                {
                    LogID = Guid.NewGuid(),
                    LogTuru = logTuru.ToString(),
                    IslemTuru = operation,
                    LogTuruInt = (int)logTuru,
                    Aciklama = details,
                    TabloAdi = tableName,
                    KayitID = string.IsNullOrEmpty(entityId) ? null : new Guid?(Guid.Parse(entityId)),
                    HataMesaji = "İşlem başarılı", // HataMesaji alanını ekliyoruz
                    KullaniciAdi = kullaniciAdi,
                    IPAdresi = ipAdresi,
                    IslemTarihi = DateTime.Now,
                    Basarili = true,
                    KayitAdi = string.Empty, // Eksik zorunlu alan
                    Mesaj = details, // Mesaj alanını dolduruyoruz
                    Sayfa = string.Empty, // Sayfa alanını boş string ile dolduruyoruz
                    KullaniciId = string.IsNullOrEmpty(entityId) ? null : entityId
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
                // Mesaj null ise boş string ata
                mesaj = mesaj ?? "Sistem işlemi";
                
                var kullaniciAdi = _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "Sistem";
                var ipAdresi = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "Bilinmiyor";
                var userAgent = _httpContextAccessor.HttpContext?.Request?.Headers["User-Agent"].ToString() ?? "Bilinmiyor";
                
                var tarayici = GetBrowserInfo(userAgent);
                var isletimSistemi = GetOSInfo(userAgent);
                
                // Kullanıcı ID'sini al
                string kullaniciID = null;
                if (_httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated == true)
                {
                    kullaniciID = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
                }
                
                var log = new ESistemLog
                {
                    LogID = Guid.NewGuid(),
                    LogTuru = logTuru.ToString(),
                    LogTuruInt = (int)logTuru,
                    IslemTuru = logTuru.ToString(),
                    Aciklama = mesaj,
                    HataMesaji = detay ?? "İşlem başarılı",
                    KullaniciAdi = kullaniciAdi,
                    IPAdresi = ipAdresi,
                    IslemTarihi = DateTime.Now,
                    TabloAdi = string.Empty,
                    KayitAdi = string.Empty,
                    Mesaj = mesaj,
                    Sayfa = string.Empty,
                    KullaniciId = kullaniciID?.ToString()
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
            var log = new ESistemLog
            {
                LogID = Guid.NewGuid(),
                LogTuru = logTuru ?? "Bilgi",
                LogTuruInt = 0,
                Mesaj = mesaj ?? "Sistem log kaydı",
                Sayfa = sayfa ?? string.Empty,
                OlusturmaTarihi = DateTime.Now,
                Aciklama = mesaj ?? "Sistem log kaydı",
                HataMesaji = "Sistem log kaydı",
                KullaniciAdi = "Sistem",
                IslemTarihi = DateTime.Now,
                IPAdresi = "127.0.0.1",
                TabloAdi = string.Empty,
                KayitAdi = string.Empty,
                IslemTuru = logTuru ?? "Bilgi",
                KullaniciId = null
            };

            await _context.SistemLoglar.AddAsync(log);
            await _context.SaveChangesAsync();
        }

        public void LogBilgi(string baslik, string detay, string kullanici)
        {
            try
            {
                using (var scope = new DbContextScope(_context))
                {
                    var ipAdresi = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "Bilinmiyor";
                    var sayfaAdresi = _httpContextAccessor.HttpContext?.Request?.Path.ToString() ?? string.Empty;

                    var log = new ESistemLog
                    {
                        LogID = Guid.NewGuid(),
                        LogTuru = "Bilgi",
                        IslemTuru = baslik,
                        LogTuruInt = (int)MuhasebeStokWebApp.Enums.LogTuru.Bilgi,
                        Aciklama = detay,
                        HataMesaji = "",
                        KullaniciAdi = kullanici ?? "Sistem",
                        IPAdresi = ipAdresi,
                        IslemTarihi = DateTime.Now,
                        Basarili = true,
                        TabloAdi = "",
                        KayitAdi = "",
                        Mesaj = baslik,
                        Sayfa = sayfaAdresi,
                        KullaniciId = null
                    };

                    scope.Context.SistemLoglar.Add(log);
                    scope.Context.SaveChanges();
                }
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
                using (var scope = new DbContextScope(_context))
                {
                    var ipAdresi = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "Bilinmiyor";
                    var sayfaAdresi = _httpContextAccessor.HttpContext?.Request?.Path.ToString() ?? string.Empty;

                    var log = new ESistemLog
                    {
                        LogID = Guid.NewGuid(),
                        LogTuru = "Uyari",
                        IslemTuru = baslik,
                        LogTuruInt = (int)MuhasebeStokWebApp.Enums.LogTuru.Uyari,
                        Aciklama = detay,
                        HataMesaji = "",
                        KullaniciAdi = kullanici ?? "Sistem",
                        IPAdresi = ipAdresi,
                        IslemTarihi = DateTime.Now,
                        Basarili = false,
                        TabloAdi = "",
                        KayitAdi = "",
                        Mesaj = baslik,
                        Sayfa = sayfaAdresi,
                        KullaniciId = null
                    };

                    scope.Context.SistemLoglar.Add(log);
                    scope.Context.SaveChanges();
                }
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
                using (var scope = new DbContextScope(_context))
                {
                    var ipAdresi = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "Bilinmiyor";
                    var sayfaAdresi = _httpContextAccessor.HttpContext?.Request?.Path.ToString() ?? string.Empty;

                    var log = new ESistemLog
                    {
                        LogID = Guid.NewGuid(),
                        LogTuru = "Hata",
                        IslemTuru = baslik,
                        LogTuruInt = (int)MuhasebeStokWebApp.Enums.LogTuru.Hata,
                        Aciklama = detay,
                        HataMesaji = detay,
                        KullaniciAdi = kullanici ?? "Sistem",
                        IPAdresi = ipAdresi,
                        IslemTarihi = DateTime.Now,
                        Basarili = false,
                        TabloAdi = "",
                        KayitAdi = "",
                        Mesaj = baslik,
                        Sayfa = sayfaAdresi,
                        KullaniciId = null
                    };

                    scope.Context.SistemLoglar.Add(log);
                    scope.Context.SaveChanges();
                }
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
                LogTuru logTuru;
                
                // string olarak gelen logLevel değerini doğru şekilde LogTuru enum değerine çevirelim
                switch (logLevel.ToLower())
                {
                    case "info":
                    case "information":
                    case "bilgi":
                        logTuru = LogTuru.Bilgi;
                        break;
                    case "warning":
                    case "warn":
                    case "uyari":
                        logTuru = LogTuru.Uyari;
                        break;
                    case "error":
                    case "hata":
                        logTuru = LogTuru.Hata;
                        break;
                    default:
                        logTuru = LogTuru.Bilgi;
                        break;
                }
                
                return await LogEkleAsync(message, logTuru, source);
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
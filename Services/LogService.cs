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
                var kullaniciAdi = _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "Sistem";
                var ipAdresi = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "Bilinmiyor";
                var userAgent = _httpContextAccessor.HttpContext?.Request?.Headers["User-Agent"].ToString() ?? "Bilinmiyor";

                // Tarayıcı bilgisini maksimum 1000 karakter ile sınırla
                if (userAgent.Length > 1000)
                {
                    userAgent = userAgent.Substring(0, 1000);
                }

                var log = new ESistemLog
                {
                    LogID = Guid.NewGuid(),
                    IslemTuru = logTuru.ToString(),
                    LogTuru = (int)logTuru,
                    Aciklama = mesaj,
                    HataMesaji = detay,
                    KullaniciAdi = kullaniciAdi,
                    IPAdresi = ipAdresi,
                    IslemTarihi = DateTime.Now,
                    Basarili = true,
                    TabloAdi = string.Empty,
                    KayitAdi = string.Empty
                };

                _context.SistemLoglar.Add(log);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                // Loglama hatası durumunda sessizce devam et
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
                    query = query.Where(l => l.LogTuru == (int)logTuruEnum);
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

        // LogOlustur metodunda da aynı düzeltmeyi yapalım
        public async Task<bool> LogOlustur(string mesaj, MuhasebeStokWebApp.Enums.LogTuru logTuru, string tabloAdi, string kayitAdi, Guid? kayitID, string kullaniciAdi = null, bool basarili = true)
        {
            try
            {
                var ipAdresi = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "Bilinmiyor";
                kullaniciAdi ??= _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "Sistem";

                var userAgent = _httpContextAccessor.HttpContext?.Request?.Headers["User-Agent"].ToString() ?? "Bilinmiyor";
                var tarayici = GetBrowserInfo(userAgent);
                var isletimSistemi = GetOSInfo(userAgent);

                var log = new ESistemLog
                {
                    LogID = Guid.NewGuid(),
                    IslemTuru = mesaj,
                    LogTuru = (int)logTuru,
                    TabloAdi = tabloAdi,
                    KayitAdi = kayitAdi,
                    KayitID = kayitID,
                    Aciklama = string.Empty,
                    KullaniciAdi = kullaniciAdi,
                    IPAdresi = ipAdresi,
                    IslemTarihi = DateTime.Now,
                    Basarili = true
                };

                _context.SistemLoglar.Add(log);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Log oluşturulurken hata: {Message}", ex.Message);
                return false;
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
                    LogTuru = (int)MuhasebeStokWebApp.Enums.LogTuru.Giris,
                    TabloAdi = "AspNetUsers",
                    KayitAdi = kullaniciAdi,
                    KayitID = Guid.TryParse(kullaniciID, out Guid guidResult) ? guidResult : (Guid?)null,
                    Aciklama = $"{kullaniciAdi} kullanıcısı sisteme giriş yaptı.",
                    KullaniciAdi = kullaniciAdi,
                    IPAdresi = ipAdresi,
                    IslemTarihi = DateTime.Now,
                    Basarili = true
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

        public async Task CariOlusturmaLogOlustur(Guid cariID, string cariAdi, string aciklama)
        {
            try
            {
                var kullaniciAdi = _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "Sistem";
                var ipAdresi = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "Bilinmiyor";

                var log = new ESistemLog
                {
                    LogID = Guid.NewGuid(),
                    IslemTuru = "Cari Oluşturma",
                    Aciklama = aciklama,
                    KullaniciAdi = kullaniciAdi,
                    IPAdresi = ipAdresi,
                    IslemTarihi = DateTime.Now,
                    TabloAdi = "Cariler",
                    KayitAdi = cariAdi,
                    KayitID = cariID // Doğrudan GUID kullanımı
                };

                _context.SistemLoglar.Add(log);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Loglama hatası durumunda sessizce devam et
                _logger.LogError(ex, "Cari oluşturma logu oluşturulurken hata: {Message}", ex.Message);
            }
        }
        
        public async Task CariGuncellemeLogOlustur(Guid cariID, string cariAdi, string aciklama)
        {
            try
            {
                var kullaniciAdi = _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "Sistem";
                var ipAdresi = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "Bilinmiyor";

                var log = new ESistemLog
                {
                    LogID = Guid.NewGuid(),
                    IslemTuru = "Cari Güncelleme",
                    Aciklama = aciklama,
                    KullaniciAdi = kullaniciAdi,
                    IPAdresi = ipAdresi,
                    IslemTarihi = DateTime.Now,
                    TabloAdi = "Cariler",
                    KayitAdi = cariAdi,
                    KayitID = cariID // Doğrudan GUID kullanımı
                };

                _context.SistemLoglar.Add(log);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Loglama hatası durumunda sessizce devam et
                _logger.LogError(ex, "Cari güncelleme logu oluşturulurken hata: {Message}", ex.Message);
            }
        }
        
        public async Task CariSilmeLogOlustur(Guid cariID, string cariAdi, string aciklama)
        {
            try
            {
                var kullaniciAdi = _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "Sistem";
                var ipAdresi = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "Bilinmiyor";

                var log = new ESistemLog
                {
                    LogID = Guid.NewGuid(),
                    IslemTuru = "Cari Silme",
                    Aciklama = aciklama,
                    KullaniciAdi = kullaniciAdi,
                    IPAdresi = ipAdresi,
                    IslemTarihi = DateTime.Now,
                    TabloAdi = "Cariler",
                    KayitAdi = cariAdi,
                    KayitID = cariID // Doğrudan GUID kullanımı
                };

                _context.SistemLoglar.Add(log);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Loglama hatası durumunda sessizce devam et
                _logger.LogError(ex, "Cari silme logu oluşturulurken hata: {Message}", ex.Message);
            }
        }
        
        public async Task CariPasifleştirmeLogOlustur(Guid cariID, string cariAdi, string aciklama)
        {
            try
            {
                var kullaniciAdi = _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "Sistem";
                var ipAdresi = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "Bilinmiyor";

                var log = new ESistemLog
                {
                    LogID = Guid.NewGuid(),
                    IslemTuru = "Cari Pasifleştirme",
                    Aciklama = aciklama,
                    KullaniciAdi = kullaniciAdi,
                    IPAdresi = ipAdresi,
                    IslemTarihi = DateTime.Now,
                    TabloAdi = "Cariler",
                    KayitAdi = cariAdi,
                    KayitID = cariID // Doğrudan GUID kullanımı
                };

                _context.SistemLoglar.Add(log);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Loglama hatası durumunda sessizce devam et
                _logger.LogError(ex, "Cari pasifleştirme logu oluşturulurken hata: {Message}", ex.Message);
            }
        }
        
        public async Task CariAktifleştirmeLogOlustur(Guid cariID, string cariAdi, string aciklama)
        {
            try
            {
                var kullaniciAdi = _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "Sistem";
                var ipAdresi = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "Bilinmiyor";

                var log = new ESistemLog
                {
                    LogID = Guid.NewGuid(),
                    IslemTuru = "Cari Aktifleştirme",
                    Aciklama = aciklama,
                    KullaniciAdi = kullaniciAdi,
                    IPAdresi = ipAdresi,
                    IslemTarihi = DateTime.Now,
                    TabloAdi = "Cariler",
                    KayitAdi = cariAdi,
                    KayitID = cariID // Doğrudan GUID kullanımı
                };

                _context.SistemLoglar.Add(log);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Loglama hatası durumunda sessizce devam et
                _logger.LogError(ex, "Cari aktifleştirme logu oluşturulurken hata: {Message}", ex.Message);
            }
        }
        
        public async Task CariHareketEklemeLogOlustur(Guid cariID, string cariAdi, string hareketTuru, decimal tutar, string aciklama)
        {
            // Cari hareket bilgileri içeren log açıklaması
            var logAciklama = $"{cariAdi} cari hesabına {tutar.ToString("N2")} TL tutarında {hareketTuru} hareketi eklendi. Açıklama: {aciklama}";

            var kullaniciID = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var kullaniciAdi = _httpContextAccessor.HttpContext?.User.Identity?.Name;
            
            // Sistem log oluştur
            var log = new ESistemLog
            {
                LogID = Guid.NewGuid(),
                IslemTarihi = DateTime.Now,
                KullaniciID = !string.IsNullOrEmpty(kullaniciID) ? Guid.Parse(kullaniciID) : (Guid?)null,
                KullaniciAdi = kullaniciAdi ?? "Sistem",
                Aciklama = logAciklama,
                IslemTuru = "Cari Hareket Ekleme",
                KayitID = cariID,
                TabloAdi = "Cariler",
                KayitAdi = cariAdi,
                Basarili = true,
                IPAdresi = "Sistem" // IPAdresi zorunlu alan ekledik
            };
            
            // Veritabanına kaydet
            _context.SistemLoglar.Add(log);
            await _context.SaveChangesAsync();
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
                    KullaniciAdi = kullaniciAdi,
                    IPAdresi = ipAdresi,
                    IslemTarihi = DateTime.Now,
                    TabloAdi = "Urunler",
                    KayitAdi = urunAdi,
                    KayitID = urunID // Doğrudan GUID kullanımı
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
                    KullaniciAdi = kullaniciAdi,
                    IPAdresi = ipAdresi,
                    IslemTarihi = DateTime.Now,
                    TabloAdi = "Urunler",
                    KayitAdi = urunAdi,
                    KayitID = urunID // Doğrudan GUID kullanımı
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
                    KullaniciAdi = kullaniciAdi,
                    IPAdresi = ipAdresi,
                    IslemTarihi = DateTime.Now,
                    TabloAdi = "Urunler",
                    KayitAdi = urunAdi,
                    KayitID = urunID // Doğrudan GUID kullanımı
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
                    LogTuru = (int)MuhasebeStokWebApp.Enums.LogTuru.Bilgi,
                    KayitID = urunID, // urunID doğrudan Guid olarak kullan, ToString() yapmadan
                    TabloAdi = "StokHareketler",
                    KayitAdi = urunAdi,
                    Aciklama = $"{urunAdi} ürününe {miktar} adet giriş yapıldı. Açıklama: {aciklama}",
                    KullaniciAdi = kullaniciAdi,
                    IPAdresi = ipAdresi,
                    IslemTarihi = DateTime.Now,
                    Basarili = true
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
                    LogTuru = (int)MuhasebeStokWebApp.Enums.LogTuru.Bilgi,
                    KayitID = urunID, // urunID doğrudan Guid olarak kullan, ToString() yapmadan
                    TabloAdi = "StokHareketler",
                    KayitAdi = urunAdi,
                    Aciklama = $"{urunAdi} ürününden {miktar} adet çıkış yapıldı. Açıklama: {aciklama}",
                    KullaniciAdi = kullaniciAdi,
                    IPAdresi = ipAdresi,
                    IslemTarihi = DateTime.Now,
                    Basarili = true
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
            string detail = ex.ToString();
            return await LogEkleAsync(message, MuhasebeStokWebApp.Enums.LogTuru.Hata, detail);
        }

        public async Task<bool> LogErrorAsync(string operation, string message)
        {
            return await LogEkleAsync(operation + ": " + message, MuhasebeStokWebApp.Enums.LogTuru.Hata);
        }

        public async Task LogErrorAsync(string operation, string stackTrace, Exception ex)
        {
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
                    KullaniciAdi = kullaniciAdi,
                    IPAdresi = ipAdresi,
                    IslemTarihi = DateTime.Now,
                    TabloAdi = "Faturalar",
                    KayitAdi = faturaNo,
                    KayitID = faturaID // Doğrudan GUID kullanımı
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
                    KullaniciAdi = kullaniciAdi,
                    IPAdresi = ipAdresi,
                    IslemTarihi = DateTime.Now,
                    TabloAdi = "Faturalar",
                    KayitAdi = faturaNo,
                    KayitID = faturaID // Doğrudan GUID kullanımı
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
                    KullaniciAdi = kullaniciAdi,
                    IPAdresi = ipAdresi,
                    IslemTarihi = DateTime.Now,
                    TabloAdi = "Faturalar",
                    KayitAdi = faturaNo,
                    KayitID = faturaID // Doğrudan GUID kullanımı
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
                var kullaniciAdi = _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "Sistem";
                var ipAdresi = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "Bilinmiyor";

                var tarayici = string.Empty;
                var isletimSistemi = string.Empty;

                if (_httpContextAccessor.HttpContext != null && _httpContextAccessor.HttpContext.Request.Headers.ContainsKey("User-Agent"))
                {
                    var userAgent = _httpContextAccessor.HttpContext.Request.Headers["User-Agent"].ToString();
                    tarayici = GetBrowserInfo(userAgent);
                    isletimSistemi = GetOSInfo(userAgent);
                }

                var log = new ESistemLog
                {
                    LogID = Guid.NewGuid(),
                    IslemTuru = mesaj,
                    LogTuru = (int)logTuru,
                    TabloAdi = tabloAdi,
                    KayitAdi = kayitAdi,
                    KayitID = kayitID,
                    Aciklama = detay ?? string.Empty,
                    KullaniciAdi = kullaniciAdi,
                    IPAdresi = ipAdresi,
                    IslemTarihi = DateTime.Now,
                    Basarili = true
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
                
                var log = new ESistemLog
                {
                    LogID = Guid.NewGuid(),
                    IslemTuru = operation,
                    LogTuru = (int)logTuru,
                    Aciklama = details,
                    TabloAdi = tableName,
                    KayitID = string.IsNullOrEmpty(entityId) ? null : new Guid?(Guid.Parse(entityId)),
                    KullaniciAdi = kullaniciAdi,
                    IPAdresi = ipAdresi,
                    IslemTarihi = DateTime.Now,
                    Basarili = true,
                    KayitAdi = string.Empty // Eksik zorunlu alan
                };
                
                _context.SistemLoglar.Add(log);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Log oluşturulurken hata: {Message}", ex.Message);
                return false;
            }
        }

        public async Task<bool> Log(string mesaj, MuhasebeStokWebApp.Enums.LogTuru logTuru, string detay = null)
        {
            return await LogEkleAsync(mesaj, logTuru, detay);
        }
    }
} 
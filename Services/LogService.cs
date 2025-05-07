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
using MuhasebeStokWebApp.Data.Repositories;

namespace MuhasebeStokWebApp.Services
{
    public class LogService : ILogService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<LogService> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public LogService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor, ILogger<LogService> logger, IUnitOfWork unitOfWork)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
            _unitOfWork = unitOfWork;
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
                    LogID = Guid.NewGuid().ToString(),
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

                await _context.SistemLoglar.AddAsync(log);
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

        // LogOlustur metodu (public olarak tanımlandı)
        public async Task<bool> LogOlustur(
            string mesaj,
            MuhasebeStokWebApp.Enums.LogTuru logTuru,
            string? tabloAdi = null,
            string? kayitAdi = null,
            string? kayitID = null,
            string? kullaniciAdi = null,
            bool basarili = true,
            string? kategori = null,
            string? aciklama = null
        )
        {
            try
            {
                var sistemLog = new ESistemLog
                {
                    LogID = Guid.NewGuid().ToString(),
                    IslemTarihi = DateTime.Now,
                    Mesaj = mesaj,
                    LogTuru = logTuru.ToString(),
                    TabloAdi = tabloAdi,
                    KayitAdi = kayitAdi,
                    KayitID = kayitID,
                    KullaniciAdi = kullaniciAdi ?? _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "Sistem",
                    Basarili = basarili,
                    Aciklama = aciklama ?? mesaj // Aciklama null ise mesaj değerini kullan
                };

                await _context.SistemLoglar.AddAsync(sistemLog);
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Log kaydı oluşturulurken hata: {Message}", ex.Message);
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
                    LogID = Guid.NewGuid().ToString(),
                    IslemTuru = "Oturum Açma",
                    LogTuruInt = (int)MuhasebeStokWebApp.Enums.LogTuru.Giris,
                    TabloAdi = "AspNetUsers",
                    KayitAdi = kullaniciAdi,
                    KayitID = kullaniciID,
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

                await _context.SistemLoglar.AddAsync(log);
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
            try
            {
                var query = _context.SistemLoglar.AsQueryable();
                
                // LogTuru filtreleme
                query = query.Where(l => l.LogTuru == logTuru.ToString());
                
                // Tarih aralığı filtreleme
                if (baslangicTarihi.HasValue)
                {
                    query = query.Where(l => l.IslemTarihi >= baslangicTarihi.Value);
                }
                
                if (bitisTarihi.HasValue)
                {
                    query = query.Where(l => l.IslemTarihi <= bitisTarihi.Value);
                }
                
                // Sıralama ve çekme
                var logs = await query
                    .OrderByDescending(l => l.IslemTarihi)
                    .ToListAsync();
                
                return logs.Select(l => new MSistemLog
                {
                    LogID = l.LogID,
                    IslemTuru = l.IslemTuru,
                    Aciklama = l.Aciklama,
                    TabloAdi = l.TabloAdi,
                    KayitAdi = l.KayitAdi,
                    KayitID = l.KayitID,
                    KullaniciAdi = l.KullaniciAdi,
                    IPAdresi = l.IPAdresi,
                    IslemTarihi = l.IslemTarihi,
                    LogTuru = l.LogTuru
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Log bilgileri alınırken hata: {Message}", ex.Message);
                return new List<MSistemLog>();
            }
        }
        
        public async Task<List<MSistemLog>> GetCariLogsAsync(string cariId)
        {
            try
            {
                var logs = await _context.SistemLoglar
                    .Where(l => l.TabloAdi == "Cari" && l.KayitID == cariId)
                    .OrderByDescending(l => l.IslemTarihi)
                    .ToListAsync();

                return logs.Select(l => new MSistemLog
                {
                    LogID = l.LogID,
                    IslemTuru = l.IslemTuru,
                    Aciklama = l.Aciklama,
                    TabloAdi = l.TabloAdi,
                    KayitAdi = l.KayitAdi,
                    KayitID = l.KayitID,
                    KullaniciAdi = l.KullaniciAdi,
                    IPAdresi = l.IPAdresi,
                    IslemTarihi = l.IslemTarihi,
                    LogTuru = l.LogTuru
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Cari logları alınamadı, ID: {CariID}, Hata: {Message}", cariId, ex.Message);
                return new List<MSistemLog>();
            }
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
        
        public async Task CariOlusturmaLogOlustur(string cariID, string cariAdi, string aciklama)
        {
            await LogOlustur(
                mesaj: $"Yeni cari kaydı oluşturuldu",
                logTuru: MuhasebeStokWebApp.Enums.LogTuru.Bilgi,
                tabloAdi: "Cari",
                kayitAdi: cariAdi,
                kayitID: cariID,
                kullaniciAdi: _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "Sistem",
                basarili: true,
                kategori: "Cari Yönetimi",
                aciklama: aciklama
            );
        }
        
        public async Task CariGuncellemeLogOlustur(string cariID, string cariAdi, string aciklama)
        {
            await LogOlustur(
                mesaj: $"Cari kaydı güncellendi",
                logTuru: MuhasebeStokWebApp.Enums.LogTuru.Bilgi,
                tabloAdi: "Cari",
                kayitAdi: cariAdi,
                kayitID: cariID,
                kullaniciAdi: _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "Sistem",
                basarili: true,
                kategori: "Cari Yönetimi",
                aciklama: aciklama
            );
        }
        
        public async Task CariSilmeLogOlustur(string cariID, string cariAdi, string aciklama)
        {
            await LogOlustur(
                mesaj: $"Cari kaydı silindi",
                logTuru: MuhasebeStokWebApp.Enums.LogTuru.Uyari,
                tabloAdi: "Cari",
                kayitAdi: cariAdi,
                kayitID: cariID,
                kullaniciAdi: _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "Sistem",
                basarili: true,
                kategori: "Cari Yönetimi",
                aciklama: aciklama
            );
        }
        
        public async Task CariPasifleştirmeLogOlustur(string cariID, string cariAdi, string aciklama)
        {
            await LogOlustur(
                mesaj: $"Cari pasif duruma alındı",
                logTuru: MuhasebeStokWebApp.Enums.LogTuru.Uyari,
                tabloAdi: "Cari",
                kayitAdi: cariAdi,
                kayitID: cariID,
                kullaniciAdi: _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "Sistem",
                basarili: true,
                kategori: "Cari Yönetimi",
                aciklama: aciklama
            );
        }
        
        public async Task CariAktifleştirmeLogOlustur(string cariID, string cariAdi, string aciklama)
        {
            await LogOlustur(
                mesaj: $"Cari aktif duruma alındı",
                logTuru: MuhasebeStokWebApp.Enums.LogTuru.Bilgi,
                tabloAdi: "Cari",
                kayitAdi: cariAdi,
                kayitID: cariID,
                kullaniciAdi: _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "Sistem",
                basarili: true,
                kategori: "Cari Yönetimi",
                aciklama: aciklama
            );
        }

        public async Task CariHareketEklemeLogOlustur(string cariID, string ad, string hareketTuru, decimal tutar, string aciklama)
        {
             await LogOlustur(
                mesaj: $"{ad} carisine {tutar} tutarında {hareketTuru} hareketi eklendi.",
                logTuru: MuhasebeStokWebApp.Enums.LogTuru.Bilgi,
                tabloAdi: "CariHareket",
                kayitAdi: $"{ad} - {hareketTuru}",
                kayitID: cariID
            );
        }
        
        public async Task UrunOlusturmaLogOlustur(string urunID, string urunAdi, string aciklama)
        {
            await LogOlustur(
                mesaj: $"Yeni ürün kaydı oluşturuldu",
                logTuru: MuhasebeStokWebApp.Enums.LogTuru.Bilgi,
                tabloAdi: "Urun",
                kayitAdi: urunAdi,
                kayitID: urunID,
                kullaniciAdi: _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "Sistem",
                basarili: true,
                kategori: "Stok Yönetimi"
            );
        }
        
        public async Task UrunGuncellemeLogOlustur(string urunID, string urunAdi, string aciklama)
        {
            await LogOlustur(
                mesaj: $"Ürün bilgileri güncellendi",
                logTuru: MuhasebeStokWebApp.Enums.LogTuru.Bilgi,
                tabloAdi: "Urun",
                kayitAdi: urunAdi,
                kayitID: urunID,
                kullaniciAdi: _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "Sistem",
                basarili: true,
                kategori: "Stok Yönetimi"
            );
        }
        
        public async Task UrunSilmeLogOlustur(string urunID, string urunAdi, string aciklama = null)
        {
            await LogOlustur(
                mesaj: $"Ürün silindi",
                logTuru: MuhasebeStokWebApp.Enums.LogTuru.Uyari,
                tabloAdi: "Urun",
                kayitAdi: urunAdi,
                kayitID: urunID,
                kullaniciAdi: _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "Sistem",
                basarili: true,
                kategori: "Stok Yönetimi"
            );
        }
        
        public async Task UrunPasifleştirmeLogOlustur(string urunID, string urunAdi)
        {
            await LogOlustur(
                mesaj: $"{urunAdi} isimli ürün pasifleştirildi.",
                logTuru: MuhasebeStokWebApp.Enums.LogTuru.Bilgi,
                tabloAdi: "Urun",
                kayitAdi: urunAdi,
                kayitID: urunID,
                kullaniciAdi: _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "Sistem",
                basarili: true,
                kategori: "Stok Yönetimi"
            );
        }
        
        public async Task UrunAktifleştirmeLogOlustur(string urunID, string urunAdi)
        {
            await LogOlustur(
                mesaj: $"{urunAdi} isimli ürün aktifleştirildi.",
                logTuru: MuhasebeStokWebApp.Enums.LogTuru.Bilgi,
                tabloAdi: "Urun",
                kayitAdi: urunAdi,
                kayitID: urunID,
                kullaniciAdi: _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "Sistem",
                basarili: true,
                kategori: "Stok Yönetimi"
            );
        }
        
        public async Task StokGirisLogOlustur(string stokHareketID, string urunID, string urunAdi, decimal miktar, string aciklama)
        {
            await LogOlustur(
                mesaj: $"{urunAdi} ürününe {miktar} adet stok girişi yapıldı.",
                logTuru: MuhasebeStokWebApp.Enums.LogTuru.Bilgi,
                tabloAdi: "StokHareket",
                kayitAdi: $"{urunAdi} - Giriş",
                kayitID: stokHareketID
            );
        }
        
        public async Task StokCikisLogOlustur(string stokHareketID, string urunID, string urunAdi, decimal miktar, string aciklama)
        {
            await LogOlustur(
                mesaj: $"{urunAdi} ürününden {miktar} adet stok çıkışı yapıldı.",
                logTuru: MuhasebeStokWebApp.Enums.LogTuru.Bilgi,
                tabloAdi: "StokHareket",
                kayitAdi: $"{urunAdi} - Çıkış",
                kayitID: stokHareketID
            );
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

        public async Task FaturaOlusturmaLogOlustur(string faturaID, string faturaNo, string aciklama)
        {
             await LogOlustur(
                mesaj: $"{faturaNo} numaralı yeni fatura oluşturuldu.",
                logTuru: MuhasebeStokWebApp.Enums.LogTuru.Bilgi,
                tabloAdi: "Fatura",
                kayitAdi: faturaNo,
                kayitID: faturaID
            );
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
        
        public async Task FaturaGuncellemeLogOlustur(string faturaID, string faturaNo, string aciklama)
        {
             await LogOlustur(
                mesaj: $"{faturaNo} numaralı fatura güncellendi.",
                logTuru: MuhasebeStokWebApp.Enums.LogTuru.Bilgi,
                tabloAdi: "Fatura",
                kayitAdi: faturaNo,
                kayitID: faturaID
            );
        }

        public async Task FaturaSilmeLogOlustur(string faturaID, string faturaNo, string aciklama)
        {
            await LogOlustur(
                mesaj: $"{faturaNo} numaralı fatura silindi.",
                logTuru: MuhasebeStokWebApp.Enums.LogTuru.Uyari, 
                tabloAdi: "Fatura",
                kayitAdi: faturaNo,
                kayitID: faturaID
            );
        }

        public async Task<ESistemLog> DetayliLogEkleAsync(string mesaj, MuhasebeStokWebApp.Enums.LogTuru logTuru, string tabloAdi, string kayitAdi, string? kayitID, string detay = null)
        {
            try
            {
                var log = new ESistemLog
                {
                    LogID = Guid.NewGuid().ToString(),
                    LogTuru = logTuru.ToString(),
                    LogTuruInt = (int)logTuru,
                    IslemTuru = logTuru.ToString(),
                    Aciklama = mesaj,
                    HataMesaji = detay ?? "",
                    KullaniciAdi = _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "Sistem",
                    IPAdresi = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "0.0.0.0",
                    IslemTarihi = DateTime.Now,
                    TabloAdi = tabloAdi,
                    KayitAdi = kayitAdi,
                    KayitID = kayitID,
                    Mesaj = mesaj,
                    Sayfa = _httpContextAccessor.HttpContext?.Request?.Path.ToString() ?? "/",
                    Basarili = true
                };

                await _context.SistemLoglar.AddAsync(log);
                await _context.SaveChangesAsync();
                return log;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Detaylı log ekleme hatası: {Message}", ex.Message);
                return null;
            }
        }

        public async Task<bool> AddLogAsync(string tableName, string operation, string details, string? entityId = null)
        {
            try
            {
                var httpContext = _httpContextAccessor.HttpContext;
                var kullaniciAdi = httpContext != null ? httpContext.User?.Identity?.Name ?? "Anonim" : "Sistem";
                var ipAdresi = httpContext != null ? httpContext.Connection?.RemoteIpAddress?.ToString() ?? "0.0.0.0" : "0.0.0.0";
                var sayfaAdresi = httpContext != null ? httpContext.Request?.Path.ToString() ?? "/" : "/";
                string? kullaniciId = GetCurrentUserId();
                Guid? kullaniciGuid = GetCurrentUserGuid();

                // Log türünü belirle (varsayılan Information)
                var logTuru = MuhasebeStokWebApp.Enums.LogTuru.Bilgi;
                if (operation.Contains("Sil", StringComparison.OrdinalIgnoreCase) || operation.Contains("Delete", StringComparison.OrdinalIgnoreCase) || operation.Contains("Hata", StringComparison.OrdinalIgnoreCase))
                {
                    logTuru = MuhasebeStokWebApp.Enums.LogTuru.Uyari;
                }
                if (operation.Contains("Exception", StringComparison.OrdinalIgnoreCase) || operation.Contains("Error", StringComparison.OrdinalIgnoreCase))
                {
                    logTuru = MuhasebeStokWebApp.Enums.LogTuru.Hata;
                }

                // Kayıt adını bulmaya çalış (şimdilik ID'yi kullanalım)
                string? kayitAdi = entityId;

                var log = new ESistemLog
                {
                    LogID = Guid.NewGuid().ToString(),
                    LogTuru = logTuru.ToString(),
                    LogTuruInt = (int)logTuru,
                    IslemTuru = operation,
                    Aciklama = details,
                    HataMesaji = logTuru >= MuhasebeStokWebApp.Enums.LogTuru.Uyari ? details : "",
                    KullaniciAdi = kullaniciAdi,
                    IPAdresi = ipAdresi,
                    IslemTarihi = DateTime.Now,
                    TabloAdi = tableName,
                    KayitAdi = kayitAdi ?? "Bilinmiyor",
                    KayitID = entityId,
                    Mesaj = details,
                    Sayfa = sayfaAdresi,
                    Basarili = logTuru < MuhasebeStokWebApp.Enums.LogTuru.Uyari,
                    KullaniciId = kullaniciId,
                    KullaniciGuid = kullaniciGuid
                };

                await _context.SistemLoglar.AddAsync(log);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AddLogAsync hatası: {Message}", ex.Message);
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
                    LogID = Guid.NewGuid().ToString(),
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

                await _context.SistemLoglar.AddAsync(log);
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
                    LogID = Guid.NewGuid().ToString(),
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

                await _context.SistemLoglar.AddAsync(log);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AddSystemLogAsync metodu çalışırken hata oluştu");
            }
        }

        public async Task<bool> LogBilgi(string baslik, string detay, string kullanici)
        {
            try
            {
                // HTTP bağlam bilgilerini al
                var httpContext = _httpContextAccessor.HttpContext;
                string ipAdresi = httpContext?.Connection?.RemoteIpAddress?.ToString() ?? "0.0.0.0";
                string browser = GetBrowserInfo(httpContext?.Request?.Headers["User-Agent"].ToString() ?? "");
                string os = GetOSInfo(httpContext?.Request?.Headers["User-Agent"].ToString() ?? "");
                string sayfaAdresi = httpContext?.Request?.Path.ToString() ?? "/";
                
                // Kullanıcı ID'sini al
                string kullaniciId = null;
                if (httpContext?.User?.Identity?.IsAuthenticated == true)
                {
                    kullaniciId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                }
                
                // Log kaydını oluştur
                var logKaydi = new ESistemLog
                {
                    LogID = Guid.NewGuid().ToString(),
                    LogTuru = "Bilgi",
                    LogTuruInt = (int)LogTuru.Bilgi,
                    IslemTuru = "Bilgi",
                    Mesaj = baslik,
                    Aciklama = baslik,
                    HataMesaji = detay,
                    TabloAdi = "Sistem",
                    KayitAdi = "Bilgi",
                    KullaniciAdi = kullanici,
                    KullaniciId = kullaniciId,
                    IPAdresi = ipAdresi,
                    IslemTarihi = DateTime.Now,
                    Sayfa = sayfaAdresi,
                    Basarili = true,
                    KullaniciGuid = kullaniciId != null ? Guid.TryParse(kullaniciId, out Guid guidResult) ? guidResult : (Guid?)null : null
                };
                
                // Veritabanına ekle
                await _context.SistemLoglar.AddAsync(logKaydi);
                await _context.SaveChangesAsync();
                
                // Debug log ekle
                _logger.LogDebug($"Bilgi logu kaydedildi: {baslik} - {kullanici}");
                
                return true;
            }
            catch (Exception ex)
            {
                // Hata oluşursa debug seviyesinde logla ama işlemi engelleme
                _logger.LogError(ex, $"Bilgi logu kaydedilirken hata oluştu: {baslik}");
                return false;
            }
        }

        public async Task<bool> LogUyari(string baslik, string detay, string kullanici)
        {
            try
            {
                var log = new ESistemLog
                {
                    LogID = Guid.NewGuid().ToString(),
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

                await _context.SistemLoglar.AddAsync(log);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Log ekleme hatası: {baslik} - {ex.Message}");
                return false;
            }
        }

        public async Task<bool> LogHata(string baslik, string detay, string kullanici)
        {
            try
            {
                var log = new ESistemLog
                {
                    LogID = Guid.NewGuid().ToString(),
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
                
                await _context.SistemLoglar.AddAsync(log);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Log ekleme hatası: {baslik} - {ex.Message}");
                return false;
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
                    LogID = Guid.NewGuid().ToString(),
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

                await _context.SistemLoglar.AddAsync(log);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AddLogAsync metodu çalışırken hata oluştu");
                return false;
            }
        }

        // Birim loglama metodları
        public async Task BirimOlusturmaLogOlustur(string birimID, string birimAdi)
        {
            await LogOlustur(
                mesaj: $"Yeni birim kaydı oluşturuldu",
                logTuru: MuhasebeStokWebApp.Enums.LogTuru.Bilgi,
                tabloAdi: "Birim",
                kayitAdi: birimAdi,
                kayitID: birimID,
                kullaniciAdi: _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "Sistem",
                basarili: true,
                kategori: "Tanımlama İşlemleri"
            );
        }

        public async Task BirimGuncellemeLogOlustur(string birimID, string birimAdi)
        {
            await LogOlustur(
                mesaj: $"Birim bilgileri güncellendi",
                logTuru: MuhasebeStokWebApp.Enums.LogTuru.Bilgi,
                tabloAdi: "Birim",
                kayitAdi: birimAdi,
                kayitID: birimID,
                kullaniciAdi: _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "Sistem",
                basarili: true,
                kategori: "Tanımlama İşlemleri"
            );
        }

        public async Task BirimSilmeLogOlustur(string birimID, string birimAdi)
        {
            await LogOlustur(
                mesaj: $"Birim silindi",
                logTuru: MuhasebeStokWebApp.Enums.LogTuru.Uyari,
                tabloAdi: "Birim",
                kayitAdi: birimAdi,
                kayitID: birimID,
                kullaniciAdi: _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "Sistem",
                basarili: true,
                kategori: "Tanımlama İşlemleri"
            );
        }

        public async Task BirimDurumDegisikligiLogOlustur(string birimID, string birimAdi, bool yeniDurum)
        {
            string durum = yeniDurum ? "aktifleştirildi" : "pasifleştirildi";
            await LogOlustur(
                mesaj: $"{birimAdi} isimli birim {durum}.",
                logTuru: MuhasebeStokWebApp.Enums.LogTuru.Bilgi,
                tabloAdi: "Birim",
                kayitAdi: birimAdi,
                kayitID: birimID
            );
        }

        // Kategori loglama metodları
        public async Task KategoriOlusturmaLogOlustur(string kategoriID, string kategoriAdi)
        {
            await LogOlustur(
                mesaj: $"Yeni kategori kaydı oluşturuldu",
                logTuru: MuhasebeStokWebApp.Enums.LogTuru.Bilgi,
                tabloAdi: "Kategori",
                kayitAdi: kategoriAdi,
                kayitID: kategoriID,
                kullaniciAdi: _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "Sistem",
                basarili: true,
                kategori: "Tanımlama İşlemleri"
            );
        }

        public async Task KategoriGuncellemeLogOlustur(string kategoriID, string kategoriAdi)
        {
            await LogOlustur(
                mesaj: $"Kategori bilgileri güncellendi",
                logTuru: MuhasebeStokWebApp.Enums.LogTuru.Bilgi,
                tabloAdi: "Kategori",
                kayitAdi: kategoriAdi,
                kayitID: kategoriID,
                kullaniciAdi: _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "Sistem",
                basarili: true,
                kategori: "Tanımlama İşlemleri"
            );
        }

        public async Task KategoriSilmeLogOlustur(string kategoriID, string kategoriAdi)
        {
            await LogOlustur(
                mesaj: $"Kategori silindi",
                logTuru: MuhasebeStokWebApp.Enums.LogTuru.Uyari,
                tabloAdi: "Kategori",
                kayitAdi: kategoriAdi,
                kayitID: kategoriID,
                kullaniciAdi: _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "Sistem",
                basarili: true,
                kategori: "Tanımlama İşlemleri"
            );
        }

        public async Task KategoriDurumDegisikligiLogOlustur(string kategoriID, string kategoriAdi, bool yeniDurum)
        {
            string durum = yeniDurum ? "aktifleştirildi" : "pasifleştirildi";
            await LogOlustur(
                mesaj: $"{kategoriAdi} isimli kategori {durum}.",
                logTuru: MuhasebeStokWebApp.Enums.LogTuru.Bilgi,
                tabloAdi: "Kategori",
                kayitAdi: kategoriAdi,
                kayitID: kategoriID
            );
        }

        // Depo loglama metodları
        public async Task DepoOlusturmaLogOlustur(string depoID, string depoAdi)
        {
            await LogOlustur(
                mesaj: $"Yeni depo kaydı oluşturuldu",
                logTuru: MuhasebeStokWebApp.Enums.LogTuru.Bilgi,
                tabloAdi: "Depo",
                kayitAdi: depoAdi,
                kayitID: depoID,
                kullaniciAdi: _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "Sistem",
                basarili: true,
                kategori: "Depo Yönetimi"
            );
        }

        public async Task DepoGuncellemeLogOlustur(string depoID, string depoAdi)
        {
            await LogOlustur(
                mesaj: $"Depo bilgileri güncellendi",
                logTuru: MuhasebeStokWebApp.Enums.LogTuru.Bilgi,
                tabloAdi: "Depo",
                kayitAdi: depoAdi,
                kayitID: depoID,
                kullaniciAdi: _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "Sistem",
                basarili: true,
                kategori: "Depo Yönetimi"
            );
        }

        public async Task DepoSilmeLogOlustur(string depoID, string depoAdi)
        {
            await LogOlustur(
                mesaj: $"Depo silindi",
                logTuru: MuhasebeStokWebApp.Enums.LogTuru.Uyari,
                tabloAdi: "Depo",
                kayitAdi: depoAdi,
                kayitID: depoID,
                kullaniciAdi: _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "Sistem",
                basarili: true,
                kategori: "Depo Yönetimi"
            );
        }

        public async Task DepoDurumDegisikligiLogOlustur(string depoID, string depoAdi, bool yeniDurum)
        {
            string durum = yeniDurum ? "aktifleştirildi" : "pasifleştirildi";
            await LogOlustur(
                mesaj: $"{depoAdi} isimli depo {durum}.",
                logTuru: MuhasebeStokWebApp.Enums.LogTuru.Bilgi,
                tabloAdi: "Depo",
                kayitAdi: depoAdi,
                kayitID: depoID
            );
        }

        // Stok kritik seviye log metodları
        public async Task StokKritikSeviyeLogOlustur(string urunID, string urunAdi, decimal miktar, decimal kritikSeviye)
        {
            await LogOlustur(
                mesaj: $"{urunAdi} ürününün stok seviyesi kritik seviyenin altına düştü: {miktar}/{kritikSeviye}",
                logTuru: MuhasebeStokWebApp.Enums.LogTuru.Uyari,
                tabloAdi: "Urun",
                kayitAdi: urunAdi,
                kayitID: urunID,
                kullaniciAdi: _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "Sistem",
                basarili: true,
                kategori: "Stok Yönetimi"
            );
        }
        
        public async Task SistemAyarlariDegisiklikLogOlustur(string ayarAdi, string eskiDeger, string yeniDeger)
        {
            await LogOlustur(
                mesaj: $"{ayarAdi} sistem ayarı değiştirildi: {eskiDeger} -> {yeniDeger}",
                logTuru: MuhasebeStokWebApp.Enums.LogTuru.Bilgi,
                tabloAdi: "SistemAyarlari",
                kayitAdi: ayarAdi,
                kayitID: null,
                kullaniciAdi: _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "Sistem",
                basarili: true,
                kategori: "Sistem"
            );
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

        private string? GetCurrentUserId()
        {
            return _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        private Guid? GetCurrentUserGuid()
        {
            var userIdClaim = GetCurrentUserId();
            if (Guid.TryParse(userIdClaim, out Guid userGuid))
            {
                return userGuid;
            }
            return null;
        }
    }
} 
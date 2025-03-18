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

namespace MuhasebeStokWebApp.Services
{
    public class LogService : ILogService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public LogService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task LogEkleAsync(string mesaj, Models.LogTuru logTuru, string detay = null)
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

                var log = new Data.Entities.SistemLog
                {
                    LogID = Guid.NewGuid(),
                    IslemTuru = logTuru.ToString(),
                    Aciklama = mesaj,
                    KullaniciAdi = kullaniciAdi,
                    IPAdresi = ipAdresi,
                    IslemTarihi = DateTime.Now,
                    TabloAdi = "Sistem",
                    KayitAdi = detay
                };

                _context.SistemLoglar.Add(log);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                // Loglama hatası durumunda sessizce devam et
            }
        }

        public async Task<IEnumerable<Models.SistemLog>> GetLogsAsync(DateTime? baslangicTarihi = null, DateTime? bitisTarihi = null)
        {
            var query = _context.SistemLoglar.AsQueryable();

            if (baslangicTarihi.HasValue)
                query = query.Where(l => l.IslemTarihi >= baslangicTarihi.Value);

            if (bitisTarihi.HasValue)
                query = query.Where(l => l.IslemTarihi <= bitisTarihi.Value);

            var logs = await query.OrderByDescending(l => l.IslemTarihi).ToListAsync();
            
            return logs.Select(l => new Models.SistemLog
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

        public async Task<IEnumerable<Models.SistemLog>> GetLogsByKullaniciAsync(string kullaniciAdi, DateTime? baslangicTarihi = null, DateTime? bitisTarihi = null)
        {
            var query = _context.SistemLoglar.AsQueryable();

            if (!string.IsNullOrEmpty(kullaniciAdi))
                query = query.Where(l => l.KullaniciAdi == kullaniciAdi);

            if (baslangicTarihi.HasValue)
                query = query.Where(l => l.IslemTarihi >= baslangicTarihi.Value);

            if (bitisTarihi.HasValue)
                query = query.Where(l => l.IslemTarihi <= bitisTarihi.Value);

            var logs = await query.OrderByDescending(l => l.IslemTarihi).ToListAsync();
            
            return logs.Select(l => new Models.SistemLog
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

        public async Task<IEnumerable<Models.SistemLog>> GetLogsByTurAsync(Models.LogTuru logTuru, DateTime? baslangicTarihi = null, DateTime? bitisTarihi = null)
        {
            var query = _context.SistemLoglar.AsQueryable();

            query = query.Where(l => l.IslemTuru == logTuru.ToString());

            if (baslangicTarihi.HasValue)
                query = query.Where(l => l.IslemTarihi >= baslangicTarihi.Value);

            if (bitisTarihi.HasValue)
                query = query.Where(l => l.IslemTarihi <= bitisTarihi.Value);

            var logs = await query.OrderByDescending(l => l.IslemTarihi).ToListAsync();
            
            return logs.Select(l => new Models.SistemLog
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

        public async Task<IEnumerable<Models.SistemLog>> GetCariLogsAsync(Guid cariId)
        {
            var query = _context.SistemLoglar.AsQueryable()
                .Where(l => l.IlgiliID == cariId.ToString());
                
            var logs = await query
                .OrderByDescending(l => l.IslemTarihi)
                .ToListAsync();
                
            return logs.Select(l => new Models.SistemLog
            {
                SistemLogID = l.LogID.GetHashCode(),
                IslemTuru = l.IslemTuru,
                Aciklama = l.Aciklama,
                KullaniciAdi = l.KullaniciAdi,
                IPAdresi = l.IPAdresi,
                IslemTarihi = l.IslemTarihi,
                IlgiliID = l.IlgiliID,
                KullaniciID = l.KullaniciID
            }).ToList();
        }

        public async Task ClearLogsAsync()
        {
            _context.SistemLoglar.RemoveRange(_context.SistemLoglar);
            await _context.SaveChangesAsync();
        }
        
        public async Task DeleteLogAsync(int logId)
        {
            var entityLog = await _context.SistemLoglar.FirstOrDefaultAsync(l => l.LogID.GetHashCode() == logId);
            if (entityLog != null)
            {
                _context.SistemLoglar.Remove(entityLog);
                await _context.SaveChangesAsync();
            }
        }

        public async Task CariOlusturmaLogOlustur(Guid cariID, string cariAdi, string aciklama)
        {
            var logAciklama = $"Cari Oluşturma: {cariAdi} - {aciklama}";
            
            // Giriş yapmış kullanıcıyı al
            var kullaniciID = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var kullaniciAdi = _httpContextAccessor.HttpContext?.User.Identity?.Name;
            
            // Sistem log oluştur
            var log = new Data.Entities.SistemLog
            {
                LogID = Guid.NewGuid(),
                IslemTarihi = DateTime.Now,
                KullaniciID = kullaniciID,
                KullaniciAdi = kullaniciAdi ?? "Sistem",
                Aciklama = logAciklama,
                IslemTuru = "Cari Oluşturma",
                KayitID = cariID, // Doğrudan Guid olarak kaydet
                TabloAdi = "Cariler",
                KayitAdi = cariAdi,
                Basarili = true,
                IlgiliID = cariID.ToString()
            };
            
            // Veritabanına kaydet
            _context.SistemLoglar.Add(log);
            await _context.SaveChangesAsync();
        }
        
        public async Task CariGuncellemeLogOlustur(Guid cariID, string cariAdi, string aciklama)
        {
            var logAciklama = $"Cari Güncelleme: {cariAdi} - {aciklama}";
            
            // Giriş yapmış kullanıcıyı al
            var kullaniciID = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var kullaniciAdi = _httpContextAccessor.HttpContext?.User.Identity?.Name;
            
            // Sistem log oluştur
            var log = new Data.Entities.SistemLog
            {
                LogID = Guid.NewGuid(),
                IslemTarihi = DateTime.Now,
                KullaniciID = kullaniciID,
                KullaniciAdi = kullaniciAdi ?? "Sistem",
                Aciklama = logAciklama,
                IslemTuru = "Cari Güncelleme",
                KayitID = cariID, // Doğrudan Guid olarak kaydet
                TabloAdi = "Cariler",
                KayitAdi = cariAdi,
                Basarili = true,
                IlgiliID = cariID.ToString()
            };
            
            // Veritabanına kaydet
            _context.SistemLoglar.Add(log);
            await _context.SaveChangesAsync();
        }
        
        public async Task CariSilmeLogOlustur(Guid cariID, string cariAdi, string aciklama)
        {
            var logAciklama = $"Cari Silme: {cariAdi} - {aciklama}";
            
            // Giriş yapmış kullanıcıyı al
            var kullaniciID = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var kullaniciAdi = _httpContextAccessor.HttpContext?.User.Identity?.Name;
            
            // Sistem log oluştur
            var log = new Data.Entities.SistemLog
            {
                LogID = Guid.NewGuid(),
                IslemTarihi = DateTime.Now,
                KullaniciID = kullaniciID,
                KullaniciAdi = kullaniciAdi ?? "Sistem",
                Aciklama = logAciklama,
                IslemTuru = "Cari Silme",
                KayitID = cariID, // Doğrudan Guid olarak kaydet
                TabloAdi = "Cariler",
                KayitAdi = cariAdi,
                Basarili = true,
                IlgiliID = cariID.ToString()
            };
            
            // Veritabanına kaydet
            _context.SistemLoglar.Add(log);
            await _context.SaveChangesAsync();
        }
        
        public async Task CariPasifleştirmeLogOlustur(Guid cariID, string cariAdi, string aciklama)
        {
            var logAciklama = $"Cari Pasifleştirme: {cariAdi} - {aciklama}";
            
            // Giriş yapmış kullanıcıyı al
            var kullaniciID = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var kullaniciAdi = _httpContextAccessor.HttpContext?.User.Identity?.Name;
            
            // Sistem log oluştur
            var log = new Data.Entities.SistemLog
            {
                LogID = Guid.NewGuid(),
                IslemTarihi = DateTime.Now,
                KullaniciID = kullaniciID,
                KullaniciAdi = kullaniciAdi ?? "Sistem",
                Aciklama = logAciklama,
                IslemTuru = "Cari Pasifleştirme",
                KayitID = cariID, // Doğrudan Guid olarak kaydet
                TabloAdi = "Cariler",
                KayitAdi = cariAdi,
                Basarili = true,
                IlgiliID = cariID.ToString()
            };
            
            // Veritabanına kaydet
            _context.SistemLoglar.Add(log);
            await _context.SaveChangesAsync();
        }
        
        public async Task CariAktifleştirmeLogOlustur(Guid cariID, string cariAdi, string aciklama)
        {
            var logAciklama = $"Cari Aktifleştirme: {cariAdi} - {aciklama}";
            
            // Giriş yapmış kullanıcıyı al
            var kullaniciID = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var kullaniciAdi = _httpContextAccessor.HttpContext?.User.Identity?.Name;
            
            // Sistem log oluştur
            var log = new Data.Entities.SistemLog
            {
                LogID = Guid.NewGuid(),
                IslemTarihi = DateTime.Now,
                KullaniciID = kullaniciID,
                KullaniciAdi = kullaniciAdi ?? "Sistem",
                Aciklama = logAciklama,
                IslemTuru = "Cari Aktifleştirme",
                KayitID = cariID, // Doğrudan Guid olarak kaydet
                TabloAdi = "Cariler",
                KayitAdi = cariAdi,
                Basarili = true,
                IlgiliID = cariID.ToString()
            };
            
            // Veritabanına kaydet
            _context.SistemLoglar.Add(log);
            await _context.SaveChangesAsync();
        }
        
        public async Task CariHareketEklemeLogOlustur(Guid cariID, string cariAdi, string hareketTuru, decimal tutar, string aciklama)
        {
            var logAciklama = $"Cari Hareket: {cariAdi} - {hareketTuru} - {tutar:N2} TL - {aciklama}";
            
            // Giriş yapmış kullanıcıyı al
            var kullaniciID = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var kullaniciAdi = _httpContextAccessor.HttpContext?.User.Identity?.Name;
            
            // Sistem log oluştur
            var log = new Data.Entities.SistemLog
            {
                LogID = Guid.NewGuid(),
                IslemTarihi = DateTime.Now,
                KullaniciID = kullaniciID,
                KullaniciAdi = kullaniciAdi ?? "Sistem",
                Aciklama = logAciklama,
                IslemTuru = "Cari Hareket Ekleme",
                KayitID = cariID, // Doğrudan Guid olarak kaydet
                TabloAdi = "Cariler",
                KayitAdi = cariAdi,
                Basarili = true,
                IlgiliID = cariID.ToString()
            };
            
            // Veritabanına kaydet
            _context.SistemLoglar.Add(log);
            await _context.SaveChangesAsync();
        }

        public async Task UrunOlusturmaLogOlustur(Guid urunID, string urunAdi, string aciklama)
        {
            var logAciklama = $"Ürün Oluşturma: {urunAdi} - {aciklama}";
            
            // Giriş yapmış kullanıcıyı al
            var kullaniciID = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var kullaniciAdi = _httpContextAccessor.HttpContext?.User.Identity?.Name;
            
            // Sistem log oluştur
            var log = new Data.Entities.SistemLog
            {
                LogID = Guid.NewGuid(),
                IslemTarihi = DateTime.Now,
                KullaniciID = kullaniciID,
                KullaniciAdi = kullaniciAdi ?? "Sistem",
                Aciklama = logAciklama,
                IslemTuru = "Ürün Oluşturma",
                KayitID = urunID, // Doğrudan Guid olarak kaydet
                TabloAdi = "Urunler",
                KayitAdi = urunAdi,
                Basarili = true,
                IlgiliID = urunID.ToString()
            };
            
            // Veritabanına kaydet
            _context.SistemLoglar.Add(log);
            await _context.SaveChangesAsync();
        }
        
        public async Task UrunGuncellemeLogOlustur(Guid urunID, string urunAdi, string aciklama)
        {
            var logAciklama = $"Ürün Güncelleme: {urunAdi} - {aciklama}";
            
            // Giriş yapmış kullanıcıyı al
            var kullaniciID = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var kullaniciAdi = _httpContextAccessor.HttpContext?.User.Identity?.Name;
            
            // Sistem log oluştur
            var log = new Data.Entities.SistemLog
            {
                LogID = Guid.NewGuid(),
                IslemTarihi = DateTime.Now,
                KullaniciID = kullaniciID,
                KullaniciAdi = kullaniciAdi ?? "Sistem",
                Aciklama = logAciklama,
                IslemTuru = "Ürün Güncelleme",
                KayitID = urunID, // Doğrudan Guid olarak kaydet
                TabloAdi = "Urunler",
                KayitAdi = urunAdi,
                Basarili = true,
                IlgiliID = urunID.ToString()
            };
            
            // Veritabanına kaydet
            _context.SistemLoglar.Add(log);
            await _context.SaveChangesAsync();
        }
        
        public async Task UrunSilmeLogOlustur(Guid urunID, string urunAdi, string aciklama)
        {
            var logAciklama = $"Ürün Silme: {urunAdi} - {aciklama}";
            
            // Giriş yapmış kullanıcıyı al
            var kullaniciID = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var kullaniciAdi = _httpContextAccessor.HttpContext?.User.Identity?.Name;
            
            // Sistem log oluştur
            var log = new Data.Entities.SistemLog
            {
                LogID = Guid.NewGuid(),
                IslemTarihi = DateTime.Now,
                KullaniciID = kullaniciID,
                KullaniciAdi = kullaniciAdi ?? "Sistem",
                Aciklama = logAciklama,
                IslemTuru = "Ürün Silme",
                KayitID = urunID, // Doğrudan Guid olarak kaydet
                TabloAdi = "Urunler",
                KayitAdi = urunAdi,
                Basarili = true,
                IlgiliID = urunID.ToString()
            };
            
            // Veritabanına kaydet
            _context.SistemLoglar.Add(log);
            await _context.SaveChangesAsync();
        }
        
        public async Task StokGirisLogOlustur(Guid stokHareketID, Guid urunID, string urunAdi, decimal miktar, string aciklama)
        {
            var logAciklama = $"Stok Giriş: {urunAdi} - {miktar} adet - {aciklama}";
            
            // Giriş yapmış kullanıcıyı al
            var kullaniciID = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var kullaniciAdi = _httpContextAccessor.HttpContext?.User.Identity?.Name;
            
            // Sistem log oluştur
            var log = new Data.Entities.SistemLog
            {
                LogID = Guid.NewGuid(),
                IslemTarihi = DateTime.Now,
                KullaniciID = kullaniciID,
                KullaniciAdi = kullaniciAdi ?? "Sistem",
                Aciklama = logAciklama,
                IslemTuru = "Stok Giriş",
                KayitID = stokHareketID, // Doğrudan Guid olarak kaydet
                TabloAdi = "StokHareketler",
                KayitAdi = urunAdi,
                Basarili = true,
                IlgiliID = urunID.ToString()
            };
            
            // Veritabanına kaydet
            _context.SistemLoglar.Add(log);
            await _context.SaveChangesAsync();
        }
        
        public async Task StokCikisLogOlustur(Guid stokHareketID, Guid urunID, string urunAdi, decimal miktar, string aciklama)
        {
            var logAciklama = $"Stok Çıkış: {urunAdi} - {miktar} adet - {aciklama}";
            
            // Giriş yapmış kullanıcıyı al
            var kullaniciID = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var kullaniciAdi = _httpContextAccessor.HttpContext?.User.Identity?.Name;
            
            // Sistem log oluştur
            var log = new Data.Entities.SistemLog
            {
                LogID = Guid.NewGuid(),
                IslemTarihi = DateTime.Now,
                KullaniciID = kullaniciID,
                KullaniciAdi = kullaniciAdi ?? "Sistem",
                Aciklama = logAciklama,
                IslemTuru = "Stok Çıkış",
                KayitID = stokHareketID, // Doğrudan Guid olarak kaydet
                TabloAdi = "StokHareketler",
                KayitAdi = urunAdi,
                Basarili = true,
                IlgiliID = urunID.ToString()
            };
            
            // Veritabanına kaydet
            _context.SistemLoglar.Add(log);
            await _context.SaveChangesAsync();
        }
        
        public async Task LogErrorAsync(string category, Exception ex)
        {
            var message = $"{category}: {ex.Message}";
            await LogEkleAsync(message, LogTuru.Hata, ex.StackTrace);
        }
        
        public async Task LogInfoAsync(string category, string message)
        {
            var logMessage = $"{category}: {message}";
            await LogEkleAsync(logMessage, LogTuru.Bilgi);
        }
        
        public async Task LogWarningAsync(string category, string message)
        {
            var logMessage = $"{category}: {message}";
            await LogEkleAsync(logMessage, LogTuru.Uyari);
        }
    }
} 
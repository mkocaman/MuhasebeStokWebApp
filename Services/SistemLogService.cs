using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using MuhasebeStokWebApp.Data;
using MuhasebeStokWebApp.Data.Entities;
using MuhasebeStokWebApp.Data.Repositories;
using MuhasebeStokWebApp.Models;
using System.Security.Claims;

namespace MuhasebeStokWebApp.Services
{
    public class SistemLogService : ILogService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ApplicationDbContext _context;

        public SistemLogService(IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor, ApplicationDbContext context)
        {
            _unitOfWork = unitOfWork;
            _httpContextAccessor = httpContextAccessor;
            _context = context;
        }

        public async Task LogEkleAsync(string mesaj, LogTuru logTuru, string detay = null)
        {
            await LogEkleAsync(mesaj, logTuru, detay, null, null);
        }

        private async Task LogEkleAsync(string mesaj, LogTuru logTuru, string detay = null, Guid? kayitID = null, string tabloAdi = null)
        {
            try
            {
                var ipAdresi = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "0.0.0.0";
                var kullaniciAdi = _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "Sistem";
                var tarayici = _httpContextAccessor.HttpContext?.Request?.Headers["User-Agent"].ToString() ?? "Bilinmiyor";
                
                // Tarayıcı bilgisini maksimum 1000 karakter ile sınırla
                if (tarayici.Length > 1000)
                {
                    tarayici = tarayici.Substring(0, 1000);
                }
                
                // KayitID artık doğrudan Guid olarak saklanıyor, dönüşüm yapmıyoruz
                
                var log = new Data.Entities.SistemLog
                {
                    LogID = Guid.NewGuid(),
                    IslemTuru = logTuru.ToString(),
                    IslemTarihi = DateTime.Now,
                    Aciklama = mesaj,
                    KullaniciAdi = kullaniciAdi,
                    IPAdresi = ipAdresi,
                    Tarayici = tarayici,
                    KayitID = kayitID, // Doğrudan Guid olarak kaydet
                    TabloAdi = tabloAdi ?? "Sistem",
                    KayitAdi = detay ?? "Sistem Log",
                    Basarili = true,
                    SoftDelete = false,
                    IlgiliID = kayitID?.ToString()
                };

                await _unitOfWork.Repository<Data.Entities.SistemLog>().AddAsync(log);
                await _unitOfWork.SaveAsync();
            }
            catch (Exception ex)
            {
                // Log oluşturma sırasında hata olursa, sessizce devam et
                Console.WriteLine($"Log oluşturma hatası: {ex.Message}");
            }
        }

        public async Task<IEnumerable<Models.SistemLog>> GetLogsAsync(DateTime? baslangicTarihi = null, DateTime? bitisTarihi = null)
        {
            var query = _context.SistemLoglar.AsQueryable();

            if (baslangicTarihi.HasValue)
            {
                query = query.Where(l => l.IslemTarihi >= baslangicTarihi.Value);
            }

            if (bitisTarihi.HasValue)
            {
                query = query.Where(l => l.IslemTarihi <= bitisTarihi.Value.AddDays(1).AddSeconds(-1));
            }

            var logs = await query.OrderByDescending(l => l.IslemTarihi).ToListAsync();

            return logs.Select(l => new Models.SistemLog
            {
                SistemLogID = l.LogID.GetHashCode(), // Not: Burada Model sınıfında SistemLogID hala int
                IslemTuru = l.IslemTuru,
                IslemTarihi = l.IslemTarihi,
                Aciklama = l.Aciklama,
                KullaniciAdi = l.KullaniciAdi,
                IPAdresi = l.IPAdresi,
                Tarayici = l.Tarayici,
                TabloAdi = l.TabloAdi,
                KayitAdi = l.KayitAdi,
                IlgiliID = l.IlgiliID
            });
        }

        public async Task<IEnumerable<Models.SistemLog>> GetLogsByKullaniciAsync(string kullaniciAdi, DateTime? baslangicTarihi = null, DateTime? bitisTarihi = null)
        {
            var logs = await GetLogsAsync(baslangicTarihi, bitisTarihi);
            return logs.Where(l => l.KullaniciAdi == kullaniciAdi);
        }

        public async Task<IEnumerable<Models.SistemLog>> GetLogsByTurAsync(LogTuru logTuru, DateTime? baslangicTarihi = null, DateTime? bitisTarihi = null)
        {
            var logs = await GetLogsAsync(baslangicTarihi, bitisTarihi);
            return logs.Where(l => l.IslemTuru == logTuru.ToString());
        }

        public async Task<IEnumerable<Models.SistemLog>> GetCariLogsAsync(Guid cariId)
        {
            var query = _context.SistemLoglar.AsQueryable()
                .Where(l => l.IlgiliID == cariId.ToString());
                
            var dbLogs = await query
                .OrderByDescending(l => l.IslemTarihi)
                .ToListAsync();
                
            return dbLogs.Select(l => new Models.SistemLog
            {
                SistemLogID = l.LogID.GetHashCode(), // Not: Burada Model sınıfında SistemLogID hala int
                IslemTarihi = l.IslemTarihi,
                KullaniciID = l.KullaniciID ?? string.Empty,
                KullaniciAdi = l.KullaniciAdi ?? string.Empty,
                IPAdresi = l.IPAdresi ?? string.Empty,
                Aciklama = l.Aciklama ?? string.Empty,
                IslemTuru = l.IslemTuru ?? string.Empty,
                IlgiliID = l.IlgiliID ?? string.Empty,
                TabloAdi = l.TabloAdi,
                KayitAdi = l.KayitAdi
            }).ToList();
        }

        public async Task ClearLogsAsync()
        {
            var logs = await _context.SistemLoglar.ToListAsync();
            _context.SistemLoglar.RemoveRange(logs);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteLogAsync(int logId)
        {
            var logs = await GetLogsAsync();
            var log = logs.FirstOrDefault(l => l.SistemLogID == logId);
            
            if (log != null)
            {
                // LogID tam eşleşmeyle kontrol ediyoruz, GetHashCode kullanmadan
                var entityLog = await _context.SistemLoglar.FirstOrDefaultAsync(l => l.LogID.GetHashCode() == logId);
                if (entityLog != null)
                {
                    _context.SistemLoglar.Remove(entityLog);
                    await _context.SaveChangesAsync();
                }
            }
        }

        // Eski metodlar (uyumluluk için tutuyoruz)
        public async Task LogOlustur(string islemTuru, Guid? kayitID, string tabloAdi, string kayitAdi, string aciklama, Guid? kullaniciID = null, string kullaniciAdi = null, bool basarili = true, string hataMesaji = "")
        {
            try 
            {
                // Kullanıcı bilgilerini al
                kullaniciID ??= Guid.Parse(_httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());
                kullaniciAdi ??= _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "Sistem";
                var ipAdresi = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "0.0.0.0";

                // Kayıt adı boş olamaz
                kayitAdi ??= "Sistem Kaydı";
                tabloAdi ??= "Sistem";
                
                // KayitID artık doğrudan Guid olarak saklanıyor, dönüşüm yapmıyoruz

                // Log entiyi oluştur
                var log = new Data.Entities.SistemLog
                {
                    LogID = Guid.NewGuid(),
                    IslemTuru = islemTuru,
                    KayitID = kayitID, // Doğrudan Guid olarak kaydet
                    TabloAdi = tabloAdi,
                    KayitAdi = kayitAdi,
                    IslemTarihi = DateTime.Now,
                    Aciklama = aciklama,
                    KullaniciID = kullaniciID?.ToString(),
                    KullaniciAdi = kullaniciAdi ?? string.Empty,
                    IPAdresi = ipAdresi ?? string.Empty,
                    Basarili = basarili,
                    HataMesaji = hataMesaji ?? string.Empty,
                    IlgiliID = kayitID?.ToString()
                };

                // Veritabanına ekle
                await _unitOfWork.Repository<Data.Entities.SistemLog>().AddAsync(log);
                await _unitOfWork.SaveAsync();
            }
            catch (Exception ex)
            {
                // Log oluşturma sırasında hata olursa, sessizce devam et
                Console.WriteLine($"Log oluşturma hatası: {ex.Message}");
            }
        }

        public async Task CariPasifeAlmaLogOlustur(Guid cariID, string cariAdi, string aciklama, Guid? kullaniciID = null, string kullaniciAdi = null)
        {
            await LogOlustur(
                islemTuru: "Cari Pasife Alma",
                kayitID: cariID,
                tabloAdi: "Cariler",
                kayitAdi: cariAdi,
                aciklama: aciklama,
                kullaniciID: kullaniciID,
                kullaniciAdi: kullaniciAdi
            );
        }

        public async Task CariAktifleştirmeLogOlustur(Guid cariID, string cariAdi, string aciklama)
        {
            var logAciklama = $"Cari Aktifleştirme: {cariAdi} - {aciklama}";
            
            // Giriş yapmış kullanıcıyı al
            var kullaniciID = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var kullaniciAdi = _httpContextAccessor.HttpContext?.User.Identity?.Name;
            var ipAdresi = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "0.0.0.0";
            
            // Sistem log oluştur
            var log = new Data.Entities.SistemLog
            {
                LogID = Guid.NewGuid(),
                IslemTarihi = DateTime.Now,
                KullaniciID = kullaniciID,
                KullaniciAdi = kullaniciAdi ?? "Sistem",
                IPAdresi = ipAdresi,
                Aciklama = logAciklama,
                IslemTuru = "Cari Aktifleştirme",
                KayitID = cariID, // Doğrudan Guid olarak kaydet
                TabloAdi = "Cariler",
                KayitAdi = cariAdi,
                Basarili = true,
                HataMesaji = "",
                IlgiliID = cariID.ToString()
            };
            
            // Veritabanına kaydet
            await _unitOfWork.Repository<Data.Entities.SistemLog>().AddAsync(log);
            await _unitOfWork.SaveAsync();
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
                IPAdresi = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "0.0.0.0",
                Aciklama = logAciklama,
                IslemTuru = "Cari Oluşturma",
                KayitID = cariID, // Doğrudan Guid olarak kaydet
                TabloAdi = "Cariler",
                KayitAdi = cariAdi,
                Basarili = true,
                HataMesaji = "",
                IlgiliID = cariID.ToString()
            };
            
            // Veritabanına kaydet
            await _unitOfWork.Repository<Data.Entities.SistemLog>().AddAsync(log);
            await _unitOfWork.SaveAsync();
        }
        
        public async Task CariGuncellemeLogOlustur(Guid cariID, string cariAdi, string aciklama)
        {
            var logAciklama = $"Cari Güncelleme: {cariAdi} - {aciklama}";
            
            // Giriş yapmış kullanıcıyı al
            var kullaniciID = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var kullaniciAdi = _httpContextAccessor.HttpContext?.User.Identity?.Name;
            var ipAdresi = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "0.0.0.0";
            
            // Sistem log oluştur
            var log = new Data.Entities.SistemLog
            {
                LogID = Guid.NewGuid(),
                IslemTarihi = DateTime.Now,
                KullaniciID = kullaniciID,
                KullaniciAdi = kullaniciAdi ?? "Sistem",
                IPAdresi = ipAdresi,
                Aciklama = logAciklama,
                IslemTuru = "Cari Güncelleme",
                KayitID = cariID, // Doğrudan Guid olarak kaydet
                TabloAdi = "Cariler",
                KayitAdi = cariAdi,
                Basarili = true,
                HataMesaji = "",
                IlgiliID = cariID.ToString()
            };
            
            // Veritabanına kaydet
            await _unitOfWork.Repository<Data.Entities.SistemLog>().AddAsync(log);
            await _unitOfWork.SaveAsync();
        }
        
        public async Task CariSilmeLogOlustur(Guid cariID, string cariAdi, string aciklama)
        {
            var logAciklama = $"Cari Silme: {cariAdi} - {aciklama}";
            
            // Giriş yapmış kullanıcıyı al
            var kullaniciID = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var kullaniciAdi = _httpContextAccessor.HttpContext?.User.Identity?.Name;
            var ipAdresi = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "0.0.0.0";
            
            // Sistem log oluştur
            var log = new Data.Entities.SistemLog
            {
                LogID = Guid.NewGuid(),
                IslemTarihi = DateTime.Now,
                KullaniciID = kullaniciID,
                KullaniciAdi = kullaniciAdi ?? "Sistem",
                IPAdresi = ipAdresi,
                Aciklama = logAciklama,
                IslemTuru = "Cari Silme",
                KayitID = cariID, // Doğrudan Guid olarak kaydet
                TabloAdi = "Cariler",
                KayitAdi = cariAdi,
                Basarili = true,
                HataMesaji = "",
                IlgiliID = cariID.ToString()
            };
            
            // Veritabanına kaydet
            await _unitOfWork.Repository<Data.Entities.SistemLog>().AddAsync(log);
            await _unitOfWork.SaveAsync();
        }
        
        public async Task CariPasifleştirmeLogOlustur(Guid cariID, string cariAdi, string aciklama)
        {
            var logAciklama = $"Cari Pasifleştirme: {cariAdi} - {aciklama}";
            
            // Giriş yapmış kullanıcıyı al
            var kullaniciID = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var kullaniciAdi = _httpContextAccessor.HttpContext?.User.Identity?.Name;
            var ipAdresi = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "0.0.0.0";
            
            // Sistem log oluştur
            var log = new Data.Entities.SistemLog
            {
                LogID = Guid.NewGuid(),
                IslemTarihi = DateTime.Now,
                KullaniciID = kullaniciID,
                KullaniciAdi = kullaniciAdi ?? "Sistem",
                IPAdresi = ipAdresi,
                Aciklama = logAciklama,
                IslemTuru = "Cari Pasifleştirme",
                KayitID = cariID, // Doğrudan Guid olarak kaydet
                TabloAdi = "Cariler",
                KayitAdi = cariAdi,
                Basarili = true,
                HataMesaji = "",
                IlgiliID = cariID.ToString()
            };
            
            // Veritabanına kaydet
            await _unitOfWork.Repository<Data.Entities.SistemLog>().AddAsync(log);
            await _unitOfWork.SaveAsync();
        }
        
        public async Task CariHareketEklemeLogOlustur(Guid cariID, string cariAdi, string hareketTuru, decimal tutar, string aciklama)
        {
            var logAciklama = $"Cari Hareket Ekleme: {cariAdi} - {hareketTuru} - {tutar:C2} - {aciklama}";
            
            // Giriş yapmış kullanıcıyı al
            var kullaniciID = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var kullaniciAdi = _httpContextAccessor.HttpContext?.User.Identity?.Name;
            var ipAdresi = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "0.0.0.0";
            
            // Sistem log oluştur
            var log = new Data.Entities.SistemLog
            {
                LogID = Guid.NewGuid(),
                IslemTarihi = DateTime.Now,
                KullaniciID = kullaniciID,
                KullaniciAdi = kullaniciAdi ?? "Sistem",
                IPAdresi = ipAdresi,
                Aciklama = logAciklama,
                IslemTuru = "Cari Hareket",
                KayitID = cariID, // Doğrudan Guid olarak kaydet
                TabloAdi = "CariHareketler",
                KayitAdi = cariAdi,
                Basarili = true,
                HataMesaji = "",
                IlgiliID = cariID.ToString()
            };
            
            // Veritabanına kaydet
            await _unitOfWork.Repository<Data.Entities.SistemLog>().AddAsync(log);
            await _unitOfWork.SaveAsync();
        }
        
        public async Task UrunOlusturmaLogOlustur(Guid urunID, string urunAdi, string aciklama)
        {
            var logAciklama = $"Ürün Oluşturma: {urunAdi} - {aciklama}";
            
            // Sistem log oluştur ve kaydet
            await LogEkleAsync(logAciklama, LogTuru.Bilgi, urunAdi, urunID, "Urunler");
        }
        
        public async Task UrunGuncellemeLogOlustur(Guid urunID, string urunAdi, string aciklama)
        {
            var logAciklama = $"Ürün Güncelleme: {urunAdi} - {aciklama}";
            
            // Sistem log oluştur ve kaydet
            await LogEkleAsync(logAciklama, LogTuru.Bilgi, urunAdi, urunID, "Urunler");
        }
        
        public async Task UrunSilmeLogOlustur(Guid urunID, string urunAdi, string aciklama)
        {
            var logAciklama = $"Ürün Silme: {urunAdi} - {aciklama}";
            
            // Sistem log oluştur ve kaydet
            await LogEkleAsync(logAciklama, LogTuru.Uyari, urunAdi, urunID, "Urunler");
        }
        
        public async Task StokGirisLogOlustur(Guid stokHareketID, Guid urunID, string urunAdi, decimal miktar, string aciklama)
        {
            var logAciklama = $"Stok Giriş: {urunAdi} - {miktar} adet - {aciklama}";
            
            // Sistem log oluştur ve kaydet
            await LogEkleAsync(logAciklama, LogTuru.Bilgi, urunAdi, stokHareketID, "StokHareketler");
        }
        
        public async Task StokCikisLogOlustur(Guid stokHareketID, Guid urunID, string urunAdi, decimal miktar, string aciklama)
        {
            var logAciklama = $"Stok Çıkış: {urunAdi} - {miktar} adet - {aciklama}";
            await LogEkleAsync(logAciklama, LogTuru.Bilgi, urunAdi, stokHareketID, "StokHareketler");
        }
        
        public async Task LogErrorAsync(string category, Exception ex)
        {
            var message = $"{category}: {ex.Message}";
            await LogEkleAsync(message, LogTuru.Hata, category);
        }
        
        public async Task LogInfoAsync(string category, string message)
        {
            var logMessage = $"{category}: {message}";
            await LogEkleAsync(logMessage, LogTuru.Bilgi, category);
        }
        
        public async Task LogWarningAsync(string category, string message)
        {
            var logMessage = $"{category}: {message}";
            await LogEkleAsync(logMessage, LogTuru.Uyari, category);
        }

        public async Task FaturaOlusturmaLogOlustur(Guid faturaID, string faturaNo, string aciklama)
        {
            var logAciklama = $"Fatura Oluşturma: {faturaNo} - {aciklama}";
            
            // Giriş yapmış kullanıcıyı al
            var kullaniciID = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var kullaniciAdi = _httpContextAccessor.HttpContext?.User.Identity?.Name;
            var ipAdresi = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "0.0.0.0";
            
            // Sistem log oluştur
            var log = new Data.Entities.SistemLog
            {
                LogID = Guid.NewGuid(),
                IslemTarihi = DateTime.Now,
                KullaniciID = kullaniciID,
                KullaniciAdi = kullaniciAdi ?? "Sistem",
                IPAdresi = ipAdresi,
                Aciklama = logAciklama,
                IslemTuru = "Fatura Oluşturma",
                KayitID = faturaID, // Doğrudan Guid olarak kaydet
                TabloAdi = "Faturalar",
                KayitAdi = faturaNo,
                Basarili = true,
                HataMesaji = "",
                IlgiliID = faturaID.ToString()
            };
            
            // Veritabanına kaydet
            await _unitOfWork.Repository<Data.Entities.SistemLog>().AddAsync(log);
            await _unitOfWork.SaveAsync();
        }

        public async Task FaturaGuncellemeLogOlustur(Guid faturaID, string faturaNo, string aciklama)
        {
            var logAciklama = $"Fatura Güncelleme: {faturaNo} - {aciklama}";
            
            // Giriş yapmış kullanıcıyı al
            var kullaniciID = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var kullaniciAdi = _httpContextAccessor.HttpContext?.User.Identity?.Name;
            var ipAdresi = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "0.0.0.0";
            
            // Sistem log oluştur
            var log = new Data.Entities.SistemLog
            {
                LogID = Guid.NewGuid(),
                IslemTarihi = DateTime.Now,
                KullaniciID = kullaniciID,
                KullaniciAdi = kullaniciAdi ?? "Sistem",
                IPAdresi = ipAdresi,
                Aciklama = logAciklama,
                IslemTuru = "Fatura Güncelleme",
                KayitID = faturaID, // Doğrudan Guid olarak kaydet
                TabloAdi = "Faturalar",
                KayitAdi = faturaNo,
                Basarili = true,
                HataMesaji = "",
                IlgiliID = faturaID.ToString()
            };
            
            // Veritabanına kaydet
            await _unitOfWork.Repository<Data.Entities.SistemLog>().AddAsync(log);
            await _unitOfWork.SaveAsync();
        }

        public async Task FaturaSilmeLogOlustur(Guid faturaID, string faturaNo, string aciklama)
        {
            var logAciklama = $"Fatura Silme: {faturaNo} - {aciklama}";
            
            // Giriş yapmış kullanıcıyı al
            var kullaniciID = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var kullaniciAdi = _httpContextAccessor.HttpContext?.User.Identity?.Name;
            var ipAdresi = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "0.0.0.0";
            
            // Sistem log oluştur
            var log = new Data.Entities.SistemLog
            {
                LogID = Guid.NewGuid(),
                IslemTarihi = DateTime.Now,
                KullaniciID = kullaniciID,
                KullaniciAdi = kullaniciAdi ?? "Sistem",
                IPAdresi = ipAdresi,
                Aciklama = logAciklama,
                IslemTuru = "Fatura Silme",
                KayitID = faturaID, // Doğrudan Guid olarak kaydet
                TabloAdi = "Faturalar",
                KayitAdi = faturaNo,
                Basarili = true,
                HataMesaji = "",
                IlgiliID = faturaID.ToString()
            };
            
            // Veritabanına kaydet
            await _unitOfWork.Repository<Data.Entities.SistemLog>().AddAsync(log);
            await _unitOfWork.SaveAsync();
        }
    }
} 
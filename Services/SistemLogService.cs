using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using MuhasebeStokWebApp.Data.Entities;
using MuhasebeStokWebApp.Data.Repositories;
using System.Security.Claims;

namespace MuhasebeStokWebApp.Services
{
    public class SistemLogService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<SistemLogService> _logger;

        public SistemLogService(
            IUnitOfWork unitOfWork, 
            IHttpContextAccessor httpContextAccessor,
            ILogger<SistemLogService> logger)
        {
            _unitOfWork = unitOfWork;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task LogOlustur(string islemTuru, Guid? kayitID, string tabloAdi, string kayitAdi, string aciklama, string? kullaniciID = null, string kullaniciAdi = null, bool basarili = true, string hataMesaji = null)
        {
            try
            {
                var httpContext = _httpContextAccessor.HttpContext;
                string ipAdresi = httpContext?.Connection?.RemoteIpAddress?.ToString();

                if (string.IsNullOrEmpty(kullaniciAdi) && httpContext?.User.Identity?.IsAuthenticated == true)
                {
                    kullaniciAdi = httpContext.User.Identity.Name;
                }

                if (string.IsNullOrEmpty(kullaniciID) && httpContext?.User.Identity?.IsAuthenticated == true)
                {
                    kullaniciID = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                }

                var logKaydi = new SistemLog
                {
                    IslemTuru = islemTuru,
                    KayitID = kayitID,
                    TabloAdi = tabloAdi,
                    KayitAdi = kayitAdi,
                    Aciklama = aciklama,
                    KullaniciId = kullaniciID,
                    KullaniciAdi = kullaniciAdi ?? "Sistem",
                    IPAdresi = ipAdresi,
                    IslemTarihi = DateTime.Now,
                    Basarili = basarili,
                    HataMesaji = hataMesaji ?? string.Empty
                };

                await _unitOfWork.Repository<SistemLog>().AddAsync(logKaydi);
                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Log oluşturulurken hata oluştu: {Error}", ex.Message);
            }
        }

        public async Task<bool> LogAsync(string mesaj, string detay = null)
        {
            try
            {
                var httpContext = _httpContextAccessor.HttpContext;
                var kullaniciAdi = httpContext?.User?.Identity?.Name ?? "Sistem";
                var ipAdresi = httpContext?.Connection?.RemoteIpAddress?.ToString() ?? "Bilinmiyor";

                var log = new SistemLog
                {
                    LogID = Guid.NewGuid(),
                    LogTuru = "Bilgi",
                    Mesaj = mesaj ?? "Sistem log kaydı",
                    HataMesaji = detay ?? "İşlem başarılı",
                    Aciklama = mesaj ?? "Sistem log kaydı",
                    KullaniciAdi = kullaniciAdi,
                    IPAdresi = ipAdresi,
                    IslemTarihi = DateTime.Now,
                    IslemTuru = "Bilgi",
                    Basarili = true,
                    TabloAdi = string.Empty,
                    KayitAdi = string.Empty
                };

                var sistemLogRepository = _unitOfWork.Repository<SistemLog>();
                await sistemLogRepository.AddAsync(log);
                await _unitOfWork.SaveAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Log kaydedilirken hata oluştu: {Message}", ex.Message);
                return false;
            }
        }

        public async Task<bool> LogErrorAsync(string mesaj, string detay = null)
        {
            try
            {
                var httpContext = _httpContextAccessor.HttpContext;
                var kullaniciAdi = httpContext?.User?.Identity?.Name ?? "Sistem";
                var ipAdresi = httpContext?.Connection?.RemoteIpAddress?.ToString() ?? "Bilinmiyor";

                var log = new SistemLog
                {
                    LogID = Guid.NewGuid(),
                    LogTuru = "Hata",
                    Mesaj = mesaj ?? "Sistem hatası",
                    HataMesaji = detay ?? "Hata detayı bulunmuyor",
                    Aciklama = mesaj ?? "Sistem hatası",
                    KullaniciAdi = kullaniciAdi,
                    IPAdresi = ipAdresi,
                    IslemTarihi = DateTime.Now,
                    IslemTuru = "Hata",
                    Basarili = false,
                    TabloAdi = string.Empty,
                    KayitAdi = string.Empty
                };

                var sistemLogRepository = _unitOfWork.Repository<SistemLog>();
                await sistemLogRepository.AddAsync(log);
                await _unitOfWork.SaveAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Hata logu kaydedilirken hata oluştu: {Message}", ex.Message);
                return false;
            }
        }

        public async Task CariPasifeAlmaLogOlustur(Guid cariID, string ad, string aciklama, string? kullaniciID = null, string kullaniciAdi = null)
        {
            await LogOlustur(
                islemTuru: "Cari Pasife Alma",
                kayitID: cariID,
                tabloAdi: "Cari",
                kayitAdi: ad,
                aciklama: aciklama,
                kullaniciID: kullaniciID,
                kullaniciAdi: kullaniciAdi,
                basarili: true
            );
        }

        public async Task CariAktifleştirmeLogOlustur(Guid cariID, string ad, string aciklama, string? kullaniciID = null, string kullaniciAdi = null)
        {
            await LogOlustur(
                islemTuru: "Cari Aktifleştirme",
                kayitID: cariID,
                tabloAdi: "Cari",
                kayitAdi: ad,
                aciklama: aciklama,
                kullaniciID: kullaniciID,
                kullaniciAdi: kullaniciAdi,
                basarili: true
            );
        }
    }
} 
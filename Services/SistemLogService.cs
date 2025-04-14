using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using MuhasebeStokWebApp.Data.Entities;
using MuhasebeStokWebApp.Data.Repositories;

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

        public async Task LogOlustur(string islemTuru, Guid? kayitID, string tabloAdi, string kayitAdi, string aciklama, Guid? kullaniciID = null, string kullaniciAdi = null, bool basarili = true, string hataMesaji = null)
        {
            try
            {
                var sistemLogRepository = _unitOfWork.Repository<SistemLog>();

                var ipAdresi = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "Bilinmiyor";

                var log = new SistemLog
                {
                    LogID = Guid.NewGuid(),
                    LogTuru = "Sistem",
                    Mesaj = aciklama ?? "Sistem log kaydı",
                    IslemTuru = islemTuru,
                    KayitID = kayitID,
                    TabloAdi = tabloAdi ?? string.Empty,
                    KayitAdi = kayitAdi ?? string.Empty,
                    IslemTarihi = DateTime.Now,
                    Aciklama = aciklama ?? "Sistem log kaydı",
                    KullaniciID = kullaniciID,
                    KullaniciAdi = kullaniciAdi ?? "Sistem",
                    IPAdresi = ipAdresi,
                    Basarili = basarili,
                    HataMesaji = hataMesaji ?? (basarili ? "İşlem başarılı" : "İşlem başarısız")
                };

                await sistemLogRepository.AddAsync(log);
                await _unitOfWork.SaveAsync();
            }
            catch (Exception ex)
            {
                // Log oluşturma sırasında hata olursa, ILogger ile logla
                _logger.LogError(ex, "Log oluşturma hatası: {IslemTuru} - {KayitAdi}", islemTuru, kayitAdi);
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

        public async Task CariPasifeAlmaLogOlustur(Guid cariID, string ad, string aciklama, Guid? kullaniciID = null, string kullaniciAdi = null)
        {
            await LogOlustur(
                islemTuru: "Cari Pasife Alma",
                kayitID: cariID,
                tabloAdi: "Cariler",
                kayitAdi: ad,
                aciklama: aciklama ?? $"{ad} adlı cari pasife alındı",
                kullaniciID: kullaniciID,
                kullaniciAdi: kullaniciAdi ?? "Sistem"
            );
        }

        public async Task CariAktifleştirmeLogOlustur(Guid cariID, string ad, string aciklama, Guid? kullaniciID = null, string kullaniciAdi = null)
        {
            await LogOlustur(
                islemTuru: "Cari Aktifleştirme",
                kayitID: cariID,
                tabloAdi: "Cariler",
                kayitAdi: ad,
                aciklama: aciklama ?? $"{ad} adlı cari aktifleştirildi",
                kullaniciID: kullaniciID,
                kullaniciAdi: kullaniciAdi ?? "Sistem"
            );
        }
    }
} 
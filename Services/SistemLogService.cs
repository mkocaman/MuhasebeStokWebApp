using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using MuhasebeStokWebApp.Data.Entities;
using MuhasebeStokWebApp.Data.Repositories;

namespace MuhasebeStokWebApp.Services
{
    public class SistemLogService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public SistemLogService(IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor)
        {
            _unitOfWork = unitOfWork;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task LogOlustur(string islemTuru, Guid? kayitID, string tabloAdi, string kayitAdi, string aciklama, Guid? kullaniciID = null, string kullaniciAdi = null, bool basarili = true, string hataMesaji = null)
        {
            try
            {
                var sistemLogRepository = _unitOfWork.Repository<SistemLog>();

                var ipAdresi = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();

                var log = new SistemLog
                {
                    LogID = Guid.NewGuid(),
                    IslemTuru = islemTuru,
                    KayitID = kayitID,
                    TabloAdi = tabloAdi,
                    KayitAdi = kayitAdi,
                    IslemTarihi = DateTime.Now,
                    Aciklama = aciklama,
                    KullaniciID = kullaniciID,
                    KullaniciAdi = kullaniciAdi,
                    IPAdresi = ipAdresi,
                    Basarili = basarili,
                    HataMesaji = hataMesaji
                };

                await sistemLogRepository.AddAsync(log);
                await _unitOfWork.SaveAsync();
            }
            catch (Exception ex)
            {
                // Log oluşturma sırasında hata olursa, sessizce devam et
                // Burada gerçek bir uygulamada, belki bir dosyaya yazma veya başka bir yedek log mekanizması kullanılabilir
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

        public async Task CariAktifleştirmeLogOlustur(Guid cariID, string cariAdi, string aciklama, Guid? kullaniciID = null, string kullaniciAdi = null)
        {
            await LogOlustur(
                islemTuru: "Cari Aktifleştirme",
                kayitID: cariID,
                tabloAdi: "Cariler",
                kayitAdi: cariAdi,
                aciklama: aciklama,
                kullaniciID: kullaniciID,
                kullaniciAdi: kullaniciAdi
            );
        }
    }
} 
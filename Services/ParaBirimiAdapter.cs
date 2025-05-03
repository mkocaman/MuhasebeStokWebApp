using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using MuhasebeStokWebApp.Data.Entities.DovizModulu;
using MuhasebeStokWebApp.Services.Interfaces;
using MuhasebeStokWebApp.Services.ParaBirimiModulu;
using Microsoft.Extensions.Logging;

namespace MuhasebeStokWebApp.Services
{
    /// <summary>
    /// Services.Interfaces.IParaBirimiService arayüzünü 
    /// Services.ParaBirimiModulu.IParaBirimiService uygulamasına adapte eder.
    /// </summary>
    public class ParaBirimiAdapter : MuhasebeStokWebApp.Services.Interfaces.IParaBirimiService
    {
        private readonly MuhasebeStokWebApp.Services.ParaBirimiModulu.IParaBirimiService _paraBirimiService;
        private readonly MuhasebeStokWebApp.Services.ParaBirimiModulu.IParaBirimiIliskiService _paraBirimiIliskiService;
        private readonly ILogger<ParaBirimiAdapter> _logger;

        public ParaBirimiAdapter(
            MuhasebeStokWebApp.Services.ParaBirimiModulu.IParaBirimiService paraBirimiService,
            MuhasebeStokWebApp.Services.ParaBirimiModulu.IParaBirimiIliskiService paraBirimiIliskiService,
            ILogger<ParaBirimiAdapter> logger)
        {
            _paraBirimiService = paraBirimiService;
            _paraBirimiIliskiService = paraBirimiIliskiService;
            _logger = logger;
        }

        public async Task<ParaBirimi> AddParaBirimiAsync(ParaBirimi paraBirimi)
        {
            _logger.LogInformation("DovizModulu.ParaBirimi -> ParaBirimiModulu.ParaBirimi dönüşümü yapılıyor");
            
            var yeniParaBirimi = new Data.Entities.ParaBirimiModulu.ParaBirimi
            {
                ParaBirimiID = paraBirimi.ParaBirimiID,
                Ad = paraBirimi.Ad,
                Kod = paraBirimi.Kod,
                Sembol = paraBirimi.Sembol,
                OndalikAyraci = paraBirimi.OndalikAyraci,
                BinlikAyraci = paraBirimi.BinlikAyraci,
                OndalikHassasiyet = paraBirimi.OndalikHassasiyet,
                AnaParaBirimiMi = paraBirimi.AnaParaBirimiMi,
                Sira = paraBirimi.Sira,
                Aciklama = paraBirimi.Aciklama,
                Aktif = paraBirimi.Aktif,
                Silindi = paraBirimi.Silindi,
                OlusturmaTarihi = paraBirimi.OlusturmaTarihi,
                GuncellemeTarihi = paraBirimi.GuncellemeTarihi,
                OlusturanKullaniciID = paraBirimi.OlusturanKullaniciID,
                SonGuncelleyenKullaniciID = paraBirimi.SonGuncelleyenKullaniciID
            };
            
            var sonuc = await _paraBirimiService.AddParaBirimiAsync(yeniParaBirimi);
            return MapToParaBirimi(sonuc);
        }

        public async Task<DovizIliski> AddParaBirimiIliskiAsync(DovizIliski dovizIliski)
        {
            _logger.LogInformation("DovizModulu.DovizIliski -> ParaBirimiModulu.ParaBirimiIliski dönüşümü yapılıyor");
            
            var yeniIliski = new Data.Entities.ParaBirimiModulu.ParaBirimiIliski
            {
                ParaBirimiIliskiID = dovizIliski.DovizIliskiID,
                KaynakParaBirimiID = dovizIliski.KaynakParaBirimiID,
                HedefParaBirimiID = dovizIliski.HedefParaBirimiID,
                Aktif = dovizIliski.Aktif,
                Silindi = dovizIliski.Silindi,
                Aciklama = "", // DovizIliski modelinde yok, varsayılan boş değer
                OlusturmaTarihi = dovizIliski.OlusturmaTarihi,
                GuncellemeTarihi = dovizIliski.GuncellemeTarihi,
                OlusturanKullaniciID = dovizIliski.OlusturanKullaniciID,
                SonGuncelleyenKullaniciID = dovizIliski.SonGuncelleyenKullaniciID
            };
            
            var sonuc = await _paraBirimiIliskiService.AddAsync(yeniIliski);
            return MapToDovizIliski(sonuc);
        }

        public async Task<bool> DeleteParaBirimiAsync(Guid paraBirimiId)
        {
            return await _paraBirimiService.DeleteParaBirimiAsync(paraBirimiId);
        }

        public async Task<bool> DeleteParaBirimiIliskiAsync(Guid iliskiId)
        {
            await _paraBirimiIliskiService.DeleteAsync(iliskiId);
            return true;
        }

        public async Task<List<DovizIliski>> GetAktifParaBirimiIliskileriAsync()
        {
            _logger.LogInformation("ParaBirimiModulu.ParaBirimiIliski -> DovizModulu.DovizIliski dönüşümü yapılıyor");
            
            var iliskiler = await _paraBirimiIliskiService.GetAllAsync();
            return iliskiler.Where(i => i.Aktif && !i.Silindi).Select(MapToDovizIliski).ToList();
        }

        public async Task<List<ParaBirimi>> GetAktifParaBirimleriAsync()
        {
            var paraBirimleri = await _paraBirimiService.GetAllParaBirimleriAsync(true);
            return paraBirimleri.Select(MapToParaBirimi).ToList();
        }

        public async Task<List<DovizIliski>> GetAllParaBirimiIliskileriAsync()
        {
            _logger.LogInformation("ParaBirimiModulu.ParaBirimiIliski -> DovizModulu.DovizIliski dönüşümü yapılıyor");
            
            var iliskiler = await _paraBirimiIliskiService.GetAllAsync();
            return iliskiler.Select(MapToDovizIliski).ToList();
        }

        public async Task<List<ParaBirimi>> GetAllParaBirimleriAsync()
        {
            var paraBirimleri = await _paraBirimiService.GetAllParaBirimleriAsync(false);
            return paraBirimleri.Select(MapToParaBirimi).ToList();
        }

        public async Task<DovizIliski> GetIliskiByParaBirimleriAsync(Guid kaynakId, Guid hedefId)
        {
            _logger.LogInformation("ParaBirimiModulu.ParaBirimiIliski -> DovizModulu.DovizIliski dönüşümü yapılıyor");
            
            var iliski = await _paraBirimiService.GetIliskiByParaBirimleriAsync(kaynakId, hedefId);
            return MapToDovizIliski(iliski);
        }

        public async Task<DovizIliski> GetParaBirimiIliskiByIdAsync(Guid iliskiId)
        {
            _logger.LogInformation("ParaBirimiModulu.ParaBirimiIliski -> DovizModulu.DovizIliski dönüşümü yapılıyor");
            
            var iliski = await _paraBirimiIliskiService.GetByIdAsync(iliskiId);
            return MapToDovizIliski(iliski);
        }

        public async Task<List<DovizIliski>> GetParaBirimiIliskileriAsync(Guid paraBirimiId)
        {
            _logger.LogInformation("ParaBirimiModulu.ParaBirimiIliski -> DovizModulu.DovizIliski dönüşümü yapılıyor");
            
            var kaynakIliskileri = await _paraBirimiIliskiService.GetByKaynakParaBirimiIdAsync(paraBirimiId);
            var hedefIliskileri = await _paraBirimiIliskiService.GetByHedefParaBirimiIdAsync(paraBirimiId);
            
            var tumIliskiler = kaynakIliskileri.Concat(hedefIliskileri).Distinct();
            return tumIliskiler.Select(MapToDovizIliski).ToList();
        }

        public async Task<ParaBirimi> GetParaBirimiByIdAsync(Guid paraBirimiId)
        {
            var paraBirimi = await _paraBirimiService.GetParaBirimiByIdAsync(paraBirimiId);
            return MapToParaBirimi(paraBirimi);
        }

        public async Task<ParaBirimi> GetParaBirimiByKodAsync(string kod)
        {
            var paraBirimi = await _paraBirimiService.GetParaBirimiByKodAsync(kod);
            return MapToParaBirimi(paraBirimi);
        }

        public async Task<bool> HasParaBirimiIliskiAsync(Guid paraBirimiId)
        {
            var kaynakIliskileri = await _paraBirimiIliskiService.GetByKaynakParaBirimiIdAsync(paraBirimiId);
            var hedefIliskileri = await _paraBirimiIliskiService.GetByHedefParaBirimiIdAsync(paraBirimiId);
            
            return kaynakIliskileri.Any() || hedefIliskileri.Any();
        }

        public async Task<bool> UpdateParaBirimiSiralamaAsync(List<Guid> paraBirimiIdSiralama)
        {
            _logger.LogInformation("UpdateParaBirimiSiralamaAsync çağrıldı");
            // Her bir para birimini güncelle
            int sira = 0;
            foreach (var id in paraBirimiIdSiralama)
            {
                var paraBirimi = await _paraBirimiService.GetParaBirimiByIdAsync(id);
                if (paraBirimi != null)
                {
                    paraBirimi.Sira = sira++;
                    await _paraBirimiService.UpdateParaBirimiAsync(paraBirimi);
                }
            }
            return true;
        }

        public async Task<ParaBirimi> UpdateParaBirimiAsync(ParaBirimi paraBirimi)
        {
            _logger.LogInformation("DovizModulu.ParaBirimi -> ParaBirimiModulu.ParaBirimi dönüşümü yapılıyor");
            
            var guncellenecekParaBirimi = new Data.Entities.ParaBirimiModulu.ParaBirimi
            {
                ParaBirimiID = paraBirimi.ParaBirimiID,
                Ad = paraBirimi.Ad,
                Kod = paraBirimi.Kod,
                Sembol = paraBirimi.Sembol,
                OndalikAyraci = paraBirimi.OndalikAyraci,
                BinlikAyraci = paraBirimi.BinlikAyraci,
                OndalikHassasiyet = paraBirimi.OndalikHassasiyet,
                AnaParaBirimiMi = paraBirimi.AnaParaBirimiMi,
                Sira = paraBirimi.Sira,
                Aciklama = paraBirimi.Aciklama,
                Aktif = paraBirimi.Aktif,
                Silindi = paraBirimi.Silindi,
                OlusturmaTarihi = paraBirimi.OlusturmaTarihi,
                GuncellemeTarihi = DateTime.Now,
                OlusturanKullaniciID = paraBirimi.OlusturanKullaniciID,
                SonGuncelleyenKullaniciID = paraBirimi.SonGuncelleyenKullaniciID
            };
            
            var sonuc = await _paraBirimiService.UpdateParaBirimiAsync(guncellenecekParaBirimi);
            return MapToParaBirimi(sonuc);
        }

        public async Task<DovizIliski> UpdateParaBirimiIliskiAsync(DovizIliski dovizIliski)
        {
            _logger.LogInformation("DovizModulu.DovizIliski -> ParaBirimiModulu.ParaBirimiIliski dönüşümü yapılıyor");
            
            // Önce mevcut ilişkiyi al
            var mevcutIliski = await _paraBirimiIliskiService.GetByIdAsync(dovizIliski.DovizIliskiID);
            if (mevcutIliski == null)
            {
                throw new Exception($"ParaBirimiIliski bulunamadı: {dovizIliski.DovizIliskiID}");
            }
            
            // Değerleri güncelle
            mevcutIliski.KaynakParaBirimiID = dovizIliski.KaynakParaBirimiID;
            mevcutIliski.HedefParaBirimiID = dovizIliski.HedefParaBirimiID;
            mevcutIliski.Aktif = dovizIliski.Aktif;
            mevcutIliski.Silindi = dovizIliski.Silindi;
            mevcutIliski.GuncellemeTarihi = DateTime.Now;
            mevcutIliski.SonGuncelleyenKullaniciID = dovizIliski.SonGuncelleyenKullaniciID;
            
            // Güncelleme işlemini yap
            await _paraBirimiIliskiService.UpdateAsync(mevcutIliski);
            
            // Güncellenmiş veriyi döndür
            return MapToDovizIliski(mevcutIliski);
        }

        public async Task<bool> VarsayilanParaBirimleriniEkleAsync()
        {
            _logger.LogInformation("VarsayilanParaBirimleriniEkleAsync çağrıldı");
            
            // Varsayılan para birimlerini ekle
            bool sonuc = true;
            
            // TRY ekle
            if (await _paraBirimiService.GetParaBirimiByKodAsync("TRY") == null)
            {
                var tryParaBirimi = new Data.Entities.ParaBirimiModulu.ParaBirimi
                {
                    ParaBirimiID = Guid.NewGuid(),
                    Kod = "TRY",
                    Ad = "Türk Lirası",
                    Sembol = "₺",
                    Aciklama = "Türkiye Cumhuriyeti resmi para birimi",
                    AnaParaBirimiMi = true,
                    Aktif = true,
                    Silindi = false,
                    Sira = 1,
                    OlusturmaTarihi = DateTime.Now,
                    OlusturanKullaniciID = "Sistem",
                    SonGuncelleyenKullaniciID = "Sistem"
                };
                await _paraBirimiService.AddParaBirimiAsync(tryParaBirimi);
            }
            
            // USD ekle
            if (await _paraBirimiService.GetParaBirimiByKodAsync("USD") == null)
            {
                var usdParaBirimi = new Data.Entities.ParaBirimiModulu.ParaBirimi
                {
                    ParaBirimiID = Guid.NewGuid(),
                    Kod = "USD",
                    Ad = "Amerikan Doları",
                    Sembol = "$",
                    Aciklama = "Amerika Birleşik Devletleri resmi para birimi",
                    AnaParaBirimiMi = false,
                    Aktif = true,
                    Silindi = false,
                    Sira = 2,
                    OlusturmaTarihi = DateTime.Now,
                    OlusturanKullaniciID = "Sistem",
                    SonGuncelleyenKullaniciID = "Sistem"
                };
                await _paraBirimiService.AddParaBirimiAsync(usdParaBirimi);
            }
            
            // EUR ekle
            if (await _paraBirimiService.GetParaBirimiByKodAsync("EUR") == null)
            {
                var eurParaBirimi = new Data.Entities.ParaBirimiModulu.ParaBirimi
                {
                    ParaBirimiID = Guid.NewGuid(),
                    Kod = "EUR",
                    Ad = "Euro",
                    Sembol = "€",
                    Aciklama = "Avrupa Birliği resmi para birimi",
                    AnaParaBirimiMi = false,
                    Aktif = true,
                    Silindi = false,
                    Sira = 3,
                    OlusturmaTarihi = DateTime.Now,
                    OlusturanKullaniciID = "Sistem",
                    SonGuncelleyenKullaniciID = "Sistem"
                };
                await _paraBirimiService.AddParaBirimiAsync(eurParaBirimi);
            }
            
            return sonuc;
        }

        /// <summary>
        /// İki para birimi kodu arasındaki kur değerini hesaplar
        /// </summary>
        /// <param name="kaynakKod">Kaynak para birimi kodu</param>
        /// <param name="hedefKod">Hedef para birimi kodu</param>
        /// <param name="tarih">Geçerli tarih (null ise şu anki tarihe göre)</param>
        /// <returns>Hesaplanan kur değeri</returns>
        public async Task<decimal> HesaplaKurDegeriByKodAsync(string kaynakKod, string hedefKod, DateTime? tarih = null)
        {
            try 
            {
                _logger.LogInformation($"HesaplaKurDegeriByKodAsync çağrıldı - Kaynak: {kaynakKod}, Hedef: {hedefKod}, Tarih: {tarih}");
                
                // Önce paraBirimiService üzerinden kur hesaplaması yap
                if (_paraBirimiService is MuhasebeStokWebApp.Services.ParaBirimiBirlesikModul.IParaBirimiService birlesikService)
                {
                    return await birlesikService.HesaplaKurDegeriByKodAsync(kaynakKod, hedefKod, tarih);
                }
                
                // Kaynak ve hedef para birimlerini bul
                var kaynakParaBirimi = await _paraBirimiService.GetParaBirimiByKodAsync(kaynakKod);
                var hedefParaBirimi = await _paraBirimiService.GetParaBirimiByKodAsync(hedefKod);
                
                if (kaynakParaBirimi == null || hedefParaBirimi == null)
                {
                    _logger.LogWarning($"Para birimi bulunamadı: Kaynak({kaynakKod}) veya Hedef({hedefKod})");
                    return 1m; // Varsayılan 1:1 kur
                }
                
                // Kaynak ve hedef aynı ise 1 döndür
                if (kaynakKod == hedefKod)
                {
                    return 1m;
                }
                
                // Basit varsayılan kur hesaplaması (gerçekte daha karmaşık olabilir)
                return 1m; // Varsayılan 1:1 kur
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Kur hesaplama hatası: {ex.Message}");
                return 1m; // Hata durumunda varsayılan 1:1 kur döndür
            }
        }

        private ParaBirimi MapToParaBirimi(Data.Entities.ParaBirimiModulu.ParaBirimi source)
        {
            if (source == null) return null;

            return new ParaBirimi
            {
                ParaBirimiID = source.ParaBirimiID,
                Kod = source.Kod,
                Ad = source.Ad,
                Sembol = source.Sembol ?? "",
                OndalikAyraci = source.OndalikAyraci,
                BinlikAyraci = source.BinlikAyraci,
                OndalikHassasiyet = source.OndalikHassasiyet,
                AnaParaBirimiMi = source.AnaParaBirimiMi,
                Sira = source.Sira,
                Aciklama = source.Aciklama,
                Aktif = source.Aktif,
                Silindi = source.Silindi,
                OlusturmaTarihi = source.OlusturmaTarihi,
                GuncellemeTarihi = source.GuncellemeTarihi,
                OlusturanKullaniciID = source.OlusturanKullaniciID ?? "",
                SonGuncelleyenKullaniciID = source.SonGuncelleyenKullaniciID ?? ""
            };
        }
        
        private DovizIliski MapToDovizIliski(Data.Entities.ParaBirimiModulu.ParaBirimiIliski source)
        {
            if (source == null) return null;

            return new DovizIliski
            {
                DovizIliskiID = source.ParaBirimiIliskiID,
                KaynakParaBirimiID = source.KaynakParaBirimiID,
                HedefParaBirimiID = source.HedefParaBirimiID,
                Aktif = source.Aktif,
                Silindi = source.Silindi,
                OlusturmaTarihi = source.OlusturmaTarihi,
                GuncellemeTarihi = source.GuncellemeTarihi,
                OlusturanKullaniciID = source.OlusturanKullaniciID ?? "",
                SonGuncelleyenKullaniciID = source.SonGuncelleyenKullaniciID ?? ""
            };
        }
    }
} 
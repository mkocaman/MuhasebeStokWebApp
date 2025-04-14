using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using MuhasebeStokWebApp.Data.Entities.DovizModulu;
using MuhasebeStokWebApp.Models;
using MuhasebeStokWebApp.Services.Interfaces;
using Microsoft.Extensions.Logging;
using MuhasebeStokWebApp.Services.ParaBirimiModulu;

namespace MuhasebeStokWebApp.Services.ParaBirimiModulu
{
    /// <summary>
    /// IDovizKuruService için adaptör sınıfı.
    /// ParaBirimiService'i IDovizKuruService'e uyumlu hale getirir.
    /// </summary>
    public class DovizKuruServiceAdapter : IDovizKuruService
    {
        private readonly IParaBirimiService _paraBirimiService;
        private readonly IKurDegeriService _kurDegeriService;
        private readonly ILogger<DovizKuruServiceAdapter> _logger;

        public DovizKuruServiceAdapter(
            IParaBirimiService paraBirimiService,
            IKurDegeriService kurDegeriService,
            ILogger<DovizKuruServiceAdapter> logger)
        {
            _paraBirimiService = paraBirimiService;
            _kurDegeriService = kurDegeriService;
            _logger = logger;
        }

        public async Task<decimal> CevirmeTutarAsync(decimal tutar, Guid kaynakParaBirimiId, Guid hedefParaBirimiId, DateTime? tarih = null)
        {
            return await _paraBirimiService.HesaplaKurDegeriAsync(kaynakParaBirimiId, hedefParaBirimiId, tarih) * tutar;
        }

        public async Task<decimal> CevirmeTutarByKodAsync(decimal tutar, string kaynakKod, string hedefKod, DateTime? tarih = null)
        {
            var kaynakParaBirimi = await _paraBirimiService.GetParaBirimiByKodAsync(kaynakKod);
            var hedefParaBirimi = await _paraBirimiService.GetParaBirimiByKodAsync(hedefKod);

            if (kaynakParaBirimi == null || hedefParaBirimi == null)
                throw new ArgumentException("Para birimi kodu bulunamadı.");

            return await CevirmeTutarAsync(tutar, kaynakParaBirimi.ParaBirimiID, hedefParaBirimi.ParaBirimiID, tarih);
        }

        public async Task<decimal> GetGuncelKurAsync(string kaynakKod, string hedefKod, DateTime? tarih = null)
        {
            return await HesaplaKurDegeriByKodAsync(kaynakKod, hedefKod, tarih);
        }

        public async Task<List<DovizKuru>> GetLatestRatesAsync(int count = 5)
        {
            var kurDegerleri = await _paraBirimiService.GetAllKurDegerleriAsync();
            var result = new List<DovizKuru>();

            foreach (var kurDegeri in kurDegerleri.Take(count))
            {
                result.Add(new DovizKuru
                {
                    DovizKuruID = Guid.NewGuid(),
                    ParaBirimiID = kurDegeri.ParaBirimiID,
                    ParaBirimiKodu = kurDegeri.ParaBirimi?.Kod ?? string.Empty,
                    ParaBirimiAdi = kurDegeri.ParaBirimi?.Ad ?? string.Empty,
                    Alis = kurDegeri.Alis,
                    Satis = kurDegeri.Satis,
                    EfektifAlis = kurDegeri.EfektifAlis,
                    EfektifSatis = kurDegeri.EfektifSatis,
                    Tarih = kurDegeri.Tarih,
                    GuncellemeTarihi = kurDegeri.GuncellemeTarihi ?? DateTime.Now,
                    Aktif = kurDegeri.Aktif,
                    Kaynak = "ParaBirimiService"
                });
            }

            return result;
        }

        /// <summary>
        /// Belirli bir tarih için tüm döviz kurlarını getirir
        /// </summary>
        public async Task<List<MuhasebeStokWebApp.Data.Entities.DovizModulu.KurDegeri>> GetKurDegerleriByTarihAsync(DateTime? tarih)
        {
            // Doğrudan GetAllKurDegerleriAsync metoduna DateTime? tipindeki tarih parametresini geçiyoruz
            // Bu metodun imzası: Task<List<KurDegeri>> GetAllKurDegerleriAsync(DateTime? tarih = null)
            var paraKurDegerleri = await _paraBirimiService.GetAllKurDegerleriAsync(tarih);
            return paraKurDegerleri.Select(pk => ConvertToKurDegeri(pk)).ToList();
        }

        public async Task<MuhasebeStokWebApp.Data.Entities.DovizModulu.KurDegeri?> GetSonKurDegeriByKodAsync(string kod)
        {
            var paraBirimi = await _paraBirimiService.GetParaBirimiByKodAsync(kod);
            if (paraBirimi == null)
                return null;

            var paraKurDegeri = await _paraBirimiService.GetSonKurDegeriByParaBirimiAsync(paraBirimi.ParaBirimiID);
            return paraKurDegeri != null ? ConvertToKurDegeri(paraKurDegeri) : null;
        }

        public async Task<MuhasebeStokWebApp.Data.Entities.DovizModulu.KurDegeri?> GetSonKurDegeriByParaBirimiAsync(Guid paraBirimiId)
        {
            var paraKurDegeri = await _paraBirimiService.GetSonKurDegeriByParaBirimiAsync(paraBirimiId);
            return paraKurDegeri != null ? ConvertToKurDegeri(paraKurDegeri) : null;
        }

        public async Task<decimal> HesaplaKurDegeriAsync(Guid kaynakParaBirimiId, Guid hedefParaBirimiId, DateTime? tarih = null)
        {
            return await _paraBirimiService.HesaplaKurDegeriAsync(kaynakParaBirimiId, hedefParaBirimiId, tarih);
        }

        public async Task<decimal> HesaplaKurDegeriByKodAsync(string kaynakKod, string hedefKod, DateTime? tarih = null)
        {
            var kaynakParaBirimi = await _paraBirimiService.GetParaBirimiByKodAsync(kaynakKod);
            var hedefParaBirimi = await _paraBirimiService.GetParaBirimiByKodAsync(hedefKod);

            if (kaynakParaBirimi == null || hedefParaBirimi == null)
                throw new ArgumentException("Para birimi kodu bulunamadı.");

            return await _paraBirimiService.HesaplaKurDegeriAsync(kaynakParaBirimi.ParaBirimiID, hedefParaBirimi.ParaBirimiID, tarih);
        }

        public async Task<decimal> ParaBirimiCevirAsync(decimal tutar, string kaynakKod, string hedefKod, DateTime? tarih = null)
        {
            return await CevirmeTutarByKodAsync(tutar, kaynakKod, hedefKod, tarih);
        }

        public async Task<bool> GuncelleKurDegerleriniFromTCMBAsync()
        {
            // ParaBirimiService'deki GuncelleKurDegerleriniFromAPIAsync metodu kullanılabilir
            return await _paraBirimiService.GuncelleKurDegerleriniFromAPIAsync();
        }

        public async Task<bool> UpdateExchangeRatesAsync()
        {
            try
            {
                return await _kurDegeriService.UpdateExchangeRatesFromApiAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Döviz kurları güncellenirken hata oluştu");
                return false;
            }
        }

        public async Task<decimal> GetExchangeRateAsync(string fromCurrency, string toCurrency, DateTime? date = null)
        {
            try
            {
                // Bu özellik daha sonra implemente edilecek
                // Şimdilik sabit bir değer döndürüyoruz
                return 1.0m;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Döviz kurları dönüştürülürken hata oluştu: {fromCurrency} -> {toCurrency}");
                return 0;
            }
        }

        public async Task<decimal> ConvertAmountAsync(decimal amount, string fromCurrency, string toCurrency, DateTime? date = null)
        {
            try
            {
                var rate = await GetExchangeRateAsync(fromCurrency, toCurrency, date);
                return amount * rate;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Tutar dönüştürülürken hata oluştu: {amount} {fromCurrency} -> {toCurrency}");
                return 0;
            }
        }

        // ParaBirimiModulu.KurDegeri'yi DovizModulu.KurDegeri'ye dönüştürme yardımcı metodu
        private MuhasebeStokWebApp.Data.Entities.DovizModulu.KurDegeri ConvertToKurDegeri(Data.Entities.ParaBirimiModulu.KurDegeri paraKurDegeri)
        {
            return new MuhasebeStokWebApp.Data.Entities.DovizModulu.KurDegeri
            {
                KurDegeriID = paraKurDegeri.KurDegeriID,
                ParaBirimiID = paraKurDegeri.ParaBirimiID,
                Tarih = paraKurDegeri.Tarih,
                Alis = paraKurDegeri.Alis,
                Satis = paraKurDegeri.Satis,
                EfektifAlis = paraKurDegeri.EfektifAlis,
                EfektifSatis = paraKurDegeri.EfektifSatis,
                Aktif = paraKurDegeri.Aktif,
                OlusturmaTarihi = paraKurDegeri.OlusturmaTarihi,
                GuncellemeTarihi = paraKurDegeri.GuncellemeTarihi,
                OlusturanKullaniciID = paraKurDegeri.OlusturanKullaniciID,
                Aciklama = paraKurDegeri.Aciklama,
                Silindi = paraKurDegeri.Silindi
            };
        }
    }
} 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MuhasebeStokWebApp.Data;
using MuhasebeStokWebApp.Data.Entities;
using MuhasebeStokWebApp.Data.Entities.ParaBirimiModulu;
using MuhasebeStokWebApp.Data.Repositories;
using MuhasebeStokWebApp.Services.Interfaces;

namespace MuhasebeStokWebApp.Services
{
    public class MaliyetHesaplamaService : IMaliyetHesaplamaService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDovizKuruService _dovizKuruService;
        private readonly ILogger<MaliyetHesaplamaService> _logger;
        private readonly IExceptionHandlingService _exceptionHandler;

        public MaliyetHesaplamaService(
            IUnitOfWork unitOfWork,
            IDovizKuruService dovizKuruService,
            ILogger<MaliyetHesaplamaService> logger,
            IExceptionHandlingService exceptionHandler)
        {
            _unitOfWork = unitOfWork;
            _dovizKuruService = dovizKuruService;
            _logger = logger;
            _exceptionHandler = exceptionHandler;
        }

        /// <summary>
        /// Belirli bir ürün için ortalama maliyet hesaplar
        /// </summary>
        public async Task<decimal> GetOrtalamaMaliyetAsync(Guid urunID, string paraBirimi = "USD")
        {
            return await _exceptionHandler.HandleExceptionAsync(async () =>
            {
                var aktifKayitlar = await _unitOfWork.StokFifoRepository.GetAllAsync(
                    f => f.UrunID == urunID && f.KalanMiktar > 0 && f.Aktif && !f.Silindi && !f.Iptal,
                    asNoTracking: true);
                
                if (!aktifKayitlar.Any())
                    return 0;

                decimal toplamMaliyet = 0;
                decimal toplamMiktar = aktifKayitlar.Sum(f => f.KalanMiktar);

                // Para birimine göre maliyet hesapla
                switch (paraBirimi.ToUpper())
                {
                    case "USD":
                        toplamMaliyet = aktifKayitlar.Sum(f => f.KalanMiktar * f.USDBirimFiyat);
                        break;
                    case "TRY":
                        toplamMaliyet = aktifKayitlar.Sum(f => f.KalanMiktar * f.TLBirimFiyat);
                        break;
                    case "UZS":
                        toplamMaliyet = aktifKayitlar.Sum(f => f.KalanMiktar * f.UZSBirimFiyat);
                        break;
                    default:
                        toplamMaliyet = aktifKayitlar.Sum(f => f.KalanMiktar * f.USDBirimFiyat);
                        break;
                }

                return toplamMiktar > 0 ? Math.Round(toplamMaliyet / toplamMiktar, 4) : 0;
            }, "GetOrtalamaMaliyetAsync", urunID, paraBirimi);
        }

        /// <summary>
        /// Belirli bir tutar için para birimi dönüşümü yapar
        /// </summary>
        public async Task<decimal> ParaBirimiCevirAsync(decimal deger, string kaynakParaBirimi, string hedefParaBirimi)
        {
            return await _exceptionHandler.HandleExceptionAsync(async () =>
            {
                if (kaynakParaBirimi == hedefParaBirimi)
                    return deger;

                return await _dovizKuruService.ParaBirimiCevirAsync(deger, kaynakParaBirimi, hedefParaBirimi);
            }, "ParaBirimiCevirAsync", deger, kaynakParaBirimi, hedefParaBirimi);
        }

        /// <summary>
        /// USD para birimi cinsinden birim fiyat hesaplar
        /// </summary>
        public async Task<decimal> HesaplaUsdBirimFiyatAsync(decimal birimFiyat, string paraBirimi, decimal? dovizKuru = null)
        {
            return await _exceptionHandler.HandleExceptionAsync(async () =>
            {
                // USD ise doğrudan dön
                if (paraBirimi == "USD")
                    return birimFiyat;

                // Döviz kuru belirtilmiş mi kontrol et
                decimal kur = 1;
                if (!dovizKuru.HasValue || dovizKuru.Value <= 0)
                {
                    // Para birimini USD'ye çevirmek için ters kur kullanılır
                    kur = await _dovizKuruService.GetGuncelKurAsync(paraBirimi, "USD");
                }
                else
                {
                    kur = dovizKuru.Value;
                }

                // USD cinsinden birim fiyat hesapla
                return Math.Round(birimFiyat * kur, 4);
            }, "HesaplaUsdBirimFiyatAsync", birimFiyat, paraBirimi, dovizKuru);
        }

        /// <summary>
        /// TL para birimi cinsinden birim fiyat hesaplar
        /// </summary>
        public async Task<decimal> HesaplaTlBirimFiyatAsync(decimal usdBirimFiyat)
        {
            return await _exceptionHandler.HandleExceptionAsync(async () =>
            {
                var kur = await _dovizKuruService.GetGuncelKurAsync("USD", "TRY");
                return Math.Round(usdBirimFiyat * kur, 4);
            }, "HesaplaTlBirimFiyatAsync", usdBirimFiyat);
        }

        /// <summary>
        /// UZS para birimi cinsinden birim fiyat hesaplar
        /// </summary>
        public async Task<decimal> HesaplaUzsBirimFiyatAsync(decimal usdBirimFiyat)
        {
            return await _exceptionHandler.HandleExceptionAsync(async () =>
            {
                var kur = await _dovizKuruService.GetGuncelKurAsync("USD", "UZS");
                return Math.Round(usdBirimFiyat * kur, 4);
            }, "HesaplaUzsBirimFiyatAsync", usdBirimFiyat);
        }

        /// <summary>
        /// FIFO kayıtları üzerinden toplam maliyet hesaplar
        /// </summary>
        public decimal HesaplaToplamMaliyet(IEnumerable<StokFifo> fifoKayitlari, string paraBirimi = "USD")
        {
            return _exceptionHandler.HandleException(() =>
            {
                if (fifoKayitlari == null || !fifoKayitlari.Any())
                    return 0;

                // Para birimine göre toplam maliyet hesapla
                switch (paraBirimi.ToUpper())
                {
                    case "USD":
                        return fifoKayitlari.Sum(f => f.KalanMiktar * f.USDBirimFiyat);
                    case "TRY":
                        return fifoKayitlari.Sum(f => f.KalanMiktar * f.TLBirimFiyat);
                    case "UZS":
                        return fifoKayitlari.Sum(f => f.KalanMiktar * f.UZSBirimFiyat);
                    default:
                        return fifoKayitlari.Sum(f => f.KalanMiktar * f.USDBirimFiyat);
                }
            }, "HesaplaToplamMaliyet", paraBirimi);
        }
    }
} 
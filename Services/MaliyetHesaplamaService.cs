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
                        toplamMaliyet = aktifKayitlar.Sum(f => f.KalanMiktar * f.BirimFiyatUSD);
                        break;
                    case "TRY":
                        toplamMaliyet = aktifKayitlar.Sum(f => f.KalanMiktar * f.BirimFiyatUSD);
                        break;
                    case "UZS":
                        toplamMaliyet = aktifKayitlar.Sum(f => f.KalanMiktar * f.BirimFiyatUZS);
                        break;
                    default:
                        toplamMaliyet = aktifKayitlar.Sum(f => f.KalanMiktar * f.BirimFiyatUSD);
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
        /// TL yerine UZS para birimi cinsinden birim fiyat hesaplar
        /// </summary>
        /// <param name="usdBirimFiyat">USD cinsinden birim fiyat</param>
        /// <returns>UZS cinsinden birim fiyat</returns>
        public async Task<decimal> HesaplaUZSBirimFiyatAsync(decimal usdBirimFiyat)
        {
            return await _exceptionHandler.HandleExceptionAsync(async () => 
            {
                var kurDegeri = await _dovizKuruService.GetGuncelKurAsync("USD", "UZS");
                if (kurDegeri <= 0)
                {
                    // Kur değeri alınamazsa varsayılan değer kullan
                    kurDegeri = 13000m; // Varsayılan USD -> UZS kuru
                    _logger.LogWarning($"Güncel USD -> UZS kuru alınamadı, varsayılan değer (13000) kullanılıyor.");
                }
                
                return usdBirimFiyat * kurDegeri;
            }, "HesaplaUZSBirimFiyatAsync", usdBirimFiyat);
        }

        /// <summary>
        /// FIFO kayıtları üzerinden toplam maliyet hesaplar
        /// </summary>
        /// <param name="fifoKayitlari">FIFO kayıtları</param>
        /// <param name="paraBirimi">İstenen para birimi</param>
        /// <returns>Toplam maliyet</returns>
        public decimal HesaplaToplamMaliyet(IEnumerable<StokFifo> fifoKayitlari, string paraBirimi = "USD")
        {
            if (fifoKayitlari == null || !fifoKayitlari.Any())
                return 0;
                
            decimal toplamMaliyet = 0;
            
            // Para birimine göre toplam maliyeti hesapla
            switch (paraBirimi.ToUpper())
            {
                case "USD":
                    toplamMaliyet = fifoKayitlari.Sum(f => f.KalanMiktar * f.BirimFiyatUSD);
                    break;
                case "UZS":
                    toplamMaliyet = fifoKayitlari.Sum(f => f.KalanMiktar * f.BirimFiyatUZS);
                    break;
                default:
                    // Varsayılan olarak USD kullan
                    toplamMaliyet = fifoKayitlari.Sum(f => f.KalanMiktar * f.BirimFiyatUSD);
                    break;
            }
            
            return toplamMaliyet;
        }

        /// <summary>
        /// Bir ürünün ortalama maliyetini hesaplar
        /// </summary>
        /// <param name="urunID">Ürün ID</param>
        /// <param name="paraBirimi">Para birimi (USD, UZS)</param>
        /// <returns>Ortalama maliyet</returns>
        public async Task<decimal> HesaplaOrtalamaMaliyetAsync(Guid urunID, string paraBirimi = "USD")
        {
            return await _exceptionHandler.HandleExceptionAsync(async () =>
            {
                // Aktif FIFO kayıtlarını getir
                var aktifKayitlar = await _unitOfWork.StokFifoRepository.GetAllAsync(
                    f => f.UrunID == urunID && f.KalanMiktar > 0 && f.Aktif && !f.Iptal && !f.Silindi,
                    asNoTracking: true);
                
                if (!aktifKayitlar.Any() || aktifKayitlar.Sum(f => f.KalanMiktar) == 0)
                    return 0;
                
                decimal toplamMiktar = aktifKayitlar.Sum(f => f.KalanMiktar);
                decimal toplamMaliyet = 0;
                
                // Para birimine göre toplam maliyeti hesapla
                switch (paraBirimi.ToUpper())
                {
                    case "USD":
                        toplamMaliyet = aktifKayitlar.Sum(f => f.KalanMiktar * f.BirimFiyatUSD);
                        break;
                    case "UZS":
                        toplamMaliyet = aktifKayitlar.Sum(f => f.KalanMiktar * f.BirimFiyatUZS);
                        break;
                    default:
                        // Varsayılan olarak USD kullan
                        toplamMaliyet = aktifKayitlar.Sum(f => f.KalanMiktar * f.BirimFiyatUSD);
                        break;
                }
                
                return toplamMaliyet / toplamMiktar;
            }, "HesaplaOrtalamaMaliyetAsync", urunID, paraBirimi);
        }
        
        /// <summary>
        /// Belirli bir tarihteki ürün maliyetini hesaplar
        /// </summary>
        /// <param name="urunID">Ürün ID</param>
        /// <param name="tarih">Tarih</param>
        /// <param name="paraBirimi">Para birimi (USD, UZS)</param>
        /// <returns>Maliyet</returns>
        public async Task<decimal> HesaplaTarihselMaliyetAsync(Guid urunID, DateTime tarih, string paraBirimi = "USD")
        {
            return await _exceptionHandler.HandleExceptionAsync(async () =>
            {
                // Belirli bir tarihe kadar olan FIFO kayıtlarını getir
                var fifoKayitlari = await _unitOfWork.Repository<StokFifo>().GetAsync(
                    f => f.UrunID == urunID && f.GirisTarihi <= tarih && f.Aktif && !f.Iptal && !f.Silindi);
                
                if (!fifoKayitlari.Any())
                    return 0;
                
                // StokCikisDetay tablosunu kontrol et
                var cikisDetaylar = await _unitOfWork.Repository<StokCikisDetay>().GetAsync(
                    c => c.StokFifoID != null && 
                         fifoKayitlari.Select(f => f.StokFifoID).Contains(c.StokFifoID.Value) && 
                         c.CikisTarihi <= tarih && !c.Iptal);
                
                decimal toplamMiktar = 0;
                decimal toplamMaliyet = 0;
                
                foreach (var fifo in fifoKayitlari)
                {
                    // O tarihe kadar olan çıkışları hesapla
                    decimal cikisMiktari = cikisDetaylar
                        .Where(c => c.StokFifoID == fifo.StokFifoID)
                        .Sum(c => c.CikisMiktari);
                    
                    decimal kalan = Math.Max(0, fifo.Miktar - cikisMiktari);
                    
                    if (kalan <= 0)
                        continue;
                    
                    toplamMiktar += kalan;
                    
                    switch (paraBirimi.ToUpper())
                    {
                        case "USD":
                            toplamMaliyet += kalan * fifo.BirimFiyatUSD;
                            break;
                        case "UZS":
                            toplamMaliyet += kalan * fifo.BirimFiyatUZS;
                            break;
                        default:
                            // Varsayılan olarak USD kullan
                            toplamMaliyet += kalan * fifo.BirimFiyatUSD;
                            break;
                    }
                }
                
                return toplamMiktar > 0 ? toplamMaliyet / toplamMiktar : 0;
            }, "HesaplaTarihselMaliyetAsync", urunID, tarih, paraBirimi);
        }
    }
} 
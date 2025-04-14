using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MuhasebeStokWebApp.Data;
using MuhasebeStokWebApp.Data.Entities;
using MuhasebeStokWebApp.Data.Entities.ParaBirimiModulu;
using MuhasebeStokWebApp.Services.Interfaces;

namespace MuhasebeStokWebApp.Services
{
    public class MaliyetHesaplamaService : IMaliyetHesaplamaService
    {
        private readonly ApplicationDbContext _context;
        private readonly IDovizKuruService _dovizKuruService;
        private readonly ILogger<MaliyetHesaplamaService> _logger;

        public MaliyetHesaplamaService(
            ApplicationDbContext context,
            IDovizKuruService dovizKuruService,
            ILogger<MaliyetHesaplamaService> logger)
        {
            _context = context;
            _dovizKuruService = dovizKuruService;
            _logger = logger;
        }

        /// <summary>
        /// Belirli bir ürün için ortalama maliyet hesaplar
        /// </summary>
        public async Task<decimal> GetOrtalamaMaliyetAsync(Guid urunID, string paraBirimi = "USD")
        {
            var aktifKayitlar = await _context.Set<StokFifo>()
                .Where(f => f.UrunID == urunID && f.KalanMiktar > 0 && f.Aktif && !f.Silindi && !f.Iptal)
                .AsNoTracking() // Performans için - EF Core takip etmesin
                .ToListAsync();
            
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
        }

        /// <summary>
        /// Belirli bir tutar için para birimi dönüşümü yapar
        /// </summary>
        public async Task<decimal> ParaBirimiCevirAsync(decimal deger, string kaynakParaBirimi, string hedefParaBirimi)
        {
            if (kaynakParaBirimi == hedefParaBirimi)
                return deger;

            try
            {
                return await _dovizKuruService.ParaBirimiCevirAsync(deger, kaynakParaBirimi, hedefParaBirimi);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Para birimi dönüşümü sırasında hata: {Message}", ex.Message);
                throw new InvalidOperationException($"Para birimi dönüşümü sırasında hata: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// USD para birimi cinsinden birim fiyat hesaplar
        /// </summary>
        public async Task<decimal> HesaplaUsdBirimFiyatAsync(decimal birimFiyat, string paraBirimi, decimal? dovizKuru = null)
        {
            // USD ise doğrudan dön
            if (paraBirimi == "USD")
                return birimFiyat;

            // Döviz kuru belirtilmiş mi kontrol et
            decimal kur = 1;
            if (!dovizKuru.HasValue || dovizKuru.Value <= 0)
            {
                try
                {
                    // Para birimini USD'ye çevirmek için ters kur kullanılır
                    kur = await _dovizKuruService.GetGuncelKurAsync(paraBirimi, "USD");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "{ParaBirimi} için güncel kur alınırken hata oluştu", paraBirimi);
                    throw new InvalidOperationException($"{paraBirimi} için güncel kur alınırken hata oluştu: {ex.Message}", ex);
                }
            }
            else
            {
                kur = dovizKuru.Value;
            }

            // USD cinsinden birim fiyat hesapla
            return Math.Round(birimFiyat * kur, 4);
        }

        /// <summary>
        /// TL para birimi cinsinden birim fiyat hesaplar
        /// </summary>
        public async Task<decimal> HesaplaTlBirimFiyatAsync(decimal usdBirimFiyat)
        {
            try
            {
                var kur = await _dovizKuruService.GetGuncelKurAsync("USD", "TRY");
                return Math.Round(usdBirimFiyat * kur, 4);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "TL birim fiyat hesaplanırken hata oluştu: {Message}", ex.Message);
                return 0; // Hata durumunda 0 dön, ancak loglama yap
            }
        }

        /// <summary>
        /// UZS para birimi cinsinden birim fiyat hesaplar
        /// </summary>
        public async Task<decimal> HesaplaUzsBirimFiyatAsync(decimal usdBirimFiyat)
        {
            try
            {
                var kur = await _dovizKuruService.GetGuncelKurAsync("USD", "UZS");
                return Math.Round(usdBirimFiyat * kur, 4);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UZS birim fiyat hesaplanırken hata oluştu: {Message}", ex.Message);
                return 0; // Hata durumunda 0 dön, ancak loglama yap
            }
        }

        /// <summary>
        /// FIFO kayıtları üzerinden toplam maliyet hesaplar
        /// </summary>
        public decimal HesaplaToplamMaliyet(IEnumerable<StokFifo> fifoKayitlari, string paraBirimi = "USD")
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
        }
    }
} 
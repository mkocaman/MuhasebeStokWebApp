using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MuhasebeStokWebApp.Data.Entities.DovizModulu;
using MuhasebeStokWebApp.Models;

namespace MuhasebeStokWebApp.Services.Interfaces
{
    /// <summary>
    /// Döviz kuru işlemleri için servis arayüzü
    /// </summary>
    public interface IDovizKuruService
    {
        /// <summary>
        /// Belirli bir tarih için tüm döviz kurlarını getirir
        /// </summary>
        Task<List<KurDegeri>> GetKurDegerleriByTarihAsync(DateTime? tarih);

        /// <summary>
        /// Belirli bir para birimi için son kur değerini getirir
        /// </summary>
        Task<KurDegeri?> GetSonKurDegeriByParaBirimiAsync(Guid paraBirimiId);

        /// <summary>
        /// Para birimi koduna göre son kur değerini getirir
        /// </summary>
        Task<KurDegeri?> GetSonKurDegeriByKodAsync(string kod);

        /// <summary>
        /// İki para birimi arasındaki çapraz kur değerini hesaplar
        /// </summary>
        Task<decimal> HesaplaKurDegeriAsync(Guid kaynakParaBirimiId, Guid hedefParaBirimiId, DateTime? tarih = null);

        /// <summary>
        /// İki para birimi kodu arasındaki çapraz kur değerini hesaplar
        /// </summary>
        Task<decimal> HesaplaKurDegeriByKodAsync(string kaynakKod, string hedefKod, DateTime? tarih = null);

        /// <summary>
        /// Belirli bir tutarı kaynak para biriminden hedef para birimine çevirir
        /// </summary>
        Task<decimal> CevirmeTutarAsync(decimal tutar, Guid kaynakParaBirimiId, Guid hedefParaBirimiId, DateTime? tarih = null);

        /// <summary>
        /// Belirli bir tutarı kaynak para birimi kodundan hedef para birimi koduna çevirir
        /// </summary>
        Task<decimal> CevirmeTutarByKodAsync(decimal tutar, string kaynakKod, string hedefKod, DateTime? tarih = null);

        /// <summary>
        /// Belirli bir tutarı kaynak para birimi kodundan hedef para birimi koduna çevirir (ParaBirimiCevir metodu ile aynı işlevi görür)
        /// </summary>
        Task<decimal> ParaBirimiCevirAsync(decimal tutar, string kaynakKod, string hedefKod, DateTime? tarih = null);

        /// <summary>
        /// İki para birimi kodu arasındaki güncel kur değerini hesaplar
        /// </summary>
        Task<decimal> GetGuncelKurAsync(string kaynakKod, string hedefKod, DateTime? tarih = null);

        /// <summary>
        /// En son döviz kurlarını getirir
        /// </summary>
        Task<List<DovizKuru>> GetLatestRatesAsync(int count = 5);

        /// <summary>
        /// Döviz kurlarını dış servislerden (örneğin TCMB) güncellemek için metod
        /// </summary>
        Task<bool> GuncelleKurDegerleriniFromTCMBAsync();
        
        /// <summary>
        /// Döviz kuru önbelleğini temizler
        /// </summary>
        Task<bool> ClearCacheAsync();
    }
} 
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MuhasebeStokWebApp.Data.Entities.ParaBirimiBirlesikModul;

namespace MuhasebeStokWebApp.Services.ParaBirimiBirlesikModul
{
    /// <summary>
    /// Birleştirilmiş para birimi ve kur işlemleri için servis arayüzü
    /// </summary>
    public interface IParaBirimiService
    {
        #region Para Birimi İşlemleri
        /// <summary>
        /// Tüm para birimlerini döndürür
        /// </summary>
        Task<List<ParaBirimi>> GetAllParaBirimleriAsync(bool aktiflerOnly = true);
        
        /// <summary>
        /// ID'ye göre para birimi döndürür
        /// </summary>
        Task<ParaBirimi> GetParaBirimiByIdAsync(Guid paraBirimiId);
        
        /// <summary>
        /// Koda göre para birimi döndürür
        /// </summary>
        Task<ParaBirimi> GetParaBirimiByKodAsync(string kod);
        
        /// <summary>
        /// Para birimi ekler
        /// </summary>
        Task<ParaBirimi> AddParaBirimiAsync(ParaBirimi paraBirimi);
        
        /// <summary>
        /// Para birimi günceller
        /// </summary>
        Task<ParaBirimi> UpdateParaBirimiAsync(ParaBirimi paraBirimi);
        
        /// <summary>
        /// Para birimi siler
        /// </summary>
        Task<bool> DeleteParaBirimiAsync(Guid paraBirimiId);
        
        /// <summary>
        /// Ana para birimi kodunu döndürür
        /// </summary>
        Task<string> GetAnaParaBirimiKoduAsync();
        
        /// <summary>
        /// Ana para birimini döndürür
        /// </summary>
        Task<ParaBirimi> GetAnaParaBirimiAsync();
        
        /// <summary>
        /// Ana para birimini değiştirir
        /// </summary>
        Task<bool> SetAnaParaBirimiAsync(Guid paraBirimiId);
        
        /// <summary>
        /// Varsayılan para birimlerini ekler
        /// </summary>
        Task<bool> VarsayilanParaBirimleriniEkleAsync();
        
        /// <summary>
        /// Para birimi sıralamasını günceller
        /// </summary>
        Task<bool> UpdateParaBirimiSiralamaAsync(List<Guid> paraBirimiIdSiralama);
        #endregion
        
        #region Kur Değeri İşlemleri
        /// <summary>
        /// Tüm kur değerlerini döndürür
        /// </summary>
        Task<List<KurDegeri>> GetAllKurDegerleriAsync(DateTime? tarih = null);
        
        /// <summary>
        /// Para birimine göre kur değerlerini döndürür
        /// </summary>
        Task<List<KurDegeri>> GetKurDegerleriByParaBirimiAsync(Guid paraBirimiId, DateTime? baslangicTarihi = null, DateTime? bitisTarihi = null);
        
        /// <summary>
        /// Belirtilen tarihteki kur değerlerini döndürür
        /// </summary>
        Task<List<KurDegeri>> GetKurDegerleriByTarihAsync(DateTime? tarih);
        
        /// <summary>
        /// Son kur değerini döndürür
        /// </summary>
        Task<KurDegeri> GetSonKurDegeriByParaBirimiAsync(Guid paraBirimiId);
        
        /// <summary>
        /// Para birimi koduna göre son kur değerini döndürür
        /// </summary>
        Task<KurDegeri> GetSonKurDegeriByKodAsync(string kod);
        
        /// <summary>
        /// Kur değeri ekler
        /// </summary>
        Task<KurDegeri> AddKurDegeriAsync(KurDegeri kurDegeri);
        
        /// <summary>
        /// Kur değerlerini toplu olarak ekler
        /// </summary>
        Task<List<KurDegeri>> AddKurDegerleriAsync(List<KurDegeri> kurDegerleri);
        
        /// <summary>
        /// Kur değeri günceller
        /// </summary>
        Task<KurDegeri> UpdateKurDegeriAsync(KurDegeri kurDegeri);
        
        /// <summary>
        /// Kur değeri siler
        /// </summary>
        Task<bool> DeleteKurDegeriAsync(Guid kurDegeriId);
        
        /// <summary>
        /// Kur değerlerini harici API'den günceller (örn. TCMB)
        /// </summary>
        Task<bool> GuncelleKurDegerleriniFromAPIAsync();
        
        /// <summary>
        /// En son döviz kurlarını getirir
        /// </summary>
        Task<List<KurDegeri>> GetLatestRatesAsync(int count = 5);
        
        /// <summary>
        /// Döviz kurlarını TCMB'den güncellemek için metod
        /// </summary>
        Task<bool> GuncelleKurDegerleriniFromTCMBAsync();
        
        /// <summary>
        /// Döviz kuru önbelleğini temizler
        /// </summary>
        Task<bool> ClearCacheAsync();
        #endregion
        
        #region Para Birimi İlişkileri
        /// <summary>
        /// Tüm para birimi ilişkilerini döndürür
        /// </summary>
        Task<List<ParaBirimiIliski>> GetAllParaBirimiIliskileriAsync(bool aktiflerOnly = true);
        
        /// <summary>
        /// ID'ye göre para birimi ilişkisini döndürür
        /// </summary>
        Task<ParaBirimiIliski> GetParaBirimiIliskiByIdAsync(Guid iliskiId);
        
        /// <summary>
        /// İki para birimi arasındaki ilişkiyi döndürür
        /// </summary>
        Task<ParaBirimiIliski> GetIliskiByParaBirimleriAsync(Guid kaynakId, Guid hedefId);
        
        /// <summary>
        /// Para birimi ilişkisi ekler
        /// </summary>
        Task<ParaBirimiIliski> AddParaBirimiIliskiAsync(ParaBirimiIliski paraBirimiIliski);
        
        /// <summary>
        /// Para birimi ilişkisi günceller
        /// </summary>
        Task<ParaBirimiIliski> UpdateParaBirimiIliskiAsync(ParaBirimiIliski paraBirimiIliski);
        
        /// <summary>
        /// Para birimi ilişkisi siler
        /// </summary>
        Task<bool> DeleteParaBirimiIliskiAsync(Guid iliskiId);
        
        /// <summary>
        /// Para birimine ait tüm ilişkileri döndürür
        /// </summary>
        Task<List<ParaBirimiIliski>> GetParaBirimiIliskileriAsync(Guid paraBirimiId);
        
        /// <summary>
        /// Para biriminin ilişkisinin olup olmadığını kontrol eder
        /// </summary>
        Task<bool> HasParaBirimiIliskiAsync(Guid paraBirimiId);
        #endregion
        
        #region Hesaplama İşlemleri
        /// <summary>
        /// İki para birimi arasındaki kur değerini hesaplar
        /// </summary>
        Task<decimal> HesaplaKurDegeriAsync(Guid kaynakParaBirimiId, Guid hedefParaBirimiId, DateTime? tarih = null);
        
        /// <summary>
        /// İki para birimi kodu arasındaki kur değerini hesaplar
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
        /// Para birimi dönüşümü yapar (CevirmeTutarByKodAsync ile aynı işlevi görür)
        /// </summary>
        Task<decimal> ParaBirimiCevirAsync(decimal tutar, string kaynakKod, string hedefKod, DateTime? tarih = null);
        
        /// <summary>
        /// İki para birimi kodu arasındaki güncel kur değerini hesaplar
        /// </summary>
        Task<decimal> GetGuncelKurAsync(string kaynakKod, string hedefKod, DateTime? tarih = null);
        
        /// <summary>
        /// Güncel kur değerini döndürür
        /// </summary>
        Task<decimal> GetCurrentExchangeRateAsync(string sourceCurrency, string targetCurrency);
        #endregion
    }
} 
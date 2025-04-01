using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MuhasebeStokWebApp.Data.Entities.ParaBirimiModulu;

namespace MuhasebeStokWebApp.Services.ParaBirimiModulu
{
    /// <summary>
    /// Para birimi ve kur işlemleri için servis arayüzü
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
        /// Son kur değerini döndürür
        /// </summary>
        Task<KurDegeri> GetSonKurDegeriByParaBirimiAsync(Guid paraBirimiId);
        
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
        /// Kur değerlerini harici API'den günceller
        /// </summary>
        Task<bool> GuncelleKurDegerleriniFromAPIAsync();
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
        #endregion
        
        #region Hesaplama İşlemleri
        /// <summary>
        /// İki para birimi arasındaki kur değerini hesaplar
        /// </summary>
        Task<decimal> HesaplaKurDegeriAsync(Guid kaynakParaBirimiId, Guid hedefParaBirimiId, DateTime? tarih = null);
        
        /// <summary>
        /// İki para birimi arasında dönüşüm yapar
        /// </summary>
        Task<decimal> CevirmeTutarAsync(decimal tutar, Guid kaynakParaBirimiId, Guid hedefParaBirimiId, DateTime? tarih = null);
        #endregion

        Task<IEnumerable<ParaBirimi>> GetAllAsync();
    }
} 
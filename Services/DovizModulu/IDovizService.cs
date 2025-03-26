using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MuhasebeStokWebApp.Data.Entities.DovizModulu;

namespace MuhasebeStokWebApp.Services.DovizModulu
{
    /// <summary>
    /// Döviz işlemleri için servis arayüzü
    /// </summary>
    public interface IDovizService
    {
        #region Para Birimi (Currency) İşlemleri
        
        /// <summary>
        /// Tüm para birimlerini listeler
        /// </summary>
        Task<List<ParaBirimi>> GetAllParaBirimleriAsync(bool aktiflerOnly = true);
        
        /// <summary>
        /// Para birimini ID'ye göre getirir
        /// </summary>
        Task<ParaBirimi?> GetParaBirimiByIdAsync(Guid paraBirimiId);
        
        /// <summary>
        /// Para birimini koduna göre getirir (USD, EUR vb.)
        /// </summary>
        Task<ParaBirimi?> GetParaBirimiByKodAsync(string kod);
        
        /// <summary>
        /// Para birimi ekler
        /// </summary>
        Task<ParaBirimi?> AddParaBirimiAsync(ParaBirimi paraBirimi);
        
        /// <summary>
        /// Para birimi günceller
        /// </summary>
        Task<ParaBirimi?> UpdateParaBirimiAsync(ParaBirimi paraBirimi);
        
        /// <summary>
        /// Para birimi siler (soft delete)
        /// </summary>
        Task<bool> DeleteParaBirimiAsync(Guid paraBirimiId);
        
        #endregion
        
        #region Kur Değeri (Exchange Rate) İşlemleri
        
        /// <summary>
        /// Tüm kur değerlerini listeler
        /// </summary>
        Task<List<KurDegeri>> GetAllKurDegerleriAsync(DateTime? tarih = null);
        
        /// <summary>
        /// Belirli bir para birimi için tüm kurları getirir
        /// </summary>
        Task<List<KurDegeri>> GetKurDegerleriByParaBirimiAsync(Guid paraBirimiId, DateTime? baslangicTarihi = null, DateTime? bitisTarihi = null);
        
        /// <summary>
        /// Belirli bir kod için tüm kurları getirir
        /// </summary>
        Task<List<KurDegeri>> GetKurDegerleriByKodAsync(string kod, DateTime? baslangicTarihi = null, DateTime? bitisTarihi = null);
        
        /// <summary>
        /// Belirli bir para birimi için son kur değerini getirir
        /// </summary>
        Task<KurDegeri?> GetSonKurDegeriByParaBirimiAsync(Guid paraBirimiId);
        
        /// <summary>
        /// Belirli bir kod için son kur değerini getirir
        /// </summary>
        Task<KurDegeri?> GetSonKurDegeriByKodAsync(string kod);
        
        /// <summary>
        /// Belirli bir para birimi ve tarihe göre kur değerini getirir
        /// </summary>
        Task<KurDegeri?> GetKurDegeriByParaBirimiAndTarihAsync(Guid paraBirimiId, DateTime tarih);
        
        /// <summary>
        /// Belirli bir kod ve tarihe göre kur değerini getirir
        /// </summary>
        Task<KurDegeri?> GetKurDegeriByKodAndTarihAsync(string kod, DateTime tarih);
        
        /// <summary>
        /// Yeni kur değeri ekler
        /// </summary>
        Task<KurDegeri?> AddKurDegeriAsync(KurDegeri kurDegeri);
        
        /// <summary>
        /// Birden fazla yeni kur değeri ekler
        /// </summary>
        Task<List<KurDegeri>> AddKurDegerleriAsync(List<KurDegeri> kurDegerleri);
        
        /// <summary>
        /// Kur değerini günceller
        /// </summary>
        Task<KurDegeri?> UpdateKurDegeriAsync(KurDegeri kurDegeri);
        
        /// <summary>
        /// Kur değerini siler (soft delete)
        /// </summary>
        Task<bool> DeleteKurDegeriAsync(Guid kurDegeriId);
        
        #endregion
        
        #region Döviz İlişkisi (Currency Relation) İşlemleri
        
        /// <summary>
        /// Tüm döviz ilişkilerini listeler
        /// </summary>
        Task<List<DovizIliski>> GetAllDovizIliskileriAsync(bool aktiflerOnly = true);
        
        /// <summary>
        /// Döviz ilişkisi ID'ye göre getirir
        /// </summary>
        Task<DovizIliski?> GetDovizIliskiByIdAsync(Guid dovizIliskiId);
        
        /// <summary>
        /// Belirli iki para birimi arasındaki döviz ilişkisini getirir
        /// </summary>
        Task<DovizIliski?> GetDovizIliskiByKurlarAsync(Guid kaynakParaBirimiId, Guid hedefParaBirimiId);
        
        /// <summary>
        /// Belirli iki kod arasındaki döviz ilişkisini getirir
        /// </summary>
        Task<DovizIliski?> GetDovizIliskiByKodlarAsync(string kaynakKod, string hedefKod);
        
        /// <summary>
        /// Döviz ilişkisi ekler
        /// </summary>
        Task<DovizIliski?> AddDovizIliskiAsync(DovizIliski dovizIliski);
        
        /// <summary>
        /// Döviz ilişkisi günceller
        /// </summary>
        Task<DovizIliski?> UpdateDovizIliskiAsync(DovizIliski dovizIliski);
        
        /// <summary>
        /// Döviz ilişkisi siler (soft delete)
        /// </summary>
        Task<bool> DeleteDovizIliskiAsync(Guid dovizIliskiId);
        
        #endregion
        
        #region Çapraz Kur Hesaplama İşlemleri
        
        /// <summary>
        /// Belirli iki para birimi arasındaki kur değerini hesaplar
        /// </summary>
        Task<decimal> HesaplaKurDegeriAsync(Guid kaynakParaBirimiId, Guid hedefParaBirimiId, DateTime? tarih = null);
        
        /// <summary>
        /// Belirli iki kod arasındaki kur değerini hesaplar
        /// </summary>
        Task<decimal> HesaplaKurDegeriByKodAsync(string kaynakKod, string hedefKod, DateTime? tarih = null);
        
        /// <summary>
        /// Para birimini belirtilen hedef para birimine çevirir
        /// </summary>
        /// <param name="tutar">Çevrilecek tutar</param>
        /// <param name="kaynakParaBirimiId">Kaynak para birimi ID</param>
        /// <param name="hedefParaBirimiId">Hedef para birimi ID</param>
        /// <param name="tarih">Çevrim için kullanılacak kur tarihi (null ise bugün)</param>
        Task<decimal> CevirmeTutarAsync(decimal tutar, Guid kaynakParaBirimiId, Guid hedefParaBirimiId, DateTime? tarih = null);
        
        /// <summary>
        /// Para birimini belirtilen hedef para birimine çevirir
        /// </summary>
        /// <param name="tutar">Çevrilecek tutar</param>
        /// <param name="kaynakKod">Kaynak para birimi kodu</param>
        /// <param name="hedefKod">Hedef para birimi kodu</param>
        /// <param name="tarih">Çevrim için kullanılacak kur tarihi (null ise bugün)</param>
        Task<decimal> CevirmeTutarByKodAsync(decimal tutar, string kaynakKod, string hedefKod, DateTime? tarih = null);
        
        #endregion
        
        #region Merkez Bankası API Entegrasyonu
        
        /// <summary>
        /// Merkez Bankası'ndan güncel kurları çeker
        /// </summary>
        Task<bool> GuncelleKurDegerleriniFromTCMBAsync();
        
        /// <summary>
        /// Son kur güncelleme tarihini getirir
        /// </summary>
        Task<DateTime> GetSonKurGuncellemeTarihiAsync();
        
        /// <summary>
        /// Otomatik güncelleme kontrolünü yapar
        /// </summary>
        Task<bool> OtomatikGuncellemeleriKontrolEtAsync();
        
        #endregion
        
        #region Para Birimi Yönetimi
        
        /// <summary>
        /// Ana para birimi kodunu getirir
        /// </summary>
        Task<string> GetAnaParaBirimiKoduAsync();
        
        /// <summary>
        /// Ana para birimi getirir
        /// </summary>
        Task<ParaBirimi?> GetAnaParaBirimiAsync();
        
        /// <summary>
        /// Ana para birimini değiştirir
        /// </summary>
        Task<bool> SetAnaParaBirimiAsync(Guid paraBirimiId);
        
        /// <summary>
        /// Ana para birimini koduna göre değiştirir
        /// </summary>
        Task<bool> SetAnaParaBirimiByKodAsync(string kod);
        
        #endregion
    }
} 
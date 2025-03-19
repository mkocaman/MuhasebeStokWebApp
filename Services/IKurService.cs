using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MuhasebeStokWebApp.Data.Entities;
using MuhasebeStokWebApp.ViewModels.Kur;

namespace MuhasebeStokWebApp.Services
{
    public interface IKurService
    {
        #region Para Birimi İşlemleri
        /// <summary>
        /// Tüm para birimlerini getirir
        /// </summary>
        Task<List<ParaBirimi>> GetParaBirimleriAsync();
        
        /// <summary>
        /// Para birimi ID'sine göre para birimini getirir
        /// </summary>
        Task<ParaBirimi> GetParaBirimiByIdAsync(Guid id);
        
        /// <summary>
        /// Para birimi koduna göre para birimini getirir
        /// </summary>
        Task<ParaBirimi> GetParaBirimiByKodAsync(string kod);
        
        /// <summary>
        /// Yeni para birimi ekler
        /// </summary>
        Task<ParaBirimi> AddParaBirimiAsync(ParaBirimi paraBirimi);
        
        /// <summary>
        /// Para birimini günceller
        /// </summary>
        Task<ParaBirimi> UpdateParaBirimiAsync(ParaBirimi paraBirimi);
        
        /// <summary>
        /// Para birimini siler (soft delete)
        /// </summary>
        Task DeleteParaBirimiAsync(Guid id);
        #endregion

        #region Kur Değeri İşlemleri
        /// <summary>
        /// Tüm kur değerlerini getirir
        /// </summary>
        Task<List<KurDegeri>> GetKurDegerleriAsync();
        
        /// <summary>
        /// Kur değerini ID'ye göre getirir
        /// </summary>
        Task<KurDegeri> GetKurDegeriByIdAsync(Guid id);
        
        /// <summary>
        /// Belirli bir para birimi çifti için kur değerini getirir
        /// </summary>
        Task<KurDegeri> GetKurDegeriAsync(string kaynakKod, string hedefKod);
        
        /// <summary>
        /// Yeni kur değeri ekler
        /// </summary>
        Task<KurDegeri> AddKurDegeriAsync(KurDegeri kurDegeri);
        
        /// <summary>
        /// Kur değerini günceller
        /// </summary>
        Task<KurDegeri> UpdateKurDegeriAsync(KurDegeri kurDegeri);
        
        /// <summary>
        /// Kur değerini siler (soft delete)
        /// </summary>
        Task DeleteKurDegeriAsync(Guid id);
        #endregion

        #region Döviz İlişkileri İşlemleri
        /// <summary>
        /// Tüm döviz ilişkilerini getirir
        /// </summary>
        Task<List<DovizIliski>> GetDovizIliskileriAsync();
        
        /// <summary>
        /// Döviz ilişkisini ID'ye göre getirir
        /// </summary>
        Task<DovizIliski> GetDovizIliskiByIdAsync(Guid id);
        
        /// <summary>
        /// Yeni döviz ilişkisi ekler
        /// </summary>
        Task<DovizIliski> AddDovizIliskiAsync(DovizIliski dovizIliski);
        
        /// <summary>
        /// Döviz ilişkisini günceller
        /// </summary>
        Task<DovizIliski> UpdateDovizIliskiAsync(DovizIliski dovizIliski);
        
        /// <summary>
        /// Döviz ilişkisini siler (soft delete)
        /// </summary>
        Task DeleteDovizIliskiAsync(Guid id);
        #endregion

        #region Kur Ayarları
        /// <summary>
        /// Kur ayarlarını getirir
        /// </summary>
        Task<SistemAyarlari> GetKurAyarlariAsync();
        
        /// <summary>
        /// Kur ayarlarını günceller
        /// </summary>
        Task<SistemAyarlari> UpdateKurAyarlariAsync(SistemAyarlari sistemAyarlari);
        #endregion

        #region Dış Kaynaklı Kur Güncelleme
        /// <summary>
        /// Merkez Bankası'ndan kurları günceller
        /// </summary>
        Task<bool> UpdateKurlarFromMerkezBankasiAsync();
        
        /// <summary>
        /// Özbekistan Merkez Bankası'ndan kurları günceller
        /// </summary>
        Task<bool> UpdateKurlarFromUzbekistanMBAsync();
        #endregion

        #region Para Birimi Çevirme
        /// <summary>
        /// Para birimlerini birbirine çevirir
        /// </summary>
        Task<decimal> ConvertParaBirimiAsync(string kaynakKod, string hedefKod, decimal miktar);
        
        /// <summary>
        /// Güncel kur alma
        /// </summary>
        Task<decimal> GetGuncelKur(string kaynakParaBirimi, string hedefParaBirimi);
        #endregion

        #region Kur Dönüşüm İşlemleri
        Task<List<KurDegeriViewModel>> GetAllKurDegerleriAsync();
        Task<List<KurDegeriViewModel>> GetActiveKurDegerleriAsync();
        Task<List<KurDegeriViewModel>> GetKurDegerleriByParaBirimiIdAsync(Guid paraBirimiId);
        Task<KurDegeriViewModel> GetKurDegeriByIdAsync(Guid id);
        Task<KurDegeri> GetKurDegeriEntityByIdAsync(Guid id);
        Task<KurDegeriViewModel> GetSonKurDegeriByParaBirimiIdAsync(Guid paraBirimiId);
        Task<bool> AddKurDegeriAsync(KurDegeriViewModel model);
        Task<bool> UpdateKurDegeriAsync(KurDegeriViewModel model);
        Task<bool> DeleteKurDegeriAsync(Guid id);
        
        Task<decimal> DonusturKurAsync(Guid kaynakParaBirimiId, Guid hedefParaBirimiId, decimal miktar);
        Task<decimal> DonusturKurByKodAsync(string kaynakKod, string hedefKod, decimal miktar);
        #endregion

        #region Kur Ayarları İşlemleri
        Task<KurAyarlariViewModel> GetKurAyarlariAsync();
        Task<bool> UpdateKurAyarlariAsync(KurAyarlariViewModel model);
        #endregion

        #region Kur Güncelleme İşlemleri
        Task<bool> KurlariGuncelleAsync();
        Task<bool> KurlariOtomatikGuncelleAsync();
        #endregion
    }
} 
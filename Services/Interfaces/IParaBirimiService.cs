using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MuhasebeStokWebApp.Data.Entities.DovizModulu;

namespace MuhasebeStokWebApp.Services.Interfaces
{
    /// <summary>
    /// Para birimi işlemleri için servis arayüzü
    /// </summary>
    public interface IParaBirimiService
    {
        #region Para Birimi İşlemleri
        /// <summary>
        /// Tüm para birimlerini getirir
        /// </summary>
        Task<List<ParaBirimi>> GetAllParaBirimleriAsync();

        /// <summary>
        /// Aktif para birimlerini getirir
        /// </summary>
        Task<List<ParaBirimi>> GetAktifParaBirimleriAsync();

        /// <summary>
        /// ID'ye göre para birimi getirir
        /// </summary>
        Task<ParaBirimi> GetParaBirimiByIdAsync(Guid paraBirimiId);

        /// <summary>
        /// Koda göre para birimi getirir
        /// </summary>
        Task<ParaBirimi> GetParaBirimiByKodAsync(string kod);

        /// <summary>
        /// Yeni para birimi ekler
        /// </summary>
        Task<ParaBirimi> AddParaBirimiAsync(ParaBirimi paraBirimi);

        /// <summary>
        /// Para birimi günceller
        /// </summary>
        Task<ParaBirimi> UpdateParaBirimiAsync(ParaBirimi paraBirimi);

        /// <summary>
        /// Para birimi siler (soft delete)
        /// </summary>
        Task<bool> DeleteParaBirimiAsync(Guid paraBirimiId);
        #endregion

        #region Para Birimi İlişkileri
        /// <summary>
        /// Tüm para birimi ilişkilerini getirir
        /// </summary>
        Task<List<DovizIliski>> GetAllParaBirimiIliskileriAsync();

        /// <summary>
        /// Aktif para birimi ilişkilerini getirir
        /// </summary>
        Task<List<DovizIliski>> GetAktifParaBirimiIliskileriAsync();

        /// <summary>
        /// Belirli bir para birimine ait ilişkileri getirir
        /// </summary>
        Task<List<DovizIliski>> GetParaBirimiIliskileriAsync(Guid paraBirimiId);

        /// <summary>
        /// ID'ye göre para birimi ilişkisi getirir
        /// </summary>
        Task<DovizIliski> GetParaBirimiIliskiByIdAsync(Guid iliskiId);

        /// <summary>
        /// İki para birimi arasındaki ilişkiyi getirir
        /// </summary>
        Task<DovizIliski> GetIliskiByParaBirimleriAsync(Guid kaynakId, Guid hedefId);

        /// <summary>
        /// Yeni para birimi ilişkisi ekler
        /// </summary>
        Task<DovizIliski> AddParaBirimiIliskiAsync(DovizIliski dovizIliski);

        /// <summary>
        /// Para birimi ilişkisi günceller
        /// </summary>
        Task<DovizIliski> UpdateParaBirimiIliskiAsync(DovizIliski dovizIliski);

        /// <summary>
        /// Para birimi ilişkisi siler (soft delete)
        /// </summary>
        Task<bool> DeleteParaBirimiIliskiAsync(Guid iliskiId);

        /// <summary>
        /// Para biriminin ilişkisi var mı kontrol eder
        /// </summary>
        Task<bool> HasParaBirimiIliskiAsync(Guid paraBirimiId);
        #endregion

        #region Diğer
        /// <summary>
        /// Para birimi sıralamalarını günceller
        /// </summary>
        Task<bool> UpdateParaBirimiSiralamaAsync(List<Guid> paraBirimiIdSiralama);

        /// <summary>
        /// Varsayılan para birimlerini ekler (ilk kurulum için)
        /// </summary>
        Task<bool> VarsayilanParaBirimleriniEkleAsync();
        #endregion
    }
} 
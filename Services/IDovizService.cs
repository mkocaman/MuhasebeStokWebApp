using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MuhasebeStokWebApp.Data.Entities;
using MuhasebeStokWebApp.ViewModels.Kur;

namespace MuhasebeStokWebApp.Services
{
    public interface IDovizService
    {
        /// <summary>
        /// Döviz kurlarını günceller
        /// </summary>
        Task<bool> GuncelleKurlariAsync();

        /// <summary>
        /// Para birimlerini birbirine çevirir
        /// </summary>
        Task<decimal> ParaBirimiCevirAsync(string kaynakParaBirimi, string hedefParaBirimi, decimal miktar);

        /// <summary>
        /// Sistemde tanımlı ana döviz kodunu getirir
        /// </summary>
        Task<string> GetAnaDovizKoduAsync();

        /// <summary>
        /// Sistemde tanımlı ana döviz kodunu ayarlar
        /// </summary>
        Task<bool> SetAnaDovizKoduAsync(string dovizKodu);

        // Para Birimi İşlemleri
        Task<List<ParaBirimiViewModel>> GetAllDovizlerAsync();
        Task<List<ParaBirimiViewModel>> GetActiveDovizsAsync();
        Task<ParaBirimiViewModel> GetDovizByIdAsync(Guid id);
        Task<ParaBirimi> GetDovizEntityByIdAsync(Guid id);
        Task<ParaBirimi> GetDovizByKodAsync(string kod);
        Task<ParaBirimiViewModel> AddDovizAsync(ParaBirimiViewModel model);
        Task<ParaBirimiViewModel> UpdateDovizAsync(ParaBirimiViewModel model);
        Task DeleteDovizAsync(Guid id);
        
        // Döviz İlişki İşlemleri
        Task<List<DovizIliskiViewModel>> GetAllDovizIliskileriAsync();
        Task<List<DovizIliskiViewModel>> GetActiveDovizIliskileriAsync();
        Task<DovizIliskiViewModel> GetDovizIliskiByIdAsync(Guid id);
        Task<DovizIliski> GetDovizIliskiEntityByIdAsync(Guid id);
        Task<DovizIliski> GetDovizIliskiByParaBirimleriAsync(Guid kaynakId, Guid hedefId);
        Task<DovizIliskiViewModel> AddDovizIliskiAsync(DovizIliskiViewModel model);
        Task<DovizIliskiViewModel> UpdateDovizIliskiAsync(DovizIliskiViewModel model);
        Task DeleteDovizIliskiAsync(Guid id);
    }
} 
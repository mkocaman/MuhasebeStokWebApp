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

        // Döviz İşlemleri
        Task<List<DovizViewModel>> GetAllDovizlerAsync();
        Task<List<DovizViewModel>> GetActiveDovizsAsync();
        Task<DovizViewModel> GetDovizByIdAsync(int id);
        Task<Doviz> GetDovizEntityByIdAsync(int id);
        Task<Doviz> GetDovizByKodAsync(string kod);
        Task<DovizViewModel> AddDovizAsync(DovizViewModel model);
        Task<DovizViewModel> UpdateDovizAsync(DovizViewModel model);
        Task DeleteDovizAsync(int id);
        
        // Döviz İlişki İşlemleri
        Task<List<DovizIliskiViewModel>> GetAllDovizIliskileriAsync();
        Task<List<DovizIliskiViewModel>> GetActiveDovizIliskileriAsync();
        Task<DovizIliskiViewModel> GetDovizIliskiByIdAsync(int id);
        Task<DovizIliski> GetDovizIliskiEntityByIdAsync(int id);
        Task<DovizIliski> GetDovizIliskiByParaBirimleriAsync(int kaynakId, int hedefId);
        Task<DovizIliskiViewModel> AddDovizIliskiAsync(DovizIliskiViewModel model);
        Task<DovizIliskiViewModel> UpdateDovizIliskiAsync(DovizIliskiViewModel model);
        Task DeleteDovizIliskiAsync(int id);
    }
} 
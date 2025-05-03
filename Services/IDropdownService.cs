using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MuhasebeStokWebApp.Services
{
    public interface IDropdownService
    {
        Task<SelectList> GetCariSelectListAsync(Guid? selectedCariId = null);
        Task<SelectList> GetUrunSelectListAsync(Guid? selectedUrunId = null);
        Task<SelectList> GetFaturaSelectListAsync(Guid? selectedFaturaId = null);
        Task<SelectList> GetDepoSelectListAsync(Guid? selectedDepoId = null);
        SelectList GetIrsaliyeTuruSelectList(string selectedTur = null);
        SelectList GetDurumSelectList(string selectedDurum = null);
        Task<SelectList> GetCariSelectList(Guid? selectedCariId = null);
        Task<SelectList> GetSozlesmeSelectListByCariId(Guid cariId, Guid? selectedSozlesmeId = null);
        
        // Yeni eklenen birim ve kategori metodları
        Task<SelectList> GetBirimSelectListAsync(Guid? selectedBirimId = null);
        Task<SelectList> GetKategoriSelectListAsync(Guid? selectedKategoriId = null);
        Task<List<SelectListItem>> GetKategoriSelectItemsAsync(Guid? selectedKategoriId = null);
        Task<List<SelectListItem>> GetBirimSelectItemsAsync(Guid? selectedBirimId = null);
        
        // Dropdown hazırlama metodları
        Task<Dictionary<string, SelectList>> PrepareCommonDropdownsAsync(Guid? selectedCariId = null, Guid? selectedUrunId = null);
        Task<Dictionary<string, SelectList>> PrepareIrsaliyeDropdownsAsync(Guid? selectedCariId = null, Guid? selectedUrunId = null, string selectedTur = null);
        Task<Dictionary<string, SelectList>> PrepareFaturaDropdownsAsync(Guid? selectedCariId = null, Guid? selectedDepoId = null);
        Task<Dictionary<string, object>> PrepareUrunDropdownsAsync(Guid? selectedBirimId = null, Guid? selectedKategoriId = null);
        Task<Dictionary<string, object>> PrepareViewBagAsync(string controller, string action, Dictionary<string, object> additionalData = null);
    }
} 
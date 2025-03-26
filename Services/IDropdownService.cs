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
        
        // Yeni eklenen metodlar
        Task<Dictionary<string, SelectList>> PrepareCommonDropdownsAsync(Guid? selectedCariId = null, Guid? selectedUrunId = null);
        Task<Dictionary<string, SelectList>> PrepareIrsaliyeDropdownsAsync(Guid? selectedCariId = null, Guid? selectedUrunId = null, string selectedTur = null);
        Task<Dictionary<string, SelectList>> PrepareFaturaDropdownsAsync(Guid? selectedCariId = null, Guid? selectedDepoId = null);
        Task<Dictionary<string, object>> PrepareViewBagAsync(string controller, string action, Dictionary<string, object> additionalData = null);
    }
} 
using System;
using System.Threading.Tasks;
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
    }
} 
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MuhasebeStokWebApp.Services.Interfaces
{
    public interface ILanguageService
    {
        Task<List<dynamic>> GetAllLanguagesAsync();
        Task<dynamic> GetLanguageByIdAsync(Guid id);
        Task<dynamic> GetLanguageByCodeAsync(string code);
        Task<string> GetCurrentLanguageCodeAsync();
        Task<bool> SetCurrentLanguageAsync(string languageCode);
    }
} 
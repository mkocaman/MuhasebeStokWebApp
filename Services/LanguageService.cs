using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using MuhasebeStokWebApp.Data;
using MuhasebeStokWebApp.Services.Interfaces;

namespace MuhasebeStokWebApp.Services
{
    public class LanguageService : ILanguageService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMemoryCache _cache;
        private readonly ILogger<LanguageService> _logger;

        public LanguageService(
            ApplicationDbContext context,
            IMemoryCache cache,
            ILogger<LanguageService> logger)
        {
            _context = context;
            _cache = cache;
            _logger = logger;
        }

        public Task<List<dynamic>> GetAllLanguagesAsync()
        {
            try
            {
                // Basitleştirilmiş örnek: Gerçek uygulamada veritabanından dilleri çekmelisiniz
                var languages = new List<dynamic>
                {
                    new { Id = Guid.NewGuid(), Code = "tr-TR", Name = "Türkçe", IsDefault = true },
                    new { Id = Guid.NewGuid(), Code = "en-US", Name = "English", IsDefault = false },
                };

                return Task.FromResult(languages);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Diller getirilirken hata oluştu");
                return Task.FromResult(new List<dynamic>());
            }
        }

        public Task<dynamic> GetLanguageByIdAsync(Guid id)
        {
            try
            {
                var languagesTask = GetAllLanguagesAsync();
                var languages = languagesTask.Result;
                return Task.FromResult(languages.FirstOrDefault(l => (Guid)l.Id == id));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ID'ye göre dil getirilirken hata oluştu");
                return Task.FromResult<dynamic>(null);
            }
        }

        public Task<dynamic> GetLanguageByCodeAsync(string code)
        {
            try
            {
                var languagesTask = GetAllLanguagesAsync();
                var languages = languagesTask.Result;
                return Task.FromResult(languages.FirstOrDefault(l => l.Code == code));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Koda göre dil getirilirken hata oluştu");
                return Task.FromResult<dynamic>(null);
            }
        }

        public Task<string> GetCurrentLanguageCodeAsync()
        {
            try
            {
                // Basitleştirilmiş örnek: Gerçek uygulamada bu değer kullanıcı bazlı veya cache'den gelebilir
                return Task.FromResult("tr-TR");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Geçerli dil kodu getirilirken hata oluştu");
                return Task.FromResult("tr-TR"); // Varsayılan dil
            }
        }

        public Task<bool> SetCurrentLanguageAsync(string languageCode)
        {
            try
            {
                // Basitleştirilmiş örnek: Gerçek uygulamada kullanıcı ayarlarına kaydedin
                // veya cookie/session'a yazın
                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Geçerli dil ayarlanırken hata oluştu");
                return Task.FromResult(false);
            }
        }
    }
} 
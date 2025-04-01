using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using MuhasebeStokWebApp.Services.Interfaces;

namespace MuhasebeStokWebApp.Services
{
    public class ValidationService : IValidationService
    {
        private readonly ILogService _logService;

        public ValidationService(ILogService logService)
        {
            _logService = logService;
        }

        /// <summary>
        /// Model doğrulama hatalarını bir dizeye dönüştürür
        /// </summary>
        public string GetModelStateErrorsAsString(ModelStateDictionary modelState)
        {
            return string.Join("; ", modelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage));
        }

        /// <summary>
        /// Model doğrulama hatalarını bir liste olarak döndürür
        /// </summary>
        public List<string> GetModelStateErrorsAsList(ModelStateDictionary modelState)
        {
            return modelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
        }

        /// <summary>
        /// Model state hatalarını loglayıp istemciye uygun bir hata yanıtı döndürür
        /// </summary>
        public async Task<object> HandleValidationErrorAsync(ModelStateDictionary modelState, string operation)
        {
            var errorMessages = GetModelStateErrorsAsString(modelState);
            await _logService.LogWarningAsync($"Model doğrulama hataları ({operation}): {errorMessages}");
            return new { success = false, message = errorMessages };
        }

        /// <summary>
        /// Metinsel bir değerin boş olup olmadığını kontrol eder, boşsa model state'e hata ekler
        /// </summary>
        public bool ValidateRequiredString(ModelStateDictionary modelState, string value, string fieldName, string errorMessage)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                modelState.AddModelError(fieldName, errorMessage);
                return false;
            }
            return true;
        }

        /// <summary>
        /// İşlemin başarısız olduğunu bildiren ve istemciye uygun bir hata yanıtı döndürür
        /// </summary>
        public async Task<object> HandleErrorAsync(string errorMessage, string operation, Exception ex = null)
        {
            if (ex != null)
            {
                await _logService.LogErrorAsync($"{operation}: {errorMessage}", ex);
            }
            else
            {
                await _logService.LogWarningAsync($"{operation}: {errorMessage}");
            }
            
            return new { success = false, message = errorMessage };
        }

        /// <summary>
        /// İşlemin başarılı olduğunu bildiren ve istemciye uygun bir yanıt döndürür
        /// </summary>
        public async Task<object> HandleSuccessAsync(string message, string operation)
        {
            await _logService.LogInfoAsync($"{operation}: {message}");
            return new { success = true, message = message };
        }
    }
} 
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
        private readonly IExceptionHandlingService _exceptionHandler;

        public ValidationService(
            ILogService logService,
            IExceptionHandlingService exceptionHandler)
        {
            _logService = logService;
            _exceptionHandler = exceptionHandler;
        }

        /// <summary>
        /// Model doğrulama hatalarını bir dizeye dönüştürür
        /// </summary>
        public string GetModelStateErrorsAsString(ModelStateDictionary modelState)
        {
            return _exceptionHandler.HandleException(() => 
            {
                return string.Join("; ", modelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));
            }, "GetModelStateErrorsAsString");
        }

        /// <summary>
        /// Model doğrulama hatalarını bir liste olarak döndürür
        /// </summary>
        public List<string> GetModelStateErrorsAsList(ModelStateDictionary modelState)
        {
            return _exceptionHandler.HandleException(() => 
            {
                return modelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
            }, "GetModelStateErrorsAsList");
        }

        /// <summary>
        /// Model state hatalarını loglayıp istemciye uygun bir hata yanıtı döndürür
        /// </summary>
        public async Task<object> HandleValidationErrorAsync(ModelStateDictionary modelState, string operation)
        {
            return await _exceptionHandler.HandleExceptionAsync(async () => 
            {
                var errorMessages = GetModelStateErrorsAsString(modelState);
                await _logService.LogWarningAsync($"Model doğrulama hataları ({operation}): {errorMessages}");
                return new { success = false, message = errorMessages };
            }, "HandleValidationErrorAsync", operation);
        }

        /// <summary>
        /// Metinsel bir değerin boş olup olmadığını kontrol eder, boşsa model state'e hata ekler
        /// </summary>
        public bool ValidateRequiredString(ModelStateDictionary modelState, string value, string fieldName, string errorMessage)
        {
            return _exceptionHandler.HandleException(() => 
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    modelState.AddModelError(fieldName, errorMessage);
                    return false;
                }
                return true;
            }, "ValidateRequiredString", fieldName);
        }

        /// <summary>
        /// İşlemin başarısız olduğunu bildiren ve istemciye uygun bir hata yanıtı döndürür
        /// </summary>
        public async Task<object> HandleErrorAsync(string errorMessage, string operation, Exception ex = null)
        {
            return await _exceptionHandler.HandleExceptionAsync(async () => 
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
            }, "HandleErrorAsync", operation, errorMessage);
        }

        /// <summary>
        /// İşlemin başarılı olduğunu bildiren ve istemciye uygun bir yanıt döndürür
        /// </summary>
        public async Task<object> HandleSuccessAsync(string message, string operation)
        {
            return await _exceptionHandler.HandleExceptionAsync(async () => 
            {
                await _logService.LogInfoAsync($"{operation}: {message}");
                return new { success = true, message = message };
            }, "HandleSuccessAsync", operation, message);
        }
    }
} 
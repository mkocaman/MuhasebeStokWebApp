using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace MuhasebeStokWebApp.Services.Interfaces
{
    public interface IValidationService
    {
        string GetModelStateErrorsAsString(ModelStateDictionary modelState);
        List<string> GetModelStateErrorsAsList(ModelStateDictionary modelState);
        Task<object> HandleValidationErrorAsync(ModelStateDictionary modelState, string operation);
        bool ValidateRequiredString(ModelStateDictionary modelState, string value, string fieldName, string errorMessage);
        Task<object> HandleErrorAsync(string errorMessage, string operation, Exception ex = null);
        Task<object> HandleSuccessAsync(string message, string operation);
    }
} 
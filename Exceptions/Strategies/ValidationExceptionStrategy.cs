using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using MuhasebeStokWebApp.Services.CustomExceptions;

namespace MuhasebeStokWebApp.Exceptions.Strategies
{
    /// <summary>
    /// ValidationException için strateji
    /// </summary>
    public class ValidationExceptionStrategy : IExceptionStrategy
    {
        /// <summary>
        /// ValidationException tipini işleyebilir
        /// </summary>
        public bool CanHandle(Exception exception)
        {
            return exception is ValidationException;
        }

        /// <summary>
        /// ValidationException için kullanıcı dostu mesaj
        /// </summary>
        public string GetUserFriendlyMessage(Exception exception)
        {
            if (exception is ValidationException validationException)
            {
                if (validationException.ValidationErrors.Any())
                {
                    // İlk birkaç validation hatasını göster
                    var errorMessages = validationException.ValidationErrors
                        .Take(3)
                        .Select(e => $"{e.PropertyName}: {e.ErrorMessage}")
                        .ToList();
                    
                    // Eğer daha fazla hata varsa belirt
                    if (validationException.ValidationErrors.Count > 3)
                    {
                        errorMessages.Add($"... ve {validationException.ValidationErrors.Count - 3} hata daha");
                    }
                    
                    return $"Lütfen girdiğiniz bilgileri kontrol ediniz: {string.Join(", ", errorMessages)}";
                }
                
                return validationException.Message;
            }
            
            return "Girdiğiniz bilgilerde hatalar var. Lütfen kontrol edip tekrar deneyiniz.";
        }

        /// <summary>
        /// ValidationException için 400 Bad Request dönüyoruz
        /// </summary>
        public int GetStatusCode(Exception exception)
        {
            return StatusCodes.Status400BadRequest;
        }
    }
} 
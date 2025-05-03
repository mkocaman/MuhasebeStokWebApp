using System;
using Microsoft.AspNetCore.Http;
using MuhasebeStokWebApp.Services.CustomExceptions;

namespace MuhasebeStokWebApp.Exceptions.Strategies
{
    /// <summary>
    /// BusinessException için strateji
    /// </summary>
    public class BusinessExceptionStrategy : IExceptionStrategy
    {
        /// <summary>
        /// BusinessException tipini işleyebilir
        /// </summary>
        public bool CanHandle(Exception exception)
        {
            return exception is BusinessException;
        }

        /// <summary>
        /// BusinessException için kullanıcı dostu mesaj
        /// </summary>
        public string GetUserFriendlyMessage(Exception exception)
        {
            if (exception is BusinessException businessException)
            {
                return businessException.Message;
            }
            
            return "İşleminiz tamamlanamadı. Lütfen girdiğiniz bilgileri kontrol edip tekrar deneyiniz.";
        }

        /// <summary>
        /// BusinessException için 400 Bad Request dönüyoruz
        /// </summary>
        public int GetStatusCode(Exception exception)
        {
            if (exception is BusinessException businessException)
            {
                return businessException.Severity switch
                {
                    ErrorSeverity.Info => StatusCodes.Status200OK,
                    ErrorSeverity.Warning => StatusCodes.Status400BadRequest,
                    ErrorSeverity.Error => StatusCodes.Status400BadRequest,
                    ErrorSeverity.Critical => StatusCodes.Status500InternalServerError,
                    _ => StatusCodes.Status400BadRequest
                };
            }
            
            return StatusCodes.Status400BadRequest;
        }
    }
} 
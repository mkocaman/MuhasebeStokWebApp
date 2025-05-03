using System;
using Microsoft.AspNetCore.Http;

namespace MuhasebeStokWebApp.Exceptions.Strategies
{
    /// <summary>
    /// Varsayılan exception stratejisi
    /// </summary>
    public class DefaultExceptionStrategy : IExceptionStrategy
    {
        /// <summary>
        /// Bu strateji tüm exception tiplerini işleyebilir (fallback)
        /// </summary>
        public bool CanHandle(Exception exception)
        {
            return true; // Varsayılan strateji - her zaman işleyebilir
        }

        /// <summary>
        /// Varsayılan kullanıcı dostu hata mesajı
        /// </summary>
        public string GetUserFriendlyMessage(Exception exception)
        {
            return "İşlem sırasında beklenmeyen bir hata oluştu. Teknik ekibimiz bu konuyla ilgileniyor.";
        }

        /// <summary>
        /// Varsayılan durum kodu 500
        /// </summary>
        public int GetStatusCode(Exception exception)
        {
            return StatusCodes.Status500InternalServerError;
        }
    }
} 
using System;

namespace MuhasebeStokWebApp.Exceptions
{
    /// <summary>
    /// Farklı exception tiplerini işlemek için strateji arayüzü
    /// </summary>
    public interface IExceptionStrategy
    {
        /// <summary>
        /// Bu strateji belirtilen exception tipini işleyebilir mi?
        /// </summary>
        /// <param name="exception">İşlenecek exception</param>
        /// <returns>İşlenebilirse true, değilse false</returns>
        bool CanHandle(Exception exception);
        
        /// <summary>
        /// Kullanıcı dostu hata mesajı oluşturur
        /// </summary>
        /// <param name="exception">Exception</param>
        /// <returns>Kullanıcı dostu hata mesajı</returns>
        string GetUserFriendlyMessage(Exception exception);
        
        /// <summary>
        /// Exception için HTTP durum kodunu döndürür
        /// </summary>
        /// <param name="exception">Exception</param>
        /// <returns>HTTP status code</returns>
        int GetStatusCode(Exception exception);
    }
} 
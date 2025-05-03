using System;

namespace MuhasebeStokWebApp.Services.Interfaces
{
    /// <summary>
    /// Exception handling işlemleri için servis arayüzü
    /// </summary>
    public interface IExceptionHandlingService
    {
        /// <summary>
        /// Exception tipine göre kullanıcı dostu bir hata mesajı döndürür
        /// </summary>
        /// <param name="exception">İşlenen exception</param>
        /// <returns>Kullanıcı dostu hata mesajı</returns>
        string GetUserFriendlyErrorMessage(Exception exception);
        
        /// <summary>
        /// Exception'ı işler ve uygun loglama yapar
        /// </summary>
        /// <param name="exception">İşlenen exception</param>
        /// <param name="errorContext">Hatanın oluştuğu bağlam/açıklama</param>
        /// <returns>Kullanıcı dostu hata mesajı</returns>
        string HandleException(Exception exception, string errorContext);
        
        /// <summary>
        /// Exception tipinin transaction'ı iptal etmesi gerekip gerekmediğini belirler
        /// </summary>
        /// <param name="exception">İşlenen exception</param>
        /// <returns>Transaction'ı iptal etmeli mi?</returns>
        bool ShouldRollbackTransaction(Exception exception);
    }
}
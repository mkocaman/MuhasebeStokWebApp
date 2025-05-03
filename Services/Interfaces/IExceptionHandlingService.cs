using System;
using System.Threading.Tasks;

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
        
        /// <summary>
        /// Verilen fonksiyonu çalıştırır ve oluşabilecek hataları yakalayıp loglar
        /// </summary>
        /// <typeparam name="T">Fonksiyonun dönüş tipi</typeparam>
        /// <param name="func">Çalıştırılacak fonksiyon</param>
        /// <param name="logContext">Log mesajında gösterilecek bağlam bilgisi (örn: metot adı, işlem adı)</param>
        /// <param name="logParams">Log mesajında gösterilecek parametreler</param>
        /// <returns>Fonksiyonun döndürdüğü değer veya hata durumunda varsayılan değer</returns>
        Task<T> HandleExceptionAsync<T>(
            Func<Task<T>> func,
            string logContext,
            params object[] logParams);

        /// <summary>
        /// Verilen fonksiyonu çalıştırır ve oluşabilecek hataları yakalayıp loglar (void fonksiyonlar için)
        /// </summary>
        /// <param name="func">Çalıştırılacak fonksiyon</param>
        /// <param name="logContext">Log mesajında gösterilecek bağlam bilgisi (örn: metot adı, işlem adı)</param>
        /// <param name="logParams">Log mesajında gösterilecek parametreler</param>
        Task HandleExceptionAsync(
            Func<Task> func,
            string logContext,
            params object[] logParams);

        /// <summary>
        /// Verilen senkron fonksiyonu çalıştırır ve oluşabilecek hataları yakalayıp loglar
        /// </summary>
        /// <typeparam name="T">Fonksiyonun dönüş tipi</typeparam>
        /// <param name="func">Çalıştırılacak fonksiyon</param>
        /// <param name="logContext">Log mesajında gösterilecek bağlam bilgisi (örn: metot adı, işlem adı)</param>
        /// <param name="logParams">Log mesajında gösterilecek parametreler</param>
        /// <returns>Fonksiyonun döndürdüğü değer veya hata durumunda varsayılan değer</returns>
        T HandleException<T>(
            Func<T> func,
            string logContext,
            params object[] logParams);

        /// <summary>
        /// Verilen senkron fonksiyonu çalıştırır ve oluşabilecek hataları yakalayıp loglar (void fonksiyonlar için)
        /// </summary>
        /// <param name="action">Çalıştırılacak fonksiyon</param>
        /// <param name="logContext">Log mesajında gösterilecek bağlam bilgisi (örn: metot adı, işlem adı)</param>
        /// <param name="logParams">Log mesajında gösterilecek parametreler</param>
        void HandleException(
            Action action,
            string logContext,
            params object[] logParams);
    }
}
using System;
using System.Threading.Tasks;

namespace MuhasebeStokWebApp.Services
{
    /// <summary>
    /// Servislerdeki tekrar eden try-catch bloklarını merkezi olarak yönetmek için kullanılan servis arayüzü
    /// </summary>
    public interface IExceptionHandlingService
    {
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
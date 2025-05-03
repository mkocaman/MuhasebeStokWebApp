using System;
using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;

namespace MuhasebeStokWebApp.Extensions
{
    /// <summary>
    /// ILogger için extension metotları
    /// </summary>
    public static class LoggingExtensions
    {
        /// <summary>
        /// Metot başlangıcını loglar
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="callerName">Çağrılan metot adı</param>
        /// <param name="parameters">Metot parametreleri</param>
        public static void LogMethodStart(this ILogger logger, 
            [CallerMemberName] string callerName = null, 
            params object[] parameters)
        {
            logger.LogInformation(
                $"{callerName} metodu başlatıldı. " +
                (parameters != null && parameters.Length > 0 
                    ? $"Parametreler: {string.Join(", ", parameters)}" 
                    : "Parametre yok")
            );
        }

        /// <summary>
        /// Metot sonunu loglar
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="result">Metot sonucu (isteğe bağlı)</param>
        /// <param name="callerName">Çağrılan metot adı</param>
        public static void LogMethodEnd(this ILogger logger, 
            object result = null, 
            [CallerMemberName] string callerName = null)
        {
            logger.LogInformation(
                $"{callerName} metodu tamamlandı. " +
                (result != null ? $"Sonuç: {result}" : "")
            );
        }

        /// <summary>
        /// Entity işlemlerini loglar
        /// </summary>
        /// <typeparam name="T">Entity tipi</typeparam>
        /// <param name="logger">Logger</param>
        /// <param name="entity">Entity</param>
        /// <param name="operation">İşlem tipi</param>
        /// <param name="idPropertyName">ID özelliğinin adı (varsayılan: ID)</param>
        public static void LogEntityOperation<T>(this ILogger logger, 
            T entity, 
            string operation, 
            string idPropertyName = "ID") where T : class
        {
            if (entity == null)
            {
                logger.LogWarning($"{typeof(T).Name} {operation} işlemi: Entity null");
                return;
            }

            try
            {
                var idProperty = typeof(T).GetProperty(idPropertyName) ?? 
                                 typeof(T).GetProperty($"{typeof(T).Name}{idPropertyName}");
                
                var id = idProperty?.GetValue(entity) ?? "Bilinmiyor";
                logger.LogInformation($"{typeof(T).Name} {operation} işlemi tamamlandı: ID={id}");
            }
            catch (Exception ex)
            {
                logger.LogWarning($"{typeof(T).Name} {operation} işlemi loglanamadı: {ex.Message}");
            }
        }

        /// <summary>
        /// Belirli bir işlemin başlangıcını ve parametrelerini loglar
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="operationName">İşlem adı</param>
        /// <param name="parameters">İşlem parametreleri</param>
        public static void LogOperationStart(this ILogger logger, 
            string operationName, 
            params object[] parameters)
        {
            logger.LogInformation(
                $"{operationName} işlemi başlatıldı. " +
                (parameters != null && parameters.Length > 0 
                    ? $"Parametreler: {string.Join(", ", parameters)}" 
                    : "Parametre yok")
            );
        }

        /// <summary>
        /// Belirli bir işlemin sonucunu loglar
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="operationName">İşlem adı</param>
        /// <param name="result">İşlem sonucu (isteğe bağlı)</param>
        public static void LogOperationEnd(this ILogger logger, 
            string operationName, 
            object result = null)
        {
            logger.LogInformation(
                $"{operationName} işlemi tamamlandı. " +
                (result != null ? $"Sonuç: {result}" : "")
            );
        }

        /// <summary>
        /// Detaylı bir hata mesajı loglar
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="ex">Exception</param>
        /// <param name="context">Hata bağlamı</param>
        /// <param name="additionalInfo">Ek bilgi</param>
        public static void LogDetailedError(this ILogger logger, 
            Exception ex, 
            string context, 
            object additionalInfo = null)
        {
            var additionalInfoText = additionalInfo != null 
                ? $", Ek Bilgi: {additionalInfo}" 
                : "";
            
            logger.LogError(ex, 
                $"Hata Bağlamı: {context}, Hata: {ex.Message}, " +
                $"İç Hata: {ex.InnerException?.Message}{additionalInfoText}"
            );
        }
    }
} 
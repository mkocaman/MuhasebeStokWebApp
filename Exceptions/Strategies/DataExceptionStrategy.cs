using System;
using Microsoft.AspNetCore.Http;
using MuhasebeStokWebApp.Services.CustomExceptions;

namespace MuhasebeStokWebApp.Exceptions.Strategies
{
    /// <summary>
    /// DataException için strateji
    /// </summary>
    public class DataExceptionStrategy : IExceptionStrategy
    {
        /// <summary>
        /// DataException tipini işleyebilir
        /// </summary>
        public bool CanHandle(Exception exception)
        {
            return exception is DataException;
        }

        /// <summary>
        /// DataException için kullanıcı dostu mesaj
        /// </summary>
        public string GetUserFriendlyMessage(Exception exception)
        {
            if (exception is DataException dataException)
            {
                return dataException.ErrorType switch
                {
                    DataErrorType.Connection => "Veritabanı bağlantısı kurulamadı. Lütfen daha sonra tekrar deneyiniz.",
                    DataErrorType.Read => "Veri okuma hatası. İstediğiniz bilgilere erişilemiyor.",
                    DataErrorType.Insert => "Veri ekleme işlemi sırasında bir hata oluştu.",
                    DataErrorType.Update => "Veri güncelleme işlemi sırasında bir hata oluştu.",
                    DataErrorType.Delete => "Veri silme işlemi sırasında bir hata oluştu.",
                    DataErrorType.UniqueConstraint => "Bu kayıt zaten sistemde mevcut. Lütfen benzersiz bir değer girin.",
                    DataErrorType.ForeignKeyConstraint => "Bu kaydı silemezsiniz çünkü başka kayıtlar tarafından kullanılıyor.",
                    DataErrorType.Transaction => "İşlem tamamlanamadı. Lütfen daha sonra tekrar deneyiniz.",
                    DataErrorType.Concurrency => "Bu kayıt başka bir kullanıcı tarafından değiştirilmiş. Lütfen sayfayı yenileyip tekrar deneyiniz.",
                    DataErrorType.Timeout => "Veritabanı işlemi zaman aşımına uğradı. Lütfen daha sonra tekrar deneyiniz.",
                    _ => dataException.Message
                };
            }
            
            return "Veritabanı işlemi sırasında bir hata oluştu.";
        }

        /// <summary>
        /// DataException için durum kodu
        /// </summary>
        public int GetStatusCode(Exception exception)
        {
            if (exception is DataException dataException)
            {
                return dataException.ErrorType switch
                {
                    DataErrorType.Connection => StatusCodes.Status503ServiceUnavailable,
                    DataErrorType.Read => StatusCodes.Status404NotFound,
                    DataErrorType.Timeout => StatusCodes.Status504GatewayTimeout,
                    DataErrorType.Concurrency => StatusCodes.Status409Conflict,
                    DataErrorType.UniqueConstraint => StatusCodes.Status409Conflict,
                    DataErrorType.ForeignKeyConstraint => StatusCodes.Status409Conflict,
                    _ => StatusCodes.Status500InternalServerError
                };
            }
            
            return StatusCodes.Status500InternalServerError;
        }
    }
} 
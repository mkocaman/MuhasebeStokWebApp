using System;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace MuhasebeStokWebApp.Exceptions.Strategies
{
    /// <summary>
    /// Entity Framework (DbUpdateException, DbUpdateConcurrencyException) için strateji
    /// </summary>
    public class DbExceptionStrategy : IExceptionStrategy
    {
        /// <summary>
        /// Entity Framework exception tiplerini işleyebilir
        /// </summary>
        public bool CanHandle(Exception exception)
        {
            return exception is DbUpdateException || exception is DbUpdateConcurrencyException;
        }

        /// <summary>
        /// Entity Framework exception tipleri için kullanıcı dostu mesaj
        /// </summary>
        public string GetUserFriendlyMessage(Exception exception)
        {
            return exception switch
            {
                DbUpdateConcurrencyException => "Veri güncelleme sırasında beklenmeyen bir çakışma oluştu. Lütfen sayfayı yenileyip tekrar deneyin.",
                DbUpdateException ex when ex.InnerException?.Message.Contains("duplicate") == true => "Eklemeye çalıştığınız kayıt zaten sistemde mevcut.",
                DbUpdateException ex when ex.InnerException?.Message.Contains("foreign key") == true => "Bu kaydı silemezsiniz çünkü başka kayıtlar tarafından kullanılıyor.",
                DbUpdateException => "Veritabanı işlemi sırasında bir hata oluştu. Girdiğiniz veriler doğru formatta mı kontrol edin.",
                _ => "Veritabanı işlemi sırasında beklenmeyen bir hata oluştu."
            };
        }

        /// <summary>
        /// Entity Framework exception tipleri için durum kodu
        /// </summary>
        public int GetStatusCode(Exception exception)
        {
            return exception switch
            {
                DbUpdateConcurrencyException => StatusCodes.Status409Conflict,
                DbUpdateException ex when ex.InnerException?.Message.Contains("duplicate") == true => StatusCodes.Status409Conflict,
                DbUpdateException ex when ex.InnerException?.Message.Contains("foreign key") == true => StatusCodes.Status409Conflict,
                _ => StatusCodes.Status500InternalServerError
            };
        }
    }
} 
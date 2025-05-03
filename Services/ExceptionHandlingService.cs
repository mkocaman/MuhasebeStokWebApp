using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using MuhasebeStokWebApp.Data;
using MuhasebeStokWebApp.Services.Interfaces;

namespace MuhasebeStokWebApp.Services
{
    /// <summary>
    /// Exception handling işlemleri için servis implementasyonu
    /// </summary>
    public class ExceptionHandlingService : IExceptionHandlingService
    {
        private readonly ILogger<ExceptionHandlingService> _logger;
        
        // Yaygın hatalar için hata mesajları
        private readonly Dictionary<Type, string> _exceptionMessages = new Dictionary<Type, string>
        {
            { typeof(DbUpdateConcurrencyException), "Veri güncelleme sırasında beklenmeyen bir çakışma oluştu. Lütfen sayfayı yenileyip tekrar deneyin." },
            { typeof(DbUpdateException), "Veritabanı işlemi sırasında bir hata oluştu. Girdiğiniz veriler doğru formatta mı kontrol edin." },
            { typeof(SqlException), "Veritabanı bağlantısında bir sorun oluştu. Lütfen daha sonra tekrar deneyin." },
            { typeof(TimeoutException), "İşlem zaman aşımına uğradı. Lütfen daha sonra tekrar deneyin." },
            { typeof(UnauthorizedAccessException), "Bu işlemi gerçekleştirmek için gereken yetkiye sahip değilsiniz." },
            { typeof(ArgumentException), "Geçersiz bir değer girildi. Lütfen girdiğiniz değerleri kontrol edin." },
            { typeof(InvalidOperationException), "İşlem şu anda gerçekleştirilemiyor. Lütfen sistem yöneticinize başvurun." },
            { typeof(FormatException), "Geçersiz format. Lütfen girdiğiniz verinin formatını kontrol edin." },
            { typeof(OverflowException), "Çok büyük veya çok küçük bir değer girdiniz." },
            { typeof(NullReferenceException), "İşlem sırasında bir referans hatası oluştu. Lütfen sistem yöneticinize başvurun." },
            { typeof(DivideByZeroException), "Sıfıra bölme hatası. Lütfen hesaplamalarınızı kontrol edin." },
            { typeof(DbException), "Veritabanı işlemi sırasında bir hata oluştu. Lütfen daha sonra tekrar deneyin." },
        };
        
        // Transaction'ı iptal etmesi gereken hata tipleri
        private readonly HashSet<Type> _rollbackExceptionTypes = new HashSet<Type>
        {
            typeof(DbUpdateConcurrencyException),
            typeof(DbUpdateException),
            typeof(SqlException),
            typeof(DbException),
            typeof(InvalidOperationException)
        };
        
        public ExceptionHandlingService(ILogger<ExceptionHandlingService> logger)
        {
            _logger = logger;
        }
        
        /// <inheritdoc/>
        public string GetUserFriendlyErrorMessage(Exception exception)
        {
            // Önce iç içe hataları kontrol et
            if (exception.InnerException != null)
            {
                // Veritabanı ile ilgili hatalar genellikle InnerException'da bulunur
                if (exception.InnerException is SqlException sqlEx)
                {
                    return GetSqlExceptionMessage(sqlEx);
                }
                
                // DbUpdateException'ın içindeki asıl hatayı bul
                if (exception is DbUpdateException && _exceptionMessages.ContainsKey(exception.InnerException.GetType()))
                {
                    return _exceptionMessages[exception.InnerException.GetType()];
                }
            }
            
            // Hata tipine göre özel mesaj
            foreach (var exType in _exceptionMessages.Keys)
            {
                if (exType.IsInstanceOfType(exception))
                {
                    return _exceptionMessages[exType];
                }
            }
            
            // SqlException için özel mesajlar
            if (exception is SqlException sqlException)
            {
                return GetSqlExceptionMessage(sqlException);
            }
            
            // Genel hata mesajı
            return "İşlem sırasında beklenmeyen bir hata oluştu. Lütfen daha sonra tekrar deneyin.";
        }
        
        /// <inheritdoc/>
        public string HandleException(Exception exception, string errorContext)
        {
            string userFriendlyMessage = GetUserFriendlyErrorMessage(exception);
            
            // Loglama seviyesini hatanın ciddiyetine göre belirle
            if (IsDataConcurrencyException(exception) || IsTransientException(exception))
            {
                _logger.LogWarning(exception, $"{errorContext}: {userFriendlyMessage}");
            }
            else if (IsAccessViolationException(exception))
            {
                _logger.LogWarning(exception, $"Yetkisiz erişim: {errorContext}: {userFriendlyMessage}");
            }
            else
            {
                _logger.LogError(exception, $"{errorContext}: {userFriendlyMessage}");
            }
            
            return userFriendlyMessage;
        }
        
        /// <inheritdoc/>
        public bool ShouldRollbackTransaction(Exception exception)
        {
            // Exception tipine göre transaction'ı iptal etme kararını ver
            foreach (var exceptionType in _rollbackExceptionTypes)
            {
                if (exceptionType.IsInstanceOfType(exception))
                {
                    return true;
                }
            }
            
            // İç içe exception'larda da kontrol et
            if (exception.InnerException != null)
            {
                foreach (var exceptionType in _rollbackExceptionTypes)
                {
                    if (exceptionType.IsInstanceOfType(exception.InnerException))
                    {
                        return true;
                    }
                }
            }
            
            return false;
        }
        
        // SQL hata kodlarına göre özel mesajlar
        private string GetSqlExceptionMessage(SqlException sqlEx)
        {
            return sqlEx.Number switch
            {
                2627 => "Bu kayıt zaten sistemde mevcut (Tekrarlanan kayıt).",
                547 => "Bu kayıt başka tablolarda kullanılıyor olabilir ve silinemez.",
                2601 => "Bu kayıt zaten sistemde mevcut (Tekrarlanan indeks).",
                8114 => "Veri tipi uyumsuzluğu. Lütfen girdiğiniz değerleri kontrol edin.",
                201 => "Eksik bir parametre gönderildi. Lütfen tüm alanları doldurun.",
                207 => "Geçersiz bir kolon adı belirtildi. Lütfen sistem yöneticinize başvurun.",
                208 => "Geçersiz bir tablo adı belirtildi. Lütfen sistem yöneticinize başvurun.",
                515 => "Zorunlu bir alanı boş bıraktınız. Lütfen tüm zorunlu alanları doldurun.",
                4060 => "Veritabanına erişim sağlanamadı. Lütfen sistem yöneticinize başvurun.",
                18456 => "Veritabanı kimlik doğrulama hatası. Lütfen sistem yöneticinize başvurun.",
                10054 => "Veritabanı bağlantısı beklenmedik şekilde kapatıldı. Lütfen tekrar deneyin.",
                53 => "Veritabanı sunucusuna bağlanılamadı. Lütfen bağlantınızı kontrol edin.",
                _ => "Veritabanı işlemi sırasında bir hata oluştu. Lütfen daha sonra tekrar deneyin."
            };
        }
        
        // Veri tutarlılığı hataları
        private bool IsDataConcurrencyException(Exception ex)
        {
            return ex is DbUpdateConcurrencyException || 
                   (ex is SqlException sqlEx && (sqlEx.Number == 2627 || sqlEx.Number == 2601));
        }
        
        // Geçici hatalar
        private bool IsTransientException(Exception ex)
        {
            return ex is TimeoutException ||
                   (ex is SqlException sqlEx && (sqlEx.Number == 10054 || sqlEx.Number == 53 || sqlEx.Number == -2));
        }
        
        // Yetki hataları
        private bool IsAccessViolationException(Exception ex)
        {
            return ex is UnauthorizedAccessException ||
                   (ex is SqlException sqlEx && sqlEx.Number == 18456);
        }
    }
} 
using System;
using System.Collections.Generic;

namespace MuhasebeStokWebApp.Services.CustomExceptions
{
    /// <summary>
    /// Veri doğrulama hatalarını temsil eden özel exception sınıfı
    /// </summary>
    public class ValidationException : BusinessException
    {
        /// <summary>
        /// Doğrulama hataları listesi
        /// </summary>
        public List<ValidationError> ValidationErrors { get; }
        
        /// <summary>
        /// Yeni bir ValidationException oluşturur
        /// </summary>
        public ValidationException(string message) 
            : base(message, "VAL-1000", "Doğrulama Hatası", ErrorSeverity.Warning)
        {
            ValidationErrors = new List<ValidationError>();
        }
        
        /// <summary>
        /// Yeni bir ValidationException oluşturur
        /// </summary>
        public ValidationException(string message, List<ValidationError> validationErrors) 
            : base(message, "VAL-1000", "Doğrulama Hatası", ErrorSeverity.Warning)
        {
            ValidationErrors = validationErrors ?? new List<ValidationError>();
        }
        
        /// <summary>
        /// Yeni bir ValidationException oluşturur
        /// </summary>
        public ValidationException(string message, string errorCode) 
            : base(message, errorCode, "Doğrulama Hatası", ErrorSeverity.Warning)
        {
            ValidationErrors = new List<ValidationError>();
        }
        
        /// <summary>
        /// Yeni bir ValidationException oluşturur
        /// </summary>
        public ValidationException(string message, string errorCode, List<ValidationError> validationErrors) 
            : base(message, errorCode, "Doğrulama Hatası", ErrorSeverity.Warning)
        {
            ValidationErrors = validationErrors ?? new List<ValidationError>();
        }
        
        /// <summary>
        /// Doğrulama hatası ekler
        /// </summary>
        public void AddValidationError(string propertyName, string errorMessage)
        {
            ValidationErrors.Add(new ValidationError(propertyName, errorMessage));
        }
        
        /// <summary>
        /// Doğrulama hatası ekler
        /// </summary>
        public void AddValidationError(ValidationError validationError)
        {
            ValidationErrors.Add(validationError);
        }
    }
    
    /// <summary>
    /// Doğrulama hatası bilgilerini temsil eden sınıf
    /// </summary>
    public class ValidationError
    {
        /// <summary>
        /// Hata oluşan özellik adı
        /// </summary>
        public string PropertyName { get; }
        
        /// <summary>
        /// Hata mesajı
        /// </summary>
        public string ErrorMessage { get; }
        
        /// <summary>
        /// Yeni bir ValidationError oluşturur
        /// </summary>
        public ValidationError(string propertyName, string errorMessage)
        {
            PropertyName = propertyName;
            ErrorMessage = errorMessage;
        }
    }
} 
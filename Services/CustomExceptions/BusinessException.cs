using System;

namespace MuhasebeStokWebApp.Services.CustomExceptions
{
    /// <summary>
    /// İş mantığı hatalarını temsil eden özel exception sınıfı
    /// </summary>
    public class BusinessException : Exception
    {
        /// <summary>
        /// Hata kodu
        /// </summary>
        public string ErrorCode { get; }
        
        /// <summary>
        /// Hata önem seviyesi
        /// </summary>
        public ErrorSeverity Severity { get; }
        
        /// <summary>
        /// Mesaj title'ı
        /// </summary>
        public string Title { get; }
        
        /// <summary>
        /// Yeni bir BusinessException oluşturur
        /// </summary>
        public BusinessException(string message) 
            : base(message)
        {
            ErrorCode = "BUS-1000";
            Severity = ErrorSeverity.Warning;
            Title = "İşlem Hatası";
        }
        
        /// <summary>
        /// Yeni bir BusinessException oluşturur
        /// </summary>
        public BusinessException(string message, string errorCode) 
            : base(message)
        {
            ErrorCode = errorCode;
            Severity = ErrorSeverity.Warning;
            Title = "İşlem Hatası";
        }
        
        /// <summary>
        /// Yeni bir BusinessException oluşturur
        /// </summary>
        public BusinessException(string message, string errorCode, string title) 
            : base(message)
        {
            ErrorCode = errorCode;
            Severity = ErrorSeverity.Warning;
            Title = title;
        }
        
        /// <summary>
        /// Yeni bir BusinessException oluşturur
        /// </summary>
        public BusinessException(string message, string errorCode, string title, ErrorSeverity severity) 
            : base(message)
        {
            ErrorCode = errorCode;
            Severity = severity;
            Title = title;
        }
        
        /// <summary>
        /// Yeni bir BusinessException oluşturur
        /// </summary>
        public BusinessException(string message, string errorCode, string title, ErrorSeverity severity, Exception innerException) 
            : base(message, innerException)
        {
            ErrorCode = errorCode;
            Severity = severity;
            Title = title;
        }
    }
    
    /// <summary>
    /// Hata önem seviyesi
    /// </summary>
    public enum ErrorSeverity
    {
        /// <summary>
        /// Bilgilendirme seviyesindeki hatalar
        /// </summary>
        Info = 0,
        
        /// <summary>
        /// Uyarı seviyesindeki hatalar
        /// </summary>
        Warning = 1,
        
        /// <summary>
        /// Kritik hatalar
        /// </summary>
        Error = 2,
        
        /// <summary>
        /// Acil müdahale gerektiren kritik hatalar
        /// </summary>
        Critical = 3
    }
} 
using System;

namespace MuhasebeStokWebApp.Services.CustomExceptions
{
    /// <summary>
    /// Veritabanı hatalarını temsil eden özel exception sınıfı
    /// </summary>
    public class DataException : BusinessException
    {
        /// <summary>
        /// Etkilenen tablonun adı
        /// </summary>
        public string TableName { get; }
        
        /// <summary>
        /// Veritabanı hatası tipi
        /// </summary>
        public DataErrorType ErrorType { get; }
        
        /// <summary>
        /// Veritabanı işlemi
        /// </summary>
        public string Operation { get; }
        
        /// <summary>
        /// Yeni bir DataException oluşturur
        /// </summary>
        public DataException(string message) 
            : base(message, "DB-1000", "Veritabanı Hatası", ErrorSeverity.Error)
        {
            TableName = string.Empty;
            ErrorType = DataErrorType.General;
            Operation = string.Empty;
        }
        
        /// <summary>
        /// Yeni bir DataException oluşturur
        /// </summary>
        public DataException(string message, DataErrorType errorType) 
            : base(message, $"DB-{(int)errorType}", "Veritabanı Hatası", ErrorSeverity.Error)
        {
            TableName = string.Empty;
            ErrorType = errorType;
            Operation = string.Empty;
        }
        
        /// <summary>
        /// Yeni bir DataException oluşturur
        /// </summary>
        public DataException(string message, string tableName, DataErrorType errorType, string operation) 
            : base(message, $"DB-{(int)errorType}", "Veritabanı Hatası", ErrorSeverity.Error)
        {
            TableName = tableName;
            ErrorType = errorType;
            Operation = operation;
        }
        
        /// <summary>
        /// Yeni bir DataException oluşturur
        /// </summary>
        public DataException(string message, string tableName, DataErrorType errorType, string operation, Exception innerException) 
            : base(message, $"DB-{(int)errorType}", "Veritabanı Hatası", ErrorSeverity.Error, innerException)
        {
            TableName = tableName;
            ErrorType = errorType;
            Operation = operation;
        }
        
        /// <summary>
        /// Yeni bir DataException oluşturur (özel hata kodlu)
        /// </summary>
        public DataException(string message, string errorCode, string tableName, DataErrorType errorType, string operation) 
            : base(message, errorCode, "Veritabanı Hatası", ErrorSeverity.Error)
        {
            TableName = tableName;
            ErrorType = errorType;
            Operation = operation;
        }
    }
    
    /// <summary>
    /// Veritabanı hatası tipleri
    /// </summary>
    public enum DataErrorType
    {
        /// <summary>
        /// Genel veritabanı hatası
        /// </summary>
        General = 1000,
        
        /// <summary>
        /// Bağlantı hatası
        /// </summary>
        Connection = 1001,
        
        /// <summary>
        /// Veri çekme hatası
        /// </summary>
        Read = 1002,
        
        /// <summary>
        /// Veri ekleme hatası
        /// </summary>
        Insert = 1003,
        
        /// <summary>
        /// Veri güncelleme hatası
        /// </summary>
        Update = 1004,
        
        /// <summary>
        /// Veri silme hatası
        /// </summary>
        Delete = 1005,
        
        /// <summary>
        /// Benzersiz kısıt hatası (Unique constraint)
        /// </summary>
        UniqueConstraint = 1006,
        
        /// <summary>
        /// Yabancı anahtar kısıt hatası (Foreign key constraint)
        /// </summary>
        ForeignKeyConstraint = 1007,
        
        /// <summary>
        /// İşlem (Transaction) hatası
        /// </summary>
        Transaction = 1008,
        
        /// <summary>
        /// Eşzamanlılık hatası (Concurrency)
        /// </summary>
        Concurrency = 1009,
        
        /// <summary>
        /// Zaman aşımı hatası (Timeout)
        /// </summary>
        Timeout = 1010
    }
} 
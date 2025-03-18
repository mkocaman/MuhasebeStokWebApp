using System;

namespace MuhasebeStokWebApp.Exceptions
{
    /// <summary>
    /// Döviz kuru işlemlerinde oluşan hataları temsil eden özel istisna sınıfı
    /// </summary>
    public class DovizKuruException : Exception
    {
        public DovizKuruException() : base() { }
        
        public DovizKuruException(string message) : base(message) { }
        
        public DovizKuruException(string message, Exception innerException) 
            : base(message, innerException) { }
    }
} 
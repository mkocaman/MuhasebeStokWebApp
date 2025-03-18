using System;

namespace MuhasebeStokWebApp.Exceptions
{
    public class DovizKuruBulunamadiException : Exception
    {
        public DovizKuruBulunamadiException() : base("Döviz kuru bulunamadı.")
        {
        }

        public DovizKuruBulunamadiException(string message) : base(message)
        {
        }

        public DovizKuruBulunamadiException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
} 
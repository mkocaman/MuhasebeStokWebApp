using System;

namespace MuhasebeStokWebApp.Data.Entities
{
    // Tüm entity'ler için temel sınıf
    public abstract class BaseEntity
    {
        public DateTime OlusturmaTarihi { get; set; } = DateTime.Now;
        public DateTime? GuncellemeTarihi { get; set; }
    }
} 
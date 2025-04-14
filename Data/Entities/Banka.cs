using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MuhasebeStokWebApp.Data.Entities
{
    public class Banka
    {
        [Key]
        public Guid BankaID { get; set; }
        
        [Required]
        [StringLength(100)]
        public string BankaAdi { get; set; }
        
        public bool Aktif { get; set; } = true;
        
        public Guid? OlusturanKullaniciID { get; set; }
        
        public Guid? SonGuncelleyenKullaniciID { get; set; }
        
        public DateTime OlusturmaTarihi { get; set; } = DateTime.Now;
        
        public DateTime? GuncellemeTarihi { get; set; }
        
        public bool Silindi { get; set; } = false;
        
        // Navigation properties
        public virtual ICollection<BankaHesap> BankaHesaplari { get; set; }
        
        public Banka()
        {
            BankaHesaplari = new HashSet<BankaHesap>();
        }
    }
} 
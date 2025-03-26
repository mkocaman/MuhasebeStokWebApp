using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MuhasebeStokWebApp.Data.Entities
{
    public class Depo
    {
        [Key]
        public Guid DepoID { get; set; }
        
        [Required]
        [StringLength(100)]
        public string DepoAdi { get; set; }
        
        [StringLength(200)]
        public string Adres { get; set; }
        
        public bool Aktif { get; set; }
        
        public Guid? OlusturanKullaniciID { get; set; }
        
        public Guid? SonGuncelleyenKullaniciID { get; set; }
        
        public DateTime? OlusturmaTarihi { get; set; }
        
        public DateTime? GuncellemeTarihi { get; set; }
        
        public bool Silindi { get; set; }
        
        // Navigation properties
        public virtual ICollection<StokHareket> StokHareketleri { get; set; }
        
        public Depo()
        {
            StokHareketleri = new HashSet<StokHareket>();
        }
    }
} 
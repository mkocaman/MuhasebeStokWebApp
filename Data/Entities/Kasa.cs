using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MuhasebeStokWebApp.Data.Entities
{
    public class Kasa
    {
        [Key]
        public Guid KasaID { get; set; }
        
        [Required]
        [StringLength(100)]
        public string KasaAdi { get; set; }
        
        [StringLength(50)]
        public string KasaTuru { get; set; }
        
        [Required]
        [StringLength(3)]
        public string ParaBirimi { get; set; } = "TRY"; // Varsayılan TL
        
        [Required]
        public decimal AcilisBakiye { get; set; } = 0;
        
        [Required]
        public decimal GuncelBakiye { get; set; } = 0;
        
        [StringLength(500)]
        public string Aciklama { get; set; }
        
        public bool Aktif { get; set; } = true;
        
        public Guid? SorumluKullaniciID { get; set; }
        
        public Guid? OlusturanKullaniciID { get; set; }
        
        public Guid? SonGuncelleyenKullaniciID { get; set; }
        
        public DateTime OlusturmaTarihi { get; set; } = DateTime.Now;
        
        public DateTime? GuncellemeTarihi { get; set; }
        
        public bool Silindi { get; set; } = false;
        
        // Navigation properties
        public virtual ICollection<KasaHareket> KasaHareketleri { get; set; }
        
        public Kasa()
        {
            KasaHareketleri = new HashSet<KasaHareket>();
        }
    }
} 
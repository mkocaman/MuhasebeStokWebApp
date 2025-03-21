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
        
        [StringLength(100)]
        public string SubeAdi { get; set; }
        
        [StringLength(50)]
        public string SubeKodu { get; set; }
        
        [StringLength(50)]
        public string HesapNo { get; set; }
        
        [StringLength(50)]
        public string IBAN { get; set; }
        
        [StringLength(10)]
        public string ParaBirimi { get; set; } = "TRY";
        
        [Required]
        public decimal AcilisBakiye { get; set; } = 0;
        
        [Required]
        public decimal GuncelBakiye { get; set; } = 0;
        
        [StringLength(500)]
        public string Aciklama { get; set; }
        
        public bool Aktif { get; set; } = true;
        
        public Guid? YetkiliKullaniciID { get; set; }
        
        public Guid? OlusturanKullaniciID { get; set; }
        
        public Guid? SonGuncelleyenKullaniciID { get; set; }
        
        public DateTime OlusturmaTarihi { get; set; } = DateTime.Now;
        
        public DateTime? GuncellemeTarihi { get; set; }
        
        public bool SoftDelete { get; set; } = false;
        
        // Navigation properties
        public virtual ICollection<BankaHareket> BankaHareketleri { get; set; }
        
        public Banka()
        {
            BankaHareketleri = new HashSet<BankaHareket>();
        }
    }
} 
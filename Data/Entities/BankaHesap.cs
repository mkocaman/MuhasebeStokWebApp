using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MuhasebeStokWebApp.Data.Entities
{
    /// <summary>
    /// Banka hesabı bilgilerini tutan sınıf.
    /// Her banka birden fazla hesaba sahip olabilir.
    /// </summary>
    [Table("BankaHesaplari")]
    public class BankaHesap
    {
        [Key]
        public Guid BankaHesapID { get; set; }
        
        [Required]
        public Guid BankaID { get; set; }
        
        [Required]
        [StringLength(100)]
        public string HesapAdi { get; set; }
        
        [StringLength(50)]
        public string HesapNo { get; set; }
        
        [StringLength(50)]
        public string IBAN { get; set; }
        
        [StringLength(100)]
        public string SubeAdi { get; set; }
        
        [StringLength(50)]
        public string SubeKodu { get; set; }
        
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
        
        public bool Silindi { get; set; } = false;
        
        // Navigation properties
        [ForeignKey("BankaID")]
        public virtual Banka Banka { get; set; }
        
        public virtual ICollection<BankaHesapHareket> BankaHesapHareketleri { get; set; }
        
        public BankaHesap()
        {
            BankaHesapHareketleri = new HashSet<BankaHesapHareket>();
        }
    }
} 
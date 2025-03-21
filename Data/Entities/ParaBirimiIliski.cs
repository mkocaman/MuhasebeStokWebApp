using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MuhasebeStokWebApp.Data.Entities
{
    /// <summary>
    /// Para birimleri arası dönüşüm ilişkisi tablosu
    /// </summary>
    [Table("ParaBirimiIliski")]
    public class ParaBirimiIliski
    {
        public ParaBirimiIliski()
        {
            ParaBirimiIliskiID = Guid.NewGuid();
            Aktif = true;
            Silindi = false;
            OlusturmaTarihi = DateTime.Now;
            GuncellemeTarihi = DateTime.Now;
        }

        [Key]
        public Guid ParaBirimiIliskiID { get; set; }
        
        [Required]
        public Guid KaynakParaBirimiID { get; set; }
        
        [Required]
        public Guid HedefParaBirimiID { get; set; }
        
        [Required]
        [Range(0.001, 10000)]
        [Display(Name = "Dönüşüm Çarpanı")]
        public decimal Carpan { get; set; } = 1.0m;
        
        [Display(Name = "Aktif")]
        public bool Aktif { get; set; } = true;
        
        [Display(Name = "Oluşturma Tarihi")]
        public DateTime OlusturmaTarihi { get; set; }
        
        [Display(Name = "Güncelleme Tarihi")]
        public DateTime GuncellemeTarihi { get; set; }
        
        [Display(Name = "Silindi")]
        public bool Silindi { get; set; } = false;
        
        // İlişkiler
        [ForeignKey("KaynakParaBirimiID")]
        public virtual ParaBirimi KaynakParaBirimi { get; set; } = null!;
        
        [ForeignKey("HedefParaBirimiID")]
        public virtual ParaBirimi HedefParaBirimi { get; set; } = null!;
    }
} 
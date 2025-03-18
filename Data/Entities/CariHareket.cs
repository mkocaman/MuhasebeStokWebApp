using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MuhasebeStokWebApp.Data.Entities
{
    public class CariHareket
    {
        [Key]
        public Guid CariHareketID { get; set; }
        
        public Guid CariID { get; set; }
        
        [Required]
        public decimal Tutar { get; set; }
        
        [Required]
        [StringLength(50)]
        public string HareketTuru { get; set; } = string.Empty;
        
        [Required]
        public DateTime Tarih { get; set; }
        
        [StringLength(50)]
        public string? ReferansNo { get; set; }
        
        [StringLength(50)]
        public string? ReferansTuru { get; set; }
        
        public Guid? ReferansID { get; set; }
        
        [StringLength(500)]
        public string? Aciklama { get; set; }
        
        public Guid? IslemYapanKullaniciID { get; set; }
        
        public Guid? SonGuncelleyenKullaniciID { get; set; }
        
        public DateTime OlusturmaTarihi { get; set; } = DateTime.Now;
        
        public DateTime? GuncellemeTarihi { get; set; }
        
        public bool SoftDelete { get; set; }
        
        // Navigation properties
        [ForeignKey("CariID")]
        public virtual Cari? Cari { get; set; }
    }
} 
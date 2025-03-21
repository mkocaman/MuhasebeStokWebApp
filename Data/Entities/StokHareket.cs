using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MuhasebeStokWebApp.Data.Entities
{
    public class StokHareket
    {
        [Key]
        public Guid StokHareketID { get; set; }
        
        public Guid UrunID { get; set; }
        
        public Guid? DepoID { get; set; }
        
        [Required]
        public decimal Miktar { get; set; }
        
        [StringLength(50)]
        public string Birim { get; set; } = string.Empty;
        
        [Required]
        public DateTime Tarih { get; set; } = DateTime.Now;
        
        [Required]
        [StringLength(50)]
        public string HareketTuru { get; set; } = string.Empty;
        
        [StringLength(50)]
        public string ReferansNo { get; set; } = string.Empty;
        
        [StringLength(50)]
        public string ReferansTuru { get; set; } = string.Empty;
        
        public Guid? ReferansID { get; set; }
        
        [StringLength(500)]
        public string? Aciklama { get; set; }
        
        public decimal? BirimFiyat { get; set; }
        
        public Guid? IslemYapanKullaniciID { get; set; }
        
        public Guid? SonGuncelleyenKullaniciID { get; set; }
        
        public DateTime? OlusturmaTarihi { get; set; }
        
        public DateTime? GuncellemeTarihi { get; set; }
        
        public bool SoftDelete { get; set; }
        
        public Guid? FaturaID { get; set; }
        
        public Guid? IrsaliyeID { get; set; }
        
        // Navigation properties
        [ForeignKey("UrunID")]
        public virtual Urun? Urun { get; set; }
        
        [ForeignKey("DepoID")]
        public virtual Depo? Depo { get; set; }
    }
} 
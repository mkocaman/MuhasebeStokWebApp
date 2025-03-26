using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MuhasebeStokWebApp.Data.Entities
{
    public class FaturaDetay
    {
        [Key]
        public Guid FaturaDetayID { get; set; }
        
        public Guid FaturaID { get; set; }
        
        public Guid UrunID { get; set; }
        
        [Required]
        public decimal Miktar { get; set; }
        
        [Required]
        public decimal BirimFiyat { get; set; }
        
        [Required]
        public decimal KdvOrani { get; set; }
        
        [Required]
        public decimal IndirimOrani { get; set; }
        
        [Required]
        public decimal? SatirToplam { get; set; }
        
        [Required]
        public decimal? SatirKdvToplam { get; set; }
        
        [StringLength(50)]
        public string? Birim { get; set; }
        
        [StringLength(200)]
        public string? Aciklama { get; set; }
        
        public Guid? OlusturanKullaniciID { get; set; }
        
        public Guid? SonGuncelleyenKullaniciID { get; set; }
        
        public DateTime OlusturmaTarihi { get; set; } = DateTime.Now;
        
        public DateTime? GuncellemeTarihi { get; set; }
        
        public bool Silindi { get; set; }
        
        public decimal? Tutar { get; set; }
        
        public decimal? KdvTutari { get; set; }
        
        public decimal? IndirimTutari { get; set; }
        
        public decimal? NetTutar { get; set; }
        
        // Navigation properties
        [ForeignKey("FaturaID")]
        public virtual Fatura? Fatura { get; set; }
        
        [ForeignKey("UrunID")]
        public virtual Urun? Urun { get; set; }
    }
} 
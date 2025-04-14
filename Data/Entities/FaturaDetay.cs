using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

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
        [Column(TypeName = "decimal(18,2)")]
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
        
        [StringLength(500)]
        public string? Aciklama { get; set; }
        
        public DateTime? OlusturmaTarihi { get; set; }
        
        public DateTime? GuncellemeTarihi { get; set; }
        
        public bool Silindi { get; set; } = false;
        
        public bool? Aktif { get; set; }
        
        public Guid? OlusturanKullaniciID { get; set; }
        
        public Guid? SonGuncelleyenKullaniciID { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal? Tutar { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal? KdvTutari { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal? IndirimTutari { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal? NetTutar { get; set; }
        
        // Dövizli değerler (seçilen para biriminde)
        [Column(TypeName = "decimal(18,2)")]
        public decimal BirimFiyatDoviz { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal? TutarDoviz { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal? KdvTutariDoviz { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal? IndirimTutariDoviz { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal? NetTutarDoviz { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal? AklananMiktar { get; set; }
        
        public bool AklanmaTamamlandi { get; set; } = false;
        
        // Navigation properties
        [ForeignKey("FaturaID")]
        public virtual Fatura? Fatura { get; set; }
        
        [ForeignKey("UrunID")]
        public virtual Urun? Urun { get; set; }
        
        public FaturaDetay()
        {
            FaturaDetayID = Guid.NewGuid();
            OlusturmaTarihi = DateTime.Now;
            Silindi = false;
            AklanmaTamamlandi = false;
            BirimFiyatDoviz = 0;
        }
    }
} 
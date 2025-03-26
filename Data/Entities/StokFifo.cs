using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MuhasebeStokWebApp.Data.Entities
{
    /// <summary>
    /// FIFO (First In, First Out) stok hareketlerini takip etmek için kullanılan sınıf.
    /// Bu sınıf, ürünlerin giriş ve çıkış hareketlerini FIFO prensibine göre yönetir.
    /// </summary>
    public class StokFifo
    {
        [Key]
        public Guid StokFifoID { get; set; }
        
        [Required]
        public Guid UrunID { get; set; }
        
        [Required]
        public decimal Miktar { get; set; }
        
        [Required]
        public decimal KalanMiktar { get; set; }
        
        [Required]
        public decimal BirimFiyat { get; set; }
        
        [Required]
        [StringLength(20)]
        public required string Birim { get; set; }
        
        [StringLength(3)]
        public required string ParaBirimi { get; set; } = "TRY";
        
        // USD'ye çevirmek için kullanılan kur
        public decimal DovizKuru { get; set; } = 1;
        
        // USD cinsinden birim fiyat (ana para birimi)
        [Required]
        public decimal USDBirimFiyat { get; set; }
        
        // TL cinsinden birim fiyat (raporlama için)
        public decimal TLBirimFiyat { get; set; }
        
        // UZS cinsinden birim fiyat (raporlama için)
        public decimal UZSBirimFiyat { get; set; }
        
        [Required]
        public DateTime GirisTarihi { get; set; }
        
        public DateTime? SonCikisTarihi { get; set; }
        
        [Required]
        [StringLength(50)]
        public required string ReferansNo { get; set; }
        
        [Required]
        [StringLength(20)]
        public required string ReferansTuru { get; set; }
        
        [Required]
        public Guid ReferansID { get; set; }
        
        [Required]
        [StringLength(500)]
        public string? Aciklama { get; set; }
        
        public bool Aktif { get; set; } = true;
        
        public bool Iptal { get; set; } = false;
        
        public DateTime? IptalTarihi { get; set; }
        
        [StringLength(500)]
        public string? IptalAciklama { get; set; }
        
        public Guid? IptalEdenKullaniciID { get; set; }
        
        [Required]
        public DateTime OlusturmaTarihi { get; set; } = DateTime.Now;
        
        public DateTime? GuncellemeTarihi { get; set; }
        
        [Required]
        public bool Silindi { get; set; } = false;
        
        // Navigation properties
        [ForeignKey("UrunID")]
        public virtual Urun? Urun { get; set; }
    }
} 
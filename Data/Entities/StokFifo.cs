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
        [ForeignKey("Urun")]
        public Guid UrunID { get; set; }
        
        [Required]
        public decimal Miktar { get; set; }
        
        [Required]
        public decimal KalanMiktar { get; set; }
        
        [Required]
        public decimal BirimFiyat { get; set; }
        
        [Required]
        [StringLength(20)]
        public string Birim { get; set; } = "";
        
        // Para birimi bilgisi
        [Required]
        [StringLength(10)]
        public string ParaBirimi { get; set; } = "TRY";
        
        // USD'ye çevirmek için kullanılan kur
        [Required]
        public decimal DovizKuru { get; set; } = 1;
        
        // Standardize edilmiş USD para birimi değeri - raporlama ve hesaplamalar için
        [Required]
        public decimal USDBirimFiyat { get; set; }

        // TL para birimi değeri - kaldırılacak (tüm hesaplamalar USD üzerinden yapılacak)
        // Migration ile bu alan kaldırılacak
        [Required]
        public decimal TLBirimFiyat { get; set; }
        
        // UZS para birimi değeri - kaldırılacak (tüm hesaplamalar USD üzerinden yapılacak)
        // Migration ile bu alan kaldırılacak
        [Required]
        public decimal UZSBirimFiyat { get; set; }
        
        [Required]
        public DateTime GirisTarihi { get; set; }
        
        public DateTime? SonCikisTarihi { get; set; }
        
        [Required]
        [StringLength(50)]
        public string ReferansNo { get; set; } = "";
        
        [Required]
        [StringLength(20)]
        public string ReferansTuru { get; set; } = "";
        
        [Required]
        public Guid ReferansID { get; set; }
        
        [StringLength(500)]
        public string? Aciklama { get; set; }
        
        [Required]
        public bool Aktif { get; set; } = true;
        
        [Required]
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
        
        // Concurrency kontrolü için RowVersion alanı
        [Timestamp]
        public byte[]? RowVersion { get; set; }
        
        // Navigation properties
        public virtual Urun Urun { get; set; } = null!;
    }
} 
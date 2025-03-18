using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MuhasebeStokWebApp.Data.Entities
{
    public class KasaHareket
    {
        [Key]
        public Guid KasaHareketID { get; set; }
        
        [Required]
        public Guid KasaID { get; set; }
        
        // Hareket kimden/kime yapıldı bilgisi
        public Guid? CariID { get; set; }
        
        // Banka transferleri için
        public Guid? KaynakBankaID { get; set; }
        
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Tutar { get; set; }
        
        [Required]
        [StringLength(20)]
        public string HareketTuru { get; set; } // Giriş, Çıkış
        
        // Transfer işlemleri için hedef
        public Guid? HedefKasaID { get; set; }
        public Guid? HedefBankaID { get; set; }
        [Required]
        [StringLength(20)]
        public string IslemTuru { get; set; } // Normal, KasaTransfer, BankaTransfer
        
        // Döviz işlemleri için
        [Column(TypeName = "decimal(18,6)")]
        public decimal DovizKuru { get; set; } = 1;
        [StringLength(3)]
        public string KarsiParaBirimi { get; set; }
        
        [Required]
        public DateTime Tarih { get; set; } = DateTime.Now;
        
        [StringLength(50)]
        public string ReferansNo { get; set; }
        
        [StringLength(50)]
        public string ReferansTuru { get; set; } // Fatura, Tahsilat, Ödeme, vs.
        
        public Guid? ReferansID { get; set; }
        
        [StringLength(500)]
        public string Aciklama { get; set; }
        
        public Guid? IslemYapanKullaniciID { get; set; }
        
        public Guid? SonGuncelleyenKullaniciID { get; set; }
        
        public DateTime OlusturmaTarihi { get; set; } = DateTime.Now;
        
        public DateTime? GuncellemeTarihi { get; set; }
        
        public bool SoftDelete { get; set; } = false;
        
        public Guid? TransferID { get; set; }
        
        // Navigation properties
        [ForeignKey("KasaID")]
        public virtual Kasa Kasa { get; set; }
        
        [ForeignKey("CariID")]
        public virtual Cari Cari { get; set; }
        
        [ForeignKey("HedefKasaID")]
        public virtual Kasa HedefKasa { get; set; }
        
        [ForeignKey("HedefBankaID")]
        public virtual Banka HedefBanka { get; set; }
        
        [ForeignKey("KaynakBankaID")]
        public virtual Banka KaynakBanka { get; set; }
    }
} 
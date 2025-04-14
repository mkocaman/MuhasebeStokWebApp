using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MuhasebeStokWebApp.Enums;

namespace MuhasebeStokWebApp.Data.Entities
{
    public class BankaHareket
    {
        [Key]
        public Guid BankaHareketID { get; set; }
        
        public Guid BankaID { get; set; }
        
        // Transfer işlemleri için
        public Guid? KaynakKasaID { get; set; }
        public Guid? HedefKasaID { get; set; }
        public Guid? TransferID { get; set; }
        
        // Cari işlemleri için
        public Guid? CariID { get; set; }
        
        [Required]
        public decimal Tutar { get; set; }
        
        [Required]
        [StringLength(50)]
        public string HareketTuru { get; set; } // Para Yatırma, Para Çekme, EFT, Havale, vs.
        
        [Required]
        public DateTime Tarih { get; set; } = DateTime.Now;
        
        [StringLength(50)]
        public string ReferansNo { get; set; }
        
        [StringLength(50)]
        public string ReferansTuru { get; set; } // Fatura, Tahsilat, Ödeme, vs.
        
        public Guid? ReferansID { get; set; }
        
        [StringLength(500)]
        public string Aciklama { get; set; }
        
        [StringLength(50)]
        public string DekontNo { get; set; }
        
        public Guid? IslemYapanKullaniciID { get; set; }
        
        public Guid? SonGuncelleyenKullaniciID { get; set; }
        
        public DateTime OlusturmaTarihi { get; set; } = DateTime.Now;
        
        public DateTime? GuncellemeTarihi { get; set; }
        
        public bool Silindi { get; set; } = false;
        
        // Karşı taraf bilgisi
        [StringLength(200)]
        public string KarsiUnvan { get; set; }
        
        [StringLength(50)]
        public string KarsiBankaAdi { get; set; }
        
        [StringLength(50)]
        public string KarsiIBAN { get; set; }
        
        // Navigation properties
        [ForeignKey("BankaID")]
        public virtual Banka Banka { get; set; }
        
        [ForeignKey("KaynakKasaID")]
        public virtual Kasa KaynakKasa { get; set; }
        
        [ForeignKey("HedefKasaID")]
        public virtual Kasa HedefKasa { get; set; }
        
        [ForeignKey("CariID")]
        public virtual Cari Cari { get; set; }
    }
} 
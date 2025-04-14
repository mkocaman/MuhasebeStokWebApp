using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MuhasebeStokWebApp.Data.Entities
{
    /// <summary>
    /// Stok çıkış detaylarını takip etmek için sınıf. 
    /// Hangi FIFO kaydından ne kadar çıkış yapıldığını takip eder.
    /// </summary>
    [Table("StokCikisDetaylari")]
    public class StokCikisDetay
    {
        [Key]
        public Guid StokCikisDetayID { get; set; }
        
        public Guid? StokFifoID { get; set; }
        
        [Required]
        [Column(TypeName = "decimal(18,6)")]
        public decimal CikisMiktari { get; set; }
        
        [Column(TypeName = "decimal(18,6)")]
        public decimal BirimFiyat { get; set; }
        
        [Column(TypeName = "decimal(18,6)")]
        public decimal BirimFiyatUSD { get; set; }
        
        [Column(TypeName = "decimal(18,6)")]
        public decimal BirimFiyatTL { get; set; }
        
        [Column(TypeName = "decimal(18,6)")]
        public decimal BirimFiyatUZS { get; set; }
        
        [Column(TypeName = "decimal(18,6)")]
        public decimal ToplamMaliyetUSD { get; set; }
        
        [StringLength(50)]
        public string ReferansNo { get; set; }
        
        [StringLength(50)]
        public string ReferansTuru { get; set; }
        
        public Guid ReferansID { get; set; }
        
        public DateTime CikisTarihi { get; set; }
        
        [StringLength(500)]
        public string Aciklama { get; set; }
        
        public DateTime OlusturmaTarihi { get; set; }
        
        public bool Iptal { get; set; } = false;
        
        public DateTime? IptalTarihi { get; set; }
        
        [StringLength(500)]
        public string IptalAciklama { get; set; }
        
        // Navigation property
        [ForeignKey("StokFifoID")]
        public virtual StokFifo StokFifo { get; set; }
    }
} 
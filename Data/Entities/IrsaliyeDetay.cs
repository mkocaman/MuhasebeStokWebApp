using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MuhasebeStokWebApp.Data.Entities
{
    public class IrsaliyeDetay
    {
        [Key]
        public Guid IrsaliyeDetayID { get; set; }
        
        public Guid IrsaliyeID { get; set; }
        
        public Guid UrunID { get; set; }
        
        [Required]
        public decimal Miktar { get; set; }
        
        [StringLength(50)]
        public string Birim { get; set; }
        
        [StringLength(200)]
        public string Aciklama { get; set; }
        
        public Guid? OlusturanKullaniciID { get; set; }
        
        public Guid? SonGuncelleyenKullaniciID { get; set; }
        
        public DateTime? OlusturmaTarihi { get; set; }
        
        public DateTime? GuncellemeTarihi { get; set; }
        
        public bool SoftDelete { get; set; }
        
        [Required]
        public decimal SatirToplam { get; set; }
        
        [Required]
        public decimal SatirKdvToplam { get; set; }
        
        // Navigation properties
        [ForeignKey("IrsaliyeID")]
        public virtual Irsaliye Irsaliye { get; set; }
        
        [ForeignKey("UrunID")]
        public virtual Urun Urun { get; set; }
    }
} 
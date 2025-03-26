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
        
        [Required]
        public decimal BirimFiyat { get; set; }
        
        [Required]
        public decimal KdvOrani { get; set; }
        
        [Required]
        public decimal IndirimOrani { get; set; }
        
        [StringLength(50)]
        public string Birim { get; set; }
        
        [StringLength(200)]
        public string Aciklama { get; set; }
        
        public Guid? OlusturanKullaniciId { get; set; }
        
        public Guid? SonGuncelleyenKullaniciId { get; set; }
        
        public DateTime? OlusturmaTarihi { get; set; }
        
        public DateTime? GuncellemeTarihi { get; set; }
        
        public bool Aktif { get; set; } = true;
        
        public bool SoftDelete { get; set; } = false;
        
        [Required]
        public decimal SatirToplam { get; set; }
        
        [Required]
        public decimal SatirKdvToplam { get; set; }
        
        // Navigation properties
        [ForeignKey("IrsaliyeID")]
        public virtual Irsaliye Irsaliye { get; set; }
        
        [ForeignKey("UrunID")]
        public virtual Urun Urun { get; set; }
        
        public IrsaliyeDetay()
        {
            IrsaliyeDetayID = Guid.NewGuid();
            OlusturmaTarihi = DateTime.Now;
            Aktif = true;
            SoftDelete = false;
        }
    }
} 
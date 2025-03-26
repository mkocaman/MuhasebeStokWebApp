using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MuhasebeStokWebApp.Data.Entities
{
    public class UrunFiyat
    {
        [Key]
        public Guid FiyatID { get; set; }
        
        public Guid? UrunID { get; set; }
        
        [Required]
        public decimal Fiyat { get; set; }
        
        [Required]
        public DateTime GecerliTarih { get; set; }
        
        public int? FiyatTipiID { get; set; }
        
        public Guid? OlusturanKullaniciID { get; set; }
        
        public Guid? SonGuncelleyenKullaniciID { get; set; }
        
        public DateTime? OlusturmaTarihi { get; set; }
        
        public DateTime? GuncellemeTarihi { get; set; }
        
        public bool Silindi { get; set; }
        
        // Navigation properties
        [ForeignKey("UrunID")]
        public virtual Urun? Urun { get; set; }
        
        [ForeignKey("FiyatTipiID")]
        public virtual FiyatTipi? FiyatTipi { get; set; }
        
        public UrunFiyat()
        {
            FiyatID = Guid.NewGuid();
        }
    }
} 
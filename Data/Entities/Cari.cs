using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MuhasebeStokWebApp.Data.Entities
{
    public class Cari : BaseEntity, ISoftDelete
    {
        [Key]
        public Guid CariID { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Ad { get; set; }
        
        [StringLength(50)]
        public string CariKodu { get; set; }
        
        [StringLength(50)]
        public string CariTipi { get; set; }
        
        [StringLength(20)]
        public string VergiNo { get; set; }
        
        [StringLength(100)]
        public string VergiDairesi { get; set; }
        
        [StringLength(20)]
        public string Telefon { get; set; }
        
        [StringLength(100)]
        public string Email { get; set; }
        
        public bool AktifMi { get; set; } = true;
        
        public Guid? OlusturanKullaniciId { get; set; }
        
        public Guid? SonGuncelleyenKullaniciId { get; set; }
        
        public DateTime OlusturmaTarihi { get; set; }
        
        public DateTime? GuncellemeTarihi { get; set; }
        
        public bool Silindi { get; set; } = false;
        
        [StringLength(200)]
        public string Adres { get; set; }
        
        [StringLength(50)]
        public string Il { get; set; }
        
        [StringLength(50)]
        public string Ilce { get; set; }
        
        [StringLength(20)]
        public string PostaKodu { get; set; }
        
        [StringLength(50)]
        public string Ulke { get; set; }
        
        [StringLength(100)]
        public string WebSitesi { get; set; }
        
        [StringLength(500)]
        public string Aciklama { get; set; }
        
        [StringLength(500)]
        public string Notlar { get; set; }
        
        [StringLength(50)]
        public string Yetkili { get; set; }
        
        public decimal BaslangicBakiye { get; set; }
        
        // Navigation properties
        public virtual ICollection<Fatura> Faturalar { get; set; } = new List<Fatura>();
        public virtual ICollection<Irsaliye> Irsaliyeler { get; set; } = new List<Irsaliye>();
        
        public Cari()
        {
            CariID = Guid.NewGuid();
            OlusturmaTarihi = DateTime.Now;
            AktifMi = true;
            Silindi = false;
        }
    }
} 
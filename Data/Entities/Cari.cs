using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MuhasebeStokWebApp.Data.Entities
{
    public class Cari
    {
        [Key]
        public Guid CariID { get; set; }
        
        [Required]
        [StringLength(200)]
        public string CariAdi { get; set; }
        
        [StringLength(50)]
        public string VergiNo { get; set; }
        
        [StringLength(20)]
        public string Telefon { get; set; }
        
        [StringLength(100)]
        public string Email { get; set; }
        
        public bool Aktif { get; set; }
        
        public Guid? OlusturanKullaniciID { get; set; }
        
        public Guid? SonGuncelleyenKullaniciID { get; set; }
        
        public DateTime? OlusturmaTarihi { get; set; }
        
        public DateTime? GuncellemeTarihi { get; set; }
        
        public bool SoftDelete { get; set; }
        
        [StringLength(250)]
        public string Adres { get; set; }
        
        [StringLength(500)]
        public string Aciklama { get; set; }
        
        [StringLength(50)]
        public string Yetkili { get; set; }
        
        // Navigation properties
        public virtual ICollection<CariHareket> CariHareketleri { get; set; }
        public virtual ICollection<Fatura> Faturalar { get; set; }
        
        public Cari()
        {
            CariHareketleri = new HashSet<CariHareket>();
            Faturalar = new HashSet<Fatura>();
        }
    }
} 
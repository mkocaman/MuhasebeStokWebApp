using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MuhasebeStokWebApp.Data.Entities
{
    public class Urun
    {
        [Key]
        public Guid UrunID { get; set; }
        
        [Required]
        [StringLength(50)]
        public string UrunKodu { get; set; } = string.Empty;
        
        [Required]
        [StringLength(200)]
        public string UrunAdi { get; set; } = string.Empty;
        
        // Birim ilişkisi
        public int? BirimID { get; set; }
        
        [ForeignKey("BirimID")]
        public virtual UrunBirim? Birim { get; set; }
        
        public decimal StokMiktar { get; set; }
        
        public bool Aktif { get; set; }
        
        // KDV Oranı
        public int KDVOrani { get; set; } = 18; // Varsayılan %18
        
        public Guid? OlusturanKullaniciID { get; set; }
        
        public Guid? SonGuncelleyenKullaniciID { get; set; }
        
        public DateTime? OlusturmaTarihi { get; set; }
        
        public DateTime? GuncellemeTarihi { get; set; }
        
        public bool Silindi { get; set; }
        
        // Kategori ilişkisi
        public Guid? KategoriID { get; set; }
        
        [ForeignKey("KategoriID")]
        public virtual UrunKategori? Kategori { get; set; }
        
        // Navigation properties
        public virtual ICollection<UrunFiyat> UrunFiyatlari { get; set; }
        public virtual ICollection<StokHareket> StokHareketleri { get; set; }
        public virtual ICollection<FaturaDetay> FaturaDetaylari { get; set; }
        
        public Urun()
        {
            UrunFiyatlari = new HashSet<UrunFiyat>();
            StokHareketleri = new HashSet<StokHareket>();
            FaturaDetaylari = new HashSet<FaturaDetay>();
        }
    }
} 
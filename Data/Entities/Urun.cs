using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MuhasebeStokWebApp.Data.Entities
{
    public class Urun : ISoftDelete
    {
        [Key]
        public Guid UrunID { get; set; }
        
        [Required]
        [StringLength(50)]
        public string UrunKodu { get; set; } = string.Empty;
        
        [Required]
        [StringLength(200)]
        public string UrunAdi { get; set; } = string.Empty;
        
        [StringLength(500)]
        public string Aciklama { get; set; } = string.Empty;
        
        // Birim ilişkisi
        public Guid? BirimID { get; set; }
        
        [ForeignKey("BirimID")]
        public virtual Birim? Birim { get; set; }
        
        public bool Aktif { get; set; }
        
        // KDV Oranı
        public int KDVOrani { get; set; } = 12; // Varsayılan %12
        
        // Kritik Stok Seviyesi
        public decimal KritikStokSeviyesi { get; set; } = 100; // Varsayılan 100
        
        // Dövizli Fiyat Alanları (USD)
        public decimal? DovizliListeFiyati { get; set; }
        public decimal? DovizliMaliyetFiyati { get; set; }
        public decimal? DovizliSatisFiyati { get; set; }
        
        // StokMiktar property'si eklendi
        [NotMapped] // Veritabanında saklanmaması için
        public decimal StokMiktar { get; set; } = 0;
        
        // NOT: StokMiktar alanı statik olarak tutulmamaktadır. 
        // Stok miktarı StokService.GetDinamikStokMiktari metodu ile dinamik olarak hesaplanmaktadır.
        // Olası kullanımlar için: var stokMiktar = await _stokService.GetDinamikStokMiktari(urun.UrunID);
        
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
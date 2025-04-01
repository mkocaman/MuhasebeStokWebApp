using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MuhasebeStokWebApp.Data.Entities
{
    public class Birim : ISoftDelete
    {
        public Birim()
        {
            // Non-nullable property'ler için varsayılan değerler
            BirimID = Guid.NewGuid();
            BirimAdi = string.Empty;
            BirimKodu = string.Empty;
            BirimSembol = string.Empty;
            Aciklama = string.Empty;
            OlusturanKullaniciID = "system"; // Varsayılan değer olarak "system" atanıyor
            Aktif = true;
            Silindi = false;
            OlusturmaTarihi = DateTime.Now;
            
            // Collection initialization
            Urunler = new HashSet<Urun>();
            FaturaDetaylari = new HashSet<FaturaDetay>();
            IrsaliyeDetaylari = new HashSet<IrsaliyeDetay>();
        }
        
        [Key]
        public Guid BirimID { get; set; }
        
        [Required]
        [StringLength(50)]
        public string BirimAdi { get; set; }
        
        [Required]
        [StringLength(20)]
        public string BirimKodu { get; set; }
        
        [Required]
        [StringLength(10)]
        public string BirimSembol { get; set; }
        
        [StringLength(200)]
        public string Aciklama { get; set; }
        
        public bool Aktif { get; set; }
        
        public bool Silindi { get; set; }
        
        public DateTime OlusturmaTarihi { get; set; }
        
        public DateTime? GuncellemeTarihi { get; set; }
        
        [Required]
        public string OlusturanKullaniciID { get; set; }
        
        public Guid? SonGuncelleyenKullaniciID { get; set; }
        
        public Guid? SirketID { get; set; }
        
        // Navigation properties
        public virtual ICollection<Urun> Urunler { get; set; }
        public virtual ICollection<FaturaDetay> FaturaDetaylari { get; set; }
        public virtual ICollection<IrsaliyeDetay> IrsaliyeDetaylari { get; set; }
    }
} 
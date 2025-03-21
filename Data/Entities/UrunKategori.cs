using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MuhasebeStokWebApp.Data.Entities
{
    public class UrunKategori
    {
        [Key]
        public Guid KategoriID { get; set; }
        
        [Required]
        [StringLength(100)]
        public required string KategoriAdi { get; set; }
        
        [StringLength(500)]
        public string? Aciklama { get; set; }
        
        public bool Aktif { get; set; } = true;
        
        public Guid? OlusturanKullaniciID { get; set; }
        
        public Guid? SonGuncelleyenKullaniciID { get; set; }
        
        public DateTime? OlusturmaTarihi { get; set; }
        
        public DateTime? GuncellemeTarihi { get; set; }
        
        public bool SoftDelete { get; set; }
        
        // Navigation properties
        public virtual ICollection<Urun> Urunler { get; set; }
        
        public UrunKategori()
        {
            Urunler = new HashSet<Urun>();
        }
    }
} 
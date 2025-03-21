using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MuhasebeStokWebApp.Data.Entities
{
    public class SistemAyarlari
    {
        [Key]
        public int SistemAyarlariID { get; set; }
        
        [Required]
        [StringLength(10)]
        public required string AnaDovizKodu { get; set; } = "TRY";
        
        [Required]
        [StringLength(50)]
        public required string SirketAdi { get; set; }
        
        [StringLength(100)]
        public required string SirketAdresi { get; set; }
        
        [StringLength(20)]
        public required string SirketTelefon { get; set; }
        
        [StringLength(100)]
        public required string SirketEmail { get; set; }
        
        [StringLength(20)]
        public required string SirketVergiNo { get; set; }
        
        [StringLength(20)]
        public required string SirketVergiDairesi { get; set; }
        
        public bool OtomatikDovizGuncelleme { get; set; } = true;
        
        public int DovizGuncellemeSikligi { get; set; } = 24; // Saat cinsinden
        
        public DateTime SonDovizGuncellemeTarihi { get; set; }
        
        [StringLength(500)]
        public string? AktifParaBirimleri { get; set; }
        
        public bool Aktif { get; set; } = true;
        
        public bool SoftDelete { get; set; } = false;
        
        public DateTime OlusturmaTarihi { get; set; } = DateTime.Now;
        
        public DateTime? GuncellemeTarihi { get; set; }
    }
} 
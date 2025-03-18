using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MuhasebeStokWebApp.Data.Entities
{
    /// <summary>
    /// Döviz Kuru entity'si
    /// </summary>
    public class DovizKuru
    {
        public DovizKuru()
        {
            // Default yapıcı metot ile varsayılan değerleri ayarla
            DovizKuruID = Guid.NewGuid();
            Aktif = true;
            SoftDelete = false;
            OlusturmaTarihi = DateTime.Now;
        }
        
        [Key]
        public Guid DovizKuruID { get; set; }
        
        [Required, StringLength(10)]
        public required string ParaBirimi { get; set; } = string.Empty;
        
        [Required, StringLength(10)]
        public required string BazParaBirimi { get; set; } = string.Empty;
        
        [StringLength(10)]
        public string? DovizKodu { get; set; } = string.Empty;
        
        [Required, StringLength(100)]
        public required string DovizAdi { get; set; } = string.Empty;
        
        [Required]
        [Column(TypeName = "decimal(18,6)")]
        public decimal Kur { get; set; }
        
        [Column(TypeName = "decimal(18,6)")]
        public decimal? AlisFiyati { get; set; }
        
        [Column(TypeName = "decimal(18,6)")]
        public decimal? SatisFiyati { get; set; }
        
        [Required]
        public DateTime Tarih { get; set; } = DateTime.Now;
        
        [StringLength(100)]
        public string? Kaynak { get; set; } = string.Empty;
        
        [StringLength(500)]
        public string? Aciklama { get; set; } = string.Empty;
        
        [Required]
        public bool Aktif { get; set; } = true;
        
        [Required]
        public bool SoftDelete { get; set; } = false;
        
        [Required]
        public DateTime OlusturmaTarihi { get; set; } = DateTime.Now;
        
        public DateTime? GuncellemeTarihi { get; set; }
    }
} 
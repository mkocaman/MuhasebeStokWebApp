using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MuhasebeStokWebApp.Data.Entities
{
    public class DovizKuru
    {
        [Key]
        public int ID { get; set; }
        
        [Required]
        [StringLength(10)]
        public string ParaBirimi { get; set; }
        
        [Required]
        [StringLength(10)]
        public string BazParaBirimi { get; set; }
        
        [Column(TypeName = "decimal(18,6)")]
        public decimal Alis { get; set; }
        
        [Column(TypeName = "decimal(18,6)")]
        public decimal Satis { get; set; }
        
        [Column(TypeName = "decimal(18,6)")]
        public decimal EfektifAlis { get; set; }
        
        [Column(TypeName = "decimal(18,6)")]
        public decimal EfektifSatis { get; set; }
        
        public DateTime Tarih { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Kaynak { get; set; }
        
        [StringLength(500)]
        public string Aciklama { get; set; }
        
        public bool Aktif { get; set; } = true;
        public bool SoftDelete { get; set; } = false;
        public DateTime OlusturmaTarihi { get; set; } = DateTime.Now;
        public DateTime? GuncellemeTarihi { get; set; }
    }
} 
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MuhasebeStokWebApp.Data.Entities
{
    [Table("SistemAyarlari")]
    public class SistemAyar
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public required string Anahtar { get; set; }
        
        [Required]
        [StringLength(500)]
        public required string Deger { get; set; }
        
        [StringLength(250)]
        public required string Aciklama { get; set; } = string.Empty;
        
        public DateTime OlusturmaTarihi { get; set; } = DateTime.Now;
        
        public DateTime GuncellemeTarihi { get; set; } = DateTime.Now;
        
        public bool Silindi { get; set; } = false;
    }
} 
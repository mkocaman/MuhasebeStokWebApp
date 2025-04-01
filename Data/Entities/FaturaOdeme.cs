using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MuhasebeStokWebApp.Data.Entities
{
    public class FaturaOdeme
    {
        [Key]
        public Guid OdemeID { get; set; }
        
        public Guid FaturaID { get; set; }
        
        [Required]
        public DateTime OdemeTarihi { get; set; }
        
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal OdemeTutari { get; set; }
        
        [Required]
        [StringLength(50)]
        public string OdemeTuru { get; set; }
        
        [StringLength(500)]
        public string Aciklama { get; set; }
        
        public Guid? OlusturanKullaniciID { get; set; }
        
        public Guid? SonGuncelleyenKullaniciID { get; set; }
        
        public DateTime? OlusturmaTarihi { get; set; }
        
        public DateTime? GuncellemeTarihi { get; set; }
        
        public bool Silindi { get; set; }
        
        public bool Aktif { get; set; } = true;
        
        // Navigation properties
        [ForeignKey("FaturaID")]
        public virtual Fatura Fatura { get; set; }
    }
} 
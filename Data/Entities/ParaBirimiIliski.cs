using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MuhasebeStokWebApp.Data.Entities
{
    [Table("ParaBirimiIliski")]
    public class ParaBirimiIliski
    {
        [Key]
        public Guid ParaBirimiIliskiID { get; set; }
        
        [Required]
        public Guid KaynakParaBirimiID { get; set; }
        
        [Required]
        public Guid HedefParaBirimiID { get; set; }
        
        [Required]
        [Range(0.001, 10000)]
        public decimal Carpan { get; set; } = 1.0m;
        
        public bool Aktif { get; set; } = true;
        
        public DateTime OlusturmaTarihi { get; set; }
        
        public DateTime GuncellemeTarihi { get; set; }
        
        public bool Silindi { get; set; } = false;
        
        // İlişkiler
        [ForeignKey("KaynakParaBirimiID")]
        public virtual ParaBirimi KaynakParaBirimi { get; set; }
        
        [ForeignKey("HedefParaBirimiID")]
        public virtual ParaBirimi HedefParaBirimi { get; set; }
    }
} 
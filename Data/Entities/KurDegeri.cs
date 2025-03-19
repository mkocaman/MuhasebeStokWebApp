using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MuhasebeStokWebApp.Data.Entities
{
    /// <summary>
    /// Kur değeri entity'si - Merkez Bankasından çekilen kurları saklar
    /// </summary>
    [Table("KurDegeri")]
    public class KurDegeri
    {
        [Key]
        public Guid KurDegeriID { get; set; }
        
        [Required]
        public Guid ParaBirimiID { get; set; }
        
        [Required]
        [Range(0.00001, 1000000)]
        public decimal AlisDegeri { get; set; }
        
        [Required]
        [Range(0.00001, 1000000)]
        public decimal SatisDegeri { get; set; }
        
        [Required]
        public DateTime Tarih { get; set; } = DateTime.Now;
        
        [Required]
        [StringLength(50)]
        public string Kaynak { get; set; } = "Manuel";
        
        public bool Aktif { get; set; } = true;
        
        public DateTime OlusturmaTarihi { get; set; }
        
        public DateTime GuncellemeTarihi { get; set; }
        
        public bool Silindi { get; set; } = false;
        
        // İlişkiler
        [ForeignKey("ParaBirimiID")]
        public virtual ParaBirimi ParaBirimi { get; set; }
    }
} 
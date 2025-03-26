using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MuhasebeStokWebApp.Data.Entities
{
    public class Fatura
    {
        [Key]
        public Guid FaturaID { get; set; }
        
        [Required]
        [StringLength(20)]
        public string FaturaNumarasi { get; set; }
        
        [Required]
        [StringLength(20)]
        public string SiparisNumarasi { get; set; }
        
        public DateTime? FaturaTarihi { get; set; }
        
        public DateTime? VadeTarihi { get; set; }
        
        [ForeignKey("Cari")]
        public Guid? CariID { get; set; }
        
        public virtual Cari Cari { get; set; }
        
        public int? FaturaTuruID { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal? AraToplam { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal? KDVToplam { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal? GenelToplam { get; set; }
        
        [StringLength(50)]
        public string OdemeDurumu { get; set; }
        
        [StringLength(500)]
        public string FaturaNotu { get; set; }
        
        public bool? Resmi { get; set; }
        
        [StringLength(10)]
        public string DovizTuru { get; set; }
        
        [Column(TypeName = "decimal(18,4)")]
        public decimal? DovizKuru { get; set; }
        
        public DateTime? OlusturmaTarihi { get; set; }
        
        public DateTime? GuncellemeTarihi { get; set; }
        
        public bool Silindi { get; set; }
        
        public bool? Aktif { get; set; }
        
        public Guid? OlusturanKullaniciID { get; set; }
        
        public Guid? SonGuncelleyenKullaniciID { get; set; }
        
        public int? OdemeTuruID { get; set; }
        
        [ForeignKey("OdemeTuruID")]
        public virtual OdemeTuru OdemeTuru { get; set; }
        
        [ForeignKey("FaturaTuruID")]
        public virtual FaturaTuru FaturaTuru { get; set; }
        
        public virtual ICollection<FaturaDetay> FaturaDetaylari { get; set; } = new List<FaturaDetay>();
        public virtual ICollection<Irsaliye> Irsaliyeler { get; set; }
        public virtual ICollection<FaturaOdeme> FaturaOdemeleri { get; set; }
        
        public Fatura()
        {
            FaturaID = Guid.NewGuid();
            Irsaliyeler = new HashSet<Irsaliye>();
            FaturaOdemeleri = new HashSet<FaturaOdeme>();
        }
    }
} 
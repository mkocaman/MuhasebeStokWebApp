using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MuhasebeStokWebApp.Enums;

namespace MuhasebeStokWebApp.Data.Entities
{
    public class FaturaAklamaKuyruk
    {
        [Key]
        public Guid AklamaID { get; set; }
        
        [Required]
        [ForeignKey("FaturaDetay")]
        public Guid FaturaKalemID { get; set; }
        
        public virtual FaturaDetay FaturaDetay { get; set; }
        
        [Required]
        [ForeignKey("Urun")]
        public Guid UrunID { get; set; }
        
        public virtual Urun Urun { get; set; }
        
        [Required]
        [Column(TypeName = "decimal(18,3)")]
        public decimal AklananMiktar { get; set; }
        
        [Required]
        public DateTime OlusturmaTarihi { get; set; } = DateTime.Now;
        
        public DateTime? AklanmaTarihi { get; set; }
        
        [StringLength(500)]
        [Required]
        public string AklanmaNotu { get; set; } = "";
        
        [Required]
        [ForeignKey("Sozlesme")]
        public Guid SozlesmeID { get; set; }
        
        public virtual Sozlesme Sozlesme { get; set; }
        
        [Required]
        public AklamaDurumu Durum { get; set; } = AklamaDurumu.Bekliyor;
        
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal BirimFiyat { get; set; }
        
        [Required]
        [StringLength(10)]
        public string ParaBirimi { get; set; } = "TL";
        
        [Required]
        [Column(TypeName = "decimal(18,4)")]
        public decimal DovizKuru { get; set; } = 1;
        
        public Guid? OlusturanKullaniciID { get; set; }
        
        public Guid? GuncelleyenKullaniciID { get; set; }
        
        public DateTime? GuncellemeTarihi { get; set; }
        
        [Required]
        public bool Silindi { get; set; } = false;
        
        public Guid? ResmiFaturaKalemID { get; set; }
        
        public FaturaAklamaKuyruk()
        {
            AklamaID = Guid.NewGuid();
        }
    }
} 
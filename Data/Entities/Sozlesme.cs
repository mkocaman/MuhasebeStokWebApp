using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MuhasebeStokWebApp.Data.Entities
{
    public class Sozlesme
    {
        [Key]
        public Guid SozlesmeID { get; set; }
        
        [Required]
        [StringLength(100)]
        public string SozlesmeNo { get; set; } = "";
        
        [Required]
        public DateTime SozlesmeTarihi { get; set; }
        
        public DateTime? BitisTarihi { get; set; }
        
        [ForeignKey("Cari")]
        public Guid? CariID { get; set; }
        
        public virtual Cari? Cari { get; set; }
        
        public bool VekaletGeldiMi { get; set; } = false;
        
        public bool ResmiFaturaKesildiMi { get; set; } = false;
        
        [StringLength(500)]
        public string? SozlesmeDosyaYolu { get; set; }
        
        [StringLength(500)]
        public string? VekaletnameDosyaYolu { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal SozlesmeTutari { get; set; }
        
        [StringLength(10)]
        public string? SozlesmeDovizTuru { get; set; } = "TL";
        
        [StringLength(1000)]
        public string? Aciklama { get; set; }
        
        public bool AktifMi { get; set; } = true;
        
        public DateTime OlusturmaTarihi { get; set; } = DateTime.Now;
        
        public DateTime? GuncellemeTarihi { get; set; }
        
        public Guid? OlusturanKullaniciID { get; set; }
        
        public Guid? GuncelleyenKullaniciID { get; set; }
        
        public bool Silindi { get; set; } = false;
        
        public Guid? AnaSozlesmeID { get; set; }
        
        public virtual ICollection<Fatura> Faturalar { get; set; }
        
        public virtual ICollection<FaturaAklamaKuyruk> AklamaKayitlari { get; set; }
        
        public Sozlesme()
        {
            SozlesmeID = Guid.NewGuid();
            Faturalar = new List<Fatura>();
            AklamaKayitlari = new List<FaturaAklamaKuyruk>();
        }
    }
} 
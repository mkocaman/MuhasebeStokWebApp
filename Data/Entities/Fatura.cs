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
        public string FaturaNumarasi { get; set; } = "";
        
        [Required]
        [StringLength(20)]
        public string SiparisNumarasi { get; set; } = "";
        
        public DateTime? FaturaTarihi { get; set; }
        
        public DateTime? VadeTarihi { get; set; }
        
        [ForeignKey("Cari")]
        public Guid? CariID { get; set; }
        
        public virtual Cari? Cari { get; set; }
        
        public int? FaturaTuruID { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal? AraToplam { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal? KDVToplam { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal? IndirimTutari { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal? GenelToplam { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal? OdenenTutar { get; set; } = 0;
        
        // Dövizli toplam değerler (seçilen para biriminde)
        [Column(TypeName = "decimal(18,2)")]
        public decimal? AraToplamDoviz { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal? KDVToplamDoviz { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal? IndirimTutariDoviz { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal? GenelToplamDoviz { get; set; }
        
        [StringLength(50)]
        public string OdemeDurumu { get; set; } = "";
        
        [StringLength(500)]
        public string FaturaNotu { get; set; } = "";
        
        [Required]
        public bool ResmiMi { get; set; } = false;
        
        public Guid? SozlesmeID { get; set; }
        
        public virtual Sozlesme? Sozlesme { get; set; }
        
        [StringLength(10)]
        public string DovizTuru { get; set; } = "USD";
        
        // Para birimi
        [StringLength(10)]
        public string ParaBirimi { get; set; } = "USD";
        
        [Column(TypeName = "decimal(18,4)")]
        public decimal? DovizKuru { get; set; } = 1;
        
        public DateTime? OlusturmaTarihi { get; set; } = DateTime.Now;
        
        public DateTime? GuncellemeTarihi { get; set; }
        
        [Required]
        public bool Silindi { get; set; } = false;
        
        [Required]
        public bool Aktif { get; set; } = true;
        
        public Guid? OlusturanKullaniciID { get; set; }
        
        public Guid? SonGuncelleyenKullaniciID { get; set; }
        
        public int? OdemeTuruID { get; set; }
        
        [ForeignKey("OdemeTuruID")]
        public virtual OdemeTuru? OdemeTuru { get; set; }
        
        [ForeignKey("FaturaTuruID")]
        public virtual FaturaTuru? FaturaTuru { get; set; }
        
        public virtual ICollection<FaturaDetay>? FaturaDetaylari { get; set; }
        public virtual ICollection<Irsaliye>? Irsaliyeler { get; set; }
        public virtual ICollection<FaturaOdeme>? FaturaOdemeleri { get; set; }
        public virtual ICollection<FaturaAklamaKuyruk>? AklamaKayitlari { get; set; }
        
        public DateTime? AklanmaTarihi { get; set; }
        
        [StringLength(500)]
        public string AklanmaNotu { get; set; } = "";
        
        public Fatura()
        {
            FaturaID = Guid.NewGuid();
            FaturaNumarasi = "";
            SiparisNumarasi = "";
            FaturaTarihi = DateTime.Now;
            VadeTarihi = DateTime.Now;
            OdemeDurumu = "";
            FaturaNotu = "";
            DovizTuru = "USD";
            ParaBirimi = "USD";
            DovizKuru = 1;
            OlusturmaTarihi = DateTime.Now;
            Silindi = false;
            Aktif = true;
            AklanmaNotu = "";
            ResmiMi = false;
            FaturaDetaylari = new HashSet<FaturaDetay>();
            Irsaliyeler = new HashSet<Irsaliye>();
            FaturaOdemeleri = new HashSet<FaturaOdeme>();
            AklamaKayitlari = new HashSet<FaturaAklamaKuyruk>();
        }
    }
} 
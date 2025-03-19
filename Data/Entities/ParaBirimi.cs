using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MuhasebeStokWebApp.Data.Entities
{
    /// <summary>
    /// Para Birimi (döviz) tablosu
    /// </summary>
    [Table("ParaBirimi")]
    public class ParaBirimi
    {
        public ParaBirimi()
        {
            ParaBirimiID = Guid.NewGuid();
            Aktif = true;
            Silindi = false;
            OlusturmaTarihi = DateTime.Now;
            GuncellemeTarihi = DateTime.Now;
        }

        [Key]
        public Guid ParaBirimiID { get; set; }
        
        [Required]
        [StringLength(5)]
        [Display(Name = "Para Birimi Kodu")]
        [Column("DovizKodu")]
        public string Kod { get; set; } = string.Empty;
        
        [Required]
        [StringLength(50)]
        [Display(Name = "Para Birimi Adı")]
        [Column("DovizAdi")]
        public string Ad { get; set; } = string.Empty;
        
        [StringLength(10)]
        [Display(Name = "Sembol")]
        [Column("Sembol")]
        public string Sembol { get; set; } = string.Empty;
        
        [Display(Name = "Aktif")]
        public bool Aktif { get; set; } = true;
        
        [Display(Name = "Oluşturma Tarihi")]
        public DateTime OlusturmaTarihi { get; set; }
        
        [Display(Name = "Güncelleme Tarihi")]
        public DateTime GuncellemeTarihi { get; set; }
        
        [Display(Name = "Silindi")]
        public bool Silindi { get; set; } = false;
        
        // Navigasyon özellikleri
        
        /// <summary>
        /// Bu para birimini kaynak olarak kullanan para birimi ilişkileri
        /// </summary>
        [InverseProperty("KaynakParaBirimi")]
        public virtual ICollection<ParaBirimiIliski> KaynakIliskiler { get; set; }
        
        /// <summary>
        /// Bu para birimini hedef olarak kullanan para birimi ilişkileri
        /// </summary>
        [InverseProperty("HedefParaBirimi")]
        public virtual ICollection<ParaBirimiIliski> HedefIliskiler { get; set; }
        
        public virtual ICollection<KurDegeri> KurDegerleri { get; set; }
    }
} 
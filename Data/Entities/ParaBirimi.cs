using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MuhasebeStokWebApp.Data.Entities
{
    /// <summary>
    /// Para Birimi tablosu
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
            KurDegerleri = new HashSet<KurDegeri>();
            Sira = 0;
            Format = "#,##0.00";
            Kod = string.Empty;
            Ad = string.Empty;
            Sembol = string.Empty;
            Aciklama = string.Empty;
        }

        [Key]
        public Guid ParaBirimiID { get; set; }
        
        [Required]
        [StringLength(5)]
        [Display(Name = "Para Birimi Kodu")]
        [Column("DovizKodu")]
        public string Kod { get; set; }
        
        [Required]
        [StringLength(50)]
        [Display(Name = "Para Birimi Adı")]
        [Column("DovizAdi")]
        public string Ad { get; set; }
        
        [StringLength(10)]
        [Display(Name = "Sembol")]
        [Column("Sembol")]
        public string Sembol { get; set; }
        
        [Display(Name = "Aktif")]
        public bool Aktif { get; set; } = true;
        
        [Display(Name = "Oluşturma Tarihi")]
        public DateTime OlusturmaTarihi { get; set; }
        
        [Display(Name = "Güncelleme Tarihi")]
        public DateTime GuncellemeTarihi { get; set; }
        
        [Display(Name = "Silindi")]
        public bool Silindi { get; set; } = false;
        
        [Display(Name = "Sıra")]
        public int Sira { get; set; }
        
        [StringLength(50)]
        [Display(Name = "Format")]
        public string Format { get; set; }
        
        [StringLength(250)]
        [Display(Name = "Açıklama")]
        public string Aciklama { get; set; }
        
        // Navigasyon özellikleri
        
        /// <summary>
        /// Bu para birimine ait kur değerleri
        /// </summary>
        public virtual ICollection<KurDegeri> KurDegerleri { get; set; }
    }
} 
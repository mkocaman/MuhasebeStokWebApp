using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MuhasebeStokWebApp.Data.Entities
{
    /// <summary>
    /// Para Birimi (döviz) tablosu
    /// </summary>
    [Table("Dovizler")]
    public class ParaBirimi
    {
        public ParaBirimi()
        {
            ParaBirimiID = Guid.NewGuid();
            Aktif = true;
            SoftDelete = false;
        }

        [Key]
        [Column("DovizID")]
        public Guid ParaBirimiID { get; set; }
        
        [Required]
        [StringLength(3)]
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
        public string? Sembol { get; set; }
        
        [Display(Name = "Aktif")]
        public bool Aktif { get; set; } = true;
        
        [Display(Name = "Silinmiş")]
        public bool SoftDelete { get; set; } = false;
        
        // Navigasyon özellikleri
        
        /// <summary>
        /// Bu para birimini kaynak olarak kullanan döviz ilişkileri
        /// </summary>
        public virtual ICollection<DovizIliski>? KaynakDovizIliskileri { get; set; }
        
        /// <summary>
        /// Bu para birimini hedef olarak kullanan döviz ilişkileri
        /// </summary>
        public virtual ICollection<DovizIliski>? HedefDovizIliskileri { get; set; }
    }
} 
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MuhasebeStokWebApp.Models
{
    public class KurDegeri
    {
        public Guid KurDegeriID { get; set; }
        
        public Guid ParaBirimiID { get; set; }
        
        [Display(Name = "Tarih")]
        public DateTime Tarih { get; set; }
        
        [Display(Name = "Alış")]
        public decimal Alis { get; set; }
        
        [Display(Name = "Satış")]
        public decimal Satis { get; set; }

        /// <summary>
        /// Efektif alış fiyatı
        /// </summary>
        [Display(Name = "Efektif Alış")]
        public decimal EfektifAlis { get; set; }

        /// <summary>
        /// Efektif satış fiyatı
        /// </summary>
        [Display(Name = "Efektif Satış")]
        public decimal EfektifSatis { get; set; }
        
        [Display(Name = "Aktif")]
        public bool Aktif { get; set; } = true;
        
        [Display(Name = "Silindi")]
        public bool Silindi { get; set; } = false;
        
        [Display(Name = "Açıklama")]
        public string? Aciklama { get; set; }
        
        public DateTime OlusturmaTarihi { get; set; } = DateTime.Now;
        
        public DateTime? GuncellemeTarihi { get; set; }
        
        public string? OlusturanKullaniciID { get; set; }
        
        public string? SonGuncelleyenKullaniciID { get; set; }
        
        // Navigation property
        public virtual MuhasebeStokWebApp.Data.Entities.ParaBirimiModulu.ParaBirimi? ParaBirimi { get; set; }
    }
} 
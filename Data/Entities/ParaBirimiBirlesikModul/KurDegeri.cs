#nullable enable

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MuhasebeStokWebApp.Data.Entities.ParaBirimiBirlesikModul
{
    /// <summary>
    /// Para birimi kur değerleri entity class'ı
    /// </summary>
    [Table("BirlesikModulKurDegerleri")]
    public class KurDegeri
    {
        /// <summary>
        /// Kur değeri benzersiz kimliği
        /// </summary>
        [Key]
        public Guid KurDegeriID { get; set; } = Guid.NewGuid();
        
        /// <summary>
        /// Para birimi ID'si
        /// </summary>
        [Required(ErrorMessage = "Para birimi ID gereklidir.")]
        [ForeignKey("ParaBirimi")]
        public Guid ParaBirimiID { get; set; }
        
        /// <summary>
        /// Alış kuru
        /// </summary>
        [Required(ErrorMessage = "Alış kuru gereklidir.")]
        [Column(TypeName = "decimal(18, 6)")]
        [Display(Name = "Alış")]
        [Range(0.000001, double.MaxValue, ErrorMessage = "Alış kuru 0'dan büyük olmalıdır.")]
        public decimal Alis { get; set; }
        
        /// <summary>
        /// Satış kuru
        /// </summary>
        [Required(ErrorMessage = "Satış kuru gereklidir.")]
        [Column(TypeName = "decimal(18, 6)")]
        [Display(Name = "Satış")]
        [Range(0.000001, double.MaxValue, ErrorMessage = "Satış kuru 0'dan büyük olmalıdır.")]
        public decimal Satis { get; set; }
        
        /// <summary>
        /// Efektif alış kuru
        /// </summary>
        [Column(TypeName = "decimal(18, 6)")]
        [Display(Name = "Efektif Alış")]
        public decimal EfektifAlis { get; set; }
        
        /// <summary>
        /// Efektif satış kuru
        /// </summary>
        [Column(TypeName = "decimal(18, 6)")]
        [Display(Name = "Efektif Satış")]
        public decimal EfektifSatis { get; set; }
        
        /// <summary>
        /// Kur tarihi
        /// </summary>
        [Required(ErrorMessage = "Tarih gereklidir.")]
        [Column(TypeName = "datetime2")]
        [Display(Name = "Tarih")]
        public DateTime Tarih { get; set; } = DateTime.Now.Date;
        
        /// <summary>
        /// Kur verisi aktif mi
        /// </summary>
        [Display(Name = "Aktif")]
        public bool Aktif { get; set; } = true;
        
        /// <summary>
        /// Soft delete için
        /// </summary>
        [Display(Name = "Silindi")]
        public bool Silindi { get; set; } = false;
        
        /// <summary>
        /// Kur veri kaynağı (örn. TCMB, Manüel vb.)
        /// </summary>
        [StringLength(50, ErrorMessage = "Veri kaynağı en fazla 50 karakter olabilir.")]
        [Display(Name = "Veri Kaynağı")]
        public string VeriKaynagi { get; set; } = "Manüel";
        
        /// <summary>
        /// Oluşturulma tarihi
        /// </summary>
        [Column(TypeName = "datetime2")]
        public DateTime OlusturmaTarihi { get; set; } = DateTime.Now;
        
        /// <summary>
        /// Son güncellenme tarihi
        /// </summary>
        [Column(TypeName = "datetime2")]
        public DateTime? GuncellemeTarihi { get; set; }
        
        /// <summary>
        /// Oluşturan kullanıcı ID'si
        /// </summary>
        public string OlusturanKullaniciID { get; set; } = string.Empty;
        
        /// <summary>
        /// Son güncelleyen kullanıcı ID'si
        /// </summary>
        public string SonGuncelleyenKullaniciID { get; set; } = string.Empty;
        
        /// <summary>
        /// Para birimi
        /// </summary>
        public virtual ParaBirimi? ParaBirimi { get; set; }
    }
} 
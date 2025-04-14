#nullable enable

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MuhasebeStokWebApp.Data.Entities.ParaBirimiModulu
{
    /// <summary>
    /// Döviz kuru değeri entity class'ı
    /// </summary>
    [Table("KurDegerleri")]
    public class KurDegeri : BaseEntity
    {
        /// <summary>
        /// Kur değeri benzersiz kimliği
        /// </summary>
        [Key]
        public Guid KurDegeriID { get; set; }
        
        /// <summary>
        /// Para birimi ID'si
        /// </summary>
        [Required(ErrorMessage = "Para birimi ID gereklidir.")]
        [Display(Name = "Para Birimi")]
        public Guid ParaBirimiID { get; set; }
        
        /// <summary>
        /// Kurun geçerli olduğu tarih
        /// </summary>
        [Required(ErrorMessage = "Tarih gereklidir.")]
        [Display(Name = "Tarih")]
        [DataType(DataType.Date)]
        public DateTime Tarih { get; set; }
        
        /// <summary>
        /// Alış değeri
        /// </summary>
        [Required(ErrorMessage = "Alış kuru gereklidir.")]
        [Display(Name = "Alış")]
        [Column(TypeName = "decimal(18,6)")]
        public decimal Alis { get; set; }
        
        /// <summary>
        /// Satış değeri
        /// </summary>
        [Required(ErrorMessage = "Satış kuru gereklidir.")]
        [Display(Name = "Satış")]
        [Column(TypeName = "decimal(18,6)")]
        public decimal Satis { get; set; }
        
        /// <summary>
        /// Efektif alış fiyatı
        /// </summary>
        [Display(Name = "Efektif Alış")]
        [Column(TypeName = "decimal(18,6)")]
        public decimal EfektifAlis { get; set; }
        
        /// <summary>
        /// Efektif satış fiyatı
        /// </summary>
        [Display(Name = "Efektif Satış")]
        [Column(TypeName = "decimal(18,6)")]
        public decimal EfektifSatis { get; set; }
        
        /// <summary>
        /// Aktif mi
        /// </summary>
        [Display(Name = "Aktif")]
        public bool Aktif { get; set; } = true;
        
        /// <summary>
        /// Silindi mi
        /// </summary>
        [Display(Name = "Silindi")]
        public bool Silindi { get; set; } = false;
        
        /// <summary>
        /// Açıklama
        /// </summary>
        [Display(Name = "Açıklama")]
        [StringLength(500, ErrorMessage = "Açıklama en fazla 500 karakter olabilir.")]
        public string? Aciklama { get; set; }
        
        /// <summary>
        /// Oluşturulma tarihi
        /// </summary>
        [Column(TypeName = "datetime2")]
        public new DateTime OlusturmaTarihi { get; set; } = DateTime.Now;
        
        /// <summary>
        /// Son güncellenme tarihi
        /// </summary>
        [Column(TypeName = "datetime2")]
        public new DateTime? GuncellemeTarihi { get; set; }
        
        /// <summary>
        /// Oluşturan kullanıcı ID'si
        /// </summary>
        public string? OlusturanKullaniciID { get; set; }
        
        /// <summary>
        /// Son güncelleyen kullanıcı ID'si
        /// </summary>
        public string? SonGuncelleyenKullaniciID { get; set; }
        
        /// <summary>
        /// İlişkili para birimi
        /// </summary>
        [ForeignKey("ParaBirimiID")]
        public virtual ParaBirimi? ParaBirimi { get; set; }
    }
} 
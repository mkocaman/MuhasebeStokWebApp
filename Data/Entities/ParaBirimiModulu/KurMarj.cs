#nullable enable

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MuhasebeStokWebApp.Data.Entities.ParaBirimiModulu
{
    /// <summary>
    /// Döviz kuru marj ayarları entity sınıfı
    /// </summary>
    [Table("KurMarjlari")]
    public class KurMarj : BaseEntity
    {
        /// <summary>
        /// Kur marj ayarı benzersiz kimliği
        /// </summary>
        [Key]
        public Guid KurMarjID { get; set; } = Guid.NewGuid();
        
        /// <summary>
        /// Alış-Satış kuru arasındaki marj yüzdesi
        /// </summary>
        [Required(ErrorMessage = "Satış marjı gereklidir.")]
        [Display(Name = "Satış Marjı (%)")]
        [Range(0, 100, ErrorMessage = "Satış marjı 0 ile 100 arasında olmalıdır.")]
        [Column(TypeName = "decimal(5,2)")]
        public decimal SatisMarji { get; set; } = 2.00m; // Varsayılan olarak %2
        
        /// <summary>
        /// Varsayılan ayar mı
        /// </summary>
        [Display(Name = "Varsayılan")]
        public bool Varsayilan { get; set; } = true;
        
        /// <summary>
        /// Tanım / Açıklama
        /// </summary>
        [Required(ErrorMessage = "Tanım gereklidir.")]
        [Display(Name = "Tanım")]
        [StringLength(100, ErrorMessage = "Tanım en fazla 100 karakter olabilir.")]
        public string Tanim { get; set; } = "Varsayılan Kur Marjı";
        
        /// <summary>
        /// Aktif mi
        /// </summary>
        [Display(Name = "Aktif")]
        public bool Aktif { get; set; } = true;
        
        /// <summary>
        /// Soft delete için
        /// </summary>
        [Display(Name = "Silindi")]
        public bool Silindi { get; set; } = false;
        
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
    }
} 
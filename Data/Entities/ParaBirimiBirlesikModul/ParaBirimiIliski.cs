#nullable enable

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MuhasebeStokWebApp.Data.Entities.ParaBirimiBirlesikModul
{
    /// <summary>
    /// Para birimleri arası ilişki entity class'ı
    /// </summary>
    [Table("ParaBirimiIliskileri")]
    public class ParaBirimiIliski
    {
        /// <summary>
        /// Para birimi ilişkisi benzersiz kimliği
        /// </summary>
        [Key]
        public Guid ParaBirimiIliskiID { get; set; } = Guid.NewGuid();
        
        /// <summary>
        /// Kaynak para birimi ID'si
        /// </summary>
        [Required(ErrorMessage = "Kaynak para birimi ID gereklidir.")]
        [Display(Name = "Kaynak Para Birimi")]
        public Guid KaynakParaBirimiID { get; set; }
        
        /// <summary>
        /// Hedef para birimi ID'si
        /// </summary>
        [Required(ErrorMessage = "Hedef para birimi ID gereklidir.")]
        [Display(Name = "Hedef Para Birimi")]
        public Guid HedefParaBirimiID { get; set; }
        
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
        /// Açıklama
        /// </summary>
        [Display(Name = "Açıklama")]
        [StringLength(500, ErrorMessage = "Açıklama en fazla 500 karakter olabilir.")]
        public string Aciklama { get; set; } = string.Empty;
        
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
        /// Kaynak para birimi (dönüştürülecek para birimi)
        /// </summary>
        [ForeignKey("KaynakParaBirimiID")]
        public virtual ParaBirimi? KaynakParaBirimi { get; set; }
        
        /// <summary>
        /// Hedef para birimi (dönüştürülen para birimi)
        /// </summary>
        [ForeignKey("HedefParaBirimiID")]
        public virtual ParaBirimi? HedefParaBirimi { get; set; }
    }
} 
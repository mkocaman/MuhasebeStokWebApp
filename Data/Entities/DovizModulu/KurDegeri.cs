using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MuhasebeStokWebApp.Data.Entities.DovizModulu
{
    /// <summary>
    /// Kur Değeri entity sınıfı
    /// </summary>
    [Table("DovizModuluKurDegerleri")]
    public class KurDegeri
    {
        /// <summary>
        /// Kur değeri benzersiz kimliği
        /// </summary>
        [Key]
        public Guid KurDegeriID { get; set; }
        
        /// <summary>
        /// Para birimi ID'si
        /// </summary>
        [Required]
        [ForeignKey("ParaBirimi")]
        public Guid ParaBirimiID { get; set; }
        
        /// <summary>
        /// Kurun geçerli olduğu tarih
        /// </summary>
        [Required]
        public DateTime Tarih { get; set; }
        
        /// <summary>
        /// Alış değeri
        /// </summary>
        [Required]
        public decimal Alis { get; set; }
        
        /// <summary>
        /// Satış değeri
        /// </summary>
        [Required]
        public decimal Satis { get; set; }
        
        /// <summary>
        /// Aktif mi
        /// </summary>
        public bool Aktif { get; set; } = true;
        
        /// <summary>
        /// Açıklama
        /// </summary>
        public string? Aciklama { get; set; }
        
        /// <summary>
        /// Silindi mi
        /// </summary>
        public bool Silindi { get; set; } = false;
        
        /// <summary>
        /// Oluşturulma tarihi
        /// </summary>
        public DateTime? OlusturmaTarihi { get; set; }
        
        /// <summary>
        /// Son güncellenme tarihi
        /// </summary>
        public DateTime? GuncellemeTarihi { get; set; }
        
        /// <summary>
        /// Oluşturan kullanıcı ID'si
        /// </summary>
        public string? OlusturanKullaniciID { get; set; }
        
        /// <summary>
        /// Son güncelleyen kullanıcı ID'si
        /// </summary>
        public string? SonGuncelleyenKullaniciID { get; set; }
        
        /// <summary>
        /// Kaynak para birimi
        /// </summary>
        public virtual ParaBirimi? ParaBirimi { get; set; }
    }
} 
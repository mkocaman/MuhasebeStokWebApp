using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MuhasebeStokWebApp.Data.Entities.DovizModulu
{
    /// <summary>
    /// Döviz İlişki entity sınıfı
    /// </summary>
    public class DovizIliski
    {
        /// <summary>
        /// Döviz ilişkisi benzersiz kimliği
        /// </summary>
        [Key]
        public Guid DovizIliskiID { get; set; }
        
        /// <summary>
        /// Kaynak para birimi ID'si
        /// </summary>
        [Required]
        [ForeignKey("KaynakParaBirimi")]
        public Guid KaynakParaBirimiID { get; set; }
        
        /// <summary>
        /// Hedef para birimi ID'si
        /// </summary>
        [Required]
        [ForeignKey("HedefParaBirimi")]
        public Guid HedefParaBirimiID { get; set; }
        
        /// <summary>
        /// Oluşturulma tarihi
        /// </summary>
        public DateTime OlusturmaTarihi { get; set; }
        
        /// <summary>
        /// Son güncellenme tarihi
        /// </summary>
        public DateTime? GuncellemeTarihi { get; set; }
        
        /// <summary>
        /// Aktif mi
        /// </summary>
        public bool Aktif { get; set; } = true;
        
        /// <summary>
        /// Soft delete için
        /// </summary>
        public bool Silindi { get; set; } = false;
        
        /// <summary>
        /// Oluşturan kullanıcı ID'si
        /// </summary>
        public string OlusturanKullaniciID { get; set; }
        
        /// <summary>
        /// Son güncelleyen kullanıcı ID'si
        /// </summary>
        public string SonGuncelleyenKullaniciID { get; set; }
        
        /// <summary>
        /// Kaynak para birimi
        /// </summary>
        public virtual ParaBirimi? KaynakParaBirimi { get; set; }
        
        /// <summary>
        /// Hedef para birimi
        /// </summary>
        public virtual ParaBirimi? HedefParaBirimi { get; set; }
    }
} 
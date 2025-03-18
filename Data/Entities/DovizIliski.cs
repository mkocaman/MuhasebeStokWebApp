using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MuhasebeStokWebApp.Data.Entities
{
    /// <summary>
    /// Döviz ilişkilerini tanımlayan entity
    /// </summary>
    [Table("DovizIliskileri")]
    public class DovizIliski
    {
        [Key]
        public int DovizIliskiID { get; set; }
        
        [Required]
        [Display(Name = "Kaynak Para Birimi")]
        public int KaynakParaBirimiID { get; set; }
        
        [ForeignKey("KaynakParaBirimiID")]
        public virtual Doviz KaynakParaBirimi { get; set; }
        
        [Required]
        [Display(Name = "Hedef Para Birimi")]
        public int HedefParaBirimiID { get; set; }
        
        [ForeignKey("HedefParaBirimiID")]
        public virtual Doviz HedefParaBirimi { get; set; }
        
        [Display(Name = "Aktif")]
        public bool Aktif { get; set; } = true;
        
        [Display(Name = "Silinmiş")]
        public bool SoftDelete { get; set; } = false;
        
        [Display(Name = "Oluşturma Tarihi")]
        public DateTime OlusturmaTarihi { get; set; } = DateTime.Now;
        
        [Display(Name = "Güncelleme Tarihi")]
        public DateTime? GuncellemeTarihi { get; set; }
    }
} 
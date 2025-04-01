using System;
using System.ComponentModel.DataAnnotations;

namespace MuhasebeStokWebApp.Models
{
    public class SistemAyari
    {
        [Key]
        public int SistemAyariID { get; set; }
        
        [Required]
        [StringLength(50)]
        [Display(Name = "Şirket Adı")]
        public required string SirketAdi { get; set; }
        
        [Required]
        [StringLength(10)]
        [Display(Name = "Ana Döviz Kodu")]
        public string AnaDovizKodu { get; set; } = "TRY";
        
        [StringLength(100)]
        [Display(Name = "Şirket Adresi")]
        public required string SirketAdresi { get; set; }
        
        [StringLength(20)]
        [Display(Name = "Şirket Telefon")]
        public required string SirketTelefon { get; set; }
        
        [StringLength(100)]
        [Display(Name = "Şirket E-posta")]
        public required string SirketEmail { get; set; }
        
        [StringLength(20)]
        [Display(Name = "Vergi No")]
        public required string SirketVergiNo { get; set; }
        
        [StringLength(20)]
        [Display(Name = "Vergi Dairesi")]
        public required string SirketVergiDairesi { get; set; }
        
        [Display(Name = "Otomatik Döviz Güncelleme")]
        public bool OtomatikDovizGuncelleme { get; set; } = true;
        
        [Display(Name = "Döviz Güncelleme Sıklığı (saat)")]
        public int DovizGuncellemeSikligi { get; set; } = 24;
        
        [Display(Name = "Son Döviz Güncelleme Tarihi")]
        public DateTime SonDovizGuncellemeTarihi { get; set; }
        
        [StringLength(500)]
        [Display(Name = "Aktif Para Birimleri")]
        public required string AktifParaBirimleri { get; set; }
        
        [Display(Name = "Aktif")]
        public bool Aktif { get; set; } = true;
        
        [Display(Name = "Silindi")]
        public bool Silindi { get; set; } = false;
        
        [Display(Name = "Oluşturma Tarihi")]
        public DateTime OlusturmaTarihi { get; set; } = DateTime.Now;
        
        [Display(Name = "Güncelleme Tarihi")]
        public DateTime? GuncellemeTarihi { get; set; }
    }
} 
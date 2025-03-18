using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MuhasebeStokWebApp.Data.Entities
{
    /// <summary>
    /// Sistem ayarları entity'si
    /// </summary>
    [Table("SistemAyarlari")]
    public class SistemAyarlari
    {
        public SistemAyarlari()
        {
            // Default yapıcı metot ile varsayılan değerleri ayarla
            SistemAyarlariID = Guid.NewGuid();
            AnaDovizKodu = "USD";
            IkinciDovizKodu = "UZS";
            UcuncuDovizKodu = "TRY";
            Aktif = true;
            SoftDelete = false;
            OlusturmaTarihi = DateTime.Now;
        }
        
        [Key]
        public int SistemAyarlariID { get; set; }
        
        [Required]
        [Display(Name = "Ana Döviz ID")]
        public int AnaDovizID { get; set; }
        
        [ForeignKey("AnaDovizID")]
        public virtual Doviz AnaDoviz { get; set; }
        
        [Required]
        [StringLength(5)]
        [Display(Name = "Ana Döviz Kodu")]
        public string AnaDovizKodu { get; set; } = "USD";
        
        [Display(Name = "İkinci Döviz ID")]
        public int? IkinciDovizID { get; set; }
        
        [ForeignKey("IkinciDovizID")]
        public virtual Doviz IkinciDoviz { get; set; }
        
        [StringLength(5)]
        [Display(Name = "İkinci Döviz Kodu")]
        public string IkinciDovizKodu { get; set; } = "UZS";
        
        [Display(Name = "Üçüncü Döviz ID")]
        public int? UcuncuDovizID { get; set; }
        
        [ForeignKey("UcuncuDovizID")]
        public virtual Doviz UcuncuDoviz { get; set; }
        
        [StringLength(5)]
        [Display(Name = "Üçüncü Döviz Kodu")]
        public string UcuncuDovizKodu { get; set; } = "TRY";
        
        [Required]
        [StringLength(100)]
        [Display(Name = "Şirket Adı")]
        public string SirketAdi { get; set; } = "Şirket";
        
        [StringLength(250)]
        [Display(Name = "Şirket Adresi")]
        public string SirketAdresi { get; set; } = "";
        
        [StringLength(20)]
        [Display(Name = "Şirket Telefon")]
        public string SirketTelefon { get; set; } = "";
        
        [StringLength(100)]
        [EmailAddress]
        [Display(Name = "Şirket E-posta")]
        public string SirketEmail { get; set; } = "";
        
        [StringLength(20)]
        [Display(Name = "Vergi No")]
        public string SirketVergiNo { get; set; } = "";
        
        [StringLength(100)]
        [Display(Name = "Vergi Dairesi")]
        public string SirketVergiDairesi { get; set; } = "";
        
        [Display(Name = "Otomatik Döviz Güncelleme")]
        public bool OtomatikDovizGuncelleme { get; set; } = true;
        
        [Display(Name = "Döviz Güncelleme Sıklığı (Saat)")]
        public int DovizGuncellemeSikligi { get; set; } = 24;
        
        [Display(Name = "Son Döviz Güncelleme Tarihi")]
        public DateTime SonDovizGuncellemeTarihi { get; set; } = DateTime.Now;
        
        [StringLength(500)]
        [Display(Name = "Aktif Para Birimleri")]
        public string? AktifParaBirimleri { get; set; } = "USD,EUR,TRY,UZS,GBP";
        
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
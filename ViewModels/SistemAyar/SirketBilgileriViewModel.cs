using System.ComponentModel.DataAnnotations;

namespace MuhasebeStokWebApp.ViewModels.SistemAyar
{
    public class SirketBilgileriViewModel
    {
        [Required(ErrorMessage = "Şirket adı zorunludur")]
        [StringLength(100, ErrorMessage = "Şirket adı 100 karakterden uzun olamaz")]
        [Display(Name = "Şirket Adı")]
        public string SirketAdi { get; set; }

        [Required(ErrorMessage = "Şirket adresi zorunludur")]
        [StringLength(200, ErrorMessage = "Şirket adresi 200 karakterden uzun olamaz")]
        [Display(Name = "Şirket Adresi")]
        public string SirketAdresi { get; set; }
        
        [Required(ErrorMessage = "Şirket telefonu zorunludur")]
        [StringLength(20, ErrorMessage = "Şirket telefonu 20 karakterden uzun olamaz")]
        [Display(Name = "Telefon")]
        public string SirketTelefon { get; set; }
        
        [Required(ErrorMessage = "E-posta adresi zorunludur")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz")]
        [StringLength(100, ErrorMessage = "E-posta 100 karakterden uzun olamaz")]
        [Display(Name = "E-posta")]
        public string SirketEmail { get; set; }
        
        [Required(ErrorMessage = "Vergi numarası zorunludur")]
        [StringLength(20, ErrorMessage = "Vergi numarası 20 karakterden uzun olamaz")]
        [Display(Name = "Vergi Numarası")]
        public string SirketVergiNo { get; set; }
        
        [Required(ErrorMessage = "Vergi dairesi zorunludur")]
        [StringLength(50, ErrorMessage = "Vergi dairesi 50 karakterden uzun olamaz")]
        [Display(Name = "Vergi Dairesi")]
        public string SirketVergiDairesi { get; set; }
        
        [Display(Name = "Logo URL")]
        [StringLength(500, ErrorMessage = "Logo URL 500 karakterden uzun olamaz")]
        public string LogoUrl { get; set; }
        
        [Display(Name = "Logo Göster")]
        public bool LogoGoster { get; set; } = true;
    }
} 
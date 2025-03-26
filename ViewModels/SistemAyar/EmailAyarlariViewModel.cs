using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MuhasebeStokWebApp.ViewModels.SistemAyar
{
    public class EmailAyarlariViewModel
    {
        [Required(ErrorMessage = "SMTP sunucu adresi gereklidir")]
        [Display(Name = "SMTP Sunucu")]
        public string SmtpServer { get; set; } = "smtp.gmail.com";
        
        [Required(ErrorMessage = "SMTP port numarası gereklidir")]
        [Range(1, 65535, ErrorMessage = "Port numarası 1-65535 arasında olmalıdır")]
        [Display(Name = "SMTP Port")]
        public int SmtpPort { get; set; } = 587;
        
        [Required(ErrorMessage = "Kullanıcı adı gereklidir")]
        [Display(Name = "Kullanıcı Adı")]
        public string Username { get; set; } = "";
        
        [Display(Name = "Şifre")]
        public string Password { get; set; } = "";
        
        [Required(ErrorMessage = "Gönderen e-posta adresi gereklidir")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz")]
        [Display(Name = "Gönderen E-posta")]
        public string SenderEmail { get; set; } = "";
        
        [Required(ErrorMessage = "Gönderen adı gereklidir")]
        [Display(Name = "Gönderen Adı")]
        public string SenderName { get; set; } = "Muhasebe Stok Sistemi";
        
        [Display(Name = "SSL Kullan")]
        public bool UseSsl { get; set; } = true;
        
        [Display(Name = "Yönetici E-postaları (Virgülle Ayırın)")]
        public string AdminEmails { get; set; } = "";
    }
} 
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace MuhasebeStokWebApp.ViewModels.Account
{
    public class ProfileViewModel
    {
        public string Id { get; set; }
        
        [Display(Name = "Kullanıcı Adı")]
        public string UserName { get; set; }
        
        [Required(ErrorMessage = "E-posta adresi zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        [Display(Name = "E-posta")]
        public string Email { get; set; }
        
        [Display(Name = "Telefon Numarası")]
        [Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz.")]
        public string PhoneNumber { get; set; }
        
        [Display(Name = "Ad Soyad")]
        [MaxLength(100, ErrorMessage = "Ad Soyad en fazla 100 karakter olabilir.")]
        public string FullName { get; set; }
        
        [Display(Name = "Hakkımda")]
        [MaxLength(500, ErrorMessage = "Biyografi en fazla 500 karakter olabilir.")]
        public string Bio { get; set; }
        
        [Display(Name = "Profil Resmi")]
        public string ProfileImage { get; set; }
        
        [Display(Name = "Profil Fotoğrafı")]
        public IFormFile ProfilePhoto { get; set; }
        
        [Display(Name = "Roller")]
        public List<string> Roles { get; set; } = new List<string>();
        
        // Son giriş zamanı - isteğe bağlı
        [Display(Name = "Son Giriş")]
        public DateTime? LastLoginTime { get; set; }
    }
} 
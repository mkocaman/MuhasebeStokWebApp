using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MuhasebeStokWebApp.ViewModels.Kullanici
{
    public class KullaniciViewModel
    {
        [Required]
        public required string Id { get; set; }
        
        [Required]
        [Display(Name = "Kullanıcı Adı")]
        public required string KullaniciAdi { get; set; }
        
        [Required]
        [Display(Name = "E-posta")]
        public required string Email { get; set; }
        
        [Required]
        [Display(Name = "Roller")]
        public required string Roller { get; set; }
    }

    public class KullaniciCreateViewModel
    {
        [Required(ErrorMessage = "Kullanıcı adı zorunludur.")]
        [Display(Name = "Kullanıcı Adı")]
        public required string KullaniciAdi { get; set; }
        
        [Required(ErrorMessage = "E-posta adresi zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        [Display(Name = "E-posta")]
        public required string Email { get; set; }
        
        [Required(ErrorMessage = "Şifre zorunludur.")]
        [StringLength(100, ErrorMessage = "Şifre en az {2} karakter uzunluğunda olmalıdır.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Şifre")]
        public required string Password { get; set; }
        
        [Required(ErrorMessage = "Şifre tekrarı zorunludur.")]
        [DataType(DataType.Password)]
        [Display(Name = "Şifre Tekrar")]
        [Compare("Password", ErrorMessage = "Şifreler eşleşmiyor.")]
        public required string ConfirmPassword { get; set; }
        
        [Display(Name = "Roller")]
        public required List<string> SelectedRoles { get; set; } = new List<string>();
    }

    public class KullaniciEditViewModel
    {
        [Required]
        public required string Id { get; set; }
        
        [Required(ErrorMessage = "Kullanıcı adı zorunludur.")]
        [Display(Name = "Kullanıcı Adı")]
        public required string KullaniciAdi { get; set; }
        
        [Required(ErrorMessage = "E-posta adresi zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        [Display(Name = "E-posta")]
        public required string Email { get; set; }
        
        [StringLength(100, ErrorMessage = "Şifre en az {2} karakter uzunluğunda olmalıdır.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Şifre")]
        public string? Password { get; set; }
        
        [DataType(DataType.Password)]
        [Display(Name = "Şifre Tekrar")]
        [Compare("Password", ErrorMessage = "Şifreler eşleşmiyor.")]
        public string? ConfirmPassword { get; set; }
        
        [Display(Name = "Roller")]
        public required List<string> SelectedRoles { get; set; } = new List<string>();
    }

    public class RoleViewModel
    {
        [Required]
        public required string Id { get; set; }
        
        [Required(ErrorMessage = "Rol adı zorunludur.")]
        [Display(Name = "Rol Adı")]
        public required string RolAdi { get; set; }
    }
} 
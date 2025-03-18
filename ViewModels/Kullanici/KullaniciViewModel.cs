using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MuhasebeStokWebApp.ViewModels.Kullanici
{
    public class KullaniciViewModel
    {
        public string Id { get; set; }
        
        [Display(Name = "Kullanıcı Adı")]
        public string KullaniciAdi { get; set; }
        
        [Display(Name = "E-posta")]
        public string Email { get; set; }
        
        [Display(Name = "Roller")]
        public string Roller { get; set; }
    }

    public class KullaniciCreateViewModel
    {
        [Required(ErrorMessage = "Kullanıcı adı zorunludur.")]
        [Display(Name = "Kullanıcı Adı")]
        public string KullaniciAdi { get; set; }
        
        [Required(ErrorMessage = "E-posta adresi zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        [Display(Name = "E-posta")]
        public string Email { get; set; }
        
        [Required(ErrorMessage = "Şifre zorunludur.")]
        [StringLength(100, ErrorMessage = "Şifre en az {2} karakter uzunluğunda olmalıdır.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Şifre")]
        public string Sifre { get; set; }
        
        [DataType(DataType.Password)]
        [Display(Name = "Şifre Tekrar")]
        [Compare("Sifre", ErrorMessage = "Şifreler eşleşmiyor.")]
        public string SifreTekrar { get; set; }
        
        [Display(Name = "Roller")]
        public List<string> SecilenRoller { get; set; }
    }

    public class KullaniciEditViewModel
    {
        public string Id { get; set; }
        
        [Required(ErrorMessage = "Kullanıcı adı zorunludur.")]
        [Display(Name = "Kullanıcı Adı")]
        public string KullaniciAdi { get; set; }
        
        [Required(ErrorMessage = "E-posta adresi zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        [Display(Name = "E-posta")]
        public string Email { get; set; }
        
        [StringLength(100, ErrorMessage = "Şifre en az {2} karakter uzunluğunda olmalıdır.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Şifre")]
        public string Sifre { get; set; }
        
        [DataType(DataType.Password)]
        [Display(Name = "Şifre Tekrar")]
        [Compare("Sifre", ErrorMessage = "Şifreler eşleşmiyor.")]
        public string SifreTekrar { get; set; }
        
        [Display(Name = "Roller")]
        public List<string> SecilenRoller { get; set; }
    }

    public class RoleViewModel
    {
        public string Id { get; set; }
        
        [Required(ErrorMessage = "Rol adı zorunludur.")]
        [Display(Name = "Rol Adı")]
        public string RolAdi { get; set; }
    }
} 
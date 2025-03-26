using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using MuhasebeStokWebApp.Data.Entities;

namespace MuhasebeStokWebApp.ViewModels.Menu
{
    public class MenuCreateViewModel
    {
        [Required(ErrorMessage = "Menü adı gereklidir.")]
        [StringLength(100, ErrorMessage = "Menü adı en fazla 100 karakter olabilir.")]
        [Display(Name = "Menü Adı")]
        public string Ad { get; set; }
        
        [StringLength(100, ErrorMessage = "Icon ismi en fazla 100 karakter olabilir.")]
        [Display(Name = "Icon Kodu")]
        public string Icon { get; set; }
        
        [StringLength(255, ErrorMessage = "URL en fazla 255 karakter olabilir.")]
        [Display(Name = "URL (Harici Link İçin)")]
        public string Url { get; set; }
        
        [StringLength(255, ErrorMessage = "Controller adı en fazla 255 karakter olabilir.")]
        [Display(Name = "Controller")]
        public string Controller { get; set; }
        
        [StringLength(255, ErrorMessage = "Action adı en fazla 255 karakter olabilir.")]
        [Display(Name = "Action")]
        public string Action { get; set; }
        
        [Display(Name = "Aktif")]
        public bool AktifMi { get; set; } = true;
        
        [Display(Name = "Sıra")]
        public int Sira { get; set; }
        
        [Display(Name = "Üst Menü")]
        public Guid? UstMenuID { get; set; }
        
        [Display(Name = "Üst Menü Adı")]
        public string UstMenuAdi { get; set; }
        
        [Display(Name = "Roller")]
        public List<string> SeciliRoller { get; set; } = new List<string>();
        
        public IEnumerable<IdentityRole> Roles { get; set; }
    }
} 
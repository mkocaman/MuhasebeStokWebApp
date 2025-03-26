using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using MuhasebeStokWebApp.Enums;

namespace MuhasebeStokWebApp.ViewModels.Menu
{
    public class MenuViewModel
    {
        public Guid MenuID { get; set; }
        
        [Display(Name = "Menü Adı")]
        public string Ad { get; set; }
        
        // MenuAdi Ad'ın alternatif adıdır
        [Display(Name = "Menü Adı")]
        public string MenuAdi => Ad;
        
        [Display(Name = "Icon")]
        public string Icon { get; set; }
        
        [Display(Name = "URL")]
        public string Url { get; set; }
        
        // URL'nin alternatif adı
        [Display(Name = "URL")]
        public string URL => Url;
        
        [Display(Name = "Controller")]
        public string Controller { get; set; }
        
        [Display(Name = "Action")]
        public string Action { get; set; }
        
        [Display(Name = "Aktif Mi?")]
        public bool AktifMi { get; set; }
        
        // AktifMi'nin alternatif adı
        [Display(Name = "Aktif")]
        public bool Aktif => AktifMi;
        
        [Display(Name = "Sıra")]
        public int Sira { get; set; }
        
        [Display(Name = "Üst Menü")]
        public Guid? UstMenuID { get; set; }
        
        [Display(Name = "Alt Menüler")]
        public List<MenuViewModel> AltMenuler { get; set; } = new List<MenuViewModel>();
        
        [Display(Name = "Roller")]
        public List<string> Roller { get; set; } = new List<string>();
        
        // Navigation properties için kullanılan ekstra özellikler
        [Display(Name = "Üst Menü Adı")]
        public string UstMenuAdi { get; set; }
        
        // Menü Tipi
        [Display(Name = "Menü Tipi")]
        public int MenuTipi { get; set; }
        
        // Görüntü özellikleri
        public int Level { get; set; }
        public bool HasChildren => AltMenuler != null && AltMenuler.Count > 0;
        
        // Menünün aktif olup olmadığını belirler (mevcut controller/action ile eşleşiyorsa)
        public bool Active { get; set; }
    }
} 
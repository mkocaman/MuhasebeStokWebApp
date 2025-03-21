using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using MuhasebeStokWebApp.Data.Entities;

namespace MuhasebeStokWebApp.ViewModels.ParaBirimi
{
    public class ParaBirimiIliskiViewModel
    {
        public Guid ParaBirimiIliskiID { get; set; }
        
        // IliskiID property'si ekleniyor - ParaBirimiIliskiID ile aynı amaca hizmet ediyor
        public Guid IliskiID { 
            get { return ParaBirimiIliskiID; } 
            set { ParaBirimiIliskiID = value; } 
        }
        
        [Required(ErrorMessage = "Kaynak para birimi seçilmelidir.")]
        [Display(Name = "Kaynak Para Birimi")]
        public Guid KaynakParaBirimiID { get; set; }
        
        [Required(ErrorMessage = "Hedef para birimi seçilmelidir.")]
        [Display(Name = "Hedef Para Birimi")]
        public Guid HedefParaBirimiID { get; set; }
        
        [Required(ErrorMessage = "Çevrim katsayısı girilmelidir.")]
        [Range(0.000001, double.MaxValue, ErrorMessage = "Çevrim katsayısı pozitif bir değer olmalıdır.")]
        [Display(Name = "Çevrim Katsayısı")]
        public decimal CevrimKatsayisi { get; set; }
        
        [Display(Name = "Açıklama")]
        public string Aciklama { get; set; }
        
        [Display(Name = "Aktif")]
        public bool Aktif { get; set; } = true;
        
        [Display(Name = "Oluşturma Tarihi")]
        public DateTime OlusturmaTarihi { get; set; }
        
        [Display(Name = "Güncelleme Tarihi")]
        public DateTime? GuncellemeTarihi { get; set; }
        
        // Gösterim özellikleri
        [Display(Name = "Kaynak Para Birimi")]
        public string KaynakParaBirimiKodu { get; set; }
        
        [Display(Name = "Hedef Para Birimi")]
        public string HedefParaBirimiKodu { get; set; }
        
        public string KaynakParaBirimiAdi { get; set; }
        public string HedefParaBirimiAdi { get; set; }
        
        // ParaBirimiController.cs için gereken özellikler
        public decimal Carpan { get; set; }
        public List<SelectListItem> HedefParaBirimleri { get; set; }
        
        // Dropdown için para birimi listesi
        public List<MuhasebeStokWebApp.Data.Entities.ParaBirimi> ParaBirimiListesi { get; set; }
    }
} 
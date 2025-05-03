using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MuhasebeStokWebApp.ViewModels.Urun
{
    public class UrunEditViewModel
    {
        public Guid UrunID { get; set; }
        
        [Required(ErrorMessage = "Ürün kodu zorunludur.")]
        [StringLength(50, ErrorMessage = "Ürün kodu en fazla 50 karakter olabilir.")]
        [Display(Name = "Ürün Kodu")]
        public string UrunKodu { get; set; }
        
        [Required(ErrorMessage = "Ürün adı zorunludur.")]
        [StringLength(200, ErrorMessage = "Ürün adı en fazla 200 karakter olabilir.")]
        [Display(Name = "Ürün Adı")]
        public string UrunAdi { get; set; }
        
        [Required(ErrorMessage = "Birim seçimi zorunludur.")]
        [Display(Name = "Birim")]
        public Guid? BirimID { get; set; }
        
        [Display(Name = "Birim")]
        public string Birim { get; set; }
        
        [Display(Name = "Açıklama")]
        public string Aciklama { get; set; }
        
        [Display(Name = "Liste Fiyatı")]
        public decimal ListeFiyati { get; set; }
        
        [Display(Name = "Maliyet Fiyatı")]
        public decimal MaliyetFiyati { get; set; }
        
        [Display(Name = "Satış Fiyatı")]
        public decimal SatisFiyati { get; set; }
        
        [Display(Name = "Liste Fiyatı (USD)")]
        public decimal? DovizliListeFiyati { get; set; }
        
        [Display(Name = "Maliyet Fiyatı (USD)")]
        public decimal? DovizliMaliyetFiyati { get; set; }
        
        [Display(Name = "Satış Fiyatı (USD)")]
        public decimal? DovizliSatisFiyati { get; set; }
        
        [Display(Name = "KDV Oranı")]
        [Range(0, 100, ErrorMessage = "KDV oranı 0-100 arasında bir değer olmalıdır")]
        public decimal KDVOrani { get; set; }
        
        [Display(Name = "Aktif")]
        public bool Aktif { get; set; }
        
        [Display(Name = "Kategori")]
        public Guid? KategoriID { get; set; }
        
        [Display(Name = "Oluşturma Tarihi")]
        public DateTime? OlusturmaTarihi { get; set; }
        
        [Display(Name = "Güncelleme Tarihi")]
        public DateTime? GuncellemeTarihi { get; set; }
        
        // Dropdown listeler için yeni özellikler
        public List<SelectListItem> Birimler { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> Kategoriler { get; set; } = new List<SelectListItem>();
        
        // Geriye uyumluluk için mevcut özellikleri koruyoruz
        public List<SelectListItem> BirimListesi { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> KategoriListesi { get; set; } = new List<SelectListItem>();
    }
} 
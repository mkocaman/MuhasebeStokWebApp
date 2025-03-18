using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MuhasebeStokWebApp.ViewModels.Urun
{
    public class UrunCreateViewModel
    {
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
        
        [Display(Name = "Stok Miktarı")]
        [Range(0, double.MaxValue, ErrorMessage = "Stok miktarı 0'dan küçük olamaz.")]
        public decimal StokMiktar { get; set; }
        
        [Required(ErrorMessage = "Liste fiyatı zorunludur.")]
        [Range(0, double.MaxValue, ErrorMessage = "Liste fiyatı 0'dan küçük olamaz.")]
        [Display(Name = "Liste Fiyatı")]
        public decimal ListeFiyati { get; set; }
        
        [Required(ErrorMessage = "Maliyet fiyatı zorunludur.")]
        [Range(0, double.MaxValue, ErrorMessage = "Maliyet fiyatı 0'dan küçük olamaz.")]
        [Display(Name = "Maliyet Fiyatı")]
        public decimal MaliyetFiyati { get; set; }
        
        [Required(ErrorMessage = "Satış fiyatı zorunludur.")]
        [Range(0, double.MaxValue, ErrorMessage = "Satış fiyatı 0'dan küçük olamaz.")]
        [Display(Name = "Satış Fiyatı")]
        public decimal SatisFiyati { get; set; }
        
        [Display(Name = "Aktif")]
        public bool Aktif { get; set; } = true;
        
        [Display(Name = "Kategori")]
        public Guid? KategoriID { get; set; }
        
        // Dropdown listeler için - zorunlu değil
        public List<SelectListItem> BirimListesi { get; set; }
        public List<SelectListItem> KategoriListesi { get; set; }
    }
} 
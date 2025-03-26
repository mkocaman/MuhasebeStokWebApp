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
        
        [Display(Name = "Stok Miktarı")]
        [Range(0, double.MaxValue, ErrorMessage = "Stok miktarı 0'dan küçük olamaz.")]
        public decimal StokMiktar { get; set; }
        
        [Display(Name = "Aktif")]
        public bool Aktif { get; set; }
        
        [Display(Name = "KDV Oranı (%)")]
        [Range(0, 100, ErrorMessage = "KDV oranı 0-100 arasında olmalıdır.")]
        public int KDVOrani { get; set; } = 18;
        
        [Display(Name = "Kategori")]
        public Guid? KategoriID { get; set; }
        
        // Dropdown listeler için - zorunlu değil
        public List<SelectListItem> BirimListesi { get; set; }
        public List<SelectListItem> KategoriListesi { get; set; }
    }
} 
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MuhasebeStokWebApp.ViewModels.Urun
{
    public class UrunViewModel
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
        
        [StringLength(500, ErrorMessage = "Açıklama en fazla 500 karakter olabilir.")]
        [Display(Name = "Açıklama")]
        public string Aciklama { get; set; }
        
        [StringLength(50, ErrorMessage = "Birim en fazla 50 karakter olabilir.")]
        [Display(Name = "Birim")]
        public string Birim { get; set; }
        
        [Display(Name = "Birim ID")]
        public Guid? BirimID { get; set; }
        
        [Display(Name = "Birim Adı")]
        public string BirimAdi { get; set; }
        
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
        public int KdvOrani { get; set; }
        
        [Display(Name = "Kritik Stok Seviyesi")]
        public decimal KritikStokSeviyesi { get; set; }
        
        [Display(Name = "Aktif")]
        public bool Aktif { get; set; }
        
        [Display(Name = "Silindi")]
        public bool Silindi { get; set; }
        
        [Display(Name = "Kategori")]
        public Guid? KategoriID { get; set; }
        
        [Display(Name = "Kategori Adı")]
        public string KategoriAdi { get; set; }
        
        // Stok miktarı
        public decimal StokMiktari { get; set; }
        
        // Miktar, geriye dönük uyumluluk için eklendi
        [Display(Name = "Miktar")]
        public decimal Miktar { get; set; }
        
        [Display(Name = "Oluşturma Tarihi")]
        public DateTime? OlusturmaTarihi { get; set; }
        
        [Display(Name = "Güncelleme Tarihi")]
        public DateTime? GuncellemeTarihi { get; set; }
    }
    
    public class UrunListViewModel
    {
        public List<UrunViewModel> Urunler { get; set; } = new List<UrunViewModel>();
        
        // Filtreleme için parametreler
        [Display(Name = "Kategori")]
        public Guid? KategoriID { get; set; }
        
        [Display(Name = "Ürün Adı")]
        public string UrunAdi { get; set; }
        
        [Display(Name = "Ürün Kodu")]
        public string UrunKodu { get; set; }
        
        [Display(Name = "Aktif")]
        public bool? AktifMi { get; set; }
        
        [Display(Name = "Aktif Sekme")]
        public string AktifTab { get; set; } = "aktif";
        
        // Dropdown listesi için kategori listesi
        public List<SelectListItem> Kategoriler { get; set; } = new List<SelectListItem>();
    }
} 
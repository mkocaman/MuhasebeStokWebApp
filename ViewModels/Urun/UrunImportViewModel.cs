using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MuhasebeStokWebApp.ViewModels.Urun
{
    public class UrunImportViewModel
    {
        [Display(Name = "Seçili")]
        public bool Secili { get; set; }
        
        [Display(Name = "Ürün Kodu")]
        public string UrunKodu { get; set; }
        
        [Display(Name = "Ürün Adı")]
        public string UrunAdi { get; set; }
        
        [Display(Name = "Kategori")]
        public string KategoriAdi { get; set; }
        
        [Display(Name = "Birim")]
        public string BirimAdi { get; set; }
        
        [Display(Name = "KDV Oranı")]
        public int KDVOrani { get; set; }
        
        [Display(Name = "Liste Fiyatı")]
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal ListeFiyati { get; set; }
        
        [Display(Name = "Maliyet Fiyatı")]
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal MaliyetFiyati { get; set; }
        
        [Display(Name = "Satış Fiyatı")]
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal SatisFiyati { get; set; }
        
        [Display(Name = "Aktif")]
        public bool Aktif { get; set; }
        
        [Display(Name = "Mevcut")]
        public bool MevcutMu { get; set; }
        
        [Display(Name = "Mevcut Ürün Bilgisi")]
        public string MevcutUrunBilgisi { get; set; }
    }
    
    public class UrunImportListViewModel
    {
        public List<UrunImportViewModel> Urunler { get; set; } = new List<UrunImportViewModel>();
    }
} 
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using MuhasebeStokWebApp.Data.Entities;
using MuhasebeStokWebApp.Enums;

namespace MuhasebeStokWebApp.ViewModels.Stok
{
    public class StokGirisViewModel
    {
        [Required(ErrorMessage = "Ürün seçilmelidir.")]
        [Display(Name = "Ürün")]
        public Guid UrunID { get; set; }

        [Required(ErrorMessage = "Depo seçilmelidir.")]
        [Display(Name = "Depo")]
        public Guid DepoID { get; set; }

        [Required(ErrorMessage = "Miktar girilmelidir.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Miktar 0'dan büyük olmalıdır.")]
        [Display(Name = "Miktar")]
        public decimal Miktar { get; set; }

        [Display(Name = "Birim")]
        public string Birim { get; set; }

        [Required(ErrorMessage = "Birim fiyat girilmelidir.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Birim fiyat 0'dan büyük olmalıdır.")]
        [Display(Name = "Birim Fiyat")]
        public decimal BirimFiyat { get; set; }

        [Display(Name = "Para Birimi")]
        public string ParaBirimi { get; set; } = "TRY";

        [Display(Name = "Döviz Kuru")]
        public decimal? DovizKuru { get; set; } = 1;

        [Display(Name = "Tarih")]
        [DataType(DataType.Date)]
        public DateTime Tarih { get; set; } = DateTime.Now;

        [Display(Name = "Hareket Türü")]
        public StokHareketTipi HareketTuru { get; set; } = StokHareketTipi.Giris;

        [Display(Name = "Referans No")]
        public string ReferansNo { get; set; }

        [Display(Name = "Referans Türü")]
        public string ReferansTuru { get; set; }

        [Display(Name = "Açıklama")]
        public string Aciklama { get; set; }
        
        // ViewBag'i değiştirmek için eklenen özellikler
        public List<SelectListItem> Urunler { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> Depolar { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> Birimler { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> ParaBirimleri { get; set; } = new List<SelectListItem>();
        
        // Ürünlerin birim bilgilerini saklamak için
        public Dictionary<string, string> UrunBirimBilgileri { get; set; } = new Dictionary<string, string>();
    }
} 
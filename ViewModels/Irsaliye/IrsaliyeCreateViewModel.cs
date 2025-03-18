using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MuhasebeStokWebApp.ViewModels.Irsaliye
{
    public class IrsaliyeCreateViewModel
    {
        [Required(ErrorMessage = "İrsaliye numarası zorunludur.")]
        [StringLength(20, ErrorMessage = "İrsaliye numarası en fazla 20 karakter olabilir.")]
        [Display(Name = "İrsaliye No")]
        public string IrsaliyeNumarasi { get; set; }

        [Required(ErrorMessage = "İrsaliye tarihi zorunludur.")]
        [Display(Name = "İrsaliye Tarihi")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime IrsaliyeTarihi { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Sevk tarihi zorunludur.")]
        [Display(Name = "Sevk Tarihi")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime SevkTarihi { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Cari seçimi zorunludur.")]
        [Display(Name = "Cari")]
        public Guid CariID { get; set; }

        [Required(ErrorMessage = "İrsaliye türü zorunludur.")]
        [Display(Name = "İrsaliye Türü")]
        public string IrsaliyeTuru { get; set; } = "Çıkış";

        [Display(Name = "Fatura")]
        public Guid? FaturaID { get; set; }

        [StringLength(500, ErrorMessage = "Açıklama en fazla 500 karakter olabilir.")]
        [Display(Name = "Açıklama")]
        public string Aciklama { get; set; }

        [Display(Name = "Durum")]
        public string Durum { get; set; } = "Açık";
        
        [Display(Name = "Aktif")]
        public bool Aktif { get; set; } = true;

        public List<IrsaliyeKalemViewModel> IrsaliyeKalemleri { get; set; } = new List<IrsaliyeKalemViewModel>();
        
        // Dropdown listeler için - sadece UI'da kullanılır
        [Display(Name = "Cari Listesi")]
        public List<SelectListItem> CariListesi { get; set; }
        
        [Display(Name = "İrsaliye Türü Listesi")]
        public List<SelectListItem> IrsaliyeTuruListesi { get; set; }
        
        [Display(Name = "Fatura Listesi")]
        public List<SelectListItem> FaturaListesi { get; set; }
    }

    public class IrsaliyeKalemViewModel
    {
        public Guid? KalemID { get; set; }

        [Required(ErrorMessage = "Ürün seçimi zorunludur.")]
        [Display(Name = "Ürün")]
        public Guid UrunID { get; set; }

        [Display(Name = "Ürün Adı")]
        public string UrunAdi { get; set; }
        
        [Display(Name = "Ürün Kodu")]
        public string UrunKodu { get; set; }

        [Required(ErrorMessage = "Miktar zorunludur.")]
        [Range(0.01, 9999999, ErrorMessage = "Miktar 0'dan büyük olmalıdır.")]
        [Display(Name = "Miktar")]
        public decimal Miktar { get; set; }

        [Display(Name = "Birim")]
        public string Birim { get; set; }

        [StringLength(500, ErrorMessage = "Açıklama en fazla 500 karakter olabilir.")]
        [Display(Name = "Açıklama")]
        public string Aciklama { get; set; }
    }
} 
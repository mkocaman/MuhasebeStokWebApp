using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MuhasebeStokWebApp.ViewModels.Fatura
{
    public class FaturaCreateViewModel
    {
        [StringLength(20, ErrorMessage = "Fatura numarası en fazla 20 karakter olabilir.")]
        [Display(Name = "Fatura No")]
        public string? FaturaNumarasi { get; set; }

        [StringLength(20, ErrorMessage = "Sipariş numarası en fazla 20 karakter olabilir.")]
        [Display(Name = "Sipariş Numarası")]
        public string? SiparisNumarasi { get; set; }

        [Required(ErrorMessage = "Fatura tarihi zorunludur.")]
        [Display(Name = "Fatura Tarihi")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime FaturaTarihi { get; set; } = DateTime.Today;

        [Display(Name = "Vade Tarihi")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? VadeTarihi { get; set; } = DateTime.Today.AddDays(30);

        [Required(ErrorMessage = "Cari seçilmelidir.")]
        [Display(Name = "Cari")]
        public Guid CariID { get; set; }

        [Display(Name = "Fatura Türü")]
        public int? FaturaTuruID { get; set; }

        [Display(Name = "Fatura Türü")]
        public string? FaturaTuru { get; set; }

        [Display(Name = "Resmi")]
        public bool Resmi { get; set; } = true;

        [StringLength(500, ErrorMessage = "Açıklama en fazla 500 karakter olabilir.")]
        [Display(Name = "Açıklama")]
        public string? Aciklama { get; set; }

        [Display(Name = "Ödeme Durumu")]
        public string? OdemeDurumu { get; set; } = "Beklemede";

        [Display(Name = "Döviz Türü")]
        public string? DovizTuru { get; set; } = "TRY";

        [Display(Name = "Döviz Kuru")]
        [DisplayFormat(DataFormatString = "{0:N4}", ApplyFormatInEditMode = false)]
        public decimal? DovizKuru { get; set; } = 1;
        
        [Display(Name = "Aktif")]
        public bool Aktif { get; set; } = true;

        [Display(Name = "Otomatik İrsaliye Oluştur")]
        public bool OtomatikIrsaliyeOlustur { get; set; } = false;

        [Display(Name = "İrsaliye")]
        public Guid? IrsaliyeID { get; set; }

        public List<FaturaKalemViewModel> FaturaKalemleri { get; set; } = new List<FaturaKalemViewModel>();
        
        // Dropdown listeler için - sadece UI'da kullanılır
        [Display(Name = "Cari Listesi")]
        public List<SelectListItem> CariListesi { get; set; } = new List<SelectListItem>();
        
        [Display(Name = "Fatura Türü Listesi")]
        public List<SelectListItem> FaturaTuruListesi { get; set; } = new List<SelectListItem>();
        
        [Display(Name = "Döviz Listesi")]
        public List<SelectListItem> DovizListesi { get; set; } = new List<SelectListItem>();
    }

    public class FaturaKalemViewModel
    {
        public Guid? KalemID { get; set; }

        [Required(ErrorMessage = "Ürün seçimi zorunludur.")]
        [Display(Name = "Ürün")]
        public Guid UrunID { get; set; }

        [Display(Name = "Ürün Adı")]
        public string UrunAdi { get; set; } = string.Empty;
        
        [Display(Name = "Ürün Kodu")]
        public string UrunKodu { get; set; } = string.Empty;

        [Required(ErrorMessage = "Miktar zorunludur.")]
        [Range(0.01, 9999999, ErrorMessage = "Miktar 0'dan büyük olmalıdır.")]
        [Display(Name = "Miktar")]
        public decimal Miktar { get; set; }

        [Display(Name = "Birim")]
        public string Birim { get; set; } = string.Empty;

        [Required(ErrorMessage = "Birim fiyat zorunludur.")]
        [Range(0.01, 9999999, ErrorMessage = "Birim fiyat 0'dan büyük olmalıdır.")]
        [Display(Name = "Birim Fiyat")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = false)]
        public decimal BirimFiyat { get; set; }
        
        [Display(Name = "Açıklama")]
        public string Aciklama { get; set; } = string.Empty;

        [Display(Name = "KDV Oranı (%)")]
        [Range(0, 100, ErrorMessage = "KDV oranı 0-100 arasında olmalıdır.")]
        public int KdvOrani { get; set; } = 18;

        [Display(Name = "İndirim Oranı (%)")]
        [Range(0, 100, ErrorMessage = "İndirim oranı 0-100 arasında olmalıdır.")]
        public int IndirimOrani { get; set; } = 0;

        [Display(Name = "Tutar")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = false)]
        public decimal Tutar { get; set; }

        [Display(Name = "KDV Tutarı")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = false)]
        public decimal KdvTutari { get; set; }

        [Display(Name = "İndirim Tutarı")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = false)]
        public decimal IndirimTutari { get; set; }

        [Display(Name = "Net Tutar")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = false)]
        public decimal NetTutar { get; set; }
    }
} 
using System;
using System.ComponentModel.DataAnnotations;

namespace MuhasebeStokWebApp.ViewModels.Fatura
{
    public class FaturaKalemViewModel
    {
        public Guid FaturaKalemID { get; set; }
        public Guid FaturaID { get; set; }

        [Required(ErrorMessage = "Ürün seçilmelidir.")]
        [Display(Name = "Ürün")]
        public Guid UrunID { get; set; }

        [Display(Name = "Ürün Kodu")]
        public string UrunKodu { get; set; } = "";

        [Display(Name = "Ürün Adı")]
        public string UrunAdi { get; set; } = "";

        [Required(ErrorMessage = "Miktar girilmelidir.")]
        [Display(Name = "Miktar")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Miktar 0'dan büyük olmalıdır.")]
        public decimal Miktar { get; set; }

        [Required(ErrorMessage = "Birim fiyat girilmelidir.")]
        [Display(Name = "Birim Fiyat")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Birim fiyat 0'dan büyük olmalıdır.")]
        public decimal BirimFiyat { get; set; }

        [Required(ErrorMessage = "KDV oranı girilmelidir.")]
        [Display(Name = "KDV Oranı (%)")]
        [Range(0, 100, ErrorMessage = "KDV oranı 0-100 arasında olmalıdır.")]
        public decimal KdvOrani { get; set; }

        [Display(Name = "İndirim Oranı (%)")]
        [Range(0, 100, ErrorMessage = "İndirim oranı 0-100 arasında olmalıdır.")]
        public decimal IndirimOrani { get; set; }

        [Display(Name = "Tutar")]
        public decimal Tutar { get; set; }

        [Display(Name = "KDV Tutarı")]
        public decimal KdvTutari { get; set; }

        [Display(Name = "İndirim Tutarı")]
        public decimal IndirimTutari { get; set; }

        [Display(Name = "Net Tutar")]
        public decimal NetTutar { get; set; }
        
        // Dövizli değerler - Para birimi USD ise UZS değerler, UZS ise USD değerler
        [Display(Name = "Birim Fiyat (Döviz)")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = false)]
        public decimal BirimFiyatDoviz { get; set; }
        
        [Display(Name = "Tutar (Döviz)")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = false)]
        public decimal TutarDoviz { get; set; }
        
        [Display(Name = "KDV Tutarı (Döviz)")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = false)]
        public decimal KdvTutariDoviz { get; set; }
        
        [Display(Name = "İndirim Tutarı (Döviz)")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = false)]
        public decimal IndirimTutariDoviz { get; set; }
        
        [Display(Name = "Net Tutar (Döviz)")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = false)]
        public decimal NetTutarDoviz { get; set; }

        [Display(Name = "Birim")]
        public string Birim { get; set; } = "Adet";
        
        [Display(Name = "Açıklama")]
        [StringLength(200)]
        public string Aciklama { get; set; } = "";

        public bool Silindi { get; set; }
        public Guid KalemID { get; set; }
    }
} 
using System;
using System.ComponentModel.DataAnnotations;

namespace MuhasebeStokWebApp.ViewModels.Fatura
{
    public class FaturaOdemeViewModel
    {
        public Guid OdemeID { get; set; }
        
        public Guid FaturaID { get; set; }

        [Display(Name = "Fatura Numarası")]
        public string FaturaNumarasi { get; set; } = "";

        [Display(Name = "Cari Adı")]
        public string CariAdi { get; set; } = "";

        [Display(Name = "Genel Toplam")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = false)]
        public decimal GenelToplam { get; set; }

        [Display(Name = "Ödenen Tutar")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = false)]
        public decimal OdenenTutar { get; set; }

        [Display(Name = "Kalan Tutar")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Tutar 0'dan büyük olmalıdır.")]
        public decimal KalanTutar { get; set; }

        [Required(ErrorMessage = "Ödeme türü seçilmelidir.")]
        [Range(1, int.MaxValue, ErrorMessage = "Ödeme türü seçilmelidir.")]
        [Display(Name = "Ödeme Türü")]
        public int OdemeTuruID { get; set; }
        
        [Display(Name = "Ödeme Türü")]
        public string OdemeTuru { get; set; } = "";

        [Required(ErrorMessage = "Ödeme tutarı girilmelidir.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Ödeme tutarı 0'dan büyük olmalıdır.")]
        [Display(Name = "Ödeme Tutarı")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = false)]
        public decimal Tutar { get; set; }
        
        [Display(Name = "Ödeme Tutarı")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = false)]
        public decimal OdemeTutari { get; set; }

        [Required(ErrorMessage = "Ödeme tarihi girilmelidir.")]
        [Display(Name = "Ödeme Tarihi")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy}", ApplyFormatInEditMode = true)]
        public DateTime OdemeTarihi { get; set; } = DateTime.Now;

        [Display(Name = "Fatura Tarihi")]
        public DateTime? FaturaTarihi { get; set; }

        [Display(Name = "Döviz Kodu")]
        public string? DovizKodu { get; set; }

        [Display(Name = "Döviz Kuru")]
        public decimal? DovizKuru { get; set; }

        [Display(Name = "TL Karşılığı")]
        public decimal? TLKarsiligi { get; set; }

        [Display(Name = "Açıklama")]
        [StringLength(500, ErrorMessage = "Açıklama en fazla 500 karakter olabilir.")]
        public string Aciklama { get; set; } = "";
    }
} 
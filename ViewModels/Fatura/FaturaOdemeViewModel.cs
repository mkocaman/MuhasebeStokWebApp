using System;
using System.ComponentModel.DataAnnotations;

namespace MuhasebeStokWebApp.ViewModels.Fatura
{
    public class FaturaOdemeViewModel
    {
        public Guid OdemeID { get; set; }
        
        public Guid FaturaID { get; set; }

        [Display(Name = "Fatura No")]
        public string FaturaNumarasi { get; set; }

        [Display(Name = "Cari Adı")]
        public string CariAdi { get; set; }

        [Display(Name = "Genel Toplam")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = false)]
        public decimal GenelToplam { get; set; }

        [Display(Name = "Ödenen Tutar")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = false)]
        public decimal OdenenTutar { get; set; }

        [Display(Name = "Kalan Tutar")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = false)]
        public decimal KalanTutar { get; set; }

        [Required(ErrorMessage = "Ödeme türü seçilmelidir.")]
        [Range(1, int.MaxValue, ErrorMessage = "Ödeme türü seçilmelidir.")]
        [Display(Name = "Ödeme Türü")]
        public int OdemeTuruID { get; set; }
        
        [Display(Name = "Ödeme Türü")]
        public string OdemeTuru { get; set; }

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
        public DateTime OdemeTarihi { get; set; }

        [Display(Name = "Açıklama")]
        [StringLength(500, ErrorMessage = "Açıklama en fazla 500 karakter olabilir.")]
        public string Aciklama { get; set; }
    }
} 
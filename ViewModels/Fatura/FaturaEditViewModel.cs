using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MuhasebeStokWebApp.ViewModels.Fatura
{
    public class FaturaEditViewModel
    {
        public Guid FaturaID { get; set; }

        [Required(ErrorMessage = "Fatura numarası zorunludur.")]
        [StringLength(20, ErrorMessage = "Fatura numarası en fazla 20 karakter olabilir.")]
        [Display(Name = "Fatura No")]
        public string FaturaNumarasi { get; set; }

        [StringLength(20, ErrorMessage = "Sipariş numarası en fazla 20 karakter olabilir.")]
        [Display(Name = "Sipariş Numarası")]
        public string SiparisNumarasi { get; set; }

        [Required(ErrorMessage = "Fatura tarihi zorunludur.")]
        [Display(Name = "Fatura Tarihi")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? FaturaTarihi { get; set; }

        [Display(Name = "Vade Tarihi")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? VadeTarihi { get; set; }

        [Required(ErrorMessage = "Cari seçilmelidir.")]
        [Display(Name = "Cari")]
        public Guid CariID { get; set; }

        [Display(Name = "Cari Adı")]
        public string CariAdi { get; set; }

        [Display(Name = "Fatura Türü")]
        public int? FaturaTuruID { get; set; }

        [Display(Name = "Fatura Türü")]
        public string FaturaTuru { get; set; }

        [Display(Name = "Resmi")]
        public bool Resmi { get; set; } = true;

        [StringLength(500, ErrorMessage = "Açıklama en fazla 500 karakter olabilir.")]
        [Display(Name = "Açıklama")]
        public string Aciklama { get; set; }

        [Display(Name = "Ödeme Durumu")]
        public string OdemeDurumu { get; set; }

        [Display(Name = "Döviz Türü")]
        public string DovizTuru { get; set; } = "TRY";

        [Display(Name = "Döviz Kuru")]
        [DisplayFormat(DataFormatString = "{0:N4}", ApplyFormatInEditMode = false)]
        public decimal? DovizKuru { get; set; } = 1;
        
        [Display(Name = "Aktif")]
        public bool Aktif { get; set; } = true;

        public List<FaturaKalemViewModel> FaturaKalemleri { get; set; } = new List<FaturaKalemViewModel>();
        
        // Dropdown listeler için
        public List<SelectListItem> CariListesi { get; set; }
        public List<SelectListItem> FaturaTuruListesi { get; set; }
        public List<SelectListItem> DovizListesi { get; set; }
    }
} 
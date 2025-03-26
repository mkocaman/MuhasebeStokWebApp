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
        public required string FaturaNumarasi { get; set; }

        [StringLength(20, ErrorMessage = "Sipariş numarası en fazla 20 karakter olabilir.")]
        [Display(Name = "Sipariş Numarası")]
        public required string SiparisNumarasi { get; set; }

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
        public required string CariAdi { get; set; }

        [Display(Name = "Fatura Türü")]
        public int? FaturaTuruID { get; set; }

        [Display(Name = "Fatura Türü")]
        public required string FaturaTuru { get; set; }

        [Display(Name = "Resmi")]
        public bool Resmi { get; set; } = true;

        [StringLength(500, ErrorMessage = "Açıklama en fazla 500 karakter olabilir.")]
        [Display(Name = "Açıklama")]
        public string? Aciklama { get; set; }

        [Display(Name = "Ödeme Durumu")]
        public required string OdemeDurumu { get; set; }

        [Display(Name = "Döviz Türü")]
        public string DovizTuru { get; set; } = "TRY";

        [Display(Name = "Döviz Kuru")]
        [DisplayFormat(DataFormatString = "{0:N4}", ApplyFormatInEditMode = false)]
        public decimal? DovizKuru { get; set; } = 1;
        
        [Display(Name = "Aktif")]
        public bool Aktif { get; set; } = true;

        [Display(Name = "İrsaliye")]
        public Guid? IrsaliyeID { get; set; }

        public List<FaturaKalemViewModel> FaturaKalemleri { get; set; } = new List<FaturaKalemViewModel>();
        
        // Dropdown listeler için
        public required List<SelectListItem> CariListesi { get; set; }
        public required List<SelectListItem> FaturaTuruListesi { get; set; }
        public required List<SelectListItem> DovizListesi { get; set; }
        
        public FaturaEditViewModel()
        {
            CariListesi = new List<SelectListItem>();
            FaturaTuruListesi = new List<SelectListItem>();
            DovizListesi = new List<SelectListItem>();
        }
    }
} 
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MuhasebeStokWebApp.ViewModels.Irsaliye
{
    public class IrsaliyeEditViewModel
    {
        public Guid IrsaliyeID { get; set; }

        [Required(ErrorMessage = "İrsaliye numarası zorunludur.")]
        [StringLength(20, ErrorMessage = "İrsaliye numarası en fazla 20 karakter olabilir.")]
        [Display(Name = "İrsaliye No")]
        public string IrsaliyeNumarasi { get; set; }

        [Required(ErrorMessage = "İrsaliye tarihi zorunludur.")]
        [Display(Name = "İrsaliye Tarihi")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? IrsaliyeTarihi { get; set; }

        [Required(ErrorMessage = "Sevk tarihi zorunludur.")]
        [Display(Name = "Sevk Tarihi")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? SevkTarihi { get; set; }

        [Required(ErrorMessage = "Cari seçimi zorunludur.")]
        [Display(Name = "Cari")]
        public Guid CariID { get; set; }
        
        [Display(Name = "Cari Adı")]
        public string CariAdi { get; set; }

        [Required(ErrorMessage = "İrsaliye türü zorunludur.")]
        [Display(Name = "İrsaliye Türü")]
        public string IrsaliyeTuru { get; set; }

        [Display(Name = "Fatura")]
        public Guid? FaturaID { get; set; }
        
        [Display(Name = "Fatura No")]
        public string FaturaNumarasi { get; set; }

        [StringLength(500, ErrorMessage = "Açıklama en fazla 500 karakter olabilir.")]
        [Display(Name = "Açıklama")]
        public string Aciklama { get; set; }

        [Display(Name = "Durum")]
        public string Durum { get; set; }
        
        [Display(Name = "Aktif")]
        public bool Aktif { get; set; } = true;

        public List<IrsaliyeKalemViewModel> IrsaliyeKalemleri { get; set; } = new List<IrsaliyeKalemViewModel>();
        
        // Dropdown listeler için
        public List<SelectListItem> CariListesi { get; set; }
        public List<SelectListItem> IrsaliyeTuruListesi { get; set; }
        public List<SelectListItem> FaturaListesi { get; set; }
    }
} 
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using MuhasebeStokWebApp.ViewModels.Shared;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MuhasebeStokWebApp.ViewModels.Irsaliye
{
    public class IrsaliyeEditViewModel
    {
        public Guid IrsaliyeID { get; set; }

        [Required(ErrorMessage = "İrsaliye numarası gereklidir.")]
        [StringLength(50, ErrorMessage = "İrsaliye numarası 50 karakterden uzun olamaz.")]
        [DisplayName("İrsaliye Numarası")]
        public string IrsaliyeNumarasi { get; set; }

        [Required(ErrorMessage = "İrsaliye tarihi gereklidir.")]
        [DisplayName("İrsaliye Tarihi")]
        [DataType(DataType.Date)]
        public DateTime IrsaliyeTarihi { get; set; }

        [Required(ErrorMessage = "Cari seçimi gereklidir.")]
        [DisplayName("Cari")]
        public Guid CariID { get; set; }

        [DisplayName("Fatura")]
        public Guid? FaturaID { get; set; }

        [DisplayName("Açıklama")]
        [StringLength(200, ErrorMessage = "Açıklama 200 karakterden uzun olamaz.")]
        public string Aciklama { get; set; }

        [DisplayName("Resmi")]
        public bool? Resmi { get; set; }
        
        [DisplayName("İrsaliye Türü")]
        public string IrsaliyeTuru { get; set; }
        
        [DisplayName("Durum")]
        public string Durum { get; set; }

        [DisplayName("İrsaliye Detayları")]
        public List<IrsaliyeDetayViewModel> Detaylar { get; set; } = new List<IrsaliyeDetayViewModel>();
        
        // Dropdown listeler
        public SelectList CariListesi { get; set; }
        public SelectList UrunListesi { get; set; }
    }
} 
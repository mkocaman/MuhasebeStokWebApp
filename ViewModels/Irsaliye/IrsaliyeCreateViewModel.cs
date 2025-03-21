using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using MuhasebeStokWebApp.ViewModels.Shared;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MuhasebeStokWebApp.ViewModels.Irsaliye
{
    public class IrsaliyeCreateViewModel
    {
        [Required(ErrorMessage = "İrsaliye numarası gereklidir.")]
        [StringLength(50, ErrorMessage = "İrsaliye numarası 50 karakterden uzun olamaz.")]
        [DisplayName("İrsaliye Numarası")]
        public string IrsaliyeNumarasi { get; set; }

        [Required(ErrorMessage = "İrsaliye tarihi gereklidir.")]
        [DisplayName("İrsaliye Tarihi")]
        [DataType(DataType.Date)]
        public DateTime IrsaliyeTarihi { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Cari seçimi gereklidir.")]
        [DisplayName("Cari")]
        public Guid CariID { get; set; }

        [DisplayName("Fatura")]
        public Guid? FaturaID { get; set; }

        [DisplayName("Açıklama")]
        [StringLength(200, ErrorMessage = "Açıklama 200 karakterden uzun olamaz.")]
        public string Aciklama { get; set; }

        [DisplayName("Resmi")]
        public bool? Resmi { get; set; } = true;

        [DisplayName("İrsaliye Detayları")]
        public List<IrsaliyeDetayViewModel> Detaylar { get; set; } = new List<IrsaliyeDetayViewModel>();
        
        [DisplayName("İrsaliye Türü")]
        public string IrsaliyeTuru { get; set; } = "Standart";
        
        [DisplayName("Durum")]
        public string Durum { get; set; } = "Açık";
        
        // Dropdown listeler
        public SelectList CariListesi { get; set; }
        public SelectList UrunListesi { get; set; }
    }
} 
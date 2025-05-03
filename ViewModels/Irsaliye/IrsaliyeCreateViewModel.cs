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
        [Required]
        [StringLength(20)]
        [Display(Name = "İrsaliye No")]
        public string IrsaliyeNumarasi { get; set; }

        [Required]
        [Display(Name = "İrsaliye Tarihi")]
        public DateTime IrsaliyeTarihi { get; set; }

        [Required]
        [Display(Name = "Cari")]
        public Guid CariID { get; set; }

        [Display(Name = "Fatura")]
        public Guid? FaturaID { get; set; }

        [Display(Name = "Depo")]
        public Guid? DepoID { get; set; }

        [StringLength(500)]
        [Display(Name = "Açıklama")]
        public string? Aciklama { get; set; }

        [Required]
        [Display(Name = "İrsaliye Türü")]
        public string IrsaliyeTuru { get; set; }

        [Display(Name = "Toplam Tutar")]
        public decimal ToplamTutar { get; set; }

        public bool Aktif { get; set; }

        public SelectList? CariListesi { get; set; }
        public SelectList? UrunListesi { get; set; }
        public SelectList? DepoListesi { get; set; }
        public List<IrsaliyeDetayViewModel> IrsaliyeDetaylari { get; set; }

        public IrsaliyeCreateViewModel()
        {
            IrsaliyeTarihi = DateTime.Now;
            IrsaliyeTuru = "Satis";
            Aktif = true;
            IrsaliyeDetaylari = new List<IrsaliyeDetayViewModel>();
        }
    }
} 
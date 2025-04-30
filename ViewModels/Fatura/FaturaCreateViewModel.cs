using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using MuhasebeStokWebApp.ViewModels.Fatura;
using Microsoft.AspNetCore.Mvc.Rendering;
using MuhasebeStokWebApp.Data.Entities;

namespace MuhasebeStokWebApp.ViewModels.Fatura
{
    public class FaturaCreateViewModel
    {
        public FaturaCreateViewModel()
        {
            FaturaKalemleri = new List<FaturaKalemViewModel>();
            CariListesi = new List<SelectListItem>();
            FaturaTuruListesi = new List<SelectListItem>();
            DovizListesi = new List<SelectListItem>();
        }

        public Guid FaturaID { get; set; }

        [Required(ErrorMessage = "Fatura numarası girilmelidir.")]
        [StringLength(20, ErrorMessage = "Fatura numarası en fazla 20 karakter olabilir.")]
        [Display(Name = "Fatura Numarası")]
        public string FaturaNumarasi { get; set; }

        [Required(ErrorMessage = "Sipariş numarası girilmelidir.")]
        [StringLength(20, ErrorMessage = "Sipariş numarası en fazla 20 karakter olabilir.")]
        [Display(Name = "Sipariş Numarası")]
        public string SiparisNumarasi { get; set; }

        [Required(ErrorMessage = "Fatura tarihi girilmelidir.")]
        [Display(Name = "Fatura Tarihi")]
        public DateTime? FaturaTarihi { get; set; }

        [Display(Name = "Vade Tarihi")]
        public DateTime? VadeTarihi { get; set; }

        [Required(ErrorMessage = "Cari seçilmelidir.")]
        [Display(Name = "Cari")]
        public Guid CariID { get; set; }

        [Required(ErrorMessage = "Cari adı girilmelidir.")]
        [Display(Name = "Cari Adı")]
        public string CariAdi { get; set; }

        [Display(Name = "Fatura Türü")]
        public int? FaturaTuruID { get; set; }

        [Required(ErrorMessage = "Fatura türü seçilmelidir.")]
        [Display(Name = "Fatura Türü")]
        public string FaturaTuru { get; set; }

        [Display(Name = "Resmi")]
        public bool ResmiMi { get; set; } = true;

        [StringLength(500, ErrorMessage = "Açıklama en fazla 500 karakter olabilir.")]
        [Display(Name = "Açıklama")]
        public string? Aciklama { get; set; }

        [StringLength(500, ErrorMessage = "Fatura notu en fazla 500 karakter olabilir.")]
        [Display(Name = "Fatura Notu")]
        public string FaturaNotu { get; set; } = "";

        [Required(ErrorMessage = "Ödeme durumu seçilmelidir.")]
        [Display(Name = "Ödeme Durumu")]
        public string OdemeDurumu { get; set; }

        [Display(Name = "Döviz Türü")]
        public string DovizTuru { get; set; } = "USD";

        [Display(Name = "Döviz Kuru")]
        public decimal? DovizKuru { get; set; } = 1;

        [Display(Name = "Aktif")]
        public bool Aktif { get; set; } = true;

        public Guid? IrsaliyeID { get; set; }
        public Guid? SozlesmeID { get; set; }
        
        [Display(Name = "Depo")]
        public Guid? DepoID { get; set; }

        [Display(Name = "Otomatik İrsaliye Oluştur")]
        public bool OtomatikIrsaliyeOlustur { get; set; } = false;

        [Display(Name = "Ara Toplam")]
        public decimal AraToplam { get; set; }

        [Display(Name = "KDV Toplam")]
        public decimal KdvToplam { get; set; }
        
        [Display(Name = "İndirim Tutarı")]
        public decimal IndirimTutari { get; set; }

        [Display(Name = "Genel Toplam")]
        public decimal GenelToplam { get; set; }
        
        // Dövizli toplam değerler
        [Display(Name = "Ara Toplam (Döviz)")]
        public decimal AraToplamDoviz { get; set; }

        [Display(Name = "KDV Toplam (Döviz)")]
        public decimal KdvToplamDoviz { get; set; }

        [Display(Name = "Genel Toplam (Döviz)")]
        public decimal GenelToplamDoviz { get; set; }

        public List<FaturaKalemViewModel> FaturaKalemleri { get; set; }
        public List<SelectListItem> CariListesi { get; set; }
        public List<SelectListItem> FaturaTuruListesi { get; set; }
        public List<SelectListItem> DovizListesi { get; set; }
    }
} 
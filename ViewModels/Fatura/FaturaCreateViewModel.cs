using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using MuhasebeStokWebApp.ViewModels.Fatura;
using Microsoft.AspNetCore.Mvc.Rendering;
using MuhasebeStokWebApp.Data.Entities;

namespace MuhasebeStokWebApp.ViewModels.Fatura
{
    // ParaBirimi sınıfını burada tanımlayalım (eksik referansı gidermek için)
    public class ParaBirimi
    {
        public string Kod { get; set; }
        public string Ad { get; set; }
        public string Sembol { get; set; }
        public bool AnaParaBirimiMi { get; set; }
    }

    // FaturaDetayViewModel sınıfını burada tanımlayalım (eksik referansı gidermek için)
    public class FaturaDetayViewModel
    {
        public Guid? FaturaDetayID { get; set; }
        public Guid? FaturaID { get; set; }
        public Guid? UrunID { get; set; }
        public string UrunAdi { get; set; }
        public decimal Miktar { get; set; }
        public string Birim { get; set; }
        public decimal BirimFiyat { get; set; }
        public decimal KDVOrani { get; set; }
        public decimal KDVTutari { get; set; }
        public decimal AraToplam { get; set; }
        public decimal GenelToplam { get; set; }
        public string Aciklama { get; set; }
    }

    public class FaturaCreateViewModel
    {
        public FaturaCreateViewModel()
        {
            FaturaKalemleri = new List<FaturaKalemViewModel>();
            CariListesi = new List<SelectListItem>();
            FaturaTuruListesi = new List<SelectListItem>();
            DovizListesi = new List<SelectListItem>();
        }

        public Guid? FaturaID { get; set; }

        [Required(ErrorMessage = "Fatura numarası zorunludur.")]
        [Display(Name = "Fatura Numarası")]
        [StringLength(50, ErrorMessage = "Fatura numarası en fazla 50 karakter olabilir.")]
        public string FaturaNumarasi { get; set; }

        [Required(ErrorMessage = "Sipariş numarası zorunludur.")]
        [Display(Name = "Sipariş Numarası")]
        [StringLength(50, ErrorMessage = "Sipariş numarası en fazla 50 karakter olabilir.")]
        public string SiparisNumarasi { get; set; }

        [Required(ErrorMessage = "Fatura tarihi zorunludur.")]
        [Display(Name = "Fatura Tarihi")]
        [DataType(DataType.Date)]
        public DateTime FaturaTarihi { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Vade tarihi zorunludur.")]
        [Display(Name = "Vade Tarihi")]
        [DataType(DataType.Date)]
        public DateTime VadeTarihi { get; set; } = DateTime.Today.AddDays(30);

        [Required(ErrorMessage = "Cari seçimi zorunludur.")]
        [Display(Name = "Cari")]
        public Guid? CariID { get; set; }

        [Display(Name = "Sözleşme")]
        public Guid? SozlesmeID { get; set; }

        [Required(ErrorMessage = "Fatura türü zorunludur.")]
        [Display(Name = "Fatura Türü")]
        public Guid? FaturaTuruID { get; set; }

        [Display(Name = "Fatura Notu")]
        [StringLength(500, ErrorMessage = "Fatura notu en fazla 500 karakter olabilir.")]
        public string FaturaNotu { get; set; }

        [Display(Name = "Açıklama")]
        [StringLength(500, ErrorMessage = "Açıklama en fazla 500 karakter olabilir.")]
        public string Aciklama { get; set; }

        [Display(Name = "Ara Toplam")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = false)]
        public decimal? AraToplam { get; set; }

        [Display(Name = "KDV Toplam")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = false)]
        public decimal? KDVToplam { get; set; }

        [Display(Name = "Genel Toplam")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = false)]
        public decimal? GenelToplam { get; set; }

        [Display(Name = "Ödeme Durumu")]
        public string OdemeDurumu { get; set; } = "Ödenmedi";

        [Display(Name = "Döviz Türü")]
        public string DovizTuru { get; set; } = "TRY";

        [Display(Name = "Döviz Kuru")]
        [DisplayFormat(DataFormatString = "{0:N4}", ApplyFormatInEditMode = false)]
        public decimal? DovizKuru { get; set; } = 1;

        [Display(Name = "Resmi mi?")]
        public bool ResmiMi { get; set; } = true;

        [Display(Name = "Otomatik irsaliye oluştur")]
        public bool OtomatikIrsaliyeOlustur { get; set; } = true;

        [Display(Name = "Depo")]
        public Guid? DepoID { get; set; }

        // Fatura Detay bilgileri
        [Display(Name = "Fatura Detayları")]
        public List<FaturaDetayViewModel> FaturaDetaylari { get; set; } = new List<FaturaDetayViewModel>();

        // ViewBag yerine ViewModel'de saklanacak veriler
        public List<SelectListItem> Cariler { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> Sozlesmeler { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> FaturaTurleri { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> Urunler { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> Depolar { get; set; } = new List<SelectListItem>();
        public List<ParaBirimi> ParaBirimleri { get; set; } = new List<ParaBirimi>();
        public Dictionary<string, string> UrunBirimBilgileri { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, string> CariParaBirimleri { get; set; } = new Dictionary<string, string>();
        public bool UrunListesiBosMu { get; set; } = false;

        public List<FaturaKalemViewModel> FaturaKalemleri { get; set; }
        public List<SelectListItem> CariListesi { get; set; }
        public List<SelectListItem> FaturaTuruListesi { get; set; }
        public List<SelectListItem> DovizListesi { get; set; }
    }
} 
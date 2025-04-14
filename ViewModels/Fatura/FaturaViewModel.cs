using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MuhasebeStokWebApp.ViewModels.Fatura
{
    public class FaturaViewModel
    {
        [Display(Name = "ID")]
        public string FaturaID { get; set; }

        [Display(Name = "Fatura No")]
        [Required(ErrorMessage = "Fatura numarası zorunludur.")]
        public required string FaturaNumarasi { get; set; }

        [Display(Name = "Fatura Tarihi")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? FaturaTarihi { get; set; }

        [Display(Name = "Vade Tarihi")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? VadeTarihi { get; set; }

        [Display(Name = "Cari")]
        public Guid CariID { get; set; }

        [Display(Name = "Cari Adı")]
        [Required(ErrorMessage = "Cari adı zorunludur.")]
        public required string CariAdi { get; set; }
        
        [Display(Name = "Cari Silindi")]
        public bool CariSilindi { get; set; }

        [Display(Name = "Fatura Türü")]
        [Required(ErrorMessage = "Fatura türü zorunludur.")]
        public required string FaturaTuru { get; set; }
        
        [Display(Name = "Fatura Türü Adı")]
        public string FaturaTuruAdi { get; set; } = string.Empty;

        [Display(Name = "Ara Toplam")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = false)]
        public decimal AraToplam { get; set; }

        [Display(Name = "KDV Tutarı")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = false)]
        public decimal KdvTutari { get; set; }
        
        [Display(Name = "KDV Toplam")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = false)]
        public decimal KDVToplam { get; set; }

        [Display(Name = "İndirim Tutarı")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = false)]
        public decimal IndirimTutari { get; set; }

        [Display(Name = "Genel Toplam")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = false)]
        public decimal GenelToplam { get; set; }
        
        // Dövizli toplam değerler
        [Display(Name = "Ara Toplam (Döviz)")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = false)]
        public decimal AraToplamDoviz { get; set; }

        [Display(Name = "KDV Tutarı (Döviz)")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = false)]
        public decimal KdvTutariDoviz { get; set; }
        
        [Display(Name = "KDV Toplam (Döviz)")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = false)]
        public decimal KDVToplamDoviz { get; set; }

        [Display(Name = "İndirim Tutarı (Döviz)")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = false)]
        public decimal IndirimTutariDoviz { get; set; }

        [Display(Name = "Genel Toplam (Döviz)")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = false)]
        public decimal GenelToplamDoviz { get; set; }

        [Display(Name = "Ödenecek Tutar")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = false)]
        public decimal OdenecekTutar { get; set; }

        [Display(Name = "Açıklama")]
        [Required(ErrorMessage = "Açıklama zorunludur.")]
        public required string Aciklama { get; set; }

        [Display(Name = "Ödeme Durumu")]
        [Required(ErrorMessage = "Ödeme durumu zorunludur.")]
        public required string OdemeDurumu { get; set; }
        
        [Display(Name = "Döviz Türü")]
        public string DovizTuru { get; set; } = "USD";
        
        [Display(Name = "Döviz Kuru")]
        [DisplayFormat(DataFormatString = "{0:N4}", ApplyFormatInEditMode = false)]
        public decimal DovizKuru { get; set; } = 1;
        
        [Display(Name = "Resmi Mi")]
        public bool ResmiMi { get; set; } = true;

        [Display(Name = "Aktif")]
        public bool Aktif { get; set; } = true;

        [Display(Name = "Oluşturma Tarihi")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm}", ApplyFormatInEditMode = false)]
        public DateTime? OlusturmaTarihi { get; set; }

        [Display(Name = "Güncelleme Tarihi")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm}", ApplyFormatInEditMode = false)]
        public DateTime? GuncellemeTarihi { get; set; }
    }
    
    public class FaturaListViewModel
    {
        public Guid FaturaID { get; set; }
        
        [Display(Name = "Fatura No")]
        public string FaturaNumarasi { get; set; } = string.Empty;
        
        [Display(Name = "Fatura Tarihi")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy}", ApplyFormatInEditMode = false)]
        public DateTime? FaturaTarihi { get; set; }
        
        [Display(Name = "Cari Adı")]
        public string CariAdi { get; set; } = string.Empty;
        
        [Display(Name = "Fatura Türü")]
        public string FaturaTuru { get; set; } = string.Empty;
        
        [Display(Name = "Genel Toplam")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = false)]
        public decimal GenelToplam { get; set; }
        
        [Display(Name = "Ödeme Durumu")]
        public string OdemeDurumu { get; set; } = string.Empty;
        
        [Display(Name = "Döviz Türü")]
        public string DovizTuru { get; set; } = "USD";
        
        [Display(Name = "Döviz Kuru")]
        [DisplayFormat(DataFormatString = "{0:N4}", ApplyFormatInEditMode = false)]
        public decimal DovizKuru { get; set; } = 1;
        
        [Display(Name = "Aktif")]
        public bool Aktif { get; set; } = true;
    }
    
    public class FaturaIndexViewModel
    {
        public List<FaturaViewModel> Faturalar { get; set; } = new List<FaturaViewModel>();
    }
} 
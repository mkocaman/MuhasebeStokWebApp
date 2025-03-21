using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MuhasebeStokWebApp.ViewModels.Fatura
{
    public class FaturaDetailViewModel
    {
        public Guid FaturaID { get; set; }

        [Display(Name = "Fatura No")]
        public required string FaturaNumarasi { get; set; }

        [Display(Name = "Fatura Tarihi")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy}", ApplyFormatInEditMode = false)]
        public DateTime? FaturaTarihi { get; set; }

        [Display(Name = "Vade Tarihi")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy}", ApplyFormatInEditMode = false)]
        public DateTime? VadeTarihi { get; set; }

        [Display(Name = "Cari ID")]
        public Guid CariID { get; set; }

        [Display(Name = "İrsaliye ID")]
        public Guid? IrsaliyeID { get; set; }

        [Display(Name = "Cari Adı")]
        public required string CariAdi { get; set; }

        [Display(Name = "Vergi No")]
        public required string CariVergiNo { get; set; }

        [Display(Name = "Adres")]
        public required string CariAdres { get; set; }

        [Display(Name = "Telefon")]
        public required string CariTelefon { get; set; }

        [Display(Name = "Fatura Türü")]
        public required string FaturaTuru { get; set; }

        [Display(Name = "Ara Toplam")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = false)]
        public decimal AraToplam { get; set; }

        [Display(Name = "KDV Tutarı")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = false)]
        public decimal KdvTutari { get; set; }

        [Display(Name = "İndirim Tutarı")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = false)]
        public decimal IndirimTutari { get; set; }

        [Display(Name = "Genel Toplam")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = false)]
        public decimal GenelToplam { get; set; }

        [Display(Name = "Ödenecek Tutar")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = false)]
        public decimal OdenecekTutar { get; set; }

        [Display(Name = "Ödenen Tutar")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = false)]
        public decimal? OdenenTutar { get; set; }

        [Display(Name = "Açıklama")]
        public string? Aciklama { get; set; }

        [Display(Name = "Ödeme Durumu")]
        public required string OdemeDurumu { get; set; }
        
        [Display(Name = "Döviz Türü")]
        public string DovizTuru { get; set; } = "TRY";
        
        [Display(Name = "Aktif")]
        public bool Aktif { get; set; }

        [Display(Name = "Oluşturma Tarihi")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm}", ApplyFormatInEditMode = false)]
        public DateTime? OlusturmaTarihi { get; set; }

        [Display(Name = "Güncelleme Tarihi")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm}", ApplyFormatInEditMode = false)]
        public DateTime? GuncellemeTarihi { get; set; }

        public List<FaturaKalemDetailViewModel> FaturaKalemleri { get; set; } = new List<FaturaKalemDetailViewModel>();
        
        public List<OdemeViewModel> Odemeler { get; set; } = new List<OdemeViewModel>();
    }

    public class FaturaKalemDetailViewModel
    {
        public Guid KalemID { get; set; }

        [Display(Name = "Ürün ID")]
        public Guid UrunID { get; set; }

        [Display(Name = "Ürün Kodu")]
        public string UrunKodu { get; set; } = string.Empty;

        [Display(Name = "Ürün Adı")]
        public string UrunAdi { get; set; } = string.Empty;

        [Display(Name = "Miktar")]
        public decimal Miktar { get; set; }

        [Display(Name = "Birim")]
        public string Birim { get; set; } = string.Empty;

        [Display(Name = "Birim Fiyat")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = false)]
        public decimal BirimFiyat { get; set; }

        [Display(Name = "KDV Oranı (%)")]
        public int KdvOrani { get; set; }

        [Display(Name = "İndirim Oranı (%)")]
        public int IndirimOrani { get; set; }

        [Display(Name = "Tutar")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = false)]
        public decimal Tutar { get; set; }

        [Display(Name = "KDV Tutarı")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = false)]
        public decimal KdvTutari { get; set; }

        [Display(Name = "İndirim Tutarı")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = false)]
        public decimal IndirimTutari { get; set; }

        [Display(Name = "Net Tutar")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = false)]
        public decimal NetTutar { get; set; }
    }

    public class OdemeViewModel
    {
        public Guid OdemeID { get; set; }

        [Display(Name = "Ödeme Tarihi")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy}", ApplyFormatInEditMode = false)]
        public DateTime? OdemeTarihi { get; set; }

        [Display(Name = "Ödeme Tutarı")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = false)]
        public decimal OdemeTutari { get; set; }

        [Display(Name = "Ödeme Türü")]
        public required string OdemeTuru { get; set; }

        [Display(Name = "Açıklama")]
        public string? Aciklama { get; set; }
    }
} 
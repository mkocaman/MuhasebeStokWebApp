using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MuhasebeStokWebApp.ViewModels.Irsaliye
{
    public class IrsaliyeDetailViewModel
    {
        public Guid IrsaliyeID { get; set; }

        [Display(Name = "İrsaliye No")]
        public string IrsaliyeNumarasi { get; set; }

        [Display(Name = "İrsaliye Tarihi")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? IrsaliyeTarihi { get; set; }

        [Display(Name = "Sevk Tarihi")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? SevkTarihi { get; set; }

        [Display(Name = "Cari")]
        public Guid CariID { get; set; }

        [Display(Name = "Cari Adı")]
        public string CariAdi { get; set; }

        [Display(Name = "Vergi No")]
        public string CariVergiNo { get; set; }

        [Display(Name = "Telefon")]
        public string CariTelefon { get; set; }

        [Display(Name = "Adres")]
        public string CariAdres { get; set; }

        [Display(Name = "İrsaliye Türü")]
        public string IrsaliyeTuru { get; set; }

        [Display(Name = "Fatura No")]
        public string FaturaNumarasi { get; set; }

        [Display(Name = "Fatura")]
        public Guid? FaturaID { get; set; }

        [Display(Name = "Açıklama")]
        public string Aciklama { get; set; }

        [Display(Name = "Durum")]
        public string Durum { get; set; }
        
        [Display(Name = "Aktif")]
        public bool Aktif { get; set; }

        [Display(Name = "Oluşturma Tarihi")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm}", ApplyFormatInEditMode = false)]
        public DateTime? OlusturmaTarihi { get; set; }

        [Display(Name = "Güncelleme Tarihi")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm}", ApplyFormatInEditMode = false)]
        public DateTime? GuncellemeTarihi { get; set; }

        public List<IrsaliyeKalemDetailViewModel> IrsaliyeKalemleri { get; set; } = new List<IrsaliyeKalemDetailViewModel>();
    }

    public class IrsaliyeKalemDetailViewModel
    {
        public Guid KalemID { get; set; }

        [Display(Name = "Ürün")]
        public Guid UrunID { get; set; }

        [Display(Name = "Ürün Adı")]
        public string UrunAdi { get; set; }

        [Display(Name = "Ürün Kodu")]
        public string UrunKodu { get; set; }

        [Display(Name = "Miktar")]
        public decimal Miktar { get; set; }

        [Display(Name = "Birim")]
        public string Birim { get; set; }

        [Display(Name = "Açıklama")]
        public string Aciklama { get; set; }
    }
} 
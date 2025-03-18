using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MuhasebeStokWebApp.ViewModels.Fatura
{
    public class FaturaViewModel
    {
        public Guid FaturaID { get; set; }

        [Display(Name = "Fatura No")]
        public string FaturaNumarasi { get; set; }

        [Display(Name = "Fatura Tarihi")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? FaturaTarihi { get; set; }

        [Display(Name = "Vade Tarihi")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? VadeTarihi { get; set; }

        [Display(Name = "Cari")]
        public Guid CariID { get; set; }

        [Display(Name = "Cari Adı")]
        public string CariAdi { get; set; }

        [Display(Name = "Fatura Türü")]
        public string FaturaTuru { get; set; }

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

        [Display(Name = "Açıklama")]
        public string Aciklama { get; set; }

        [Display(Name = "Ödeme Durumu")]
        public string OdemeDurumu { get; set; }

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
        public List<FaturaViewModel> Faturalar { get; set; }
    }
} 
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MuhasebeStokWebApp.ViewModels.Irsaliye
{
    public class IrsaliyeDetailViewModel
    {
        public Guid IrsaliyeID { get; set; }

        [Required]
        [StringLength(20)]
        [Display(Name = "İrsaliye No")]
        public string IrsaliyeNumarasi { get; set; }

        [Required]
        [Display(Name = "İrsaliye Tarihi")]
        public DateTime IrsaliyeTarihi { get; set; }

        [Required]
        public Guid CariID { get; set; }

        [Display(Name = "Cari Adı")]
        public string CariAdi { get; set; }

        [Display(Name = "Vergi No")]
        public string CariVergiNo { get; set; }

        [Display(Name = "Telefon")]
        public string CariTelefon { get; set; }

        [Display(Name = "Adres")]
        public string CariAdres { get; set; }

        [Required]
        [Display(Name = "İrsaliye Türü")]
        public string IrsaliyeTuru { get; set; }

        [Display(Name = "Fatura No")]
        public string? FaturaNumarasi { get; set; }

        public Guid? FaturaID { get; set; }

        [StringLength(500)]
        [Display(Name = "Açıklama")]
        public string? Aciklama { get; set; }

        [Display(Name = "Aktif")]
        public bool Aktif { get; set; }

        [Display(Name = "Toplam Tutar")]
        public decimal ToplamTutar { get; set; }

        [Display(Name = "Oluşturma Tarihi")]
        public DateTime OlusturmaTarihi { get; set; }

        [Display(Name = "Güncelleme Tarihi")]
        public DateTime? GuncellemeTarihi { get; set; }

        public virtual ICollection<IrsaliyeKalemViewModel> IrsaliyeDetaylari { get; set; } = new List<IrsaliyeKalemViewModel>();
    }

    public class IrsaliyeKalemViewModel
    {
        public Guid KalemID { get; set; }
        public Guid IrsaliyeID { get; set; }
        public Guid UrunID { get; set; }

        [Display(Name = "Ürün Adı")]
        public string UrunAdi { get; set; }

        [Display(Name = "Ürün Kodu")]
        public string UrunKodu { get; set; }

        [Required]
        [Display(Name = "Miktar")]
        public decimal Miktar { get; set; }

        [Display(Name = "Birim")]
        public string Birim { get; set; }

        [Required]
        [Display(Name = "Birim Fiyat")]
        public decimal BirimFiyat { get; set; }

        [Display(Name = "Toplam Tutar")]
        public decimal ToplamTutar { get; set; }

        [StringLength(500)]
        [Display(Name = "Açıklama")]
        public string? Aciklama { get; set; }
    }
} 
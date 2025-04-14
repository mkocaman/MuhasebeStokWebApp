using System;
using System.ComponentModel.DataAnnotations;
using MuhasebeStokWebApp.Enums;

namespace MuhasebeStokWebApp.ViewModels.Banka
{
    public class BankaHesapHareketViewModel
    {
        public Guid BankaHesapHareketID { get; set; }
        
        public Guid BankaHesapID { get; set; }
        
        [Display(Name = "Banka")]
        public string BankaAdi { get; set; } = "";
        
        [Display(Name = "Hesap")]
        public string HesapAdi { get; set; } = "";
        
        [Display(Name = "IBAN")]
        public string IBAN { get; set; } = "";
        
        [Display(Name = "Hesap No")]
        public string BankaHesapNo { get; set; } = "";
        
        [Display(Name = "Hareket Türü")]
        public string HareketTuru { get; set; } = "";
        
        [Display(Name = "Tarih")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy}", ApplyFormatInEditMode = false)]
        public DateTime Tarih { get; set; }
        
        [Display(Name = "Tutar")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = false)]
        public decimal Tutar { get; set; }
        
        [Display(Name = "Dekont No")]
        public string DekontNo { get; set; } = "";
        
        [Display(Name = "Referans No")]
        public string ReferansNo { get; set; } = "";
        
        [Display(Name = "Referans Türü")]
        public string ReferansTuru { get; set; } = "";
        
        [Display(Name = "Açıklama")]
        public string Aciklama { get; set; } = "";
        
        [Display(Name = "Cari")]
        public string CariUnvani { get; set; } = "";
        
        public Guid? CariID { get; set; }
        
        [Display(Name = "Oluşturma Tarihi")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm}", ApplyFormatInEditMode = false)]
        public DateTime OlusturmaTarihi { get; set; }
        
        [Display(Name = "Oluşturan Kullanıcı")]
        public string OlusturanKullaniciAdi { get; set; } = "";
        
        [Display(Name = "Son Güncelleme Tarihi")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm}", ApplyFormatInEditMode = false)]
        public DateTime? SonGuncellemeTarihi { get; set; }
        
        [Display(Name = "Güncelleyen Kullanıcı")]
        public string GuncelleyenKullaniciAdi { get; set; } = "";
    }
} 
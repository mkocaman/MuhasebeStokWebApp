using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MuhasebeStokWebApp.ViewModels.Banka
{
    public class BankaViewModel
    {
        public Guid BankaID { get; set; }
        
        [Display(Name = "Banka Adı")]
        public string BankaAdi { get; set; }
        
        [Display(Name = "Şube Adı")]
        public string SubeAdi { get; set; }
        
        [Display(Name = "Şube Kodu")]
        public string SubeKodu { get; set; }
        
        [Display(Name = "Hesap No")]
        public string HesapNo { get; set; }
        
        [Display(Name = "IBAN")]
        public string IBAN { get; set; }
        
        [Display(Name = "Para Birimi")]
        public string ParaBirimi { get; set; }
        
        [Display(Name = "Açılış Bakiyesi")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = false)]
        public decimal AcilisBakiye { get; set; }
        
        [Display(Name = "Güncel Bakiye")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = false)]
        public decimal GuncelBakiye { get; set; }
        
        [Display(Name = "Açıklama")]
        public string Aciklama { get; set; }
        
        [Display(Name = "Aktif")]
        public bool Aktif { get; set; }
        
        [Display(Name = "Oluşturma Tarihi")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy}", ApplyFormatInEditMode = false)]
        public DateTime OlusturmaTarihi { get; set; }
    }
    
    public class BankaListViewModel
    {
        public List<BankaViewModel> Bankalar { get; set; } = new List<BankaViewModel>();
        public decimal ToplamBakiye { get; set; }
        
        public Dictionary<string, decimal> ParaBirimiToplamlari { get; set; } = new Dictionary<string, decimal>();
    }
    
    public class BankaCreateViewModel
    {
        [Required(ErrorMessage = "Banka adı zorunludur.")]
        [Display(Name = "Banka Adı")]
        [StringLength(100, ErrorMessage = "Banka adı en fazla 100 karakter olabilir.")]
        public string BankaAdi { get; set; }
        
        [Display(Name = "Şube Adı")]
        [StringLength(100, ErrorMessage = "Şube adı en fazla 100 karakter olabilir.")]
        public string SubeAdi { get; set; }
        
        [Display(Name = "Şube Kodu")]
        [StringLength(50, ErrorMessage = "Şube kodu en fazla 50 karakter olabilir.")]
        public string SubeKodu { get; set; }
        
        [Display(Name = "Hesap No")]
        [StringLength(50, ErrorMessage = "Hesap no en fazla 50 karakter olabilir.")]
        public string HesapNo { get; set; }
        
        [Display(Name = "IBAN")]
        [StringLength(50, ErrorMessage = "IBAN en fazla 50 karakter olabilir.")]
        public string IBAN { get; set; }
        
        [Display(Name = "Para Birimi")]
        [StringLength(10, ErrorMessage = "Para birimi en fazla 10 karakter olabilir.")]
        public string ParaBirimi { get; set; } = "TRY";
        
        [Required(ErrorMessage = "Açılış bakiyesi zorunludur.")]
        [Display(Name = "Açılış Bakiyesi")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = true)]
        [Range(0, double.MaxValue, ErrorMessage = "Açılış bakiyesi negatif değer alamaz.")]
        public decimal AcilisBakiye { get; set; } = 0;
        
        [Display(Name = "Açıklama")]
        [StringLength(500, ErrorMessage = "Açıklama en fazla 500 karakter olabilir.")]
        public string Aciklama { get; set; }
        
        [Display(Name = "Aktif")]
        public bool Aktif { get; set; } = true;
    }
    
    public class BankaEditViewModel
    {
        public Guid BankaID { get; set; }
        
        [Required(ErrorMessage = "Banka adı zorunludur.")]
        [Display(Name = "Banka Adı")]
        [StringLength(100, ErrorMessage = "Banka adı en fazla 100 karakter olabilir.")]
        public string BankaAdi { get; set; }
        
        [Display(Name = "Şube Adı")]
        [StringLength(100, ErrorMessage = "Şube adı en fazla 100 karakter olabilir.")]
        public string SubeAdi { get; set; }
        
        [Display(Name = "Şube Kodu")]
        [StringLength(50, ErrorMessage = "Şube kodu en fazla 50 karakter olabilir.")]
        public string SubeKodu { get; set; }
        
        [Display(Name = "Hesap No")]
        [StringLength(50, ErrorMessage = "Hesap no en fazla 50 karakter olabilir.")]
        public string HesapNo { get; set; }
        
        [Display(Name = "IBAN")]
        [StringLength(50, ErrorMessage = "IBAN en fazla 50 karakter olabilir.")]
        public string IBAN { get; set; }
        
        [Display(Name = "Para Birimi")]
        [StringLength(10, ErrorMessage = "Para birimi en fazla 10 karakter olabilir.")]
        public string ParaBirimi { get; set; }
        
        [Display(Name = "Açılış Bakiyesi")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = true)]
        [Range(0, double.MaxValue, ErrorMessage = "Açılış bakiyesi negatif değer alamaz.")]
        public decimal AcilisBakiye { get; set; }
        
        [Display(Name = "Güncel Bakiye")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = false)]
        public decimal GuncelBakiye { get; set; }
        
        [Display(Name = "Açıklama")]
        [StringLength(500, ErrorMessage = "Açıklama en fazla 500 karakter olabilir.")]
        public string Aciklama { get; set; }
        
        [Display(Name = "Aktif")]
        public bool Aktif { get; set; }
    }
    
    public class BankaHareketViewModel
    {
        public Guid BankaHareketID { get; set; }
        
        public Guid BankaID { get; set; }
        
        [Display(Name = "Banka Adı")]
        public string BankaAdi { get; set; }
        
        [Display(Name = "Tutar")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = false)]
        public decimal Tutar { get; set; }
        
        [Display(Name = "Hareket Türü")]
        public string HareketTuru { get; set; }
        
        [Display(Name = "Tarih")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm}", ApplyFormatInEditMode = false)]
        public DateTime Tarih { get; set; }
        
        [Display(Name = "Referans No")]
        public string ReferansNo { get; set; }
        
        [Display(Name = "Referans Türü")]
        public string ReferansTuru { get; set; }
        
        [Display(Name = "Dekont No")]
        public string DekontNo { get; set; }
        
        [Display(Name = "Açıklama")]
        public string Aciklama { get; set; }
        
        [Display(Name = "Karşı Ünvan")]
        public string KarsiUnvan { get; set; }
        
        [Display(Name = "Karşı Banka")]
        public string KarsiBankaAdi { get; set; }
        
        [Display(Name = "Karşı IBAN")]
        public string KarsiIBAN { get; set; }
        
        [Display(Name = "Cari ID")]
        public Guid? CariID { get; set; }
        
        [Display(Name = "Cari Adı")]
        public string CariAdi { get; set; }
    }
} 
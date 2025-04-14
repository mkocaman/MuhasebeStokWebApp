using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using MuhasebeStokWebApp.Enums;

namespace MuhasebeStokWebApp.ViewModels.Banka
{
    public class BankaHesapViewModel
    {
        public Guid BankaHesapID { get; set; }
        
        public Guid BankaID { get; set; }
        
        [Required]
        [Display(Name = "Banka Adı")]
        public required string BankaAdi { get; set; }
        
        [Required]
        [Display(Name = "Hesap Adı")]
        public required string HesapAdi { get; set; }
        
        [Display(Name = "Şube Adı")]
        public required string SubeAdi { get; set; }
        
        [Display(Name = "Şube Kodu")]
        public required string SubeKodu { get; set; }
        
        [Display(Name = "Hesap No")]
        public required string HesapNo { get; set; }
        
        [Display(Name = "IBAN")]
        public required string IBAN { get; set; }
        
        [Display(Name = "Para Birimi")]
        public required string ParaBirimi { get; set; }
        
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

        public List<BankaHareketViewModel> BankaHareketleri { get; set; } = new List<BankaHareketViewModel>();
    }
    
    public class BankaHesapListViewModel
    {
        public List<BankaHesapViewModel> BankaHesaplari { get; set; } = new List<BankaHesapViewModel>();
        public decimal ToplamBakiye { get; set; }
        public Dictionary<string, decimal> ParaBirimiToplamlari { get; set; } = new Dictionary<string, decimal>();
    }
    
    public class BankaHesapCreateViewModel
    {
        public Guid BankaID { get; set; }
        
        [Required(ErrorMessage = "Banka seçimi zorunludur.")]
        [Display(Name = "Banka")]
        public required string BankaAdi { get; set; }
        
        [Required(ErrorMessage = "Hesap adı zorunludur.")]
        [Display(Name = "Hesap Adı")]
        [StringLength(100, ErrorMessage = "Hesap adı en fazla 100 karakter olabilir.")]
        public required string HesapAdi { get; set; }
        
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
        public required string ParaBirimi { get; set; } = "TRY";
        
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
    
    public class BankaHesapEditViewModel
    {
        public Guid BankaHesapID { get; set; }
        
        public Guid BankaID { get; set; }
        
        [Required(ErrorMessage = "Hesap adı zorunludur.")]
        [Display(Name = "Hesap Adı")]
        [StringLength(100, ErrorMessage = "Hesap adı en fazla 100 karakter olabilir.")]
        public required string HesapAdi { get; set; }
        
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
        public required string ParaBirimi { get; set; }
        
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
} 
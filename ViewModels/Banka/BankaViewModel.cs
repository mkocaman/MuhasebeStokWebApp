using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MuhasebeStokWebApp.ViewModels.Banka
{
    public class BankaViewModel
    {
        public Guid BankaID { get; set; }
        
        [Required]
        [Display(Name = "Banka Adı")]
        public required string BankaAdi { get; set; }
        
        [Required]
        [Display(Name = "Şube Adı")]
        public required string SubeAdi { get; set; }
        
        [Required]
        [Display(Name = "Şube Kodu")]
        public required string SubeKodu { get; set; }
        
        [Required]
        [Display(Name = "Hesap No")]
        public required string HesapNo { get; set; }
        
        [Required]
        [Display(Name = "IBAN")]
        public required string IBAN { get; set; }
        
        [Required]
        [Display(Name = "Para Birimi")]
        public required string ParaBirimi { get; set; }
        
        [Display(Name = "Açılış Bakiyesi")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = false)]
        public decimal AcilisBakiye { get; set; }
        
        [Display(Name = "Güncel Bakiye")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = false)]
        public decimal GuncelBakiye { get; set; }
        
        [Required]
        [Display(Name = "Açıklama")]
        public required string Aciklama { get; set; }
        
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
        public required string BankaAdi { get; set; }
        
        [Display(Name = "Şube Adı")]
        [StringLength(100, ErrorMessage = "Şube adı en fazla 100 karakter olabilir.")]
        public required string SubeAdi { get; set; }
        
        [Display(Name = "Şube Kodu")]
        [StringLength(50, ErrorMessage = "Şube kodu en fazla 50 karakter olabilir.")]
        public required string SubeKodu { get; set; }
        
        [Display(Name = "Hesap No")]
        [StringLength(50, ErrorMessage = "Hesap no en fazla 50 karakter olabilir.")]
        public required string HesapNo { get; set; }
        
        [Display(Name = "IBAN")]
        [StringLength(50, ErrorMessage = "IBAN en fazla 50 karakter olabilir.")]
        public required string IBAN { get; set; }
        
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
        public required string Aciklama { get; set; }
        
        [Display(Name = "Aktif")]
        public bool Aktif { get; set; } = true;
    }
    
    public class BankaEditViewModel
    {
        public Guid BankaID { get; set; }
        
        [Required(ErrorMessage = "Banka adı zorunludur.")]
        [Display(Name = "Banka Adı")]
        [StringLength(100, ErrorMessage = "Banka adı en fazla 100 karakter olabilir.")]
        public required string BankaAdi { get; set; }
        
        [Display(Name = "Şube Adı")]
        [StringLength(100, ErrorMessage = "Şube adı en fazla 100 karakter olabilir.")]
        public required string SubeAdi { get; set; }
        
        [Display(Name = "Şube Kodu")]
        [StringLength(50, ErrorMessage = "Şube kodu en fazla 50 karakter olabilir.")]
        public required string SubeKodu { get; set; }
        
        [Display(Name = "Hesap No")]
        [StringLength(50, ErrorMessage = "Hesap no en fazla 50 karakter olabilir.")]
        public required string HesapNo { get; set; }
        
        [Display(Name = "IBAN")]
        [StringLength(50, ErrorMessage = "IBAN en fazla 50 karakter olabilir.")]
        public required string IBAN { get; set; }
        
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
        public required string Aciklama { get; set; }
        
        [Display(Name = "Aktif")]
        public bool Aktif { get; set; }
    }
    
    public class BankaHareketViewModel
    {
        public Guid BankaHareketID { get; set; }
        
        public Guid BankaID { get; set; }
        
        [Display(Name = "Banka Adı")]
        public required string BankaAdi { get; set; }
        
        [Display(Name = "Tutar")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = false)]
        public decimal Tutar { get; set; }
        
        [Display(Name = "Hareket Türü")]
        public required string HareketTuru { get; set; }
        
        [Display(Name = "Tarih")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm}", ApplyFormatInEditMode = false)]
        public DateTime Tarih { get; set; }
        
        [Display(Name = "Referans No")]
        public required string ReferansNo { get; set; }
        
        [Display(Name = "Referans Türü")]
        public required string ReferansTuru { get; set; }
        
        [Display(Name = "Dekont No")]
        public required string DekontNo { get; set; }
        
        [Display(Name = "Açıklama")]
        public required string Aciklama { get; set; }
        
        [Display(Name = "Karşı Ünvan")]
        public required string KarsiUnvan { get; set; }
        
        [Display(Name = "Karşı Banka")]
        public required string KarsiBankaAdi { get; set; }
        
        [Display(Name = "Karşı IBAN")]
        public required string KarsiIBAN { get; set; }
        
        [Display(Name = "Cari ID")]
        public Guid? CariID { get; set; }
        
        [Display(Name = "Cari Adı")]
        public required string CariAdi { get; set; }
    }
    
    public class BankaHareketCreateViewModel
    {
        public Guid BankaID { get; set; }
        
        [Required(ErrorMessage = "Tutar zorunludur.")]
        [Display(Name = "Tutar")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = true)]
        [Range(0.01, double.MaxValue, ErrorMessage = "Tutar 0'dan büyük olmalıdır.")]
        public decimal Tutar { get; set; }
        
        [Required(ErrorMessage = "Hareket türü zorunludur.")]
        [Display(Name = "Hareket Türü")]
        public required string HareketTuru { get; set; }
        
        [Required(ErrorMessage = "Tarih zorunludur.")]
        [Display(Name = "Tarih")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy}", ApplyFormatInEditMode = true)]
        public DateTime Tarih { get; set; } = DateTime.Now;
        
        [Display(Name = "Referans No")]
        [StringLength(50, ErrorMessage = "Referans no en fazla 50 karakter olabilir.")]
        public required string ReferansNo { get; set; }
        
        [Display(Name = "Referans Türü")]
        [StringLength(50, ErrorMessage = "Referans türü en fazla 50 karakter olabilir.")]
        public required string ReferansTuru { get; set; }
        
        [Display(Name = "Dekont No")]
        [StringLength(50, ErrorMessage = "Dekont no en fazla 50 karakter olabilir.")]
        public required string DekontNo { get; set; }
        
        [Display(Name = "Açıklama")]
        [StringLength(500, ErrorMessage = "Açıklama en fazla 500 karakter olabilir.")]
        public required string Aciklama { get; set; }
        
        [Display(Name = "Karşı Ünvan")]
        [StringLength(200, ErrorMessage = "Karşı ünvan en fazla 200 karakter olabilir.")]
        public required string KarsiUnvan { get; set; }
        
        [Display(Name = "Karşı Banka")]
        [StringLength(100, ErrorMessage = "Karşı banka en fazla 100 karakter olabilir.")]
        public required string KarsiBankaAdi { get; set; }
        
        [Display(Name = "Karşı IBAN")]
        [StringLength(50, ErrorMessage = "Karşı IBAN en fazla 50 karakter olabilir.")]
        public required string KarsiIBAN { get; set; }
        
        [Display(Name = "Cari")]
        public Guid? CariID { get; set; }
    }
} 
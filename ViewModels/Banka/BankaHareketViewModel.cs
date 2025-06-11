using System;
using System.ComponentModel.DataAnnotations;
using MuhasebeStokWebApp.Enums;

namespace MuhasebeStokWebApp.ViewModels.Banka
{
    public class BankaHareketViewModel
    {
        public Guid BankaHareketID { get; set; }
        
        public Guid BankaHesapID { get; set; }
        
        [Display(Name = "Banka")]
        public string BankaAdi { get; set; } = "";
        
        [Display(Name = "Hesap")]
        public string HesapAdi { get; set; } = "";
        
        [Display(Name = "Hareket Tipi")]
        public BankaHareketTipi HareketTipi { get; set; }

        [Display(Name = "Hareket Türü")]
        public string HareketTuru { get; set; } = "";
        
        [Display(Name = "Tarih")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy}", ApplyFormatInEditMode = false)]
        public DateTime Tarih { get; set; }
        
        [Display(Name = "Tutar")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = false)]
        public decimal Tutar { get; set; }
        
        [Display(Name = "Para Birimi")]
        public string ParaBirimi { get; set; } = "";
        
        [Display(Name = "Açıklama")]
        public string Aciklama { get; set; } = "";
        
        [Display(Name = "Belge No")]
        public string BelgeNo { get; set; } = "";

        [Display(Name = "Referans No")]
        public string ReferansNo { get; set; } = "";
        
        [Display(Name = "Referans Türü")]
        public string ReferansTuru { get; set; } = "";
        
        [Display(Name = "Dekont No")]
        public string DekontNo { get; set; } = "";
        
        [Display(Name = "Cari")]
        public string CariUnvani { get; set; } = "";
        
        public Guid? CariID { get; set; }
        
        [Display(Name = "Cari ile Dengelensin")]
        public bool CariIleDengelensin { get; set; }
        
        [Display(Name = "Hesap Modülüne Kaydet")]
        public bool HesabaKaydet { get; set; }
        
        [Required(ErrorMessage = "Karşı para birimi seçimi zorunludur.")]
        [Display(Name = "Karşı Para Birimi")]
        public string KarsiParaBirimi { get; set; } = string.Empty;
        
        [Display(Name = "Oluşturma Tarihi")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm}", ApplyFormatInEditMode = false)]
        public DateTime OlusturmaTarihi { get; set; }
    }
    
    public class BankaHareketCreateViewModel
    {
        public Guid BankaHesapID { get; set; }
        
        [Required(ErrorMessage = "Hareket tipi seçimi zorunludur.")]
        [Display(Name = "Hareket Tipi")]
        public BankaHareketTipi HareketTipi { get; set; }
        
        [Required(ErrorMessage = "Tarih girilmesi zorunludur.")]
        [Display(Name = "Tarih")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy}", ApplyFormatInEditMode = true)]
        public DateTime Tarih { get; set; } = DateTime.Now;
        
        [Required(ErrorMessage = "Tutar girilmesi zorunludur.")]
        [Display(Name = "Tutar")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Tutar sıfırdan büyük olmalıdır.")]
        public decimal Tutar { get; set; }
        
        [Display(Name = "Belge No")]
        [StringLength(50, ErrorMessage = "Belge no en fazla 50 karakter olabilir.")]
        public string BelgeNo { get; set; } = "";

        [Display(Name = "Referans No")]
        [StringLength(50, ErrorMessage = "Referans no en fazla 50 karakter olabilir.")]
        public string ReferansNo { get; set; } = "";
        
        [Display(Name = "Referans Türü")]
        [StringLength(50, ErrorMessage = "Referans türü en fazla 50 karakter olabilir.")]
        public string ReferansTuru { get; set; } = "";
        
        [Display(Name = "Dekont No")]
        [StringLength(50, ErrorMessage = "Dekont no en fazla 50 karakter olabilir.")]
        public string DekontNo { get; set; } = "";
        
        [Display(Name = "Açıklama")]
        [StringLength(500, ErrorMessage = "Açıklama en fazla 500 karakter olabilir.")]
        public string Aciklama { get; set; } = string.Empty;

        [Display(Name = "Kur Değeri")]
        public decimal? DovizKuru { get; set; }

        [Display(Name = "Para Birimi")]
        public string ParaBirimi { get; set; } = "TRY";

        [Display(Name = "Cari ile Dengelensin")]
        public bool CariIleDengelensin { get; set; }
        
        [Display(Name = "Cari")]
        public Guid? CariID { get; set; }
        
        [Display(Name = "Hesap Modülüne Kaydet")]
        public bool HesabaKaydet { get; set; }
        
        [Display(Name = "Karşı Para Birimi")]
        public string KarsiParaBirimi { get; set; } = string.Empty;
    }
    
    public class BankaHareketEditViewModel
    {
        public Guid BankaHareketID { get; set; }
        
        public Guid BankaHesapID { get; set; }
        [Display(Name = "Hesap Adı")]
        public string HesapAdi { get; set; } = string.Empty;

        [Required(ErrorMessage = "Hareket tipi seçimi zorunludur.")]
        [Display(Name = "Hareket Tipi")]
        public string HareketTuru { get; set; }
        
        [Required(ErrorMessage = "Tarih girilmesi zorunludur.")]
        [Display(Name = "Tarih")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy}", ApplyFormatInEditMode = true)]
        public DateTime Tarih { get; set; }
        
        [Required(ErrorMessage = "Tutar girilmesi zorunludur.")]
        [Display(Name = "Tutar")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Tutar sıfırdan büyük olmalıdır.")]
        public decimal Tutar { get; set; }
        
        [Display(Name = "Referans No")]
        [StringLength(50, ErrorMessage = "Referans no en fazla 50 karakter olabilir.")]
        public string ReferansNo { get; set; } = "";
        
        [Display(Name = "Referans Türü")]
        [StringLength(50, ErrorMessage = "Referans türü en fazla 50 karakter olabilir.")]
        public string ReferansTuru { get; set; } = "Manuel";
        
        [Display(Name = "Dekont No")]
        [StringLength(50, ErrorMessage = "Dekont no en fazla 50 karakter olabilir.")]
        public string DekontNo { get; set; } = "";
        
        [Display(Name = "Açıklama")]
        [StringLength(500, ErrorMessage = "Açıklama en fazla 500 karakter olabilir.")]
        public string Aciklama { get; set; } = "";

        [Display(Name = "Cari ID")]
        public Guid? CariID { get; set; }

        [Display(Name = "Cari Adı")]
        public string CariAdi { get; set; } = string.Empty;

        [Display(Name = "Karşı Para Birimi")]
        public string KarsiParaBirimi { get; set; } = string.Empty;

        [Display(Name = "Döviz Kuru")]
        [Range(0.0001, double.MaxValue, ErrorMessage = "Döviz kuru sıfırdan büyük olmalıdır.")]
        public decimal? DovizKuru { get; set; } = 1;

        [Display(Name = "Hedef Banka ID")]
        public Guid? HedefBankaID { get; set; }

        [Display(Name = "Hedef Banka Adı")]
        public string HedefBankaAdi { get; set; } = string.Empty;

        [Display(Name = "Hedef Kasa ID")]
        public Guid? HedefKasaID { get; set; }

        [Display(Name = "Hedef Kasa Adı")]
        public string HedefKasaAdi { get; set; } = string.Empty;

        [Display(Name = "Kaynak Kasa ID")]
        public Guid? KaynakKasaID { get; set; }

        [Display(Name = "Kaynak Kasa Adı")]
        public string KaynakKasaAdi { get; set; } = string.Empty;

        public Guid? TransferID { get; set; }
    }
} 

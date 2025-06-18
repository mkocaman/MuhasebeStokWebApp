using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using MuhasebeStokWebApp.Data.Entities;
using System.Linq;

namespace MuhasebeStokWebApp.ViewModels.Kasa
{
    public class KasaViewModel
    {
        public Guid KasaID { get; set; }
        
        [Required(ErrorMessage = "Kasa adı zorunludur.")]
        [StringLength(100, ErrorMessage = "Kasa adı en fazla 100 karakter olabilir.")]
        [Display(Name = "Kasa Adı")]
        public string KasaAdi { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Kasa türü zorunludur.")]
        [StringLength(50, ErrorMessage = "Kasa türü en fazla 50 karakter olabilir.")]
        [Display(Name = "Kasa Türü")]
        public string KasaTuru { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Para birimi zorunludur.")]
        [StringLength(3, ErrorMessage = "Para birimi en fazla 3 karakter olabilir.")]
        [Display(Name = "Para Birimi")]
        public string ParaBirimi { get; set; } = "TRY";
        
        [Display(Name = "Açılış Bakiye")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = true)]
        public decimal AcilisBakiye { get; set; }
        
        [Display(Name = "Güncel Bakiye")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = true)]
        public decimal GuncelBakiye { get; set; }
        
        [StringLength(500, ErrorMessage = "Açıklama en fazla 500 karakter olabilir.")]
        [Display(Name = "Açıklama")]
        public string Aciklama { get; set; } = string.Empty;
        
        [Display(Name = "Aktif")]
        public bool Aktif { get; set; }
        
        [Display(Name = "Oluşturma Tarihi")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm}", ApplyFormatInEditMode = false)]
        public DateTime OlusturmaTarihi { get; set; }
        
        [Display(Name = "Cari")]
        public Guid? CariID { get; set; }
    }
    
    public class KasaListViewModel
    {
        public List<KasaViewModel> Kasalar { get; set; } = new List<KasaViewModel>();
        public decimal ToplamBakiye { get; set; }
    }
    
    public class KasaCreateViewModel
    {
        [Required(ErrorMessage = "Kasa adı zorunludur.")]
        [StringLength(100, ErrorMessage = "Kasa adı en fazla 100 karakter olabilir.")]
        [Display(Name = "Kasa Adı")]
        public string KasaAdi { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Para birimi zorunludur.")]
        [StringLength(3, ErrorMessage = "Para birimi en fazla 3 karakter olabilir.")]
        [Display(Name = "Para Birimi")]
        public string ParaBirimi { get; set; } = "TRY";
        
        [Display(Name = "Açılış Bakiye")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = true)]
        public decimal AcilisBakiye { get; set; }
        
        [StringLength(500, ErrorMessage = "Açıklama en fazla 500 karakter olabilir.")]
        [Display(Name = "Açıklama")]
        public string Aciklama { get; set; } = string.Empty;
        
        [Display(Name = "Aktif")]
        public bool Aktif { get; set; } = true;
    }
    
    public class KasaEditViewModel
    {
        public Guid KasaID { get; set; }
        
        [Required(ErrorMessage = "Kasa adı zorunludur.")]
        [StringLength(100, ErrorMessage = "Kasa adı en fazla 100 karakter olabilir.")]
        [Display(Name = "Kasa Adı")]
        public string KasaAdi { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Kasa türü zorunludur.")]
        [StringLength(50, ErrorMessage = "Kasa türü en fazla 50 karakter olabilir.")]
        [Display(Name = "Kasa Türü")]
        public string KasaTuru { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Para birimi zorunludur.")]
        [StringLength(3, ErrorMessage = "Para birimi en fazla 3 karakter olabilir.")]
        [Display(Name = "Para Birimi")]
        public string ParaBirimi { get; set; } = "TRY";
        
        [Display(Name = "Açılış Bakiye")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = true)]
        public decimal AcilisBakiye { get; set; }
        
        [Display(Name = "Güncel Bakiye")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = true)]
        public decimal GuncelBakiye { get; set; }
        
        [StringLength(500, ErrorMessage = "Açıklama en fazla 500 karakter olabilir.")]
        [Display(Name = "Açıklama")]
        public string Aciklama { get; set; } = string.Empty;
        
        [Display(Name = "Aktif")]
        public bool Aktif { get; set; }
    }
    
    public class KasaHareketViewModel
    {
        public Guid KasaHareketID { get; set; }
        
        public Guid KasaID { get; set; }
        
        [Display(Name = "Kasa Adı")]
        public string KasaAdi { get; set; } = string.Empty;
        
        [Display(Name = "Tutar")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = false)]
        public decimal Tutar { get; set; }
        
        [Display(Name = "Hareket Türü")]
        public string HareketTuru { get; set; } = string.Empty;
        
        [Display(Name = "Tarih")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm}", ApplyFormatInEditMode = false)]
        public DateTime Tarih { get; set; }
        
        [Display(Name = "Referans No")]
        public string ReferansNo { get; set; } = string.Empty;
        
        [Display(Name = "Referans Türü")]
        public string ReferansTuru { get; set; } = string.Empty;
        
        [Display(Name = "Açıklama")]
        public string Aciklama { get; set; } = string.Empty;
        
        [Display(Name = "Kur Değeri")]
        public decimal? DovizKuru { get; set; }
        
        [Display(Name = "Para Birimi")]
        public string ParaBirimi { get; set; } = "TRY";
        
        [Display(Name = "Karşı Para Birimi")]
        public string KarsiParaBirimi { get; set; } = string.Empty;
        
        [Display(Name = "Transfer ID")]
        public Guid? TransferID { get; set; }
        public Guid? HedefKasaID { get; set; }
        
        [Display(Name = "Hedef Kasa Adı")]
        public string HedefKasaAdi { get; set; } = string.Empty;
        
        [Display(Name = "İşlem Türü")]
        public string IslemTuru { get; set; } = string.Empty;
        
        [Display(Name = "Cari ID")]
        public Guid? CariID { get; set; }
        
        [Display(Name = "Cari Adı")]
        public string CariAdi { get; set; } = string.Empty;
        
        [Display(Name = "Cari ile Dengelensin")]
        public bool CariIleDengelensin { get; set; }
        
        [Display(Name = "Hesap Modülüne Kaydet")]
        public bool HesabaKaydet { get; set; }
        public Guid? HedefBankaID { get; set; }
        
        [Display(Name = "Hedef Banka Adı")]
        public string HedefBankaAdi { get; set; } = string.Empty;
        public Guid? KaynakBankaID { get; set; }
        
        [Display(Name = "Kaynak Banka Adı")]
        public string KaynakBankaAdi { get; set; } = string.Empty;
    }
    
    public class KasaTransferViewModel
    {
        public Guid TransferID { get; set; }
        
        [Required(ErrorMessage = "Kaynak kasa seçilmelidir.")]
        [Display(Name = "Kaynak Kasa")]
        public Guid KaynakKasaID { get; set; }
        
        [Required(ErrorMessage = "Hedef kasa seçilmelidir.")]
        [Display(Name = "Hedef Kasa")]
        public Guid HedefKasaID { get; set; }
        
        [Required(ErrorMessage = "Kaynak tutar girilmelidir.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Tutar 0'dan büyük olmalıdır.")]
        [Display(Name = "Kaynak Tutar")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = true)]
        public decimal KaynakTutar { get; set; }
        
        [Required(ErrorMessage = "Hedef tutar girilmelidir.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Tutar 0'dan büyük olmalıdır.")]
        [Display(Name = "Hedef Tutar")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = true)]
        public decimal HedefTutar { get; set; }
        
        [Required(ErrorMessage = "Kur değeri girilmelidir.")]
        [Range(0.001, double.MaxValue, ErrorMessage = "Kur değeri 0'dan büyük olmalıdır.")]
        [Display(Name = "Kur Değeri")]
        [DisplayFormat(DataFormatString = "{0:N6}", ApplyFormatInEditMode = true)]
        public decimal KurDegeri { get; set; } = 1;
        
        [Display(Name = "Kaynak Para Birimi")]
        public string KaynakParaBirimi { get; set; } = "TRY";
        
        [Display(Name = "Hedef Para Birimi")]
        public string HedefParaBirimi { get; set; } = "TRY";
        
        [StringLength(500, ErrorMessage = "Açıklama en fazla 500 karakter olabilir.")]
        [Display(Name = "Açıklama")]
        public string Aciklama { get; set; } = string.Empty;
        
        [Display(Name = "İşlem Tarihi")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm}", ApplyFormatInEditMode = false)]
        public DateTime IslemTarihi { get; set; } = DateTime.Now;
        
        [Display(Name = "Hedef Banka")]
        public Guid? HedefBankaID { get; set; }
        
        [Display(Name = "Kaynak Banka")]
        public Guid? KaynakBankaID { get; set; }
        
        [Display(Name = "Transfer Tipi")]
        public string TransferTipi { get; set; } = "KasaToKasa";
    }
    
    public class KasaHareketFilterViewModel
    {
        public Guid? KasaID { get; set; }
        public Guid? CariID { get; set; }
        public DateTime? BaslangicTarihi { get; set; }
        public DateTime? BitisTarihi { get; set; }
        public string? HareketTuru { get; set; }
        public string? ReferansTuru { get; set; }
    }
    
    public class KasaHareketTarihViewModel
    {
        [Required(ErrorMessage = "Başlangıç tarihi zorunludur.")]
        [Display(Name = "Başlangıç Tarihi")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy}", ApplyFormatInEditMode = true)]
        public DateTime BaslangicTarihi { get; set; }
        
        [Required(ErrorMessage = "Bitiş tarihi zorunludur.")]
        [Display(Name = "Bitiş Tarihi")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy}", ApplyFormatInEditMode = true)]
        public DateTime BitisTarihi { get; set; }
        
        public List<KasaHareket> Hareketler { get; set; } = new List<KasaHareket>();
        
        public List<KasaHareketOzetViewModel> Ozet { get; set; } = new List<KasaHareketOzetViewModel>();
        
        [Display(Name = "Toplam Giriş")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = false)]
        public decimal ToplamGiris => Hareketler.Where(h => h.HareketTuru == "Giriş").Sum(h => h.Tutar);
        
        [Display(Name = "Toplam Çıkış")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = false)]
        public decimal ToplamCikis => Hareketler.Where(h => h.HareketTuru == "Çıkış").Sum(h => h.Tutar);
        
        [Display(Name = "Net Bakiye Değişimi")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = false)]
        public decimal NetDegisim => ToplamGiris - ToplamCikis;
    }
    
    public class KasaHareketOzetViewModel
    {
        [Display(Name = "Kasa ID")]
        public Guid KasaID { get; set; }
        
        [Display(Name = "Kasa Adı")]
        public string KasaAdi { get; set; } = string.Empty;
        
        [Display(Name = "Para Birimi")]
        public string ParaBirimi { get; set; } = "TRY";
        
        [Display(Name = "Toplam Giriş")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = false)]
        public decimal ToplamGiris { get; set; }
        
        [Display(Name = "Toplam Çıkış")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = false)]
        public decimal ToplamCikis { get; set; }
        
        [Display(Name = "Net Bakiye")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = false)]
        public decimal NetBakiye { get; set; }
    }
} 

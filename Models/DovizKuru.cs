using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MuhasebeStokWebApp.Models
{
    /// <summary>
    /// Döviz kuru bilgilerini içeren model sınıfı
    /// </summary>
    public class DovizKuru
    {
        /// <summary>
        /// Kur kaydı ID'si
        /// </summary>
        [Key]
        public Guid DovizKuruID { get; set; }

        /// <summary>
        /// Para birimi ID'si
        /// </summary>
        public Guid ParaBirimiID { get; set; }

        /// <summary>
        /// Para birimi kodu (örn. USD, EUR, GBP)
        /// </summary>
        [Required(ErrorMessage = "Para birimi zorunludur.")]
        [StringLength(10, ErrorMessage = "Para birimi en fazla 10 karakter olabilir.")]
        [Display(Name = "Para Birimi")]
        public string ParaBirimiKodu { get; set; } = "";

        /// <summary>
        /// Para birimi adı (örn. Amerikan Doları)
        /// </summary>
        [Required(ErrorMessage = "Para birimi zorunludur.")]
        [StringLength(50, ErrorMessage = "Para birimi en fazla 50 karakter olabilir.")]
        [Display(Name = "Para Birimi Adı")]
        public string ParaBirimiAdi { get; set; } = "";

        /// <summary>
        /// Alış fiyatı
        /// </summary>
        [Required(ErrorMessage = "Kur zorunludur.")]
        [Column(TypeName = "decimal(18, 6)")]
        [Display(Name = "Alış")]
        [DisplayFormat(DataFormatString = "{0:N6}", ApplyFormatInEditMode = false)]
        public decimal Alis { get; set; }

        /// <summary>
        /// Satış fiyatı
        /// </summary>
        [Required(ErrorMessage = "Kur zorunludur.")]
        [Column(TypeName = "decimal(18, 6)")]
        [Display(Name = "Satış")]
        [DisplayFormat(DataFormatString = "{0:N6}", ApplyFormatInEditMode = false)]
        public decimal Satis { get; set; }

        /// <summary>
        /// Efektif alış fiyatı
        /// </summary>
        [Required(ErrorMessage = "Kur zorunludur.")]
        [Column(TypeName = "decimal(18, 6)")]
        [Display(Name = "Efektif Alış")]
        [DisplayFormat(DataFormatString = "{0:N6}", ApplyFormatInEditMode = false)]
        public decimal EfektifAlis { get; set; }

        /// <summary>
        /// Efektif satış fiyatı
        /// </summary>
        [Required(ErrorMessage = "Kur zorunludur.")]
        [Column(TypeName = "decimal(18, 6)")]
        [Display(Name = "Efektif Satış")]
        [DisplayFormat(DataFormatString = "{0:N6}", ApplyFormatInEditMode = false)]
        public decimal EfektifSatis { get; set; }

        /// <summary>
        /// Kurun geçerli olduğu tarih
        /// </summary>
        [Required(ErrorMessage = "Tarih zorunludur.")]
        [Display(Name = "Tarih")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy}", ApplyFormatInEditMode = false)]
        public DateTime Tarih { get; set; }

        /// <summary>
        /// Son güncelleme tarihi
        /// </summary>
        [Display(Name = "Son Güncelleme")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm}", ApplyFormatInEditMode = false)]
        public DateTime GuncellemeTarihi { get; set; }

        /// <summary>
        /// Para biriminin sembolü (örn. $, €, £)
        /// </summary>
        public string ParaBirimiSembol { get; set; } = "";

        /// <summary>
        /// Kurun güncel olup olmadığı
        /// </summary>
        [Display(Name = "Aktif")]
        public bool Aktif { get; set; } = true;

        /// <summary>
        /// Kuru oluşturan veya güncelleyen kaynak bilgisi (örn. TCMB, Manuel)
        /// </summary>
        [Required(ErrorMessage = "Kaynak zorunludur.")]
        [StringLength(50, ErrorMessage = "Kaynak en fazla 50 karakter olabilir.")]
        [Display(Name = "Kaynak")]
        public string Kaynak { get; set; } = "TCMB";

        /// <summary>
        /// Açıklama
        /// </summary>
        [StringLength(500, ErrorMessage = "Açıklama en fazla 500 karakter olabilir.")]
        [Display(Name = "Açıklama")]
        public string? Aciklama { get; set; }

        /// <summary>
        /// Silindi
        /// </summary>
        [Display(Name = "Silindi")]
        public bool Silindi { get; set; } = false;

        /// <summary>
        /// Oluşturma Tarihi
        /// </summary>
        [Display(Name = "Oluşturma Tarihi")]
        public DateTime OlusturmaTarihi { get; set; } = DateTime.Now;

        /// <summary>
        /// Oluşturan Kullanıcı ID'si
        /// </summary>
        public string? OlusturanKullaniciId { get; set; }

        /// <summary>
        /// Son güncelleyen Kullanıcı ID'si
        /// </summary>
        public string? SonGuncelleyenKullaniciId { get; set; }

        /// <summary>
        /// DovizID
        /// </summary>
        public Guid DovizID { get; set; }

        /// <summary>
        /// Döviz Kodu
        /// </summary>
        [Display(Name = "Döviz Kodu")]
        public string DovizKodu { get; set; } = "";

        /// <summary>
        /// Döviz Adı
        /// </summary>
        [Display(Name = "Döviz Adı")]
        public string DovizAdi { get; set; } = "";

        /// <summary>
        /// Kaynak Para Birimi
        /// </summary>
        [Display(Name = "Kaynak Para Birimi")]
        public string KaynakParaBirimi { get; set; } = "";

        /// <summary>
        /// Hedef Para Birimi
        /// </summary>
        [Display(Name = "Hedef Para Birimi")]
        public string HedefParaBirimi { get; set; } = "";

        /// <summary>
        /// Kur Değeri
        /// </summary>
        [Display(Name = "Kur Değeri")]
        public decimal KurDegeri { get; set; }

        /// <summary>
        /// Alış Fiyatı
        /// </summary>
        [Display(Name = "Alış Fiyatı")]
        public decimal AlisFiyati { get { return Alis; } }

        /// <summary>
        /// Satış Fiyatı
        /// </summary>
        [Display(Name = "Satış Fiyatı")]
        public decimal SatisFiyati { get { return Satis; } }

        /// <summary>
        /// Efektif Alış Fiyatı
        /// </summary>
        [Display(Name = "Efektif Alış Fiyatı")]
        public decimal EfektifAlisFiyati { get { return EfektifAlis; } }

        /// <summary>
        /// Efektif Satış Fiyatı
        /// </summary>
        [Display(Name = "Efektif Satış Fiyatı")]
        public decimal EfektifSatisFiyati { get { return EfektifSatis; } }
    }
} 
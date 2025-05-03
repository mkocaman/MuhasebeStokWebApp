#nullable enable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MuhasebeStokWebApp.Data.Entities.ParaBirimiBirlesikModul
{
    /// <summary>
    /// Para birimi entity class'ı
    /// </summary>
    [Table("BirlesikModulParaBirimleri")]
    public class ParaBirimi
    {
        /// <summary>
        /// Para birimi benzersiz kimliği
        /// </summary>
        [Key]
        public Guid ParaBirimiID { get; set; } = Guid.NewGuid();
        
        /// <summary>
        /// Para biriminin adı (örn. Türk Lirası, Amerikan Doları, Euro)
        /// </summary>
        [Required(ErrorMessage = "Para birimi adı gereklidir.")]
        [StringLength(50, ErrorMessage = "Para birimi adı en fazla 50 karakter olabilir.")]
        [Display(Name = "Para Birimi Adı")]
        public string Ad { get; set; } = string.Empty;
        
        /// <summary>
        /// Para biriminin kodu (örn. TRY, USD, EUR)
        /// </summary>
        [Required(ErrorMessage = "Para birimi kodu gereklidir.")]
        [StringLength(10, ErrorMessage = "Para birimi kodu en fazla 10 karakter olabilir.")]
        [Display(Name = "Kod")]
        public string Kod { get; set; } = string.Empty;
        
        /// <summary>
        /// Para biriminin sembolü (örn. ₺, $, €)
        /// </summary>
        [StringLength(10, ErrorMessage = "Para birimi sembolü en fazla 10 karakter olabilir.")]
        [Display(Name = "Sembol")]
        public string Sembol { get; set; } = string.Empty;
        
        /// <summary>
        /// Ondalık ayracı (örn. "." veya ",")
        /// </summary>
        [Display(Name = "Ondalık Ayracı")]
        [StringLength(1, ErrorMessage = "Ondalık ayracı en fazla 1 karakter olabilir.")]
        public string OndalikAyraci { get; set; } = ",";
        
        /// <summary>
        /// Binlik ayracı (örn. "," veya ".")
        /// </summary>
        [Display(Name = "Binlik Ayracı")]
        [StringLength(1, ErrorMessage = "Binlik ayracı en fazla 1 karakter olabilir.")]
        public string BinlikAyraci { get; set; } = ".";
        
        /// <summary>
        /// Ondalık kısım hassasiyeti (örn. 2 = 0.00, 3 = 0.000)
        /// </summary>
        [Display(Name = "Ondalık Hassasiyet")]
        [Range(0, 6, ErrorMessage = "Ondalık hassasiyet 0 ile 6 arasında olmalıdır.")]
        public int OndalikHassasiyet { get; set; } = 2;
        
        /// <summary>
        /// Ana para birimi mi? (Hesaplamalarda baz alınacak para birimi)
        /// </summary>
        [Display(Name = "Ana Para Birimi Mi?")]
        public bool AnaParaBirimiMi { get; set; } = false;
        
        /// <summary>
        /// Para birimi sıralama değeri
        /// </summary>
        [Display(Name = "Sıra")]
        public int Sira { get; set; } = 0;
        
        /// <summary>
        /// Para birimi için açıklama
        /// </summary>
        [Display(Name = "Açıklama")]
        [StringLength(500, ErrorMessage = "Açıklama en fazla 500 karakter olabilir.")]
        public string Aciklama { get; set; } = string.Empty;
        
        /// <summary>
        /// Para biriminin aktif olup olmadığını belirtir
        /// </summary>
        [Display(Name = "Aktif")]
        public bool Aktif { get; set; } = true;
        
        /// <summary>
        /// Soft delete için
        /// </summary>
        [Display(Name = "Silindi")]
        public bool Silindi { get; set; } = false;
        
        /// <summary>
        /// Oluşturulma tarihi
        /// </summary>
        [Column(TypeName = "datetime2")]
        public DateTime OlusturmaTarihi { get; set; } = DateTime.Now;
        
        /// <summary>
        /// Son güncellenme tarihi
        /// </summary>
        [Column(TypeName = "datetime2")]
        public DateTime? GuncellemeTarihi { get; set; }
        
        /// <summary>
        /// Oluşturan kullanıcı ID'si
        /// </summary>
        public string OlusturanKullaniciID { get; set; } = string.Empty;
        
        /// <summary>
        /// Son güncelleyen kullanıcı ID'si
        /// </summary>
        public string SonGuncelleyenKullaniciID { get; set; } = string.Empty;
        
        /// <summary>
        /// Bu para biriminin kur değerleri
        /// </summary>
        public virtual ICollection<KurDegeri> KurDegerleri { get; set; } = new HashSet<KurDegeri>();
        
        /// <summary>
        /// Para birimi ilişkileri - Bu para birimi kaynak olarak
        /// </summary>
        [InverseProperty("KaynakParaBirimi")]
        public virtual ICollection<ParaBirimiIliski> KaynakParaBirimiIliskileri { get; set; } = new HashSet<ParaBirimiIliski>();
        
        /// <summary>
        /// Para birimi ilişkileri - Bu para birimi hedef olarak
        /// </summary>
        [InverseProperty("HedefParaBirimi")]
        public virtual ICollection<ParaBirimiIliski> HedefParaBirimiIliskileri { get; set; } = new HashSet<ParaBirimiIliski>();
        
        /// <summary>
        /// UZS para birimi (Özbekistan Somu) oluşturur
        /// </summary>
        public static ParaBirimi CreateUzbekistanSomu()
        {
            return new ParaBirimi
            {
                ParaBirimiID = Guid.NewGuid(),
                Kod = "UZS",
                Ad = "Özbekistan Somu",
                Sembol = "UZS",
                Aciklama = "Özbekistan Cumhuriyeti'nin resmi para birimi",
                AnaParaBirimiMi = true,
                Aktif = true,
                Silindi = false,
                Sira = 1,
                OlusturmaTarihi = DateTime.Now,
                OlusturanKullaniciID = "Sistem",
                SonGuncelleyenKullaniciID = "Sistem"
            };
        }
        
        /// <summary>
        /// USD para birimi (ABD Doları) oluşturur
        /// </summary>
        public static ParaBirimi CreateUSDolar()
        {
            return new ParaBirimi
            {
                ParaBirimiID = Guid.NewGuid(),
                Kod = "USD",
                Ad = "Amerikan Doları",
                Sembol = "$",
                Aciklama = "Amerika Birleşik Devletleri'nin resmi para birimi",
                AnaParaBirimiMi = false,
                Aktif = true,
                Silindi = false,
                Sira = 2,
                OlusturmaTarihi = DateTime.Now,
                OlusturanKullaniciID = "Sistem",
                SonGuncelleyenKullaniciID = "Sistem"
            };
        }
        
        /// <summary>
        /// EUR para birimi (Euro) oluşturur
        /// </summary>
        public static ParaBirimi CreateEuro()
        {
            return new ParaBirimi
            {
                ParaBirimiID = Guid.NewGuid(),
                Kod = "EUR",
                Ad = "Euro",
                Sembol = "€",
                Aciklama = "Avrupa Birliği'nin resmi para birimi",
                AnaParaBirimiMi = false,
                Aktif = true,
                Silindi = false,
                Sira = 3,
                OlusturmaTarihi = DateTime.Now,
                OlusturanKullaniciID = "Sistem",
                SonGuncelleyenKullaniciID = "Sistem"
            };
        }
        
        /// <summary>
        /// TRY para birimi (Türk Lirası) oluşturur
        /// </summary>
        public static ParaBirimi CreateTurkLirasi()
        {
            return new ParaBirimi
            {
                ParaBirimiID = Guid.NewGuid(),
                Kod = "TRY",
                Ad = "Türk Lirası",
                Sembol = "₺",
                Aciklama = "Türkiye Cumhuriyeti'nin resmi para birimi",
                AnaParaBirimiMi = false,
                Aktif = true,
                Silindi = false,
                Sira = 4,
                OlusturmaTarihi = DateTime.Now,
                OlusturanKullaniciID = "Sistem",
                SonGuncelleyenKullaniciID = "Sistem"
            };
        }
    }
} 
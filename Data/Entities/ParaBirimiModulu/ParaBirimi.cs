#nullable enable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MuhasebeStokWebApp.Data.Entities.ParaBirimiModulu
{
    /// <summary>
    /// Para birimi entity class'ı
    /// </summary>
    [Table("ParaBirimleri")]
    public class ParaBirimi : BaseEntity
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
        public string Ad { get; set; }
        
        /// <summary>
        /// Para biriminin kodu (örn. TRY, USD, EUR)
        /// </summary>
        [Required(ErrorMessage = "Para birimi kodu gereklidir.")]
        [StringLength(10, ErrorMessage = "Para birimi kodu en fazla 10 karakter olabilir.")]
        [Display(Name = "Kod")]
        public string Kod { get; set; }
        
        /// <summary>
        /// Para biriminin sembolü (örn. ₺, $, €)
        /// </summary>
        [StringLength(10, ErrorMessage = "Para birimi sembolü en fazla 10 karakter olabilir.")]
        [Display(Name = "Sembol")]
        public string Sembol { get; set; }
        
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
        public string Aciklama { get; set; }
        
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
        public new DateTime OlusturmaTarihi { get; set; } = DateTime.Now;
        
        /// <summary>
        /// Son güncellenme tarihi
        /// </summary>
        [Column(TypeName = "datetime2")]
        public new DateTime? GuncellemeTarihi { get; set; }
        
        /// <summary>
        /// Oluşturan kullanıcı ID'si
        /// </summary>
        public string OlusturanKullaniciID { get; set; }
        
        /// <summary>
        /// Son güncelleyen kullanıcı ID'si
        /// </summary>
        public string SonGuncelleyenKullaniciID { get; set; }
        
        /// <summary>
        /// Bu para biriminin kur değerleri
        /// </summary>
        public virtual ICollection<KurDegeri> KurDegerleri { get; set; }
        
        /// <summary>
        /// Para birimi ilişkileri - Bu para birimi kaynak olarak
        /// </summary>
        [InverseProperty("KaynakParaBirimi")]
        public virtual ICollection<ParaBirimiIliski> KaynakParaBirimiIliskileri { get; set; }
        
        /// <summary>
        /// Para birimi ilişkileri - Bu para birimi hedef olarak
        /// </summary>
        [InverseProperty("HedefParaBirimi")]
        public virtual ICollection<ParaBirimiIliski> HedefParaBirimiIliskileri { get; set; }
        
        public ParaBirimi()
        {
            Ad = string.Empty;
            Kod = string.Empty;
            Sembol = string.Empty;
            Aciklama = string.Empty;
            OlusturanKullaniciID = string.Empty;
            SonGuncelleyenKullaniciID = string.Empty;
            ParaBirimiID = Guid.NewGuid();
            Aktif = true;
            Silindi = false;
            AnaParaBirimiMi = false;
            Sira = 100;
            OlusturmaTarihi = DateTime.Now;
            KurDegerleri = new HashSet<KurDegeri>();
            KaynakParaBirimiIliskileri = new HashSet<ParaBirimiIliski>();
            HedefParaBirimiIliskileri = new HashSet<ParaBirimiIliski>();
        }
        
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
                SonGuncelleyenKullaniciID = "Sistem",
                KurDegerleri = new HashSet<KurDegeri>(),
                KaynakParaBirimiIliskileri = new HashSet<ParaBirimiIliski>(),
                HedefParaBirimiIliskileri = new HashSet<ParaBirimiIliski>()
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
                Ad = "ABD Doları",
                Sembol = "$",
                Aciklama = "Amerika Birleşik Devletleri'nin resmi para birimi",
                AnaParaBirimiMi = false,
                Aktif = true,
                Silindi = false,
                Sira = 2,
                OlusturmaTarihi = DateTime.Now,
                OlusturanKullaniciID = "Sistem",
                SonGuncelleyenKullaniciID = "Sistem",
                KurDegerleri = new HashSet<KurDegeri>(),
                KaynakParaBirimiIliskileri = new HashSet<ParaBirimiIliski>(),
                HedefParaBirimiIliskileri = new HashSet<ParaBirimiIliski>()
            };
        }
    }
} 
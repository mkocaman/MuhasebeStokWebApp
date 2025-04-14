using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MuhasebeStokWebApp.Data.Entities.ParaBirimiModulu;

namespace MuhasebeStokWebApp.Data.Entities.DovizModulu
{
    /// <summary>
    /// Para Birimi entity sınıfı
    /// </summary>
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
        [Required(ErrorMessage = "Para birimi adı zorunludur")]
        [StringLength(100, ErrorMessage = "Para birimi adı en fazla 100 karakter olabilir")]
        public string Ad { get; set; }
        
        /// <summary>
        /// Para biriminin kodu (örn. TRY, USD, EUR)
        /// </summary>
        [Required(ErrorMessage = "Para birimi kodu zorunludur")]
        [StringLength(10, ErrorMessage = "Para birimi kodu en fazla 10 karakter olabilir")]
        public string Kod { get; set; }
        
        /// <summary>
        /// Para biriminin sembolü (örn. ₺, $, €)
        /// </summary>
        [Required(ErrorMessage = "Para birimi sembolü zorunludur")]
        [StringLength(10, ErrorMessage = "Para birimi sembolü en fazla 10 karakter olabilir")]
        public string Sembol { get; set; }
        
        /// <summary>
        /// Ondalık ayracı (örn. "." veya ",")
        /// </summary>
        [StringLength(1, ErrorMessage = "Ondalık ayracı en fazla 1 karakter olabilir")]
        public string OndalikAyraci { get; set; } = ".";
        
        /// <summary>
        /// Binlik ayracı (örn. "," veya ".")
        /// </summary>
        [StringLength(1, ErrorMessage = "Binlik ayracı en fazla 1 karakter olabilir")]
        public string BinlikAyraci { get; set; } = ",";
        
        /// <summary>
        /// Ondalık kısım hassasiyeti (örn. 2 = 0.00, 3 = 0.000)
        /// </summary>
        public int OndalikHassasiyet { get; set; } = 2;
        
        /// <summary>
        /// Ana para birimi mi? (Hesaplamalarda baz alınacak para birimi)
        /// </summary>
        public bool AnaParaBirimiMi { get; set; } = false;
        
        /// <summary>
        /// Para birimi sıralama değeri
        /// </summary>
        public int Sira { get; set; } = 0;
        
        /// <summary>
        /// Para birimi için açıklama
        /// </summary>
        [StringLength(500, ErrorMessage = "Açıklama en fazla 500 karakter olabilir")]
        public string Aciklama { get; set; }
        
        /// <summary>
        /// Para biriminin aktif olup olmadığını belirtir
        /// </summary>
        public bool Aktif { get; set; } = true;
        
        /// <summary>
        /// Soft delete için
        /// </summary>
        public bool Silindi { get; set; } = false;
        
        /// <summary>
        /// Oluşturulma tarihi
        /// </summary>
        public DateTime OlusturmaTarihi { get; set; } = DateTime.Now;
        
        /// <summary>
        /// Son güncellenme tarihi
        /// </summary>
        public DateTime? GuncellemeTarihi { get; set; }
        
        /// <summary>
        /// Oluşturan kullanıcı ID'si
        /// </summary>
        public string OlusturanKullaniciID { get; set; }
        
        /// <summary>
        /// Son güncelleyen kullanıcı ID'si
        /// </summary>
        public string SonGuncelleyenKullaniciID { get; set; }
        
        /// <summary>
        /// Para birimi için kur değerleri koleksiyonu - Kaynak para birimi olarak
        /// </summary>
        [InverseProperty("ParaBirimi")]
        public virtual ICollection<KurDegeri> KaynakKurDegerleri { get; set; }
        
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
    }
} 
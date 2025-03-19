using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MuhasebeStokWebApp.Data.Entities
{
    /// <summary>
    /// Sistem ayarları entity'si
    /// </summary>
    [Table("SistemAyarlari")]
    public class SistemAyarlari
    {
        [Key]
        public Guid SistemAyarlariID { get; set; }
        
        // Para birimi ayarları
        public Guid AnaDovizID { get; set; }
        
        [StringLength(5)]
        public string AnaDovizKodu { get; set; }
        
        public Guid? IkinciDovizID { get; set; }
        
        [StringLength(5)]
        public string IkinciDovizKodu { get; set; }
        
        public Guid? UcuncuDovizID { get; set; }
        
        [StringLength(5)]
        public string UcuncuDovizKodu { get; set; }
        
        // Şirket bilgileri
        [StringLength(100)]
        public string SirketAdi { get; set; } = "Şirket Adı";
        
        [StringLength(250)]
        public string SirketAdresi { get; set; } = "Şirket Adresi";
        
        [StringLength(20)]
        public string SirketTelefon { get; set; } = "+90";
        
        [StringLength(100)]
        public string SirketEmail { get; set; } = "info@sirket.com";
        
        [StringLength(20)]
        public string SirketVergiNo { get; set; } = "1234567890";
        
        [StringLength(100)]
        public string SirketVergiDairesi { get; set; } = "Vergi Dairesi";
        
        // Kur güncelleme ayarları
        public bool OtomatikDovizGuncelleme { get; set; } = true;
        
        public int DovizGuncellemeSikligi { get; set; } = 24;  // saat cinsinden
        
        public DateTime SonDovizGuncellemeTarihi { get; set; } = DateTime.Now;
        
        public bool Aktif { get; set; } = true;
        
        public DateTime OlusturmaTarihi { get; set; } = DateTime.Now;
        
        public DateTime GuncellemeTarihi { get; set; } = DateTime.Now;
        
        // İlişkiler
        [ForeignKey("AnaDovizID")]
        public virtual ParaBirimi AnaDoviz { get; set; }
        
        [ForeignKey("IkinciDovizID")]
        public virtual ParaBirimi IkinciDoviz { get; set; }
        
        [ForeignKey("UcuncuDovizID")]
        public virtual ParaBirimi UcuncuDoviz { get; set; }
    }
} 
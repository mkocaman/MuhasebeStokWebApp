#nullable enable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MuhasebeStokWebApp.Data.Entities
{
    public class Cari : BaseEntity, ISoftDelete
    {
        [Key]
        public Guid CariID { get; set; }

        // Id özelliği CariID'yi döndürecek şekilde tanımlıyorum
        public Guid Id => CariID;

        [Required]
        [StringLength(100)]
        public string Ad { get; set; } = string.Empty;
        
        [StringLength(100)]
        public string? CariUnvani { get; set; }
        
        [Required]
        [StringLength(20)]
        public string CariKodu { get; set; } = string.Empty;
        
        [Required]
        [StringLength(50)]
        public string CariTipi { get; set; } = "Müşteri";
        
        [StringLength(20)]
        public string? VergiNo { get; set; }
        
        [StringLength(50)]
        public string? VergiDairesi { get; set; }
        
        [Required]
        [StringLength(15)]
        public string Telefon { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        [StringLength(50)]
        public string Yetkili { get; set; } = string.Empty;
        
        public decimal BaslangicBakiye { get; set; }
        
        [Required]
        [StringLength(250)]
        public string Adres { get; set; } = string.Empty;
        
        [StringLength(500)]
        public string? Aciklama { get; set; }
        
        [Required]
        [StringLength(50)]
        public string Il { get; set; } = string.Empty;
        
        [Required]
        [StringLength(50)]
        public string Ilce { get; set; } = string.Empty;
        
        [Required]
        [StringLength(10)]
        public string PostaKodu { get; set; } = string.Empty;
        
        [Required]
        [StringLength(50)]
        public string Ulke { get; set; } = string.Empty;
        
        [StringLength(100)]
        public string? WebSitesi { get; set; }
        
        [Required]
        [StringLength(1000)]
        public string Notlar { get; set; } = string.Empty;
        
        // Cari para birimi alanları
        public Guid? VarsayilanParaBirimiId { get; set; }
        
        [ForeignKey("VarsayilanParaBirimiId")]
        public virtual ParaBirimiModulu.ParaBirimi? VarsayilanParaBirimi { get; set; }
        
        // Borç hesaplamalarında varsayılan döviz kuru kullanılsın mı?
        public bool VarsayilanKurKullan { get; set; } = true;
        
        public bool AktifMi { get; set; } = true;
        
        public Guid? OlusturanKullaniciId { get; set; }
        
        public Guid? SonGuncelleyenKullaniciId { get; set; }
        
        [Column(TypeName = "datetime2")]
        public new DateTime OlusturmaTarihi { get; set; } = DateTime.Now;
        
        [Column(TypeName = "datetime2")]
        public new DateTime? GuncellemeTarihi { get; set; }
        
        public bool Silindi { get; set; } = false;
        
        // Navigation properties
        public virtual ICollection<Fatura> Faturalar { get; set; } = new List<Fatura>();
        public virtual ICollection<Irsaliye> Irsaliyeler { get; set; } = new List<Irsaliye>();
        public virtual ICollection<CariHareket> CariHareketler { get; set; } = new List<CariHareket>();
        
        // Aşağıdaki koleksiyon DB'de var mı kontrol edip ekleyebilirsiniz
        // Bu koleksiyon sadece detay view'inde kullanılabilir
        [NotMapped]
        public virtual ICollection<object> SonFaturalar { get; set; } = new List<object>();

        public Cari()
        {
            // Constructor'ı temizledim çünkü property'lere zaten default değerler atanmış
        }
    }

    // Cari tipleri için enum tanımlıyoruz
    public enum CariTipleri
    {
        Müşteri,
        Tedarikçi,
        MüşteriVeTedarikçi
    }
} 
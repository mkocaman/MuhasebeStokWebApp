using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MuhasebeStokWebApp.Data.Entities
{
    /// <summary>
    /// Kur değeri entity'si - Merkez Bankasından çekilen kurları saklar
    /// </summary>
    [Table("KurDegeri")]
    public class KurDegeri
    {
        public KurDegeri()
        {
            KurDegeriID = Guid.NewGuid();
            Aktif = true;
            Silindi = false;
            OlusturmaTarihi = DateTime.Now;
            GuncellemeTarihi = DateTime.Now;
            Tarih = DateTime.Now;
            Kaynak = "Manuel";
            VeriKaynagi = "Kullanıcı";
        }

        [Key]
        public Guid KurDegeriID { get; set; }
        
        [Required]
        public Guid ParaBirimiID { get; set; }
        
        [Required]
        [Range(0.00001, 1000000)]
        [Display(Name = "Alış Değeri")]
        public decimal AlisDegeri { get; set; }
        
        [Required]
        [Range(0.00001, 1000000)]
        [Display(Name = "Satış Değeri")]
        public decimal SatisDegeri { get; set; }
        
        [Required]
        [Display(Name = "Tarih")]
        public DateTime Tarih { get; set; }
        
        [Required]
        [StringLength(50)]
        [Display(Name = "Kaynak")]
        public string Kaynak { get; set; }
        
        [StringLength(250)]
        [Display(Name = "Açıklama")]
        public string Aciklama { get; set; }
        
        [StringLength(50)]
        [Display(Name = "Veri Kaynağı")]
        public string VeriKaynagi { get; set; }
        
        [Display(Name = "Aktif")]
        public bool Aktif { get; set; } = true;
        
        [Display(Name = "Oluşturma Tarihi")]
        public DateTime OlusturmaTarihi { get; set; }
        
        [Display(Name = "Güncelleme Tarihi")]
        public DateTime GuncellemeTarihi { get; set; }
        
        [Display(Name = "Silindi")]
        public bool Silindi { get; set; } = false;
        
        // İlişkiler
        [ForeignKey("ParaBirimiID")]
        public virtual ParaBirimi ParaBirimi { get; set; } = null!;
    }
} 
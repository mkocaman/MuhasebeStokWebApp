using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MuhasebeStokWebApp.Data.Entities
{
    /// <summary>
    /// Kur değeri entity'si - Merkez Bankasından çekilen kurları saklar
    /// </summary>
    [Table("DovizKurlari")]
    public class KurDegeri
    {
        [Key]
        public int KurDegeriID { get; set; }
        
        [Required]
        [Display(Name = "Döviz İlişkisi")]
        public int DovizIliskiID { get; set; }
        
        [ForeignKey("DovizIliskiID")]
        public virtual DovizIliski DovizIliski { get; set; }
        
        [Required]
        [Column(TypeName = "decimal(18,6)")]
        [Display(Name = "Kur Değeri")]
        public decimal Deger { get; set; }
        
        [Column(TypeName = "decimal(18,6)")]
        [Display(Name = "Alış Fiyatı")]
        public decimal? AlisFiyati { get; set; }
        
        [Column(TypeName = "decimal(18,6)")]
        [Display(Name = "Satış Fiyatı")]
        public decimal? SatisFiyati { get; set; }
        
        [Required]
        [Display(Name = "Tarih")]
        public DateTime Tarih { get; set; } = DateTime.Now;
        
        [StringLength(50)]
        [Display(Name = "Kaynak")]
        public string Kaynak { get; set; }
        
        [Display(Name = "Aktif")]
        public bool Aktif { get; set; } = true;
        
        [Display(Name = "Silinmiş")]
        public bool SoftDelete { get; set; } = false;
        
        [Display(Name = "Oluşturma Tarihi")]
        public DateTime OlusturmaTarihi { get; set; } = DateTime.Now;
        
        [Display(Name = "Güncelleme Tarihi")]
        public DateTime? GuncellemeTarihi { get; set; }
    }
} 
#nullable enable

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace MuhasebeStokWebApp.Data.Entities
{
    [Table("CariHareketler")]
    public class CariHareket : BaseEntity, ISoftDelete
    {
        public CariHareket()
        {
            // Non-nullable properties için başlangıç değerleri
            HareketTuru = string.Empty;
            OlusturmaTarihi = DateTime.Now;
            Tarih = DateTime.Now;
            Borc = 0;
            Alacak = 0;
            // Required string alanlar için boş değerler
            ReferansNo = string.Empty;
            ReferansTuru = string.Empty;
            Aciklama = string.Empty;
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("CariHareketId")]
        public Guid CariHareketID { get; set; }
        
        // Id özelliği CariHareketID'yi döndürecek şekilde
        public Guid Id => CariHareketID;
        
        [Required]
        [Column("CariId")]
        public Guid CariID { get; set; }
        
        [Required]
        [StringLength(50)]
        public string HareketTuru { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal Tutar { get; set; }
        
        [Required]
        [Column(TypeName = "datetime2")]
        public DateTime Tarih { get; set; }
        
        // IslemTarihi özelliği artık Tarih.get/set'i çağırmasına gerek yok,
        // NotMapped ile veritabanında kolonu oluşturmayı engelleyelim
        [NotMapped]
        public DateTime IslemTarihi { get => Tarih; set => Tarih = value; }
        
        // Vade Tarihi özelliği
        [Column("VadeTarihi")]
        public DateTime? VadeTarihi { get; set; }
        
        [Required]
        [StringLength(50)]
        public string ReferansNo { get; set; }
        
        [Required]
        [StringLength(50)]
        public string ReferansTuru { get; set; }
        
        [Column("ReferansId")]
        public Guid? ReferansID { get; set; }
        
        [Required]
        [StringLength(500)]
        public string Aciklama { get; set; }
        
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Borc { get; set; }
        
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Alacak { get; set; }
        
        // Hesaplama amaçlı kullanılacak Bakiye özelliği, veritabanında saklanmayacak
        [NotMapped]
        public decimal Bakiye { get; set; }
        
        [Column(TypeName = "datetime2")]
        public new DateTime OlusturmaTarihi { get; set; } = DateTime.Now;
        
        [Column("OlusturanKullaniciId")]
        public Guid? OlusturanKullaniciID { get; set; }
        
        // OlusturanKullaniciId özelliği artık NotMapped ile işaretlendi
        [NotMapped]
        [JsonPropertyName("olusturanKullaniciIdentifier")] // JSON çakışmasını önlemek için özel ad
        public Guid? OlusturanKullaniciId { get => OlusturanKullaniciID; set => OlusturanKullaniciID = value; }
        
        public bool Silindi { get; set; } = false;
        
        // Navigation property
        [ForeignKey("CariID")]
        public virtual Cari Cari { get; set; } = null!;
    }
} 
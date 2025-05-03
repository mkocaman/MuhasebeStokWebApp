using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using MuhasebeStokWebApp.Enums;

namespace MuhasebeStokWebApp.Data.Entities
{
    /// <summary>
    /// Sistem log kayıtlarını tutan sınıf. 
    /// Uygulama içerisindeki tüm işlemlerin kayıtları bu sınıf ile tutulur.
    /// </summary>
    public class SistemLog
    {
        /// <summary>
        /// Log kaydının benzersiz tanımlayıcısı
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        
        /// <summary>
        /// Log kaydının benzersiz GUID değeri
        /// </summary>
        public Guid LogID { get; set; } = Guid.NewGuid();
        
        /// <summary>
        /// Log türü (Bilgi, Uyarı, Hata, Kritik, vb.)
        /// </summary>
        [Required, StringLength(50)]
        public string LogTuru { get; set; } = string.Empty;
        
        /// <summary>
        /// Log mesajı
        /// </summary>
        [Required, StringLength(2000)]
        public string Mesaj { get; set; } = string.Empty;
        
        /// <summary>
        /// Logla ilgili sayfa adı
        /// </summary>
        [StringLength(250)]
        public string Sayfa { get; set; } = string.Empty;
        
        /// <summary>
        /// Log kaydının oluşturulma tarihi
        /// </summary>
        public DateTime OlusturmaTarihi { get; set; } = DateTime.Now;
        
        /// <summary>
        /// Yapılan işlemin türü
        /// </summary>
        [StringLength(50)]
        public string IslemTuru { get; set; } = string.Empty;
        
        /// <summary>
        /// Log türünün sayısal karşılığı.
        /// Bu değer, LogLevel enum değerini (Information=0, Warning=1, Error=2, Critical=3, vb.) saklar.
        /// Filtreleme ve raporlama işlemlerinde kullanılır.
        /// </summary>
        public int? LogTuruInt { get; set; }
        
        /// <summary>
        /// Log ile ilgili detaylı açıklama
        /// </summary>
        [Required]
        public string Aciklama { get; set; } = string.Empty;
        
        /// <summary>
        /// Eğer bir hata oluştuysa hata mesajı
        /// </summary>
        [Required]
        public string HataMesaji { get; set; } = string.Empty;
        
        /// <summary>
        /// İşlemi yapan kullanıcının adı
        /// </summary>
        [Required]
        public string KullaniciAdi { get; set; } = string.Empty;
        
        /// <summary>
        /// İşlemi yapan kullanıcının IP adresi
        /// </summary>
        [StringLength(50)]
        public string IPAdresi { get; set; } = string.Empty;
        
        /// <summary>
        /// İşlemin gerçekleştiği tarih
        /// </summary>
        public DateTime IslemTarihi { get; set; } = DateTime.Now;
        
        /// <summary>
        /// İşlemin başarılı olup olmadığı
        /// </summary>
        public bool Basarili { get; set; } = true;
        
        /// <summary>
        /// İşlemin gerçekleştiği tablonun adı
        /// </summary>
        [StringLength(250)]
        public string TabloAdi { get; set; } = string.Empty;
        
        /// <summary>
        /// İşlem yapılan kaydın adı
        /// </summary>
        [StringLength(250)]
        public string KayitAdi { get; set; } = string.Empty;
        
        /// <summary>
        /// İşlem yapılan kaydın ID'si
        /// </summary>
        public Guid? KayitID { get; set; }
        
        /// <summary>
        /// İşlemi yapan kullanıcının ID'si (IdentityUser ID)
        /// </summary>
        public string? KullaniciId { get; set; }
        
        /// <summary>
        /// İşlemi yapan kullanıcının ID'si (eski sistem için)
        /// </summary>
        public Guid? KullaniciGuid { get; set; }
        
        /// <summary>
        /// İşlemi yapan kullanıcı
        /// </summary>
        [ForeignKey("KullaniciId")]
        public virtual ApplicationUser? Kullanici { get; set; }
    }
} 
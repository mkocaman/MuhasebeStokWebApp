using System;
using System.ComponentModel.DataAnnotations;

namespace MuhasebeStokWebApp.Models
{
    public class SistemLog
    {
        [Key]
        public int SistemLogID { get; set; }  // Bu alanı int olarak bırakıyoruz çünkü GetHashCode() int dönüyor

        [Required]
        [StringLength(50)]
        [Display(Name = "İşlem Türü")]
        public string IslemTuru { get; set; }

        [Display(Name = "İşlem Tarihi")]
        public DateTime IslemTarihi { get; set; }

        [Required]
        [StringLength(500)]
        [Display(Name = "Açıklama")]
        public string Aciklama { get; set; }

        [StringLength(50)]
        [Display(Name = "Kullanıcı ID")]
        public string? KullaniciID { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Kullanıcı Adı")]
        public string KullaniciAdi { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "IP Adresi")]
        public string IPAdresi { get; set; }

        [StringLength(500)]
        [Display(Name = "Tarayıcı")]
        public string Tarayici { get; set; }

        [StringLength(100)]
        [Display(Name = "İlgili ID")]
        public string? IlgiliID { get; set; }

        [Display(Name = "Başarılı")]
        public bool Basarili { get; set; } = true;

        [StringLength(500)]
        [Display(Name = "Hata Mesajı")]
        public string? HataMesaji { get; set; }

        [Display(Name = "Tablo Adı")]
        public string? TabloAdi { get; set; }

        [Display(Name = "Kayıt Adı")]
        public string? KayitAdi { get; set; }
    }
} 
using System;
using System.ComponentModel.DataAnnotations;

namespace MuhasebeStokWebApp.Models
{
    public class SistemAyari
    {
        [Key]
        public int SistemAyariID { get; set; }
        
        [Required(ErrorMessage = "Anahtar gereklidir.")]
        [StringLength(50)]
        [Display(Name = "Anahtar")]
        public string Anahtar { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Değer gereklidir.")]
        [StringLength(250)]
        [Display(Name = "Değer")]
        public string Deger { get; set; } = string.Empty;
        
        [StringLength(250)]
        [Display(Name = "Açıklama")]
        public string Aciklama { get; set; } = string.Empty;
        
        [Display(Name = "Aktif")]
        public bool Aktif { get; set; } = true;
        
        [Display(Name = "Silinmiş")]
        public bool SoftDelete { get; set; } = false;
        
        [Display(Name = "Oluşturma Tarihi")]
        public DateTime OlusturmaTarihi { get; set; } = DateTime.Now;
        
        [Display(Name = "Güncelleme Tarihi")]
        public DateTime? GuncellemeTarihi { get; set; }
        
        [Display(Name = "Oluşturan Kullanıcı")]
        public string? OlusturanKullaniciId { get; set; }
        
        [Display(Name = "Güncelleyen Kullanıcı")]
        public string? GuncelleyenKullaniciId { get; set; }

        // DovizService için gerekli ek özellikler
        [Display(Name = "Ana Döviz Kodu")]
        public string AnaDovizKodu { get; set; }

        [Display(Name = "Şirket Adı")]
        public string SirketAdi { get; set; }

        [Display(Name = "Şirket Adresi")]
        public string SirketAdresi { get; set; }

        [Display(Name = "Şirket Telefon")]
        public string SirketTelefon { get; set; }

        [Display(Name = "Şirket E-posta")]
        public string SirketEmail { get; set; }

        [Display(Name = "Şirket Vergi No")]
        public string SirketVergiNo { get; set; }

        [Display(Name = "Şirket Vergi Dairesi")]
        public string SirketVergiDairesi { get; set; }

        [Display(Name = "Son Döviz Güncelleme Tarihi")]
        public DateTime? SonDovizGuncellemeTarihi { get; set; }

        [Display(Name = "Aktif Para Birimleri")]
        public string AktifParaBirimleri { get; set; }
    }
} 
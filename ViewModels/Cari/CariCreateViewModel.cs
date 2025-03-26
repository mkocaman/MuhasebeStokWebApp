using System;
using System.ComponentModel.DataAnnotations;

namespace MuhasebeStokWebApp.ViewModels.Cari
{
    public class CariCreateViewModel
    {
        [StringLength(100, ErrorMessage = "Cari adı en fazla 100 karakter olabilir")]
        [Display(Name = "Cari Adı")]
        public string Ad { get; set; } = "";

        [StringLength(11, MinimumLength = 10, ErrorMessage = "Vergi no 10-11 karakter olmalıdır")]
        [Display(Name = "Vergi No")]
        public string VergiNo { get; set; } = "";

        [StringLength(15, ErrorMessage = "Telefon en fazla 15 karakter olabilir")]
        [Display(Name = "Telefon")]
        public string Telefon { get; set; } = "";

        [EmailAddress(ErrorMessage = "Geçerli bir email adresi giriniz")]
        [StringLength(100, ErrorMessage = "Email en fazla 100 karakter olabilir")]
        [Display(Name = "Email")]
        public string Email { get; set; } = "";

        [StringLength(50, ErrorMessage = "Yetkili adı en fazla 50 karakter olabilir")]
        [Display(Name = "Yetkili")]
        public string? Yetkili { get; set; }

        [Display(Name = "Başlangıç Bakiye")]
        public decimal BaslangicBakiye { get; set; }

        [StringLength(200, ErrorMessage = "Adres en fazla 200 karakter olabilir")]
        [Display(Name = "Adres")]
        public string? Adres { get; set; }

        [StringLength(500, ErrorMessage = "Açıklama en fazla 500 karakter olabilir")]
        [Display(Name = "Açıklama")]
        public string? Aciklama { get; set; }

        [Display(Name = "Cari Kodu")]
        [StringLength(50, ErrorMessage = "Cari kodu en fazla 50 karakter olabilir")]
        public string? CariKodu { get; set; }
        
        [Display(Name = "Cari Tipi")]
        [StringLength(50, ErrorMessage = "Cari tipi en fazla 50 karakter olabilir")]
        public string? CariTipi { get; set; }
        
        [Display(Name = "Vergi Dairesi")]
        [StringLength(100, ErrorMessage = "Vergi dairesi en fazla 100 karakter olabilir")]
        public string? VergiDairesi { get; set; }

        [Display(Name = "İl")]
        [StringLength(50, ErrorMessage = "İl en fazla 50 karakter olabilir")]
        public string? Il { get; set; }

        [Display(Name = "İlçe")]
        [StringLength(50, ErrorMessage = "İlçe en fazla 50 karakter olabilir")]
        public string? Ilce { get; set; }

        [Display(Name = "Posta Kodu")]
        [StringLength(20, ErrorMessage = "Posta kodu en fazla 20 karakter olabilir")]
        public string? PostaKodu { get; set; }

        [Display(Name = "Ülke")]
        [StringLength(50, ErrorMessage = "Ülke en fazla 50 karakter olabilir")]
        public string? Ulke { get; set; }

        [Display(Name = "Web Sitesi")]
        [StringLength(100, ErrorMessage = "Web sitesi en fazla 100 karakter olabilir")]
        public string? WebSitesi { get; set; }

        [Display(Name = "Notlar")]
        [StringLength(500, ErrorMessage = "Notlar en fazla 500 karakter olabilir")]
        public string? Notlar { get; set; }
        
        [Display(Name = "Aktif")]
        public bool Aktif { get; set; } = true;
    }
} 
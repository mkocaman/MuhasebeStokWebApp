using System;
using System.ComponentModel.DataAnnotations;

namespace MuhasebeStokWebApp.ViewModels.Cari
{
    public class CariEditViewModel
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Cari adı zorunludur.")]
        [StringLength(100, ErrorMessage = "Cari adı en fazla 100 karakter olabilir.")]
        [Display(Name = "Cari Adı")]
        public required string Ad { get; set; }

        [StringLength(15, MinimumLength = 0, ErrorMessage = "Vergi numarası 0 ve 15 karakter arasında olmalıdır.")]
        [Display(Name = "Vergi No")]
        public string? VergiNo { get; set; }

        [StringLength(15, ErrorMessage = "Telefon numarası en fazla 15 karakter olabilir.")]
        [Display(Name = "Telefon")]
        public string? Telefon { get; set; }

        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        [StringLength(100, ErrorMessage = "E-posta adresi en fazla 100 karakter olabilir.")]
        [Display(Name = "E-posta Adresi")]
        public string? Email { get; set; }

        [StringLength(50, ErrorMessage = "Yetkili adı en fazla 50 karakter olabilir.")]
        [Display(Name = "Yetkili")]
        public string? Yetkili { get; set; }

        [Display(Name = "Başlangıç Bakiyesi")]
        public decimal BaslangicBakiye { get; set; }

        // Önceki bakiyeyi saklamak için kullanılacak
        public decimal MevcutBakiye { get; set; }

        [StringLength(250, ErrorMessage = "Adres en fazla 250 karakter olabilir.")]
        [Display(Name = "Adres")]
        public string? Adres { get; set; }

        [StringLength(500, ErrorMessage = "Açıklama en fazla 500 karakter olabilir.")]
        [Display(Name = "Açıklama")]
        public string? Aciklama { get; set; }

        [Display(Name = "Aktif")]
        public bool AktifMi { get; set; } = true;

        [StringLength(20, ErrorMessage = "Cari kodu en fazla 20 karakter olabilir.")]
        [Display(Name = "Cari Kodu")]
        public string? CariKodu { get; set; }

        [Required(ErrorMessage = "Cari tipi zorunludur.")]
        [Display(Name = "Cari Tipi")]
        public required string CariTipi { get; set; } = "Müşteri";

        [StringLength(50, ErrorMessage = "Vergi dairesi en fazla 50 karakter olabilir.")]
        [Display(Name = "Vergi Dairesi")]
        public string? VergiDairesi { get; set; }

        [StringLength(50, ErrorMessage = "İl en fazla 50 karakter olabilir.")]
        [Display(Name = "İl")]
        public string? Il { get; set; }

        [StringLength(50, ErrorMessage = "İlçe en fazla 50 karakter olabilir.")]
        [Display(Name = "İlçe")]
        public string? Ilce { get; set; }

        [StringLength(10, ErrorMessage = "Posta kodu en fazla 10 karakter olabilir.")]
        [Display(Name = "Posta Kodu")]
        public string? PostaKodu { get; set; }

        [StringLength(50, ErrorMessage = "Ülke en fazla 50 karakter olabilir.")]
        [Display(Name = "Ülke")]
        public string? Ulke { get; set; }

        [StringLength(100, ErrorMessage = "Web sitesi en fazla 100 karakter olabilir.")]
        [Display(Name = "Web Sitesi")]
        public string? WebSitesi { get; set; }

        [StringLength(1000, ErrorMessage = "Notlar en fazla 1000 karakter olabilir.")]
        [Display(Name = "Notlar")]
        public string? Notlar { get; set; }

        [Display(Name = "Oluşturma Tarihi")]
        public DateTime OlusturmaTarihi { get; set; }

        [Display(Name = "Varsayılan Para Birimi")]
        public Guid? VarsayilanParaBirimiId { get; set; }

        [Display(Name = "Hesaplamalarda Varsayılan Kur Kullanılsın")]
        public bool VarsayilanKurKullan { get; set; } = true;

        // Ekstra bilgiler - view'de görüntü için kullanılabilir, entity'ye kaydedilmez
        public string? VarsayilanParaBirimiKodu { get; set; }
        public string? VarsayilanParaBirimiAdi { get; set; }
        public string? VarsayilanParaBirimiSembol { get; set; }
    }
} 
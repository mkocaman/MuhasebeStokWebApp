using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using MuhasebeStokWebApp.Data.Entities;

namespace MuhasebeStokWebApp.ViewModels.Cari
{
    public class CariViewModel
    {
        public Guid CariID { get; set; }
        
        [Display(Name = "Cari Adı")]
        [Required(ErrorMessage = "Cari adı zorunludur.")]
        public string CariAdi { get; set; }
        
        [Display(Name = "Vergi No")]
        [Required(ErrorMessage = "Vergi numarası zorunludur.")]
        public string VergiNo { get; set; }
        
        [Display(Name = "Telefon")]
        [Required(ErrorMessage = "Telefon numarası zorunludur.")]
        public string Telefon { get; set; }
        
        [Display(Name = "E-posta")]
        [Required(ErrorMessage = "E-posta adresi zorunludur.")]
        public string Email { get; set; }
        
        [Display(Name = "Adres")]
        [Required(ErrorMessage = "Adres zorunludur.")]
        public string Adres { get; set; }
        
        [Display(Name = "Yetkili")]
        [Required(ErrorMessage = "Yetkili adı zorunludur.")]
        public string Yetkili { get; set; }
        
        [Display(Name = "Açıklama")]
        [Required(ErrorMessage = "Açıklama zorunludur.")]
        public string Aciklama { get; set; }
        
        [Display(Name = "Aktif")]
        public bool Aktif { get; set; }
        
        [Display(Name = "Oluşturma Tarihi")]
        public DateTime? OlusturmaTarihi { get; set; }
        
        [Display(Name = "Güncelleme Tarihi")]
        public DateTime? GuncellemeTarihi { get; set; }
        
        // Hesap bakiyesi (CariHareketler tablosundan hesaplanacak)
        [Display(Name = "Bakiye")]
        public decimal Bakiye { get; set; }
    }

    // Cari listesini ve ilgili bilgileri taşıyan model
    public class CariListViewModel
    {
        public List<Data.Entities.Cari> Cariler { get; set; } = new List<Data.Entities.Cari>();
        public Dictionary<Guid, decimal> Bakiyeler { get; set; } = new Dictionary<Guid, decimal>();
        public string SearchString { get; set; } = string.Empty;
    }

    // Cari detay görünümü için model (detailsviewmodel)
    public class CariRaporViewModel
    {
        public Data.Entities.Cari Cari { get; set; } = null!;
        public List<CariHareket> CariHareketler { get; set; } = new List<CariHareket>();
        public List<Data.Entities.Fatura> Faturalar { get; set; } = new List<Data.Entities.Fatura>();
        public decimal ToplamBakiye { get; set; }
    }
} 
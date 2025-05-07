using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using MuhasebeStokWebApp.Data.Entities;

namespace MuhasebeStokWebApp.ViewModels.Cari
{
    public class CariDetailViewModel
    {
        public Data.Entities.Cari? Cari { get; set; }
        public List<Data.Entities.CariHareket>? CariHareketler { get; set; }
        public List<Data.Entities.Fatura>? Faturalar { get; set; }
        public decimal ToplamBakiye { get; set; }

        public Guid CariID { get; set; }
        
        [Display(Name = "Cari Adı")]
        public string? CariAdi { get; set; }

        [Display(Name = "Cari Kodu")]
        public string? CariKodu { get; set; }
        
        [Display(Name = "Cari Tipi")]
        public string? CariTipi { get; set; }
        
        [Display(Name = "Vergi No")]
        public string? VergiNo { get; set; }

        [Display(Name = "Vergi Dairesi")]
        public string? VergiDairesi { get; set; }
        
        [Display(Name = "Telefon")]
        public string? Telefon { get; set; }
        
        [Display(Name = "E-posta")]
        public string? Email { get; set; }
        
        [Display(Name = "Adres")]
        public string? Adres { get; set; }

        [Display(Name = "İl")]
        public string? Il { get; set; }

        [Display(Name = "İlçe")]
        public string? Ilce { get; set; }

        [Display(Name = "Posta Kodu")]
        public string? PostaKodu { get; set; }

        [Display(Name = "Ülke")]
        public string? Ulke { get; set; }

        [Display(Name = "Web Sitesi")]
        public string? WebSitesi { get; set; }
        
        [Display(Name = "Yetkili")]
        public string? Yetkili { get; set; }
        
        [Display(Name = "Açıklama")]
        public string? Aciklama { get; set; }

        [Display(Name = "Notlar")]
        public string? Notlar { get; set; }
        
        [Display(Name = "Aktif")]
        public bool Aktif { get; set; }

        [Display(Name = "Aktif Mi")]
        public bool AktifMi { get; set; }
        
        [Display(Name = "Oluşturma Tarihi")]
        public DateTime? OlusturmaTarihi { get; set; }
        
        [Display(Name = "Güncelleme Tarihi")]
        public DateTime? GuncellemeTarihi { get; set; }
        
        // Hesap bakiyesi (CariHareketler tablosundan hesaplanacak)
        [Display(Name = "Bakiye")]
        public decimal Bakiye { get; set; }
        
        // Cari hareketleri
        public List<CariHareketViewModel>? CariHareketleri { get; set; }
        
        // Son faturalar
        public List<FaturaViewModel>? SonFaturalar { get; set; }
        
        public CariDetailViewModel()
        {
            CariHareketler = new List<Data.Entities.CariHareket>();
            Faturalar = new List<Data.Entities.Fatura>();
            CariHareketleri = new List<CariHareketViewModel>();
            SonFaturalar = new List<FaturaViewModel>();
        }
    }
    
    // Cari hareket için basit bir ViewModel
    public class CariHareketViewModel
    {
        public Guid HareketID { get; set; }
        
        [Display(Name = "İşlem Tarihi")]
        public DateTime Tarih { get; set; }
        
        [Display(Name = "İşlem Türü")]
        public string HareketTuru { get; set; }
        
        [Display(Name = "Tutar")]
        public decimal Tutar { get; set; }
        
        [Display(Name = "Açıklama")]
        public string Aciklama { get; set; }
        
        [Display(Name = "Evrak No")]
        public string EvrakNo { get; set; }
    }
    
    // Fatura için basit bir ViewModel
    public class FaturaViewModel
    {
        public Guid FaturaID { get; set; }
        
        [Display(Name = "Fatura No")]
        public string FaturaNumarasi { get; set; }
        
        [Display(Name = "Fatura Tarihi")]
        public DateTime FaturaTarihi { get; set; }
        
        [Display(Name = "Vade Tarihi")]
        public DateTime? VadeTarihi { get; set; }
        
        [Display(Name = "Genel Toplam")]
        public decimal? GenelToplam { get; set; }
        
        [Display(Name = "Fatura Türü")]
        public string FaturaTuru { get; set; }
    }
} 
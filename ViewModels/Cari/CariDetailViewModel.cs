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
    
    // Cari Ekstre için ViewModel
    public class CariEkstreViewModel
    {
        public Guid CariID { get; set; }
        
        [Display(Name = "Cari Adı")]
        public string CariAdi { get; set; }
        
        [Display(Name = "Vergi No")]
        public string VergiNo { get; set; }
        
        [Display(Name = "Adres")]
        public string Adres { get; set; }
        
        [Display(Name = "Başlangıç Tarihi")]
        [DataType(DataType.Date)]
        public DateTime BaslangicTarihi { get; set; }
        
        [Display(Name = "Bitiş Tarihi")]
        [DataType(DataType.Date)]
        public DateTime BitisTarihi { get; set; }
        
        [Display(Name = "Rapor Tarihi")]
        public DateTime RaporTarihi { get; set; } = DateTime.Now;
        
        // Başlangıç bakiyesi
        [Display(Name = "Başlangıç Bakiyesi")]
        public decimal BaslangicBakiye { get; set; }
        
        // Bitiş bakiyesi
        [Display(Name = "Bitiş Bakiyesi")]
        public decimal BitisGuncelBakiye { get; set; }
        
        // Tüm hareketleri içeren liste (Cari, Kasa, Banka, Fatura)
        public List<CariEkstreHareketViewModel> Hareketler { get; set; }
        
        // Toplam değerler
        [Display(Name = "Toplam Borç")]
        public decimal ToplamBorc { get; set; }
        
        [Display(Name = "Toplam Alacak")]
        public decimal ToplamAlacak { get; set; }
        
        [Display(Name = "Bakiye")]
        public decimal Bakiye { get; set; }
        
        // Para birimi bazında bakiyeler
        public Dictionary<string, decimal> ParaBirimiBakiyeleri { get; set; }
        
        public CariEkstreViewModel()
        {
            Hareketler = new List<CariEkstreHareketViewModel>();
            ParaBirimiBakiyeleri = new Dictionary<string, decimal>();
            BaslangicTarihi = DateTime.Now.AddMonths(-1);
            BitisTarihi = DateTime.Now;
        }
    }
    
    // Cari Ekstre Hareket için ViewModel
    public class CariEkstreHareketViewModel
    {
        public Guid HareketID { get; set; }

        public DateTime Tarih { get; set; }
        
        [Display(Name = "Açıklama")]
        public string Aciklama { get; set; }
        
        [Display(Name = "Vade Tarihi")]
        public DateTime? VadeTarihi { get; set; }
        
        [Display(Name = "İşlem Türü")]
        public string IslemTuru { get; set; }
        
        [Display(Name = "İşlem No")]
        public string IslemNo { get; set; }
        
        [Display(Name = "Borç")]
        public decimal Borc { get; set; }
        
        [Display(Name = "Alacak")]
        public decimal Alacak { get; set; }
        
        [Display(Name = "Bakiye")]
        public decimal Bakiye { get; set; }
        
        [Display(Name = "Evrak No")]
        public string EvrakNo { get; set; }
        
        [Display(Name = "Para Birimi")]
        public string ParaBirimi { get; set; } = "TL";
        
        [Display(Name = "Kaynak")]
        public string Kaynak { get; set; } // Fatura, Kasa, Banka, Cari
    }
} 
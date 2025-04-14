using System;
using System.ComponentModel.DataAnnotations;
using MuhasebeStokWebApp.Enums;

namespace MuhasebeStokWebApp.ViewModels.Fatura
{
    public class FaturaAklamaKuyrukViewModel
    {
        public Guid AklamaID { get; set; }
        
        [Required(ErrorMessage = "Fatura seçimi zorunludur.")]
        [Display(Name = "Fatura")]
        public Guid FaturaID { get; set; }
        
        public string FaturaNo { get; set; } = "";
        
        public DateTime FaturaTarihi { get; set; }
        
        [Required(ErrorMessage = "Cari seçimi zorunludur.")]
        [Display(Name = "Cari")]
        public Guid CariID { get; set; }
        
        public string CariAdi { get; set; } = "";
        
        [Display(Name = "Ürün ID")]
        public Guid UrunID { get; set; }
        
        public string UrunAdi { get; set; } = "";
        
        [Required(ErrorMessage = "Fatura Kalem seçimi zorunludur.")]
        [Display(Name = "Fatura Kalem")]
        public Guid FaturaKalemID { get; set; }
        
        [Required(ErrorMessage = "Sözleşme seçimi zorunludur.")]
        [Display(Name = "Sözleşme")]
        public Guid SozlesmeID { get; set; }
        
        [Required(ErrorMessage = "Aklama Miktarı zorunludur.")]
        [Display(Name = "Aklama Miktarı")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Aklama miktarı sıfırdan büyük olmalıdır.")]
        public decimal AklananMiktar { get; set; }
        
        [Required(ErrorMessage = "Fiyat zorunludur.")]
        [Display(Name = "Fiyat")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Fiyat sıfırdan büyük olmalıdır.")]
        public decimal BirimFiyat { get; set; }
        
        [Display(Name = "Döviz Türü")]
        public string ParaBirimi { get; set; } = "TL";
        
        [Display(Name = "Döviz Kuru")]
        public decimal DovizKuru { get; set; }
        
        [Display(Name = "Toplam Tutar")]
        public decimal ToplamTutar { get; set; }
        
        [Display(Name = "TL Karşılığı")]
        public decimal TutarTL { get; set; }
        
        [Display(Name = "Aklama Durumu")]
        public AklamaDurumu Durum { get; set; }
        
        [Display(Name = "İşlem Tarihi")]
        public DateTime AklanmaTarihi { get; set; }
        
        [Display(Name = "Oluşturma Tarihi")]
        public DateTime OlusturmaTarihi { get; set; }
        
        [Required(ErrorMessage = "Aklama notu zorunludur.")]
        [StringLength(500, ErrorMessage = "Açıklama en fazla 500 karakter olabilir.")]
        [Display(Name = "Açıklama")]
        public string AklanmaNotu { get; set; } = "";
    }
} 
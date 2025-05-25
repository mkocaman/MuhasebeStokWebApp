using System;
using System.ComponentModel.DataAnnotations;

namespace MuhasebeStokWebApp.ViewModels.Transfer
{
    public class IcTransferViewModel
    {
        [Required(ErrorMessage = "Transfer türü zorunludur.")]
        [Display(Name = "Transfer Türü")]
        public string TransferTuru { get; set; } // KasadanKasaya, KasadanBankaya, BankadanKasaya, BankadanBankaya

        // Kaynak
        [Display(Name = "Kaynak Kasa")]
        public Guid? KaynakKasaID { get; set; }

        [Display(Name = "Kaynak Banka Hesabı")]
        public Guid? KaynakBankaHesapID { get; set; }

        // Hedef
        [Display(Name = "Hedef Kasa")]
        public Guid? HedefKasaID { get; set; }

        [Display(Name = "Hedef Banka Hesabı")]
        public Guid? HedefBankaHesapID { get; set; }

        [Required(ErrorMessage = "Tutar zorunludur.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Tutar 0'dan büyük olmalıdır.")]
        [Display(Name = "Tutar")]
        public decimal Tutar { get; set; }

        [Display(Name = "Kaynak Para Birimi")]
        public string KaynakParaBirimi { get; set; }

        [Display(Name = "Hedef Para Birimi")]
        public string HedefParaBirimi { get; set; }

        [Display(Name = "Döviz Kuru")]
        public decimal? DovizKuru { get; set; } = 1;

        [Display(Name = "Döviz Karşılığı")]
        public decimal? DovizKarsiligi { get; set; }

        [Required(ErrorMessage = "Tarih zorunludur.")]
        [Display(Name = "Tarih")]
        [DataType(DataType.DateTime)]
        public DateTime Tarih { get; set; } = DateTime.Now;

        [StringLength(500, ErrorMessage = "Açıklama en fazla 500 karakter olabilir.")]
        [Display(Name = "Açıklama")]
        public string Aciklama { get; set; }

        [Display(Name = "Referans No")]
        public string ReferansNo { get; set; }

        // Transfer işlemi için benzersiz ID
        public Guid TransferID { get; set; } = Guid.NewGuid();
    }

    public class TransferSonucViewModel
    {
        public bool Basarili { get; set; }
        public string Mesaj { get; set; }
        public Guid TransferID { get; set; }
        public string KaynakBilgisi { get; set; }
        public string HedefBilgisi { get; set; }
        public decimal Tutar { get; set; }
        public string ParaBirimi { get; set; }
        public string YonlendirmeUrl { get; set; }
    }
} 
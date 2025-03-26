using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MuhasebeStokWebApp.ViewModels.Irsaliye
{
    public class IrsaliyeViewModel
    {
        public Guid IrsaliyeID { get; set; }

        [Required]
        [StringLength(20)]
        [Display(Name = "İrsaliye No")]
        public string IrsaliyeNumarasi { get; set; }

        [Required]
        [Display(Name = "İrsaliye Tarihi")]
        public DateTime IrsaliyeTarihi { get; set; }

        [Required]
        [Display(Name = "Cari")]
        public Guid CariID { get; set; }

        [Display(Name = "Cari Adı")]
        public string CariAdi { get; set; }

        [Display(Name = "Fatura")]
        public Guid? FaturaID { get; set; }

        [Display(Name = "Fatura Numarası")]
        public string FaturaNumarasi { get; set; }

        [Display(Name = "İrsaliye Türü")]
        public string IrsaliyeTuru { get; set; }

        [StringLength(500)]
        [Display(Name = "Açıklama")]
        public string? Aciklama { get; set; }

        [Display(Name = "Toplam Tutar")]
        public decimal ToplamTutar { get; set; }

        [Display(Name = "Oluşturma Tarihi")]
        public DateTime OlusturmaTarihi { get; set; }

        [Display(Name = "Güncelleme Tarihi")]
        public DateTime? GuncellemeTarihi { get; set; }

        [Display(Name = "Aktif")]
        public bool Aktif { get; set; }

        public virtual ICollection<IrsaliyeKalemViewModel> IrsaliyeDetaylari { get; set; }

        public IrsaliyeViewModel()
        {
            IrsaliyeDetaylari = new List<IrsaliyeKalemViewModel>();
        }
    }
} 
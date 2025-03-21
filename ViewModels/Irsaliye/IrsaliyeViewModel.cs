using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MuhasebeStokWebApp.ViewModels.Irsaliye
{
    public class IrsaliyeViewModel
    {
        public Guid IrsaliyeID { get; set; }

        [Display(Name = "İrsaliye No")]
        public string IrsaliyeNumarasi { get; set; }

        [Display(Name = "İrsaliye Tarihi")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? IrsaliyeTarihi { get; set; }

        [Display(Name = "Cari")]
        public Guid CariID { get; set; }

        [Display(Name = "Cari Adı")]
        public string CariAdi { get; set; }

        [Display(Name = "İrsaliye Türü")]
        public string IrsaliyeTuru { get; set; }

        [Display(Name = "Fatura No")]
        public string FaturaNumarasi { get; set; }

        [Display(Name = "Fatura")]
        public Guid? FaturaID { get; set; }

        [Display(Name = "Açıklama")]
        public string Aciklama { get; set; }

        [Display(Name = "Durum")]
        public string Durum { get; set; }
        
        [Display(Name = "Aktif")]
        public bool Aktif { get; set; } = true;

        [Display(Name = "Oluşturma Tarihi")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm}", ApplyFormatInEditMode = false)]
        public DateTime? OlusturmaTarihi { get; set; }

        [Display(Name = "Güncelleme Tarihi")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm}", ApplyFormatInEditMode = false)]
        public DateTime? GuncellemeTarihi { get; set; }
    }
} 
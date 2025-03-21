using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MuhasebeStokWebApp.ViewModels.Irsaliye
{
    public class IrsaliyeKalemViewModel
    {
        public Guid KalemID { get; set; }

        [DisplayName("Ürün")]
        public Guid UrunID { get; set; }

        [DisplayName("Ürün Adı")]
        public string UrunAdi { get; set; }
        
        [DisplayName("Ürün Kodu")]
        public string UrunKodu { get; set; }

        [DisplayName("Miktar")]
        public decimal Miktar { get; set; }

        [DisplayName("Birim")]
        public string Birim { get; set; }

        [DisplayName("Açıklama")]
        public string Aciklama { get; set; }
    }
} 
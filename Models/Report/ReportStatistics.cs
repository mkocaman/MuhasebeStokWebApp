using System;
using System.Collections.Generic;

namespace MuhasebeStokWebApp.Models.Report
{
    public class ReportStatistics
    {
        public DateTime BaslangicTarihi { get; set; }
        public DateTime BitisTarihi { get; set; }
        public decimal ToplamGelir { get; set; }
        public decimal ToplamGider { get; set; }
        public decimal NetKar { get; set; }
        public int ToplamIslem { get; set; }
        public Dictionary<string, decimal> KategoriBazliGelir { get; set; }
        public Dictionary<string, decimal> KategoriBazliGider { get; set; }
        public Dictionary<string, int> IslemTurleri { get; set; }
        public List<TopProduct> EnCokSatilanUrunler { get; set; }
        public List<TopCustomer> EnCokIslemYapilanMusteriler { get; set; }

        public ReportStatistics()
        {
            KategoriBazliGelir = new Dictionary<string, decimal>();
            KategoriBazliGider = new Dictionary<string, decimal>();
            IslemTurleri = new Dictionary<string, int>();
            EnCokSatilanUrunler = new List<TopProduct>();
            EnCokIslemYapilanMusteriler = new List<TopCustomer>();
        }
    }

    public class TopProduct
    {
        public string UrunAdi { get; set; }
        public string Barkod { get; set; }
        public decimal ToplamSatis { get; set; }
        public decimal ToplamMiktar { get; set; }
    }

    public class TopCustomer
    {
        public string CariAdi { get; set; }
        public string CariKodu { get; set; }
        public int ToplamIslem { get; set; }
        public decimal ToplamTutar { get; set; }
    }
} 
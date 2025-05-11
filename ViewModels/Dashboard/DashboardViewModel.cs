using System;
using System.Collections.Generic;

namespace MuhasebeStokWebApp.ViewModels.Dashboard
{
    public class DashboardViewModel
    {
        // En Çok Satılan Ürünler
        public List<TopSellingProductViewModel> TopSellingProducts { get; set; } = new List<TopSellingProductViewModel>();
        
        // En Çok Satış Yapılan Cariler
        public List<TopCustomerViewModel> TopCustomers { get; set; } = new List<TopCustomerViewModel>();
        
        // Aylık Satış Trendi
        public List<MonthlySalesTrendViewModel> MonthlySalesTrend { get; set; } = new List<MonthlySalesTrendViewModel>();
        
        // Kritik Stoktaki Ürünler
        public List<CriticalStockProductViewModel> CriticalStockProducts { get; set; } = new List<CriticalStockProductViewModel>();
        
        // Döviz Bazlı Satış Dağılımı
        public List<CurrencySalesDistributionViewModel> CurrencySalesDistribution { get; set; } = new List<CurrencySalesDistributionViewModel>();
        
        // Toplam Alış / Toplam Satış
        public TotalPurchaseSalesViewModel TotalPurchaseSales { get; set; } = new TotalPurchaseSalesViewModel();
        
        // Ödeme / Tahsilat Özeti
        public PaymentReceiptSummaryViewModel PaymentReceiptSummary { get; set; } = new PaymentReceiptSummaryViewModel();
        
        // Son 5 Fatura
        public List<RecentInvoiceViewModel> RecentInvoices { get; set; } = new List<RecentInvoiceViewModel>();
        
        // Günlük İşlem Aktivitesi
        public List<DailyActivityViewModel> DailyActivities { get; set; } = new List<DailyActivityViewModel>();
        
        // Günlük Aktivite Toplu Verileri
        public DailyActivityStatViewModel DailyActivity { get; set; } = new DailyActivityStatViewModel();
        
        // Ortalama Kar Marjı
        public ProfitMarginViewModel ProfitMargin { get; set; } = new ProfitMarginViewModel();
    }

    // En Çok Satılan Ürünler için ViewModel
    public class TopSellingProductViewModel
    {
        public string UrunAdi { get; set; }
        public decimal ToplamSatisMiktari { get; set; }
        public decimal ToplamSatisTutari { get; set; }
        public decimal SatisMiktari { get; set; }
        public decimal BirimFiyat { get; set; }
    }

    // En Çok Satış Yapılan Cariler için ViewModel
    public class TopCustomerViewModel
    {
        public string CariAdi { get; set; }
        public decimal ToplamCiro { get; set; }
        public string MusteriAdi { get; set; }
        public decimal SatisTutari { get; set; }
    }

    // Aylık Satış Trendi için ViewModel
    public class MonthlySalesTrendViewModel
    {
        public DateTime Ay { get; set; }
        public decimal ToplamSatisUSD { get; set; }
        public decimal ToplamAlisUSD { get; set; }
    }

    // Kritik Stoktaki Ürünler için ViewModel
    public class CriticalStockProductViewModel
    {
        public string UrunAdi { get; set; }
        public decimal MevcutMiktar { get; set; }
        public decimal KritikSeviye { get; set; }
        public decimal StokMiktari { get; set; }
        public decimal KritikStokSeviyesi { get; set; }
    }

    // Döviz Bazlı Satış Dağılımı için ViewModel
    public class CurrencySalesDistributionViewModel
    {
        public string ParaBirimiKodu { get; set; }
        public string ParaBirimiAdi { get; set; }
        public decimal ToplamTutar { get; set; }
        public int IslemSayisi { get; set; }
    }

    // Toplam Alış / Toplam Satış için ViewModel
    public class TotalPurchaseSalesViewModel
    {
        public decimal ToplamAlisUSD { get; set; }
        public decimal ToplamSatisUSD { get; set; }
    }

    // Ödeme / Tahsilat Özeti için ViewModel
    public class PaymentReceiptSummaryViewModel
    {
        public decimal ToplamOdeme { get; set; }
        public decimal ToplamTahsilat { get; set; }
        public int VadesiGelenIslemSayisi { get; set; }
    }

    public class PaymentReceiptDetailViewModel
    {
        public Guid Id { get; set; }
        public string Tur { get; set; } // Tahsilat veya Ödeme
        public string Aciklama { get; set; }
        public decimal Tutar { get; set; }
        public string ParaBirimi { get; set; }
    }

    // Son 5 Fatura için ViewModel
    public class RecentInvoiceViewModel
    {
        public Guid FaturaID { get; set; }
        public string FaturaNo { get; set; }
        public DateTime? FaturaTarihi { get; set; }
        public string CariAdi { get; set; }
        public decimal Tutar { get; set; }
        public string ParaBirimi { get; set; }
        public string OdemeDurumu { get; set; }
    }

    // Günlük İşlem Aktivitesi için ViewModel
    public class DailyActivityViewModel
    {
        public DateTime Tarih { get; set; }
        public int FaturaIslemSayisi { get; set; }
        public int OdemeTahsilatIslemSayisi { get; set; }
        public int ToplamIslemSayisi { get; set; }
    }

    // Günlük Aktivite Toplu İstatistik ViewModel
    public class DailyActivityStatViewModel
    {
        public decimal GunlukArtisYuzdesi { get; set; }
        public List<DailyActivitySalesStatsViewModel> GunlukSatisVerileri { get; set; } = new List<DailyActivitySalesStatsViewModel>();
    }

    // Günlük Satış İstatistikleri Detay ViewModel
    public class DailyActivitySalesStatsViewModel
    {
        public DateTime Tarih { get; set; }
        public decimal SatisTutari { get; set; }
        public decimal AlisTutari { get; set; }
    }

    // Ortalama Kar Marjı için ViewModel
    public class ProfitMarginViewModel
    {
        public decimal KarMarjiYuzdesi { get; set; }
        public decimal ToplamSatisTutari { get; set; }
        public decimal ToplamMaliyetTutari { get; set; }
        public int IslemSayisi { get; set; }
    }
} 
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MuhasebeStokWebApp.ViewModels.Dashboard;

namespace MuhasebeStokWebApp.Services.Interfaces
{
    public interface IDashboardService
    {
        // Ana dashboard verilerini getir
        Task<DashboardViewModel> GetDashboardDataAsync();
        
        // En çok satılan ürünleri getir
        Task<List<TopSellingProductViewModel>> GetTopSellingProductsAsync(int count = 5);
        
        // En çok satış yapılan carileri getir
        Task<List<TopCustomerViewModel>> GetTopCustomersAsync(int count = 5);
        
        // Aylık satış trendini getir
        Task<List<MonthlySalesTrendViewModel>> GetMonthlySalesTrendAsync(int months = 12);
        
        // Kritik stoktaki ürünleri getir
        Task<List<CriticalStockProductViewModel>> GetCriticalStockProductsAsync();
        
        // Döviz bazlı satış dağılımını getir
        Task<List<CurrencySalesDistributionViewModel>> GetCurrencySalesDistributionAsync();
        
        // Toplam alış/satış verilerini getir
        Task<TotalPurchaseSalesViewModel> GetTotalPurchaseSalesAsync();
        
        // Ödeme/tahsilat özetini getir
        Task<PaymentReceiptSummaryViewModel> GetPaymentReceiptSummaryAsync();
        
        // Son faturaları getir
        Task<List<RecentInvoiceViewModel>> GetRecentInvoicesAsync(int count = 5);
        
        // Günlük işlem aktivitesini getir
        Task<List<DailyActivityViewModel>> GetDailyActivitiesAsync(int days = 7);
        
        // Ortalama kar marjını getir
        Task<ProfitMarginViewModel> GetProfitMarginAsync();
    }
} 
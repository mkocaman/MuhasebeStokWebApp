using System;

namespace MuhasebeStokWebApp.Models.Currency
{
    public class ExchangeRate
    {
        public string CurrencyCode { get; set; }
        public string CurrencyName { get; set; }
        public decimal BuyingRate { get; set; }
        public decimal SellingRate { get; set; }
        public decimal EffectiveBuyingRate { get; set; }
        public decimal EffectiveSellingRate { get; set; }
        public DateTime Date { get; set; }
        public DateTime LastUpdated { get; set; }
    }
} 
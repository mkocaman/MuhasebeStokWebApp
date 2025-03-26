using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using MuhasebeStokWebApp.Models.Currency;

namespace MuhasebeStokWebApp.Services.Currency
{
    public interface ICurrencyService
    {
        Task<ExchangeRate> GetExchangeRateAsync(string sourceCurrency, string targetCurrency, DateTime? date = null);
        Task<List<ExchangeRate>> GetDailyRatesAsync(DateTime? date = null);
        Task<bool> UpdateExchangeRatesAsync();
        Task<decimal> ConvertCurrencyAsync(decimal amount, string sourceCurrency, string targetCurrency, DateTime? date = null);
        Task<List<string>> GetAvailableCurrenciesAsync();
    }
} 
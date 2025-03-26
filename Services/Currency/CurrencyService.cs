using System;
using System.Xml;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using MuhasebeStokWebApp.Models.Currency;

namespace MuhasebeStokWebApp.Services.Currency
{
    public class CurrencyService : ICurrencyService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IMemoryCache _cache;
        private readonly ILogger<CurrencyService> _logger;
        private const string TCMB_URL = "https://www.tcmb.gov.tr/kurlar/today.xml";
        private const string CACHE_KEY = "ExchangeRates";
        private static readonly TimeSpan CACHE_DURATION = TimeSpan.FromMinutes(30);

        public CurrencyService(
            IHttpClientFactory httpClientFactory,
            IMemoryCache cache,
            ILogger<CurrencyService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _cache = cache;
            _logger = logger;
        }

        public async Task<ExchangeRate> GetExchangeRateAsync(string sourceCurrency, string targetCurrency, DateTime? date = null)
        {
            var rates = await GetDailyRatesAsync(date);
            var rate = rates.Find(r => r.CurrencyCode == sourceCurrency);
            
            if (rate == null)
                throw new KeyNotFoundException($"Döviz kuru bulunamadı: {sourceCurrency}");

            return rate;
        }

        public async Task<List<ExchangeRate>> GetDailyRatesAsync(DateTime? date = null)
        {
            if (_cache.TryGetValue(CACHE_KEY, out List<ExchangeRate> cachedRates))
                return cachedRates;

            var rates = await FetchRatesFromTCMBAsync();
            _cache.Set(CACHE_KEY, rates, CACHE_DURATION);
            
            return rates;
        }

        public async Task<bool> UpdateExchangeRatesAsync()
        {
            try
            {
                var rates = await FetchRatesFromTCMBAsync();
                _cache.Set(CACHE_KEY, rates, CACHE_DURATION);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Döviz kurları güncellenirken hata oluştu");
                return false;
            }
        }

        public async Task<decimal> ConvertCurrencyAsync(
            decimal amount,
            string sourceCurrency,
            string targetCurrency,
            DateTime? date = null)
        {
            if (sourceCurrency == targetCurrency)
                return amount;

            var sourceRate = await GetExchangeRateAsync(sourceCurrency, targetCurrency, date);
            var targetRate = await GetExchangeRateAsync(targetCurrency, sourceCurrency, date);

            var result = amount * (sourceRate.SellingRate / targetRate.BuyingRate);
            return Math.Round(result, 4);
        }

        public async Task<List<string>> GetAvailableCurrenciesAsync()
        {
            var rates = await GetDailyRatesAsync();
            return rates.Select(r => r.CurrencyCode).ToList();
        }

        private async Task<List<ExchangeRate>> FetchRatesFromTCMBAsync()
        {
            try
            {
                using var client = _httpClientFactory.CreateClient();
                var response = await client.GetStringAsync(TCMB_URL);
                
                var rates = new List<ExchangeRate>();
                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(response);

                var nodes = xmlDoc.SelectNodes("//Currency");
                if (nodes == null)
                    return rates;

                foreach (XmlNode node in nodes)
                {
                    var rate = new ExchangeRate
                    {
                        CurrencyCode = node.Attributes["CurrencyCode"]?.Value,
                        CurrencyName = node.SelectSingleNode("CurrencyName")?.InnerText,
                        BuyingRate = decimal.Parse(node.SelectSingleNode("ForexBuying")?.InnerText ?? "0"),
                        SellingRate = decimal.Parse(node.SelectSingleNode("ForexSelling")?.InnerText ?? "0"),
                        EffectiveBuyingRate = decimal.Parse(node.SelectSingleNode("BanknoteBuying")?.InnerText ?? "0"),
                        EffectiveSellingRate = decimal.Parse(node.SelectSingleNode("BanknoteSelling")?.InnerText ?? "0"),
                        Date = DateTime.Today,
                        LastUpdated = DateTime.Now
                    };
                    rates.Add(rate);
                }

                return rates;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "TCMB'den döviz kurları alınırken hata oluştu");
                throw;
            }
        }
    }
} 
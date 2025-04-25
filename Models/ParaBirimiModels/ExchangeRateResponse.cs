using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MuhasebeStokWebApp.Models.ParaBirimiModels
{
    public class ExchangeRateResponse
    {
        [JsonPropertyName("base")]
        public string? Base { get; set; }

        [JsonPropertyName("date")]
        public string? Date { get; set; }

        [JsonPropertyName("rates")]
        public Dictionary<string, decimal>? Rates { get; set; }
    }
} 
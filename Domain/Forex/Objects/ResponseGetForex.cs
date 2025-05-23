﻿using Newtonsoft.Json;

namespace Domain.Forex.Objects
{
    public class ResponseGetForex
    {
        [JsonProperty("Time Series FX (Daily)")]
        public Dictionary<DateTime, ForexPrice> TimeSeries { get; set; }
    }

    public class ForexPrice
    {
        [JsonProperty("1. open")]
        public double Open { get; set; }

        [JsonProperty("2. high")]
        public double High { get; set; }

        [JsonProperty("3. low")]
        public double Low { get; set; }

        [JsonProperty("4. close")]
        public double Close { get; set; }
    }
}

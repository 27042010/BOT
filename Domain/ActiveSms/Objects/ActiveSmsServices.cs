using Newtonsoft.Json;

namespace Domain.ActiveSms.Objects
{
    public class ActiveSmsServices
    {
        [JsonProperty("countries")]
        public Dictionary<string, int> Countries { get; set; } = new();

        [JsonProperty("operators")]
        public Dictionary<string, string> Operators { get; set; } = new();

        [JsonProperty("services")]
        public Dictionary<string, ServiceCost> Services { get; set; } = new();

        [JsonProperty("realHours")]
        public int RealHours { get; set; }
    }

    public class ServiceCost
    {
        [JsonProperty("cost")]
        public double Cost { get; set; }

        [JsonProperty("retail_cost")]
        public double RetailCost { get; set; }

        [JsonProperty("quant")]
        public Quant Quant { get; set; } = new();

        [JsonProperty("search_name")]
        public string SearchName { get; set; } = string.Empty;
    }

    public class Quant
    {
        [JsonProperty("current")]
        public int Current { get; set; }

        [JsonProperty("total")]
        public int Total { get; set; }
    }
}

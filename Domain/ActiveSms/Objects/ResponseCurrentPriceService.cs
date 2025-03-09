using Newtonsoft.Json;

namespace Domain.ActiveSms.Objects
{
    public class ResponseCurrentPriceService
    {
        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("cost")]
        public int Cost { get; set; }

        [JsonProperty("service")]
        public string Service { get; set; }

        [JsonProperty("phone")]
        public int Phone { get; set; }

        [JsonProperty("country")]
        public int Country { get; set; }
    }
}

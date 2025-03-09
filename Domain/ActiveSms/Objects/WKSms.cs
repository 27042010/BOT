using Newtonsoft.Json;

namespace Domain.ActiveSms.Objects
{
    public class WKSms
    {
        [JsonProperty("activationId")]
        public long ActivationId { get; set; }

        [JsonProperty("service")]
        public string Service { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("code")]
        public string Code { get; set; }

        [JsonProperty("country")]
        public int Country { get; set; }

        [JsonProperty("receivedAt")]
        public string ReceivedAt { get; set; }
    }
}

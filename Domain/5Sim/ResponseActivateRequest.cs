using Newtonsoft.Json;

namespace Domain._5Sim
{
    public class ResponseActivateRequest
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("sms")]
        public List<Sm>? Sms { get; set; }

        [JsonProperty("phone")]
        public string Phone { get; set; }

        [JsonProperty("operator")]
        public string Operator { get; set; }

        [JsonProperty("product")]
        public string Product { get; set; }

        [JsonProperty("price")]
        public double Price { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("expires")]
        public string Expires { get; set; }

        [JsonProperty("created_at")]
        public string CreatedAt { get; set; }

        [JsonProperty("forwarding")]
        public bool Forwarding { get; set; }

        [JsonProperty("forwarding_number")]
        public string ForwardingNumber { get; set; }

        [JsonProperty("country")]
        public string Country { get; set; }
    }

    public class Sm
    {
        [JsonProperty("created_at")]
        public string CreatedAt { get; set; }

        [JsonProperty("date")]
        public DateTime Date { get; set; }

        [JsonProperty("sender")]
        public string Sender { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("code")]
        public string Code { get; set; }
    }
}

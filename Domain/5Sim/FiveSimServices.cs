using Newtonsoft.Json;

namespace Domain._5Sim
{
    public class FiveSimServices
    {
        [JsonProperty("Category")]
        public string Category { get; set; }

        [JsonProperty("Qty")]
        public int Qty { get; set; }

        [JsonProperty("Price")]
        public double Price { get; set; }
    }
}

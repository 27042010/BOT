using Newtonsoft.Json;

namespace Domain.ActiveSms.Objects
{
    public class ResponseRequestAtivationService
    {
        [JsonProperty("activationId")]
        public string ActivationId { get; set; }

        [JsonProperty("phoneNumber")]
        public string PhoneNumber { get; set; }

        [JsonProperty("activationCost")]
        public string ActivationCost { get; set; }

        [JsonProperty("countryCode")]
        public string CountryCode { get; set; }

        [JsonProperty("canGetAnotherSms")]
        public bool CanGetAnotherSms { get; set; }

        [JsonProperty("activationTime")]
        public string ActivationTime { get; set; }

        [JsonProperty("activationEndTime")]
        public string ActivationEndTime { get; set; }

        [JsonProperty("activationOperator")]
        public string ActivationOperator { get; set; }
    }
}

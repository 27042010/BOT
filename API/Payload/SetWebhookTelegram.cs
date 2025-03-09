namespace API.Payload
{
    public class SetWebhookTelegram
    {
        public string Url { get; set; } = string.Empty;
        public string Secret { get; set; } = string.Empty;
    }
}

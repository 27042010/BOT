using MongoDB.Bson.Serialization.Attributes;using MongoDB.Bson;namespace Domain{
    public class UserBalanceHistory
    {
        [BsonIgnore]
        public static string TABLE = "USER_BALANCE_HISTORY";

        [BsonId]
        [BsonElement("_id")]
        public ObjectId Id { get; set; }

        public ObjectId OwnerId { get; set; }

        public string ChatId { get; set; } = string.Empty;
        public string Guid { get; set; } = string.Empty;
        public string TransactionId { get; set; } = string.Empty;
        public string TransactionName { get; set; } = string.Empty;
        public string ServiceKey { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public double Amount { get; set; }
        public string Status { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Service { get; set; } = string.Empty;
        public TransationType TransationType { get; set; }
        public DateTime ConfirmedAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }    public enum TransationType
    {
        PIX,
        RENT,
        VOUCHER,
        RE_RENT,
    }
}
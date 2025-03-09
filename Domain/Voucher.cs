using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Domain
{
    public class Voucher
    {
        [BsonIgnore]
        public static string TABLE = "VOUCHER";

        [BsonId]
        [BsonElement("_id")]
        public ObjectId Id { get; set; }

        public string Guid { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public double Amount { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}

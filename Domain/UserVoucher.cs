using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Domain
{
    public class UserVoucher
    {
        [BsonIgnore]
        public static string TABLE = "USER_VOUCHER";

        [BsonId]
        [BsonElement("_id")]
        public ObjectId Id { get; set; }

        public string ChatId { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public DateTime UsageDate { get; set; }
    }
}

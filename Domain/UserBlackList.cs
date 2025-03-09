using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Domain
{
    public class UserBlackList
    {
        [BsonIgnore]
        public static string TABLE = "USER_BLACK_LIST";

        [BsonId]
        [BsonElement("_id")]
        public ObjectId Id { get; set; }

        public string ChatId { get; set; } = string.Empty;
        public BlackListReason Reason { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public enum BlackListReason
    {
        Fraud,
        Spam
    }
}

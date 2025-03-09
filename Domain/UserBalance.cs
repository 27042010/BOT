using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Domain
{
    public class UserBalance
    {
        [BsonIgnore]
        public static string TABLE = "USER_BALANCE";

        [BsonId]
        [BsonElement("_id")]
        public ObjectId Id { get; set; }

        public string Guid { get; set; } = string.Empty;
        public string ChatId { get; set; } = string.Empty;
        public double Balance { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
    }
}

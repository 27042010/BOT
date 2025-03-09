using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Domain
{
    public class User
    {
        [BsonIgnore]
        public static string TABLE = "USER";

        [BsonId]
        [BsonElement("_id")]
        public ObjectId Id { get; set; }

        public string ChatId { get; set; } = string.Empty;
        public string ContactId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Language { get; set; } = string.Empty;
        public string SelectetdCountryCode { get; set; } = string.Empty;
        public string SelectetdCountryName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}

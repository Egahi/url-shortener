using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace url_shortener.Models
{
    public class Url
    {
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public string ShortUrl { get; set; } = string.Empty;
        public string LongUrl { get; set; } = string.Empty;
        public int AccessCount { get; set; } = 0;
    }
}

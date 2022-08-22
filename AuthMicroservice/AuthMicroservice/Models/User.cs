using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AuthMicroservice.Models
{
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = null!;
        public string Login { get; set; } = null!;
        public string Hash { get; set; } = null!;
        public string Salt { get; set; } = null!;
    }
}

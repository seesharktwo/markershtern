using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace UserBalanceMicroservice.Models
{
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public decimal Money { get; set; }

        public string LastTransactId { get; set; }

    }
}

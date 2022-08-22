using EnumList.CustomTypes;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace UserBalanceMicroservice.Models
{
    public class Transact
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string UserId { get; set; }
        public string GlobalTransactId { get; set; }
        public Operation Mode { get; set; }

    }
}

using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace TransactService.Models
{
    public class GlobalTransact
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public decimal Value { get; set; }
        public DateTime Date { get; set; }
        public Status Status    { get; set; }
    }
}

using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace TransactService.Models
{
    public class OrderTransact 
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string IdTransact { get; set; }
        public string UserId { get; set; }
        public string IdOrder { get; set; }
        public int Value { get; set; }
        public DateTime Date { get; set; }
        public Status Status { get; set; }
    }
}

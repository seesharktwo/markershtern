using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace TransactService.Models
{
    public class BalanceTransact
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string IdTransact { get; set; }
        public decimal Value { get; set; }
        public string IdBalance { get; set; }
        public string UserId { get; set; }
        public DateTime Date { get; set; }
        public Status Status { get; set; }
    }
}

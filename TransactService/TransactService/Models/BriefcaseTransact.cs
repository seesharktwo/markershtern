using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace TransactService.Models
{
    public class BriefcaseTransact 
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string IdTransact { get; set; }
        public int Value { get; set; }
        public string IdProduct { get; set; }
        public string UserId { get; set; }
        public DateTime Date { get; set; }
        public Status Status { get; set; }
    }
}

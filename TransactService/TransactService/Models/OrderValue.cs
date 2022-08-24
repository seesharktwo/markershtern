using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace TransactService.Models
{
    public class OrderValue
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string IdObject { get; set; }
        public string UserId { get; set; }
        public int Credit { get; set; }
        public int VirtualDebit { get; set; }
        public int Quanity { get; set; }
        public string LastIdTransact { get; set; }
        public ulong CountUpdate { get; set; }
    }
}

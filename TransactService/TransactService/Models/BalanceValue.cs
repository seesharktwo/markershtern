using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace TransactService.Models
{
    public class BalanceValue
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string IdBalance { get; set; }
        public string UserId { get; set; }
        public decimal Credit { get; set; }
        public decimal VirtualDebit { get; set; }
        public decimal Summ { get; set; }
        public string LastIdTransact { get; set; }
        public ulong CountUpdate { get; set; }
    }
}

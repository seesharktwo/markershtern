using MongoDB.Bson;

namespace UserBagMicroservice.Data.Abstraction
{
    public abstract class Document : IDocument
    {
        public ObjectId Id { get; set; }
        public DateTime CreatedAt => Id.CreationTime;
    }
}

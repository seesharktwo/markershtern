using UserBagMicroservice.Data.Abstraction;

namespace UserBagMicroservice.Models
{
    [BsonCollection("products")]
    public class Product : Document
    {
        public string Name { get; set; } = null!;
        public string AuthorId { get; set; } = null!;

    }
}

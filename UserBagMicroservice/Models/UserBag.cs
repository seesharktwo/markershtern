using UserBagMicroservice.Data.Abstraction;

namespace UserBagMicroservice.Models
{
    [BsonCollection("users_bags")]
    public class UserBag : Document
    {
        public List<Product> Products { get; set; }
    }
}

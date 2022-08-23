using UserBagMicroservice.Data.Abstraction;

namespace UserBagMicroservice.Models
{
    [BsonCollection("users_bags")]
    public class UserBag : Document
    {
        public List<UserBagProduct> Products { get; set; } = null!;
    }
}

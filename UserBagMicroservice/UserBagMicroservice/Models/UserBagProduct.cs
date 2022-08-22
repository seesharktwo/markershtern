using UserBagMicroservice.Data.Abstraction;

namespace UserBagMicroservice.Models
{
    public class UserBagProduct : Document
    {
        public int Quantity { get; set; }

        public string TransactionId { get; set; } = null!;
    }
}

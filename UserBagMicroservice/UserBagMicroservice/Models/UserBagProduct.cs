using UserBagMicroservice.Data.Abstraction;

namespace UserBagMicroservice.Models
{
    public class UserBagProduct : Document
    {
        public int Quantity { get; set; } = 0;

        public string TransactionId { get; set; } = "";
    }
}

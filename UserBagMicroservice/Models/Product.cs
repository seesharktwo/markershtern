using UserBagMicroservice.Data.Abstraction;

namespace UserBagMicroservice.Models
{
    public class Product : Document, ICloneable
    {
        public string Name { get; set; }
        public string AuthorId { get; set; }
        public int Quantity { get; set; }

        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}

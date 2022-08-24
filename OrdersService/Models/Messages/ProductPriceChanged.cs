using OrdersService.Models.Enums;

namespace OrdersService.Models.Messages
{
    public class ProductPriceChanged
    {
        public string ProductId { get; set; }
        public string ProductName { get; set; }
        public PriceType PriceType { get; set; }
        public decimal Price { get; set; }
    }
}

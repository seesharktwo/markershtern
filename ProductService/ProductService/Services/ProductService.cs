using ProductService.Protos.Events;

namespace ProductService.Services
{
    public class ProductService
    {
        private readonly ProductContext _context;
        public ProductService(ProductContext context)
        {
            _context = context;
        }

        public async Task ProcessingSellOrder(SellOrderCreated order)
        {
            var product = await _context.GetByNameAsync(order.Name);
            if (product is null)
            {
                product = new Models.Product
                {
                    Name = order.Name,
                    Ask = order.Price,
                    Bid = 0
                };
                await _context.CreateAsync(product);
            }
            else if (product.Ask < order.Price)
            {
                product.Ask = order.Price;
                _context.UpdateAsync(product.Id, product);
            }
        }

        public async Task ProcessingBuyOrder(BuyOrderCreated order)
        {
            var product = await _context.GetByNameAsync(order.Name);
            if (product is null)
            {
                product = new Models.Product
                {
                    Name = order.Name,
                    Bid = order.Price,
                    Ask = 0
                };
                await _context.CreateAsync(product);
            }
            else if (product.Bid < order.Price)
            {
                product.Bid = order.Price;
                _context.UpdateAsync(product.Id, product);
            }
        }

        public async Task ProcessingPriceChangedEvent(ProductPriceChanged order)
        {
            var product = await _context.GetAsync(order.ProductId);
            if (!(product is null))
            {
                switch (order.Type)
                {
                    case PriceType.Ask:
                        product.Ask = order.Price;

                        break;

                    case PriceType.Bid:
                        product.Bid = order.Price;

                        break;
                }
                _context.UpdateAsync(product.Id, product);
            }
        }
    }
}

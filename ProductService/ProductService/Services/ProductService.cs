using ProductService.Protos.Events;

namespace ProductService.Services
{
    /// <summary>
    /// Product service, here are procesing events of BuyOrderCreated,SellorderCreated,ProudctPriceChanged
    /// </summary>
    public class ProductService
    {
        private readonly ProductContext _context;
        public ProductService(ProductContext context)
        {
            _context = context;
        }

        /// <summary>
        /// method that processing sellOrderCreated event.
        /// result is create/update document of product with ask.
        /// </summary>
        /// <param name="sellOrderCreated"></param>
        /// <returns></returns>
        public async Task ProcessingSellOrder(SellOrderCreated sellOrderCreated)
        {
            var product = await _context.GetByNameAsync(sellOrderCreated.Name);
            decimal price = sellOrderCreated.Price;
            if (product is null) // когда нет такого товара в бд - необходимо его создать
            {
                product = new Models.Product
                {
                    Name = sellOrderCreated.Name,
                    Ask = sellOrderCreated.Price,
                    Bid = 0
                };
                await _context.CreateAsync(product);
            }
            else if (product.Ask < price)
            {
                product.Ask = sellOrderCreated.Price;
                await _context.UpdateAsync(product.Id, product);
            }
        }
        /// <summary>
        /// method that processing buyOrderCreated event.
        /// result is create/update document of product with bid.
        /// </summary>
        /// <param name="buyOrderCreated"></param>
        /// <returns></returns>
        public async Task ProcessingBuyOrder(BuyOrderCreated buyOrderCreated)
        {
            var product = await _context.GetByNameAsync(buyOrderCreated.Name);
            if (product is null) // когда нет такого товара в бд - необходимо его создать
            {
                product = new Models.Product
                {
                    Name = buyOrderCreated.Name,
                    Bid = buyOrderCreated.Price,
                    Ask = 0
                };
                await _context.CreateAsync(product);
            }
            else if (product.Bid < buyOrderCreated.Price)
            {
                product.Bid = buyOrderCreated.Price;
                await _context.UpdateAsync(product.Id, product);
            }
        }

        /// <summary>
        /// method that processing productPriceChanged event.
        /// result is update document of product with bid or ask.
        /// </summary>
        /// <param name="productPriceChanged"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public async Task ProcessingPriceChangedEvent(ProductPriceChanged productPriceChanged)
        {
            if (productPriceChanged.Name is null)
                throw new ArgumentException("product's name is null");

            var product = await _context.GetByNameAsync(productPriceChanged.Name);

            if (product is null)
                throw new ArgumentException($"product \"{productPriceChanged.Name}\" is not found");
            switch (productPriceChanged.Type)
            {
                case PriceType.Ask:
                    product.Ask = productPriceChanged.Price;

                    break;

                case PriceType.Bid:
                    product.Bid = productPriceChanged.Price;

                    break;
            }
            await _context.UpdateAsync(product.Id, product);
        }
    }
}

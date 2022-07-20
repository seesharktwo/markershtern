

using Grpc.Core;

namespace ProductService.Services
{
    public class ProductServiceGrpc : Product.ProductBase
    {
        private readonly ProductContext _context;
        private readonly ILogger<ProductServiceGrpc> _logger;
        public ProductServiceGrpc(ILogger<ProductServiceGrpc> logger, ProductContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async override Task<AllProductsResponce> GetAllProducts(AllProductsRequest request, ServerCallContext context)
        {
            var productsMessages = new List<ProductResponce>();
            var responce = new AllProductsResponce();
            var products = await _context.GetAsync();
            products.ForEach(p =>
            {
                var productResponce = Mapper.Map<Models.Product, ProductResponce>(p);
                productsMessages.Add(productResponce);
            }); 
            responce.Products.AddRange(productsMessages);
            return responce;
        }

    }
}

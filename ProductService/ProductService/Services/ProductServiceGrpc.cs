using Grpc.Core;
using ProductService.Protos.Services;

namespace ProductService.Services
{
    // Сервис реализующий контракт на получение списка с Bid and Ask
    public class ProductServiceGrpc : Protos.Services.ProductService.ProductServiceBase
    {
        private readonly ProductContext _context;

        private readonly ILogger<ProductServiceGrpc> _logger;
        public ProductServiceGrpc(ILogger<ProductServiceGrpc> logger, ProductContext context)
        {   
            _logger = logger;
            _context = context;
        }



        public override async Task<GetProductsResponse> GetProducts(GetProductsRequest request, ServerCallContext context)
        {
            var responce = new GetProductsResponse();
            var products = await  _context.GetAsync();
            products.ForEach(p =>
            {
                var productResponce = Mapper.Map<Models.Product, GetProductsResponse.Types.Product>(p);
                responce.Products.Add(productResponce);
            }); 
            return responce;
        }

    }
}

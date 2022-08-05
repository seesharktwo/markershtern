using Grpc.Core;
using ProductService.Protos.Services;

namespace ProductService.Services
{
    /// <summary>
    /// implementation of proto contract
    /// </summary>
    public class ProductServiceGrpc : Protos.Services.ProductService.ProductServiceBase
    {
        private readonly ProductContext _context;

        public ProductServiceGrpc( ProductContext context)
        {   
            _context = context;
        }


        /// <summary>
        /// returns list of products with bid and ask
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <returns></returns>
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

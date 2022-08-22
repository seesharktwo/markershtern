using AutoMapper;
using Grpc.Core;
using Product;
using ProductService.Mapper;

namespace ProductService.Services
{
    /// <summary>
    /// implementation of proto contract
    /// </summary>
    public class ProductServiceGrpc : Product.ProductService.ProductServiceBase
    {
        private readonly ProductContext _context;
        private readonly IMapper _mapper;

        public ProductServiceGrpc(ProductContext context, IMapper mapper)
        {   
            _context = context;
            _mapper = mapper;
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
                var productResponce = _mapper.Map<Models.Product, GetProductsResponse.Types.Product>(p);
                responce.Products.Add(productResponce);
            }); 
            return responce;
        }

    }
}

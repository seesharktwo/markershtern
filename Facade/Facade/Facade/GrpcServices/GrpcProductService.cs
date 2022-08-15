using Facade.Mapper;
using Facade.Services;
using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Facade.GrpcServices
{
    public class GrpcProductService : ProductClient.ProductServiceClient.ProductServiceClientBase
    {
        private Services.ProductService _productService;
        private IMapper _mapper;

        public GrpcProductService(Services.ProductService productService, Facade.Mapper.IMapper mapper)
        {
            _productService = productService;
            _mapper = mapper;
        }

        public override async Task<ProductClient.GetProductsResponse> GetProducts(ProductClient.GetProductsRequest request, ServerCallContext context)
        {
            var mappedRequest = _mapper.Map<ProductClient.GetProductsRequest, Product.GetProductsRequest>(request);
            Product.GetProductsResponse response =
                        await _productService.GetProductsAsync();
            if (response is null)
                throw new RpcException(Status.DefaultCancelled,
                    "Exception in getting products for marketing");
            var mappedResponse = _mapper.Map<Product.GetProductsResponse,
                ProductClient.GetProductsResponse>(response);
            return mappedResponse;
        }
    }
}

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

        public GrpcProductService(Services.ProductService productService)
        {
            _productService = productService;
        }

        public override async Task<ProductClient.GetProductsResponse> GetProducts(ProductClient.GetProductsRequest request, ServerCallContext context)
        {
            var mappedRequest = Mapper.Map<ProductClient.GetProductsRequest, Product.GetProductsRequest>(request);
            Product.GetProductsResponse response =
                        await _productService.GetProductsAsync();
            if (response is null)
                throw new RpcException(Status.DefaultCancelled,
                    "Exception in getting products for marketing");
            var mappedResponse = Mapper.Map<Product.GetProductsResponse,
                ProductClient.GetProductsResponse>(response);
            return mappedResponse;
        }
    }
}

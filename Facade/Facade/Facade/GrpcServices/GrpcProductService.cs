using Facade.Mapper;
using Facade.Services;
using Grpc.Core;
using Microsoft.Extensions.Logging;
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
        private ILogger<GrpcProductService> _logger;

        public GrpcProductService(Services.ProductService productService, Facade.Mapper.IMapper mapper, ILogger<GrpcProductService> logger)
        {
            _productService = productService;
            _mapper = mapper;
            _logger = logger;
        }

        public override async Task<ProductClient.GetProductsResponse> GetProducts(ProductClient.GetProductsRequest request, ServerCallContext context)
        {

            var mappedRequest = _mapper.Map<ProductClient.GetProductsRequest, Product.GetProductsRequest>(request);
            try
            {
                Product.GetProductsResponse response =
                        await _productService.GetProductsAsync();
                var mappedResponse = _mapper.Map<Product.GetProductsResponse,
                ProductClient.GetProductsResponse>(response);
                return mappedResponse;
            }
            catch (ArgumentNullException ex)
            {
                _logger.LogError(ex.Message);
                throw new RpcException(Status.DefaultCancelled,
                    "Exception in getting products for marketing");
            }
            
        }
    }
}

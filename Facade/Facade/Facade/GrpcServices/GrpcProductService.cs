using Facade.Services;
using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Facade.GrpcServices
{
    public class GrpcProductService : FacadeClient.ProductServiceClient.ProductServiceClientBase
    {
        private GetListTradeProducts _productService;

        public GrpcProductService(Services.GetListTradeProducts productService)
        {
            _productService = productService;
        }

        public override async Task<FacadeClient.GetProductsResponse> GetProducts(FacadeClient.GetProductsRequest request, ServerCallContext context)
        {
            var mappedRequest = Mapper.Map<FacadeClient.GetProductsRequest, Facade.GetProductsRequest>(request);
            (bool isComplite, Facade.GetProductsResponse responce, Exception exception) creationResult =
                        await _productService.GetProductsAsync();
            if (!creationResult.isComplite)
                throw new RpcException(Status.DefaultCancelled, creationResult.exception.Message);
            var mappedResponse = Mapper.Map<Facade.GetProductsResponse, FacadeClient.GetProductsResponse>(creationResult.responce);
            return mappedResponse;
        }
    }
}

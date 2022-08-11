using Facade.Services;
using Grpc.Core;
using OrdersClient;
using System;
using System.Threading.Tasks;

namespace Facade.GrpcServices
{
    public class GrpcOrderService : OrdersClient.OrdersClient.OrdersClientBase
    {
        private OrderService _orderService;

        public GrpcOrderService(OrderService orderService)
        {
            _orderService = orderService;
        }
        
        

        public override async Task<CreateOrderResponse> CreateOrder(CreateOrderRequest request, ServerCallContext context)
        {
            var mappedRequest = Mapper.Map<OrdersClient.CreateOrderRequest, Orders.CreateOrderRequest>(request);
            (bool isComplite, Orders.CreateOrderResponse responce, Exception exception) creationResult =
                        await _orderService.CreateOrderAsync(mappedRequest);
            if (!creationResult.isComplite)
                throw new RpcException(Status.DefaultCancelled, creationResult.exception.Message);
            var mappedResponse = Mapper.Map<Orders.CreateOrderResponse, OrdersClient.CreateOrderResponse>(creationResult.responce);
            return mappedResponse;
        }
    }
}

using Facade.Services;
using Grpc.Core;
using OrderClient;
using System;
using System.Threading.Tasks;

namespace Facade.GrpcServices
{
    public class GrpcOrderService : OrderServiceClient.OrderServiceClientBase
    {
        private OrderService _orderService;

        public GrpcOrderService(OrderService orderService)
        {
            _orderService = orderService;
        }
        
        

        public override async Task<CreateOrderResponse> CreateOrder(CreateOrderRequest request, ServerCallContext context)
        {
            var mappedRequest = Mapper.Map<CreateOrderRequest, Order.CreateOrderRequest>(request);
            Order.CreateOrderResponse response =
                        await _orderService.CreateOrderAsync(mappedRequest);
            if (response is null)
                throw new RpcException(Status.DefaultCancelled, "Exception in creating order");
            var mappedResponse = Mapper.Map<Order.CreateOrderResponse, CreateOrderResponse>(response);
            return mappedResponse;
        }
    }
}

using Facade.Mapper;
using Facade.Services;
using Grpc.Core;
using OrderClient;
using System;
using System.Threading.Tasks;

namespace Facade.GrpcServices
{
    public class GrpcOrderService : OrderServiceClient.OrderServiceClientBase
    {
        private IMapper _mapper;
        private OrderService _orderService;

        public GrpcOrderService(OrderService orderService, Facade.Mapper.IMapper mapper)
        {
            _mapper = mapper;
            _orderService = orderService;
        }
        
        

        public override async Task<CreateOrderResponse> CreateOrder(CreateOrderRequest request, ServerCallContext context)
        {
            var mappedRequest = _mapper.Map<CreateOrderRequest, Order.CreateOrderRequest>(request);
            Order.CreateOrderResponse response =
                        await _orderService.CreateOrderAsync(mappedRequest);
            if (response is null)
                throw new RpcException(Status.DefaultCancelled, "Exception in creating order");
            var mappedResponse = _mapper.Map<Order.CreateOrderResponse, CreateOrderResponse>(response);
            return mappedResponse;
        }
    }
}

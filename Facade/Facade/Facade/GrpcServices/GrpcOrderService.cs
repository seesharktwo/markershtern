using AutoMapper;
using Facade.Services;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using OrderClient;
using System;
using System.Threading.Tasks;

namespace Facade.GrpcServices
{
    public class GrpcOrderService : OrderProcessingClient.OrderProcessingClientBase
    {
        private IMapper _mapper;
        private OrderService _orderService;
        private ILogger<GrpcOrderService> _logger;

        public GrpcOrderService(OrderService orderService, IMapper mapper, ILogger<GrpcOrderService> logger)
        {
            _mapper = mapper;
            _orderService = orderService;
            _logger = logger;
        }
        
        

        public override async Task<CreateOrderResponse> CreateOrder(CreateOrderRequest request, ServerCallContext context)
        {
            var mappedRequest = _mapper.Map<CreateOrderRequest, Order.CreateOrderRequest>(request);
            try
            {
                Order.CreateOrderResponse response =
                        await _orderService.CreateOrderAsync(mappedRequest);
                var mappedResponse = _mapper.Map<Order.CreateOrderResponse, CreateOrderResponse>(response);
                return mappedResponse;
            }
            catch(ArgumentNullException ex)
            {
                _logger.LogError(ex.Message);
                throw new RpcException(Status.DefaultCancelled, "Exception in creating order");
            }
            
        }
    }
}

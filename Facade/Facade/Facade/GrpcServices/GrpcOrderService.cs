using AutoMapper;
using Facade.Services;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Facade.GrpcServices
{
    public class GrpcOrderService : OrderForClient.OrderProcessingForClient.OrderProcessingForClientBase
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
        
        

        public override async Task<OrderForClient.CreateOrderResponse> CreateOrder(OrderForClient.CreateOrderRequest request, ServerCallContext context)
        {
            var mappedRequest = _mapper.Map<OrderForClient.CreateOrderRequest, OrderProtos.CreateOrderRequest>(request);
            try
            {
                OrderProtos.CreateOrderResponse response =
                        await _orderService.CreateOrderAsync(mappedRequest);
                var mappedResponse = _mapper.Map<OrderProtos.CreateOrderResponse, OrderForClient.CreateOrderResponse>(response);
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

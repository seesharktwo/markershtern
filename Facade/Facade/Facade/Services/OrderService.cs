using Confluent.Kafka;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using OrderProtos;
using System;
using Microsoft.Extensions.Configuration;
using Grpc.Net.Client;
using System.IO;
using Microsoft.Extensions.Logging;

namespace Facade.Services
{
    public class OrderService
    {
        private OrderProtos.OrderProcessing.OrderProcessingClient _client;
        private ILogger<OrderService> _logger;

        public OrderService(OrderProtos.OrderProcessing.OrderProcessingClient client, ILogger<OrderService> logger)
        {
            _client = client;
            _logger = logger;
        }

        /// <summary>
        /// Method create's new order
        /// </summary>
        /// <param name="request"></param>
        /// <returns>CreateOrderResponse</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public async Task<CreateOrderResponse>
            CreateOrderAsync(CreateOrderRequest request)
        {
            
            CreateOrderResponse response = await LoadCreateOrderResponseAsync(request);

            

            if (response is null)
                throw new ArgumentNullException("CreateOrderResponse");

            return response;
            
        }

        public async Task<CreateOrderResponse> LoadCreateOrderResponseAsync(CreateOrderRequest request)
        {
            var reply = await _client.CreateOrderAsync(request);
            return reply;
        }

    }
}

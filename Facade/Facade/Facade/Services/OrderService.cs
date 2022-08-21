using Confluent.Kafka;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using Order;
using System;
using Microsoft.Extensions.Configuration;
using Grpc.Net.Client;
using System.IO;
using Microsoft.Extensions.Logging;

namespace Facade.Services
{
    public class OrderService
    {
        private Order.OrderProcessing.OrderProcessingClient _client;
        private ILogger<OrderService> _logger;

        public OrderService(Order.OrderProcessing.OrderProcessingClient client, ILogger<OrderService> logger)
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

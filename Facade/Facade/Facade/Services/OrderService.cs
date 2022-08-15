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
        private Orders.OrdersClient _client;
        private ILogger<OrderService> _logger;

        public OrderService(Orders.OrdersClient client, ILogger<OrderService> logger)
        {
            _client = client;
            _logger = logger;
        }

        /// <summary>
        /// Method create's new order
        /// </summary>
        /// <param name="request"></param>
        /// <returns>CreateOrderResponse if success, null if catches error</returns>
        public async Task<CreateOrderResponse>
            CreateOrderAsync(CreateOrderRequest request)
        {
            try
            {
                CreateOrderResponse responce = await LoadCreateOrderResponseAsync(request);
                return responce;
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return null;
            }
        }

        public async Task<CreateOrderResponse> LoadCreateOrderResponseAsync(CreateOrderRequest request)
        {
            var reply = await _client.CreateOrderAsync(request);
            return reply;
        }

    }
}

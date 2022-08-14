using Confluent.Kafka;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using Orders;
using System;
using Microsoft.Extensions.Configuration;
using Grpc.Net.Client;
using System.IO;
using Microsoft.Extensions.Logging;

namespace Facade.Services
{
    public class OrderService
    {
        private Orders.Orders.OrdersClient _client;
        private ILogger<OrderService> _logger;

        public OrderService(Orders.Orders.OrdersClient client, ILogger<OrderService> logger)
        {
            _client = client;
            _logger = logger;
        }

        public async Task<(bool isComplite, CreateOrderResponse responce, Exception exception)>
            CreateOrderAsync(CreateOrderRequest request)
        {
            Exception exception = null;
            CreateOrderResponse responce = null;
            try
            {
                responce = await LoadCreateOrderResponseAsync(request);
                return (true, responce, exception);
            }
            catch (Exception e)
            {
                exception = e;
            }

            return (false, responce, exception);
        }

        public async Task<CreateOrderResponse> LoadCreateOrderResponseAsync(CreateOrderRequest request)
        {
            var reply = await _client.CreateOrderAsync(request);
            return reply;
        }

    }
}

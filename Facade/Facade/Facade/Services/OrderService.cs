using Confluent.Kafka;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using Orders;
using System;
using Microsoft.Extensions.Configuration;
using Grpc.Net.Client;
using System.IO;
using Facade.Сonfigs;
using Microsoft.Extensions.Logging;

namespace Facade.Services
{
    public class OrderService
    {
        private string _connectionString;
        private ILogger<OrderService> _logger;

        public OrderService(IOptions<ConnectionString<OrderService>> config, ILogger<OrderService> logger)
        {
            _connectionString = config.Value.String;
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
            using (var channel = GrpcChannel.ForAddress(_connectionString))
            {
                var client = new Orders.Orders.OrdersClient(channel);
                var reply = await client.CreateOrderAsync(request);
                return reply;
            }
        }

    }
}

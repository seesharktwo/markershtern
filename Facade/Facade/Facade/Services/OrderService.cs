using Confluent.Kafka;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using Orders;
using System;
using Microsoft.Extensions.Configuration;
using Grpc.Net.Client;
using System.IO;

namespace Facade.Services
{
    public class OrderService
    {
        private string _connectionString;

        public OrderService()
        {
            _connectionString = GetConnectionString();
        }

        public async Task<(bool isComplite, CreateOrderResponce responce, Exception exception)>
            CreateOrderAsync(CreateOrderRequest request)
        {
            Exception exception = null;
            CreateOrderResponce responce = null;
            try
            {
                responce = await LoadDataAsync(request);
                return (true, responce, exception);
            }
            catch (Exception e)
            {
                exception = e;
            }

            return (false, responce, exception);
        }

        public async Task<CreateOrderResponce> LoadDataAsync(CreateOrderRequest request)
        {
            using (var channel = GrpcChannel.ForAddress(_connectionString))
            {
                var client = new Orders.OrderService.OrderServiceClient(channel);
                var reply = await client.CreateOrderAsync(request);
                return reply;
            }
        }

        private string GetConnectionString()
        {
            string result = string.Empty;
            try
            {
                var config = new ConfigurationBuilder();
                config.SetBasePath(Directory.GetCurrentDirectory());
                config.AddJsonFile("appsettings.json");
                IConfigurationRoot root = config.Build();
                result = root["ConnectionOrderMicroservice"];
            }
            catch(Exception e)
            {
                //To DO log
            }
            return result;
        }

    }
}

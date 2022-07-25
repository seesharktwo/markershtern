using Grpc.Net.Client;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Facade.Services
{
    public class GetListTradeProducts
    {
        readonly string _connectionString;
        public GetListTradeProducts()
        {
            _connectionString = GetConnectionString();
        }

        public async Task<(bool isComplite, List<Product> products, Exception exception)>
            GetProductsAsync()
        {
            Exception exception = null;
            List<Product> products = new List<Product>();

            try
            {
                products = await LoadDataAsync();
                return (true, products, exception);
            }
            catch(Exception e)
            {
                exception = e;
            }

            return (false, products, exception);
        }

        private async Task<List<Product>> LoadDataAsync()
        {
            using (var channel = GrpcChannel.ForAddress(_connectionString)) 
            {
                var client = new ProductService.ProductServiceClient(channel);
                var reply = await client.GetProductsAsync(new GetProductsRequest());
                return reply.Products.ToList();
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
                result = root["ConnectionTradeCargoMicroservice"];
            }
            catch
            {
                //To DO log
            }
            return result;
        }
    }
}

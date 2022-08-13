
using Facade.Сonfigs;
using Grpc.Net.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static Facade.GetProductsResponse.Types;

namespace Facade.Services
{
    public class GetListTradeProducts
    {
        readonly string _connectionString;

        public GetListTradeProducts(IOptions<ConnectionString<OrderService>> config)
        {
            _connectionString = config.Value.String;
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
                var client = new Facade.ProductService.ProductServiceClient(channel);
                var reply = await client.GetProductsAsync(new GetProductsRequest());
                return reply.Products.ToList();
            }
        }
    }
}

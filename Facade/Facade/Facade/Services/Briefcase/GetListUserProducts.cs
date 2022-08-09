using Grpc.Net.Client;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Facade2;

namespace Facade.Services.Briefcase
{
    public class GetListUserProducts
    {
        readonly string _connectionString;
        public GetListUserProducts()
        {
            _connectionString = GetConnectionString();
        }

        public async Task<(bool isComplite, List<Product> products, Exception exception)>
            GetProductsAsync(string userId)
        {
            Exception exception = null;
            List<Product> products = new List<Product>();

            try
            {
                products = await LoadDataAsync(userId);
                return (true, products, exception);
            }
            catch (Exception e)
            {
                exception = e;
            }

            return (false, products, exception);
        }

        private async Task<List<Product>> LoadDataAsync(string userId)
        {
            var request = new GetUserProductsRequest { UserId = userId };

            using (var channel = GrpcChannel.ForAddress(_connectionString))
            {
                var client = new UserBriefcase.UserBriefcaseClient(channel);
                var reply = await client.GetUserProductsAsync(request);

                if (reply.ResponseCase.Equals(GetUserProductsResponse.ResponseOneofCase.Error))
                {
                    throw new Exception(reply.Error.ToString());
                }

                return reply.List.Products.ToList();
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
                result = root["ConnectionUserBrifcaseMicroservice"];
            }
            catch
            {
                //To DO log
            }
            return result;
        }
    }
}

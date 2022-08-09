using Facade2;
using Grpc.Net.Client;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Facade.Services.Briefcase
{
    public class RemoveProduct
    {
        readonly string _connectionString;
        public RemoveProduct()
        {
            _connectionString = GetConnectionString();
        }

        public async Task<(bool isComplite, Exception exception)>
            GetProductsAsync(string userId, string productId, string authorId)
        {
            Exception exception = null;

            try
            {
                await LoadDataAsync(userId, productId, authorId);
                return (true, exception);
            }
            catch (Exception e)
            {
                exception = e;
            }

            return (false, exception);
        }

        private async Task<SuccessResponse> LoadDataAsync(string userId, string productId, string authorId)
        {
            var request = new RemoveProductRequest { UserId = userId, ProductId = productId, AuthorId = authorId};

            using (var channel = GrpcChannel.ForAddress(_connectionString))
            {
                var client = new UserBriefcase.UserBriefcaseClient(channel);
                var reply = await client.RemoveProductAsync(request);

                if (reply.ResultCase.Equals(AddProductResponse.ResultOneofCase.Error))
                {
                    throw new Exception(reply.Error.ToString());
                }

                return reply.Success;
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

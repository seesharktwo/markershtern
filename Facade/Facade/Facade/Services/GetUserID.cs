using Grpc.Net.Client;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Facade.Services
{
    public class GetUserID
    {
        readonly string _connectionString;

        public GetUserID()
        {
            _connectionString = GetConnectionString();
        }

        public async Task<(bool isComplite, string userId, Exception exception)>
            GetUserId(string login, string password)
        {
            Exception exception = null;
            string userId = null;

            try
            {
                userId = await LoadDataAsync(login, password);
                return (true, userId, exception);
            }
            catch (Exception e)
            {
                exception = e;
            }

            return (false, userId, exception);
        }

        private async Task<string> LoadDataAsync(string login, string password)
        {
            var request = new LoginRequest{ Login = login, Password = password };

            using (var channel = GrpcChannel.ForAddress(_connectionString))
            {
                var client = new Authorization.AuthorizationClient(channel);
                var reply = await client.LoginAsync(request);

                return reply.UserId;
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

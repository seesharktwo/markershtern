
using Grpc.Net.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Facade.Services
{
    public class GetUserID
    {
        private Authorization.AuthorizationClient _client;

        public GetUserID(Authorization.AuthorizationClient client)
        {
            _client = client;
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

            var reply = await _client.LoginAsync(request);

            return reply.UserId;
        }

    }
}

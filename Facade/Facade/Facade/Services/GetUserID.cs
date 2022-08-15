
using Authorization;
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
        private AuthorizationService.AuthorizationServiceClient _client;
        private ILogger<GetUserID> _logger;

        public GetUserID(AuthorizationService.AuthorizationServiceClient client, ILogger<GetUserID> logger)
        {
            _client = client;
            _logger = logger;
        }

        /// <summary>
        /// Login method
        /// </summary>
        /// <param name="login"></param>
        /// <param name="password"></param>
        /// <returns><LoginResponse/returns>
        /// <exception cref="ArgumentNullException"></exception>
        public async Task<LoginResponse>
            GetUserId(string login, string password)
        {
            LoginResponse response = await LoadLoginResponseAsync(login, password);

            if (response is null)
                throw new ArgumentNullException("LoginResponse");

            return response;
        }

        private async Task<LoginResponse> LoadLoginResponseAsync(string login, string password)
        {
            var request = new LoginRequest{ Login = login, Password = password };

            var reply = await _client.LoginAsync(request);
            
            return reply;
        }

    }
}

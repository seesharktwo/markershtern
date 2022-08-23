using Protos.CustomTypes;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Facade.Services
{
    public class UserBalanceService
    {
        private Protos.CustomTypes.BalanceService.BalanceServiceClient _client;
        private ILogger<UserBalanceService> _logger;

        public UserBalanceService(Protos.CustomTypes.BalanceService.BalanceServiceClient client, ILogger<UserBalanceService> logger)
        {
            _client = client;
            _logger = logger;
            
        }
        /// <summary>
        /// Method for getting balance
        /// </summary>
        /// <returns>GetProductsResponse</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public async Task<GetBalanceResponse> GetBalanceAsync(GetBalanceRequest request)
        {
            GetBalanceResponse response = await LoadtGetBalanceResponseAsync(request);

            if (response is null)
                throw new ArgumentNullException("GetProductsResponse");


            return response;
        }

        private async Task<GetBalanceResponse> LoadtGetBalanceResponseAsync(GetBalanceRequest request)
        {
            var reply = await _client.GetBalanceAsync(request);
            return reply;
        }
    }
}

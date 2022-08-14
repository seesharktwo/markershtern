
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
        private ProductService.ProductServiceClient _client;

        public GetListTradeProducts(ProductService.ProductServiceClient client)
        {
            _client = client;
        }

        public async Task<(bool isComplite, GetProductsResponse products, Exception exception)>
            GetProductsAsync()
        {
            Exception exception = null;
            GetProductsResponse response = new GetProductsResponse();

            try
            {
                response = await LoadDataAsync();
                return (true, response, exception);
            }
            catch(Exception e)
            {
                exception = e;
            }

            return (false, response, exception);
        }

        private async Task<GetProductsResponse> LoadDataAsync()
        {
            var reply = await _client.GetProductsAsync(new GetProductsRequest());
            return reply;
        }
    }
}

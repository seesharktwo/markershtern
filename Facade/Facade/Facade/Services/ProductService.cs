
using Grpc.Net.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Product;

namespace Facade.Services
{
    public class ProductService
    {
        private Product.ProductService.ProductServiceClient _client;
        private ILogger<ProductService> _logger;

        public ProductService(Product.ProductService.ProductServiceClient client, ILogger<ProductService> logger)
        {
            _client = client;
            _logger = logger;
        }
        /// <summary>
        /// Method for getting products for marketing
        /// </summary>
        /// <returns>GetProductsResponse</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public async Task<GetProductsResponse>
            GetProductsAsync()
        {
          
            GetProductsResponse response = await LoadtGetProductsResponseAsync();

            if (response is null)
                throw new ArgumentNullException("GetProductsResponse");


            return response;
   

        }

        private async Task<GetProductsResponse> LoadtGetProductsResponseAsync()
        {
            var reply = await _client.GetProductsAsync(new GetProductsRequest());
            return reply;
        }
    }
}

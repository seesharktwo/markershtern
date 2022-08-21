using Grpc.Net.Client;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Briefcase;
using Google.Protobuf;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

namespace Facade.Services
{
    public class UserBriefcaseService
    {
        private Briefcase.UserBriefcaseService.UserBriefcaseServiceClient _client;
        private ILogger<UserBriefcaseService> _logger;

        public UserBriefcaseService(Briefcase.UserBriefcaseService.UserBriefcaseServiceClient client, ILogger<UserBriefcaseService> logger)
        {
            _client = client;
            _logger = logger;
        }

        /// <summary>
        /// Get products from briefcase micro-service method
        /// </summary>
        /// <param name="request"></param>
        /// <returns>GetUserResponse</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="Exception"></exception>
        public async Task<GetUserProductsResponse>
            GetUserProductsAsync(GetUserProductsRequest request)
        {

            var reply = await _client.GetUserProductsAsync(request);

            if (reply is null)
                throw new ArgumentNullException("GetUserProductsResponse");


            if (reply.ResultCase.Equals(AddProductResponse.ResultOneofCase.Error))
            {
                throw new Exception(reply.Error.ToString());
            }

            return reply;
        }

        /// <summary>
        /// Add product to briefcase micro-service method
        /// </summary>
        /// <param name="request"></param>
        /// <returns>AddProductResponse</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="Exception"></exception>
        public async Task<AddProductResponse>
          AddProduct(AddProductRequest request)
        {
            var reply = await _client.AddProductAsync(request);

            if (reply is null)
                throw new ArgumentNullException("GetUserProductsResponse");

            if (reply.ResultCase.Equals(AddProductResponse.ResultOneofCase.Error))
            {
                throw new Exception(reply.Error.ToString());
            }

            return reply;

        }

        /// <summary>
        /// Remove product from briefcase micro-service method
        /// </summary>
        /// <param name="request"></param>
        /// <returns>RemoveProductResponse</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="Exception"></exception>
        public async Task<RemoveProductResponse>
           RemoveProduct(RemoveProductRequest request)
        {
            var reply = await _client.RemoveProductAsync(request);

            if (reply is null)
                throw new ArgumentNullException("GetUserProductsResponse");


            if (reply.ResultCase.Equals(RemoveProductResponse.ResultOneofCase.Error))
            {
                throw new Exception(reply.Error.ToString());
            }

            return reply;
        }

    }
}

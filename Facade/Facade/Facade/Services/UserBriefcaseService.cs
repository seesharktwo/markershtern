using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Briefcase;


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
            {
                throw new ArgumentNullException("GetUserProductsResponse");
            }

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
          AddProductAsync(AddProductRequest request)
        {
            var reply = await _client.AddProductAsync(request);

            if (reply is null)
            {
                throw new ArgumentNullException("AddProductResponse");
            }

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
           RemoveProductAsync(RemoveProductRequest request)
        {
            var reply = await _client.RemoveProductAsync(request);

            if (reply is null)
            {
                throw new ArgumentNullException("RemoveProductResponse");
            }

            if (reply.ResultCase.Equals(RemoveProductResponse.ResultOneofCase.Error))
            {
                throw new Exception(reply.Error.ToString());
            }

            return reply;
        }


        /// <summary>
        /// Checks the availability of the required amount of product from briefcase micro-service method
        /// </summary>
        /// <param name="request"></param>
        /// <returns>ValidateOrderResponse</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="Exception"></exception>
        public async Task<ValidateOrderResponse>
            ValidateOrderAsync(ValidateOrderRequest request)
        {
            var reply = await _client.ValidateOrderAsync(request);

            if (reply is null)
            {
                throw new ArgumentNullException("ValidateOrderResponse");
            }
                
            if (reply.ResultCase.Equals(RemoveProductResponse.ResultOneofCase.Error))
            {
                throw new Exception(reply.Error.ToString());
            }

            return reply;
        }
    }
}

using Grpc.Net.Client;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Facade2;
using Google.Protobuf;
using Microsoft.Extensions.Options;
using Facade.Сonfigs;
using Microsoft.Extensions.Logging;

namespace Facade.Services
{
    public class UserBriefcaseService
    {
        private UserBriefcase.UserBriefcaseClient _client;
        private Exception _exception;
        private string _connectionString;

        public UserBriefcaseService(Facade2.UserBriefcase.UserBriefcaseClient client)
        {
            _client = client;
        }

        public async Task<(bool isComplite, List<Product> products, Exception exception)>
            GetUserProductsAsync(GetUserProductsRequest request)
        {
            List<Product> products = new List<Product>();

            try
            {
                var reply = await _client.GetUserProductsAsync(request);

                if (reply.ResultCase.Equals(AddProductResponse.ResultOneofCase.Error))
                {
                    throw new Exception(reply.Error.ToString());
                }

                products = reply.List.Products.ToList();

                return (true, products, null);
            }
            catch (Exception e)
            {
                _exception = e;
            }

            return (false, products, _exception);
        }

        public async Task<(bool isComplite, Exception exception)>
          AddProduct(AddProductRequest request)
        {
            try
            {
                var reply = await _client.AddProductAsync(request);

                if (reply.ResultCase.Equals(AddProductResponse.ResultOneofCase.Error))
                {
                    throw new Exception(reply.Error.ToString());
                }

                return (true, null);
            }
            catch (Exception e)
            {
                _exception = e;
            }

            return (false, _exception);
        }

        public async Task<(bool isComplite, Exception exception)>
           RemoveProduct(RemoveProductRequest request)
        {
            try
            {
                var reply = await _client.RemoveProductAsync(request);

                if (reply.ResultCase.Equals(RemoveProductResponse.ResultOneofCase.Error))
                {
                    throw new Exception(reply.Error.ToString());
                }

                return (true, null);
            }
            catch (Exception e)
            {
                _exception = e;
            }

            return (false, _exception);
        }

    }
}

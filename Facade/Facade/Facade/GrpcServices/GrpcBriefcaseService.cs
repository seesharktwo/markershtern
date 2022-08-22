using AutoMapper;
using BriefcaseForClient;
using Facade.Services;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Facade.GrpcServices
{
    public class GrpcBriefcaseService : BriefcaseForClient.UserBriefcaseServiceForClient.UserBriefcaseServiceForClientBase
    {
        private IMapper _mapper;
        private UserBriefcaseService _briefcaseService;
        private ILogger<GrpcBriefcaseService> _logger;

        public GrpcBriefcaseService(UserBriefcaseService briefcaseService, IMapper mapper, ILogger<GrpcBriefcaseService> logger)
        {
            _mapper = mapper;
            _logger = logger;
            _briefcaseService = briefcaseService;
        }

        public async override Task<AddProductResponse> AddProduct(AddProductRequest request, ServerCallContext context)
        {
            var mappedRequest = _mapper.Map<BriefcaseForClient.AddProductRequest, Briefcase.AddProductRequest>(request);
            try
            {
                Briefcase.AddProductResponse response =
                        await _briefcaseService.AddProductAsync(mappedRequest);
                var mappedResponse = _mapper.Map<Briefcase.AddProductResponse,
                                                 BriefcaseForClient.AddProductResponse>(response);
                return mappedResponse;
            }
            catch (ArgumentNullException ex)
            {
                _logger.LogError(ex.Message);
                throw new RpcException(Status.DefaultCancelled, "Exception in creating order");
            }
        }

        public async override Task<GetUserProductsResponse> GetUserProducts(GetUserProductsRequest request, ServerCallContext context)
        {
            var mappedRequest = _mapper.Map<BriefcaseForClient.GetUserProductsRequest, Briefcase.GetUserProductsRequest>(request);
            try
            {
                Briefcase.GetUserProductsResponse response =
                        await _briefcaseService.GetUserProductsAsync(mappedRequest);
                var mappedResponse = _mapper.Map<Briefcase.GetUserProductsResponse,
                                                 BriefcaseForClient.GetUserProductsResponse>(response);
                return mappedResponse;
            }
            catch (ArgumentNullException ex)
            {
                _logger.LogError(ex.Message);
                throw new RpcException(Status.DefaultCancelled, "Exception in creating order");
            }
        }

        public async override Task<RemoveProductResponse> RemoveProduct(RemoveProductRequest request, ServerCallContext context)
        {
            var mappedRequest = _mapper.Map<BriefcaseForClient.RemoveProductRequest, Briefcase.RemoveProductRequest>(request);
            try
            {
                Briefcase.RemoveProductResponse response =
                        await _briefcaseService.RemoveProductAsync(mappedRequest);
                var mappedResponse = _mapper.Map<Briefcase.RemoveProductResponse,
                                                 BriefcaseForClient.RemoveProductResponse>(response);
                return mappedResponse;
            }
            catch (ArgumentNullException ex)
            {
                _logger.LogError(ex.Message);
                throw new RpcException(Status.DefaultCancelled, "Exception in creating order");
            }
        }
    }
}

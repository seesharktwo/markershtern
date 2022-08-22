using AutoMapper;
using BalanceForClient;
using Facade.Services;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Facade.GrpcServices
{
    public class GrpcBalanceService : BalanceForClient.BalanceService.BalanceServiceBase
    {
        private IMapper _mapper;
        private UserBalanceService _balanceService;
        private ILogger<GrpcBalanceService> _logger;

        public GrpcBalanceService(IMapper mapper, UserBalanceService userBalanceService, ILogger<GrpcBalanceService> logger)
        {
            _mapper = mapper;
            _balanceService = userBalanceService;
            _logger = logger;
        }

        public async override Task<GetBalanceResponse> GetBalance(GetBalanceRequest request, ServerCallContext context)
        {
            var mappedRequest = _mapper.Map<BalanceForClient.GetBalanceRequest, Protos.CustomTypes.GetBalanceRequest>(request);
            try
            {
                Protos.CustomTypes.GetBalanceResponse response =
                        await _balanceService.GetBalanceAsync(mappedRequest);
                var mappedResponse = _mapper.Map<Protos.CustomTypes.GetBalanceResponse,
                                                 BalanceForClient.GetBalanceResponse>(response);
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

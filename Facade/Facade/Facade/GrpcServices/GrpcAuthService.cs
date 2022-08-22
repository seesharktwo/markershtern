using AutoMapper;
using Facade.Services;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Facade.GrpcServices
{
    public class GrpcAuthService : AuthorizationForClient.AuthorizationServiceForClient.AuthorizationServiceForClientBase
    {
        private IMapper _mapper;
        private AuthService _authService;
        private ILogger<GrpcAuthService> _logger;

        public GrpcAuthService(AuthService authService, IMapper mapper, ILogger<GrpcAuthService> logger)
        {
            _mapper = mapper;
            _logger = logger;
            _authService = authService;
        }

        public async override Task<AuthorizationForClient.LoginResponse> Login(AuthorizationForClient.LoginRequest request, ServerCallContext context)
        {
            var mappedRequest = _mapper.Map<AuthorizationForClient.LoginRequest, Authorization.LoginRequest>(request);
            try
            {
                Authorization.LoginResponse response =
                        await _authService.GetUserId(request.Login, request.Password);
                var mappedResponse = _mapper.Map<Authorization.LoginResponse,
                                                 AuthorizationForClient.LoginResponse>(response);
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

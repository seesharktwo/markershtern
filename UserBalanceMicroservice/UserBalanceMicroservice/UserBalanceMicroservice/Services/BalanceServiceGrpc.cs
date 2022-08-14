using Grpc.Core;
using UserBalanceMicroservice;
using Protos.CustomTypes;
using AutoMapper;
using UserBalanceMicroservice.Models;
namespace UserBalanceMicroservice.Services
{
    public class BalanceServiceGrpc : BalanceService.BalanceServiceBase
    {
        private readonly ILogger<BalanceServiceGrpc> _logger;
        private readonly BalanceContext _balanceContext;
        public BalanceServiceGrpc(ILogger<BalanceServiceGrpc> logger, BalanceContext balanceContext)
        {
            _logger = logger;
            _balanceContext = balanceContext;
        }

        public async override Task<GetBalanceResponse> GetBalance(GetBalanceRequest request, ServerCallContext context)
        {
            var user = await _balanceContext.GetBalanceAsync(request.Id);

            return
                new GetBalanceResponse()
                {
                    UserId = user.Id,
                    Balance = DecimalValue.FromDecimal(user.Money)
                };
        }
    }
}
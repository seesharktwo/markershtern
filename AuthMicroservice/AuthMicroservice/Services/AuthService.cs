using Authorization;
using Grpc.Core;

namespace AuthMicroservice.Services
{
    public class AuthService : AuthorizationService.AuthorizationServiceBase
    {
        private readonly ILogger<AuthService> _logger;
        private readonly UsersContext _context;

        public AuthService(ILogger<AuthService> logger, UsersContext context)
        {
            _logger = logger;
            _context = context;
        }

        /// <summary>
        /// Validates the user's login and password
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <returns>The LoginResponse message</returns>
        /// <exception cref="RpcException"></exception>
        public async override Task<LoginResponse> Login(LoginRequest request, ServerCallContext context)
        {
            var response = new LoginResponse();    

            var user = await _context.GetAsyncByLogin(request.Login);

            if (user == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, "Login not found"));
            }

            if (!BCrypt.Net.BCrypt.Verify(request.Password + user.Salt, user.Hash))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Wrong password"));
            }

            response.UserId = user.Id;

            return response;
        }
    }
}
using AutoMapper;

namespace Facade.MapperProfiles
{
    public class AuthMapperConfiguration : Profile
    {
        public AuthMapperConfiguration()
        {
            CreateMap<Authorization.LoginRequest, AuthorizationForClient.LoginRequest>();
            CreateMap<AuthorizationForClient.LoginRequest, Authorization.LoginRequest>();
            CreateMap<AuthorizationForClient.LoginResponse, Authorization.LoginResponse>();
            CreateMap<Authorization.LoginResponse, AuthorizationForClient.LoginResponse>();
        }
    }
}

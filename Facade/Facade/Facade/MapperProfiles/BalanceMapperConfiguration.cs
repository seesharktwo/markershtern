using AutoMapper;

namespace Facade.MapperProfiles
{
    public class BalanceMapperConfiguration : Profile
    {
        public BalanceMapperConfiguration()
        {
            CreateMap<Protos.CustomTypes.GetBalanceRequest, BalanceForClient.GetBalanceRequest>();
            CreateMap<BalanceForClient.GetBalanceRequest, Protos.CustomTypes.GetBalanceRequest>();
            CreateMap<BalanceForClient.GetBalanceResponse, Protos.CustomTypes.GetBalanceResponse>();
            CreateMap<Protos.CustomTypes.GetBalanceResponse, BalanceForClient.GetBalanceResponse>();
        }
    }
}

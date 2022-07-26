using AutoMapper;

namespace UserBagMicroservice.Data.Map
{
    public class UserBagProfile<T> : Profile 
        where T : class
    {
        public UserBagProfile()
        {
            CreateMap<Models.Product, Protos.Product>();

            CreateMap<Models.UserBag, Protos.UserBag>()
                .ForMember(dest => dest.Products, opt => opt.MapFrom(src => src.Products));

            CreateMap<Models.Wrapper, Protos.ToWrapper>()
                .ForMember(dest => dest.Products, opt => opt.MapFrom(src => src.Value));
        }
    }
}

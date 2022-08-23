using AutoMapper;
using MoneyTypes;

namespace UserBagMicroservice.Data.Map
{
    public class UserBagProfile : Profile 
    {
        public UserBagProfile()
        {

            CreateMap<Briefcase.Product, Protos.Product>();
            CreateMap<Protos.Product, Briefcase.Product>();

            CreateMap<Protos.ProductsList, Briefcase.ProductsList>()
                .ForMember(dest => dest.Products, opt => opt.MapFrom(src => src.Value));

            CreateMap<DecimalValue, decimal>().ConvertUsing(val => val);

            CreateMap<decimal, DecimalValue>().ConvertUsing(val => val);
        }
    }
}

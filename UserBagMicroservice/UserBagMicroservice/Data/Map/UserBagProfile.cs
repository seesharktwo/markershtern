using AutoMapper;
using Briefcase;
using MoneyTypes;

namespace UserBagMicroservice.Data.Map
{
    public class UserBagProfile : Profile 
    {
        public UserBagProfile()
        {
            CreateMap<Protos.Product, Product>();

            CreateMap<Protos.ProductsList, ProductsList>()
                .ForMember(dest => dest.Products, opt => opt.MapFrom(src => src.Value));

            CreateMap<DecimalValue, decimal>().ConvertUsing(val => val);

            CreateMap<decimal, DecimalValue>().ConvertUsing(val => val);
        }
    }
}

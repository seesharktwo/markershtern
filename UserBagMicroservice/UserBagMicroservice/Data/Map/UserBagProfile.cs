using AutoMapper;
using MoneyTypes;
using MongoDB.Bson;

namespace UserBagMicroservice.Data.Map
{
    public class UserBagProfile : Profile 
    {
        public UserBagProfile()
        {
            CreateMap<Protos.ProductsList, Briefcase.ProductsList>()
                .ForMember(dest => dest.Products, opt => opt.MapFrom(src => src.Value));

            CreateMap<Briefcase.Product, Protos.Product>()
               .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.ProductId));

            CreateMap<Protos.Product, Briefcase.Product>()
                .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => src.Id));

            CreateMap<DecimalValue, decimal>().ConvertUsing(val => val);

            CreateMap<decimal, DecimalValue>().ConvertUsing(val => val);
        }
    }
}

using AutoMapper;
using MoneyTypes;

namespace Facade.MapperProfiles
{
    public class BriefcaseMapperConfiguration : Profile
    {
        public BriefcaseMapperConfiguration()
        {
            CreateMap<Briefcase.GetUserProductsRequest, BriefcaseForClient.GetUserProductsRequest>();
            CreateMap<BriefcaseForClient.GetUserProductsRequest, Briefcase.GetUserProductsRequest>();
            CreateMap<BriefcaseForClient.GetUserProductsResponse, Briefcase.GetUserProductsResponse>();
            CreateMap<Briefcase.GetUserProductsResponse, BriefcaseForClient.GetUserProductsResponse>();

            CreateMap<Briefcase.AddProductRequest, BriefcaseForClient.AddProductRequest>();
            CreateMap<BriefcaseForClient.AddProductRequest, Briefcase.AddProductRequest>();
            CreateMap<BriefcaseForClient.AddProductResponse, Briefcase.AddProductResponse>();
            CreateMap<Briefcase.AddProductResponse, BriefcaseForClient.AddProductResponse>();

            CreateMap<Briefcase.RemoveProductRequest, BriefcaseForClient.RemoveProductRequest>();
            CreateMap<BriefcaseForClient.RemoveProductRequest, Briefcase.RemoveProductRequest>();
            CreateMap<BriefcaseForClient.RemoveProductResponse, Briefcase.RemoveProductResponse>();
            CreateMap<Briefcase.RemoveProductResponse, BriefcaseForClient.RemoveProductResponse>();

            CreateMap<Briefcase.Product, BriefcaseForClient.Product>();
            CreateMap<BriefcaseForClient.Product, Briefcase.Product>();
        }
    }
}

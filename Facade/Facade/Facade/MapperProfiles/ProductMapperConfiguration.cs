using AutoMapper;
using MoneyTypes;

namespace Facade.Mapper
{
    public class ProductMapperConfiguration : Profile
    {
        public ProductMapperConfiguration()
        {
            CreateMap<Product.GetProductsRequest, ProductForClient.GetProductsRequest>();
            CreateMap<ProductForClient.GetProductsRequest, Product.GetProductsRequest>();
            CreateMap<ProductForClient.GetProductsResponse, Product.GetProductsResponse>();
            CreateMap<Product.GetProductsResponse, ProductForClient.GetProductsResponse>();
        }

    }
}

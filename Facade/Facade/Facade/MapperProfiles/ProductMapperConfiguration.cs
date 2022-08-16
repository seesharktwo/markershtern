using AutoMapper;
using MoneyTypes;

namespace Facade.Mapper
{
    public class ProductMapperConfiguration : Profile
    {
        public ProductMapperConfiguration()
        {
            CreateMap<Product.GetProductsRequest, ProductClient.GetProductsRequest>();
            CreateMap<ProductClient.GetProductsRequest, Product.GetProductsRequest>();
            CreateMap<ProductClient.GetProductsResponse, Product.GetProductsResponse>();
            CreateMap<Product.GetProductsResponse, ProductClient.GetProductsResponse>();
            CreateMap<DecimalValue, decimal>().ConvertUsing(val => val);
            CreateMap<decimal, DecimalValue>().ConvertUsing(val => val);
        }

    }
}

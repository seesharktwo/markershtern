using AutoMapper;
using MoneyTypes;

namespace ProductService.Mapper
{
    public class ProductMapperConfiguration : Profile
    {
        public ProductMapperConfiguration()
        {
            CreateMap<DecimalValue, decimal>().ConvertUsing(val => val);
            CreateMap<decimal, DecimalValue>().ConvertUsing(val => val);
            CreateMap<ProductService.Models.Product, Product.GetProductsResponse.Types.Product>();
            CreateMap<Product.GetProductsResponse.Types.Product, ProductService.Models.Product>();
        }
    }
}

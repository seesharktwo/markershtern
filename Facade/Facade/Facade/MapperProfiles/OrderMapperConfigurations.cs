using AutoMapper;
using MoneyTypes;

namespace Facade.Mapper
{
    public class OrderMapperConfigurations : Profile
    {
        public OrderMapperConfigurations()
        {
            CreateMap<Order.CreateOrderRequest, OrderClient.CreateOrderRequest>();
            CreateMap<OrderClient.CreateOrderRequest, Order.CreateOrderRequest>();
            CreateMap<OrderClient.CreateOrderResponse, Order.CreateOrderResponse>();
            CreateMap<Order.CreateOrderResponse, OrderClient.CreateOrderResponse>();
            CreateMap<DecimalValue, decimal>().ConvertUsing(val => val);
            CreateMap<decimal, DecimalValue>().ConvertUsing(val => val);
        }
    }
}

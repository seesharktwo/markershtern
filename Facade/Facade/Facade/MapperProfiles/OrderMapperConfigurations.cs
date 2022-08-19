using AutoMapper;
using MoneyTypes;

namespace Facade.Mapper
{
    public class OrderMapperConfigurations : Profile
    {
        public OrderMapperConfigurations()
        {
            CreateMap<Order.CreateOrderRequest, OrderForClient.CreateOrderRequest>();
            CreateMap<OrderForClient.CreateOrderRequest, Order.CreateOrderRequest>();
            CreateMap<OrderForClient.CreateOrderResponse, Order.CreateOrderResponse>();
            CreateMap<Order.CreateOrderResponse, OrderForClient.CreateOrderResponse>();
        }
    }
}

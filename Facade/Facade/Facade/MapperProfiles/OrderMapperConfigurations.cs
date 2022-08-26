using AutoMapper;
using MoneyTypes;

namespace Facade.Mapper
{
    public class OrderMapperConfigurations : Profile
    {
        public OrderMapperConfigurations()
        {
            CreateMap<OrderProtos.CreateOrderRequest, OrderForClient.CreateOrderRequest>();
            CreateMap<OrderForClient.CreateOrderRequest, OrderProtos.CreateOrderRequest>();
            CreateMap<OrderForClient.CreateOrderResponse, OrderProtos.CreateOrderResponse>();
            CreateMap<OrderProtos.CreateOrderResponse, OrderForClient.CreateOrderResponse>();
            CreateMap<OrderProtos.DataCreateOrder, OrderForClient.DataCreateOrder>();
            CreateMap<OrderForClient.DataCreateOrder, OrderProtos.DataCreateOrder>();
            CreateMap<OrderForClient.SuccessResponse, OrderProtos.SuccessResponse>();
            CreateMap<OrderProtos.SuccessResponse, OrderForClient.SuccessResponse>();
        }
    }
}

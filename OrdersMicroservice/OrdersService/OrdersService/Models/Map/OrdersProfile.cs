using AutoMapper;
using OrdersService.Models.Enums;
using OrdersService.Models.Messages;
using MoneyTypes;

namespace OrdersService.Models.Map
{
    public class OrdersProfile : Profile
    {
        public OrdersProfile()
        {
            CreateMap<DataCreateOrder, OrderProtos.DataCreateOrder>();
            CreateMap<OrderProtos.DataCreateOrder, DataCreateOrder>();

            CreateMap<ProductPriceChanged, OrderProtos.ProductPriceChanged>();
            CreateMap<OrderProtos.ProductPriceChanged, ProductPriceChanged>();

            CreateMap<decimal, DecimalValue>().ConvertUsing(val => val);
            CreateMap<DecimalValue, decimal>().ConvertUsing(val => val);

            CreateMap<DecimalValue, MoneyTypes.DecimalValue>();
            CreateMap<MoneyTypes.DecimalValue, DecimalValue>();

            CreateMap<OrderType, OrderProtos.OrderType>();
            CreateMap<OrderProtos.OrderType, OrderType>();

            CreateMap<OrderCandidateOccuredProcessSuccess, OrderProtos.OrderCandidateOccuredProcessSuccess>();
            CreateMap<OrderProtos.OrderCandidateOccuredProcessSuccess, OrderCandidateOccuredProcessSuccess>();
        }
    }
}

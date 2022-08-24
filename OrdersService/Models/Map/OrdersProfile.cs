using AutoMapper;
using OrdersService.Models.Messages;

namespace OrdersService.Models.Map
{
    public class OrdersProfile : Profile
    {
        public OrdersProfile()
        {
            CreateMap<DataCreateOrder, Protos.DataCreateOrder>();
            CreateMap<ProductPriceChanged, Protos.ProductPriceChanged>();
        }
    }
}

using AutoMapper;
using MoneyTypes;

namespace Facade.MapperProfiles
{
    public class DecimalValueMapperConfiguration : Profile
    {
        public DecimalValueMapperConfiguration()
        {
            CreateMap<DecimalValue, decimal>().ConvertUsing(val => val);
            CreateMap<decimal, DecimalValue>().ConvertUsing(val => val);
        }
    }
}

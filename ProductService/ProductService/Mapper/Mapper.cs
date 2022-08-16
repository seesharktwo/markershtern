using AutoMapper;
using MoneyTypes;

namespace ProductService.Mapper
{
    public class Mapper : ProductService.Mapper.IMapper
    {
        public TOut Map<TIn, TOut>(TIn valueToConvert)
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<TIn, TOut>();
                // configs mapper for custom types
                cfg.CreateMap<DecimalValue, decimal>().ConvertUsing(val => val);
                cfg.CreateMap<decimal, DecimalValue>().ConvertUsing(val => val);
            });
            var mapper = new AutoMapper.Mapper(config);

            var result = mapper.Map<TOut>(valueToConvert);
            return result;


        }
    }
}

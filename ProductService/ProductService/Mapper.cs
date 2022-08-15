using AutoMapper;
using MoneyTypes;

namespace ProductService
{
    public static class Mapper
    {
        public static TOut Map<TIn, TOut>(TIn valueToConvert)
        {
            var config = new MapperConfiguration(cfg => {
                cfg.CreateMap<TIn, TOut>();
                // настройки маппинга кастомного типа
                cfg.CreateMap<DecimalValue, decimal>().ConvertUsing(val => val);
                cfg.CreateMap<decimal,DecimalValue>().ConvertUsing(val => val);
            });
            var mapper = new AutoMapper.Mapper(config);

            var result = mapper.Map<TOut>(valueToConvert);
            return result;
            

        }
    }
}

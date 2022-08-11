﻿using AutoMapper;

namespace Facade
{
    public class Mapper
    {
        public static TOut Map<TIn, TOut>(TIn valueToConvert)
        {
            var config = new MapperConfiguration(cfg => {
                cfg.CreateMap<TIn, TOut>();
                // настройки маппинга кастомного типа
                cfg.CreateMap<CustomTypes.DecimalValue, decimal>().ConvertUsing(val => val);
                cfg.CreateMap<decimal, CustomTypes.DecimalValue>().ConvertUsing(val => val);
            });
            var mapper = new AutoMapper.Mapper(config);

            var result = mapper.Map<TOut>(valueToConvert);
            return result;


        }
    }
}

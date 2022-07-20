using AutoMapper;

namespace ProductService
{
    public static class Mapper
    {
        public static TOut Map<TIn, TOut>(TIn valueToConvert)
        {
            var config = new MapperConfiguration(cfg => cfg.CreateMap<TIn, TOut>());
            var mapper = new AutoMapper.Mapper(config);

            var result = mapper.Map<TOut>(valueToConvert);
            return result;
            

        }
    }
}

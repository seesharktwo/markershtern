namespace Facade.Mapper
{
    public interface IMapper
    {

        public TOut Map<TIn, TOut>(TIn valueToConvert);
    }
}

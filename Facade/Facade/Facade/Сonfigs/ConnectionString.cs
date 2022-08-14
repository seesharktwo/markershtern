namespace Facade.Сonfigs
{
    /// <summary>
    /// connection string class by service
    /// </summary>
    /// <typeparam name="TService">Service type that will use this connection string</typeparam>
    public class ConnectionString<TService>
    {
        public string String { get; set; }
    }
}

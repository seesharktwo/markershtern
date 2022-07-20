namespace ProductService.Models
{
    public class ProductStoreDatabaseSettings
    {
        public string ConnectionString { get; set; }

        public string DatabaseName { get; set; }

        public string ProductsCollectionName { get; set; }
    }
}

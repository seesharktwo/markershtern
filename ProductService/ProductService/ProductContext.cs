using Microsoft.Extensions.Options;
using MongoDB.Driver;
using ProductService.Configs;
using ProductService.Models;

namespace ProductService
{
    /// <summary>
    /// Прослойка между БД и репозиторием
    /// </summary>
    public class ProductContext
    {
        private readonly IMongoCollection<Models.Product> _productsCollection;

        public ProductContext(
            IOptions<ProductStoreDatabaseSettings> productsStoreDatabaseSettings)
        {
            var mongoClient = new MongoClient(
                productsStoreDatabaseSettings.Value.ConnectionString);

            var mongoDatabase = mongoClient.GetDatabase(
                productsStoreDatabaseSettings.Value.DatabaseName);

            _productsCollection = mongoDatabase.GetCollection<Models.Product>(
                productsStoreDatabaseSettings.Value.ProductsCollectionName);
        }

        public async Task<List<Models.Product>> GetAsync() =>
            await _productsCollection.Find(_ => true).ToListAsync();

        public async Task<Models.Product?> GetAsync(string id) =>
            await _productsCollection.Find(x => x.Id == id).FirstOrDefaultAsync();

        public async Task<Models.Product?> GetByNameAsync(string name) =>
           await _productsCollection.Find(x => x.Name == name).FirstOrDefaultAsync();


        public async Task CreateAsync(Models.Product newProduct) =>
            await _productsCollection.InsertOneAsync(newProduct);

        public async Task UpdateAsync(string id, Models.Product updatedProduct) =>
            await _productsCollection.ReplaceOneAsync(x => x.Id == id, updatedProduct);

        public async Task RemoveAsync(string id) =>
            await _productsCollection.DeleteOneAsync(x => x.Id == id);
    }
}

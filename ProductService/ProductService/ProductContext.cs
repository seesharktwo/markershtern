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
        private readonly IMongoCollection<Models.Product> _productsCOllection;

        public ProductContext(
            IOptions<ProductStoreDatabaseSettings> bookStoreDatabaseSettings)
        {
            var mongoClient = new MongoClient(
                bookStoreDatabaseSettings.Value.ConnectionString);

            var mongoDatabase = mongoClient.GetDatabase(
                bookStoreDatabaseSettings.Value.DatabaseName);

            _productsCOllection = mongoDatabase.GetCollection<Models.Product>(
                bookStoreDatabaseSettings.Value.ProductsCollectionName);

            _productsCOllection.InsertOne(new Product { Ask = 40.04m, Bid = 35.03m, Name = "Euro" });
        }

        public async Task<List<Models.Product>> GetAsync() =>
            await _productsCOllection.Find(_ => true).ToListAsync();

        public async Task<Models.Product?> GetAsync(string id) =>
            await _productsCOllection.Find(x => x.Id == id).FirstOrDefaultAsync();

        public async Task<Models.Product?> GetByNameAsync(string name) =>
           await _productsCOllection.Find(x => x.Name == name).FirstOrDefaultAsync();


        public async Task CreateAsync(Models.Product newProduct) =>
            await _productsCOllection.InsertOneAsync(newProduct);

        public async Task UpdateAsync(string id, Models.Product updatedProduct) =>
            await _productsCOllection.ReplaceOneAsync(x => x.Id == id, updatedProduct);

        public async Task RemoveAsync(string id) =>
            await _productsCOllection.DeleteOneAsync(x => x.Id == id);
    }
}

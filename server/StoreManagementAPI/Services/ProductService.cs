using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using StoreManagementAPI.Configs;
using StoreManagementAPI.Models;
using System.Text.RegularExpressions;

namespace StoreManagementAPI.Services
{
    public class ProductService
    {
        private readonly IMongoCollection<Product> _products;

        public ProductService(IOptions<StoreManagementDBSettings> dbSettings)
        {
            var client = new MongoClient(dbSettings.Value.ConnectionString);
            var database = client.GetDatabase(dbSettings.Value.DatabaseName);

            _products = database.GetCollection<Product>(dbSettings.Value.ProductsCollectionName);
        }

        public async Task<List<Product>> GetAllProducts()
        {
            return await _products.Find(product => true).ToListAsync();
        }

        public async Task<List<Product>> FindProductByName(string name)
        {
            var filter = Builders<Product>.Filter.Regex("Name", new BsonRegularExpression($".*{name}.*", "i"));
            return await _products.Find(filter).ToListAsync();
        }

        public async Task<Product> GetById(string id) =>
            await _products.Find(product => product.Pid == id).FirstOrDefaultAsync();
            
        public async Task<List<Product>> GetByBarcode(string barcode)
        {
            var filter = Builders<Product>.Filter.Eq("Barcode", barcode);
            return await _products.Find(filter).ToListAsync();
        }

        public async Task<bool> CreateProduct(Product product)
        {
            product.Pid = ObjectId.GenerateNewId().ToString();
            product.CreatedAt = DateTime.UtcNow.AddHours(+7);
            product.UpdatedAt = DateTime.UtcNow.AddHours(+7);

            await _products.InsertOneAsync(product);
            return true;
        }

        public async Task<bool> UpdateProduct(string id, Product productIn)
        {
            var product = await GetById(id);

            if (product == null)
                return false;

            productIn.Pid = product.Pid;
            productIn.CreatedAt = product.CreatedAt;
            productIn.UpdatedAt = DateTime.UtcNow.AddHours(+7);

            await _products.ReplaceOneAsync(product => product.Pid == id, productIn);
            return true;
        }

        public async Task<bool> DeleteProduct(string id)
        {
            var product = await GetById(id);

            if (product == null)
                return false;

            await _products.DeleteOneAsync(product => product.Pid == id);
            return true;
        }

        public long GetTotalProduct()
        {
            var filter = Builders<Product>.Filter.Empty;
            var totalCount = _products.CountDocuments(filter);

            return totalCount;
        }
    }
}

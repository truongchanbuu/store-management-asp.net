using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using StoreManagementAPI.Configs;
using StoreManagementAPI.Models;

namespace StoreManagementAPI.Services
{
    public class OrderProductsService
    {
        private readonly IMongoCollection<OrderProduct> _orderProducts;

        public OrderProductsService(IOptions<StoreManagementDBSettings> dbSettings)
        {
            var client = new MongoClient(dbSettings.Value.ConnectionString);
            var database = client.GetDatabase(dbSettings.Value.DatabaseName);

            _orderProducts = database.GetCollection<OrderProduct>(dbSettings.Value.OrderProductsCollectionName);
        }

        public async Task<List<OrderProduct>> GetAllOrderProducts()
        {
            return await _orderProducts.Find(product => true).ToListAsync();
        }

        public async Task<List<OrderProduct>> GetByProductId(string id)
        {
            var filter = Builders<OrderProduct>.Filter.Regex("pid", new BsonRegularExpression(id));
            return await _orderProducts.Find(filter).ToListAsync();
        }

        public async Task<OrderProduct> GetByProductIdAndOrderId(string pid, string oid)
        {
            var filter = Builders<OrderProduct>.Filter.And(
                Builders<OrderProduct>.Filter.Regex("pid", new BsonRegularExpression(pid)),
                Builders<OrderProduct>.Filter.Regex("oid", new BsonRegularExpression(oid))
            );

            return await _orderProducts.Find(filter).FirstOrDefaultAsync();
        }

        public OrderProduct? CreateOrderProduct(OrderProduct orderProduct)
        {
            try
            {
                _orderProducts.InsertOne(orderProduct);
                return orderProduct;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        public OrderProduct? UpdateOrderProduct(OrderProduct existingOrderProduct)
        {
            try
            {
                var filter = Builders<OrderProduct>.Filter.Eq(op => op.Id, existingOrderProduct.Id);
                var update = Builders<OrderProduct>.Update
                    .Set(op => op.Quantity, existingOrderProduct.Quantity);

                var result = _orderProducts.UpdateOne(filter, update);

                if (result.ModifiedCount > 0)
                {
                    return existingOrderProduct;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to update order product: {ex.Message}");
                return null;
            }
        }
    }
}

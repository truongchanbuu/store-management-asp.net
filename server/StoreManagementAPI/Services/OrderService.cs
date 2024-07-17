using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using StoreManagementAPI.Configs;
using StoreManagementAPI.Models;

namespace StoreManagementAPI.Services
{
    public class OrderService
    {
        private readonly IMongoCollection<Order> _orders;

        public OrderService(IOptions<StoreManagementDBSettings> dbSettings)
        {
            var client = new MongoClient(dbSettings.Value.ConnectionString);
            var database = client.GetDatabase(dbSettings.Value.DatabaseName);

            _orders = database.GetCollection<Order>(dbSettings.Value.OrdersCollectionName);
        }

        public async Task<List<Order>> GetAllOrders()
        {
            return await _orders.Find(order => true).ToListAsync();
        }

        public async Task<List<Order>> GetByCustomerId(string id)
        {
            return await _orders.Find(order => order.Customer.CustId == id).ToListAsync();
        }

        public Order? CreatePendingOrder(User user)
        {
            Order createdOrder = new Order();
            createdOrder.User = user;
            createdOrder.OrderStatus = Status.PENDING;
            createdOrder.CreatedAt = DateTime.UtcNow.AddHours(7);
            try
            {
                _orders.InsertOne(createdOrder);
                return createdOrder;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        public Order? GetOrderByOID(string oid)
        {
            if (string.IsNullOrEmpty(oid) || oid.Length != 24 || !ObjectId.TryParse(oid, out _))
            {
                return null;
            }

            FilterDefinition<Order> filter = Builders<Order>.Filter.Eq(order => order.Oid, oid);
            return _orders.Find(filter).FirstOrDefault();
        }

        public bool UpdateOrder(Order existingOrder)
        {
            try
            {
                var filter = Builders<Order>.Filter.Eq(o => o.Oid, existingOrder.Oid);
                var updateDefinition = Builders<Order>.Update
                    .Set(o => o.OrderProducts, existingOrder.OrderProducts)
                    .Set(o => o.Customer, existingOrder.Customer)
                    .Set(o => o.TotalPrice, Math.Round(CalculateTotalPrice(existingOrder.OrderProducts), 2))
                    .Set(o => o.UpdatedAt, DateTime.UtcNow.AddHours(7));

                var result = _orders.UpdateOne(filter, updateDefinition);

                return result.ModifiedCount > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        public Order? UpdateOrderStatus(string orderId, Status newStatus)
        {
            Order existingOrder = _orders.Find(order => order.Oid == orderId).FirstOrDefault();

            if (existingOrder != null)
            {
                DateTime.UtcNow.AddHours(7);
                existingOrder.OrderStatus = newStatus;

                try
                {
                    _orders.ReplaceOne(order => order.Oid == orderId, existingOrder);
                    return existingOrder;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    return null;
                }
            }

            return null;
        }

        public List<Order> getOrderByStatus(Status? status)
        {
            if (status == null)
            {
                return _orders.Find(order => order.OrderStatus.Equals(status)).ToList();
            }
            else
            {
                return _orders.Find(_ => true).ToList();
            }
        }

        public List<Order> GetOrdersByTimeAndStatus(DateTime startDate, DateTime endDate, Status? status)
        {
            var filterBuilder = Builders<Order>.Filter;
            var dateFilter = filterBuilder.Gte("CreatedAt", startDate) & filterBuilder.Lte("CreatedAt", endDate);

            FilterDefinition<Order> finalFilter;
            if (status.HasValue)
            {
                var statusFilter = filterBuilder.Eq("OrderStatus", status.Value);
                finalFilter = filterBuilder.And(dateFilter, statusFilter);
            }
            else
            {
                finalFilter = dateFilter;
            }

            return _orders.Find(finalFilter).ToList();
        }

        private double CalculateTotalPrice(List<OrderProduct> orderProducts)
        {
            return orderProducts == null ? 0 :
                orderProducts
                    .Select(orderProduct => orderProduct.RetailPrice * orderProduct.Quantity)
                    .Where(price => !double.IsNaN(price) && !double.IsInfinity(price))
                    .Sum();
        }

        public long GetTotalOrder()
        {
            var filter = Builders<Order>.Filter.Empty;
            var totalCount = _orders.CountDocuments(filter);

            return totalCount;
        }
    }
    

}

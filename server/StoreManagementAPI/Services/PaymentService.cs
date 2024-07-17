using Microsoft.Extensions.Options;
using MongoDB.Driver;
using StoreManagementAPI.Configs;
using StoreManagementAPI.Models;

namespace StoreManagementAPI.Services
{
    public class PaymentService
    {
        private readonly IMongoCollection<Payment> _payments;

        public PaymentService(IOptions<StoreManagementDBSettings> dbSettings)
        {
            var client = new MongoClient(dbSettings.Value.ConnectionString);
            var database = client.GetDatabase(dbSettings.Value.DatabaseName);
            _payments = database.GetCollection<Payment>(dbSettings.Value.PaymentCollectionName);
        }

        public bool CreatePayment(Order existingOrder, string paymentMethod)
        {
            Payment payment = new Payment();
            payment.Status = Status.PENDING;
            payment.Oid = existingOrder.Oid;
            payment.PaymentTime = DateTime.UtcNow;
            payment.Amount = existingOrder.TotalPrice;
            payment.Uid = existingOrder?.User?.Id;
            payment.PaymentMethod = paymentMethod;

            try
            {
                _payments.InsertOne(payment);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        public List<Payment> GetPaymentByBetweenDate(DateTime startDate, DateTime endDate)
        {
            try
            {
                var filterBuilder = Builders<Payment>.Filter;
                var dateFilter = filterBuilder.Gte("PaymentTime", startDate) & filterBuilder.Lte("PaymentTime", endDate);

                return _payments.Find(dateFilter).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new List<Payment>();
            }
        }

    }
}

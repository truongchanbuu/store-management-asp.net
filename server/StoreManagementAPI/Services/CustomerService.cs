using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using StoreManagementAPI.Configs;
using StoreManagementAPI.Models;
using System.Net;

namespace StoreManagementAPI.Services
{
    public class CustomerService
    {
        private readonly IMongoCollection<Customer> _customers;

        public CustomerService(IOptions<StoreManagementDBSettings> dbSettings)
        {
            var client = new MongoClient(dbSettings.Value.ConnectionString);
            var database = client.GetDatabase(dbSettings.Value.DatabaseName);

            _customers = database.GetCollection<Customer>(dbSettings.Value.CustomersCollectionName);
        }

        public async Task<List<Customer>> GetAllCustomers()
        {
            return await _customers.Find(customer => true).ToListAsync();
        }

        public async Task<Customer> GetCustomerById(string id)
        {
            return await _customers.Find(customer => customer.CustId == id).FirstOrDefaultAsync();
        }

        public async Task<List<Customer>> GetCustomerByPhone(string phone)
        {
            return await _customers.Find(customer => customer.Phone == phone).ToListAsync();
        }

        public async Task<List<Customer>> GetCustomerByName(string name)
        {
            return await _customers.Find(customer => customer.Name == name).ToListAsync();
        }

        public async Task<List<Customer>> SearchByName(string name)
        {
            var filter = Builders<Customer>.Filter.Regex("Name", new BsonRegularExpression($".*{name}.*", "i"));
            return await _customers.Find(filter).ToListAsync();
        }

        public async Task<bool> CreateCustomer(Customer customer)
        {
            try { 
                await _customers.InsertOneAsync(customer);
                return true;
            }
            catch { return false; }
        }

        public async Task<bool> UpdateCustomer(string id, Customer customerIn)
        {
            try { 
                if (await GetCustomerById(id) == null)
                    return false;

                await _customers.ReplaceOneAsync(customer => customer.CustId == id, customerIn);
                return true;
            }
            catch { return false; }
        }

        public async Task<bool> DeleteCustomer(string id)
        {
            try
            {
                if (await GetCustomerById(id) == null)
                    return false;

                await _customers.DeleteOneAsync(customer => customer.CustId == id);
                return true;
            }
            catch { return false; }
        }

        public long GetTotalCustomer()
        {
            var filter = Builders<Customer>.Filter.Empty;
            var totalCount = _customers.CountDocuments(filter);

            return totalCount;
        }
    }
}

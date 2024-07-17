namespace StoreManagementAPI.Models
{
    public class AnalyticsReport
    {
        public Double TotalAmountReceived { get; set; }
        public int NumberOfOrders { get; set; }
        public int NumberOfProducts { get; set; }
        public List<Order> Orders { get; set; }
    }
}

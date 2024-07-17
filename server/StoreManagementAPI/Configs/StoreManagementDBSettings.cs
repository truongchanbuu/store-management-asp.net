namespace StoreManagementAPI.Configs
{
    public class StoreManagementDBSettings
    {
        public string ConnectionString { get; set; } = "";

        public string DatabaseName { get; set; } = "";

        public string UsersCollectionName { get; set; } = "";

        public string PRTCollectionName { get; set; } = "";
        public string ProductsCollectionName { get; set; } = "";
        public string OrdersCollectionName { get; set; } = "";
        public string CustomersCollectionName { get; set; } = "";
        public string OrderProductsCollectionName { get; set; } = "";
        public string PaymentCollectionName { get; set; } = "";
    }
}

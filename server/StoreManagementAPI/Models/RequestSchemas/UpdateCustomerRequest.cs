namespace StoreManagementAPI.Models.RequestSchemas
{
    public class UpdateCustomerRequest
    {
        public string Name { get; set; } = "";
        public string Phone { get; set; } = "";
        public string Email { get; set; } = "";
        public double Point { get; set; } = 0;
    }
}

namespace StoreManagementAPI.Models.RequestSchemas
{
    public class ResetPasswordRequest
    {
        public string NewPassword { get; set; } = "";
        public string ConfirmPassword { get; set; } = "";
    }
}

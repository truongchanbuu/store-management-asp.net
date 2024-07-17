namespace StoreManagementAPI.Configs
{
    public class MailSettings
    {
        public string Host { get; set; } = "";
        public int Port { get; set; } = 0;
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
        public bool EnableSSL { get; set; } = false;
    }
}

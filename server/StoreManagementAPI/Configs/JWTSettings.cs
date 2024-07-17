namespace StoreManagementAPI.Configs
{
    public class JWTSettings
    {
        public string Secret { get; set; } = "";
        public int ExpireInSecond { get; set; } = 0;
    }
}

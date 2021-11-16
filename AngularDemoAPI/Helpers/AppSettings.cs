namespace WebApi.Helpers
{
    public class AppSettings
    {
        public string Secret { get; set; }
        public string SMTPUserName { get; set; }
        public string SMTPPassword { get; set; }
        public string StripeApiKey { get; set; }
    }
}
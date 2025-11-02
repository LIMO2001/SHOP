namespace LaptopStore.Models
{
    public class MpesaConfig
    {
        public string ConsumerKey { get; set; }
        public string ConsumerSecret { get; set; }
        public string Passkey { get; set; }
        public string BusinessShortCode { get; set; }
        public string CallbackUrl { get; set; }
        public string Environment { get; set; } // "sandbox" or "production"
    }
}
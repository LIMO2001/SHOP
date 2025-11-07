namespace LaptopStore.Models
{
    public class CheckoutViewModel
    {
        public string ShippingAddress { get; set; } = string.Empty;
        public string PaymentMethod { get; set; } = string.Empty;
        
        public decimal CartTotal { get; set; }
        public List<CartItem> CartItems { get; set; } = new List<CartItem>();
    }
}
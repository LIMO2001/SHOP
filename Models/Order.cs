namespace LaptopStore.Models
{
    public class Order
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; } = Guid.NewGuid().ToString();
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = "Pending";
        public string ShippingAddress { get; set; } = string.Empty;
        public string PaymentMethod { get; set; } = string.Empty;

        // Foreign key
        public int UserId { get; set; }
        public User User { get; set; } = null!;

        // Navigation property
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}
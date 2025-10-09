namespace LaptopStore.Models
{
    public class CartItem
    {
        public int Id { get; set; }
        public int Quantity { get; set; }
        public DateTime AddedAt { get; set; } = DateTime.UtcNow;

        // Foreign keys
        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;
    }
}
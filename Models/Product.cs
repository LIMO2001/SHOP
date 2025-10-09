namespace LaptopStore.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string ImageUrl { get; set; } = "/images/default-laptop.jpg";
        public int StockQuantity { get; set; }
        public string Processor { get; set; } = string.Empty;
        public string RAM { get; set; } = string.Empty;
        public string Storage { get; set; } = string.Empty;
        public string Display { get; set; } = string.Empty;
        public string Graphics { get; set; } = string.Empty;
        public bool IsFeatured { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Foreign key
        public int CategoryId { get; set; }
        public Category Category { get; set; } = null!;
    }
}
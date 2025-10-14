using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;

namespace LaptopStore.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Product name is required")]
        [StringLength(100, ErrorMessage = "Product name cannot exceed 100 characters")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Price is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        public string ImageUrl { get; set; } = "/images/default-laptop.jpg";

        [Required(ErrorMessage = "Stock quantity is required")]
        [Range(0, int.MaxValue, ErrorMessage = "Stock quantity cannot be negative")]
        public int StockQuantity { get; set; }

        [StringLength(50, ErrorMessage = "Processor cannot exceed 50 characters")]
        public string Processor { get; set; } = string.Empty;

        [StringLength(20, ErrorMessage = "RAM cannot exceed 20 characters")]
        public string RAM { get; set; } = string.Empty;

        [StringLength(50, ErrorMessage = "Storage cannot exceed 50 characters")]
        public string Storage { get; set; } = string.Empty;

        [StringLength(50, ErrorMessage = "Display cannot exceed 50 characters")]
        public string Display { get; set; } = string.Empty;

        [StringLength(50, ErrorMessage = "Graphics cannot exceed 50 characters")]
        public string Graphics { get; set; } = string.Empty;

        public bool IsFeatured { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Foreign key
        [Required(ErrorMessage = "Category is required")]
        public int CategoryId { get; set; }
        
        public Category Category { get; set; } = null!;

        // Add this property for file uploads (not mapped to database)
        [NotMapped]
        public IFormFile? ImageFile { get; set; }
    }
}
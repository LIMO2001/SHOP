using System.ComponentModel.DataAnnotations;
using LaptopStore.Models;

namespace LaptopStore.Areas.Admin.Models
{
    public class ProductViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Product name is required")]
        [StringLength(100, ErrorMessage = "Product name cannot exceed 100 characters")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Price is required")]
        [Range(0.01, 10000.00, ErrorMessage = "Price must be between $0.01 and $10,000")]
        public decimal Price { get; set; }

        [Url(ErrorMessage = "Please enter a valid URL")]
        public string ImageUrl { get; set; } = "/images/default-laptop.jpg";

        [Required(ErrorMessage = "Stock quantity is required")]
        [Range(0, 1000, ErrorMessage = "Stock quantity must be between 0 and 1000")]
        public int StockQuantity { get; set; }

        [Required(ErrorMessage = "Processor information is required")]
        [StringLength(100, ErrorMessage = "Processor cannot exceed 100 characters")]
        public string Processor { get; set; } = string.Empty;

        [Required(ErrorMessage = "RAM information is required")]
        [StringLength(50, ErrorMessage = "RAM cannot exceed 50 characters")]
        public string RAM { get; set; } = string.Empty;

        [Required(ErrorMessage = "Storage information is required")]
        [StringLength(50, ErrorMessage = "Storage cannot exceed 50 characters")]
        public string Storage { get; set; } = string.Empty;

        [Required(ErrorMessage = "Display information is required")]
        [StringLength(100, ErrorMessage = "Display cannot exceed 100 characters")]
        public string Display { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "Graphics cannot exceed 100 characters")]
        public string Graphics { get; set; } = string.Empty;

        public bool IsFeatured { get; set; }

        [Required(ErrorMessage = "Category is required")]
        [Display(Name = "Category")]
        public int CategoryId { get; set; }

        // For dropdown list
        public List<Category>? Categories { get; set; }

        // Display only properties
        public string? CategoryName { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class ProductListViewModel
    {
        public List<Product> Products { get; set; } = new List<Product>();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public string SearchTerm { get; set; } = string.Empty;
        public int? CategoryId { get; set; }
    }
}
using System.ComponentModel.DataAnnotations;
using LaptopStore.Models;

namespace LaptopStore.Areas.Admin.Models
{
    public class CategoryViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Category name is required")]
        [StringLength(50, ErrorMessage = "Category name cannot exceed 50 characters")]
        public string Name { get; set; } = string.Empty;

        [StringLength(200, ErrorMessage = "Description cannot exceed 200 characters")]
        public string Description { get; set; } = string.Empty;

        [Url(ErrorMessage = "Please enter a valid URL")]
        public string ImageUrl { get; set; } = "/images/default-category.jpg";

        // Display only properties
        public int ProductCount { get; set; }
        public DateTime? LastProductAdded { get; set; }
    }

    public class CategoryListViewModel
    {
        public List<Category> Categories { get; set; } = new List<Category>();
        public int TotalCount { get; set; }
    }
}
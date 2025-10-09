using LaptopStore.Models;

namespace LaptopStore.Areas.Admin.Models
{
    public class AdminSearchViewModel
    {
        public string SearchTerm { get; set; } = string.Empty;
        public string SearchType { get; set; } = "products"; // products, categories, users, orders
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public int? CategoryId { get; set; }
        public string Role { get; set; } = string.Empty;
    }

    public class AdminSearchResultsViewModel
    {
        public List<Product> Products { get; set; } = new List<Product>();
        public List<Category> Categories { get; set; } = new List<Category>();
        public List<User> Users { get; set; } = new List<User>();
        public List<Order> Orders { get; set; } = new List<Order>();
        public string SearchType { get; set; } = string.Empty;
        public int TotalResults { get; set; }
    }
}
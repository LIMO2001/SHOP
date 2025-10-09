namespace LaptopStore.Areas.Admin.Models
{
    public class AdminDashboardViewModel
    {
        public int TotalProducts { get; set; }
        public int TotalCategories { get; set; }
        public int TotalUsers { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public List<RecentOrder> RecentOrders { get; set; } = new List<RecentOrder>();
        public List<PopularProduct> PopularProducts { get; set; } = new List<PopularProduct>();
    }

    public class RecentOrder
    {
        public int OrderId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime OrderDate { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class PopularProduct
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int SalesCount { get; set; }
        public decimal Revenue { get; set; }
    }
}
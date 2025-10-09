using System.ComponentModel.DataAnnotations;

namespace LaptopStore.Areas.Admin.Models
{
    public class OrderManagementViewModel
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        
        [Required]
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        
        public DateTime OrderDate { get; set; }
        
        [Required]
        [Range(0.01, 100000.00)]
        public decimal TotalAmount { get; set; }
        
        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Pending";
        
        [Required]
        [StringLength(500)]
        public string ShippingAddress { get; set; } = string.Empty;
        
        [Required]
        [StringLength(50)]
        public string PaymentMethod { get; set; } = string.Empty;
        
        public List<OrderItemViewModel> OrderItems { get; set; } = new List<OrderItemViewModel>();
    }

    public class OrderItemViewModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ProductImage { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice => Quantity * UnitPrice;
    }

    public class OrderListViewModel
    {
        public List<OrderManagementViewModel> Orders { get; set; } = new List<OrderManagementViewModel>();
        public int TotalCount { get; set; }
        public decimal TotalRevenue { get; set; }
        public int PendingOrders { get; set; }
        public int CompletedOrders { get; set; }
        public int CancelledOrders { get; set; }
    }

    public class OrderStatsViewModel
    {
        public int TodayOrders { get; set; }
        public int ThisWeekOrders { get; set; }
        public int ThisMonthOrders { get; set; }
        public decimal TodayRevenue { get; set; }
        public decimal ThisWeekRevenue { get; set; }
        public decimal ThisMonthRevenue { get; set; }
        public decimal AverageOrderValue { get; set; }
    }
}
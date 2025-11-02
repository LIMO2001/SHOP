using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using LaptopStore.Data;
using LaptopStore.Models;

namespace LaptopStore.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                // Get real counts from database
                var totalProducts = await _context.Products.CountAsync();
                var totalCategories = await _context.Categories.CountAsync();
                var totalUsers = await _context.Users.CountAsync();
                var totalOrders = await _context.Orders.CountAsync();

                Console.WriteLine($"Products: {totalProducts}, Categories: {totalCategories}, Users: {totalUsers}, Orders: {totalOrders}");

                // Calculate TOTAL REVENUE from completed and paid orders
                var totalRevenue = await _context.Orders
                    .Where(o => o.Status == "Completed" && o.PaymentStatus == "Paid")
                    .SumAsync(o => (decimal?)o.TotalAmount) ?? 0m;

                // Get recent orders for display - Use only properties that exist in User entity
                var recentOrders = await _context.Orders
                    .Include(o => o.User)
                    .OrderByDescending(o => o.OrderDate)
                    .Take(10)
                    .Select(o => new
                    {
                        o.Id,
                        o.OrderNumber,
                        o.OrderDate,
                        o.TotalAmount,
                        o.Status,
                        o.PaymentStatus,
                        // Use only properties that exist in your actual User entity
                        CustomerEmail = o.User != null ? o.User.Email : "Unknown",
                        CustomerId = o.UserId
                    })
                    .ToListAsync();

                // Get low stock products (assuming you have StockQuantity property)
                var lowStockProducts = new List<object>(); // Initialize empty for now
                // If you have StockQuantity in Product model, uncomment below:
                /*
                var lowStockProducts = await _context.Products
                    .Where(p => p.StockQuantity <= 5)
                    .Take(10)
                    .Select(p => new
                    {
                        p.Id,
                        p.Name,
                        p.StockQuantity,
                        p.Price
                    })
                    .ToListAsync();
                */

                // Get blog and career statistics
                var totalBlogPosts = await _context.BlogPosts.CountAsync();
                var publishedBlogPosts = await _context.BlogPosts.CountAsync(b => b.IsPublished);
                var totalCareers = await _context.Careers.CountAsync();
                var activeCareers = await _context.Careers.CountAsync(c => c.IsActive);

                // Get recent blog posts
                var recentBlogPosts = await _context.BlogPosts
                    .OrderByDescending(b => b.DatePosted)
                    .Take(5)
                    .ToListAsync();

                // Get recent careers
                var recentCareers = await _context.Careers
                    .OrderByDescending(c => c.ApplicationDeadline)
                    .Take(5)
                    .ToListAsync();

                // Pass data to view
                ViewBag.TotalProducts = totalProducts;
                ViewBag.TotalCategories = totalCategories;
                ViewBag.TotalUsers = totalUsers;
                ViewBag.TotalOrders = totalOrders;
                ViewBag.TotalRevenue = totalRevenue;
                ViewBag.RecentOrders = recentOrders;
                ViewBag.LowStockProducts = lowStockProducts;
                ViewBag.TotalBlogPosts = totalBlogPosts;
                ViewBag.PublishedBlogPosts = publishedBlogPosts;
                ViewBag.TotalCareers = totalCareers;
                ViewBag.ActiveCareers = activeCareers;
                ViewBag.RecentBlogPosts = recentBlogPosts;
                ViewBag.RecentCareers = recentCareers;

                return View();
            }
            catch (Exception ex)
            {
                // Log the detailed error
                Console.WriteLine($"Error in Dashboard: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                
                // Set default values
                ViewBag.TotalProducts = 0;
                ViewBag.TotalCategories = 0;
                ViewBag.TotalUsers = 0;
                ViewBag.TotalOrders = 0;
                ViewBag.TotalRevenue = 0;
                ViewBag.RecentOrders = new List<object>();
                ViewBag.LowStockProducts = new List<object>();
                ViewBag.TotalBlogPosts = 0;
                ViewBag.PublishedBlogPosts = 0;
                ViewBag.TotalCareers = 0;
                ViewBag.ActiveCareers = 0;
                ViewBag.RecentBlogPosts = new List<object>();
                ViewBag.RecentCareers = new List<object>();

                return View();
            }
        }

        // Additional method for revenue analytics
        public async Task<JsonResult> GetRevenueStats()
        {
            try
            {
                var today = DateTime.UtcNow.Date;
                var startOfMonth = new DateTime(today.Year, today.Month, 1);
                var startOfYear = new DateTime(today.Year, 1, 1);

                var stats = new
                {
                    TodayRevenue = await _context.Orders
                        .Where(o => o.OrderDate.Date == today && 
                                   o.Status == "Completed" && 
                                   o.PaymentStatus == "Paid")
                        .SumAsync(o => (decimal?)o.TotalAmount) ?? 0m,

                    MonthlyRevenue = await _context.Orders
                        .Where(o => o.OrderDate >= startOfMonth && 
                                   o.Status == "Completed" && 
                                   o.PaymentStatus == "Paid")
                        .SumAsync(o => (decimal?)o.TotalAmount) ?? 0m,

                    YearlyRevenue = await _context.Orders
                        .Where(o => o.OrderDate >= startOfYear && 
                                   o.Status == "Completed" && 
                                   o.PaymentStatus == "Paid")
                        .SumAsync(o => (decimal?)o.TotalAmount) ?? 0m,

                    TotalRevenue = await _context.Orders
                        .Where(o => o.Status == "Completed" && o.PaymentStatus == "Paid")
                        .SumAsync(o => (decimal?)o.TotalAmount) ?? 0m,

                    PendingOrders = await _context.Orders
                        .CountAsync(o => o.Status == "Pending Payment"),

                    CompletedOrders = await _context.Orders
                        .CountAsync(o => o.Status == "Completed" && o.PaymentStatus == "Paid")
                };

                return Json(new { success = true, data = stats });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        // Method to get recent transactions - Fixed for actual User entity
        public async Task<JsonResult> GetRecentTransactions()
        {
            try
            {
                var recentTransactions = await _context.Orders
                    .Include(o => o.User)
                    .Where(o => o.Status == "Completed" && o.PaymentStatus == "Paid")
                    .OrderByDescending(o => o.PaymentDate)
                    .Take(10)
                    .Select(o => new
                    {
                        OrderId = o.Id,
                        OrderNumber = o.OrderNumber,
                        // Use only properties that exist in User entity
                        CustomerEmail = o.User != null ? o.User.Email : "Unknown",
                        CustomerId = o.UserId,
                        Amount = o.TotalAmount,
                        PaymentDate = o.PaymentDate,
                        PaymentMethod = o.PaymentMethod,
                        MpesaReceipt = o.MpesaReceiptNumber
                    })
                    .ToListAsync();

                return Json(new { success = true, data = recentTransactions });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }
    }
}
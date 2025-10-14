using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using LaptopStore.Data;

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

                // Pass data to view
                ViewBag.TotalProducts = totalProducts;
                ViewBag.TotalCategories = totalCategories;
                ViewBag.TotalUsers = totalUsers;
                ViewBag.TotalOrders = totalOrders;
                ViewBag.TotalRevenue = 0; // You can calculate this later
                ViewBag.RecentOrders = new List<object>();
                ViewBag.LowStockProducts = new List<object>();

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

                return View();
            }
        }
    }
}
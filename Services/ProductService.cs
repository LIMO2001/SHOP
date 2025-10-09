using LaptopStore.Data;
using LaptopStore.Models;
using Microsoft.EntityFrameworkCore;

namespace LaptopStore.Services
{
    public class ProductService
    {
        private readonly ApplicationDbContext _context;

        public ProductService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Product>> GetFeaturedProductsAsync()
        {
            return await _context.Products
                .Include(p => p.Category)
                .Where(p => p.IsFeatured && p.StockQuantity > 0)
                .Take(8)
                .ToListAsync();
        }

        public async Task<List<Product>> GetProductsByCategoryAsync(int categoryId)
        {
            return await _context.Products
                .Include(p => p.Category)
                .Where(p => p.CategoryId == categoryId && p.StockQuantity > 0)
                .ToListAsync();
        }

        public async Task<List<Category>> GetAllCategoriesAsync()
        {
            return await _context.Categories
                .Include(c => c.Products)
                .ToListAsync();
        }

        public async Task<Product?> GetProductByIdAsync(int id)
        {
            return await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<List<Product>> SearchProductsAsync(string searchTerm)
        {
            return await _context.Products
                .Include(p => p.Category)
                .Where(p => p.Name.Contains(searchTerm) || 
                           p.Description.Contains(searchTerm) ||
                           p.Processor.Contains(searchTerm))
                .ToListAsync();
        }
    }
}
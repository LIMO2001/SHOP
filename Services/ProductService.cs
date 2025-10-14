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
                .OrderByDescending(p => p.CreatedAt)
                .Take(8)
                .ToListAsync();
        }

        public async Task<List<Product>> GetProductsByCategoryAsync(int categoryId)
        {
            return await _context.Products
                .Include(p => p.Category)
                .Where(p => p.CategoryId == categoryId && p.StockQuantity > 0)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Category>> GetAllCategoriesAsync()
        {
            return await _context.Categories
                .Include(c => c.Products)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<List<Category>> GetActiveCategoriesAsync()
        {
            return await _context.Categories
                .Where(c => c.Products.Any(p => p.StockQuantity > 0))
                .OrderBy(c => c.Name)
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
            if (string.IsNullOrWhiteSpace(searchTerm))
                return new List<Product>();

            return await _context.Products
                .Include(p => p.Category)
                .Where(p => p.StockQuantity > 0 && 
                           (p.Name.Contains(searchTerm) || 
                            p.Description.Contains(searchTerm) ||
                            p.Processor.Contains(searchTerm) ||
                            p.Category.Name.Contains(searchTerm)))
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Product>> GetAllProductsAsync()
        {
            return await _context.Products
                .Include(p => p.Category)
                .Where(p => p.StockQuantity > 0)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Product>> GetNewArrivalsAsync(int count = 6)
        {
            return await _context.Products
                .Include(p => p.Category)
                .Where(p => p.StockQuantity > 0)
                .OrderByDescending(p => p.CreatedAt)
                .Take(count)
                .ToListAsync();
        }

        public async Task<List<Product>> GetRelatedProductsAsync(int productId, int categoryId, int count = 4)
        {
            return await _context.Products
                .Include(p => p.Category)
                .Where(p => p.Id != productId && 
                           p.CategoryId == categoryId && 
                           p.StockQuantity > 0)
                .OrderByDescending(p => p.CreatedAt)
                .Take(count)
                .ToListAsync();
        }

        public async Task<int> GetProductsCountAsync()
        {
            return await _context.Products
                .Where(p => p.StockQuantity > 0)
                .CountAsync();
        }

        public async Task<int> GetProductsCountByCategoryAsync(int categoryId)
        {
            return await _context.Products
                .Where(p => p.CategoryId == categoryId && p.StockQuantity > 0)
                .CountAsync();
        }

        public async Task<Category?> GetCategoryByIdAsync(int id)
        {
            return await _context.Categories
                .Include(c => c.Products)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<List<Product>> GetProductsWithLowStockAsync(int threshold = 5)
        {
            return await _context.Products
                .Include(p => p.Category)
                .Where(p => p.StockQuantity <= threshold && p.StockQuantity > 0)
                .OrderBy(p => p.StockQuantity)
                .ToListAsync();
        }
    }
}
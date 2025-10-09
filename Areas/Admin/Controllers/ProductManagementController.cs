using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using LaptopStore.Data;
using LaptopStore.Models;
using LaptopStore.Areas.Admin.Models;

namespace LaptopStore.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ProductManagementController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductManagementController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var products = await _context.Products
                .Include(p => p.Category)
                .ToListAsync();

            return View(products); // Pass List<Product> instead of ProductViewModel
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            // Use ViewBag for categories instead of putting them in the model
            ViewBag.Categories = await _context.Categories.ToListAsync();
            return View(); // Return empty view for create
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product) // Accept Product directly
        {
            if (ModelState.IsValid)
            {
                product.CreatedAt = DateTime.UtcNow;
                _context.Products.Add(product);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Product created successfully!";
                return RedirectToAction("Index");
            }

            // If we got this far, something failed; redisplay form
            ViewBag.Categories = await _context.Categories.ToListAsync();
            return View(product);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id);
                
            if (product == null)
            {
                return NotFound();
            }

            ViewBag.Categories = await _context.Categories.ToListAsync();
            return View(product); // Pass Product directly
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Product product) // Accept Product directly
        {
            if (ModelState.IsValid)
            {
                var existingProduct = await _context.Products.FindAsync(product.Id);
                if (existingProduct == null)
                {
                    return NotFound();
                }

                // Update properties
                existingProduct.Name = product.Name;
                existingProduct.Description = product.Description;
                existingProduct.Price = product.Price;
                existingProduct.ImageUrl = product.ImageUrl;
                existingProduct.StockQuantity = product.StockQuantity;
                existingProduct.Processor = product.Processor;
                existingProduct.RAM = product.RAM;
                existingProduct.Storage = product.Storage;
                existingProduct.Display = product.Display;
                existingProduct.Graphics = product.Graphics;
                existingProduct.IsFeatured = product.IsFeatured;
                existingProduct.CategoryId = product.CategoryId;

                _context.Products.Update(existingProduct);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Product updated successfully!";
                return RedirectToAction("Index");
            }

            // If we got this far, something failed; redisplay form
            ViewBag.Categories = await _context.Categories.ToListAsync();
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Product deleted successfully!";
            }

            return RedirectToAction("Index");
        }
    }
}
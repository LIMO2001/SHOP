using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using LaptopStore.Data;
using LaptopStore.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.IO;
namespace LaptopStore.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ProductManagementController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public ProductManagementController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        public async Task<IActionResult> Index()
        {
            var products = await _context.Products
                .Include(p => p.Category)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return View(products);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await PopulateCategoriesViewBag();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product, IFormFile? imageFile)
        {
            if (ModelState.IsValid)
            {
                // Handle image upload
                if (imageFile != null && imageFile.Length > 0)
                {
                    var imageUrl = await SaveImageAsync(imageFile);
                    if (!string.IsNullOrEmpty(imageUrl))
                    {
                        product.ImageUrl = imageUrl;
                    }
                }

                product.CreatedAt = DateTime.UtcNow;
                _context.Products.Add(product);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Product created successfully!";
                return RedirectToAction("Index");
            }

            await PopulateCategoriesViewBag();
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

            await PopulateCategoriesViewBag();
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Product product, IFormFile? imageFile)
        {
            if (ModelState.IsValid)
            {
                var existingProduct = await _context.Products.FindAsync(product.Id);
                if (existingProduct == null)
                {
                    return NotFound();
                }

                // Handle image upload if a new file is provided
                if (imageFile != null && imageFile.Length > 0)
                {
                    var imageUrl = await SaveImageAsync(imageFile);
                    if (!string.IsNullOrEmpty(imageUrl))
                    {
                        // Delete old image if it's not the default
                        if (existingProduct.ImageUrl != "/images/default-laptop.jpg")
                        {
                            DeleteImage(existingProduct.ImageUrl);
                        }
                        existingProduct.ImageUrl = imageUrl;
                    }
                }
                else
                {
                    // Keep the existing image URL if no new file is uploaded
                    existingProduct.ImageUrl = product.ImageUrl;
                }

                // Update other properties
                existingProduct.Name = product.Name;
                existingProduct.Description = product.Description;
                existingProduct.Price = product.Price;
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

            await PopulateCategoriesViewBag();
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                // Delete associated image if it's not the default
                if (product.ImageUrl != "/images/default-laptop.jpg")
                {
                    DeleteImage(product.ImageUrl);
                }

                _context.Products.Remove(product);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Product deleted successfully!";
            }

            return RedirectToAction("Index");
        }

        private async Task PopulateCategoriesViewBag()
        {
            try
            {
                Console.WriteLine("=== DEBUG: Starting PopulateCategoriesViewBag ===");
                
                // Test if database connection works
                var canConnect = await _context.Database.CanConnectAsync();
                Console.WriteLine($"Database can connect: {canConnect}");
                
                // Get categories with debugging
                var categories = await _context.Categories
                    .OrderBy(c => c.Name)
                    .ToListAsync();
                    
                Console.WriteLine($"Retrieved {categories.Count} categories from database:");
                foreach (var category in categories)
                {
                    Console.WriteLine($"  - ID: {category.Id}, Name: '{category.Name}'");
                }
                
                // Create SelectList
                var selectList = new SelectList(categories, "Id", "Name");
                ViewBag.Categories = selectList;
                
                Console.WriteLine($"Created SelectList with {selectList.Count()} items");
                Console.WriteLine("=== DEBUG: Ending PopulateCategoriesViewBag ===");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR in PopulateCategoriesViewBag: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                
                // Set empty SelectList on error
                ViewBag.Categories = new SelectList(Enumerable.Empty<SelectListItem>());
            }
        }

        private async Task<string> SaveImageAsync(IFormFile imageFile)
        {
            try
            {
                // Validate file type
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                var fileExtension = Path.GetExtension(imageFile.FileName).ToLower();
                
                if (!allowedExtensions.Contains(fileExtension))
                {
                    ModelState.AddModelError("ImageFile", "Only image files (jpg, jpeg, png, gif, webp) are allowed.");
                    return string.Empty;
                }

                // Validate file size (5MB limit)
                if (imageFile.Length > 5 * 1024 * 1024)
                {
                    ModelState.AddModelError("ImageFile", "The image file size must be less than 5MB.");
                    return string.Empty;
                }

                // Create uploads directory if it doesn't exist
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "images", "products");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                // Generate unique filename
                var uniqueFileName = Guid.NewGuid().ToString() + fileExtension;
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                // Save the file
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(fileStream);
                }

                return $"/images/products/{uniqueFileName}";
            }
            catch (Exception ex)
            {
                // Log the error
                Console.WriteLine($"Error saving image: {ex.Message}");
                ModelState.AddModelError("ImageFile", "Error saving image file.");
                return string.Empty;
            }
        }

        private void DeleteImage(string imageUrl)
        {
            try
            {
                if (string.IsNullOrEmpty(imageUrl) || imageUrl == "/images/default-laptop.jpg")
                    return;

                var filePath = Path.Combine(_environment.WebRootPath, imageUrl.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }
            catch (Exception ex)
            {
                // Log the error but don't throw
                Console.WriteLine($"Error deleting image: {ex.Message}");
            }
        }
    }
}
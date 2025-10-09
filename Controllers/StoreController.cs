using Microsoft.AspNetCore.Mvc;
using LaptopStore.Services;
using LaptopStore.Models;

namespace LaptopStore.Controllers
{
    public class StoreController : Controller
    {
        private readonly ProductService _productService;

        public StoreController(ProductService productService)
        {
            _productService = productService;
        }

        public async Task<IActionResult> Index()
        {
            var featuredProducts = await _productService.GetFeaturedProductsAsync();
            var categories = await _productService.GetAllCategoriesAsync();
            
            ViewBag.Categories = categories;
            return View(featuredProducts);
        }

        public async Task<IActionResult> Category(int id)
        {
            var products = await _productService.GetProductsByCategoryAsync(id);
            var category = (await _productService.GetAllCategoriesAsync()).FirstOrDefault(c => c.Id == id);
            
            ViewBag.CategoryName = category?.Name;
            return View(products);
        }

        public async Task<IActionResult> ProductDetails(int id)
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            
            return View(product);
        }

        public async Task<IActionResult> Search(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return RedirectToAction("Index");
            }

            var products = await _productService.SearchProductsAsync(query);
            ViewBag.SearchQuery = query;
            return View(products);
        }
    }
}
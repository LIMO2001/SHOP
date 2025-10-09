using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using LaptopStore.Services;

namespace LaptopStore.Controllers
{
    [Authorize(Roles = "Customer")]
    public class CartController : Controller
    {
        private readonly CartService _cartService;

        public CartController(CartService cartService)
        {
            _cartService = cartService;
        }

        public async Task<IActionResult> Index()
        {
            var userId = HttpContext.Session.GetInt32("UserId") ?? 0;
            var cartItems = await _cartService.GetCartItemsAsync(userId);
            var total = await _cartService.GetCartTotalAsync(userId);
            
            ViewBag.CartTotal = total;
            return View(cartItems);
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
        {
            var userId = HttpContext.Session.GetInt32("UserId") ?? 0;
            await _cartService.AddToCartAsync(userId, productId, quantity);
            
            TempData["Success"] = "Product added to cart successfully!";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> UpdateQuantity(int cartItemId, int quantity)
        {
            var userId = HttpContext.Session.GetInt32("UserId") ?? 0;
            await _cartService.UpdateCartItemQuantityAsync(cartItemId, quantity);
            
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> RemoveFromCart(int cartItemId)
        {
            await _cartService.RemoveFromCartAsync(cartItemId);
            
            TempData["Success"] = "Product removed from cart successfully!";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Checkout()
        {
            var userId = HttpContext.Session.GetInt32("UserId") ?? 0;
            var cartItems = await _cartService.GetCartItemsAsync(userId);
            var total = await _cartService.GetCartTotalAsync(userId);
            
            ViewBag.CartTotal = total;
            return View(cartItems);
        }

        [HttpPost]
        public async Task<IActionResult> PlaceOrder(string shippingAddress, string paymentMethod)
        {
            var userId = HttpContext.Session.GetInt32("UserId") ?? 0;
            
            // Implementation for creating order would go here
            // This would involve creating an Order and OrderItems from cart items
            
            await _cartService.ClearCartAsync(userId);
            
            TempData["Success"] = "Order placed successfully!";
            return RedirectToAction("Index", "Store");
        }
    }
}
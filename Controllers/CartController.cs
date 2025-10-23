using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using LaptopStore.Services;
using LaptopStore.Models;
using LaptopStore.Data;
using Microsoft.EntityFrameworkCore;

namespace LaptopStore.Controllers
{
    // Disable auth temporarily for local testing
    // [Authorize(Roles = "Customer")]
    public class CartController : Controller
    {
        private readonly CartService _cartService;
        private readonly ApplicationDbContext _context;
        private readonly ReceiptService _receiptService;

        public CartController(CartService cartService, ApplicationDbContext context, ReceiptService receiptService)
        {
            _cartService = cartService;
            _context = context;
            _receiptService = receiptService;
        }

        // -------------------------
        // Utility: Get UserId
        // -------------------------
        private int GetUserId()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                // Default for local test — avoid null user errors
                userId = 1;
                HttpContext.Session.SetInt32("UserId", userId.Value);
            }
            return userId.Value;
        }

        // -------------------------
        // CART ACTIONS
        // -------------------------
        public async Task<IActionResult> Index()
        {
            var userId = GetUserId();
            var cartItems = await _cartService.GetCartItemsAsync(userId);
            ViewBag.CartTotal = await _cartService.GetCartTotalAsync(userId);
            return View(cartItems);
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
        {
            var userId = GetUserId();
            var result = await _cartService.AddToCartAsync(userId, productId, quantity);

            if (result != null)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    var count = await _cartService.GetCartItemCountAsync(userId);
                    return Json(new { success = true, message = "Added successfully!", cartCount = count });
                }
                TempData["Success"] = "Product added successfully!";
            }
            else
            {
                TempData["Error"] = "Failed to add product to cart.";
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> UpdateQuantity(int cartItemId, int quantity)
        {
            var userId = GetUserId();
            if (await _cartService.UpdateCartItemQuantityAsync(userId, cartItemId, quantity))
                TempData["Success"] = "Cart updated!";
            else
                TempData["Error"] = "Failed to update item.";

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> RemoveFromCart(int cartItemId)
        {
            var userId = GetUserId();
            var success = await _cartService.RemoveFromCartAsync(userId, cartItemId);

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                var count = await _cartService.GetCartItemCountAsync(userId);
                return Json(new { success, message = success ? "Removed!" : "Failed to remove!", cartCount = count });
            }

            TempData[success ? "Success" : "Error"] = success ? "Removed from cart!" : "Failed to remove!";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<JsonResult> GetCartCount()
        {
            var userId = GetUserId();
            var count = await _cartService.GetCartItemCountAsync(userId);
            return Json(count);
        }

        // -------------------------
        // CHECKOUT
        // -------------------------
        [HttpGet]
        public async Task<IActionResult> Checkout()
        {
            var userId = GetUserId();
            var items = await _cartService.GetCartItemsAsync(userId);
            if (!items.Any())
            {
                TempData["Error"] = "Your cart is empty.";
                return RedirectToAction("Index");
            }

            ViewBag.CartItems = items;
            ViewBag.CartTotal = await _cartService.GetCartTotalAsync(userId);
            return View(new Order { UserId = userId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(Order order, string paymentMethod)
        {
            var userId = GetUserId();

            try
            {
                var cartItems = await _cartService.GetCartItemsAsync(userId);
                if (!cartItems.Any())
                {
                    TempData["Error"] = "Your cart is empty.";
                    return RedirectToAction("Index");
                }

                var totalAmount = cartItems.Sum(c => c.Quantity * (c.Product?.Price ?? 0));

                var newOrder = new Order
                {
                    UserId = userId,
                    ShippingAddress = order.ShippingAddress,
                    PaymentMethod = paymentMethod,
                    TotalAmount = totalAmount,
                    Status = "Completed",
                    OrderDate = DateTime.UtcNow,
                    OrderNumber = Guid.NewGuid().ToString().Substring(0, 8).ToUpper(),
                    OrderItems = new List<OrderItem>()
                };

                // Add order items
                foreach (var item in cartItems)
                {
                    newOrder.OrderItems.Add(new OrderItem
                    {
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        UnitPrice = item.Product?.Price ?? 0,
                        ProductName = item.Product?.Name ?? "Unknown Product"
                    });
                }

                _context.Orders.Add(newOrder);
                await _context.SaveChangesAsync();

                await _cartService.ClearCartAsync(userId);

                TempData["Success"] = $"Order #{newOrder.OrderNumber} placed successfully!";
                return RedirectToAction("OrderConfirmation", new { id = newOrder.Id });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Checkout error: {ex.Message}");
                TempData["Error"] = "An error occurred while placing your order.";
                return RedirectToAction("Checkout");
            }
        }

        // -------------------------
        // ORDER CONFIRMATION
        // -------------------------
        [HttpGet]
        public async Task<IActionResult> OrderConfirmation(int id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems).ThenInclude(oi => oi.Product)
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
                return NotFound();

            return View(order);
        }

        // -------------------------
        // RECEIPT DOWNLOAD
        // -------------------------
        [HttpGet]
        public async Task<IActionResult> DownloadReceipt(int id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems).ThenInclude(oi => oi.Product)
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                TempData["Error"] = "Order not found.";
                return RedirectToAction("Index");
            }

            try
            {
                var user = order.User ?? await _context.Users.FindAsync(order.UserId);
                var items = order.OrderItems?.ToList() ?? new List<OrderItem>();

                var pdfBytes = _receiptService.GenerateReceipt(order, items, user);
                return File(pdfBytes, "application/pdf", $"Receipt-{order.OrderNumber}.pdf");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Receipt generation error: {ex.Message}");
                TempData["Error"] = "Failed to generate receipt. Please try again.";
                return RedirectToAction("OrderConfirmation", new { id });
            }
        }

        // -------------------------
        // CART SUMMARY (AJAX)
        // -------------------------
        [HttpGet]
        public async Task<JsonResult> GetCartSummary()
        {
            var userId = GetUserId();
            var cartItems = await _cartService.GetCartItemsAsync(userId);
            var subtotal = await _cartService.GetCartTotalAsync(userId);

            var shipping = subtotal > 0 ? 10.00m : 0.00m;
            var tax = subtotal * 0.08m;
            var total = subtotal + shipping + tax;

            var items = cartItems.Select(ci => new
            {
                id = ci.Id,
                name = ci.Product?.Name ?? "Unknown Product",
                price = ci.Product?.Price ?? 0,
                quantity = ci.Quantity,
                totalPrice = (ci.Product?.Price ?? 0) * ci.Quantity,
                imageUrl = ci.Product?.ImageUrl ?? "/images/default-laptop.jpg"
            }).ToList();

            return Json(new { items, subtotal, shipping, tax, total });
        }
    }
}

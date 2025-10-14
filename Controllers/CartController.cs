using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using LaptopStore.Services;
using LaptopStore.Models;
using LaptopStore.Data;
using Microsoft.EntityFrameworkCore;

namespace LaptopStore.Controllers
{
    // Remove authorization temporarily for testing
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

        private int GetUserId()
        {
            // For testing, use a fixed user ID
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                userId = 1; // Default user ID for testing
                HttpContext.Session.SetInt32("UserId", userId.Value);
            }
            return userId.Value;
        }

        public async Task<IActionResult> Index()
        {
            var userId = GetUserId();
            var cartItems = await _cartService.GetCartItemsAsync(userId);
            var total = await _cartService.GetCartTotalAsync(userId);
            
            ViewBag.CartTotal = total;
            return View(cartItems);
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
        {
            try
            {
                var userId = GetUserId();
                var result = await _cartService.AddToCartAsync(userId, productId, quantity);
                
                if (result != null)
                {
                    // For AJAX requests, return JSON with cart count
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        var cartCount = await _cartService.GetCartItemCountAsync(userId);
                        return Json(new { 
                            success = true, 
                            message = "Product added to cart successfully!",
                            cartCount = cartCount
                        });
                    }
                    
                    TempData["Success"] = "Product added to cart successfully!";
                }
                else
                {
                    TempData["Error"] = "Failed to add product to cart.";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error adding product to cart: " + ex.Message;
            }
            
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> UpdateQuantity(int cartItemId, int quantity)
        {
            var userId = GetUserId();
            var success = await _cartService.UpdateCartItemQuantityAsync(userId, cartItemId, quantity);
            
            if (success)
            {
                TempData["Success"] = "Cart updated successfully!";
            }
            else
            {
                TempData["Error"] = "Failed to update cart item.";
            }
            
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> RemoveFromCart(int cartItemId)
        {
            var userId = GetUserId();
            var success = await _cartService.RemoveFromCartAsync(userId, cartItemId);
            
            if (success)
            {
                // For AJAX requests, return JSON with cart count
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    var cartCount = await _cartService.GetCartItemCountAsync(userId);
                    return Json(new { 
                        success = true, 
                        message = "Product removed from cart successfully!",
                        cartCount = cartCount
                    });
                }
                
                TempData["Success"] = "Product removed from cart successfully!";
            }
            else
            {
                TempData["Error"] = "Failed to remove product from cart.";
            }
            
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<JsonResult> GetCartCount()
        {
            var userId = GetUserId();
            var cartCount = await _cartService.GetCartItemCountAsync(userId);
            return Json(cartCount);
        }

        [HttpGet]
        public async Task<IActionResult> Checkout()
        {
            var userId = GetUserId();
            
            // Check if cart has items
            var cartItems = await _cartService.GetCartItemsAsync(userId);
            if (!cartItems.Any())
            {
                TempData["Error"] = "Your cart is empty";
                return RedirectToAction("Index");
            }

            // Create a new Order object for the checkout form
            var order = new Order
            {
                UserId = userId
            };

            // Pass cart total to view
            var total = await _cartService.GetCartTotalAsync(userId);
            ViewBag.CartTotal = total;
            ViewBag.CartItems = cartItems;

            return View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(Order order, string paymentMethod)
        {
            var userId = GetUserId();

            // ADD DEBUG LOGGING
            Console.WriteLine("=== CHECKOUT POST STARTED ===");
            Console.WriteLine($"ModelState.IsValid: {ModelState.IsValid}");
            Console.WriteLine($"ShippingAddress: {order?.ShippingAddress}");
            Console.WriteLine($"PaymentMethod: {paymentMethod}");
            Console.WriteLine($"UserId: {userId}");

            if (!ModelState.IsValid)
            {
                Console.WriteLine("=== MODELSTATE ERRORS ===");
                foreach (var state in ModelState)
                {
                    foreach (var error in state.Value.Errors)
                    {
                        Console.WriteLine($"{state.Key}: {error.ErrorMessage}");
                    }
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Get cart items
                    var cartItems = await _cartService.GetCartItemsAsync(userId);
                    Console.WriteLine($"Cart items count: {cartItems.Count}");
                    
                    if (!cartItems.Any())
                    {
                        TempData["Error"] = "Your cart is empty";
                        return RedirectToAction("Index");
                    }

                    // Calculate total amount
                    var totalAmount = cartItems.Sum(ci => ci.Quantity * ci.Product?.Price ?? 0);
                    Console.WriteLine($"Total amount: {totalAmount}");

                    // Create the order
                    var newOrder = new Order
                    {
                        UserId = userId,
                        ShippingAddress = order.ShippingAddress,
                        PaymentMethod = paymentMethod,
                        TotalAmount = totalAmount,
                        Status = "Completed",
                        OrderDate = DateTime.UtcNow,
                        OrderNumber = Guid.NewGuid().ToString().Substring(0, 8).ToUpper()
                    };

                    // Add order items WITH PRODUCT NAME
                    foreach (var cartItem in cartItems)
                    {
                        newOrder.OrderItems.Add(new OrderItem
                        {
                            ProductId = cartItem.ProductId,
                            Quantity = cartItem.Quantity,
                            UnitPrice = cartItem.Product?.Price ?? 0,
                            ProductName = cartItem.Product?.Name ?? "Unknown Product"
                        });
                    }

                    // Save order to database
                    _context.Orders.Add(newOrder);
                    await _context.SaveChangesAsync();

                    Console.WriteLine($"=== ORDER CREATED: {newOrder.OrderNumber} ===");

                    // Clear the cart
                    await _cartService.ClearCartAsync(userId);

                    TempData["Success"] = $"Order #{newOrder.OrderNumber} placed successfully!";
                    return RedirectToAction("OrderConfirmation", new { id = newOrder.Id });
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error creating order: {ex.Message}");
                    Console.WriteLine($"Stack trace: {ex.StackTrace}");
                    TempData["Error"] = "Error placing order. Please try again.";
                }
            }

            // If we got here, something went wrong
            Console.WriteLine("=== CHECKOUT FAILED ===");
            var cartItemsForView = await _cartService.GetCartItemsAsync(userId);
            var total = await _cartService.GetCartTotalAsync(userId);
            ViewBag.CartTotal = total;
            ViewBag.CartItems = cartItemsForView;

            return View(order);
        }

        [HttpGet]
        public async Task<IActionResult> OrderConfirmation(int id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        [HttpGet]
        public async Task<IActionResult> DownloadReceipt(int id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                TempData["Error"] = "Order not found.";
                return RedirectToAction("Index");
            }

            try
            {
                var pdfBytes = _receiptService.GenerateReceipt(order, order.OrderItems.ToList(), order.User);
                return File(pdfBytes, "application/pdf", $"Receipt-{order.OrderNumber}.pdf");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error generating PDF: {ex.Message}");
                TempData["Error"] = "Error generating receipt. Please try again.";
                return RedirectToAction("OrderConfirmation", new { id });
            }
        }

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

            return Json(new
            {
                items,
                subtotal,
                shipping,
                tax,
                total
            });
        }
    }
}
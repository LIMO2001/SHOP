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
        private readonly IMpesaService _mpesaService;

        public CartController(CartService cartService, ApplicationDbContext context, 
                            ReceiptService receiptService, IMpesaService mpesaService)
        {
            _cartService = cartService;
            _context = context;
            _receiptService = receiptService;
            _mpesaService = mpesaService;
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
                    Status = paymentMethod == "M-Pesa" ? "Pending Payment" : "Completed",
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

                // If M-Pesa payment, redirect to M-Pesa payment page
                if (paymentMethod == "M-Pesa")
                {
                    return RedirectToAction("MpesaPayment", new { orderId = newOrder.Id });
                }

                // For other payment methods, complete order immediately
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
        // M-PESA PAYMENT PROCESSING
        // -------------------------

        [HttpPost]
        public async Task<IActionResult> CheckoutWithMpesa([FromBody] MpesaCheckoutRequest request)
        {
            try
            {
                var userId = GetUserId();
                
                // Get cart items and calculate total
                var cartItems = await _cartService.GetCartItemsAsync(userId);
                if (!cartItems.Any())
                    return BadRequest(new { Success = false, Message = "Cart is empty" });

                var totalAmount = cartItems.Sum(ci => ci.Quantity * (ci.Product?.Price ?? 0));

                // Create order
                var order = new Order
                {
                    UserId = userId,
                    TotalAmount = totalAmount,
                    Status = "Pending Payment",
                    OrderDate = DateTime.UtcNow,
                    ShippingAddress = request.ShippingAddress,
                    PaymentMethod = "M-Pesa",
                    OrderNumber = Guid.NewGuid().ToString().Substring(0, 8).ToUpper(),
                    OrderItems = new List<OrderItem>()
                };

                // Create order items
                foreach (var cartItem in cartItems)
                {
                    var orderItem = new OrderItem
                    {
                        ProductId = cartItem.ProductId,
                        Quantity = cartItem.Quantity,
                        UnitPrice = cartItem.Product?.Price ?? 0,
                        ProductName = cartItem.Product?.Name ?? "Unknown Product"
                    };
                    order.OrderItems.Add(orderItem);
                }

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                // Initiate M-Pesa payment using ProcessMpesaPayment logic
                var mpesaResponse = await _mpesaService.InitiateSTKPushAsync(
                    request.PhoneNumber,
                    totalAmount,
                    $"ORDER_{order.Id}",
                    $"Laptop Purchase - Order #{order.OrderNumber}"
                );

                if (mpesaResponse.Success)
                {
                    // Update order with payment reference
                    order.PaymentReference = mpesaResponse.CheckoutRequestID;
                    await _context.SaveChangesAsync();

                    // Store in session for status checking
                    HttpContext.Session.SetString("MpesaCheckoutRequestID", mpesaResponse.CheckoutRequestID);
                    HttpContext.Session.SetInt32("PendingOrderId", order.Id);

                    return Ok(new 
                    { 
                        Success = true, 
                        OrderId = order.Id,
                        CheckoutRequestID = mpesaResponse.CheckoutRequestID,
                        Message = mpesaResponse.CustomerMessage
                    });
                }
                else
                {
                    // If payment fails, mark order as failed
                    order.Status = "Payment Failed";
                    await _context.SaveChangesAsync();
                    
                    return BadRequest(new { Success = false, Message = mpesaResponse.Message });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"M-Pesa checkout error: {ex.Message}");
                return StatusCode(500, new { Success = false, Message = "An error occurred during checkout" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ProcessMpesaPayment([FromBody] MpesaPaymentRequest request)
        {
            try
            {
                var userId = GetUserId();
                
                // Validate order exists and belongs to user
                var order = await _context.Orders
                    .FirstOrDefaultAsync(o => o.Id == request.OrderId && o.UserId == userId);
                    
                if (order == null)
                {
                    return BadRequest(new { Success = false, Message = "Order not found" });
                }

                // Validate order is in pending payment status
                if (order.Status != "Pending Payment")
                {
                    return BadRequest(new { Success = false, Message = "Order payment already processed" });
                }

                // Initiate M-Pesa payment
                var mpesaResponse = await _mpesaService.InitiateSTKPushAsync(
                    request.PhoneNumber,
                    request.Amount,
                    request.AccountReference ?? $"ORDER_{request.OrderId}",
                    request.TransactionDescription ?? "Laptop Purchase"
                );

                if (mpesaResponse.Success)
                {
                    // Update order with payment reference
                    order.PaymentReference = mpesaResponse.CheckoutRequestID;
                    await _context.SaveChangesAsync();

                    // Store in session for status checking
                    HttpContext.Session.SetString("MpesaCheckoutRequestID", mpesaResponse.CheckoutRequestID);
                    HttpContext.Session.SetInt32("PendingOrderId", order.Id);

                    return Ok(new 
                    { 
                        Success = true, 
                        OrderId = order.Id,
                        CheckoutRequestID = mpesaResponse.CheckoutRequestID,
                        CustomerMessage = mpesaResponse.CustomerMessage,
                        Message = "Payment initiated successfully"
                    });
                }
                else
                {
                    return BadRequest(new { Success = false, Message = mpesaResponse.Message });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"M-Pesa processing error: {ex.Message}");
                return StatusCode(500, new { Success = false, Message = "An error occurred while processing payment" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> MpesaPayment(int orderId)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
            {
                TempData["Error"] = "Order not found.";
                return RedirectToAction("Index");
            }

            // Validate order belongs to current user
            var userId = GetUserId();
            if (order.UserId != userId)
            {
                TempData["Error"] = "Access denied.";
                return RedirectToAction("Index");
            }

            ViewBag.Order = order;
            return View();
        }

        [HttpGet]
        public async Task<JsonResult> CheckMpesaPaymentStatus(string checkoutRequestId)
        {
            try
            {
                var payment = await _mpesaService.GetPaymentByCheckoutIdAsync(checkoutRequestId);
                
                if (payment != null && payment.PaymentStatus == "Completed")
                {
                    // Get the pending order ID from session or payment record
                    var orderId = HttpContext.Session.GetInt32("PendingOrderId") ?? payment.OrderId;
                    if (orderId > 0)
                    {
                        var order = await _context.Orders.FindAsync(orderId);
                        if (order != null && order.Status == "Pending Payment")
                        {
                            // Update order status and clear cart
                            order.Status = "Completed";
                            order.PaymentStatus = "Paid";
                            order.PaymentDate = DateTime.UtcNow;
                            order.MpesaReceiptNumber = payment.MpesaReceiptNumber;
                            await _context.SaveChangesAsync();

                            await _cartService.ClearCartAsync(GetUserId());

                            // Clear session data
                            HttpContext.Session.Remove("MpesaCheckoutRequestID");
                            HttpContext.Session.Remove("PendingOrderId");

                            return Json(new { 
                                isPaid = true, 
                                message = "Payment completed successfully!",
                                receiptNumber = payment.MpesaReceiptNumber
                            });
                        }
                    }
                }
                else if (payment != null && payment.PaymentStatus == "Failed")
                {
                    return Json(new { 
                        isPaid = false, 
                        message = "Payment failed. Please try again.",
                        error = payment.ResultDescription
                    });
                }

                return Json(new { 
                    isPaid = false, 
                    message = "Waiting for payment...",
                    status = payment?.PaymentStatus ?? "Pending"
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Payment status check error: {ex.Message}");
                return Json(new { 
                    isPaid = false, 
                    message = "Error checking payment status",
                    error = ex.Message
                });
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

            // Validate order belongs to current user
            var userId = GetUserId();
            if (order.UserId != userId)
            {
                TempData["Error"] = "Access denied.";
                return RedirectToAction("Index");
            }

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

    // M-Pesa Checkout Request Model
    public class MpesaCheckoutRequest
    {
        public string PhoneNumber { get; set; }
        public string ShippingAddress { get; set; }
    }
}
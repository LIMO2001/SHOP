using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using LaptopStore.Models;
using LaptopStore.Services;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using LaptopStore.Data;

namespace LaptopStore.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MpesaController : ControllerBase
    {
        private readonly IMpesaService _mpesaService;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<MpesaController> _logger;

        public MpesaController(
            IMpesaService mpesaService,
            ApplicationDbContext context,
            ILogger<MpesaController> logger)
        {
            _mpesaService = mpesaService;
            _context = context;
            _logger = logger;
        }

        [HttpPost("stkpush")]
        public async Task<IActionResult> InitiateSTKPush([FromBody] MpesaPaymentRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new { success = false, message = "Invalid request data" });
                }

                // Validate order exists
                var order = await _context.Orders.FindAsync(request.OrderId);
                if (order == null)
                {
                    return BadRequest(new { success = false, message = "Order not found" });
                }

                // Check if order is already paid
                if (order.PaymentStatus == "Paid")
                {
                    return BadRequest(new { success = false, message = "Order is already paid" });
                }

                // Use order total if amount not specified or doesn't match
                var amount = request.Amount > 0 ? request.Amount : order.TotalAmount;

                // Validate amount matches order total (with small tolerance for rounding)
                if (Math.Abs(amount - order.TotalAmount) > 1)
                {
                    return BadRequest(new { success = false, message = "Amount does not match order total" });
                }

                var response = await _mpesaService.InitiateSTKPushAsync(
                    request.PhoneNumber, 
                    amount,
                    request.AccountReference ?? $"ORDER_{request.OrderId}",
                    request.TransactionDescription ?? "Laptop Purchase"
                );

                if (response.Success)
                {
                    //  payment reference
                    order.PaymentReference = response.CheckoutRequestID;
                    order.PaymentStatus = "Pending";
                    order.PaymentMethod = "M-Pesa";
                    await _context.SaveChangesAsync();

                    // Update payment record with OrderId
                    var payment = await _context.MpesaPayments
                        .FirstOrDefaultAsync(p => p.CheckoutRequestID == response.CheckoutRequestID);
                    if (payment != null)
                    {
                        payment.OrderId = request.OrderId;
                        await _context.SaveChangesAsync();
                    }

                    return Ok(new
                    {
                        success = true,
                        message = "Payment initiated successfully",
                        checkoutRequestId = response.CheckoutRequestID,
                        merchantRequestId = response.MerchantRequestID,
                        customerMessage = response.CustomerMessage
                    });
                }
                else
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = response.Message
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initiating STK Push");
                return StatusCode(500, new { success = false, message = "An error occurred while processing your payment" });
            }
        }

        [HttpPost("callback")]
        public async Task<IActionResult> Callback([FromBody] MpesaCallback callback)
        {
            try
            {
                _logger.LogInformation("Received M-Pesa callback");

                var success = await _mpesaService.HandleCallbackAsync(callback);
                
                if (success)
                {
                    return Ok(new { ResultCode = 0, ResultDesc = "Success" });
                }
                else
                {
                    return Ok(new { ResultCode = 1, ResultDesc = "Failed" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing M-Pesa callback");
                return Ok(new { ResultCode = 1, ResultDesc = "Failed" });
            }
        }

        [HttpGet("payment-status/{checkoutRequestId}")]
        public async Task<IActionResult> GetPaymentStatus(string checkoutRequestId)
        {
            try
            {
                var payment = await _mpesaService.GetPaymentByCheckoutIdAsync(checkoutRequestId);
                
                if (payment == null)
                {
                    return NotFound(new { success = false, message = "Payment not found" });
                }

                return Ok(new
                {
                    success = true,
                    paymentStatus = payment.PaymentStatus,
                    resultCode = payment.ResultCode,
                    resultDescription = payment.ResultDescription,
                    mpesaReceiptNumber = payment.MpesaReceiptNumber,
                    transactionDate = payment.TransactionDate,
                    orderId = payment.OrderId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payment status");
                return StatusCode(500, new { success = false, message = "Error retrieving payment status" });
            }
        }

        [HttpGet("order/{orderId}/payment-info")]
        public async Task<IActionResult> GetOrderPaymentInfo(int orderId)
        {
            try
            {
                var order = await _context.Orders
                    .Include(o => o.MpesaPayments)
                    .FirstOrDefaultAsync(o => o.Id == orderId);

                if (order == null)
                {
                    return NotFound(new { success = false, message = "Order not found" });
                }

                var paymentInfo = new
                {
                    orderId = order.Id,
                    orderNumber = order.OrderNumber,
                    totalAmount = order.TotalAmount,
                    paymentStatus = order.PaymentStatus,
                    paymentMethod = order.PaymentMethod,
                    paymentReference = order.PaymentReference,
                    mpesaReceiptNumber = order.MpesaReceiptNumber,
                    payments = order.MpesaPayments.Select(p => new
                    {
                        p.Id,
                        p.PhoneNumber,
                        p.Amount,
                        p.PaymentStatus,
                        p.MpesaReceiptNumber,
                        p.TransactionDate
                    }).ToList()
                };

                return Ok(new { success = true, paymentInfo });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting order payment info");
                return StatusCode(500, new { success = false, message = "Error retrieving payment information" });
            }
        }
    }
}
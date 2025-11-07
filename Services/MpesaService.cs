using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using LaptopStore.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using LaptopStore.Data;

namespace LaptopStore.Services
{
    public class MpesaService : IMpesaService
    {
        private readonly MpesaConfig _config;
        private readonly HttpClient _httpClient;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<MpesaService> _logger;
        private bool _configValidated = false;

        public MpesaService(
            IOptions<MpesaConfig> config,
            HttpClient httpClient,
            ApplicationDbContext context,
            ILogger<MpesaService> logger)
        {
            _config = config.Value;
            _httpClient = httpClient;
            _context = context;
            _logger = logger;

            // Validate configuration
            ValidateConfiguration();
        }

        private void ValidateConfiguration()
        {
            _logger.LogInformation("=== 🔧 MPESA CONFIGURATION VALIDATION ===");
            
            // Check each property individually
            _logger.LogInformation("ConsumerKey: {Status}", 
                string.IsNullOrEmpty(_config.ConsumerKey) ? "❌ NULL" : "✅ LOADED");
            _logger.LogInformation("ConsumerSecret: {Status}", 
                string.IsNullOrEmpty(_config.ConsumerSecret) ? "❌ NULL" : "✅ LOADED");
            _logger.LogInformation("Passkey: {Status}", 
                string.IsNullOrEmpty(_config.Passkey) ? "❌ NULL" : "✅ LOADED");
            _logger.LogInformation("BusinessShortCode: {Value}", 
                string.IsNullOrEmpty(_config.BusinessShortCode) ? "NULL" : _config.BusinessShortCode);
            _logger.LogInformation("CallbackUrl: {Value}", 
                string.IsNullOrEmpty(_config.CallbackUrl) ? "NULL" : _config.CallbackUrl);
            _logger.LogInformation("Environment: {Value}", 
                string.IsNullOrEmpty(_config.Environment) ? "NULL" : _config.Environment);

            // Check if all required configs are present
            var hasRequiredConfig = !string.IsNullOrEmpty(_config.ConsumerKey) &&
                                  !string.IsNullOrEmpty(_config.ConsumerSecret) &&
                                  !string.IsNullOrEmpty(_config.Passkey) &&
                                  !string.IsNullOrEmpty(_config.BusinessShortCode);

            if (hasRequiredConfig)
            {
                _configValidated = true;
                _logger.LogInformation("=== ✅ MPESA CONFIGURATION VALID ===");
            }
            else
            {
                _logger.LogError("=== ❌ MPESA CONFIGURATION INVALID ===");
                _logger.LogError("Missing required configuration values. Check appsettings.json");
                throw new InvalidOperationException("Mpesa configuration is incomplete. Check appsettings.json");
            }
        }

        public async Task<string> GetAccessTokenAsync()
        {
            if (!_configValidated)
                throw new InvalidOperationException("Mpesa configuration not validated");

            try
            {
                _logger.LogInformation("🔑 Getting M-Pesa access token...");
                
                var authString = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_config.ConsumerKey}:{_config.ConsumerSecret}"));
                
                var url = _config.Environment == "sandbox" 
                    ? "https://sandbox.safaricom.co.ke/oauth/v1/generate?grant_type=client_credentials"
                    : "https://api.safaricom.co.ke/oauth/v1/generate?grant_type=client_credentials";
                
                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Add("Authorization", $"Basic {authString}");

                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                using var document = JsonDocument.Parse(content);
                var accessToken = document.RootElement.GetProperty("access_token").GetString();
                
                _logger.LogInformation("✅ Access token retrieved successfully");
                return accessToken;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error getting M-Pesa access token");
                throw;
            }
        }

        public async Task<MpesaPaymentResponse> InitiateSTKPushAsync(string phoneNumber, decimal amount, string accountReference, string transactionDesc)
        {
            if (!_configValidated)
                throw new InvalidOperationException("Mpesa configuration not validated");

            _logger.LogInformation("===  STARTING STK PUSH ===");
            _logger.LogInformation("Phone: {Phone}, Amount: {Amount}, Ref: {Reference}", 
                phoneNumber, amount, accountReference);

            try
            {
                var accessToken = await GetAccessTokenAsync();
                var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
                var password = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_config.BusinessShortCode}{_config.Passkey}{timestamp}"));

                // Format phone number
                var formattedPhone = FormatPhoneNumber(phoneNumber);
                
                // ✅ FIX: Convert amount to whole number (M-Pesa requirement)
                string formattedAmount = ((int)Math.Round(amount, MidpointRounding.AwayFromZero)).ToString();
                
                _logger.LogInformation("📞 Formatted phone: {FormattedPhone}", formattedPhone);
                _logger.LogInformation("💰 Original amount: {OriginalAmount}, Formatted amount: {FormattedAmount}", 
                    amount, formattedAmount);

                // ✅ ADDED: Debug logging for callback URL
                _logger.LogInformation("🌐 FINAL Callback URL being used: {CallbackUrl}", _config.CallbackUrl);
                _logger.LogInformation("🔧 BusinessShortCode: {ShortCode}", _config.BusinessShortCode);
                _logger.LogInformation("🔧 Environment: {Environment}", _config.Environment);

                var stkRequest = new
                {
                    BusinessShortCode = _config.BusinessShortCode,
                    Password = password,
                    Timestamp = timestamp,
                    TransactionType = "CustomerPayBillOnline",
                    Amount = formattedAmount,
                    PartyA = formattedPhone,
                    PartyB = _config.BusinessShortCode,
                    PhoneNumber = formattedPhone,
                
                    CallBackURL = "https://webhook.site/dd189cde-6150-4f45-b17e-fd368f3df1cc",
                    AccountReference = accountReference,
                    TransactionDesc = transactionDesc
                };

                
                _logger.LogInformation("📦 STK Request Payload: {@StkRequest}", stkRequest);

                var url = _config.Environment == "sandbox"
                    ? "https://sandbox.safaricom.co.ke/mpesa/stkpush/v1/processrequest"
                    : "https://api.safaricom.co.ke/mpesa/stkpush/v1/processrequest";
                
                var request = new HttpRequestMessage(HttpMethod.Post, url);
                request.Headers.Add("Authorization", $"Bearer {accessToken}");
                request.Content = new StringContent(JsonSerializer.Serialize(stkRequest), Encoding.UTF8, "application/json");

                _logger.LogInformation("📤 Sending STK Push request to: {Url}", url);
                var response = await _httpClient.SendAsync(request);
                var content = await response.Content.ReadAsStringAsync();
                
                _logger.LogInformation("📥 Response Status: {StatusCode}", response.StatusCode);
                _logger.LogInformation("📥 Response Content: {Content}", content);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("❌ STK Push failed with status: {StatusCode}", response.StatusCode);
                    return new MpesaPaymentResponse 
                    { 
                        Success = false, 
                        Message = $"STK Push failed: {response.StatusCode}" 
                    };
                }

                var stkResponse = JsonSerializer.Deserialize<MpesaSTKResponse>(content);
                
                if (stkResponse.ResponseCode != "0")
                {
                    _logger.LogError("❌ STK Push API error: {Error}", stkResponse.ResponseDescription);
                    return new MpesaPaymentResponse 
                    { 
                        Success = false, 
                        Message = stkResponse.ResponseDescription 
                    };
                }
                
                try
                {
                    // Extract OrderId from accountReference (format: "ORDER_34")
                    int? orderId = ExtractOrderIdFromReference(accountReference);
                    
                    var payment = new MpesaPayment
                    {
                        CheckoutRequestID = stkResponse.CheckoutRequestID,
                        MerchantRequestID = stkResponse.MerchantRequestID,
                        PhoneNumber = formattedPhone,
                        Amount = amount, 
                        AccountReference = accountReference,
                        TransactionDescription = transactionDesc,
                        ResponseCode = stkResponse.ResponseCode,
                        ResponseDescription = stkResponse.ResponseDescription,
                        CustomerMessage = stkResponse.CustomerMessage,
                        PaymentStatus = "Pending",
                        OrderId = orderId 
                    };

                    _context.MpesaPayments.Add(payment);
                    await _context.SaveChangesAsync();
                    
                    _logger.LogInformation("✅ Payment record saved successfully. ID: {PaymentId}, OrderId: {OrderId}", payment.Id, orderId);
                }
                catch (Exception dbEx)
                {
                    _logger.LogError(dbEx, "❌ Database error saving payment record");
                    _logger.LogWarning("⚠️ STK Push was successful but payment record couldn't be saved");
                }

                _logger.LogInformation("✅ STK Push initiated successfully. CheckoutRequestID: {CheckoutID}", stkResponse.CheckoutRequestID);

                return new MpesaPaymentResponse
                {
                    Success = true,
                    Message = "Payment initiated successfully",
                    CheckoutRequestID = stkResponse.CheckoutRequestID,
                    MerchantRequestID = stkResponse.MerchantRequestID,
                    CustomerMessage = stkResponse.CustomerMessage
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error initiating STK Push");
                return new MpesaPaymentResponse 
                { 
                    Success = false, 
                    Message = $"Error initiating payment: {ex.Message}" 
                };
            }
        }

        public async Task<bool> HandleCallbackAsync(MpesaCallback callback)
        {
            try
            {
                _logger.LogInformation("📨 Processing M-Pesa callback...");
                var stkCallback = callback.Body.stkCallback;
                
                var payment = await _context.MpesaPayments
                    .FirstOrDefaultAsync(p => p.CheckoutRequestID == stkCallback.CheckoutRequestID);

                if (payment == null)
                {
                    _logger.LogWarning("⚠️ Payment not found for CheckoutRequestID: {CheckoutID}", stkCallback.CheckoutRequestID);
                    return false;
                }

                payment.ResultCode = stkCallback.ResultCode.ToString();
                payment.ResultDescription = stkCallback.ResultDesc;
                payment.UpdatedAt = DateTime.UtcNow;

                if (stkCallback.ResultCode == 0)
                {
                    payment.PaymentStatus = "Completed";
                    _logger.LogInformation("✅ Payment completed successfully");

                    if (stkCallback.CallbackMetadata?.Item != null)
                    {
                        foreach (var item in stkCallback.CallbackMetadata.Item)
                        {
                            switch (item.Name)
                            {
                                case "MpesaReceiptNumber":
                                    payment.MpesaReceiptNumber = item.Value?.ToString();
                                    break;
                                case "Amount":
                                    if (decimal.TryParse(item.Value?.ToString(), out decimal amount))
                                        payment.Amount = amount;
                                    break;
                                case "TransactionDate":
                                    if (long.TryParse(item.Value?.ToString(), out long timestamp))
                                        payment.TransactionDate = DateTime.ParseExact(timestamp.ToString(), "yyyyMMddHHmmss", null);
                                    break;
                                case "PhoneNumber":
                                    payment.PhoneNumber = item.Value?.ToString();
                                    break;
                            }
                        }
                    }

                    // Update order status
                    var order = await _context.Orders
                        .FirstOrDefaultAsync(o => o.PaymentReference == stkCallback.CheckoutRequestID);
                    if (order != null)
                    {
                        order.PaymentStatus = "Paid";
                        order.PaymentDate = DateTime.UtcNow;
                        order.MpesaReceiptNumber = payment.MpesaReceiptNumber;
                        order.Status = "Confirmed";
                        _logger.LogInformation("✅ Order {OrderId} updated to Paid", order.Id);
                    }
                }
                else
                {
                    payment.PaymentStatus = "Failed";
                    _logger.LogWarning("❌ Payment failed: {Error}", stkCallback.ResultDesc);
                    
                    var order = await _context.Orders
                        .FirstOrDefaultAsync(o => o.PaymentReference == stkCallback.CheckoutRequestID);
                    if (order != null)
                    {
                        order.PaymentStatus = "Failed";
                        order.Status = "Payment Failed";
                    }
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error handling M-Pesa callback");
                return false;
            }
        }

        public async Task<MpesaPayment> GetPaymentByCheckoutIdAsync(string checkoutRequestId)
        {
            return await _context.MpesaPayments
                .FirstOrDefaultAsync(p => p.CheckoutRequestID == checkoutRequestId);
        }

        private string FormatPhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrEmpty(phoneNumber))
                return string.Empty;

            if (phoneNumber.StartsWith("0"))
                return "254" + phoneNumber.Substring(1);
            else if (phoneNumber.StartsWith("+"))
                return phoneNumber.Substring(1);
            else
                return phoneNumber;
        }

        private int? ExtractOrderIdFromReference(string accountReference)
        {
            if (string.IsNullOrEmpty(accountReference))
            {
                _logger.LogWarning("⚠️ Account reference is null or empty");
                return null;
            }

            if (accountReference.StartsWith("ORDER_") && int.TryParse(accountReference.Replace("ORDER_", ""), out int orderId))
            {
                _logger.LogInformation("📋 Extracted OrderId: {OrderId} from reference: {Reference}", orderId, accountReference);
                return orderId;
            }
            else
            {
                _logger.LogWarning("⚠️ Could not extract OrderId from reference: {Reference}", accountReference);
                return null;
            }
        }
    }

    internal class MpesaSTKResponse
    {
        public string MerchantRequestID { get; set; }
        public string CheckoutRequestID { get; set; }
        public string ResponseCode { get; set; }
        public string ResponseDescription { get; set; }
        public string CustomerMessage { get; set; }
    }
}
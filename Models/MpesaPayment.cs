using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LaptopStore.Models
{
    public class MpesaPayment
    {
        public int Id { get; set; }
        
        [Required]
        public string CheckoutRequestID { get; set; }
        
        [Required]
        public string MerchantRequestID { get; set; }
        
        public int? OrderId { get; set; }
        
        // ✅ TEMPORARILY REMOVED:
        // public Order Order { get; set; }
        
        [Required]
        public string PhoneNumber { get; set; }
        
        [Required]
        public decimal Amount { get; set; }
        
        // ✅ MAKE THESE NULLABLE
        public string? AccountReference { get; set; }
        public string? TransactionDescription { get; set; }
        public string? ResponseCode { get; set; }
        public string? ResponseDescription { get; set; }
        public string? CustomerMessage { get; set; }
        public string? ResultCode { get; set; }
        public string? ResultDescription { get; set; }
        public string? MpesaReceiptNumber { get; set; }
        
        public DateTime? TransactionDate { get; set; }
        
        public string PaymentStatus { get; set; } = "Pending";
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }

    public class MpesaPaymentRequest
    {
        [Required]
        [Phone]
        public string PhoneNumber { get; set; }
        
        [Required]
        [Range(1, 1000000)]
        public decimal Amount { get; set; }
        
        [Required]
        public int OrderId { get; set; }
        
        public string? AccountReference { get; set; }
        public string? TransactionDescription { get; set; }
    }

    public class MpesaPaymentResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string CheckoutRequestID { get; set; }
        public string MerchantRequestID { get; set; }
        public string CustomerMessage { get; set; }
    }

    // Callback models
    public class MpesaCallback
    {
        public Body Body { get; set; }
    }

    public class Body
    {
        public StkCallback stkCallback { get; set; }
    }

    public class StkCallback
    {
        public string MerchantRequestID { get; set; }
        public string CheckoutRequestID { get; set; }
        public int ResultCode { get; set; }
        public string ResultDesc { get; set; }
        public CallbackMetadata CallbackMetadata { get; set; }
    }

    public class CallbackMetadata
    {
        public List<Item> Item { get; set; }
    }

    public class Item
    {
        public string Name { get; set; }
        public object Value { get; set; }
    }
}
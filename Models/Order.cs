using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LaptopStore.Models
{
    public class Order
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; } = Guid.NewGuid().ToString();
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = "Pending";
        public string ShippingAddress { get; set; } = string.Empty;
        public string PaymentMethod { get; set; } = string.Empty;

        // Add these M-Pesa payment properties
        public string PaymentStatus { get; set; } = "Pending"; // Pending, Paid, Failed
        public string PaymentReference { get; set; } = string.Empty; // M-Pesa CheckoutRequestID
        public DateTime? PaymentDate { get; set; }
        public string MpesaReceiptNumber { get; set; } = string.Empty;

        // Foreign key
        public int UserId { get; set; }
        
        // Navigation property
        public User User { get; set; } = null!;

        // Navigation property
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

        // Navigation property for M-Pesa payments
        public ICollection<MpesaPayment> MpesaPayments { get; set; } = new List<MpesaPayment>();
    }
}
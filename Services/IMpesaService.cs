using LaptopStore.Models;
using System.Threading.Tasks;

namespace LaptopStore.Services
{
    public interface IMpesaService
    {
        Task<string> GetAccessTokenAsync();
        Task<MpesaPaymentResponse> InitiateSTKPushAsync(string phoneNumber, decimal amount, string accountReference, string transactionDesc);
        Task<bool> HandleCallbackAsync(MpesaCallback callback);
        Task<MpesaPayment> GetPaymentByCheckoutIdAsync(string checkoutRequestId);
    }
}
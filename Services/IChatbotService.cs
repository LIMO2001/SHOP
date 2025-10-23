using LaptopStore.Models;

namespace LaptopStore.Services
{
    public interface IChatbotService
    {
        Task<ChatbotResponse> ProcessMessageAsync(string userMessage);
    }
}
using LaptopStore.Models;

namespace LaptopStore.Services
{
    public class ChatbotService : IChatbotService
    {
        private readonly ProductService _productService;

        public ChatbotService(ProductService productService)
        {
            _productService = productService;
        }

        public async Task<ChatbotResponse> ProcessMessageAsync(string userMessage)
        {
            var lowerMessage = userMessage.ToLower();
            var response = new ChatbotResponse();

            // Greeting detection
            if (ContainsAny(lowerMessage, ["hello", "hi", "hey", "greetings"]))
            {
                response.Response = "Hello! I'm your LaptopStore assistant. How can I help you today?";
                response.Suggestions = ["Browse laptops", "Check prices", "Technical support", "Order status"];
            }
            // Product inquiry
            else if (ContainsAny(lowerMessage, ["laptop", "product", "item", "model", "spec"]))
            {
                var products = await _productService.GetAllProductsAsync();
                response.Response = $"We have {products.Count} laptops available. Would you like to browse by brand, price range, or specific features?";
                response.Suggestions = ["Gaming laptops", "Budget laptops", "Business laptops", "Latest models"];
            }
            // Price inquiry
            else if (ContainsAny(lowerMessage, ["price", "cost", "expensive", "cheap", "budget"]))
            {
                response.Response = "Our laptops range from $300 to $3000. We have options for every budget!";
                response.Suggestions = ["Under $500", "$500-$1000", "$1000-$2000", "Premium laptops"];
            }
            // Support inquiry
            else if (ContainsAny(lowerMessage, ["support", "help", "problem", "issue", "warranty"]))
            {
                response.Response = "For technical support, please contact our support team at support@laptopstore.com or call 1-800-LAPTOP.";
                response.Suggestions = ["Warranty info", "Return policy", "Contact support", "FAQ"];
            }
            // Order inquiry
            else if (ContainsAny(lowerMessage, ["order", "track", "shipping", "delivery", "status"]))
            {
                response.Response = "To check your order status, please visit the 'My Orders' section in your account or provide your order number.";
                response.Suggestions = ["Track order", "Shipping info", "Return item", "Contact support"];
            }
            // Brand specific
            else if (ContainsAny(lowerMessage, ["dell", "hp", "lenovo", "asus", "acer", "apple", "macbook"]))
            {
                var brand = GetBrandFromMessage(lowerMessage);
                response.Response = $"We have a great selection of {brand} laptops! Would you like to see gaming models, business laptops, or budget options?";
                response.Suggestions = [$"{brand} gaming", $"{brand} business", $"{brand} budget", "All brands"];
            }
            // Default response
            else
            {
                response.Response = "I'm here to help with laptop purchases, technical support, and order inquiries. What would you like to know?";
                response.Suggestions = ["Browse products", "Pricing info", "Technical support", "Order help"];
            }

            return response;
        }

        private bool ContainsAny(string message, string[] keywords)
        {
            return keywords.Any(keyword => message.Contains(keyword));
        }

        private string GetBrandFromMessage(string message)
        {
            if (message.Contains("dell")) return "Dell";
            if (message.Contains("hp")) return "HP";
            if (message.Contains("lenovo")) return "Lenovo";
            if (message.Contains("asus")) return "ASUS";
            if (message.Contains("acer")) return "Acer";
            if (message.Contains("apple") || message.Contains("macbook")) return "Apple";
            return "laptop";
        }
    }
}
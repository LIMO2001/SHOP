namespace LaptopStore.Models
{
    public class ChatMessage
    {
        public string Message { get; set; } = string.Empty;
        public bool IsUser { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }

    public class ChatbotResponse
    {
        public string Response { get; set; } = string.Empty;
        public List<string> Suggestions { get; set; } = new List<string>();
    }
}
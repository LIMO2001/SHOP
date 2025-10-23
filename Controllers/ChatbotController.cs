using Microsoft.AspNetCore.Mvc;
using LaptopStore.Models;
using LaptopStore.Services;

namespace LaptopStore.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatbotController : ControllerBase
    {
        private readonly IChatbotService _chatbotService;

        public ChatbotController(IChatbotService chatbotService)
        {
            _chatbotService = chatbotService;
        }

        [HttpPost("send")]
        public async Task<ActionResult<ChatbotResponse>> SendMessage([FromBody] ChatMessage message)
        {
            if (string.IsNullOrWhiteSpace(message.Message))
            {
                return BadRequest("Message cannot be empty");
            }

            var response = await _chatbotService.ProcessMessageAsync(message.Message);
            return Ok(response);
        }

        [HttpGet("suggestions")]
        public ActionResult<List<string>> GetInitialSuggestions()
        {
            var suggestions = new List<string>
            {
                "What laptops do you have?",
                "Show me gaming laptops",
                "What's your return policy?",
                "Need technical support",
                "Track my order",
                "Best laptop for students"
            };

            return Ok(suggestions);
        }
    }
}
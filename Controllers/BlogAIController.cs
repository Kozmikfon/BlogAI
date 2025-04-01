using BlogProject.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace BlogProject.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BlogAIController : ControllerBase
    {
        private readonly OpenAIService _aiService;

        public BlogAIController(OpenAIService aiService)
        {
            _aiService = aiService;
        }

        [HttpGet("generate")]
        public async Task<IActionResult> GenerateBlog([FromQuery] string topic = "Yapay Zeka  Geleek")
        {
            var content = await _aiService.GenerateBlogAsync(topic);
            return Ok(new { topic, content });
        }
    }
}

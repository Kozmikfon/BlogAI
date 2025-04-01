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
        public async Task<IActionResult> GenerateBlog()
        {
            var blog = await _aiService.GenerateSmartBlogAsync();
            return Ok(blog);
        }

    }
}

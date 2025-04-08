using BlogProject.Application.Agents;
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
        [HttpPost("generate-test")]
        public async Task<IActionResult> GenerateTestBlog([FromQuery] string category = "teknoloji")
        {
            var agent = HttpContext.RequestServices.GetRequiredService<BlogAgentService>();

            // ❗️ Doğrudan DB'ye kayıt eden metodu çağır
            await agent.GenerateSmartBlogAndSave(category);

            return Ok("✅ Test blog üretildi ve veritabanına kaydedildi.");
        }





    }



}

﻿using BlogProject.Application.Agents;
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
            var store = HttpContext.RequestServices.GetRequiredService<InMemoryBlogStore>();

            var blog = await agent.GenerateSmartBlogAsync(category);

            if (blog == null)
                return StatusCode(500, "AI içerik üretemedi");

            blog.Category = category;
            store.Add(blog);
            return Ok(blog);
        }



    }



}

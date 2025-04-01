using BlogProject.Application.Services;
using BlogProject.Core.Entities;
using Microsoft.AspNetCore.Mvc;

namespace BlogProject.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BlogController : ControllerBase
    {
        private readonly InMemoryBlogStore _store;
        private readonly OpenAIService _aiService;

        public BlogController(InMemoryBlogStore store, OpenAIService aiService)
        {
            _store = store;
            _aiService = aiService;
        }

        //  Tüm blogları getir
        [HttpGet]
        public IActionResult GetAll()
        {
            var blogs = _store.GetAll();
            return Ok(blogs);
        }

        //  ID'ye göre detay
        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var blog = _store.GetById(id);
            if (blog == null)
                return NotFound("Blog bulunamadı.");
            return Ok(blog);
        }

        //  Elle blog üretmek için (opsiyonel)
        [HttpPost("generate")]
        public async Task<IActionResult> GenerateNew()
        {
            var result = await _aiService.GenerateSmartBlogAsync();
            _store.Add(result);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var success = _store.Delete(id);
            if (!success)
                return NotFound();

            return NoContent();
        }

    }
}

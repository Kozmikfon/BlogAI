using BlogProject.Core.Entities;
using BlogProject.Application.Stores;
using Microsoft.AspNetCore.Mvc;

namespace BlogProject.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CommentController : ControllerBase
    {
        private readonly InMemoryCommentStore _commentStore;

        public CommentController(InMemoryCommentStore commentStore)
        {
            _commentStore = commentStore;
        }

        // 🔽 1. Belirli bir bloga ait yorumları getir
        [HttpGet("blog/{blogId}")]
        public IActionResult GetByBlog(int blogId)
        {
            var comments = _commentStore.GetByBlogId(blogId);
            return Ok(comments);
        }

        // 🔽 2. Tüm yorumları getir (admin için)
        [HttpGet]
        public IActionResult GetAll()
        {
            var comments = _commentStore.GetAll();
            return Ok(comments);
        }

        // 🔼 3. Yeni yorum ekle
        [HttpPost]
        public IActionResult Add([FromBody] Comment comment)
        {
            _commentStore.Add(comment);
            return Ok(comment);
        }

        // 🗑️ 4. Yorum sil
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var success = _commentStore.Delete(id);
            if (!success)
                return NotFound();

            return NoContent();
        }
    }
}

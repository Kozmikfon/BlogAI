using BlogProject.Application.Services;
using BlogProject.Core.Entities;
using Microsoft.AspNetCore.Mvc;

namespace BlogProject.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CommentController : ControllerBase
    {
        private readonly CommentService _service;

        public CommentController(CommentService service)
        {
            _service = service;
        }

        // 🧾 Belirli bir bloga ait yorumları getir
        [HttpGet("byblog/{blogId}")]
        public async Task<IActionResult> GetByBlog(int blogId)
        {
            var comments = await _service.GetByBlogIdAsync(blogId);
            return Ok(comments);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var allComments = await _service.GetAllCommentsAsync();
            return Ok(allComments);
        }

        // ➕ Yeni yorum ekle
        [HttpPost]
        public async Task<IActionResult> Add([FromBody] Comment comment)
        {
            // Güvenlik için CreatedAt burada verilmezse, servis içinde atanmalı
            var added = await _service.AddCommentAsync(comment);
            return Ok(added);
        }

        // ❌ Yorum sil
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _service.DeleteCommentAsync(id);
            return result ? Ok("Silindi") : NotFound("Yorum bulunamadı");
        }
    }
}
    
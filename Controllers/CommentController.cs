using BlogProject.Core.Entities;
using BlogProject.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BlogProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommentsController : ControllerBase
    {
        private readonly BlogDbContext _context;

        public CommentsController(BlogDbContext context)
        {
            _context = context;
        }

        // GET: api/comments?blogId=1
        [HttpGet]
        public async Task<IActionResult> GetComments(int blogId)
        {
            var comments = await _context.Comments
                .Where(c => c.BlogId == blogId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            return Ok(comments);
        }

        // POST: api/comments
        [HttpPost]
        public async Task<IActionResult> AddComment([FromBody] Comment comment)
        {
            comment.CreatedAt = DateTime.Now;
            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();
            return Ok(comment);
        }
    }
}

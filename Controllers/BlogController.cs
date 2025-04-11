using BlogProject.Core.Entities;
using BlogProject.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BlogProject.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BlogController : ControllerBase
    {
        private readonly BlogDbContext _db;

        public BlogController(BlogDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var blogs = await _db.Blogs
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

            return Ok(blogs);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var blog = await _db.Blogs.FindAsync(id);
            if (blog == null) return NotFound();
            return Ok(blog);
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Blog updatedBlog)
        {
            var blog = await _db.Blogs.FindAsync(id);
            if (blog == null) return NotFound();

            blog.Title = updatedBlog.Title;
            blog.Content = updatedBlog.Content;
            blog.Summary = updatedBlog.Summary;
            blog.Category = updatedBlog.Category;
            blog.Tags = updatedBlog.Tags;
            blog.ImageUrl = updatedBlog.ImageUrl;

            await _db.SaveChangesAsync();
            return Ok(blog);
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var blog = await _db.Blogs.FindAsync(id);
            if (blog == null) return NotFound();

            _db.Blogs.Remove(blog);
            await _db.SaveChangesAsync();

            return NoContent();
        }

    }
}

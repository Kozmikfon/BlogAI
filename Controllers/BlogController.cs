using BlogProject.Infrastructure.Data;
using BlogProject.Core.Entities;
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

        // ✅ Tüm blogları getir
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var blogs = await _db.Blogs
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

            return Ok(blogs);
        }

        // ✅ ID'ye göre blog getir
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var blog = await _db.Blogs.FindAsync(id);
            if (blog == null) return NotFound();

            return Ok(blog);
        }

        // ✅ Yeni blog ekle (manuel kullanım veya test için)
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] GeneratedBlog blog)
        {
            blog.CreatedAt = DateTime.Now;
            _db.Blogs.Add(blog);
            await _db.SaveChangesAsync();

            return Ok(blog);
        }

        // ✅ Blog sil
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

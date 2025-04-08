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
    }
}

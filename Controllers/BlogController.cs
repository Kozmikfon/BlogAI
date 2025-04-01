using BlogProject.Application.Services;
using BlogProject.Core.Entities;
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

        public BlogController(InMemoryBlogStore store)
        {
            _store = store;
        }

        [HttpGet]
        public ActionResult<List<Blog>> GetAll()
        {
            return Ok(_store.GetAll());
        }

        [HttpGet("{id}")]
        public ActionResult<Blog> GetById(int id)
        {
            var blog = _store.GetById(id);
            if (blog == null) return NotFound();
            return Ok(blog);
        }
    }


}

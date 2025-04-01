using BlogProject.Core.Entities;
using BlogProject.Core.Entities;

namespace BlogProject.Application.Services
{
    public class InMemoryBlogStore
    {
        private readonly List<Blog> _blogs = new();
        private int _nextId = 1;

        public void Add(Blog blog)
        {
            blog.Id = _nextId++;
            blog.CreatedAt = DateTime.Now;
            _blogs.Add(blog);
        }

        public List<Blog> GetAll()
        {
            return _blogs.OrderByDescending(b => b.CreatedAt).ToList();
        }

        public Blog? GetById(int id)
        {
            return _blogs.FirstOrDefault(b => b.Id == id);
        }
    }
}

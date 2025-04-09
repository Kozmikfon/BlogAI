using BlogProject.Application.Repositories;
using BlogProject.Core.Entities;
using BlogProject.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BlogProject.Infrastructure.Repositories
{
    public class BlogRepository : IBlogRepository
    {
        private readonly BlogDbContext _context;

        public BlogRepository(BlogDbContext context)
        {
            _context = context;
        }

        public async Task<List<Blog>> GetAllAsync()
        {
            return await _context.Blogs.OrderByDescending(x => x.CreatedAt).ToListAsync();
        }

        public async Task<Blog?> GetByIdAsync(int id)
        {
            return await _context.Blogs.FindAsync(id);
        }

        public async Task AddAsync(Blog blog)
        {
            _context.Blogs.Add(blog);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var blog = await _context.Blogs.FindAsync(id);
            if (blog != null)
            {
                _context.Blogs.Remove(blog);
                await _context.SaveChangesAsync();
            }
        }
    }
}

using BlogProject.Core.Entities;
using BlogProject.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BlogProject.Application.Services
{
    public class CommentService
    {
        private readonly BlogDbContext _db;

        public CommentService(BlogDbContext db)
        {
            _db = db;
        }

        public async Task<List<Comment>> GetByBlogIdAsync(int blogId)
        {
            return await _db.Comments
                .Where(c => c.BlogId == blogId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<Comment> AddCommentAsync(Comment comment)
        {
            comment.CreatedAt = DateTime.UtcNow;
            _db.Comments.Add(comment);
            await _db.SaveChangesAsync();
            return comment;
        }

        public async Task<bool> DeleteCommentAsync(int id)
        {
            var comment = await _db.Comments.FindAsync(id);
            if (comment == null) return false;

            _db.Comments.Remove(comment);
            await _db.SaveChangesAsync();
            return true;
        }
    }
}

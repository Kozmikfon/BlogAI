using BlogProject.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BlogProject.Application.Jobs
{
    public class BlogCleanupJob
    {
        private readonly BlogDbContext _db;
        private readonly ILogger<BlogCleanupJob> _logger;

        public BlogCleanupJob(BlogDbContext db, ILogger<BlogCleanupJob> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task DeleteOldBlogsAsync()
        {
            var threshold = DateTime.UtcNow.AddDays(-15);

            var oldBlogs = await _db.Blogs
                .Where(b => b.CreatedAt < threshold)
                .ToListAsync();

            if (oldBlogs.Count == 0)
            {
                _logger.LogInformation("🧹 Temizlenecek eski blog bulunamadı.");
                return;
            }

            _db.Blogs.RemoveRange(oldBlogs);
            await _db.SaveChangesAsync();

            _logger.LogWarning($"🧹 {oldBlogs.Count} eski blog silindi.");
        }
    }
}

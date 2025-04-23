using BlogProject.Application.Services;
using BlogProject.Core.Entities;
using BlogProject.Infrastructure.Data;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace BlogProject.Application.Agents
{
    public class BlogAgentService
    {
        private readonly OpenAIService _ai;
        private readonly BlogDbContext _db;
        private readonly ILogger<BlogAgentService> _logger;

        public BlogAgentService(OpenAIService ai, BlogDbContext db, ILogger<BlogAgentService> logger)
        {
            _ai = ai;
            _db = db;
            _logger = logger;
        }

        public async Task<Blog?> GenerateSmartBlogAsync(string category)
        {
            try
            {
                _logger.LogInformation($"🚀 Uzun blog üretimi başlatıldı: {category}");

                // OpenAIService içinde artık parça parça üretim yapılmakta
                var blog = await _ai.GenerateSmartBlogAsync();
                if (blog != null)
                {
                    // Eğer içerik uzunluğu 800'den azsa uyarı ver (ama kayıt yapılabilir)
                    int wordCount = GetWordCount(blog.Content);
                    if (wordCount < 800)
                    {
                        _logger.LogWarning($"⚠️ Üretilen içerik yeterince uzun değil: {wordCount} kelime");
                    }
                    return blog;
                }

                _logger.LogWarning("⛔ Blog null döndü.");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError("🛑 Blog üretim hatası: " + ex.Message);
                return null;
            }
        }

        private int GetWordCount(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return 0;

            return text.Split(new[] { ' ', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).Length;
        }

        public async Task GenerateSmartBlogAndSave(string category)
        {
            var blog = await GenerateSmartBlogAsync(category);

            if (blog != null)
            {
                blog.Category = category;
                blog.CreatedAt = DateTime.UtcNow;

                _db.Blogs.Add(blog);
                await _db.SaveChangesAsync();

                _logger.LogInformation($"✅ AI içerik başarıyla kaydedildi: {blog.Title}");
            }
            else
            {
                _logger.LogWarning("❌ Blog üretimi başarısız oldu, veritabanına kayıt yapılmadı.");
            }
        }
    }
}

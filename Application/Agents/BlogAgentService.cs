using BlogProject.Application.Services;
using BlogProject.Core.Entities;
using BlogProject.Infrastructure.Data; // Veritabanı context
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

        public async Task<GeneratedBlog?> GenerateSmartBlogAsync(string category)
        {
            var recentTitles = await _db.Blogs
                .OrderByDescending(b => b.CreatedAt)
                .Select(b => b.Title!)
                .Take(10)
                .ToListAsync();

            string prompt = $@"
Bugün için {category} kategorisinde yaratıcı, özgün ve bilgi dolu bir blog yazısı üret.

❗️ Son 10 başlık:
- {string.Join("\n- ", recentTitles)}

Bu başlıklara benzemeyen yeni bir konu üret.
İçeriğin yapısı:
- Giriş, Gelişme, Sonuç
- En az 800 kelime
- 1 özet, 3 etiket, 1 görsel URL

Cevabı şu JSON formatında ver:
{{
  ""title"": ""..."",
  ""summary"": ""..."",
  ""content"": ""..."",
  ""imageUrl"": ""..."",
  ""tags"": ""...""
}}";

            var blog = await _ai.GenerateStructuredBlogAsync(prompt, category);

            if (blog == null || string.IsNullOrWhiteSpace(blog.Content) || blog.Content.Length < 400)
            {
                _logger.LogWarning("⛔ İçerik kısa. Tekrar deneniyor...");
                blog = await _ai.GenerateStructuredBlogAsync(prompt, category);
            }

            if (blog != null && string.IsNullOrWhiteSpace(blog.ImageUrl))
            {
                blog.ImageUrl = await _ai.GetImageFromPexelsAsync(category);
            }

            _logger.LogInformation($"✅ Agent tarafından içerik üretildi: {blog?.Title}");
            return blog;
        }

        public async Task GenerateSmartBlogAndSave(string category)
        {
            // Blog oluştur (AI aracılığıyla)
            var blog = await GenerateSmartBlogAsync(category);

            if (blog != null)
            {
                blog.Category = category;

                // PostgreSQL için UTC zaman kullan
                blog.CreatedAt = DateTime.UtcNow;

                // Veritabanına ekle
                _db.Blogs.Add(blog);
                await _db.SaveChangesAsync();

                _logger.LogInformation($"✅ Hangfire ile eklendi (DB): {blog.Title}");
            }
            else
            {
                _logger.LogWarning("⛔ Blog üretimi başarısız oldu, kayıt yapılmadı.");
            }
        }


    }
}

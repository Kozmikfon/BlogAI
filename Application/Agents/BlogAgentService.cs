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
        private int GetWordCount(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return 0;

            return text.Split(new[] { ' ', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).Length;
        }


        public async Task<Blog?> GenerateSmartBlogAsync(string category)
        {
            var recentTitles = await _db.Blogs
                .OrderByDescending(b => b.CreatedAt)
                .Select(b => b.Title!)
                .Take(10)
                .ToListAsync();

            string prompt = $@"
Bugün için {category} kategorisinde son derece detaylı, teknik, özgün ve bilgi dolu bir blog yazısı üret.

Son 10 başlık:
- {string.Join("\n- ", recentTitles)}

Bu başlıklardan tamamen farklı, benzersiz bir konu seç.

📌 Kurallar:
- İçerik **KESİNLİKLE en az 1000 kelime** olacak. Daha az üretirsen içerik geçersiz sayılacak.
- Konuyu çok derinlemesine anlat, örnekler ve açıklamalarla destekle
- Yazı halkın anlayacağı sade dille yazılmış olmalı ama yüzeysel olmamalı
- 1-2 cümlelik etkileyici özet ekle
- 3 adet etiket ver (virgülle ayır)
- Görsel URL’si sadece ""https://source.unsplash.com/..."" ile başlamalı
- **Yalnızca aşağıdaki JSON formatında** yanıt ver, başka açıklama yazma:

{{
  ""title"": ""..."",
  ""summary"": ""..."",
  ""content"": ""..."",
  ""imageUrl"": ""..."",
  ""tags"": ""...""
}}";

            var blog = await _ai.GenerateStructuredBlogAsync(prompt, category);
            var enUzunBlog = blog;
            int maxWordCount = GetWordCount(blog?.Content ?? "");

            for (int i = 0; i < 2; i++) // Toplam 3 deneme (1+2)
            {
                if (maxWordCount >= 800)
                    break;

                _logger.LogWarning($"❗️ İçerik yeterince uzun değil ({maxWordCount} kelime), tekrar deneniyor...");

                var newBlog = await _ai.GenerateStructuredBlogAsync(prompt, category);
                int newWordCount = GetWordCount(newBlog?.Content ?? "");

                if (newWordCount > maxWordCount)
                {
                    enUzunBlog = newBlog;
                    maxWordCount = newWordCount;
                }
            }

            if (enUzunBlog != null && string.IsNullOrWhiteSpace(enUzunBlog.ImageUrl))
            {
                enUzunBlog.ImageUrl = await _ai.GetImageFromPexelsAsync(category);
            }

            _logger.LogInformation($"✅ AI içerik üretildi: {enUzunBlog?.Title} ({maxWordCount} kelime)");
            return enUzunBlog;
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

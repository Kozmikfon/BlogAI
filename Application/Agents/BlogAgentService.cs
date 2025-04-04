using BlogProject.Application.Services;
using BlogProject.Core.Entities;
using BlogProject.Application.Stores;
using Microsoft.Extensions.Logging;

namespace BlogProject.Application.Agents
{
    public class BlogAgentService
    {
        private readonly OpenAIService _ai;
        private readonly InMemoryBlogStore _store;
        private readonly ILogger<BlogAgentService> _logger;

        public BlogAgentService(OpenAIService ai, InMemoryBlogStore store, ILogger<BlogAgentService> logger)
        {
            _ai = ai;
            _store = store;
            _logger = logger;
        }

        public async Task<GeneratedBlog?> GenerateSmartBlogAsync(string category)
        {
            var recentTitles = _store.GetRecentTitles();

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
    }
}

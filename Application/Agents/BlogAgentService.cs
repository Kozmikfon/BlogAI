using BlogProject.Core.Entities;
using BlogProject.Application.Services;
using Microsoft.Extensions.Logging;

namespace BlogProject.Application.Agents
{
    public class BlogAgentService
    {
        private readonly OpenAIService _ai;
        private readonly ILogger<BlogAgentService> _logger;

        public BlogAgentService(OpenAIService ai, ILogger<BlogAgentService> logger)
        {
            _ai = ai;
            _logger = logger;
        }

        public async Task<GeneratedBlog?> GenerateSmartBlogAsync(List<string> recentTitles, string category)
        {
            // 🔧 Agent Prompt
            string prompt = $@"
Bugün için {category} kategorisinde yaratıcı, özgün ve bilgi dolu bir blog yazısı üret.

🧠 Son 5 başlık (bunlara benzemesin):
- {string.Join("\n- ", recentTitles)}


Blog yazısı şu formatta olsun:
- Giriş: Konuya ilgi çeken bir başlangıç
- Gelişme: Konunun detaylı açıklaması, örneklerle destekle
- Sonuç: Konuyu özetle, okuyucuya düşünce ver

🎯 Kurallar:
- İçerik en az **800 kelime** uzunluğunda olsun (çok detaylı yaz)
- Giriş, gelişme, sonuç bölümleri olsun
- Gerçek bilgiler ve örneklerle destekle
- Kategoriyle alakalı etkileyici bir başlık üret
- Farklı bir konu seç (tekrarlama!)
- 1-2 cümlelik bir özet yaz
- 3 adet etiket (virgülle ayır) ver
- Bir görsel URL’si ekle (Unsplash kullanılabilir)

Yanıtı şu JSON formatında ver:
{{
  ""title"": ""..."",
  ""summary"": ""..."",
  ""content"": ""..."",
  ""imageUrl"": ""..."",
  ""tags"": ""...""
}}";

            // 🧠 AI'den içerik al
            var blog = await _ai.GenerateStructuredBlogAsync(prompt);

            // 🛡️ İçerik kontrolü
            if (blog == null || string.IsNullOrWhiteSpace(blog.Content) || blog.Content.Length < 1000)
            {
                _logger.LogWarning("⛔ Üretilen içerik yetersiz. Agent yeniden deniyor...");
                blog = await _ai.GenerateStructuredBlogAsync(prompt);
            }

            // ❌ Hala başarısızsa
            if (blog == null)
            {
                _logger.LogError("❌ Agent 2. denemede de içerik üretemedi.");
                return null;
            }

            _logger.LogInformation($"✅ Agent tarafından içerik üretildi: {blog.Title}");
            return blog;
        }
    }
}

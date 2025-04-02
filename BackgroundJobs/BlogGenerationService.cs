using BlogProject.Application.Agents;
using BlogProject.Application.Services;
using BlogProject.Core.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace BlogProject.BackgroundJobs
{
    public class BlogGenerationService : BackgroundService
    {
        private readonly ILogger<BlogGenerationService> _logger;
        private readonly IServiceScopeFactory _scopeFactory;

        public BlogGenerationService(ILogger<BlogGenerationService> logger, IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("🤖 Blog yazma servisi başlatıldı (AI Agent destekli).");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();

                    var store = scope.ServiceProvider.GetRequiredService<InMemoryBlogStore>();
                    var aiAgent = scope.ServiceProvider.GetRequiredService<BlogAgentService>();

                    // 📚 Son 5 blog başlığını al (tekrar üretimini engellemek için)
                    var recentTitles = store.GetAll()
                                            .OrderByDescending(x => x.CreatedAt)
                                            .Take(5)
                                            .Select(x => x.Title ?? "")
                                            .ToList();

                    // 🔀 Kategori rotasyonu
                    string[] categories = { "Teknoloji", "Bilim", "Sağlık", "Girişimcilik", "Yapay Zeka" };
                    string category = categories[DateTime.Now.Day % categories.Length];

                    _logger.LogInformation($"📡 Agent tetiklendi - Kategori: {category}");

                    // 🧠 Blog üret
                    var blog = await aiAgent.GenerateSmartBlogAsync(recentTitles, category);

                    if (blog != null)
                    {
                        blog.Category = category;
                        store.Add(blog);
                        _logger.LogInformation($"✅ AI tarafından içerik eklendi: {blog.Title}");
                    }
                    else
                    {
                        _logger.LogWarning("⚠️ Agent içerik üretemedi.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"🔥 Agent çalışırken hata oluştu: {ex.Message}");
                }

                // 🕒 Bir sonraki denemeye kadar bekle (TEST: 9999 sn)
                await Task.Delay(TimeSpan.FromSeconds(9999), stoppingToken);
            }
        }
    }
}

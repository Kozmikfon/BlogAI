using BlogProject.Application.Agents;
using BlogProject.Application.Stores;
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
            _logger.LogInformation("🤖 Blog yazma servisi başlatıldı (Agent + Görsel destekli)");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();

                    var store = scope.ServiceProvider.GetRequiredService<InMemoryBlogStore>();
                    var aiAgent = scope.ServiceProvider.GetRequiredService<BlogAgentService>();

                    // 📚 Son başlıkları al
                    var lastTitles = store.GetAll()
                                          .OrderByDescending(x => x.CreatedAt)
                                          .Take(5)
                                          .Select(x => x.Title ?? "")
                                          .ToList();

                    // 🔀 Günlük kategori belirle
                    string[] categories = { "Teknoloji", "Bilim", "Sağlık", "Girişimcilik", "Yapay Zeka" };
                    var now = DateTime.Now;
                    var category = categories[now.Day % categories.Length];

                    _logger.LogInformation($"📡 Agent tetiklendi - Kategori: {category}");

                    // 🧠 Blog üret
                    var blog = await aiAgent.GenerateSmartBlogAsync(lastTitles, category);

                    if (blog != null)
                    {
                        blog.Category = category;
                        store.Add(blog);
                        _logger.LogInformation($"✅ Blog eklendi: {blog.Title}");
                    }
                    else
                    {
                        _logger.LogWarning("⛔ Blog üretilemedi.");
                    }

                    // 🕓 Bekleme süresi (test için uzun, üretimde kısaltılabilir)
                    await Task.Delay(TimeSpan.FromSeconds(9999), stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"🔥 BlogGenerationService hata: {ex.Message}");
                }
            }
        }
    }
}

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
            _logger.LogInformation("🤖 Blog yazma servisi başlatıldı (Zamanlama destekli)");

            while (!stoppingToken.IsCancellationRequested)
            {
                var now = DateTime.Now;

                // 🔁 SADECE 12:00 VEYA 02:00'DE çalış
                if ((now.Hour == 0 && now.Minute == 0) || (now.Hour == 2 && now.Minute == 0))
                {
                    using var scope = _scopeFactory.CreateScope();
                    var store = scope.ServiceProvider.GetRequiredService<InMemoryBlogStore>();
                    var aiAgent = scope.ServiceProvider.GetRequiredService<BlogAgentService>();

                    var lastTitles = store.GetAll()
                                          .OrderByDescending(x => x.CreatedAt)
                                          .Take(10)
                                          .Select(x => x.Title ?? "")
                                          .ToList();

                    string[] categories = { "Teknoloji", "Bilim", "Sağlık", "Girişimcilik", "Yapay Zeka" };
                    var category = categories[now.Day % categories.Length];

                    _logger.LogInformation($"📡 Agent tetiklendi - {now} - Kategori: {category}");

                    var blog = await aiAgent.GenerateSmartBlogAsync( category);
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

                    await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken); // tekrar tetiklenmemesi için
                }

                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken); // sürekli saat kontrolü
            }
        }




    }
}

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

        private int _lastGeneratedHour = -1; // ✅ En son üretim saati

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

                // ✅ Saat 00:00 veya 02:00 ve aynı saat içinde daha önce üretilmemişse
                if ((now.Hour == 0 || now.Hour == 2) && now.Hour != _lastGeneratedHour)
                {
                    _lastGeneratedHour = now.Hour;

                    using var scope = _scopeFactory.CreateScope();

                    var store = scope.ServiceProvider.GetRequiredService<InMemoryBlogStore>();
                    var aiAgent = scope.ServiceProvider.GetRequiredService<BlogAgentService>();

                    string category = GetCategoryForToday(now);

                    _logger.LogInformation($"📡 Agent tetiklendi - Kategori: {category}");

                    var blog = await aiAgent.GenerateSmartBlogAsync(category);

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
                }

                // ⏳ Her dakika kontrol eder
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }

        private string GetCategoryForToday(DateTime now)
        {
            string[] categories = { "Teknoloji", "Bilim", "Sağlık", "Girişimcilik", "Yapay Zeka" };
            return categories[now.Day % categories.Length];
        }
    }
}

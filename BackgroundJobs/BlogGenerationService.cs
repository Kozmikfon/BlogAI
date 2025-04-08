using BlogProject.Application.Agents;
using BlogProject.Infrastructure.Data; // ✅ EF Core DbContext
using Microsoft.Extensions.DependencyInjection;

namespace BlogProject.BackgroundJobs
{
    public class BlogGenerationService : BackgroundService
    {
        private readonly ILogger<BlogGenerationService> _logger;
        private readonly IServiceScopeFactory _scopeFactory;

        private int _lastGeneratedHour = -1;

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

                if ((now.Hour == 0 || now.Hour == 2) && now.Hour != _lastGeneratedHour)
                {
                    _lastGeneratedHour = now.Hour;

                    using var scope = _scopeFactory.CreateScope();

                    var aiAgent = scope.ServiceProvider.GetRequiredService<BlogAgentService>();

                    string category = GetCategoryForToday(now);

                    _logger.LogInformation($"📡 Agent tetiklendi - Kategori: {category}");

                    // 💾 Artık doğrudan DB'ye kaydeden metot
                    await aiAgent.GenerateSmartBlogAndSave(category);
                }

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

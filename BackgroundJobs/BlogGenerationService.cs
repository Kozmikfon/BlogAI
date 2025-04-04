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
    _logger.LogInformation("🤖 AI Agent tabanlı blog üretim servisi başladı.");

    while (!stoppingToken.IsCancellationRequested)
    {
        using var scope = _scopeFactory.CreateScope();
        var store = scope.ServiceProvider.GetRequiredService<InMemoryBlogStore>();
        var aiAgent = scope.ServiceProvider.GetRequiredService<BlogAgentService>();

        string[] categories = { "Teknoloji", "Bilim", "Sağlık", "Girişimcilik", "Yapay Zeka" };
        var category = categories[DateTime.Now.Day % categories.Length];

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

        await Task.Delay(TimeSpan.FromSeconds(9999), stoppingToken); // TEST amaçlı
    }
}


    }
}

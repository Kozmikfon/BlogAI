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
            _logger.LogInformation("🧠 Blog yazma servisi başlatıldı.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var aiService = scope.ServiceProvider.GetRequiredService<OpenAIService>();
                    var store = scope.ServiceProvider.GetRequiredService<InMemoryBlogStore>();

                    var result = await aiService.GenerateSmartBlogAsync();

                    var blog = new GeneratedBlog
                    {
                        Title = result.Title,
                        Summary = result.Summary,
                        Content = result.Content,
                        ImageUrl = result.ImageUrl,
                        Tags = result.Tags
                    };

                    store.Add(blog);
                    _logger.LogInformation($"✅ Blog eklendi: {blog.Title}");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"❌ HATA: {ex.Message}");
                }

                // Tek sefer çalışsın diye uzun delay
                await Task.Delay(TimeSpan.FromSeconds(9999), stoppingToken);
            }
        }
    }
}

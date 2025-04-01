using BlogProject.Application.Services;
using BlogProject.Core.Entities;
using Microsoft.Extensions.DependencyInjection;

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
        _logger.LogInformation("Blog yazma servisi başlatıldı.");

        while (!stoppingToken.IsCancellationRequested)
        {
            if (true) // TEST İÇİN HER ZAMAN ÇALIŞSIN
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    var aiService = scope.ServiceProvider.GetRequiredService<OpenAIService>();
                    var store = scope.ServiceProvider.GetRequiredService<InMemoryBlogStore>();

                    try
                    {
                        // AI'dan konu üret
                        var topic = await aiService.GenerateTopicAsync();
                        _logger.LogInformation($"AI konu üretti: {topic}");

                        // Konuya göre içerik üret
                        var content = await aiService.GenerateBlogAsync(topic);

                        // İçeriğe göre başlık üret
                        var title = await aiService.GenerateTitleAsync(content);

                        // Blog objesi oluştur
                        var blog = new Blog
                        {
                            Title = title,
                            Content = content,
                            Category = "Yapay Zeka",
                            Tags = "AI, teknoloji, blog"
                        };

                        // Store'a ekle
                        store.Add(blog);
                        _logger.LogInformation($"Blog eklendi: {blog.Title}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"HATA: {ex.Message}");
                    }
                }

                await Task.Delay(TimeSpan.FromSeconds(9999), stoppingToken); // tekrar tetiklenmesin
            }
            else
            {
                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }
        }
    }
}

using System.Net.Http.Headers;
using System.Text.Json;

public class PexelsService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly ILogger<PexelsService> _logger;

    public PexelsService(HttpClient httpClient, IConfiguration config, ILogger<PexelsService> logger)
    {
        _httpClient = httpClient;
        _apiKey = config["Pexels:ApiKey"]!;
        _logger = logger;
    }

    public async Task<string> GetImageUrlAsync(string keyword)
    {
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

        var response = await _httpClient.GetAsync($"https://api.pexels.com/v1/search?query={keyword}&per_page=1");
        var json = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("❌ Pexels API hatası: " + json);
            return "https://source.unsplash.com/600x400/?technology"; // fallback
        }

        var doc = JsonDocument.Parse(json);
        var imageUrl = doc.RootElement.GetProperty("photos")[0].GetProperty("src").GetProperty("medium").GetString();
        return imageUrl!;
    }
}

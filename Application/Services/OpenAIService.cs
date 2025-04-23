using BlogProject.Core.Entities;
using BlogProject.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace BlogProject.Application.Services
{
    public class OpenAIService
    {
        private readonly HttpClient _httpClient;
        private readonly HttpClient _pexelsClient;
        private readonly BlogDbContext _db;
        private readonly string _apiKey;
        private readonly ILogger<OpenAIService> _logger;
        private readonly IConfiguration _configuration;

        public OpenAIService(HttpClient httpClient, BlogDbContext db, IHttpClientFactory clientFactory, IConfiguration config, ILogger<OpenAIService> logger)
        {
            _httpClient = httpClient;
            _pexelsClient = clientFactory.CreateClient();
            _configuration = config;
            _apiKey = config["OpenAI:ApiKey"]!;
            _logger = logger;
            _db = db;
        }

        public async Task<Blog> GenerateSmartBlogAsync()
        {
            var topic = await GenerateTopicAsync();
            var title = await GenerateTitleAsync(topic);
            var summary = await GenerateSummaryAsync(topic);
            var intro = await GenerateIntroductionAsync(topic);
            var body = await GenerateBodyAsync(topic);
            var conclusion = await GenerateConclusionAsync(topic);

            var content = $"{intro}\n\n{body}\n\n{conclusion}";

            return new Blog
            {
                Title = title,
                Summary = summary,
                Content = content,
                ImageUrl = await GetImageFromPexelsAsync("technology"),
                Tags = "AI, teknoloji, yazılım"
            };
        }

        public async Task<Blog?> GenerateStructuredBlogAsync(string prompt, string category)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(category))
                    category = "technology";

                category = RemoveTurkishChars(category.ToLower().Trim());

                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", _apiKey);

                var request = new
                {
                    model = "gpt-3.5-turbo",
                    messages = new[] {
                         new { role = "user", content = prompt }
                    },
                    temperature = 0.8,
                    max_tokens = 3000
                };

                var content = new StringContent(
                    JsonSerializer.Serialize(request),
                    Encoding.UTF8,
                    "application/json");

                var response = await _httpClient.PostAsync("https://api.openai.com/v1/chat/completions", content);
                var responseString = await response.Content.ReadAsStringAsync();

                _logger.LogInformation("📥 Agent AI cevabı: " + responseString);

                var json = JsonDocument.Parse(responseString);
                var rawContent = json.RootElement
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString();

                var cleaned = rawContent?
                    .Replace("```json", "")
                    .Replace("```", "")
                    .Trim();

                var result = JsonSerializer.Deserialize<Blog>(cleaned!, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (result != null)
                {
                    result.ImageUrl = await GetImageFromPexelsAsync(category);

                    _httpClient.DefaultRequestHeaders.Clear();
                    _httpClient.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", _apiKey);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError("🛑 Agent JSON çözümlemesi başarısız: " + ex.Message);
                return null;
            }
        }

        private async Task<string> GenerateTopicAsync() =>
            await SimpleChat("Sen yaratıcı bir blog konusu üreticisisin.",
                "Yapay zeka, teknoloji veya gelecekle ilgili bir blog konusu öner.");

        private async Task<string> GenerateTitleAsync(string topic) =>
            await SimpleChat("Sen yaratıcı bir başlık üreticisisin.",
                $"'{topic}' hakkında etkileyici bir başlık öner.");

        private async Task<string> GenerateSummaryAsync(string topic) =>
            await SimpleChat("Sen bir içerik özetleyicisin.",
                $"'{topic}' hakkında 1-2 cümlelik bilgi dolu ve etkileyici bir özet yaz.");

        private async Task<string> GenerateIntroductionAsync(string topic) =>
            await SimpleChat("Sen bir blog giriş bölümü yazıcısısın.",
                $"'{topic}' hakkında dikkat çekici ve giriş niteliğinde bir yazı yaz.");

        private async Task<string> GenerateBodyAsync(string topic) =>
            await SimpleChat("Sen bir blog gelişme bölümü yazıcısısın.",
                $"'{topic}' hakkında detaylı açıklamalar, teknik bilgiler ve örneklerle dolu bir gelişme bölümü yaz.");

        private async Task<string> GenerateConclusionAsync(string topic) =>
            await SimpleChat("Sen bir blog sonuç bölümü yazıcısısın.",
                $"'{topic}' hakkında yazıyı özetleyen ve okuyucuya mesaj veren bir sonuç bölümü yaz.");

        private async Task<string> SimpleChat(string system, string user)
        {
            var request = new
            {
                model = "gpt-3.5-turbo",
                messages = new[] {
                    new { role = "system", content = system },
                    new { role = "user", content = user }
                },
                temperature = 0.8,
                max_tokens = 1000
            };

            var req = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/chat/completions");
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
            req.Content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

            var res = await _httpClient.SendAsync(req);
            var json = await res.Content.ReadAsStringAsync();

            var doc = JsonDocument.Parse(json);
            return doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString()
                .Trim();
        }

        public async Task<string> GetImageFromPexelsAsync(string category)
        {
            try
            {
                var pexelsKey = _configuration["Pexels:ApiKey"];

                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Authorization", pexelsKey);

                var searchQuery = TranslateCategory(category);
                _logger.LogInformation($"📷 Pexels araması yapılıyor: {searchQuery}");

                var usedImageUrls = await _db.Blogs
                    .Where(b => !string.IsNullOrEmpty(b.ImageUrl))
                    .Select(b => b.ImageUrl)
                    .ToListAsync();

                var response = await _httpClient.GetAsync($"https://api.pexels.com/v1/search?query={searchQuery}&per_page=15");
                var json = await response.Content.ReadAsStringAsync();

                using var doc = JsonDocument.Parse(json);
                var photos = doc.RootElement.GetProperty("photos");

                if (photos.GetArrayLength() > 0)
                {
                    var rnd = new Random();
                    string? imageUrl = null;
                    int retryCount = 10;

                    while (retryCount-- > 0)
                    {
                        var index = rnd.Next(photos.GetArrayLength());
                        var candidate = photos[index].GetProperty("src").GetProperty("medium").GetString();

                        if (!usedImageUrls.Contains(candidate))
                        {
                            imageUrl = candidate;
                            break;
                        }
                    }

                    if (imageUrl == null)
                    {
                        imageUrl = photos[0].GetProperty("src").GetProperty("medium").GetString();
                        _logger.LogWarning("⚠️ Tüm resimler daha önce kullanılmıştı, ilk görsel tekrar seçildi.");
                    }

                    return imageUrl!;
                }

                _logger.LogWarning("⚠️ Pexels: Arama sonucunda görsel bulunamadı.");
            }
            catch (Exception ex)
            {
                _logger.LogError("📷 Pexels görsel hatası: " + ex.Message);
            }

            return "https://via.placeholder.com/600x400?text=No+Image";
        }

        private string RemoveTurkishChars(string input)
        {
            var replacements = new Dictionary<char, char>
            {
                { 'ç', 'c' }, { 'ğ', 'g' }, { 'ı', 'i' }, { 'ö', 'o' },
                { 'ş', 's' }, { 'ü', 'u' },
                { 'Ç', 'C' }, { 'Ğ', 'G' }, { 'İ', 'I' }, { 'Ö', 'O' },
                { 'Ş', 'S' }, { 'Ü', 'U' }
            };

            var chars = input.ToCharArray();
            for (int i = 0; i < chars.Length; i++)
            {
                if (replacements.ContainsKey(chars[i]))
                {
                    chars[i] = replacements[chars[i]];
                }
            }

            return new string(chars);
        }

        private string TranslateCategory(string category)
        {
            return category.ToLower() switch
            {
                "teknoloji" => "technology",
                "bilim" => "science",
                "sağlık" => "health",
                "girişimcilik" => "entrepreneurship",
                "yapay zeka" => "artificial intelligence",
                _ => "technology"
            };
        }
    }
}

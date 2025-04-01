using BlogProject.Core.Entities;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace BlogProject.Application.Services
{
    public class OpenAIService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly ILogger<OpenAIService> _logger;

        public OpenAIService(HttpClient httpClient, IConfiguration config, ILogger<OpenAIService> logger)
        {
            _httpClient = httpClient;
            _apiKey = config["OpenAI:ApiKey"]!;
            _logger = logger;
        }

        public async Task<GeneratedBlog> GenerateSmartBlogAsync()
        {
            try
            {
                _logger.LogInformation("🎯 JSON formatlı AI blog üretimi deneniyor...");
                var result = await GenerateBlogFromAI();
                if (!string.IsNullOrWhiteSpace(result?.Title)) return result!;
            }
            catch (Exception ex)
            {
                _logger.LogWarning("❌ JSON üretimi başarısız: " + ex.Message);
            }

            // Fallback: klasik üretim
            var topic = await GenerateTopicAsync();
            var content = await GenerateBlogTextAsync(topic);
            var title = await GenerateTitleAsync(content);

            return new GeneratedBlog
            {
                Title = title,
                Summary = content.Substring(0, Math.Min(120, content.Length)) + "...",
                Content = content,
                ImageUrl = "https://source.unsplash.com/600x400/?technology",
                Tags = "AI, teknoloji, yazılım"
            };
        }

        // --- JSON formatlı üretim ---
        private async Task<GeneratedBlog> GenerateBlogFromAI()
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

            var prompt = @"
Teknoloji, yapay zeka veya bilimle ilgili bir blog yazısı oluştur.
Giriş, gelişme, sonuç yapısında olsun.
Ayrıca başlık, özet, 3 etiket ve görsel URL'si ver.
Cevabı bu formatta döndür:

{
  ""title"": ""..."",
  ""summary"": ""..."",
  ""content"": ""..."",
  ""imageUrl"": ""..."",
  ""tags"": ""...""
}";

            var request = new
            {
                model = "gpt-3.5-turbo",
                messages = new[] {
                    new { role = "user", content = prompt }
                },
                temperature = 0.8
            };

            var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("https://api.openai.com/v1/chat/completions", content);
            var responseString = await response.Content.ReadAsStringAsync();

            _logger.LogInformation("📥 AI cevabı: " + responseString);

            var json = JsonDocument.Parse(responseString);
            var message = json.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .ToString();

            Console.WriteLine("🎯 Temizlenmemiş AI cevabı:");
            Console.WriteLine(message);


            var cleaned = message
                .Replace("```json", "")
                .Replace("```", "")
                .Trim();

            var result = JsonSerializer.Deserialize<GeneratedBlog>(cleaned);
            return result!;
        }

        // --- Klasik fallback üretimler ---
        private async Task<string> GenerateTopicAsync() =>
            await SimpleChat("Sen yaratıcı bir blog konusu üreticisisin.",
                "Yapay zeka, teknoloji veya gelecekle ilgili bir blog konusu öner.");

        private async Task<string> GenerateBlogTextAsync(string topic) =>
            await SimpleChat("Sen deneyimli bir blog yazarısın.",
                $"'{topic}' hakkında detaylı ve özgün bir blog yazısı yaz.");

        private async Task<string> GenerateTitleAsync(string content) =>
            await SimpleChat("Sen yaratıcı bir başlık üreticisisin.",
                $"Bu yazı için etkileyici bir başlık öner:\n\n{content}");

        private async Task<string> SimpleChat(string system, string user)
        {
            var request = new
            {
                model = "gpt-3.5-turbo",
                messages = new[]
                {
                    new { role = "system", content = system },
                    new { role = "user", content = user }
                },
                temperature = 0.8
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
    }
}

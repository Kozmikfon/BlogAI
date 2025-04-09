using BlogProject.Core.Entities;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace BlogProject.Application.Services
{
    public class OpenAIService
    {
        private readonly HttpClient _httpClient;
        private readonly HttpClient _pexelsClient;

        private readonly string _apiKey;
        private readonly ILogger<OpenAIService> _logger;
        private readonly IConfiguration _configuration;

        public OpenAIService(HttpClient httpClient, IHttpClientFactory clientFactory, IConfiguration config, ILogger<OpenAIService> logger)
        {
            _httpClient = httpClient;
            _pexelsClient = clientFactory.CreateClient(); // yeni client
            _configuration = config;
            _apiKey = config["OpenAI:ApiKey"]!;
            _logger = logger;
        }

        // Ana metot – agent veya klasik sistem tarafından çağrılır
        public async Task<Blog> GenerateSmartBlogAsync()
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

            // Fallback klasik üretim
            var topic = await GenerateTopicAsync();
            var content = await GenerateBlogTextAsync(topic);
            var title = await GenerateTitleAsync(content);

            return new Blog
            {
                Title = title,
                Summary = content.Substring(0, Math.Min(120, content.Length)) + "...",
                Content = content,
                ImageUrl = "https://source.unsplash.com/600x400/?technology",
                Tags = "AI, teknoloji, yazılım"
            };
        }

        // AI'den yapılandırılmış içerik üret (agent'tan gelen prompt ile)
        public async Task<Blog?> GenerateStructuredBlogAsync(string prompt, string category)
        {
            try
            {
                // ✅ Güvenli kategori üret
                if (string.IsNullOrWhiteSpace(category))
                    category = "technology";

                category = RemoveTurkishChars(category.ToLower().Trim());

                // 🧠 OpenAI çağrısı için header'ı ayarla
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", _apiKey);

                var request = new
                {
                    model = "gpt-3.5-turbo",
                    messages = new[] {
                         new { role = "user", content = prompt }
            },
                    temperature = 0.8
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
                    // 🔄 Önce Pexels görseli al
                    result.ImageUrl = await GetImageFromPexelsAsync(category);

                    // ✅ Sonra OpenAI için Authorization tekrar ekleniyor
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


        // AI'den doğrudan JSON blog üret
        private async Task<Blog> GenerateBlogFromAI()
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

            var prompt = @"
Teknoloji, yapay zeka veya bilimle ilgili bir blog yazısı oluştur.
Giriş, gelişme, sonuç yapısında olsun.

🎯 Kurallar:
- İçerik en az **800 kelime** uzunluğunda olsun (çok detaylı yaz)
- Giriş, gelişme, sonuç bölümleri olsun
- Gerçek bilgiler ve örneklerle destekle
- Kategoriyle alakalı etkileyici bir başlık üret
- Farklı bir konu seç (tekrarlama!)
- 1-2 cümlelik bir özet yaz
- 3 adet etiket (virgülle ayır) ver
- Görsel URL'si verirken sadece ""https://source.unsplash.com/..."" ile başlayan, doğrudan açılabilen bir görsel linki üret.



Cevabı şu formatta ve SADECE JSON olarak döndür:

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

            _logger.LogInformation("📥 JSON formatlı AI cevabı: " + responseString);

            var json = JsonDocument.Parse(responseString);
            var raw = json.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            var cleaned = raw?
                .Replace("```json", "")
                .Replace("```", "")
                .Trim();

            var result = JsonSerializer.Deserialize<Blog>(cleaned!);
            return result!;
        }

        // Fallback üretim – konu + içerik + başlık
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
                messages = new[] {
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

        public async Task<string> GetImageFromPexelsAsync(string category)
        {
            try
            {
                var pexelsKey = _configuration["Pexels:ApiKey"];

                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Authorization", pexelsKey);

                var searchQuery = TranslateCategory(category);
                _logger.LogInformation($"📷 Pexels araması yapılıyor: {searchQuery}");

                var response = await _httpClient.GetAsync($"https://api.pexels.com/v1/search?query={searchQuery}&per_page=15");
                var json = await response.Content.ReadAsStringAsync();

                using var doc = JsonDocument.Parse(json);
                var photos = doc.RootElement.GetProperty("photos");

                if (photos.GetArrayLength() > 0)
                {
                    var rnd = new Random();
                    var index = rnd.Next(photos.GetArrayLength());
                    var imageUrl = photos[index].GetProperty("src").GetProperty("medium").GetString();
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

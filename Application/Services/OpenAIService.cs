using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace BlogProject.Application.Services
{
    public class OpenAIService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public OpenAIService(IConfiguration config)
        {
            _httpClient = new HttpClient();
            _apiKey = config["OpenAI:ApiKey"]!;
        }

        public async Task<string> GenerateBlogAsync(string topic)
        {
            var requestBody = new
            {
                model = "gpt-3.5-turbo",
                messages = new[]
                {
                    new { role = "system", content = "Sen deneyimli bir blog yazarı gibi davran." },
                    new { role = "user", content = $"'{topic}' hakkında detaylı ve özgün bir blog yazısı yaz." }
                },
                max_tokens = 600,
                temperature = 0.7
            };

            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/chat/completions");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
            request.Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);
            var jsonString = await response.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(jsonString);
            if (doc.RootElement.TryGetProperty("choices", out var choicesElement))
            {
                return choicesElement[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString();
            }
            else if (doc.RootElement.TryGetProperty("error", out var errorElement))
            {
                var message = errorElement.GetProperty("message").GetString();
                throw new Exception($"OpenAI API hatası: {message}");
            }
            else
            {
                throw new Exception("Bilinmeyen bir hata oluştu. OpenAI cevabı beklenenden farklı.");
            }

        }
        public async Task<string> GenerateTitleAsync(string content)
        {
            var requestBody = new
            {
                model = "gpt-3.5-turbo",
                messages = new[]
                {
            new { role = "system", content = "Sen yaratıcı bir başlık üreticisisin." },
            new { role = "user", content = $"Aşağıdaki yazı için etkileyici bir başlık öner:\n\n{content}" }
        },
                max_tokens = 50,
                temperature = 0.8
            };

            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/chat/completions");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
            request.Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);
            var jsonString = await response.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(jsonString);
            return doc.RootElement
                      .GetProperty("choices")[0]
                      .GetProperty("message")
                      .GetProperty("content")
                      .GetString();
        }
        public async Task<string> GenerateTopicAsync()
        {
            var requestBody = new
            {
                model = "gpt-3.5-turbo",
                messages = new[]
                {
            new { role = "system", content = "Sen yaratıcı bir blog konusu üreticisisin." },
            new { role = "user", content = "Yapay zeka, teknoloji, bilim veya gelecekle ilgili bir blog konusu önerir misin?" }
        },
                max_tokens = 50,
                temperature = 0.9
            };

            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/chat/completions");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
            request.Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);
            var jsonString = await response.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(jsonString);
            return doc.RootElement
                      .GetProperty("choices")[0]
                      .GetProperty("message")
                      .GetProperty("content")
                      .GetString()
                      .Trim();
        }



    }
}

using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using AI_Writing_Assistant.Forms;
using AI_Writing_Assistant.Models;

namespace AI_Writing_Assistant.Services
{
    public class OpenAiService : BaseAiService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string apiUrl;

        public OpenAiService(IHttpClientFactory httpClientFactory, SettingsService settingsService) : base(settingsService)
        {
            _httpClientFactory = httpClientFactory;
            apiUrl = "https://api.openai.com/v1/chat/completions";
        }

        protected override async Task<string> CallWritingSuggestionApiAsync(string text)
        {
            var apiKey = _settingsService.GetApiKey();
            var requestBody = new
            {
                model = "gpt-4.1-mini",
                response_format = new { type = "json_object" },
                messages = new[]
                {
                    new {
                        role = "system",
                        content = _settingsService.GetWritingSystemPrompt()
                    },
                    new {
                        role = "user",
                        content = $"Please help me improve the following content:\n \"{text}\""
                    }
                },
                max_tokens = 1000,
                temperature = 0.5
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var httpClient = _httpClientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

            var response = await httpClient.PostAsync(apiUrl, content);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }

        protected override async Task<string> CallTranslationApiAsync(string text)
        {
            var apiKey = _settingsService.GetApiKey();
            var requestBody = new
            {
                model = "gpt-4.1-mini",
                response_format = new { type = "json_object" },
                messages = new[]
                {
                    new {
                        role = "system",
                        content = _settingsService.GetTranslationSystemPrompt()
                    },
                    new {
                        role = "user",
                        content = $"Translate the following text to Vietnamese:\n \"{text}\""
                    }
                },
                max_tokens = 1000,
                temperature = 0.7
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var httpClient = _httpClientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

            var response = await httpClient.PostAsync(apiUrl, content);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }
    }

    // Helper classes for deserializing OpenAI API response
    public class OpenAIChatCompletionResponse
    {
        [JsonPropertyName("choices")]
        public List<Choice>? Choices { get; set; }
    }

    public class Choice
    {
        [JsonPropertyName("message")]
        public Message? Message { get; set; }
    }

    public class Message
    {
        [JsonPropertyName("role")]
        public string? Role { get; set; }

        [JsonPropertyName("content")]
        public string? Content { get; set; }
    }
}

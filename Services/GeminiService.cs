using AI_Writing_Assistant.Forms;
using AI_Writing_Assistant.Models;
using GenerativeAI;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace AI_Writing_Assistant.Services
{
    public class GeminiService : BaseAiService
    {

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string apiUrl;

        public GeminiService(IHttpClientFactory httpClientFactory, SettingsService settingsService) : base(settingsService)
        {
            _httpClientFactory = httpClientFactory;
            apiUrl = "https://generativelanguage.googleapis.com/v1beta/models/gemini-pro:generateContent";
        }

        protected override async Task<string> CallWritingSuggestionApiAsync(string text)
        {
            var requestBody = new
            {
                contents = new[]
                {
                        new
                        {
                            parts = new[]
                            {
                                new { text = $"{_settingsService.GetWritingSystemPrompt()}\n\nPlease help me improve the following content and provide the output in JSON format: \n \"{text}\"" }
                            }
                        }
                    },
                generationConfig = new
                {
                    temperature = 0.3,
                    topK = 40,
                    topP = 0.95,
                    maxOutputTokens = 1024
                }
            };
            return await CallApiAsync(requestBody);
        }

        protected override async Task<string> CallTranslationApiAsync(string text)
        {

            var requestBody = new
            {
                contents = new[]
                {
                        new
                        {
                            parts = new[]
                            {
                                new { text = $"{_settingsService.GetTranslationSystemPrompt()}\n\nPlease translate the following text to Vietnamese:\n \"{text}\"" }
                            }
                        }
                    },
                generationConfig = new
                {
                    temperature = 0.3,
                    topK = 40,
                    topP = 0.95,
                    maxOutputTokens = 1024
                }
            };

           return await CallApiAsync(requestBody);
        }

        private async Task<string> CallApiAsync(Object requestBody)
        {
            var apiKey = _settingsService.GetApiKey();
            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var url = $"{apiUrl}?key={_settingsService.GetApiKey()}";

            var httpClient = _httpClientFactory.CreateClient();
            var response = await httpClient.PostAsync(url, content);

            if (response.IsSuccessStatusCode)
                return await response.Content.ReadAsStringAsync();

            return string.Empty;
        }

        protected override List<WritingSuggestion> ParseSuggestionResponse(string response)
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                var geminiResponse = JsonSerializer.Deserialize<GeminiResponse>(response, options);

                if (geminiResponse?.Candidates?.Length > 0 &&
                    geminiResponse.Candidates[0].Content?.Parts?.Length > 0)
                {
                    var responseText = geminiResponse.Candidates[0].Content.Parts[0].Text;

                    // Extract JSON from the response (in case it's wrapped in markdown)
                    var jsonStart = responseText.IndexOf('[');
                    var jsonEnd = responseText.LastIndexOf(']') + 1;

                    if (jsonStart >= 0 && jsonEnd > jsonStart)
                    {
                        var jsonText = responseText.Substring(jsonStart, jsonEnd - jsonStart);
                        return JsonSerializer.Deserialize<List<WritingSuggestion>>(jsonText) ?? new List<WritingSuggestion>();
                    }
                }
                return new List<WritingSuggestion>();
            }
            catch (JsonException ex)
            {
                System.Console.WriteLine($"Error parsing AI response: {ex.Message}");
            }
            return new List<WritingSuggestion>();
        }

        protected override string ParseTranslationResponse(string response)
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                var geminiResponse = JsonSerializer.Deserialize<GeminiResponse>(response, options);

                if (geminiResponse?.Candidates?.Length > 0 &&
                    geminiResponse.Candidates[0].Content?.Parts?.Length > 0)
                {
                    var responseText = geminiResponse.Candidates[0].Content.Parts[0].Text;

                    // Extract JSON from the response (in case it's wrapped in markdown)
                    var jsonStart = responseText.IndexOf('[');
                    var jsonEnd = responseText.LastIndexOf(']') + 1;

                    if (jsonStart >= 0 && jsonEnd > jsonStart)
                    {
                        var jsonText = responseText.Substring(jsonStart, jsonEnd - jsonStart);
                        if (!string.IsNullOrEmpty(jsonText))
                        {
                            var translationResponse = JsonSerializer.Deserialize<TranslationResponse>(jsonText, options);
                            return translationResponse?.Translation ?? string.Empty;
                        }
                    }
                }
                return string.Empty;
            }
            catch (JsonException ex)
            {
                System.Console.WriteLine($"Error parsing AI response: {ex.Message}");
            }

            return string.Empty;
        }
    }

    // Helper classes for Gemini API response
    public class GeminiResponse
    {
        [JsonPropertyName("candidates")]
        public Candidate[]? Candidates { get; set; }
    }

    public class Candidate
    {
        [JsonPropertyName("content")]
        public ResponseContent? Content { get; set; }
    }

    public class ResponseContent
    {
        [JsonPropertyName("parts")]
        public Part[]? Parts { get; set; }
    }

    public class Part
    {
        [JsonPropertyName("text")]
        public string Text { get; set; } = string.Empty;
    }
}

using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AI_Writing_Assistant
{
    public class AIService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly SettingsService _settingsService;
        private readonly string apiUrl;

        public AIService(IHttpClientFactory httpClientFactory, SettingsService settingsService)
        {
            _httpClientFactory = httpClientFactory;
            _settingsService = settingsService;
            apiUrl = "https://api.openai.com/v1/chat/completions";
        }

        public async Task<List<WritingSuggestion>> GetWritingSuggestions(string text)
        {
            var apiKey = _settingsService.GetApiKey();
            try
            {
                // Fallback for when API key is not configured
                if (string.IsNullOrEmpty(apiKey) || apiKey.Contains("your-api-key"))
                {
                    return GetDefaultText(text);
                }

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
                var responseContent = await response.Content.ReadAsStringAsync();

                return ParseAIResponse(responseContent);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error calling AI service: {ex.Message}");
                return GetDefaultText(text);
            }
        }

        public async Task<string> TranslateToVietnameseAsync(string text)
        {
            var apiKey = _settingsService.GetApiKey();
            if (string.IsNullOrEmpty(apiKey) || apiKey.Contains("your-api-key"))
            {
                return "API key not configured.";
            }

            try
            {
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
                var responseContent = await response.Content.ReadAsStringAsync();

                return ParseTranslationResponse(responseContent);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error calling AI service for translation: {ex.Message}");
                return "Error during translation.";
            }
        }

        private List<WritingSuggestion> GetDefaultText(string text)
        {
            var suggestions = new List<WritingSuggestion>();

            suggestions.Add(new WritingSuggestion
            {
                Type = "Style",
                ImprovedText = text.Trim(),
                Reason = "Cleaned up whitespace and formatting"
            });

            return suggestions;
        }

        private List<WritingSuggestion> ParseAIResponse(string response)
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var aiResponse = JsonSerializer.Deserialize<OpenAIChatCompletionResponse>(response, options);

                if (aiResponse?.Choices?.Count > 0)
                {
                    var messageContent = aiResponse.Choices[0].Message?.Content;
                    if (!string.IsNullOrEmpty(messageContent))
                    {
                        var suggestionsResponse = JsonSerializer.Deserialize<SuggestionsResponse>(messageContent, options);
                        return suggestionsResponse?.Suggestions ?? new List<WritingSuggestion>();
                    }
                }
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"Error parsing AI response: {ex.Message}");
            }

            return new List<WritingSuggestion>();
        }

        private string ParseTranslationResponse(string response)
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var aiResponse = JsonSerializer.Deserialize<OpenAIChatCompletionResponse>(response, options);

                if (aiResponse?.Choices?.Count > 0)
                {
                    var messageContent = aiResponse.Choices[0].Message?.Content;
                    if (!string.IsNullOrEmpty(messageContent))
                    {
                        var translationResponse = JsonSerializer.Deserialize<TranslationResponse>(messageContent, options);
                        return translationResponse?.Translation ?? "No translation found.";
                    }
                }
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"Error parsing translation response: {ex.Message}");
            }

            return "Error parsing translation.";
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

    public class SuggestionsResponse
    {
        [JsonPropertyName("suggestions")]
        public List<WritingSuggestion>? Suggestions { get; set; }
    }

    public class TranslationResponse
    {
        [JsonPropertyName("translation")]
        public string? Translation { get; set; }
    }
}

using AI_Writing_Assistant.Forms;
using AI_Writing_Assistant.Models;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace AI_Writing_Assistant.Services
{
    public abstract class BaseAiService : IAiService
    {
        protected readonly SettingsService _settingsService;

        protected BaseAiService(SettingsService settingsService)
        {
            _settingsService = settingsService;
        }

        public async Task<List<WritingSuggestion>> GetWritingSuggestions(string text)
        {
            var apiKey = _settingsService.GetApiKey();
            if (string.IsNullOrEmpty(apiKey) || apiKey.Contains("your-api-key"))
            {
                return GetDefaultText(text);
            }

            try
            {
                var response = await CallWritingSuggestionApiAsync(text);
                return ParseSuggestionResponse(response);
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine($"Error calling AI service: {ex.Message}");
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
                var response = await CallTranslationApiAsync(text);
                return ParseTranslationResponse(response);
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine($"Error calling AI service for translation: {ex.Message}");
                return "Error during translation.";
            }
        }

        protected abstract Task<string> CallWritingSuggestionApiAsync(string text);
        protected abstract Task<string> CallTranslationApiAsync(string text);

        private List<WritingSuggestion> GetDefaultText(string text)
        {
            var suggestions = new List<WritingSuggestion>
            {
                new WritingSuggestion
                {
                    Type = "Style",
                    ImprovedText = text.Trim(),
                    Reason = "Cleaned up whitespace and formatting"
                }
            };
            return suggestions;
        }

        protected abstract List<WritingSuggestion> ParseSuggestionResponse(string response);
        protected abstract string ParseTranslationResponse(string response);
    }
}

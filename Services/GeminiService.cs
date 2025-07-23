using AI_Writing_Assistant.Forms;
using AI_Writing_Assistant.Models;
using GenerativeAI;
using System.Text.Json;

namespace AI_Writing_Assistant.Services
{
    public class GeminiService : BaseAiService
    {
        public GeminiService(SettingsService settingsService) : base(settingsService)
        {
        }

        protected override async Task<string> CallWritingSuggestionApiAsync(string text)
        {
            var apiKey = _settingsService.GetApiKey();
            var model = new GenerativeModel(apiKey, model: "gemini-1.5-flash");
            var prompt = $"{_settingsService.GetWritingSystemPrompt()}\n\nPlease help me improve the following content:\n \"{text}\"";
            var response = await model.GenerateContentAsync(prompt);
            return response.Text;
        }

        protected override async Task<string> CallTranslationApiAsync(string text)
        {
            var apiKey = _settingsService.GetApiKey();
            var model = new GenerativeModel(apiKey, model: "gemini-1.5-flash");
            var prompt = $"{_settingsService.GetTranslationSystemPrompt()}\n\nTranslate the following text to Vietnamese:\n \"{text}\"";
            var response = await model.GenerateContentAsync(prompt);
            return response.Text;
        }
    }
}

using System;
using Microsoft.Extensions.DependencyInjection;

namespace AI_Writing_Assistant.Services
{
    public interface IAiServiceFactory
    {
        IAiService CreateService();
    }

    public class AiServiceFactory : IAiServiceFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public AiServiceFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IAiService CreateService()
        {
            var settingsService = _serviceProvider.GetRequiredService<SettingsService>();
            var aiProvider = settingsService.GetAiProvider();
            return aiProvider switch
            {
                AiProvider.Gemini => _serviceProvider.GetRequiredService<GeminiService>(),
                _ => _serviceProvider.GetRequiredService<OpenAiService>(),
            };
        }
    }
}

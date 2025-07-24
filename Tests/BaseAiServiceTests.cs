using Xunit;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using AI_Writing_Assistant.Services;
using AI_Writing_Assistant.Models;
using AI_Writing_Assistant.Forms;

public class BaseAiServiceTests
{
    // Minimal concrete implementation for testing
    private class TestAiService : BaseAiService
    {
        public string LastText = "";
        public bool ThrowOnApi = false;
        public string SuggestionResponse = "";
        public string TranslationResponse = "";

        public TestAiService(SettingsService settingsService) : base(settingsService) { }

        protected override async Task<string> CallWritingSuggestionApiAsync(string text)
        {
            LastText = text;
            if (ThrowOnApi) throw new System.Exception("API error");
            await Task.Yield();
            return SuggestionResponse;
        }

        protected override async Task<string> CallTranslationApiAsync(string text)
        {
            LastText = text;
            if (ThrowOnApi) throw new System.Exception("API error");
            await Task.Yield();
            return TranslationResponse;
        }

        protected override List<WritingSuggestion> ParseSuggestionResponse(string response)
        {
            return new List<WritingSuggestion> { new WritingSuggestion { Type = "Test", ImprovedText = "Improved", Reason = "Reason" } };
        }

        protected override string ParseTranslationResponse(string response)
        {
            return "Translated";
        }
    }

    [Fact]
    public async Task GetWritingSuggestions_ReturnsDefault_WhenApiKeyMissing()
    {
        var mockSettings = new Mock<SettingsService>();
        mockSettings.Setup(s => s.GetApiKey()).Returns("");
        var service = new TestAiService(mockSettings.Object);

        var result = await service.GetWritingSuggestions("input");
        Assert.Single(result);
        Assert.Equal("Style", result[0].Type);
        Assert.Equal("input", result[0].ImprovedText);
    }

    [Fact]
    public async Task GetWritingSuggestions_ReturnsDefault_WhenApiKeyIsDefault()
    {
        var mockSettings = new Mock<SettingsService>();
        mockSettings.Setup(s => s.GetApiKey()).Returns("your-api-key");
        var service = new TestAiService(mockSettings.Object);

        var result = await service.GetWritingSuggestions("input");
        Assert.Single(result);
        Assert.Equal("Style", result[0].Type);
        Assert.Equal("input", result[0].ImprovedText);
    }

    [Fact]
    public async Task GetWritingSuggestions_CallsApiAndParsesResponse_WhenApiKeyValid()
    {
        var mockSettings = new Mock<SettingsService>();
        mockSettings.Setup(s => s.GetApiKey()).Returns("valid-key");
        var service = new TestAiService(mockSettings.Object)
        {
            SuggestionResponse = "api-response"
        };

        var result = await service.GetWritingSuggestions("input");
        Assert.Single(result);
        Assert.Equal("Improved", result[0].ImprovedText);
        Assert.Equal("input", service.LastText);
    }

    [Fact]
    public async Task GetWritingSuggestions_ReturnsDefault_OnApiException()
    {
        var mockSettings = new Mock<SettingsService>();
        mockSettings.Setup(s => s.GetApiKey()).Returns("valid-key");
        var service = new TestAiService(mockSettings.Object)
        {
            ThrowOnApi = true
        };

        var result = await service.GetWritingSuggestions("input");
        Assert.Single(result);
        Assert.Equal("Style", result[0].Type);
        Assert.Equal("input", result[0].ImprovedText);
    }

    [Fact]
    public async Task TranslateToVietnameseAsync_ReturnsError_WhenApiKeyMissing()
    {
        var mockSettings = new Mock<SettingsService>();
        mockSettings.Setup(s => s.GetApiKey()).Returns("");
        var service = new TestAiService(mockSettings.Object);

        var result = await service.TranslateToVietnameseAsync("input");
        Assert.Equal("API key not configured.", result);
    }

    [Fact]
    public async Task TranslateToVietnameseAsync_ReturnsError_WhenApiKeyIsDefault()
    {
        var mockSettings = new Mock<SettingsService>();
        mockSettings.Setup(s => s.GetApiKey()).Returns("your-api-key");
        var service = new TestAiService(mockSettings.Object);

        var result = await service.TranslateToVietnameseAsync("input");
        Assert.Equal("API key not configured.", result);
    }

    [Fact]
    public async Task TranslateToVietnameseAsync_CallsApiAndParsesResponse_WhenApiKeyValid()
    {
        var mockSettings = new Mock<SettingsService>();
        mockSettings.Setup(s => s.GetApiKey()).Returns("valid-key");
        var service = new TestAiService(mockSettings.Object)
        {
            TranslationResponse = "api-response"
        };

        var result = await service.TranslateToVietnameseAsync("input");
        Assert.Equal("Translated", result);
        Assert.Equal("input", service.LastText);
    }

    [Fact]
    public async Task TranslateToVietnameseAsync_ReturnsError_OnApiException()
    {
        var mockSettings = new Mock<SettingsService>();
        mockSettings.Setup(s => s.GetApiKey()).Returns("valid-key");
        var service = new TestAiService(mockSettings.Object)
        {
            ThrowOnApi = true
        };

        var result = await service.TranslateToVietnameseAsync("input");
        Assert.Equal("Error during translation.", result);
    }
}

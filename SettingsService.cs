using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AI_Writing_Assistant
{
    public enum CompletionMode
    {
        Select,
        Auto
    }

    public class AppSettings
    {
        public string ApiKey { get; set; }
        public CompletionMode CompletionMode { get; set; } = CompletionMode.Select;
    }

    public class SettingsService
    {
        private readonly string settingsFilePath;
        private AppSettings settings;

        public SettingsService()
        {
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string appFolderPath = Path.Combine(appDataPath, "AI-Writing-Assistant");
            Directory.CreateDirectory(appFolderPath);
            settingsFilePath = Path.Combine(appFolderPath, "settings.json");

            LoadSettings();
        }

        public string GetApiKey()
        {
            return settings?.ApiKey;
        }

        public CompletionMode GetCompletionMode()
        {
            return settings?.CompletionMode ?? CompletionMode.Select;
        }

        public void SaveAllSettings(string apiKey, CompletionMode mode)
        {
            settings.ApiKey = apiKey;
            settings.CompletionMode = mode;
            SaveSettings();
        }

        private void SaveSettings()
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    Converters = { new JsonStringEnumConverter() }
                };
                string json = JsonSerializer.Serialize(settings, options);
                File.WriteAllText(settingsFilePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving settings: {ex.Message}");
            }
        }

        private void LoadSettings()
        {
            try
            {
                if (File.Exists(settingsFilePath))
                {
                    string json = File.ReadAllText(settingsFilePath);
                    var options = new JsonSerializerOptions
                    {
                        Converters = { new JsonStringEnumConverter() }
                    };
                    settings = JsonSerializer.Deserialize<AppSettings>(json, options);
                }
                else
                {
                    settings = new AppSettings();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading settings: {ex.Message}");
                settings = new AppSettings();
            }
        }
    }
}

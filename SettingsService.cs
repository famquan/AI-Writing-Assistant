using System.Text.Json;

namespace AI_Writing_Assistant
{
    public class AppSettings
    {
        public string ApiKey { get; set; }
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

        public void SaveApiKey(string apiKey)
        {
            settings.ApiKey = apiKey;
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
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
                    settings = JsonSerializer.Deserialize<AppSettings>(json);
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

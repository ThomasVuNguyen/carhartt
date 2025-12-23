using System;
using System.IO;
using System.Text.Json;

namespace Carhartt.Core
{
    public class BrowserConfig
    {
        public string DefaultHomepage { get; set; } = "https://www.google.com";
        public string UserAgent { get; set; } = "";
    }

    public static class ConfigManager
    {
        public static BrowserConfig Load(string profilePath)
        {
            try
            {
                string path = Path.Combine(profilePath, "config.json");
                if (File.Exists(path))
                {
                    string json = File.ReadAllText(path);
                    var config = JsonSerializer.Deserialize<BrowserConfig>(json);
                    return config ?? new BrowserConfig();
                }
            }
            catch
            {
                // Fallback to default on error
            }
            return new BrowserConfig();
        }

        public static void Save(string profilePath, BrowserConfig config)
        {
            try
            {
                string path = Path.Combine(profilePath, "config.json");
                string json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(path, json);
            }
            catch
            {
                // Ignore save errors
            }
        }
    }
}

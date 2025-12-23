using System;
using System.IO;
using System.Windows;
using Carhartt.Core;

namespace Carhartt.App
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            string profile = "Default";
            string? cliUrl = null;

            for (int i = 0; i < e.Args.Length; i++)
            {
                if (e.Args[i] == "--profile" && i + 1 < e.Args.Length)
                {
                    profile = e.Args[i + 1];
                    i++;
                }
                else if (!e.Args[i].StartsWith("-"))
                {
                    cliUrl = e.Args[i];
                }
            }

            string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string appDataDir = Path.Combine(localAppData, "Carhartt");
            string userDataDir = Path.Combine(appDataDir, "Profiles", profile);
            string logDir = Path.Combine(appDataDir, "Logs");

            // Ensure profile directory exists for config
            Directory.CreateDirectory(userDataDir);

            // Initialize Logging
            Logger.Initialize(logDir);

            // Load Config
            BrowserConfig config = ConfigManager.Load(userDataDir);
            
            // Determine initial URL
            string url = cliUrl ?? config.DefaultHomepage;

            Logger.Log($"App Startup. Profile: {profile}, URL: {url}");

            var mainWindow = new MainWindow(userDataDir, url);
            mainWindow.Show();
        }
    }
}

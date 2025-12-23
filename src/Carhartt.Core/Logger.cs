using System;
using System.IO;

namespace Carhartt.Core
{
    public static class Logger
    {
        private static string? _logFilePath;
        private static readonly object _lock = new object();

        public static void Initialize(string logDirectory)
        {
            if (!Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }
            string fileName = $"{DateTime.Now:yyyy-MM-dd}.log";
            _logFilePath = Path.Combine(logDirectory, fileName);
        }

        public static void Log(string message)
        {
            if (_logFilePath == null) return;

            lock (_lock)
            {
                try
                {
                    string entry = $"[{DateTime.Now:HH:mm:ss}] {message}{Environment.NewLine}";
                    File.AppendAllText(_logFilePath, entry);
                }
                catch
                {
                    // Swallow logging errors to prevent crash loop
                }
            }
        }
    }
}

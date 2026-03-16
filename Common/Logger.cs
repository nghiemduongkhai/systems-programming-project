using System;
using System.IO;

namespace InventoryKPI.Common
{
    public static class Logger
    {
        private static readonly object _lock = new object();
        private static readonly string _logFolder = @"../../../InventoryData/logs";

        public static void Info(string message)
        {
            Write("INFO", message);
        }

        public static void Warning(string message)
        {
            Write("WARNING", message);
        }

        public static void Error(string message)
        {
            Write("ERROR", message);
        }

        private static void Write(string level, string message)
        {
            try
            {
                string logLine = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{level}] {message}";

                Console.WriteLine(logLine);

                lock (_lock)
                {
                    if (!Directory.Exists(_logFolder))
                        Directory.CreateDirectory(_logFolder);

                    string filePath = Path.Combine(_logFolder, $"log_{DateTime.Now:yyyyMMdd}.txt");

                    File.AppendAllText(filePath, logLine + Environment.NewLine);
                }
            }
            catch
            {
                // tránh crash hệ thống
            }
        }
    }
}
using System;
using System.IO;
using System.Text;

namespace ScanwordGenerator
{
    public static class PerformanceLogger
    {
        // Додано параметр totalTimeMs
        public static void LogGeneration(int width, int height, bool useImages, long genTimeMs, long totalTimeMs)
        {
            try
            {
                string fileName = $"performance_log_{DateTime.Now:yyyy-MM-dd}.csv";
                string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);
                bool fileExists = File.Exists(filePath);

                using (StreamWriter sw = new StreamWriter(filePath, true, Encoding.UTF8))
                {
                    if (!fileExists)
                    {
                        // Оновлений заголовок CSV
                        sw.WriteLine("Timestamp;Width;Height;Images;GenTime_ms;TotalTime_ms;GenTime_sec;TotalTime_sec");
                    }

                    double genSec = genTimeMs / 1000.0;
                    double totalSec = totalTimeMs / 1000.0;

                    // Записуємо два показники часу
                    string logLine = $"{DateTime.Now:HH:mm:ss};{width};{height};{useImages};{genTimeMs};{totalTimeMs};{genSec:F2};{totalSec:F2}";

                    sw.WriteLine(logLine);
                }
            }
            catch (Exception)
            {
                // Ігноруємо помилки логування
            }
        }
    }
}
//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.IO;
//using System.Text;
//using System.Windows.Forms;

//namespace ScanwordGenerator
//{
//    public static class BenchmarkRunner
//    {
//        public static void RunFullTest(ScanwordService service, List<WordData> words)
//        {
//            // 1. Параметри для тестування
//            // Які розміри тестуємо (Ширина, Висота)
//            var sizes = new List<(int w, int h)>
//            {
//                (15, 15), // Малий
//                (20, 20), // Середній
//                (30, 25)  // Великий
//            };

//            // Скільки спроб (ітерацій) робити в алгоритмі
//            var attemptsOptions = new List<int> { 5, 15, 30, 50, 100 };

//            // Варіанти з картинками і без
//            var imageOptions = new List<bool> { false, true };

//            // 2. Підготовка файлу
//            string fileName = $"BENCHMARK_{DateTime.Now:HH-mm-ss}.csv";
//            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);

//            StringBuilder csv = new StringBuilder();
//            csv.AppendLine("Size;CellsCount;UseImages;Attempts;Time_ms;Time_sec;Density_Percent;WordsCount");

//            // 3. Запуск циклів (Глибоке тестування)
//            // Проходимо по всіх комбінаціях
//            foreach (var size in sizes)
//            {
//                foreach (var useImages in imageOptions)
//                {
//                    foreach (var attempts in attemptsOptions)
//                    {
//                        // Щоб дані були точнішими, можна проганяти кожен тест 3 рази і брати середнє,
//                        // але для курсової вистачить і 1 разу.

//                        Stopwatch sw = Stopwatch.StartNew();

//                        // Викликаємо генерацію
//                        Cell[,] resultGrid = service.GenerateBestGrid(size.w, size.h, useImages, words, attempts);

//                        sw.Stop();

//                        // Збираємо статистику
//                        double density = 0;
//                        int wordsCount = 0; // Приблизно (кількість літер)

//                        if (resultGrid != null)
//                        {
//                            density = CalculateDensity(resultGrid);
//                            wordsCount = CountFilledCells(resultGrid); // Це кількість заповнених клітинок
//                        }

//                        // Формуємо рядок звіту
//                        string line = $"{size.w}x{size.h};{size.w * size.h};{useImages};{attempts};{sw.ElapsedMilliseconds};{sw.Elapsed.TotalSeconds:F2};{density:F2};{wordsCount}";

//                        csv.AppendLine(line);
//                    }
//                }
//            }

//            // 4. Збереження
//            File.WriteAllText(filePath, csv.ToString(), Encoding.UTF8);

//            MessageBox.Show($"Тестування завершено!\nЗвіт збережено у файл:\n{fileName}\n\nВідкрийте його в Excel.");
//        }

//        // Метод для розрахунку щільності (дублюємо логіку з алгоритму, щоб не лазити в приватні поля)
//        private static double CalculateDensity(Cell[,] grid)
//        {
//            int width = grid.GetLength(1);
//            int height = grid.GetLength(0);
//            int filled = 0;

//            for (int y = 0; y < height; y++)
//            {
//                for (int x = 0; x < width; x++)
//                {
//                    if (grid[y, x].Type != CellType.Empty)
//                    {
//                        filled++;
//                    }
//                }
//            }

//            return (double)filled / (width * height) * 100.0;
//        }

//        private static int CountFilledCells(Cell[,] grid)
//        {
//            int count = 0;
//            foreach (var cell in grid)
//            {
//                if (cell.Type != CellType.Empty) count++;
//            }
//            return count;
//        }
//    }
//}
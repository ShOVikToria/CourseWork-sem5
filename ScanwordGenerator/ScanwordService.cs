using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace ScanwordGenerator
{
    public class ScanwordService
    {
        private List<WordData> _allWords;

        public bool IsDictionaryLoaded => _allWords != null && _allWords.Any();

        public void LoadDictionary(string fileName)
        {
            if (File.Exists(fileName))
            {
                string json = File.ReadAllText(fileName);
                _allWords = JsonSerializer.Deserialize<List<WordData>>(json);
            }
            else
            {
                throw new FileNotFoundException($"Словник {fileName} не знайдено.");
            }
        }

        // НОВИЙ МЕТОД: Повертає слова тільки для конкретної теми
        public List<WordData> GetWordsByTheme(string theme)
        {
            if (_allWords == null) return new List<WordData>();

            // Порівнюємо без урахування регістру (щоб "Кіно" знайшло "кіно")
            return _allWords
                .Where(w => w.Theme.Trim().Equals(theme.Trim(), StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        // ОНОВЛЕНИЙ МЕТОД: Тепер приймає список слів (wordsToUse) як аргумент
        // Додано параметр string imagePrefix
        public Cell[,] GenerateBestGrid(int width, int height, bool useImages, List<WordData> wordsToUse, string imagePrefix, int attempts = 30)
        {
            if (wordsToUse == null || !wordsToUse.Any()) return null;

            var validWords = wordsToUse.Where(w => w.Length < width && w.Length < height).ToList();

            // Передаємо префікс у конструктор
            var generator = new ScanwordAlgorithm(width, height, imagePrefix);

            double bestScore = -1;
            Cell[,] bestGrid = null;

            for (int i = 0; i < attempts; i++)
            {
                generator.Generate(validWords, useImages);
                double score = generator.CalculateFillPercentage();

                if (score > bestScore)
                {
                    bestScore = score;
                    bestGrid = generator.GetGridClone();
                }
            }
            return bestGrid;
        }
    }
}
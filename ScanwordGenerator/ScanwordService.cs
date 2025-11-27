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

        // У методі GenerateBestGrid додайте параметр:
        public Cell[,] GenerateBestGrid(int width, int height, bool useImages, int attempts = 50)
        {
            if (!IsDictionaryLoaded) return null;

            var validWords = _allWords.Where(w => w.Term.Length < width && w.Term.Length < height).ToList();

            var generator = new ScanwordAlgorithm(width, height);
            double bestScore = -1;
            Cell[,] bestGrid = null;

            for (int i = 0; i < attempts; i++)
            {
                // ПЕРЕДАЄМО useImages СЮДИ
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
using System;
using System.Collections.Generic;
using System.Linq;

namespace ScanwordGenerator
{
    public class ScanwordAlgorithm // <--- ЗМІНЕНО ІМ'Я КЛАСУ
    {
        private int _width;
        private int _height;
        public Cell[,] Grid { get; private set; }
        private Random _rng = new Random();
        private Dictionary<char, List<WordData>> _wordIndex;

        public ScanwordAlgorithm(int width, int height)
        {
            _width = width;
            _height = height;
            Grid = new Cell[height, width];
            InitializeGrid();
        }

        private void InitializeGrid()
        {
            for (int y = 0; y < _height; y++)
                for (int x = 0; x < _width; x++)
                    Grid[y, x] = new Cell();
        }

        // --- Основні методи генерації (з вашого старого коду) ---

        public int Generate(List<WordData> allWords)
        {
            if (_wordIndex == null) BuildIndex(allWords);
            InitializeGrid();

            var usedWords = new HashSet<string>();

            // Скелет: Перше слово
            var startWords = allWords.Where(w => w.Term.Length >= 6).ToList();
            if (startWords.Count == 0) startWords = allWords;
            if (startWords.Count == 0) return 0;

            var firstWord = startWords[_rng.Next(startWords.Count)];
            int totalLen = firstWord.Term.Length + 1;
            int startX = Math.Max(0, (_width - totalLen) / 2);
            int startY = _height / 2;

            PlaceWord(firstWord, startX, startY, true);
            usedWords.Add(firstWord.Term);

            int placedCount = 1;
            var activeAnchors = new List<(int x, int y)>();
            AddAnchorsFromWord(firstWord, startX, startY, true, activeAnchors);

            bool stuck = false;
            while (!stuck)
            {
                stuck = true;
                int anchorsToCheck = activeAnchors.Count;
                for (int k = 0; k < anchorsToCheck; k++)
                {
                    if (activeAnchors.Count == 0) break;
                    int idx = _rng.Next(activeAnchors.Count);
                    var anchor = activeAnchors[idx];

                    if (TryAttachBestWordToAnchor(anchor.x, anchor.y, usedWords, activeAnchors))
                    {
                        placedCount++;
                        stuck = false;
                    }
                }
            }
            return placedCount;
        }

        // --- Допоміжні методи ---

        private void BuildIndex(List<WordData> allWords)
        {
            _wordIndex = new Dictionary<char, List<WordData>>();
            foreach (var word in allWords)
            {
                foreach (char c in word.Term.Distinct())
                {
                    if (!_wordIndex.ContainsKey(c)) _wordIndex[c] = new List<WordData>();
                    _wordIndex[c].Add(word);
                }
            }
        }

        private void AddAnchorsFromWord(WordData word, int startX, int startY, bool isHor, List<(int x, int y)> anchors)
        {
            for (int i = 0; i < word.Term.Length; i++)
            {
                int x = isHor ? startX + 1 + i : startX;
                int y = isHor ? startY : startY + 1 + i;
                anchors.Add((x, y));
            }
        }

        private bool TryAttachBestWordToAnchor(int x, int y, HashSet<string> usedWords, List<(int x, int y)> anchors)
        {
            char anchorChar = Grid[y, x].Letter;
            if (_wordIndex == null || !_wordIndex.ContainsKey(anchorChar)) return false;

            var candidates = _wordIndex[anchorChar];
            bool anchorIsHor = CheckIfCellIsPartHor(x, y);
            bool tryHor = !anchorIsHor;

            var validMoves = new List<(WordData word, int startX, int startY, int score)>();
            int sampleSize = 50;
            int count = 0;
            int startIdx = _rng.Next(candidates.Count);

            for (int k = 0; k < candidates.Count; k++)
            {
                if (count >= sampleSize) break;
                var word = candidates[(startIdx + k) % candidates.Count];
                if (usedWords.Contains(word.Term)) continue;

                for (int i = 0; i < word.Term.Length; i++)
                {
                    if (word.Term[i] == anchorChar)
                    {
                        int startX = tryHor ? x - (i + 1) : x;
                        int startY = tryHor ? y : y - (i + 1);

                        if (CanPlaceSafe(word.Term, startX, startY, tryHor, out int intersections))
                        {
                            int score = (word.Term.Length * 2) + (intersections * 10);
                            validMoves.Add((word, startX, startY, score));
                        }
                    }
                }
                count++;
            }

            if (validMoves.Count == 0) return false;

            var bestMove = validMoves.OrderByDescending(m => m.score).ThenBy(m => _rng.Next()).First();
            PlaceWord(bestMove.word, bestMove.startX, bestMove.startY, tryHor);
            usedWords.Add(bestMove.word.Term);
            AddAnchorsFromWord(bestMove.word, bestMove.startX, bestMove.startY, tryHor, anchors);
            return true;
        }

        private bool CheckIfCellIsPartHor(int x, int y)
        {
            if (x > 0 && Grid[y, x - 1].Type == CellType.Letter) return true;
            if (x < _width - 1 && Grid[y, x + 1].Type == CellType.Letter) return true;
            return false;
        }

        private bool CanPlaceSafe(string term, int startX, int startY, bool isHor, out int intersections)
        {
            intersections = 0;
            int totalLen = term.Length + 1;
            if (startX < 0 || startY < 0) return false;
            if (isHor && startX + totalLen > _width) return false;
            if (!isHor && startY + totalLen > _height) return false;

            if (isHor) { if (startX + totalLen < _width && Grid[startY, startX + totalLen].Type == CellType.Letter) return false; }
            else { if (startY + totalLen < _height && Grid[startY + totalLen, startX].Type == CellType.Letter) return false; }

            for (int k = 0; k < totalLen; k++)
            {
                int cx = isHor ? startX + k : startX;
                int cy = isHor ? startY : startY + k;
                var cell = Grid[cy, cx];

                if (k == 0) // Питання
                {
                    if (cell.Type != CellType.Empty) return false;
                }
                else // Літера
                {
                    char charToPlace = term[k - 1];
                    if (cell.Type != CellType.Empty)
                    {
                        if (cell.Type != CellType.Letter) return false;
                        if (cell.Letter != charToPlace) return false;
                        intersections++;
                    }
                    else
                    {
                        if (HasSideConflict(cx, cy, isHor)) return false;
                    }
                }
            }
            return true;
        }

        private bool HasSideConflict(int x, int y, bool isHor)
        {
            if (isHor)
            {
                if (y > 0 && Grid[y - 1, x].Type == CellType.Letter) return true;
                if (y < _height - 1 && Grid[y + 1, x].Type == CellType.Letter) return true;
            }
            else
            {
                if (x > 0 && Grid[y, x - 1].Type == CellType.Letter) return true;
                if (x < _width - 1 && Grid[y, x + 1].Type == CellType.Letter) return true;
            }
            return false;
        }

        private void PlaceWord(WordData word, int startX, int startY, bool isHor)
        {
            var defCell = Grid[startY, startX];
            defCell.Type = CellType.Definition;
            defCell.DefinitionText = word.GetRandomQuestion();
            defCell.ArrowDirection = isHor ? "->" : "v";

            for (int i = 0; i < word.Term.Length; i++)
            {
                int lx = isHor ? startX + 1 + i : startX;
                int ly = isHor ? startY : startY + 1 + i;
                Grid[ly, lx].Type = CellType.Letter;
                Grid[ly, lx].Letter = word.Term[i];
            }
        }

        public double CalculateFillPercentage()
        {
            int filled = 0;
            for (int y = 0; y < _height; y++) for (int x = 0; x < _width; x++) if (Grid[y, x].Type != CellType.Empty) filled++;
            return (double)filled / (_width * _height) * 100.0;
        }

        public Cell[,] GetGridClone()
        {
            Cell[,] clone = new Cell[_height, _width];
            for (int y = 0; y < _height; y++) for (int x = 0; x < _width; x++)
                    clone[y, x] = new Cell { Type = Grid[y, x].Type, Letter = Grid[y, x].Letter, DefinitionText = Grid[y, x].DefinitionText, ArrowDirection = Grid[y, x].ArrowDirection };
            return clone;
        }

        public void RestoreGrid(Cell[,] state) { Grid = state; }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;

namespace ScanwordGenerator
{
    public class ScanwordAlgorithm
    {
        private int _width;
        private int _height;
        public Cell[,] Grid { get; private set; }
        private Random _rng = new Random();
        private Dictionary<char, List<WordData>> _wordIndex;

        // Лічильник встановлених картинок
        public int PlacedImagesCount { get; private set; }

        // Цільова кількість картинок
        private int _targetImageCount;

        // Діагностика
        public bool DictionaryHasImages { get; private set; }

        private string _imagePrefix;
        
        public ScanwordAlgorithm(int width, int height, string imagePrefix)
        {
            _width = width;
            _height = height;
            _imagePrefix = imagePrefix; // Зберігаємо префікс (напр. "animals", "cinema")
            Grid = new Cell[height, width];
            InitializeGrid();
        }

        private void InitializeGrid()
        {
            for (int y = 0; y < _height; y++)
                for (int x = 0; x < _width; x++)
                    Grid[y, x] = new Cell();
        }

        private void BuildIndex(List<WordData> allWords)
        {
            _wordIndex = new Dictionary<char, List<WordData>>();
            foreach (var word in allWords)
            {
                if (word.Images != null && word.Images.HasAny) DictionaryHasImages = true;

                foreach (char c in word.Term.Distinct())
                {
                    if (!_wordIndex.ContainsKey(c)) _wordIndex[c] = new List<WordData>();
                    _wordIndex[c].Add(word);
                }
            }
        }

        // --- ГОЛОВНИЙ МЕТОД ГЕНЕРАЦІЇ ---
        public int Generate(List<WordData> allWords, bool useImages)
        {
            if (_wordIndex == null) BuildIndex(allWords);
            InitializeGrid();
            PlacedImagesCount = 0;

            // Визначення цільової кількості картинок
            if (useImages)
            {
                int maxDim = Math.Max(_width, _height);
                if (maxDim <= 15) _targetImageCount = _rng.Next(1, 3);
                else if (maxDim <= 25) _targetImageCount = _rng.Next(3, 5);
                else _targetImageCount = _rng.Next(5, 9);
            }
            else
            {
                _targetImageCount = 0;
            }

            var usedWords = new HashSet<string>();

            WordData firstWord = null;
            string firstImagePath = null;
            int defW = 1, defH = 1;

            // Випадковий напрямок для першого слова
            bool firstIsHor = _rng.Next(2) == 0;

            // 1. ВИБІР ПЕРШОГО СЛОВА
            if (useImages)
            {
                var imageWords = allWords.Where(w => w.Images != null && w.Images.HasAny && w.Term.Length >= 3).ToList();

                if (imageWords.Any())
                {
                    firstWord = imageWords[_rng.Next(imageWords.Count)];

                    var validFormats = GetValidImageFormats(firstWord);
                    if (validFormats.Count > 0)
                    {
                        var chosen = validFormats[_rng.Next(validFormats.Count)];
                        firstImagePath = chosen.path;
                        defW = chosen.w;
                        defH = chosen.h;
                    }
                }
            }

            if (firstWord == null)
            {
                var startWords = allWords.Where(w => w.Term.Length >= 5).ToList();
                if (startWords.Count == 0) startWords = allWords;
                if (startWords.Count == 0) return 0;

                firstWord = startWords[_rng.Next(startWords.Count)];
            }

            // 2. РОЗРАХУНОК ПОЗИЦІЇ (З ВИПРАВЛЕННЯМ ПОМИЛКИ ІНДЕКСУ)
            int startX, startY, defX, defY;

            if (firstIsHor)
            {
                int requiredWidth = firstWord.Term.Length + defW;
                startX = Math.Max(defW, (_width - requiredWidth) / 2);
                startY = _height / 2;

                // FIX: Перевірка правого краю (щоб слово не вилізло за ширину)
                if (startX + firstWord.Term.Length > _width)
                {
                    startX = _width - firstWord.Term.Length;
                }

                defX = startX - defW;
                defY = startY - (defH - 1) / 2;
            }
            else
            {
                int requiredHeight = firstWord.Term.Length + defH;
                startX = _width / 2;
                startY = Math.Max(defH, (_height - requiredHeight) / 2);

                // FIX: Перевірка нижнього краю (щоб слово не вилізло за висоту)
                if (startY + firstWord.Term.Length > _height)
                {
                    startY = _height - firstWord.Term.Length;
                }

                defY = startY - defH;
                defX = startX - (defW - 1) / 2;
            }

            // Клампінг для блоку визначення (щоб картинка не вилізла за межі)
            if (defX < 0) defX = 0;
            if (defX + defW > _width) defX = _width - defW;
            if (defY < 0) defY = 0;
            if (defY + defH > _height) defY = _height - defH;

            // Додаткова перевірка безпеки перед записом (якщо слово все одно не лізе - перериваємо)
            if (firstIsHor && (startX < 0 || startX + firstWord.Term.Length > _width)) return 0;
            if (!firstIsHor && (startY < 0 || startY + firstWord.Term.Length > _height)) return 0;

            // 3. РОЗМІЩЕННЯ
            PlaceWord(firstWord, startX, startY, firstIsHor, defW, defH, firstImagePath, firstWord.GetRandomQuestion(), defX, defY);
            usedWords.Add(firstWord.Term);

            if (firstImagePath != null) PlacedImagesCount++;

            int placedCount = 1;
            var activeAnchors = new List<(int x, int y)>();
            AddAnchorsFromWord(firstWord, startX, startY, firstIsHor, activeAnchors);

            // 4. ЦИКЛ ГЕНЕРАЦІЇ
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

                    if (TryAttachBestWordToAnchor(anchor.x, anchor.y, usedWords, activeAnchors, useImages))
                    {
                        placedCount++;
                        stuck = false;
                    }
                }
            }
            return placedCount;
        }

        private List<(string path, int w, int h)> GetValidImageFormats(WordData word)
        {
            var list = new List<(string, int, int)>();

            // Замість "animals_s" використовуємо змінну _imagePrefix + "_s"
            if (!string.IsNullOrEmpty(word.Images.Square))
                list.Add(($"{_imagePrefix}_s/" + word.Images.Square, 2, 2));

            if (!string.IsNullOrEmpty(word.Images.Horizontal))
                list.Add(($"{_imagePrefix}_h/" + word.Images.Horizontal, 3, 2));

            if (!string.IsNullOrEmpty(word.Images.Vertical))
                list.Add(($"{_imagePrefix}_v/" + word.Images.Vertical, 2, 3));

            return list;
        }

        private bool TryAttachBestWordToAnchor(int x, int y, HashSet<string> usedWords, List<(int x, int y)> anchors, bool useImages)
        {
            char anchorChar = Grid[y, x].Letter;
            if (_wordIndex == null || !_wordIndex.ContainsKey(anchorChar)) return false;

            var candidates = _wordIndex[anchorChar];
            bool anchorIsHor = CheckIfCellIsPartHor(x, y);
            bool tryHor = !anchorIsHor;

            bool prioritizeImage = useImages && PlacedImagesCount < _targetImageCount;

            int startIdx = _rng.Next(candidates.Count);

            for (int k = 0; k < candidates.Count; k++)
            {
                var word = candidates[(startIdx + k) % candidates.Count];
                if (usedWords.Contains(word.Term)) continue;
                if (word.Questions == null || !word.Questions.Any()) continue;

                for (int i = 0; i < word.Term.Length; i++)
                {
                    if (word.Term[i] == anchorChar)
                    {
                        if (prioritizeImage && word.Images != null && word.Images.HasAny)
                        {
                            var formats = GetValidImageFormats(word);
                            formats = formats.OrderBy(a => _rng.Next()).ToList();

                            bool fit = false;
                            foreach (var fmt in formats)
                            {
                                if (TryFitShape(word, x, y, i, tryHor, fmt.w, fmt.h, fmt.path, usedWords, anchors))
                                {
                                    fit = true;
                                    break;
                                }
                            }

                            if (fit) return true;
                            continue;
                        }

                        if (!prioritizeImage || (word.Images == null || !word.Images.HasAny))
                        {
                            if (TryFitShape(word, x, y, i, tryHor, 1, 1, null, usedWords, anchors)) return true;
                        }
                    }
                }
            }
            return false;
        }

        private bool TryFitShape(WordData word, int anchorX, int anchorY, int charIndex, bool isHor,
                                 int defW, int defH, string imagePath,
                                 HashSet<string> usedWords, List<(int x, int y)> anchors)
        {
            int wordStartX, wordStartY;
            int defStartX, defStartY;

            if (isHor)
            {
                wordStartX = anchorX - charIndex;
                wordStartY = anchorY;
                defStartX = wordStartX - defW;
                defStartY = wordStartY - (defH - 1) / 2;

                if (defStartX < 0) return false;
                if (defStartY < 0) defStartY = 0;
                if (defStartY + defH > _height) defStartY = _height - defH;
            }
            else
            {
                wordStartX = anchorX;
                wordStartY = anchorY - charIndex;
                defStartY = wordStartY - defH;
                defStartX = wordStartX - (defW - 1) / 2;

                if (defStartY < 0) return false;
                if (defStartX < 0) defStartX = 0;
                if (defStartX + defW > _width) defStartX = _width - defW;
            }

            if (!CanPlaceWordLetters(word.Term, wordStartX, wordStartY, isHor, out int intersections)) return false;
            if (!IsAreaFree(defStartX, defStartY, defW, defH)) return false;

            PlaceWord(word, wordStartX, wordStartY, isHor, defW, defH, imagePath, word.GetRandomQuestion(), defStartX, defStartY);

            usedWords.Add(word.Term);
            if (imagePath != null) PlacedImagesCount++;
            AddAnchorsFromWord(word, wordStartX, wordStartY, isHor, anchors);
            return true;
        }

        private void AddAnchorsFromWord(WordData word, int startX, int startY, bool isHor, List<(int x, int y)> anchors)
        {
            for (int i = 0; i < word.Term.Length; i++)
            {
                int x = isHor ? startX + i : startX;
                int y = isHor ? startY : startY + i;
                if (!anchors.Contains((x, y))) anchors.Add((x, y));
            }
        }

        private bool CheckIfCellIsPartHor(int x, int y)
        {
            if (x > 0 && Grid[y, x - 1].Type == CellType.Letter) return true;
            if (x < _width - 1 && Grid[y, x + 1].Type == CellType.Letter) return true;
            return false;
        }

        private bool IsAreaFree(int startX, int startY, int w, int h)
        {
            if (startX < 0 || startY < 0 || startX + w > _width || startY + h > _height) return false;
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                    if (Grid[startY + y, startX + x].Type != CellType.Empty) return false;
            return true;
        }

        private bool CanPlaceWordLetters(string term, int startX, int startY, bool isHor, out int intersections)
        {
            intersections = 0;
            if (startX < 0 || startY < 0) return false;
            if (isHor && startX + term.Length > _width) return false;
            if (!isHor && startY + term.Length > _height) return false;

            if (isHor) { if (startX + term.Length < _width && Grid[startY, startX + term.Length].Type == CellType.Letter) return false; }
            else { if (startY + term.Length < _height && Grid[startY + term.Length, startX].Type == CellType.Letter) return false; }

            for (int k = 0; k < term.Length; k++)
            {
                int cx = isHor ? startX + k : startX;
                int cy = isHor ? startY : startY + k;
                var cell = Grid[cy, cx];

                if (cell.Type != CellType.Empty)
                {
                    if (cell.Type != CellType.Letter) return false;
                    if (cell.Letter != term[k]) return false;
                    intersections++;
                }
                else
                {
                    if (HasSideConflict(cx, cy, isHor)) return false;
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

        // ОНОВЛЕНИЙ МЕТОД PlaceWord
        private void PlaceWord(WordData word, int wordX, int wordY, bool isHor, int defW, int defH, string imagePath, string question, int defX, int defY)
        {
            // Розраховуємо зміщення стрілки
            // Якщо слово горизонтальне: різниця між Y слова і Y картинки
            // Якщо вертикальне: різниця між X слова і X картинки
            int arrowOffset = isHor ? (wordY - defY) : (wordX - defX);

            // 1. Ставимо блок визначення
            for (int dy = 0; dy < defH; dy++)
            {
                for (int dx = 0; dx < defW; dx++)
                {
                    var cell = Grid[defY + dy, defX + dx];
                    if (imagePath != null)
                    {
                        cell.Type = CellType.Picture;
                        cell.ImagePath = imagePath;
                        cell.ImageWidthCells = defW;
                        cell.ImageHeightCells = defH;
                        cell.IsPictureMainCell = (dx == 0 && dy == 0);

                        // Зберігаємо точне зміщення для стрілки
                        cell.ArrowOffset = arrowOffset;
                    }
                    else
                    {
                        cell.Type = CellType.Definition;
                        cell.DefinitionText = question;
                    }

                    if (isHor) cell.ArrowDirection = "->";
                    else cell.ArrowDirection = "v";
                }
            }

            // 2. Ставимо літери (без змін)
            for (int i = 0; i < word.Length; i++)
            {
                int lx = isHor ? wordX + i : wordX;
                int ly = isHor ? wordY : wordY + i;
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
            for (int y = 0; y < _height; y++)
                for (int x = 0; x < _width; x++)
                {
                    var src = Grid[y, x];
                    clone[y, x] = new Cell
                    {
                        Type = src.Type,
                        Letter = src.Letter,
                        DefinitionText = src.DefinitionText,
                        ArrowDirection = src.ArrowDirection,
                        ImagePath = src.ImagePath,
                        ImageWidthCells = src.ImageWidthCells,
                        ImageHeightCells = src.ImageHeightCells,
                        IsPictureMainCell = src.IsPictureMainCell
                    };
                }
            return clone;
        }

        public void RestoreGrid(Cell[,] state) { Grid = state; }
    }
}
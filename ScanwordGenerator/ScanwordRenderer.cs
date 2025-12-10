using System.Drawing.Drawing2D;

namespace ScanwordGenerator
{
    public class ScanwordRenderer
    {
        private readonly Brush _brushText = Brushes.Black;
        private readonly Brush _brushGray = new SolidBrush(Color.FromArgb(220, 220, 220));
        private readonly Brush _brushArrow = Brushes.Black;
        private readonly Pen _penBorder = new Pen(Color.Gray, 1);

        public Cell Cell
        {
            get => default;
            set
            {
            }
        }

        public Bitmap DrawGrid(Cell[,] grid, int widthPx, int heightPx, bool showAnswers)
        {
            if (grid == null) return new Bitmap(1, 1);

            int gridW = grid.GetLength(1);
            int gridH = grid.GetLength(0);

            float cellW = (float)(widthPx - 10) / gridW;
            float cellH = (float)(heightPx - 10) / gridH;
            float cellSize = Math.Min(cellW, cellH);

            Bitmap bmp = new Bitmap(widthPx, heightPx);

            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
                g.Clear(Color.White);

                // --- ПРОХІД 1: ФОН ТА РАМКИ ---
                for (int y = 0; y < gridH; y++)
                    for (int x = 0; x < gridW; x++)
                        DrawCellBackground(g, grid[y, x], x, y, cellSize);

                // --- ПРОХІД 2: КОНТЕНТ (ТЕКСТ, КАРТИНКИ ТА СТРІЛКИ) ---
                using (Font fontLetter = new Font("Arial", cellSize * 0.5f, FontStyle.Regular))
                using (Font fontDef = new Font("Arial Narrow", cellSize * 0.22f, FontStyle.Regular))
                using (StringFormat sf = new StringFormat())
                {
                    sf.Alignment = StringAlignment.Center;
                    sf.LineAlignment = StringAlignment.Near;
                    sf.Trimming = StringTrimming.Word;

                    for (int y = 0; y < gridH; y++)
                        for (int x = 0; x < gridW; x++)
                            DrawCellContent(g, grid[y, x], x, y, cellSize, fontLetter, fontDef, showAnswers, sf);
                }
            }
            return bmp;
        }

        private void DrawCellBackground(Graphics g, Cell cell, int x, int y, float size)
        {
            float px = x * size + 5;
            float py = y * size + 5;
            RectangleF rect = new RectangleF(px, py, size, size);

            if (cell.Type == CellType.Empty)
                g.FillRectangle(_brushGray, rect);
            else
                g.FillRectangle(Brushes.White, rect);

            g.DrawRectangle(_penBorder, px, py, size, size);
        }

        private void DrawCellContent(Graphics g, Cell cell, int x, int y, float size, Font fLetter, Font fDef, bool showAnswers, StringFormat sf)
        {
            float px = x * size + 5;
            float py = y * size + 5;

            if (cell.Type == CellType.Letter && showAnswers)
            {
                RectangleF rect = new RectangleF(px, py, size, size);
                StringFormat sfLetter = new StringFormat(sf) { LineAlignment = StringAlignment.Center };
                g.DrawString(cell.Letter.ToString(), fLetter, _brushText, rect, sfLetter);
            }
            else if (cell.Type == CellType.Definition)
            {
                // ТЕКСТОВЕ ПИТАННЯ
                float padding = 2;
                RectangleF textRect = new RectangleF(px + padding, py + padding, size - (padding * 2), size - (padding * 2));

                if (!string.IsNullOrEmpty(cell.DefinitionText) && cell.DefinitionText != "No question")
                {
                    float maxFontSize = size * 0.22f;
                    float minFontSize = size * 0.12f;

                    using (Font fd = FitTextToRectangle(g, cell.DefinitionText, "Arial Narrow", textRect, maxFontSize, minFontSize))
                    {
                        g.DrawString(cell.DefinitionText, fd, _brushText, textRect, sf);
                    }
                    DrawArrowPointingToTarget(g, px, py, size, cell.ArrowDirection);
                }
            }
            else if (cell.Type == CellType.Picture && cell.IsPictureMainCell)
            {
                // КАРТИНКА
                float imgWidth = cell.ImageWidthCells * size;
                float imgHeight = cell.ImageHeightCells * size;
                RectangleF imgRect = new RectangleF(px, py, imgWidth, imgHeight);

                string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, cell.ImagePath);

                if (File.Exists(fullPath))
                {
                    try
                    {
                        using (Image img = Image.FromFile(fullPath))
                        {
                            g.DrawImage(img, imgRect);
                        }
                    }
                    catch
                    {
                        g.DrawString("Broken Img", fLetter, Brushes.Red, imgRect, sf);
                    }
                }
                else
                {
                    string fileName = Path.GetFileName(fullPath);
                    using (Font errFont = new Font("Arial", size * 0.12f))
                    {
                        g.DrawString($"Not Found:\n{relativePathDisplay(cell.ImagePath)}", errFont, Brushes.Red, imgRect, sf);
                    }
                }

                g.DrawRectangle(new Pen(Color.Gray, 2), px, py, imgWidth, imgHeight);

                // Малюємо стрілку
                DrawArrowForPicture(g, px, py, size, cell.ArrowDirection, cell.ImageWidthCells, cell.ImageHeightCells);
            }
        }

        // Допоміжний метод для скорочення шляху при помилці
        private string relativePathDisplay(string path)
        {
            try { return Path.GetFileName(Path.GetDirectoryName(path)) + "/" + Path.GetFileName(path); }
            catch { return path; }
        }

        private Font FitTextToRectangle(Graphics g, string text, string fontFamily, RectangleF rect, float maxSize, float minSize)
        {
            Font font = new Font(fontFamily, maxSize, FontStyle.Regular);
            while (font.Size > minSize)
            {
                SizeF size = g.MeasureString(text, font, (int)rect.Width);
                if (size.Height <= rect.Height && size.Width <= rect.Width) return font;

                float newSize = font.Size - 0.5f;
                font.Dispose();
                font = new Font(fontFamily, newSize, FontStyle.Regular);
            }
            return font;
        }

        // Стрілка для текстового питання (1х1)
        private void DrawArrowPointingToTarget(Graphics g, float defX, float defY, float size, string direction)
        {
            float arrowLen = size * 0.2f;
            PointF[] triangle;

            if (direction == "->")
            {
                float targetX = defX + size;
                float targetY = defY + size / 2;
                triangle = new PointF[] {
                    new PointF(targetX + arrowLen, targetY),
                    new PointF(targetX, targetY - arrowLen / 1.5f),
                    new PointF(targetX, targetY + arrowLen / 1.5f)
                };
            }
            else
            {
                float targetX = defX + size / 2;
                float targetY = defY + size;
                triangle = new PointF[] {
                    new PointF(targetX, targetY + arrowLen),
                    new PointF(targetX - arrowLen / 1.5f, targetY),
                    new PointF(targetX + arrowLen / 1.5f, targetY)
                };
            }
            g.FillPolygon(_brushArrow, triangle);
        }

        // Стрілка для КАРТИНКИ
        private void DrawArrowForPicture(Graphics g, float imgX, float imgY, float cellSize, string direction, int wCells, int hCells)
        {
            float arrowLen = cellSize * 0.2f; // Розмір стрілки
            PointF[] triangle;

            // Знаходимо центр рядка/колонки, з якої виходить слово
            int cellRowIndex = (hCells - 1) / 2;
            int cellColIndex = (wCells - 1) / 2;

            if (direction == "->")
            {
                // Стрілка на правій грані картинки
                float rightEdgeX = imgX + (wCells * cellSize);
                float targetCenterY = imgY + (cellRowIndex * cellSize) + (cellSize / 2);

                triangle = new PointF[] {
                    new PointF(rightEdgeX + arrowLen, targetCenterY),                      
                    new PointF(rightEdgeX, targetCenterY - arrowLen/1.5f), 
                    new PointF(rightEdgeX, targetCenterY + arrowLen/1.5f)  
                };
            }
            else // "v"
            {
                // Стрілка на нижній грані картинки
                float bottomEdgeY = imgY + (hCells * cellSize);
                float targetCenterX = imgX + (cellColIndex * cellSize) + (cellSize / 2);

                triangle = new PointF[] {
                    new PointF(targetCenterX, bottomEdgeY + arrowLen),                      
                    new PointF(targetCenterX - arrowLen/1.5f, bottomEdgeY), 
                    new PointF(targetCenterX + arrowLen/1.5f, bottomEdgeY)  
                };
            }
            // Малюємо чорну стрілку
            g.FillPolygon(Brushes.Black, triangle);
        }
    }
}
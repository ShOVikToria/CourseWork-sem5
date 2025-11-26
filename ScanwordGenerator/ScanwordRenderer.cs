using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace ScanwordGenerator
{
    public class ScanwordRenderer
    {
        private readonly Brush _brushText = Brushes.Black;
        private readonly Brush _brushGray = new SolidBrush(Color.FromArgb(220, 220, 220));
        private readonly Brush _brushArrow = Brushes.Black; // Стрілки краще чорним або темно-сірим
        private readonly Pen _penBorder = new Pen(Color.Gray, 1);

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
                g.Clear(Color.White);

                // ЗМІНА 2: Шрифт відповідей тонший (Regular) і трішки менший (0.5f замість 0.6f)
                using (Font fontLetter = new Font("Arial", cellSize * 0.5f, FontStyle.Regular))
                using (Font fontDef = new Font("Arial Narrow", cellSize * 0.18f, FontStyle.Regular))
                {
                    for (int y = 0; y < gridH; y++)
                    {
                        for (int x = 0; x < gridW; x++)
                        {
                            DrawCell(g, grid[y, x], x, y, cellSize, fontLetter, fontDef, showAnswers);
                        }
                    }
                }
            }
            return bmp;
        }

        private void DrawCell(Graphics g, Cell cell, int x, int y, float size, Font fLetter, Font fDef, bool showAnswers)
        {
            float px = x * size + 5;
            float py = y * size + 5;
            RectangleF rect = new RectangleF(px, py, size, size);

            // 1. Фон
            if (cell.Type == CellType.Empty)
                g.FillRectangle(_brushGray, rect);
            else
                g.FillRectangle(Brushes.White, rect);

            // 2. Рамка
            g.DrawRectangle(_penBorder, px, py, size, size);

            // 3. Вміст
            if (cell.Type == CellType.Letter && showAnswers)
            {
                string letter = cell.Letter.ToString();
                SizeF strSize = g.MeasureString(letter, fLetter);
                // Центруємо літеру
                g.DrawString(letter, fLetter, _brushText, px + (size - strSize.Width) / 2, py + (size - strSize.Height) / 2);
            }
            else if (cell.Type == CellType.Definition)
            {
                // Текст питання
                RectangleF textRect = new RectangleF(px + 2, py + 2, size - 4, size - 4);
                g.DrawString(cell.DefinitionText, fDef, _brushText, textRect);

                // ЗМІНА 1: Малюємо стрілку не тут, а в напрямку клітинки-відповіді
                DrawArrowPointingToTarget(g, px, py, size, cell.ArrowDirection);
            }
        }

        private void DrawArrowPointingToTarget(Graphics g, float defX, float defY, float size, string direction)
        {
            float arrowLen = size * 0.2f; // Довжина стрілки

            PointF[] triangle;

            if (direction == "->") // Вправо (малюємо в лівій частині НАСТУПНОЇ клітинки)
            {
                // Координати цільової клітинки (тієї, що справа)
                float targetX = defX + size;
                float targetY = defY;

                // Малюємо стрілку на початку цільової клітинки (зліва по центру)
                float ax = targetX + 2; // +2 пікселі відступу від лінії
                float ay = targetY + size / 2;

                triangle = new PointF[] {
                    new PointF(ax + arrowLen, ay),                 // Носик (вправо)
                    new PointF(ax, ay - arrowLen / 1.5f),          // Верх
                    new PointF(ax, ay + arrowLen / 1.5f)           // Низ
                };
            }
            else // "v" - Вниз (малюємо у верхній частині НИЖНЬОЇ клітинки)
            {
                // Координати цільової клітинки (тієї, що знизу)
                float targetX = defX;
                float targetY = defY + size;

                // Малюємо стрілку зверху цільової клітинки (по центру)
                float ax = targetX + size / 2;
                float ay = targetY + 2; // +2 пікселі відступу від лінії

                triangle = new PointF[] {
                    new PointF(ax, ay + arrowLen),                 // Носик (вниз)
                    new PointF(ax - arrowLen / 1.5f, ay),          // Ліво
                    new PointF(ax + arrowLen / 1.5f, ay)           // Право
                };
            }

            g.FillPolygon(_brushArrow, triangle);
        }
    }
}
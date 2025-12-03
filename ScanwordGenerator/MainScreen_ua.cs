using System.Drawing.Imaging; // Для PNG
using PdfSharp.Pdf;           // Для PDF
using PdfSharp.Drawing;       // Для малювання 
using System.Diagnostics;

namespace ScanwordGenerator
{
    public partial class MainScreen_ua : UserControl
    {
        public event EventHandler BackButtonClicked;

        private readonly ScanwordService _service = new ScanwordService();
        private readonly ScanwordRenderer _renderer = new ScanwordRenderer();
        private Cell[,] _currentGrid;
        private readonly Random _rng = new Random();

        public MainScreen_ua()
        {
            InitializeComponent();

            InitializeTopics();
            radioButton_SizeSmall.Checked = true;

            try
            {
                _service.LoadDictionary("words_ua.json");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка завантаження словника: {ex.Message}");
            }

            button1.Click += ButtonGenerate_Click;
            checkBox_ShowAnswers.CheckedChanged += CheckBoxShowAnswers_CheckedChanged;
            LabelBack.Click += LabelBack_Click;
        }

        private void InitializeTopics()
        {
            Topics.Items.Clear();
            Topics.Items.AddRange(new string[] {
                "Тваринний світ",
                "Світ рослин",
                "Географія",
                "Космос і погода",
                "Кіно і ТБ",        
                "Музика",
                "Мистецтво та архітектура",
                "Спорт",
                "Кулінарія",
                "Дім та побут",
                "Професії та хобі"
            });
            Topics.SelectedIndex = 0;
        }

        private void LabelBack_Click(object sender, EventArgs e)
        {
            BackButtonClicked?.Invoke(this, EventArgs.Empty);
        }

        private (int w, int h)? GetSelectedSize()
        {
            if (radioButton_SizeSmall.Checked) return (_rng.Next(10, 16), _rng.Next(10, 16));
            if (radioButton_SizeMiddle.Checked) return (_rng.Next(16, 26), _rng.Next(16, 26));
            if (radioButton_SizeBig.Checked) return (_rng.Next(26, 41), _rng.Next(26, 41));

            // ВЛАСНИЙ РОЗМІР
            if (radioButton_SizeCustom.Checked)
            {
                // 1. Спроба перетворити текст у число
                bool wParsed = int.TryParse(textBox_GorizontalSize.Text, out int w);
                bool hParsed = int.TryParse(textBox_VerticalSize.Text, out int h);

                // Якщо хоча б одне поле не є числом -> ПОМИЛКА
                if (!wParsed || !hParsed)
                {
                    MessageBox.Show("Будь ласка, введіть коректні цілі числа у поля розміру!", "Помилка вводу", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return null; // Повертаємо null, щоб зупинити генерацію
                }

                // 2. Якщо це числа, перевіряємо чи вони в межах (5-50)
                if (w < 5 || w > 50 || h < 5 || h > 50)
                {
                    MessageBox.Show("Розмір сторони має бути в межах від 5 до 50 клітинок!", "Некоректний розмір", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return null; // Повертаємо null
                }

                // 3. Якщо все добре - повертаємо розміри
                return (w, h);
            }
            return (15, 15);
        }

        private void ButtonGenerate_Click(object sender, EventArgs e)
        {
            if (!_service.IsDictionaryLoaded)
            {
                MessageBox.Show("Словник не завантажено! Перевірте файл words_ua.json.");
                return;
            }

            // 1. Отримуємо тему
            string selectedTopic = Topics.SelectedItem.ToString();
            var themeWords = _service.GetWordsByTheme(selectedTopic);

            if (themeWords.Count < 5)
            {
                MessageBox.Show($"У темі '{selectedTopic}' замало слів ({themeWords.Count}).");
                return;
            }

            string imagePrefix = "animals"; // За замовчуванням

            if (selectedTopic == "Тваринний світ") imagePrefix = "animals";
            else if (selectedTopic == "Кіно і ТБ") imagePrefix = "cinema";
            else if (selectedTopic == "Світ рослин") imagePrefix = "plants";
            else if (selectedTopic == "Космос і погода") imagePrefix = "space";
            else if (selectedTopic == "Музика") imagePrefix = "music";
            else if (selectedTopic == "Спорт") imagePrefix = "sport";
            else if (selectedTopic == "Кулінарія") imagePrefix = "cooking";
            else if (selectedTopic == "Дім та побут") imagePrefix = "home";
            else if (selectedTopic == "Професії та хобі") imagePrefix = "professions";
            else if (selectedTopic == "Мистецтво та архітектура") imagePrefix = "art";
            else if (selectedTopic == "Географія") imagePrefix = "geography";

            // 2. Отримання налаштувань
            var sizeResult = GetSelectedSize();

            // ЯКЩО ПОВЕРНУВСЯ NULL (була помилка) -> ВИХОДИМО З МЕТОДУ
            if (sizeResult == null) return;

            var (w, h) = sizeResult.Value;

            bool useImages = checkBox_Pictures.Checked;

            Cursor.Current = Cursors.WaitCursor;

            // 1. ЗАПУСК ТАЙМЕРА
            Stopwatch sw = Stopwatch.StartNew();

            try
            {
                // --- ПЕРЕДАЄМО imagePrefix У МЕТОД ---
                _currentGrid = _service.GenerateBestGrid(w, h, useImages, themeWords, imagePrefix, attempts: 30);

                long genTime = sw.ElapsedMilliseconds;

                if (_currentGrid != null)
                {
                    UpdateImage();
                    long totalTime = sw.ElapsedMilliseconds;
                    sw.Stop();
                }
                else
                {
                    sw.Stop();
                    MessageBox.Show($"Не вдалося згенерувати сканворд розміром {w}x{h}.");
                }
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        private void buttonTest_Click(object sender, EventArgs e)
        {
            if (!_service.IsDictionaryLoaded) return;

            var words = _service.GetWordsByTheme("Тваринний світ");

            if (words.Count < 10)
            {
                MessageBox.Show("Замало слів для тестування!");
                return;
            }

            Cursor.Current = Cursors.WaitCursor;

            BenchmarkRunner.RunFullTest(_service, words, "animals");

            Cursor.Current = Cursors.Default;
        }

        private void CheckBoxShowAnswers_CheckedChanged(object sender, EventArgs e) => UpdateImage();

        private void UpdateImage()
        {
            if (_currentGrid == null) return;
            Bitmap resultImage = _renderer.DrawGrid(_currentGrid, ScanwordFild.Width, ScanwordFild.Height, checkBox_ShowAnswers.Checked);
            if (ScanwordFild.Image != null) ScanwordFild.Image.Dispose();
            ScanwordFild.Image = resultImage;
        }

        // --- ЕКСПОРТ У PNG ---
        private void button_PNG_Click(object sender, EventArgs e)
        {
            if (_currentGrid == null)
            {
                MessageBox.Show("Спочатку згенеруйте сканворд!");
                return;
            }

            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "PNG Image|*.png";
                sfd.FileName = "scanword.png";

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    bool withAnswers = checkBox_DownloadWithAnswers.Checked;

                    // 1. Розраховуємо розміри картинки
                    // Задаємо бажаний розмір клітинки для експорту
                    int exportCellSize = 60;
                    int padding = 10; // Невеликий відступ по краях

                    int gridW = _currentGrid.GetLength(1); // Кількість колонок
                    int gridH = _currentGrid.GetLength(0); // Кількість рядків

                    // Точна ширина та висота, щоб не було зайвого сірого фону
                    int finalWidth = (gridW * exportCellSize) + padding;
                    int finalHeight = (gridH * exportCellSize) + padding;

                    // 2. Генеруємо
                    using (Bitmap bmp = _renderer.DrawGrid(_currentGrid, finalWidth, finalHeight, withAnswers))
                    {
                        bmp.Save(sfd.FileName, ImageFormat.Png);
                    }
                    MessageBox.Show("Збережено!");
                }
            }
        }

        // --- ЕКСПОРТ У PDF ---
        private void button_PDF_Click(object sender, EventArgs e)
        {
            if (_currentGrid == null)
            {
                MessageBox.Show("Спочатку згенеруйте сканворд!");
                return;
            }

            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "PDF Document|*.pdf";
                sfd.FileName = "scanword.pdf";

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        using (PdfDocument document = new PdfDocument())
                        {
                            document.Info.Title = "Scanword";
                            // 1. Створюємо сторінку
                            PdfPage page = document.AddPage();
                            page.Size = PdfSharp.PageSize.A4;
                            // 2. Генеруємо високоякісну картинку (Tight Crop)
                            bool withAnswers = checkBox_DownloadWithAnswers.Checked;
                            int exportCellSize = 80; 
                            int gridW = _currentGrid.GetLength(1);
                            int gridH = _currentGrid.GetLength(0);
                            int finalPixelW = (gridW * exportCellSize) + 10;
                            int finalPixelH = (gridH * exportCellSize) + 10;

                            using (Bitmap bmp = _renderer.DrawGrid(_currentGrid, finalPixelW, finalPixelH, withAnswers))
                            {
                                // 3. АВТОМАТИЧНА ОРІЄНТАЦІЯ
                                if (finalPixelW > finalPixelH)
                                {page.Orientation = PdfSharp.PageOrientation.Landscape;}
                                else
                                {page.Orientation = PdfSharp.PageOrientation.Portrait;}

                                using (MemoryStream ms = new MemoryStream())
                                {
                                    bmp.Save(ms, ImageFormat.Png);
                                    ms.Position = 0;
                                    using (XImage xImage = XImage.FromStream(ms))
                                    {
                                        using (XGraphics gfx = XGraphics.FromPdfPage(page))
                                        {
                                            // 4. МАСШТАБУВАННЯ ПІД СТОРІНКУ
                                            // Отримуємо розміри сторінки в пунктах (Points)
                                            double pageWidth = page.Width;
                                            double pageHeight = page.Height;
                                            double margin = 40; // Відступи від країв аркуша

                                            // Область для малювання
                                            double drawAreaW = pageWidth - (margin * 2);
                                            double drawAreaH = pageHeight - (margin * 2);

                                            // Розрахунок пропорцій, щоб вписати картинку (Fit)
                                            double ratioX = drawAreaW / xImage.PixelWidth;
                                            double ratioY = drawAreaH / xImage.PixelHeight;
                                            double ratio = Math.Min(ratioX, ratioY); // Беремо менший коефіцієнт, щоб влізло все

                                            // Нові розміри на PDF
                                            double newWidth = xImage.PixelWidth * ratio;
                                            double newHeight = xImage.PixelHeight * ratio;

                                            // Центрування на сторінці
                                            double x = (pageWidth - newWidth) / 2;
                                            double y = (pageHeight - newHeight) / 2;

                                            // Малюємо
                                            gfx.DrawImage(xImage, x, y, newWidth, newHeight);
                                        }
                                    }
                                }
                            }
                            document.Save(sfd.FileName);
                        }
                        MessageBox.Show("PDF Збережено!");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Помилка: {ex.Message}");
                    }
                }
            }
        }
    }
}
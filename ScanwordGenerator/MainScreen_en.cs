using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging; // Для PNG
using System.IO;
using System.Windows.Forms;
using PdfSharp.Drawing;       // Для PDF
using PdfSharp.Pdf;           // Для PDF
using ScanwordGenerator;

namespace ScanwordGenerator
{
    public partial class MainScreen_en : UserControl
    {
        public event EventHandler BackButtonClicked;

        private readonly ScanwordService _service = new ScanwordService();
        private readonly ScanwordRenderer _renderer = new ScanwordRenderer();
        private Cell[,] _currentGrid;
        private readonly Random _rng = new Random();

        public MainScreen_en()
        {
            InitializeComponent();
            InitializeTopics();
            radioButton_SizeSmall.Checked = true;

            try
            {
                _service.LoadDictionary("words_en.json");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Dictionary Error: {ex.Message}");
            }

            button1.Click += ButtonGenerate_Click;
            checkBox_ShowAnswers.CheckedChanged += CheckBoxShowAnswers_CheckedChanged;
            LabelBack.Click += LabelBack_Click;

            // Якщо кнопки експорту не підписані в дизайнері:
            // button_PNG.Click += button_PNG_Click;
            // button_PDF.Click += button_PDF_Click;
        }

        private void InitializeTopics()
        {
            Topics.Items.Clear();
            Topics.Items.AddRange(new string[] {
                "Animal Kingdom",        // 0
                "Plant Kingdom",         // 1
                "Geography",             // 2
                "Space & Weather",       // 3
                "Movies & TV",           // 4
                "Music",                 // 5
                "Literature",            // 6
                "Art & Architecture",    // 7
                "Sports",                // 8
                "Science & Tech",        // 9
                "History",               // 10
                "Cooking",               // 11
                "Home & Living",         // 12
                "Professions & Hobbies"  // 13
            });
            Topics.SelectedIndex = 0;
        }

        private void LabelBack_Click(object sender, EventArgs e)
        {
            BackButtonClicked?.Invoke(this, EventArgs.Empty);
        }

        private void ButtonGenerate_Click(object sender, EventArgs e)
        {
            if (!_service.IsDictionaryLoaded)
            {
                MessageBox.Show("Dictionary not loaded! Check words_en.json.");
                return;
            }

            string selectedTopic = Topics.SelectedItem.ToString();
            var themeWords = _service.GetWordsByTheme(selectedTopic);

            if (themeWords.Count < 5)
            {
                MessageBox.Show($"Not enough words in topic '{selectedTopic}' ({themeWords.Count}).");
                return;
            }

            // --- ВИЗНАЧЕННЯ ПРЕФІКСУ (Mapping for English) ---
            string imagePrefix = "animals"; // Default

            if (selectedTopic == "Animal Kingdom") imagePrefix = "animals";
            else if (selectedTopic == "Movies & TV") imagePrefix = "cinema";
            // -------------------------------------------------

            var sizeResult = GetSelectedSize();
            if (sizeResult == null) return;

            var (w, h) = sizeResult.Value;
            bool useImages = checkBox_Pictures.Checked;

            Cursor.Current = Cursors.WaitCursor;
            Stopwatch sw = Stopwatch.StartNew();

            try
            {
                // 4. Генерація з ПРЕФІКСОМ
                _currentGrid = _service.GenerateBestGrid(w, h, useImages, themeWords, imagePrefix, attempts: 30);

                long genTime = sw.ElapsedMilliseconds;

                if (_currentGrid != null)
                {
                    UpdateImage();

                    long totalTime = sw.ElapsedMilliseconds;
                    sw.Stop();

                    // Логування (опціонально)
                    // PerformanceLogger.LogGeneration(w, h, useImages, genTime, totalTime);
                }
                else
                {
                    sw.Stop();
                    MessageBox.Show($"Failed to generate scanword ({w}x{h}).");
                }
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        private void CheckBoxShowAnswers_CheckedChanged(object sender, EventArgs e) => UpdateImage();

        private void UpdateImage()
        {
            if (_currentGrid == null) return;
            Bitmap resultImage = _renderer.DrawGrid(_currentGrid, ScanwordFild.Width, ScanwordFild.Height, checkBox_ShowAnswers.Checked);
            if (ScanwordFild.Image != null) ScanwordFild.Image.Dispose();
            ScanwordFild.Image = resultImage;
        }

        private (int w, int h)? GetSelectedSize()
        {
            if (radioButton_SizeSmall.Checked) return (_rng.Next(10, 16), _rng.Next(10, 16));
            if (radioButton_SizeMiddle.Checked) return (_rng.Next(16, 26), _rng.Next(16, 26));
            if (radioButton_SizeBig.Checked) return (_rng.Next(26, 41), _rng.Next(26, 41));

            if (radioButton_SizeCustom.Checked)
            {
                bool wParsed = int.TryParse(textBox_GorizontalSize.Text, out int w);
                bool hParsed = int.TryParse(textBox_VerticalSize.Text, out int h);

                if (!wParsed || !hParsed)
                {
                    MessageBox.Show("Please enter valid integer numbers!", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return null;
                }

                if (w < 5 || w > 50 || h < 5 || h > 50)
                {
                    MessageBox.Show("Size must be between 5 and 50 cells!", "Invalid Size", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return null;
                }
                return (w, h);
            }
            return (15, 15);
        }

        private void button_PNG_Click(object sender, EventArgs e)
        {
            if (_currentGrid == null)
            {
                MessageBox.Show("Generate scanword first!");
                return;
            }

            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "PNG Image|*.png";
                sfd.FileName = "scanword.png";

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    bool withAnswers = checkBox_DownloadWithAnswers.Checked;
                    int exportCellSize = 60;
                    int padding = 10;
                    int gridW = _currentGrid.GetLength(1);
                    int gridH = _currentGrid.GetLength(0);
                    int finalWidth = (gridW * exportCellSize) + padding;
                    int finalHeight = (gridH * exportCellSize) + padding;

                    using (Bitmap bmp = _renderer.DrawGrid(_currentGrid, finalWidth, finalHeight, withAnswers))
                    {
                        bmp.Save(sfd.FileName, ImageFormat.Png);
                    }
                    MessageBox.Show("Saved!");
                }
            }
        }

        private void button_PDF_Click(object sender, EventArgs e)
        {
            if (_currentGrid == null)
            {
                MessageBox.Show("Generate scanword first!");
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
                            PdfPage page = document.AddPage();
                            page.Size = PdfSharp.PageSize.A4;

                            bool withAnswers = checkBox_DownloadWithAnswers.Checked;
                            int exportCellSize = 80;
                            int gridW = _currentGrid.GetLength(1);
                            int gridH = _currentGrid.GetLength(0);
                            int finalPixelW = (gridW * exportCellSize) + 10;
                            int finalPixelH = (gridH * exportCellSize) + 10;

                            using (Bitmap bmp = _renderer.DrawGrid(_currentGrid, finalPixelW, finalPixelH, withAnswers))
                            {
                                if (finalPixelW > finalPixelH) page.Orientation = PdfSharp.PageOrientation.Landscape;
                                else page.Orientation = PdfSharp.PageOrientation.Portrait;

                                using (MemoryStream ms = new MemoryStream())
                                {
                                    bmp.Save(ms, ImageFormat.Png);
                                    ms.Position = 0;
                                    using (XImage xImage = XImage.FromStream(ms))
                                    {
                                        using (XGraphics gfx = XGraphics.FromPdfPage(page))
                                        {
                                            double pageWidth = page.Width;
                                            double pageHeight = page.Height;
                                            double margin = 40;
                                            double drawAreaW = pageWidth - (margin * 2);
                                            double drawAreaH = pageHeight - (margin * 2);
                                            double ratioX = drawAreaW / xImage.PixelWidth;
                                            double ratioY = drawAreaH / xImage.PixelHeight;
                                            double ratio = Math.Min(ratioX, ratioY);
                                            double newWidth = xImage.PixelWidth * ratio;
                                            double newHeight = xImage.PixelHeight * ratio;
                                            double x = (pageWidth - newWidth) / 2;
                                            double y = (pageHeight - newHeight) / 2;
                                            gfx.DrawImage(xImage, x, y, newWidth, newHeight);
                                        }
                                    }
                                }
                            }
                            document.Save(sfd.FileName);
                        }
                        MessageBox.Show("PDF Saved!");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error: {ex.Message}");
                    }
                }
            }
        }
    }
}
using System;
using System.Drawing;
using System.Windows.Forms;
using ScanwordGenerator; // Додай, якщо namespaces не вирівняні

namespace ScanwordGenerator
{
    public partial class MainScreen : UserControl
    {
        public event EventHandler BackButtonClicked;

        private readonly ScanwordService _service = new ScanwordService();
        private readonly ScanwordRenderer _renderer = new ScanwordRenderer();
        private Cell[,] _currentGrid;

        // Генератор випадкових чисел для розмірів
        private readonly Random _rng = new Random();

        public MainScreen()
        {
            InitializeComponent();

            try
            {
                _service.LoadDictionary("animal_ua.json");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            button1.Click += ButtonGenerate_Click;
            checkBox_ShowAnswers.CheckedChanged += CheckBoxShowAnswers_CheckedChanged;
        }

        private void LabelBack_Click(object sender, EventArgs e)
        {
            BackButtonClicked?.Invoke(this, EventArgs.Empty);
        }

        private void ButtonGenerate_Click(object sender, EventArgs e)
        {
            // 1. Перевірка словника
            if (!_service.IsDictionaryLoaded)
            {
                MessageBox.Show("Словник не завантажено!");
                return;
            }

            // 2. Отримання налаштувань
            var (w, h) = GetSelectedSize();
            bool useImages = checkBox_Pictures.Checked;

            // 3. Вмикаємо годинник (очікування)
            Cursor.Current = Cursors.WaitCursor;

            try
            {
                // 4. Запуск генерації
                _currentGrid = _service.GenerateBestGrid(w, h, useImages, attempts: 50);

                // 5. Обробка результату
                if (_currentGrid != null)
                {
                    UpdateImage();
                }
                else
                {
                    MessageBox.Show($"Не вдалося згенерувати сканворд розміром {w}x{h}. Спробуйте ще раз.");
                }
            }
            finally
            {
                // 6. ОБОВ'ЯЗКОВО повертаємо звичайний курсор (навіть якщо сталась помилка)
                Cursor.Current = Cursors.Default;
            }
        }

        private void CheckBoxShowAnswers_CheckedChanged(object sender, EventArgs e)
        {
            UpdateImage();
        }

        private void UpdateImage()
        {
            if (_currentGrid == null) return;

            Bitmap resultImage = _renderer.DrawGrid(
                _currentGrid,
                ScanwordFild.Width,
                ScanwordFild.Height,
                checkBox_ShowAnswers.Checked
            );

            if (ScanwordFild.Image != null) ScanwordFild.Image.Dispose();
            ScanwordFild.Image = resultImage;
        }

        // --- НОВА ЛОГІКА РОЗМІРІВ ---
        private (int w, int h) GetSelectedSize()
        {
            // 1. Малий: від 10 до 15
            if (radioButton_SizeSmall.Checked)
            {
                // Next(min, max) -> max не включається, тому беремо 16
                int w = _rng.Next(10, 16);
                int h = _rng.Next(10, 16);
                return (w, h);
            }

            // 2. Середній: від 16 до 25
            if (radioButton_SizeMiddle.Checked)
            {
                int w = _rng.Next(16, 26);
                int h = _rng.Next(16, 26);
                return (w, h);
            }

            // 3. Великий: від 26 до 40
            if (radioButton_SizeBig.Checked)
            {
                int w = _rng.Next(26, 41);
                int h = _rng.Next(26, 41);
                return (w, h); // Увага: 40x40 може генеруватися довго!
            }

            // 4. Власний
            if (radioButton_SizeCustom.Checked)
            {
                int w = 15, h = 15;

                // Парсимо введення, якщо там сміття - буде 0
                int.TryParse(textBox_GorizontalSize.Text, out w);
                int.TryParse(textBox_VerticalSize.Text, out h);

                // Обмеження (мінімум 5, максимум 50)
                w = Math.Clamp(w, 5, 50);
                h = Math.Clamp(h, 5, 50);

                // Оновлюємо TextBox'и, щоб користувач бачив реальні значення
                textBox_GorizontalSize.Text = w.ToString();
                textBox_VerticalSize.Text = h.ToString();

                return (w, h);
            }

            return (15, 15); // Значення за замовчуванням
        }
    }
}
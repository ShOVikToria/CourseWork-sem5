using System;
using System.Windows.Forms;

namespace ScanwordGenerator
{
    public partial class MainContainerForm : Form
    {
        // Зберігаємо поточну мову ("ua" або "en")
        private string _currentLanguage = "ua";

        public MainContainerForm()
        {
            InitializeComponent();

            // Завантажуємо стартовий екран українською за замовчуванням
            ShowStartScreen("ua");
        }

        /// <summary>
        /// Показує стартовий екран обраною мовою
        /// </summary>
        private void ShowStartScreen(string langCode)
        {
            // Оновлюємо глобальну змінну мови
            _currentLanguage = langCode;

            // Очищаємо панель
            ContentPanel.Controls.Clear();

            if (langCode == "ua")
            {
                StartScreen_ua startScreen = new StartScreen_ua();
                startScreen.Dock = DockStyle.Fill;

                // 1. Якщо натиснули "ПОЧАТИ" -> йдемо на головний екран
                startScreen.StartButtomClicked += (s, e) => ShowMainScreen();

                // 2. Якщо змінили мову в комбобоксі -> перезавантажуємо цей метод з новою мовою
                startScreen.LanguageChanged += (s, newLang) => ShowStartScreen(newLang);

                ContentPanel.Controls.Add(startScreen);
            }
            else // "en"
            {
                StartScreen_en startScreen = new StartScreen_en();
                startScreen.Dock = DockStyle.Fill;

                // 1. Start button logic
                startScreen.StartButtomClicked += (s, e) => ShowMainScreen();

                // 2. Language change logic
                startScreen.LanguageChanged += (s, newLang) => ShowStartScreen(newLang);

                ContentPanel.Controls.Add(startScreen);
            }
        }

        /// <summary>
        /// Показує основний екран генерації (відповідно до обраної мови)
        /// </summary>
        private void ShowMainScreen()
        {
            ContentPanel.Controls.Clear();

            if (_currentLanguage == "ua")
            {
                MainScreen_ua mainScreen = new MainScreen_ua();
                mainScreen.Dock = DockStyle.Fill;

                // Якщо натиснули "Назад" -> повертаємось на старт (зберігаючи мову)
                mainScreen.BackButtonClicked += (s, e) => ShowStartScreen("ua");

                ContentPanel.Controls.Add(mainScreen);
            }
            else // "en"
            {
                MainScreen_en mainScreen = new MainScreen_en();
                mainScreen.Dock = DockStyle.Fill;

                // Back button logic
                mainScreen.BackButtonClicked += (s, e) => ShowStartScreen("en");

                ContentPanel.Controls.Add(mainScreen);
            }
        }
    }
}
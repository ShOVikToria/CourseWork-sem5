using System;
using System.Windows.Forms;

namespace ScanwordGenerator
{
    public partial class StartScreen_ua : UserControl
    {
        // Подія натискання кнопки "ПОЧАТИ"
        public event EventHandler StartButtomClicked;
        // Подія зміни мови (передає "en" або "ua")
        public event EventHandler<string> LanguageChanged;

        public StartScreen_ua()
        {
            InitializeComponent();
            InitializeLanguageComboBox();
        }

        private void InitializeLanguageComboBox()
        {
            // Налаштовуємо список мов
            comboBox_Languages.Items.Clear();
            comboBox_Languages.Items.Add("Українська"); // Index 0
            comboBox_Languages.Items.Add("English");    // Index 1

            // Українська за замовчуванням
            comboBox_Languages.SelectedIndex = 0;

            // Підписуємось на зміну вибору
            comboBox_Languages.SelectedIndexChanged += ComboBox_Languages_SelectedIndexChanged;
        }

        private void ComboBox_Languages_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Якщо обрали English (індекс 1), повідомляємо головну форму
            if (comboBox_Languages.SelectedIndex == 1)
            {
                LanguageChanged?.Invoke(this, "en");
            }
        }

        private void start_buttom_Click(object sender, EventArgs e)
        {
            StartButtomClicked?.Invoke(this, EventArgs.Empty);
        }
    }
}
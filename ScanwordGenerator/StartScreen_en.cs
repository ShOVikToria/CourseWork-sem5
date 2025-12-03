using System;
using System.Windows.Forms;

namespace ScanwordGenerator
{
    public partial class StartScreen_en : UserControl
    {
        // Подія натискання кнопки "START"
        public event EventHandler StartButtomClicked;
        // Подія зміни мови
        public event EventHandler<string> LanguageChanged;

        public StartScreen_en()
        {
            InitializeComponent();
            InitializeLanguageComboBox();
        }

        private void InitializeLanguageComboBox()
        {
            // Налаштовуємо список мов
            comboBox_Languages.Items.Clear();
            comboBox_Languages.Items.Add("Українська"); // Index 0
            comboBox_Languages.Items.Add("English");   // Index 1

            // Вибираємо англійську за замовчуванням
            comboBox_Languages.SelectedIndex = 1;

            // Підписуємось на зміну вибору
            comboBox_Languages.SelectedIndexChanged += ComboBox_Languages_SelectedIndexChanged;
        }

        private void ComboBox_Languages_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Якщо обрали Українська (індекс 0), повідомляємо головну форму
            if (comboBox_Languages.SelectedIndex == 0)
            {
                LanguageChanged?.Invoke(this, "ua");
            }
        }

        private void start_buttom_Click(object sender, EventArgs e)
        {
            StartButtomClicked?.Invoke(this, EventArgs.Empty);
        }
    }
}
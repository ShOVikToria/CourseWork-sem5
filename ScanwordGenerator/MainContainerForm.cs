namespace ScanwordGenerator
{
    public partial class MainContainerForm : Form
    {
        public MainContainerForm()
        {
            // 1. Ініціалізація компонентів форми (створює ContentPanel)
            InitializeComponent();

            // 2. Викликаємо функцію, яка завантажує початковий екран
            // Це гарантує, що інтерфейс завантажиться першим
            ShowStartScreen();
        }

        /// <summary>
        /// Завантажує User Control початкового екрана на головну панель.
        /// </summary>
        private void ShowStartScreen()
        {
            // Очищаємо панель перед додаванням нового елемента
            // (Це важливо, коли ви будете повертатися з MainScreenControl)
            ContentPanel.Controls.Clear();

            // Створюємо екземпляр User Control
            StartScreen startScreen = new StartScreen();

            // Встановлюємо розмір та прив'язку на весь простір панелі
            startScreen.Dock = DockStyle.Fill;

            // ПІДПИСКА на подію
            startScreen.StartButtomClicked += StartScreen_StartButtomClicked; 

            // Додаємо його на головну панель ContentPanel
            ContentPanel.Controls.Add(startScreen);
        }

        private void ShowMainScreen()
        {
            // 1. Очищуємо контейнер
            ContentPanel.Controls.Clear();

            // 2. Створюємо та налаштовуємо User Control головного екрана
            MainScreen mainScreen = new MainScreen();
            mainScreen.Dock = DockStyle.Fill;

            mainScreen.BackButtonClicked += MainScreen_BackButtonClicked;

            // 3. Додаємо на панель
            ContentPanel.Controls.Add(mainScreen);
        }
        // Обробник події, який отримує сигнал від StartScreenControl
        private void StartScreen_StartButtomClicked(object sender, EventArgs e)
        {
            // Коли подія спрацьовує, MainContainerForm сама виконує перехід
            ShowMainScreen();
        }

        private void MainScreen_BackButtonClicked(object sender, EventArgs e)
        {
            // Коли сигнал "Назад" надійде, MainContainerForm знає, що робити:
            // Повернутися до стартового екрана
            ShowStartScreen();
        }

    }
}

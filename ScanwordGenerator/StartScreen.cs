using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ScanwordGenerator
{
    public partial class StartScreen : UserControl
    {
        public event EventHandler StartButtomClicked;
        public StartScreen()
        {
            InitializeComponent();
        }

        private void start_buttom_Click(object sender, EventArgs e)
        {
            StartButtomClicked?.Invoke(this, EventArgs.Empty);
        }
    }
}

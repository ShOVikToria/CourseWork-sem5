namespace ScanwordGenerator
{
    partial class MainContainerForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            ContentPanel = new Panel();
            SuspendLayout();
            // 
            // ContentPanel
            // 
            ContentPanel.Location = new Point(2, 1);
            ContentPanel.Name = "ContentPanel";
            ContentPanel.Size = new Size(1912, 992);
            ContentPanel.TabIndex = 0;
            // 
            // StartForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.ControlLight;
            ClientSize = new Size(1912, 993);
            Controls.Add(ContentPanel);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Name = "StartForm";
            StartPosition = FormStartPosition.Manual;
            Text = "Генератор сканвордів";
            ResumeLayout(false);
        }

        #endregion

        private Panel ContentPanel;
    }
}

namespace ScanwordGenerator
{
    partial class StartScreen_en
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            MainTitle = new Label();
            start_buttom = new Button();
            comboBox_Languages = new ComboBox();
            label_Language = new Label();
            SuspendLayout();
            // 
            // MainTitle
            // 
            MainTitle.AutoSize = true;
            MainTitle.Font = new Font("Century Gothic", 80F);
            MainTitle.Location = new Point(642, 258);
            MainTitle.Name = "MainTitle";
            MainTitle.Size = new Size(620, 258);
            MainTitle.TabIndex = 5;
            MainTitle.Text = "Scanwords\r\ngenerator";
            MainTitle.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // start_buttom
            // 
            start_buttom.BackColor = SystemColors.ScrollBar;
            start_buttom.FlatAppearance.BorderColor = SystemColors.ScrollBar;
            start_buttom.FlatStyle = FlatStyle.Flat;
            start_buttom.Font = new Font("Century Gothic", 32F, FontStyle.Regular, GraphicsUnit.Point, 204);
            start_buttom.Location = new Point(866, 566);
            start_buttom.Name = "start_buttom";
            start_buttom.Size = new Size(200, 58);
            start_buttom.TabIndex = 6;
            start_buttom.Text = "START";
            start_buttom.UseVisualStyleBackColor = false;
            start_buttom.Click += start_buttom_Click;
            // 
            // comboBox_Languages
            // 
            comboBox_Languages.BackColor = SystemColors.ScrollBar;
            comboBox_Languages.FormattingEnabled = true;
            comboBox_Languages.Location = new Point(866, 711);
            comboBox_Languages.Name = "comboBox_Languages";
            comboBox_Languages.Size = new Size(200, 23);
            comboBox_Languages.TabIndex = 9;
            // 
            // label_Language
            // 
            label_Language.AutoSize = true;
            label_Language.Font = new Font("Century Gothic", 14.25F, FontStyle.Regular, GraphicsUnit.Point, 204);
            label_Language.ForeColor = SystemColors.ControlDark;
            label_Language.Location = new Point(878, 671);
            label_Language.Name = "label_Language";
            label_Language.Size = new Size(176, 22);
            label_Language.TabIndex = 8;
            label_Language.Text = "Choose language";
            // 
            // StartScreen_en
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(comboBox_Languages);
            Controls.Add(label_Language);
            Controls.Add(start_buttom);
            Controls.Add(MainTitle);
            Name = "StartScreen_en";
            Size = new Size(1912, 992);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label MainTitle;
        private Button start_buttom;
        private ComboBox comboBox_Languages;
        private Label label_Language;
    }
}

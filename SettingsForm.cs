using System;
using System.Drawing;
using System.Windows.Forms;

namespace AI_Writing_Assistant
{
    public class SettingsForm : Form
    {
        private TextBox apiKeyTextBox;
        private Button saveButton;
        private readonly SettingsService _settingsService;

        public SettingsForm(SettingsService settingsService)
        {
            _settingsService = settingsService;
            InitializeComponents();
            LoadSettings();
        }

        private void InitializeComponents()
        {
            this.Text = "Settings";
            this.Size = new Size(400, 300);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            var apiKeyLabel = new Label
            {
                Text = "API Key:",
                Location = new Point(20, 30),
                AutoSize = true
            };

            apiKeyTextBox = new TextBox
            {
                Location = new Point(20, 50),
                Size = new Size(350, 20),
                PasswordChar = '*'
            };

            saveButton = new Button
            {
                Text = "Save",
                Location = new Point(295, 200),
                Size = new Size(75, 23),
                BackColor = Color.FromArgb(0, 123, 255),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };

            saveButton.Click += SaveSettings;

            this.Controls.Add(apiKeyLabel);
            this.Controls.Add(apiKeyTextBox);
            this.Controls.Add(saveButton);
        }

        private void LoadSettings()
        {
            apiKeyTextBox.Text = _settingsService.GetApiKey();
        }

        private void SaveSettings(object sender, EventArgs e)
        {
            _settingsService.SaveApiKey(apiKeyTextBox.Text);
            MessageBox.Show("Settings saved successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            this.Close();
        }
    }
}

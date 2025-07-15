using System;
using System.Drawing;
using System.Windows.Forms;

namespace AI_Writing_Assistant
{
    public class SettingsForm : Form
    {
        private TextBox apiKeyTextBox;
        private Button saveButton;
        private ComboBox completionModeComboBox;
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

            var completionModeLabel = new Label
            {
                Text = "Completion Mode:",
                Location = new Point(20, 90),
                AutoSize = true
            };

            completionModeComboBox = new ComboBox
            {
                Location = new Point(20, 110),
                Size = new Size(350, 20),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            completionModeComboBox.Items.AddRange(Enum.GetNames(typeof(CompletionMode)));

            this.Controls.Add(apiKeyLabel);
            this.Controls.Add(apiKeyTextBox);
            this.Controls.Add(completionModeLabel);
            this.Controls.Add(completionModeComboBox);
            this.Controls.Add(saveButton);
        }

        private void LoadSettings()
        {
            apiKeyTextBox.Text = _settingsService.GetApiKey();
            completionModeComboBox.SelectedItem = _settingsService.GetCompletionMode().ToString();
        }

        private void SaveSettings(object sender, EventArgs e)
        {
            if (completionModeComboBox.SelectedItem != null)
            {
                var apiKey = apiKeyTextBox.Text;
                var mode = (CompletionMode)Enum.Parse(typeof(CompletionMode), completionModeComboBox.SelectedItem.ToString());
                _settingsService.SaveAllSettings(apiKey, mode);
            }
            MessageBox.Show("Settings saved successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            this.Close();
        }
    }
}

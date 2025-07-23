using System;
using System.Drawing;
using System.Windows.Forms;
using AI_Writing_Assistant.Services;

namespace AI_Writing_Assistant.Forms
{
    public class SettingsForm : Form
    {
        private TextBox apiKeyTextBox;
        private Button saveButton;
        private ComboBox completionModeComboBox;
        private ComboBox aiProviderComboBox;
        private TextBox writingPromptTextBox;
        private TextBox translationPromptTextBox;
        private readonly SettingsService _settingsService;

        public SettingsForm(SettingsService settingsService)
        {
            _settingsService = settingsService;
            InitializeComponents();
            LoadSettings();
        }

        private void InitializeComponents()
        {
            Text = "Settings";
            Size = new Size(400, 550);
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;

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

            var aiProviderLabel = new Label
            {
                Text = "AI Provider:",
                Location = new Point(20, 90),
                AutoSize = true
            };

            aiProviderComboBox = new ComboBox
            {
                Location = new Point(20, 110),
                Size = new Size(350, 20),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            aiProviderComboBox.Items.AddRange(Enum.GetNames(typeof(AiProvider)));

            var writingPromptLabel = new Label
            {
                Text = "Writing System Prompt:",
                Location = new Point(20, 210),
                AutoSize = true
            };

            writingPromptTextBox = new TextBox
            {
                Location = new Point(20, 230),
                Size = new Size(350, 100),
                Multiline = true,
                ScrollBars = ScrollBars.Vertical
            };

            var translationPromptLabel = new Label
            {
                Text = "Translation System Prompt:",
                Location = new Point(20, 350),
                AutoSize = true
            };

            translationPromptTextBox = new TextBox
            {
                Location = new Point(20, 370),
                Size = new Size(350, 100),
                Multiline = true,
                ScrollBars = ScrollBars.Vertical
            };

            saveButton = new Button
            {
                Text = "Save",
                Location = new Point(295, 470),
                Size = new Size(75, 23),
                BackColor = Color.FromArgb(0, 123, 255),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };

            saveButton.Click += SaveSettings;

            var completionModeLabel = new Label
            {
                Text = "Completion Mode:",
                Location = new Point(20, 150),
                AutoSize = true
            };

            completionModeComboBox = new ComboBox
            {
                Location = new Point(20, 170),
                Size = new Size(350, 20),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            completionModeComboBox.Items.AddRange(Enum.GetNames(typeof(CompletionMode)));

            Controls.Add(apiKeyLabel);
            Controls.Add(apiKeyTextBox);
            Controls.Add(aiProviderLabel);
            Controls.Add(aiProviderComboBox);
            Controls.Add(completionModeLabel);
            Controls.Add(completionModeComboBox);
            Controls.Add(writingPromptLabel);
            Controls.Add(writingPromptTextBox);
            Controls.Add(translationPromptLabel);
            Controls.Add(translationPromptTextBox);
            Controls.Add(saveButton);
        }

        private void LoadSettings()
        {
            apiKeyTextBox.Text = _settingsService.GetApiKey();
            aiProviderComboBox.SelectedItem = _settingsService.GetAiProvider().ToString();
            completionModeComboBox.SelectedItem = _settingsService.GetCompletionMode().ToString();
            writingPromptTextBox.Text = _settingsService.GetWritingSystemPrompt();
            translationPromptTextBox.Text = _settingsService.GetTranslationSystemPrompt();
        }

        private void SaveSettings(object sender, EventArgs e)
        {
            if (completionModeComboBox.SelectedItem != null && aiProviderComboBox.SelectedItem != null)
            {
                var apiKey = apiKeyTextBox.Text;
                var provider = (AiProvider)Enum.Parse(typeof(AiProvider), aiProviderComboBox.SelectedItem.ToString());
                var mode = (CompletionMode)Enum.Parse(typeof(CompletionMode), completionModeComboBox.SelectedItem.ToString());
                var writingPrompt = writingPromptTextBox.Text;
                var translationPrompt = translationPromptTextBox.Text;
                _settingsService.SaveAllSettings(apiKey, provider, mode, writingPrompt, translationPrompt);
            }
            MessageBox.Show("Settings saved successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            Close();
        }
    }
}

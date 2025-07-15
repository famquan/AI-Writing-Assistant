using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AI_Writing_Assistant
{
    public partial class MainForm : Form
    {
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                keyboardHook?.Dispose();
                trayIcon?.Dispose();
            }
            base.Dispose(disposing);
        }
        private NotifyIcon? trayIcon;
        private GlobalKeyboardHook? keyboardHook;
        private SuggestionWindow? suggestionWindow;
        private readonly AIService _aiService;
        private readonly SettingsService _settingsService;
        private bool isProcessing = false;

        public MainForm(AIService aiService, SettingsService settingsService)
        {
            InitializeComponent();
            _aiService = aiService;
            _settingsService = settingsService;
            SetupTrayIcon();
            SetupKeyboardHook();

            // Hide the main form
            this.WindowState = FormWindowState.Minimized;
            this.ShowInTaskbar = false;
            this.Visible = false;
        }

        private void SetupTrayIcon()
        {
            trayIcon = new NotifyIcon();
            trayIcon.Icon = SystemIcons.Information;
            trayIcon.Text = "AI Writing Assistant";
            trayIcon.Visible = true;

            var contextMenu = new ContextMenuStrip();
            contextMenu.Items.Add("Settings", null, ShowSettings);
            contextMenu.Items.Add("About", null, ShowAbout);
            contextMenu.Items.Add("-");
            contextMenu.Items.Add("Exit", null, ExitApplication);

            trayIcon.ContextMenuStrip = contextMenu;
            trayIcon.DoubleClick += ShowSettings;
        }

        private void SetupKeyboardHook()
        {
            keyboardHook = new GlobalKeyboardHook();
            keyboardHook.KeyDown += OnGlobalKeyDown;
        }

        private async void OnGlobalKeyDown(object? sender, GlobalKeyboardHookEventArgs e)
        {
            // Check for CTRL+SHIFT+Z
            if (e.KeyData == (Keys.Z | Keys.Control | Keys.Shift))
            {
                e.Handled = true;
                await TriggerWritingAssistant();
            }
        }

        private async Task TriggerWritingAssistant()
        {
            if (isProcessing) return;
            isProcessing = true;

            try
            {
                // Get selected text from clipboard
                string selectedText = GetSelectedText();

                if (string.IsNullOrWhiteSpace(selectedText))
                {
                    ShowNotification("No text selected", "Please select some text first.");
                    return;
                }

                // Show loading indicator
                ShowNotification("Processing...", "AI is analyzing your text...");

                // Get AI suggestions
                var suggestions = await _aiService.GetWritingSuggestions(selectedText);

                if (suggestions != null && suggestions.Any())
                {
                    // Show suggestion window
                    ShowSuggestionWindow(selectedText, suggestions);
                }
                else
                {
                    ShowNotification("No suggestions", "Your text looks good already!");
                }
            }
            catch (Exception ex)
            {
                ShowNotification("Error", $"Failed to process text: {ex.Message}");
            }
            finally
            {
                isProcessing = false;
            }
        }

        private string GetSelectedText()
        {
            try
            {
                // Copy selected text to clipboard
                SendKeys.SendWait("^c");
                System.Threading.Thread.Sleep(100);

                if (Clipboard.ContainsText())
                {
                    return Clipboard.GetText();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting selected text: {ex.Message}");
            }
            return string.Empty;
        }

        private void ShowSuggestionWindow(string originalText, List<WritingSuggestion> suggestions)
        {
            if (suggestionWindow != null)
            {
                suggestionWindow.Close();
            }

            suggestionWindow = new SuggestionWindow(originalText, suggestions);
            suggestionWindow.SuggestionSelected += OnSuggestionSelected;
            suggestionWindow.Show();
            suggestionWindow.BringToFront();
        }

        private void OnSuggestionSelected(object? sender, SuggestionSelectedEventArgs e)
        {
            try
            {
                // Copy selected suggestion to clipboard
                Clipboard.SetText(e.SelectedText);

                // Paste the suggestion
                SendKeys.SendWait("^v");

                ShowNotification("Applied", "Suggestion applied successfully!");
            }
            catch (Exception ex)
            {
                ShowNotification("Error", $"Failed to apply suggestion: {ex.Message}");
            }
        }

        private void ShowNotification(string title, string message)
        {
            trayIcon.ShowBalloonTip(3000, title, message, ToolTipIcon.Info);
        }

        private void ShowSettings(object? sender, EventArgs e)
        {
            var settingsForm = new SettingsForm(_settingsService);
            settingsForm.Show();
        }

        private void ShowAbout(object? sender, EventArgs e)
        {
            MessageBox.Show("AI Writing Assistant v1.0\n\nPress CTRL+SHIFT+Z to improve selected text.\n\nPowered by AI language models.",
                          "About", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ExitApplication(object? sender, EventArgs e)
        {
            keyboardHook?.Dispose();
            trayIcon.Visible = false;
            Application.Exit();
        }


    }
}

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
using AI_Writing_Assistant.Forms;
using AI_Writing_Assistant.Helpers;
using AI_Writing_Assistant.Services;

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
            // Check for CTRL+SHIFT+V
            else if (e.KeyData == (Keys.V | Keys.Control | Keys.Shift))
            {
                e.Handled = true;
                await TriggerTranslation();
            }
        }

        private async Task TriggerWritingAssistant()
        {
            if (isProcessing) return;
            isProcessing = true;
            SetWaitCursor();

            try
            {
                string selectedText = GetSelectedText();
                if (string.IsNullOrWhiteSpace(selectedText))
                {
                    return;
                }

                var suggestions = await _aiService.GetWritingSuggestions(selectedText);

                if (suggestions != null && suggestions.Any())
                {
                    var completionMode = _settingsService.GetCompletionMode();
                    if (completionMode == CompletionMode.Auto)
                    {
                        OnSuggestionSelected(this, new SuggestionSelectedEventArgs(suggestions.First().ImprovedText));
                    }
                    else
                    {
                        ShowSuggestionWindow(selectedText, suggestions);
                    }
                }
            }
            catch (Exception ex)
            {
                ShowNotification("Error", $"Failed to process text: {ex.Message}");
            }
            finally
            {
                isProcessing = false;
                SetDefaultCursor();
            }
        }

        private async Task TriggerTranslation()
        {
            if (isProcessing) return;
            isProcessing = true;
            SetWaitCursor();

            try
            {
                string selectedText = GetSelectedText();
                if (string.IsNullOrWhiteSpace(selectedText))
                {
                    return;
                }

                string translatedText = await _aiService.TranslateToVietnameseAsync(selectedText);

                if (!string.IsNullOrWhiteSpace(translatedText))
                {
                    ShowSuggestionWindow(translatedText);
                }
            }
            catch (Exception ex)
            {
                ShowNotification("Error", $"Translation failed: {ex.Message}");
            }
            finally
            {
                isProcessing = false;
                SetDefaultCursor();
            }
        }

        // Copy selected text to clipboard
        private string GetSelectedText()
        {
            try
            {
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

        private void ShowSuggestionWindow(string translatedText)
        {
            if (suggestionWindow != null)
            {
                suggestionWindow.Close();
            }

            suggestionWindow = new SuggestionWindow(translatedText);
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
            }
            catch (Exception ex)
            {
                ShowNotification("Error", $"Failed to apply suggestion: {ex.Message}");
            }
        }

        private void SetWaitCursor()
        {
            Cursor.Current = Cursors.AppStarting;
        }

        private void SetDefaultCursor()
        {
            Cursor.Current = Cursors.Default;
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
            MessageBox.Show("AI Writing Assistant v1.1\n\nPress CTRL+SHIFT+Z to improve selected text.\n\nPress CTRL+SHIFT+V to translate selected text.\n\nPowered by AI language models.",
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

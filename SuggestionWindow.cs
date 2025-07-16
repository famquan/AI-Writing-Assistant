using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text.Json.Serialization;
using System.Windows.Forms;

namespace AI_Writing_Assistant
{
    public class SuggestionWindow : Form
    {
        private readonly string originalText;
        private List<WritingSuggestion> suggestions;
        private Panel mainPanel;
        private ListBox suggestionsListBox;

        public event EventHandler<SuggestionSelectedEventArgs> SuggestionSelected;

        public SuggestionWindow(string original, List<WritingSuggestion> suggestions)
        {
            this.originalText = original;
            this.suggestions = new List<WritingSuggestion> { new WritingSuggestion { Type = "Original", ImprovedText = original, Reason = "Your original text." } };
            this.suggestions.AddRange(suggestions);
            InitializeComponents();
            LoadSuggestions();
        }

        public SuggestionWindow(string translatedText)
        {
            this.originalText = translatedText;
            this.suggestions = new List<WritingSuggestion> { new WritingSuggestion { Type = "Translation", ImprovedText = translatedText, Reason = "Translated text." } };
            InitializeComponents();
            LoadSuggestions();
            suggestionsListBox.KeyDown += SuggestionsListBox_KeyDown;
        }

        private void InitializeComponents()
        {
            // Window setup
            this.Text = "AI Assistant";
            this.TopMost = true;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // Main panel
            mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10)
            };

            suggestionsListBox = new ListBox
            {
                DrawMode = DrawMode.OwnerDrawVariable,
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.None,
                BackColor = Color.White ,
            };

            suggestionsListBox.MeasureItem += SuggestionsListBox_MeasureItem;
            suggestionsListBox.DrawItem += SuggestionsListBox_DrawItem;
            suggestionsListBox.DoubleClick += SuggestionsListBox_DoubleClick;
            suggestionsListBox.KeyDown += SuggestionsListBox_KeyDown;


            // Add controls to main panel
            mainPanel.Controls.Add(suggestionsListBox);

            this.Controls.Add(mainPanel);
        }

        private void SuggestionsListBox_DoubleClick(object? sender, EventArgs e)
        {
            if (suggestionsListBox.SelectedItem is WritingSuggestion selectedSuggestion)
            {
                OnSuggestionButtonClick(selectedSuggestion.ImprovedText);
            }
        }

        private void SuggestionsListBox_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && suggestionsListBox.SelectedItem is WritingSuggestion selectedSuggestion)
            {
                OnSuggestionButtonClick(selectedSuggestion.ImprovedText);
            }
            else if (e.KeyCode == Keys.Escape)
            {
                this.Close();
            }
        }

        private void SuggestionsListBox_MeasureItem(object sender, MeasureItemEventArgs e)
        {
            if (e.Index < 0 || e.Index >= suggestions.Count) return;
            var suggestion = suggestions[e.Index];
            var reasonHeight = (int)e.Graphics.MeasureString($"Reason: {suggestion.Reason}", new Font("Segoe UI", 8)).Height;
            var textHeight = (int)e.Graphics.MeasureString(suggestion.ImprovedText, new Font("Segoe UI", 9), 520).Height;
            e.ItemHeight = 60 + textHeight + reasonHeight;
        }

        private void SuggestionsListBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0 || e.Index >= suggestions.Count) return;

            var suggestion = suggestions[e.Index];
            e.DrawBackground();

            // Draw selection rectangle
            if ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
            {
                e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(0, 123, 255)), e.Bounds);
            }
            else
            {
                e.Graphics.FillRectangle(new SolidBrush(suggestionsListBox.BackColor), e.Bounds);
            }

            // Draw suggestion type
            var typeColor = GetTypeColor(suggestion.Type);
            var typeFont = new Font("Segoe UI", 9, FontStyle.Bold);
            var typeBrush = new SolidBrush(typeColor);
            e.Graphics.DrawString(suggestion.Type, typeFont, typeBrush, e.Bounds.Left + 10, e.Bounds.Top + 10);

            // Draw improved text
            var textFont = new Font("Segoe UI", 9);
            var textBrush = new SolidBrush((e.State & DrawItemState.Selected) == DrawItemState.Selected ? Color.White : Color.Black);
            var textRect = new Rectangle(e.Bounds.Left + 10, e.Bounds.Top + 30, e.Bounds.Width - 20, e.Bounds.Height - 40);
            e.Graphics.DrawString(suggestion.ImprovedText, textFont, textBrush, textRect);

            // Draw reason
            var reasonFont = new Font("Segoe UI", 8);
            var reasonBrush = new SolidBrush((e.State & DrawItemState.Selected) == DrawItemState.Selected ? Color.WhiteSmoke : Color.Gray);
            var reasonY = e.Bounds.Bottom - 25;
            e.Graphics.DrawString($"Reason: {suggestion.Reason}", reasonFont, reasonBrush, e.Bounds.Left + 10, reasonY);

            e.DrawFocusRectangle();
        }

        private void LoadSuggestions()
        {
            suggestionsListBox.Items.Clear();
            foreach (var suggestion in suggestions)
            {
                suggestionsListBox.Items.Add(suggestion);
            }
            if (suggestionsListBox.Items.Count > 0)
            {
                suggestionsListBox.SelectedIndex = 0;
            }
            AdjustWindowSize();
        }

        private Color GetTypeColor(string type)
        {
            switch (type.ToLower())
            {
                case "original": return Color.FromArgb(108, 117, 125);
                case "grammar": return Color.FromArgb(220, 53, 69);
                case "clarity": return Color.FromArgb(0, 123, 255);
                case "style": return Color.FromArgb(40, 167, 69);
                case "conciseness": return Color.FromArgb(255, 193, 7);
                default: return Color.DarkBlue;
            }
        }

        private void OnSuggestionButtonClick(string selectedText)
        {
            SuggestionSelected?.Invoke(this, new SuggestionSelectedEventArgs(selectedText));
            this.Close();
        }

        private void AdjustWindowSize()
        {
            int totalHeight = 0;
            using (Graphics g = suggestionsListBox.CreateGraphics())
            {
                for (int i = 0; i < suggestionsListBox.Items.Count; i++)
                {
                    var suggestion = (WritingSuggestion)suggestionsListBox.Items[i];
                    var reasonHeight = (int)g.MeasureString($"Reason: {suggestion.Reason}", new Font("Segoe UI", 8)).Height;
                    var textHeight = (int)g.MeasureString(suggestion.ImprovedText, new Font("Segoe UI", 9), 520).Height;
                    totalHeight += 60 + textHeight + reasonHeight;
                }
            }

            int newHeight = totalHeight + mainPanel.Padding.Top + mainPanel.Padding.Bottom + 50;
            int screenHeight = Screen.PrimaryScreen.WorkingArea.Height;
            this.Size = new Size(600, Math.Min(newHeight, screenHeight));
        }
    }

    public class WritingSuggestion
    {
        [JsonPropertyName("type")]
        public string? Type { get; set; }
        [JsonPropertyName("improved_text")]
        public string? ImprovedText { get; set; }
        [JsonPropertyName("reason")]
        public string? Reason { get; set; }
    }

    public class SuggestionSelectedEventArgs : EventArgs
    {
        public string SelectedText { get; }

        public SuggestionSelectedEventArgs(string selectedText)
        {
            SelectedText = selectedText;
        }
    }
}

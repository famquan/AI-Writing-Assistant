using System.Collections.Generic;
using System.Text.Json.Serialization;
using AI_Writing_Assistant.Forms;

namespace AI_Writing_Assistant.Models
{
    public class SuggestionsResponse
    {
        [JsonPropertyName("suggestions")]
        public List<WritingSuggestion>? Suggestions { get; set; }
    }

    public class TranslationResponse
    {
        [JsonPropertyName("translation")]
        public string? Translation { get; set; }
    }
}

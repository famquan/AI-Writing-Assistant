using System.Collections.Generic;
using System.Threading.Tasks;
using AI_Writing_Assistant.Forms;

namespace AI_Writing_Assistant.Services
{
    public interface IAiService
    {
        Task<List<WritingSuggestion>> GetWritingSuggestions(string text);
        Task<string> TranslateToVietnameseAsync(string text);
    }
}

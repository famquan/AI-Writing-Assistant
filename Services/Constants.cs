namespace AI_Writing_Assistant.Services
{
    public static class Constants
    {
        public const string DefaultWritingSystemPrompt = @"You are an English language assistant who helps improve grammar, style, and clarity. 
You must respond in JSON format with a single key 'suggestions' which is an array of objects. Each object should have the properties: 'type' (string), 'improved_text' (string), and 'reason' (string). 
Provide up to 3 suggestions. When improving content, revise the entire input as a single block of text rather than splitting it into multiple lines.";

        public const string DefaultTranslationSystemPrompt = @"You are a translation assistant. Translate the given text to Vietnamese.
You must respond in JSON format with a single key 'translation' which is a string.";
    }
}

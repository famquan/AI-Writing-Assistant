# AI Writing Assistant Documentation

## 1. Overview

The AI Writing Assistant is a .NET 8 WinForms application that helps users improve their writing by providing suggestions for grammar, style, and clarity. The application runs in the background and can be activated with a global hotkey (`CTRL+SHIFT+Z`).

## 2. Features

- **System Tray Integration**: The application runs as a system tray icon, providing access to settings, an about dialog, and an exit option.
- **Global Hotkeys**:
  - `CTRL+SHIFT+Z`: Triggers the writing assistant on the currently selected text.
  - `CTRL+SHIFT+V`: Translates the selected text to Vietnamese and copies it to the clipboard.
- **Multi-AI Support**: Integrates with both OpenAI and Gemini models for generating suggestions.
- **AI Service Factory**: Uses a factory pattern (`AiServiceFactory`) to dynamically select the AI service based on user settings.
- **Two Completion Modes**:
    - **Select Mode**: Displays a window with a list of suggestions for the user to choose from.
    - **Auto Mode**: Automatically replaces the selected text with the first suggestion.
- **Settings Management**: Persists API keys, completion mode, and selected AI service in a JSON file.

## 3. Project Structure

The solution is organized into several key components:

- **`Program.cs`**: The application's entry point, which configures the dependency injection container and starts the main form.
- **`MainForm.cs`**: The core of the application. It runs as a hidden window, manages the system tray icon, and handles global keyboard hooks.
- **`Services/`**:
    - **`IAiService.cs`**: Defines the common interface for all AI services.
    - **`OpenAiService.cs`**: Implements the `IAiService` interface for OpenAI models.
    - **`GeminiService.cs`**: Implements the `IAiService` interface for Gemini models.
    - **`AiServiceFactory.cs`**: A factory responsible for creating the appropriate AI service instance based on settings.
    - **`SettingsService.cs`**: Manages loading and saving application settings.
- **`Helpers/GlobalKeyboardHook.cs`**: Implements a low-level keyboard hook to capture global key presses.
- **`Forms/`**:
    - **`SuggestionWindow.cs`**: A form that displays writing suggestions to the user.
    - **`SettingsForm.cs`**: A form that allows the user to configure the application's settings, including the AI service, API key, and completion mode.

## 4. Workflow

1. The application starts and creates a `MainForm` instance, which is hidden from view. A system tray icon is displayed.
2. The `MainForm` sets up a `GlobalKeyboardHook` to listen for the `CTRL+SHIFT+Z` and `CTRL+SHIFT+V` hotkeys.
3. When a hotkey is pressed, the `OnGlobalKeyDown` event is triggered.
4. The `TriggerWritingAssistant` method is called, which gets the selected text from the clipboard.
5. The `MainForm` uses the `IAiService` (created by the `AiServiceFactory`) to process the text.
6. The selected AI service (`OpenAiService` or `GeminiService`) sends a request to the corresponding API.
7. The service receives a JSON response with writing suggestions and parses it.
7. Depending on the `CompletionMode` setting:
    - **`Auto`**: The first suggestion is automatically copied to the clipboard and pasted, replacing the original text.
    - **`Select`**: A `SuggestionWindow` is shown, displaying the suggestions. When the user selects a suggestion, it is copied to the clipboard and pasted.
8. The cursor changes to a waiting icon during processing and reverts to the default when the operation is complete.

## 5. Dependencies

- **`Microsoft.Extensions.Http`**: Used for making HTTP requests to the OpenAI API.
- **`System.Text.Json`**: Used for serializing and deserializing JSON data.

## 6. Setup and Configuration

1.  **AI Service Selection**: In the settings, the user can choose between the `OpenAI` and `Gemini` services.
2.  **API Key**: The user must provide a valid API key for the selected service. If no key is provided, the application will use a fallback mechanism that provides a basic text-cleaning suggestion.
3.  **System Prompts**: The user can customize the system prompts for both writing suggestions and translations to tailor the AI's behavior.
4.  **Settings File**: The application settings are stored in `C:\Users\<YourUsername>\AppData\Roaming\AI-Writing-Assistant\settings.json`.

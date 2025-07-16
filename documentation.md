# AI Writing Assistant Documentation

## 1. Overview

The AI Writing Assistant is a .NET 8 WinForms application that helps users improve their writing by providing suggestions for grammar, style, and clarity. The application runs in the background and can be activated with a global hotkey (`CTRL+SHIFT+Z`).

## 2. Features

- **System Tray Integration**: The application runs as a system tray icon, providing access to settings, an about dialog, and an exit option.
- **Global Hotkeys**:
  - `CTRL+SHIFT+Z`: Triggers the writing assistant on the currently selected text.
  - `CTRL+SHIFT+V`: Translates the selected text to Vietnamese and copies it to the clipboard.
- **AI-Powered Suggestions**: Uses OpenAI's `gpt-4.1-mini` model to provide writing suggestions.
- **Two Completion Modes**:
    - **Select Mode**: Displays a window with a list of suggestions for the user to choose from.
    - **Auto Mode**: Automatically replaces the selected text with the first suggestion.
- **Settings Management**: Persists the OpenAI API key and completion mode settings in a JSON file.

## 3. Project Structure

The solution is organized into several key components:

- **`Program.cs`**: The application's entry point, which sets up the dependency injection container and starts the main form.
- **`MainForm.cs`**: The core of the application. It runs as a hidden window, manages the system tray icon, and handles the global keyboard hook.
- **`AIService.cs`**: Communicates with the OpenAI API to fetch writing suggestions.
- **`SettingsService.cs`**: Manages loading and saving the application's settings.
- **`GlobalKeyboardHook.cs`**: Implements a low-level keyboard hook to capture global key presses.
- **`SuggestionWindow.cs`**: A form that displays the writing suggestions to the user.
- **`SettingsForm.cs`**: A form that allows the user to configure the application's settings.

## 4. Workflow

1. The application starts and creates a `MainForm` instance, which is hidden from view. A system tray icon is displayed.
2. The `MainForm` sets up a `GlobalKeyboardHook` to listen for the `CTRL+SHIFT+Z` and `CTRL+SHIFT+V` hotkeys.
3. When a hotkey is pressed, the `OnGlobalKeyDown` event is triggered.
4. The `TriggerWritingAssistant` method is called, which gets the selected text from the clipboard.
5. The selected text is passed to the `AIService`, which sends a request to the OpenAI API.
6. The `AIService` receives a JSON response with writing suggestions and parses it.
7. Depending on the `CompletionMode` setting:
    - **`Auto`**: The first suggestion is automatically copied to the clipboard and pasted, replacing the original text.
    - **`Select`**: A `SuggestionWindow` is shown, displaying the suggestions. When the user selects a suggestion, it is copied to the clipboard and pasted.
8. The cursor changes to a waiting icon during processing and reverts to the default when the operation is complete.

## 5. Dependencies

- **`Microsoft.Extensions.Http`**: Used for making HTTP requests to the OpenAI API.
- **`System.Text.Json`**: Used for serializing and deserializing JSON data.

## 6. Setup and Configuration

1.  **API Key**: The user must provide a valid OpenAI API key in the settings for the AI features to work. If no key is provided, the application will use a fallback mechanism that provides a basic text-cleaning suggestion.
2.  **Settings File**: The application settings are stored in `C:\Users\<YourUsername>\AppData\Roaming\AI-Writing-Assistant\settings.json`.

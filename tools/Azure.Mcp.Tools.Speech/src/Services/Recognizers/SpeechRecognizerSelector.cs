// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using Azure.Mcp.Tools.Speech.Models;

namespace Azure.Mcp.Tools.Speech.Services.Recognizers;

/// <summary>
/// Utility class for selecting the appropriate speech recognizer based on input parameters.
/// </summary>
public static class SpeechRecognizerSelector
{
    /// <summary>
    /// Gets the recommended speech recognizer type based on the provided parameters.
    /// </summary>
    /// <param name="filePath">Path to the audio file</param>
    /// <param name="language">The target language for transcription</param>
    /// <param name="phrases">Phrase list for improved recognition</param>
    /// <param name="format">Output format specification</param>
    /// <returns>Recommended speech recognizer type</returns>
    /// <exception cref="NotSupportedException">Thrown when the language is not supported by any recognizer</exception>
    public static RecognizerType GetSpeechRecognizer(string filePath, string? language, string[]? phrases, string? format)
    {
        // If language not provided, use Fast Transcription Multi-lingual model
        if (string.IsNullOrWhiteSpace(language))
        {
            return RecognizerType.Fast;
        }

        // if format is specified, we must use Realtime as Fast Transcription does not support format options
        if (!string.IsNullOrEmpty(format))
        {
            return RecognizerType.Realtime;
        }

        // Check file size constraints (Fast Transcription has a 300 MB limit)
        if (File.Exists(filePath))
        {
            var fileInfo = new FileInfo(filePath);
            if (fileInfo.Length > 300 * 1024 * 1024) // 300 MB
            {
                return RecognizerType.Realtime; // File too large for Fast Transcription, use Realtime
            }
        }

        // Map language to supported locale
        var locale = LocaleSupport.MapLanguageToValidLocale(language);
        var hasPhrases = phrases != null && phrases.Length > 0;

        // If language is provided and supported by Fast Transcription
        if (locale != null && LocaleSupport.IsSupportedInFastTranscription(locale))
        {
            // If phrases are provided but not supported in Fast Transcription, use Realtime
            if (hasPhrases && !LocaleSupport.IsPhraseListSupportedInFastTranscription(locale))
            {
                return RecognizerType.Realtime;
            }

            return RecognizerType.Fast;
        }
        else if (LocaleSupport.IsSupportedInRealtimeTranscription(language))
        {
            return RecognizerType.Realtime;
        }

        throw new NotSupportedException($"Language '{language}' is not supported by either Fast Transcription or Realtime Speech recognizer.");
    }
}

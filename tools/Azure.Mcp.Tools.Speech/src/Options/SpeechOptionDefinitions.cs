// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Speech.Options;

public static class SpeechOptionDefinitions
{
    public const string EndpointName = "endpoint";
    public const string FileName = "file";
    public const string LanguageName = "language";
    public const string PhrasesName = "phrases";
    public const string FormatName = "format";
    public const string ProfanityName = "profanity";
    public const string TextName = "text";
    public const string VoiceName = "voice";
    public const string EndpointIdName = "endpointId";

    public static readonly Option<string> Endpoint = new(
        $"--{EndpointName}")
    {
        Description = "The Azure AI Services endpoint URL (e.g., https://your-service.cognitiveservices.azure.com/).",
        Required = true
    };

    public static readonly Option<string> File = new(
        $"--{FileName}")
    {
        Description = "Path to the audio file to recognize.",
        Required = true
    };

    public static readonly Option<string?> Language = new(
        $"--{LanguageName}")
    {
        Description = "The language for speech recognition (e.g., en-US, es-ES). Default is en-US."
    };

    public static readonly Option<string[]?> Phrases = new(
        $"--{PhrasesName}")
    {
        Description = "Phrase hints to improve recognition accuracy. Can be specified multiple times (--phrases \"phrase1\" --phrases \"phrase2\") or as comma-separated values (--phrases \"phrase1,phrase2\").",
        AllowMultipleArgumentsPerToken = true
    };

    public static readonly Option<string?> Format = new(
        $"--{FormatName}")
    {
        Description = "Output format: simple or detailed."
    };

    public static readonly Option<string?> Profanity = new(
        $"--{ProfanityName}")
    {
        Description = "Profanity filter: masked, removed, or raw. Default is masked."
    };

    public static readonly Option<string> Text = new(
        $"--{TextName}")
    {
        Description = "The text to convert to speech.",
        Required = true
    };

    public static readonly Option<string?> Voice = new(
        $"--{VoiceName}")
    {
        Description = "The voice to use for speech synthesis (e.g., en-US-JennyNeural). If not specified, the default voice for the language will be used."
    };

    public static readonly Option<string?> EndpointId = new(
        $"--{EndpointIdName}")
    {
        Description = "The endpoint ID of a custom voice model for speech synthesis."
    };
}

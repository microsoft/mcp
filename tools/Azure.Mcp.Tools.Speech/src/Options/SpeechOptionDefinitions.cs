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
        Description = "Output format: simple or detailed. Default is simple."
    };

    public static readonly Option<string?> Profanity = new(
        $"--{ProfanityName}")
    {
        Description = "Profanity filter: masked, removed, or raw. Default is masked."
    };
}

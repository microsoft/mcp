// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Speech.Options.Stt;

public class SttRecognizeOptions : BaseSpeechOptions
{
    [JsonPropertyName(SpeechOptionDefinitions.FileName)]
    public string? File { get; set; }

    [JsonPropertyName(SpeechOptionDefinitions.LanguageName)]
    public string? Language { get; set; }

    [JsonPropertyName(SpeechOptionDefinitions.PhrasesName)]
    public string[]? Phrases { get; set; }

    [JsonPropertyName(SpeechOptionDefinitions.FormatName)]
    public string? Format { get; set; }

    [JsonPropertyName(SpeechOptionDefinitions.ProfanityName)]
    public string? Profanity { get; set; }
}

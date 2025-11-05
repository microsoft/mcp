// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Speech.Models.FastTranscription;

/// <summary>
/// Request configuration for Fast Transcription API.
/// </summary>
public record FastTranscriptionRequest
{
    /// <summary>
    /// The list of locales that should match the expected locale of the audio data.
    /// </summary>
    [JsonPropertyName("locales")]
    public List<string> Locales { get; set; } = new();

    /// <summary>
    /// Specifies how to handle profanity in recognition results.
    /// </summary>
    [JsonPropertyName("profanityFilterMode")]
    public string? ProfanityFilterMode { get; set; }

    /// <summary>
    /// The list of phrases to improve recognition accuracy separated by semicolon.
    /// </summary>
    [JsonPropertyName("phraseList")]
    public string? PhraseList { get; set; }

    /// <summary>
    /// The channels to transcribe (0-based indices).
    /// </summary>
    [JsonPropertyName("channels")]
    public List<int>? Channels { get; set; }

    /// <summary>
    /// Whether to enable diarization (speaker identification).
    /// </summary>
    [JsonPropertyName("diarization")]
    public FastTranscriptionDiarization? Diarization { get; set; }
}

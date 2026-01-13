// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Speech.Models.FastTranscription;

/// <summary>
/// Represents an individual phrase in the Fast Transcription response.
/// </summary>
public record FastTranscriptionPhrase
{
    /// <summary>
    /// The offset of this phrase in milliseconds.
    /// </summary>
    [JsonPropertyName("offsetMilliseconds")]
    public int OffsetMilliseconds { get; set; }

    /// <summary>
    /// The duration of this phrase in milliseconds.
    /// </summary>
    [JsonPropertyName("durationMilliseconds")]
    public int DurationMilliseconds { get; set; }

    /// <summary>
    /// The transcribed text for this phrase.
    /// </summary>
    [JsonPropertyName("text")]
    public string? Text { get; set; }

    /// <summary>
    /// The words that make up this phrase.
    /// </summary>
    [JsonPropertyName("words")]
    public List<FastTranscriptionWord> Words { get; set; } = new();

    /// <summary>
    /// The locale of the recognized speech.
    /// </summary>
    [JsonPropertyName("locale")]
    public string? Locale { get; set; }

    /// <summary>
    /// The confidence score of the recognition (0.0 to 1.0).
    /// </summary>
    [JsonPropertyName("confidence")]
    public double Confidence { get; set; }
}

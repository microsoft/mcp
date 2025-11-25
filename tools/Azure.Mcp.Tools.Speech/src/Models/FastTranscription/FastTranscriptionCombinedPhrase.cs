// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Speech.Models.FastTranscription;

/// <summary>
/// Represents a combined phrase in the Fast Transcription response.
/// </summary>
public record FastTranscriptionCombinedPhrase
{
    /// <summary>
    /// The channel identifier (0-based index).
    /// </summary>
    [JsonPropertyName("channel")]
    public int Channel { get; set; }

    /// <summary>
    /// The transcribed text for this phrase.
    /// </summary>
    [JsonPropertyName("text")]
    public string? Text { get; set; }

    /// <summary>
    /// The locale of the recognized speech.
    /// </summary>
    [JsonPropertyName("locale")]
    public string? Locale { get; set; }

    /// <summary>
    /// The confidence score of the recognition (0.0 to 1.0).
    /// </summary>
    [JsonPropertyName("confidence")]
    public double? Confidence { get; set; }

    /// <summary>
    /// The phrases that make up this combined phrase.
    /// </summary>
    [JsonPropertyName("phrases")]
    public List<FastTranscriptionPhrase> Phrases { get; set; } = new();
}

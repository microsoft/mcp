// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Speech.Models.FastTranscription;

/// <summary>
/// Represents a word in the Fast Transcription response.
/// </summary>
public record FastTranscriptionWord
{
    /// <summary>
    /// The word text.
    /// </summary>
    [JsonPropertyName("text")]
    public string? Text { get; set; }

    /// <summary>
    /// The offset of this word in milliseconds.
    /// </summary>
    [JsonPropertyName("offsetMilliseconds")]
    public int OffsetMilliseconds { get; set; }

    /// <summary>
    /// The duration of this word in milliseconds.
    /// </summary>
    [JsonPropertyName("durationMilliseconds")]
    public int DurationMilliseconds { get; set; }

    /// <summary>
    /// The confidence score of this word (0.0 to 1.0).
    /// </summary>
    [JsonPropertyName("confidence")]
    public double Confidence { get; set; }
}

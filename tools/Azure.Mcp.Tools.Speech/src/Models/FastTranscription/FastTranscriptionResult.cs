// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Speech.Models.FastTranscription;

/// <summary>
/// Represents the response from the Fast Transcription API.
/// </summary>
public record FastTranscriptionResult
{
    /// <summary>
    /// The duration of the audio in milliseconds.
    /// </summary>
    [JsonPropertyName("durationMilliseconds")]
    public int DurationMilliseconds { get; set; }

    /// <summary>
    /// The combined phrases containing the transcription results.
    /// </summary>
    [JsonPropertyName("combinedPhrases")]
    public List<FastTranscriptionCombinedPhrase> CombinedPhrases { get; set; } = new();
}

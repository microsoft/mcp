// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Speech.Models.FastTranscription;

/// <summary>
/// Diarization configuration for Fast Transcription.
/// </summary>
public record FastTranscriptionDiarization
{
    /// <summary>
    /// Whether to enable speaker identification.
    /// </summary>
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; }

    /// <summary>
    /// The minimum number of speakers.
    /// </summary>
    [JsonPropertyName("minSpeakers")]
    public int? MinSpeakers { get; set; }

    /// <summary>
    /// The maximum number of speakers.
    /// </summary>
    [JsonPropertyName("maxSpeakers")]
    public int? MaxSpeakers { get; set; }
}

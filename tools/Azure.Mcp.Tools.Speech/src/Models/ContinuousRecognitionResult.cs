// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Speech.Models;

public record ContinuousRecognitionResult
{
    [JsonPropertyName("fullText")]
    public string? FullText { get; set; }

    [JsonPropertyName("segments")]
    public List<SpeechRecognitionResult> Segments { get; set; } = new();
}

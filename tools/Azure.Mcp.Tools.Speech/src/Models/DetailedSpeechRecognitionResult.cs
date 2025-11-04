// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Speech.Models;

public record DetailedSpeechRecognitionResult : SpeechRecognitionResult
{
    [JsonPropertyName("nBest")]
    public List<NBestResult>? NBest { get; set; }
}

// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Speech.Models.Realtime;

public record RealtimeRecognitionContinuousResult
{
    [JsonPropertyName("fullText")]
    public string? FullText { get; set; }

    [JsonPropertyName("segments")]
    public List<RealtimeRecognitionResult> Segments { get; set; } = new();
}

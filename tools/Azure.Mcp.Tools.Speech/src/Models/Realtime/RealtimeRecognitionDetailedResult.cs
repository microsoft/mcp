// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Speech.Models.Realtime;

public record RealtimeRecognitionDetailedResult : RealtimeRecognitionResult
{
    [JsonPropertyName("nBest")]
    public List<RealtimeRecognitionNBestResult>? NBest { get; set; }
}

// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Speech.Models.Realtime;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
[JsonDerivedType(typeof(RealtimeRecognitionResult), "simple")]
[JsonDerivedType(typeof(RealtimeRecognitionDetailedResult), "detailed")]
public record RealtimeRecognitionResult
{
    [JsonPropertyName("text")]
    public string? Text { get; set; }

    [JsonPropertyName("offset")]
    public ulong? Offset { get; set; }

    [JsonPropertyName("duration")]
    public ulong? Duration { get; set; }

    [JsonPropertyName("language")]
    public string? Language { get; set; }

    [JsonPropertyName("reason")]
    public string? Reason { get; set; }
}

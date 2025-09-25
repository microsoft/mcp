// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Speech.Models;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
[JsonDerivedType(typeof(SpeechRecognitionResult), "simple")]
[JsonDerivedType(typeof(DetailedSpeechRecognitionResult), "detailed")]
public record SpeechRecognitionResult
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

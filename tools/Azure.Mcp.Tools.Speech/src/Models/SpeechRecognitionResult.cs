// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Speech.Models;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
[JsonDerivedType(typeof(SpeechRecognitionResult), "simple")]
[JsonDerivedType(typeof(DetailedSpeechRecognitionResult), "detailed")]
public class SpeechRecognitionResult
{
    [JsonPropertyName("text")]
    public string? Text { get; set; }

    [JsonPropertyName("confidence")]
    public double? Confidence { get; set; }

    [JsonPropertyName("offset")]
    public ulong? Offset { get; set; }

    [JsonPropertyName("duration")]
    public ulong? Duration { get; set; }

    [JsonPropertyName("language")]
    public string? Language { get; set; }

    [JsonPropertyName("reason")]
    public string? Reason { get; set; }
}

public class DetailedSpeechRecognitionResult : SpeechRecognitionResult
{
    [JsonPropertyName("nBest")]
    public List<NBestResult>? NBest { get; set; }
}

public class NBestResult
{
    [JsonPropertyName("text")]
    public string? Text { get; set; }

    [JsonPropertyName("confidence")]
    public double? Confidence { get; set; }

    [JsonPropertyName("words")]
    public List<WordResult>? Words { get; set; }
}

public class WordResult
{
    [JsonPropertyName("word")]
    public string? Word { get; set; }

    [JsonPropertyName("offset")]
    public ulong? Offset { get; set; }

    [JsonPropertyName("duration")]
    public ulong? Duration { get; set; }

    [JsonPropertyName("confidence")]
    public double? Confidence { get; set; }
}

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

public record ContinuousRecognitionResult
{
    [JsonPropertyName("fullText")]
    public string? FullText { get; set; }

    [JsonPropertyName("segments")]
    public List<SpeechRecognitionResult> Segments { get; set; } = new();
}

public record DetailedSpeechRecognitionResult : SpeechRecognitionResult
{
    [JsonPropertyName("nBest")]
    public List<NBestResult>? NBest { get; set; }
}

public record NBestResult
{
    [JsonPropertyName("confidence")]
    public double? Confidence { get; set; }

    [JsonPropertyName("lexical")]
    public string? Lexical { get; set; }

    [JsonPropertyName("itn")]
    public string? ITN { get; set; }

    [JsonPropertyName("maskedITN")]
    public string? MaskedITN { get; set; }

    [JsonPropertyName("display")]
    public string? Display { get; set; }

    [JsonPropertyName("words")]
    public List<WordResult>? Words { get; set; }
}

public record WordResult
{
    [JsonPropertyName("word")]
    public string? Word { get; set; }

    [JsonPropertyName("offset")]
    public ulong? Offset { get; set; }

    [JsonPropertyName("duration")]
    public ulong? Duration { get; set; }
}

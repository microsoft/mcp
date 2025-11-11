// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Speech.Options.Tts;

public class TtsSynthesizeOptions : BaseSpeechOptions
{
    [JsonPropertyName(SpeechOptionDefinitions.TextName)]
    public string? Text { get; set; }

    [JsonPropertyName(SpeechOptionDefinitions.OutputAudioName)]
    public string? OutputAudio { get; set; }

    [JsonPropertyName(SpeechOptionDefinitions.LanguageName)]
    public string? Language { get; set; }

    [JsonPropertyName(SpeechOptionDefinitions.VoiceName)]
    public string? Voice { get; set; }

    [JsonPropertyName(SpeechOptionDefinitions.FormatName)]
    public string? Format { get; set; }

    [JsonPropertyName(SpeechOptionDefinitions.EndpointIdName)]
    public string? EndpointId { get; set; }
}

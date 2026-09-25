// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Speech.Options.Tts;

public sealed class TtsSynthesizeOptions : BaseSpeechOptions
{
    [Option(Description = "The text to convert to speech.")]
    public required string Text { get; set; }

    [Option(Name = "outputAudio", Description = "Path where the synthesized audio file will be saved.")]
    public required string OutputAudio { get; set; }

    [Option(Description = SpeechOptionDescriptions.Language)]
    public string? Language { get; set; }

    [Option(Description = "The voice to use for speech synthesis (e.g., en-US-JennyNeural). If not specified, the default voice for the language will be used.")]
    public string? Voice { get; set; }

    [Option(Description = SpeechOptionDescriptions.Format)]
    public string? Format { get; set; }

    [Option(Name = "endpointId", Description = "The endpoint ID of a custom voice model for speech synthesis.")]
    public string? EndpointId { get; set; }
}

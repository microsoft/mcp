// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Speech.Options.Stt;

public sealed class SttRecognizeOptions : BaseSpeechOptions
{
    [Option(Description = "Path to the audio file to recognize.")]
    public required string File { get; set; }

    [Option(Description = SpeechOptionDescriptions.Language)]
    public string? Language { get; set; }

    [Option(Description = "Phrase hints to improve recognition accuracy. Can be specified multiple times (--phrases \"phrase1\" --phrases \"phrase2\") or as comma-separated values (--phrases \"phrase1,phrase2\").")]
    public string[]? Phrases { get; set; }

    [Option(Description = SpeechOptionDescriptions.Format)]
    public string? Format { get; set; }

    [Option(Description = "Profanity filter: masked, removed, or raw. Default is masked.")]
    public string? Profanity { get; set; }
}

// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Tools.Speech.Commands.Stt;
using Azure.Mcp.Tools.Speech.Models;

[JsonSerializable(typeof(ContinuousRecognitionResult))]
[JsonSerializable(typeof(DetailedSpeechRecognitionResult))]
[JsonSerializable(typeof(NBestResult))]
[JsonSerializable(typeof(SpeechRecognitionResult))]
[JsonSerializable(typeof(SttRecognizeCommand.SttRecognizeCommandResult))]
[JsonSerializable(typeof(WordResult))]
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    WriteIndented = true,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
internal partial class SpeechJsonContext : JsonSerializerContext;

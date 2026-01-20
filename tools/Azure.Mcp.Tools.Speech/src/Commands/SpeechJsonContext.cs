// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Tools.Speech.Commands.Stt;
using Azure.Mcp.Tools.Speech.Commands.Tts;
using Azure.Mcp.Tools.Speech.Models;
using Azure.Mcp.Tools.Speech.Models.FastTranscription;
using Azure.Mcp.Tools.Speech.Models.Realtime;

[JsonSerializable(typeof(RealtimeRecognitionContinuousResult))]
[JsonSerializable(typeof(RealtimeRecognitionDetailedResult))]
[JsonSerializable(typeof(RealtimeRecognitionNBestResult))]
[JsonSerializable(typeof(RealtimeRecognitionWordResult))]
[JsonSerializable(typeof(RealtimeRecognitionResult))]

[JsonSerializable(typeof(FastTranscriptionDiarization))]
[JsonSerializable(typeof(FastTranscriptionPhrase))]
[JsonSerializable(typeof(FastTranscriptionRequest))]
[JsonSerializable(typeof(FastTranscriptionResult))]
[JsonSerializable(typeof(FastTranscriptionWord))]
[JsonSerializable(typeof(FastTranscriptionCombinedPhrase))]

[JsonSerializable(typeof(RecognizerType))]
[JsonSerializable(typeof(SpeechRecognitionResult))]

[JsonSerializable(typeof(SttRecognizeCommand.SttRecognizeCommandResult))]
[JsonSerializable(typeof(SynthesisResult))]
[JsonSerializable(typeof(TtsSynthesizeCommand.TtsSynthesizeCommandResult))]
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
internal partial class SpeechJsonContext : JsonSerializerContext;

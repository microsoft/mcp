// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Tools.Speech.Models.FastTranscription;
using Azure.Mcp.Tools.Speech.Models.Realtime;

namespace Azure.Mcp.Tools.Speech.Models;

/// <summary>
/// A union-like model that can contain results from either real-time continuous recognition
/// or fast transcription, providing a unified interface for different speech recognition results.
/// </summary>
public record SpeechRecognitionResult
{
    /// <summary>
    /// Result from real-time continuous recognition, if applicable.
    /// </summary>
    [JsonPropertyName("realtimeContinuousResult")]
    public RealtimeRecognitionContinuousResult? RealtimeContinuousResult { get; set; }

    /// <summary>
    /// Result from fast transcription, if applicable.
    /// </summary>
    [JsonPropertyName("fastTranscriptionResult")]
    public FastTranscriptionResult? FastTranscriptionResult { get; set; }

    /// <summary>
    /// The type of recognizer that produced this result.
    /// </summary>
    [JsonPropertyName("recognizerType")]
    public RecognizerType RecognizerType { get; set; }

    /// <summary>
    /// Gets the transcribed text based on the recognizer type.
    /// </summary>
    [JsonIgnore]
    public string? Text =>
        RecognizerType switch
        {
            RecognizerType.Realtime => RealtimeContinuousResult?.FullText,
            RecognizerType.Fast => FastTranscriptionResult?.CombinedPhrases?.FirstOrDefault()?.Text,
            _ => null
        };

    /// <summary>
    /// Gets the duration of the audio in milliseconds, if available.
    /// </summary>
    [JsonIgnore]
    public int? DurationMilliseconds =>
        RecognizerType switch
        {
            RecognizerType.Fast => FastTranscriptionResult?.DurationMilliseconds,
            RecognizerType.Realtime => null, // Real-time doesn't provide total duration
            _ => null
        };

    /// <summary>
    /// Gets the number of segments/phrases in the result.
    /// </summary>
    [JsonIgnore]
    public int SegmentCount =>
        RecognizerType switch
        {
            RecognizerType.Realtime => RealtimeContinuousResult?.Segments?.Count ?? 0,
            RecognizerType.Fast => FastTranscriptionResult?.CombinedPhrases?.Count ?? 0,
            _ => 0
        };

    /// <summary>
    /// Creates a unified SpeechRecognitionResult from a real-time continuous recognition result.
    /// </summary>
    /// <param name="realtimeResult">The real-time continuous recognition result</param>
    /// <returns>A unified SpeechRecognitionResult containing the real-time result</returns>
    public static SpeechRecognitionResult FromRealtimeResult(RealtimeRecognitionContinuousResult realtimeResult)
    {
        return new SpeechRecognitionResult
        {
            RealtimeContinuousResult = realtimeResult,
            RecognizerType = RecognizerType.Realtime
        };
    }

    /// <summary>
    /// Creates a unified SpeechRecognitionResult from a fast transcription result.
    /// </summary>
    /// <param name="fastResult">The fast transcription result</param>
    /// <returns>A unified SpeechRecognitionResult containing the fast transcription result</returns>
    public static SpeechRecognitionResult FromFastTranscriptionResult(FastTranscriptionResult fastResult)
    {
        return new SpeechRecognitionResult
        {
            FastTranscriptionResult = fastResult,
            RecognizerType = RecognizerType.Fast
        };
    }
}

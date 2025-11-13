// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.Speech.Models.Realtime;

namespace Azure.Mcp.Tools.Speech.Services.Recognizers;

/// <summary>
/// Interface for real-time speech transcription recognizer using Azure AI Services Speech SDK.
/// </summary>
public interface IRealtimeTranscriptionRecognizer
{
    /// <summary>
    /// Recognizes speech from an audio file using Azure AI Services Speech with continuous recognition,
    /// capturing individual segments for detailed analysis.
    /// </summary>
    /// <param name="endpoint">Azure AI Services endpoint (e.g., https://your-service.cognitiveservices.azure.com/)</param>
    /// <param name="filePath">Path to the audio file to process</param>
    /// <param name="language">Speech recognition language (default: en-US)</param>
    /// <param name="phrases">Optional phrases to improve recognition accuracy</param>
    /// <param name="format">Output format (simple or detailed)</param>
    /// <param name="profanity">Profanity filtering option (masked, removed, or raw)</param>
    /// <param name="retryPolicy">Optional retry policy for resilience</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>Continuous recognition result containing full text and individual segments</returns>
    Task<RealtimeRecognitionContinuousResult> RecognizeAsync(
        string endpoint,
        string filePath,
        string? language = null,
        string[]? phrases = null,
        string? format = null,
        string? profanity = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);
}

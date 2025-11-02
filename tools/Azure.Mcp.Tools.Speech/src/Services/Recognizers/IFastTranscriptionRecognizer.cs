// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.Speech.Models.FastTranscription;

namespace Azure.Mcp.Tools.Speech.Services.Recognizers;

/// <summary>
/// Interface for Fast Transcription recognizer using Azure AI Services Speech REST API.
/// </summary>
public interface IFastTranscriptionRecognizer
{
    /// <summary>
    /// Transcribes audio using the Fast Transcription API.
    /// </summary>
    /// <param name="endpoint">Azure AI Services endpoint</param>
    /// <param name="filePath">Path to the audio file to process</param>
    /// <param name="language">Speech recognition language</param>
    /// <param name="phrases">Optional phrases to improve recognition accuracy</param>
    /// <param name="profanity">Profanity filtering option</param>
    /// <param name="retryPolicy">Optional retry policy for resilience</param>
    /// <returns>Continuous recognition result converted from Fast Transcription response</returns>
    Task<FastTranscriptionResult> RecognizeAsync(
        string endpoint,
        string filePath,
        string? language = null,
        string[]? phrases = null,
        string? profanity = null,
        RetryPolicyOptions? retryPolicy = null);
}

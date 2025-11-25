// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.Speech.Models;

namespace Azure.Mcp.Tools.Speech.Services.Synthesizers;

/// <summary>
/// Interface for speech synthesis services.
/// </summary>
public interface IRealtimeTtsSynthesizer
{
    /// <summary>
    /// Synthesizes speech from text and saves it to an audio file.
    /// </summary>
    /// <param name="endpoint">Azure AI Services endpoint</param>
    /// <param name="text">The text to convert to speech</param>
    /// <param name="outputFilePath">Path where the audio file will be saved</param>
    /// <param name="language">Language for synthesis (default: en-US)</param>
    /// <param name="voice">Voice name to use (e.g., en-US-JennyNeural)</param>
    /// <param name="format">Output audio format (default: Riff24Khz16BitMonoPcm)</param>
    /// <param name="endpointId">Optional endpoint ID for custom voice model</param>
    /// <param name="retryPolicy">Optional retry policy for resilience</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>Synthesis result with file information</returns>
    Task<SynthesisResult> SynthesizeToFileAsync(
        string endpoint,
        string text,
        string outputFilePath,
        string? language = null,
        string? voice = null,
        string? format = null,
        string? endpointId = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);
}

// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Core;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Tools.Speech.Models;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.Speech.Services.Synthesizers;

/// <summary>
/// Neural speech synthesizer using Azure AI Services Speech SDK.
/// Implements streaming synthesis for efficient memory management with large texts.
/// </summary>
public class RealtimeTtsSynthesizer(ITenantService tenantService, ILogger<RealtimeTtsSynthesizer> logger)
    : BaseAzureService(tenantService), IRealtimeTtsSynthesizer
{
    private readonly ILogger<RealtimeTtsSynthesizer> _logger = logger;

    /// <inheritdoc/>
    public async Task<SynthesisResult> SynthesizeToFileAsync(
        string endpoint,
        string text,
        string outputFilePath,
        string? language = null,
        string? voice = null,
        string? format = null,
        string? endpointId = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters((nameof(endpoint), endpoint), (nameof(text), text), (nameof(outputFilePath), outputFilePath));

        if (string.IsNullOrWhiteSpace(text))
        {
            throw new ArgumentException("Text cannot be empty or whitespace.", nameof(text));
        }

        try
        {
            // Use the reusable streaming synthesis method
            var (audioData, actualVoice) = await SynthesizeSpeechToStreamAsync(
                endpoint, text, language, voice, format, endpointId, cancellationToken);

            // Write the complete audio data to file
            await File.WriteAllBytesAsync(outputFilePath, audioData, cancellationToken);

            _logger.LogInformation(
                "Speech synthesized and saved to file: {OutputFile}, Audio size: {AudioSize} bytes",
                outputFilePath,
                audioData.Length);

            return new SynthesisResult
            {
                FilePath = outputFilePath,
                AudioSize = audioData.Length,
                Format = format ?? "Riff24Khz16BitMonoPcm",
                Voice = actualVoice,
                Language = language ?? "en-US"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during speech synthesis.");

            // Clean up partial file on error
            if (File.Exists(outputFilePath))
            {
                try
                {
                    File.Delete(outputFilePath);
                    _logger.LogInformation("Cleaned up partial output file after error: {OutputFile}", outputFilePath);
                }
                catch (Exception cleanupEx)
                {
                    _logger.LogWarning(cleanupEx, "Failed to clean up partial output file: {OutputFile}", outputFilePath);
                }
            }

            throw;
        }
    }

    /// <summary>
    /// Synthesizes speech from text and returns the audio data as a byte array.
    /// This method uses push stream to collect audio data during synthesis for efficient memory management.
    /// </summary>
    private async Task<(byte[] AudioData, string Voice)> SynthesizeSpeechToStreamAsync(
        string endpoint,
        string text,
        string? language = null,
        string? voice = null,
        string? format = null,
        string? endpointId = null,
        CancellationToken cancellationToken = default)
    {
        // Get Azure AD credential and token
        var credential = await GetCredential(cancellationToken);

        // Get access token for Cognitive Services with proper scope
        var tokenRequestContext = new TokenRequestContext(["https://cognitiveservices.azure.com/.default"]);
        var accessToken = await credential.GetTokenAsync(tokenRequestContext, cancellationToken);

        // Configure Speech SDK with endpoint
        var config = SpeechConfig.FromEndpoint(new Uri(endpoint));

        // Set the authorization token
        config.AuthorizationToken = accessToken.Token;

        // Set language (default to en-US)
        var synthesisLanguage = language ?? "en-US";
        config.SpeechSynthesisLanguage = synthesisLanguage;

        // Set voice if provided
        string? actualVoice = voice;
        if (!string.IsNullOrEmpty(voice))
        {
            config.SpeechSynthesisVoiceName = voice;
        }

        // Set output format (default to Riff24Khz16BitMonoPcm)
        var outputFormat = ParseOutputFormat(format);
        config.SetSpeechSynthesisOutputFormat(outputFormat);

        // Set custom endpoint ID if provided
        if (!string.IsNullOrEmpty(endpointId))
        {
            config.EndpointId = endpointId;
        }

        // Create a memory stream to collect audio data via push stream
        var audioStream = new MemoryStream();
        using var pushStream = AudioOutputStream.CreatePushStream(new PushAudioStreamCallback(audioStream, _logger));
        using var audioConfig = AudioConfig.FromStreamOutput(pushStream);
        using var synthesizer = new SpeechSynthesizer(config, audioConfig);

        // Track synthesis progress
        var taskCompletionSource = new TaskCompletionSource<bool>();
        SpeechSynthesisCancellationDetails? cancellationDetails = null;

        // Subscribe to synthesis events
        synthesizer.SynthesisStarted += (s, e) =>
        {
            _logger.LogInformation("Speech synthesis started for text length: {Length} characters", text.Length);
        };

        synthesizer.Synthesizing += (s, e) =>
        {
            if (e.Result.AudioData.Length > 0)
            {
                _logger.LogDebug("Received audio chunk: {ChunkSize} bytes", e.Result.AudioData.Length);
            }
        };

        synthesizer.SynthesisCompleted += (s, e) =>
        {
            _logger.LogInformation("Speech synthesis completed");
            taskCompletionSource.TrySetResult(true);
        };

        synthesizer.SynthesisCanceled += (s, e) =>
        {
            var details = SpeechSynthesisCancellationDetails.FromResult(e.Result);
            _logger.LogError("Speech synthesis canceled: Reason={Reason}, ErrorCode={ErrorCode}, ErrorDetails={ErrorDetails}",
                details.Reason, details.ErrorCode, details.ErrorDetails);
            cancellationDetails = details;
            taskCompletionSource.TrySetResult(false);
        };

        // Start synthesis
        await synthesizer.SpeakTextAsync(text);

        // Wait for synthesis to complete
        var success = await taskCompletionSource.Task;

        // Check if synthesis was successful
        if (!success && cancellationDetails != null)
        {
            if (IsSynthesisInvalidEndpointError(cancellationDetails))
            {
                throw new InvalidOperationException(
                    $"Invalid endpoint or connectivity issue. Reason: {cancellationDetails.Reason}, ErrorCode: {cancellationDetails.ErrorCode}, Details: {cancellationDetails.ErrorDetails}");
            }

            throw new InvalidOperationException(
                $"Speech synthesis failed: {cancellationDetails.Reason} - {cancellationDetails.ErrorDetails}");
        }

        if (!success)
        {
            throw new InvalidOperationException("Speech synthesis failed for unknown reason");
        }

        // Get the collected audio data from the stream
        var audioData = audioStream.ToArray();

        _logger.LogInformation(
            "Speech synthesized successfully. Total audio length: {AudioLength} bytes",
            audioData.Length);

        // Get actual voice used (either specified or default)
        if (string.IsNullOrEmpty(actualVoice))
        {
            actualVoice = voice ?? "default";
        }

        return (audioData, actualVoice);
    }

    /// <summary>
    /// Push stream callback that writes audio data to a memory stream as it arrives.
    /// This allows for efficient collection of audio data during synthesis without blocking.
    /// </summary>
    private sealed class PushAudioStreamCallback(MemoryStream targetStream, ILogger logger) : PushAudioOutputStreamCallback
    {
        private readonly MemoryStream _targetStream = targetStream;
        private readonly ILogger _logger = logger;

        public override uint Write(byte[] dataBuffer)
        {
            if (dataBuffer != null && dataBuffer.Length > 0)
            {
                _targetStream.Write(dataBuffer, 0, dataBuffer.Length);
                _logger.LogDebug("Wrote {BytesWritten} bytes to audio stream", dataBuffer.Length);
                return (uint)dataBuffer.Length;
            }
            return 0;
        }

        public override void Close()
        {
            _logger.LogDebug("Push stream closed, total bytes collected: {TotalBytes}", _targetStream.Length);
        }
    }

    /// <summary>
    /// Determines if the cancellation details indicate an invalid endpoint error for synthesis.
    /// </summary>
    private static bool IsSynthesisInvalidEndpointError(SpeechSynthesisCancellationDetails cancellationDetails)
    {
        return cancellationDetails.Reason == CancellationReason.Error &&
               (cancellationDetails.ErrorCode == CancellationErrorCode.ConnectionFailure ||
                cancellationDetails.ErrorCode == CancellationErrorCode.AuthenticationFailure ||
                cancellationDetails.ErrorCode == CancellationErrorCode.Forbidden ||
                cancellationDetails.ErrorDetails?.Contains("endpoint", StringComparison.OrdinalIgnoreCase) == true ||
                cancellationDetails.ErrorDetails?.Contains("connection", StringComparison.OrdinalIgnoreCase) == true ||
                cancellationDetails.ErrorDetails?.Contains("network", StringComparison.OrdinalIgnoreCase) == true);
    }

    /// <summary>
    /// Parses the output format string to SpeechSynthesisOutputFormat enum.
    /// </summary>
    private static SpeechSynthesisOutputFormat ParseOutputFormat(string? format)
    {
        if (string.IsNullOrEmpty(format))
        {
            return SpeechSynthesisOutputFormat.Riff24Khz16BitMonoPcm;
        }

        // Try to parse the format string directly to enum
        if (Enum.TryParse<SpeechSynthesisOutputFormat>(format, true, out var parsedFormat))
        {
            return parsedFormat;
        }

        // If parsing fails, default to Riff24Khz16BitMonoPcm
        return SpeechSynthesisOutputFormat.Riff24Khz16BitMonoPcm;
    }
}

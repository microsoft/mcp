// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Tools.Speech.Models;
using Azure.Mcp.Tools.Speech.Services.Recognizers;
using Azure.Mcp.Tools.Speech.Services.Synthesizers;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.Speech.Services;

public class SpeechService(
    ITenantService tenantService,
    ILogger<SpeechService> logger,
    IFastTranscriptionRecognizer fastTranscriptionRecognizer,
    IRealtimeTranscriptionRecognizer realtimeTranscriptionRecognizer,
    IRealtimeTtsSynthesizer speechSynthesizer)
    : BaseAzureService(tenantService), ISpeechService
{
    private readonly ILogger<SpeechService> _logger = logger;
    private readonly IFastTranscriptionRecognizer _fastTranscriptionRecognizer = fastTranscriptionRecognizer;
    private readonly IRealtimeTranscriptionRecognizer _realtimeTranscriptionRecognizer = realtimeTranscriptionRecognizer;
    private readonly IRealtimeTtsSynthesizer _speechSynthesizer = speechSynthesizer;
    /// <summary>
    /// Recognizes speech from an audio file using either Fast Transcription or Realtime Transcription.
    /// Fast Transcription is preferred when the language is supported.
    /// </summary>
    /// <param name="endpoint">Azure AI Services endpoint (e.g., https://your-service.cognitiveservices.azure.com/)</param>
    /// <param name="filePath">Path to the audio file to process</param>
    /// <param name="language">Optional Speech recognition language (default: en-US)</param>
    /// <param name="phrases">Optional phrases to improve recognition accuracy (ignored for Fast Transcription)</param>
    /// <param name="format">Output format (simple or detailed)</param>
    /// <param name="profanity">Profanity filtering option (masked, removed, or raw)</param>
    /// <param name="retryPolicy">Optional retry policy for resilience</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation</param>
    /// <returns>Continuous recognition result containing full text and individual segments</returns>
    public async Task<SpeechRecognitionResult> RecognizeSpeechFromFile(
        string endpoint,
        string filePath,
        string? language = null,
        string[]? phrases = null,
        string? format = null,
        string? profanity = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters((nameof(endpoint), endpoint), (nameof(filePath), filePath));

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Audio file not found: {filePath}");
        }

        try
        {
            // Use the selector to determine the best transcription method
            var speechRecognizer = SpeechRecognizerSelector.GetSpeechRecognizer(filePath, language, phrases, format);
            _logger.LogInformation("Transcription method selection: {RecognizerType}", speechRecognizer);
            var locale = LocaleSupport.MapLanguageToValidLocale(language);

            if (speechRecognizer == RecognizerType.Fast)
            {
                try
                {
                    var fastResult = await _fastTranscriptionRecognizer.RecognizeAsync(
                        endpoint, filePath, locale, phrases, profanity, retryPolicy, cancellationToken);

                    // Convert to unified result
                    return SpeechRecognitionResult.FromFastTranscriptionResult(fastResult);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Fast Transcription failed for language '{Language}', falling back to Realtime Transcription", language);
                    // Fall through to use Realtime Transcription
                }
            }

            // Use Realtime Transcription as fallback or primary choice
            _logger.LogInformation("Using Realtime Transcription for language '{Language}' with file '{FilePath}'", language, filePath);
            var realtimeResult = await _realtimeTranscriptionRecognizer.RecognizeAsync(
                endpoint, filePath, locale, phrases, format, profanity, retryPolicy, cancellationToken);

            // Convert to unified result
            return SpeechRecognitionResult.FromRealtimeResult(realtimeResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during speech recognition from file.");
            throw;
        }
    }

    /// <summary>
    /// Synthesizes speech from text and saves it to an audio file using Azure AI Services Speech.
    /// Delegates to the speech synthesizer for actual synthesis implementation.
    /// </summary>
    /// <param name="endpoint">Azure AI Services endpoint (e.g., https://your-service.cognitiveservices.azure.com/)</param>
    /// <param name="text">The text to convert to speech</param>
    /// <param name="outputFilePath">Path where the audio file will be saved</param>
    /// <param name="language">Language for synthesis (default: en-US)</param>
    /// <param name="voice">Voice name to use (e.g., en-US-JennyNeural). If not specified, default voice for language is used</param>
    /// <param name="format">Output audio format (default: Riff24Khz16BitMonoPcm)</param>
    /// <param name="endpointId">Optional endpoint ID for custom voice model</param>
    /// <param name="retryPolicy">Optional retry policy for resilience</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation</param>
    /// <returns>Synthesis result with file information</returns>
    public async Task<SynthesisResult> SynthesizeSpeechToFile(
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
        return await _speechSynthesizer.SynthesizeToFileAsync(
            endpoint,
            text,
            outputFilePath,
            language,
            voice,
            format,
            endpointId,
            retryPolicy,
            cancellationToken);
    }
}

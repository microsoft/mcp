// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Tools.Speech.Models;
using Azure.Mcp.Tools.Speech.Services.Recognizers;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.Speech.Services;

public class SpeechService(ITenantService tenantService, ILogger<SpeechService> logger, IFastTranscriptionRecognizer fastTranscriptionRecognizer, IRealtimeTranscriptionRecognizer realtimeTranscriptionRecognizer) : BaseAzureService(tenantService), ISpeechService
{
    private readonly ILogger<SpeechService> _logger = logger;
    private readonly IFastTranscriptionRecognizer _fastTranscriptionRecognizer = fastTranscriptionRecognizer;
    private readonly IRealtimeTranscriptionRecognizer _realtimeTranscriptionRecognizer = realtimeTranscriptionRecognizer;
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
    /// <returns>Continuous recognition result containing full text and individual segments</returns>
    public async Task<SpeechRecognitionResult> RecognizeSpeechFromFile(
        string endpoint,
        string filePath,
        string? language = null,
        string[]? phrases = null,
        string? format = null,
        string? profanity = null,
        RetryPolicyOptions? retryPolicy = null)
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
                        endpoint, filePath, locale, phrases, profanity, retryPolicy);

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
                endpoint, filePath, locale, phrases, format, profanity, retryPolicy);

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
    /// Determines if the cancellation details indicate an invalid endpoint error.
    /// </summary>
    /// <param name="cancellationDetails">The cancellation details from the speech recognition</param>
    /// <returns>True if the error indicates an invalid endpoint, false otherwise</returns>
    private static bool IsInvalidEndpointError(CancellationDetails cancellationDetails)
    {
        // Check for common error codes that indicate endpoint issues
        return cancellationDetails.Reason == CancellationReason.Error &&
               (cancellationDetails.ErrorCode == CancellationErrorCode.ConnectionFailure ||
                cancellationDetails.ErrorCode == CancellationErrorCode.AuthenticationFailure ||
                cancellationDetails.ErrorCode == CancellationErrorCode.Forbidden ||
                cancellationDetails.ErrorDetails?.Contains("endpoint", StringComparison.OrdinalIgnoreCase) == true ||
                cancellationDetails.ErrorDetails?.Contains("connection", StringComparison.OrdinalIgnoreCase) == true ||
                cancellationDetails.ErrorDetails?.Contains("network", StringComparison.OrdinalIgnoreCase) == true);
    }

    /// <summary>
    /// Creates an AudioConfig from a file, automatically detecting the format based on file extension.
    /// Supports WAV, MP3, OPUS/OGG, FLAC, and other common audio formats using GStreamer when available.
    /// </summary>
    /// <param name="filePath">Path to the audio file</param>
    /// <returns>AudioConfig configured for the specified audio file</returns>
    /// <exception cref="InvalidOperationException">Thrown when compressed audio format is used but GStreamer is not properly configured</exception>
    private static AudioConfig CreateAudioConfigFromFile(string filePath)
    {
        var extension = Path.GetExtension(filePath).ToLowerInvariant();

        // WAV files don't require GStreamer
        if (extension == ".wav")
        {
            return AudioConfig.FromWavFileInput(filePath);
        }

        // For compressed formats, check if GStreamer is available
        var isCompressedFormat = extension is ".mp3" or ".ogg" or ".opus" or ".flac" or ".alaw" or ".mulaw" or ".mp4" or ".m4a" or ".aac";

        if (isCompressedFormat)
        {
            return extension switch
            {
                ".mp3" => CreateCompressedAudioConfig(filePath, AudioStreamContainerFormat.MP3),
                ".ogg" => CreateCompressedAudioConfig(filePath, AudioStreamContainerFormat.OGG_OPUS),
                ".opus" => CreateCompressedAudioConfig(filePath, AudioStreamContainerFormat.OGG_OPUS),
                ".flac" => CreateCompressedAudioConfig(filePath, AudioStreamContainerFormat.FLAC),
                ".alaw" => CreateCompressedAudioConfig(filePath, AudioStreamContainerFormat.ALAW),
                ".mulaw" => CreateCompressedAudioConfig(filePath, AudioStreamContainerFormat.MULAW),
                ".mp4" => CreateCompressedAudioConfig(filePath, AudioStreamContainerFormat.ANY),
                ".m4a" => CreateCompressedAudioConfig(filePath, AudioStreamContainerFormat.ANY),
                ".aac" => CreateCompressedAudioConfig(filePath, AudioStreamContainerFormat.ANY),
                _ => throw new NotSupportedException($"Audio format {extension} is not supported")
            };
        }

        // Throw exception for unsupported formats
        throw new NotSupportedException($"Audio format '{extension}' is not supported. Supported formats are: .wav, .mp3, .ogg, .opus, .flac, .alaw, .mulaw, .mp4, .m4a, .aac");
    }

    /// <summary>
    /// Creates an AudioConfig for compressed audio formats using PullAudioInputStream.
    /// Requires GStreamer to be installed and available in the system PATH.
    /// </summary>
    /// <param name="filePath">Path to the compressed audio file</param>
    /// <param name="containerFormat">The audio container format</param>
    /// <returns>AudioConfig configured for the compressed audio file</returns>
    private static AudioConfig CreateCompressedAudioConfig(string filePath, AudioStreamContainerFormat containerFormat)
    {
        // Create compressed audio stream format
        var audioFormat = AudioStreamFormat.GetCompressedFormat(containerFormat);

        // Create a custom PullAudioInputStream using a callback
        var callback = new BinaryFileReaderCallback(filePath);
        var pullStream = AudioInputStream.CreatePullStream(callback, audioFormat);

        return AudioConfig.FromStreamInput(pullStream);
    }

    /// <summary>
    /// Determines if an exception indicates that GStreamer is missing or not properly configured.
    /// </summary>
    /// <param name="ex">The exception to check</param>
    /// <returns>True if the exception indicates GStreamer is missing, false otherwise</returns>
    private static bool IsGStreamerMissingError(Exception ex)
    {
        // Check for common GStreamer-related error patterns
        var message = ex.Message?.ToLowerInvariant() ?? "";
        var innerMessage = ex.InnerException?.Message?.ToLowerInvariant() ?? "";

        // Common GStreamer error indicators
        var gstreamerErrorPatterns = new[]
        {
            "gstreamer",
            "0x27", // SPXERR_GSTREAMER_INTERNAL_ERROR
            "spxerr_gstreamer",
            "compressed audio",
            "codec",
            "audio format not supported",
            "audio stream format",
            "pipeline",
            "element",
            "decoder"
        };

        return gstreamerErrorPatterns.Any(pattern =>
            message.Contains(pattern) || innerMessage.Contains(pattern));
    }

    /// <summary>
    /// Binary file reader callback for PullAudioInputStream.
    /// Reads binary audio data from file for compressed audio processing.
    /// </summary>
    private sealed class BinaryFileReaderCallback : PullAudioInputStreamCallback
    {
        private readonly FileStream _fileStream;

        public BinaryFileReaderCallback(string filePath)
        {
            _fileStream = File.OpenRead(filePath);
        }

        public override int Read(byte[] dataBuffer, uint size)
        {
            try
            {
                var bytesToRead = Math.Min((int)size, dataBuffer.Length);
                return _fileStream.Read(dataBuffer, 0, bytesToRead);
            }
            catch
            {
                return 0; // End of stream or error
            }
        }

        public override void Close()
        {
            _fileStream?.Dispose();
        }
    }

    private static Models.SpeechRecognitionResult CreateNoMatchResult()
    {
        return new Models.SpeechRecognitionResult
        {
            Text = string.Empty,
            Reason = ResultReason.NoMatch.ToString()
        };
    }

    private static ProfanityOption GetProfanityOption(string profanity) =>
        profanity.ToLowerInvariant() switch
        {
            "masked" => ProfanityOption.Masked,
            "removed" => ProfanityOption.Removed,
            "raw" => ProfanityOption.Raw,
            _ => ProfanityOption.Masked
        };

    private static Models.SpeechRecognitionResult ConvertToSpeechRecognitionResult(SdkSpeechRecognitionResult speechResult, string? format)
    {
        // detailed format
        if (format?.ToLowerInvariant() == "detailed")
        {
            return new Models.DetailedSpeechRecognitionResult
            {
                Text = speechResult.Text,
                Reason = speechResult.Reason.ToString(),
                Offset = (ulong)speechResult.OffsetInTicks,
                Duration = (ulong)speechResult.Duration.Ticks,
                NBest = ExtractNBestResults(speechResult)
            };
        }
        // simple format
        else
        {
            return new Models.SpeechRecognitionResult
            {
                Text = speechResult.Text,
                Reason = speechResult.Reason.ToString(),
                Offset = (ulong)speechResult.OffsetInTicks,
                Duration = (ulong)speechResult.Duration.Ticks
            };
        }
    }

    /// <summary>
    /// Extracts NBest results from speech recognition result properties.
    /// Parses the detailed JSON response to get confidence scores and alternative text candidates.
    /// </summary>
    /// <param name="speechResult">The speech recognition result</param>
    /// <returns>List of NBest results with actual confidence values</returns>
    private static List<NBestResult> ExtractNBestResults(SdkSpeechRecognitionResult speechResult)
    {
        var nbestResults = new List<NBestResult>();
        try
        {
            // Try to get the detailed JSON result from Properties
            var jsonProperty = speechResult.Properties.GetProperty(PropertyId.SpeechServiceResponse_JsonResult);

            if (!string.IsNullOrEmpty(jsonProperty))
            {
                using var jsonDoc = JsonDocument.Parse(jsonProperty);

                if (jsonDoc.RootElement.TryGetProperty("NBest", out var nbestArray))
                {
                    foreach (var item in nbestArray.EnumerateArray())
                    {
                        var confidence = item.TryGetProperty("Confidence", out var confidenceProp) ? confidenceProp.GetDouble() : 0.0;
                        var lexical = item.TryGetProperty("Lexical", out var lexicalProp) ? lexicalProp.GetString() : "";
                        var itn = item.TryGetProperty("ITN", out var itnProp) ? itnProp.GetString() : "";
                        var maskedITN = item.TryGetProperty("MaskedITN", out var maskedITNProp) ? maskedITNProp.GetString() : "";
                        var display = item.TryGetProperty("Display", out var displayProp) ? displayProp.GetString() : "";

                        // Extract words if available
                        List<WordResult>? words = null;
                        if (item.TryGetProperty("Words", out var wordsArray))
                        {
                            words = new List<WordResult>();
                            foreach (var wordItem in wordsArray.EnumerateArray())
                            {
                                var word = new WordResult
                                {
                                    Word = wordItem.TryGetProperty("Word", out var wordProp) ? wordProp.GetString() : "",
                                    Offset = wordItem.TryGetProperty("Offset", out var offsetProp) ? (ulong)offsetProp.GetInt64() : null,
                                    Duration = wordItem.TryGetProperty("Duration", out var durationProp) ? (ulong)durationProp.GetInt64() : null
                                };
                                words.Add(word);
                            }
                        }

                        nbestResults.Add(new NBestResult
                        {
                            Confidence = confidence,
                            Lexical = lexical,
                            ITN = itn,
                            MaskedITN = maskedITN,
                            Display = display,
                            Words = words
                        });
                    }
                }
            }
        }
        catch (JsonException)
        {
            // If JSON parsing fails, fall back to simple result
        }

        return nbestResults;
    }

    /// <summary>
    /// Synthesizes speech from text and saves it to an audio file using Azure AI Services Speech.
    /// </summary>
    /// <param name="endpoint">Azure AI Services endpoint (e.g., https://your-service.cognitiveservices.azure.com/)</param>
    /// <param name="text">The text to convert to speech</param>
    /// <param name="outputFilePath">Path where the audio file will be saved</param>
    /// <param name="language">Language for synthesis (default: en-US)</param>
    /// <param name="voice">Voice name to use (e.g., en-US-JennyNeural). If not specified, default voice for language is used</param>
    /// <param name="format">Output audio format (default: Riff24Khz16BitMonoPcm)</param>
    /// <param name="endpointId">Optional endpoint ID for custom voice model</param>
    /// <param name="retryPolicy">Optional retry policy for resilience</param>
    /// <returns>Synthesis result with file information</returns>
    public async Task<SynthesisResult> SynthesizeSpeechToFile(
        string endpoint,
        string text,
        string outputFilePath,
        string? language = null,
        string? voice = null,
        string? format = null,
        string? endpointId = null,
        RetryPolicyOptions? retryPolicy = null)
    {
        ValidateRequiredParameters((nameof(endpoint), endpoint), (nameof(text), text), (nameof(outputFilePath), outputFilePath));

        if (string.IsNullOrWhiteSpace(text))
        {
            throw new ArgumentException("Text cannot be empty or whitespace.", nameof(text));
        }

        try
        {
            // Get Azure AD credential and token
            var credential = await GetCredential();

            // Get access token for Cognitive Services with proper scope
            var tokenRequestContext = new TokenRequestContext(["https://cognitiveservices.azure.com/.default"]);
            var accessToken = await credential.GetTokenAsync(tokenRequestContext, CancellationToken.None);

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

            // Create audio configuration for file output
            using var audioConfig = AudioConfig.FromWavFileOutput(outputFilePath);
            using var synthesizer = new SpeechSynthesizer(config, audioConfig);

            // Perform synthesis
            var startTime = DateTime.UtcNow;
            var result = await synthesizer.SpeakTextAsync(text);
            var duration = (DateTime.UtcNow - startTime).Ticks;

            // Check result
            if (result.Reason == ResultReason.SynthesizingAudioCompleted)
            {
                _logger.LogInformation(
                    "Speech synthesized successfully. Output file: {OutputFile}, Audio length: {AudioLength} bytes",
                    outputFilePath,
                    result.AudioData.Length);

                // Get actual voice used (either specified or default)
                if (string.IsNullOrEmpty(actualVoice))
                {
                    // The voice name might not be easily retrievable from result properties
                    // Set to a default or leave as is
                    actualVoice = voice ?? "default";
                }

                return new SynthesisResult
                {
                    FilePath = outputFilePath,
                    Duration = duration,
                    AudioLength = result.AudioData.Length,
                    Format = format ?? "Riff24Khz16BitMonoPcm",
                    Voice = actualVoice,
                    Language = synthesisLanguage
                };
            }
            else if (result.Reason == ResultReason.Canceled)
            {
                var cancellation = SpeechSynthesisCancellationDetails.FromResult(result);
                _logger.LogError(
                    "Speech synthesis canceled: Reason={Reason}, ErrorCode={ErrorCode}, ErrorDetails={ErrorDetails}",
                    cancellation.Reason,
                    cancellation.ErrorCode,
                    cancellation.ErrorDetails);

                if (IsSynthesisInvalidEndpointError(cancellation))
                {
                    throw new InvalidOperationException(
                        $"Invalid endpoint or connectivity issue. Reason: {cancellation.Reason}, ErrorCode: {cancellation.ErrorCode}, Details: {cancellation.ErrorDetails}");
                }

                throw new InvalidOperationException(
                    $"Speech synthesis failed: {cancellation.Reason} - {cancellation.ErrorDetails}");
            }

            throw new InvalidOperationException($"Speech synthesis failed with reason: {result.Reason}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during speech synthesis.");
            throw;
        }
    }

    /// <summary>
    /// Determines if the cancellation details indicate an invalid endpoint error for synthesis.
    /// </summary>
    /// <param name="cancellationDetails">The cancellation details from speech synthesis</param>
    /// <returns>True if the error indicates an invalid endpoint, false otherwise</returns>
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
    /// <param name="format">Format string (e.g., "Riff24Khz16BitMonoPcm", "Audio16Khz32KBitRateMonoMp3")</param>
    /// <returns>SpeechSynthesisOutputFormat enum value</returns>
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

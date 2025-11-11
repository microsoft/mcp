// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Core;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Tools.Speech.Models.Realtime;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using SdkSpeechRecognitionResult = Microsoft.CognitiveServices.Speech.SpeechRecognitionResult;

namespace Azure.Mcp.Tools.Speech.Services.Recognizers;

/// <summary>
/// Recognizer for real-time speech transcription using Azure AI Services Speech SDK.
/// </summary>
public class RealtimeTranscriptionRecognizer(ITenantService tenantService, ILogger<RealtimeTranscriptionRecognizer> logger)
    : BaseAzureService(tenantService), IRealtimeTranscriptionRecognizer
{
    private readonly ILogger<RealtimeTranscriptionRecognizer> _logger = logger;

    /// <inheritdoc/>
    public async Task<RealtimeRecognitionContinuousResult> RecognizeAsync(
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

        if (string.IsNullOrEmpty(language) || !LocaleSupport.IsSupportedInRealtimeTranscription(language))
        {
            language = "en-US"; // Default to en-US if not specified or unsupported
            _logger.LogWarning("Language not specified or unsupported for Realtime Transcription. Defaulting to 'en-US'.");
        }

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Audio file not found: {filePath}");
        }

        // Apply retry policy configuration
        var maxRetries = retryPolicy?.MaxRetries ?? 3;
        var delaySeconds = retryPolicy?.DelaySeconds ?? 1.0;
        var maxDelaySeconds = retryPolicy?.MaxDelaySeconds ?? 30.0;
        var isExponentialBackoff = retryPolicy?.Mode == RetryMode.Exponential;

        Exception? lastException = null;

        for (int attempt = 0; attempt <= maxRetries; attempt++)
        {
            try
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
                config.SpeechRecognitionLanguage = language ?? "en-US";

                // set output format (default to simple)
                config.OutputFormat = (format?.ToLowerInvariant() == "detailed")
                    ? OutputFormat.Detailed
                    : OutputFormat.Simple;

                // Configure profanity filtering
                if (!string.IsNullOrEmpty(profanity))
                {
                    config.SetProfanity(GetProfanityOption(profanity));
                }

                // Create audio configuration from file
                using var audioConfig = CreateAudioConfigFromFile(filePath);
                using var recognizer = new SpeechRecognizer(config, audioConfig);

                // Add phrase hints if provided
                if (phrases?.Length > 0)
                {
                    var phraseList = PhraseListGrammar.FromRecognizer(recognizer);
                    foreach (var phrase in phrases)
                    {
                        phraseList.AddPhrase(phrase);
                    }
                }

                var taskCompletionSource = new TaskCompletionSource<bool>();
                var recognizedText = new System.Text.StringBuilder();
                var recognizedSegments = new List<RealtimeRecognitionResult>();
                CancellationDetails? cancellationDetails = null;

                // Subscribe to recognition events
                recognizer.SessionStarted += (s, e) =>
                {
                    _logger.LogInformation("Continuous recognition session started: {SessionId}", e.SessionId);
                };

                recognizer.Recognizing += (s, e) =>
                {
                    _logger.LogDebug("Recognizing intermediate result: Text={Text}", e.Result.Text);
                };

                recognizer.Recognized += (s, e) =>
                {
                    if (e.Result.Reason == ResultReason.RecognizedSpeech)
                    {
                        _logger.LogInformation("Recognized segment: Text={Text}", e.Result.Text);
                        recognizedText.Append(e.Result.Text).Append(" ");

                        // Create a segment for this recognition result
                        var segment = ConvertToSpeechRecognitionResult(e.Result, format);
                        recognizedSegments.Add(segment);
                    }
                    else if (e.Result.Reason == ResultReason.NoMatch)
                    {
                        _logger.LogWarning("NoMatch: Speech could not be recognized in this segment.");
                    }
                };

                recognizer.Canceled += (s, e) =>
                {
                    _logger.LogInformation("Continuous recognition canceled: Reason={Reason}", e.Reason);

                    if (e.Reason == CancellationReason.Error)
                    {
                        _logger.LogError("Continuous recognition error: ErrorCode={ErrorCode}, ErrorDetails={ErrorDetails}",
                            e.ErrorCode, e.ErrorDetails);

                        cancellationDetails = CancellationDetails.FromResult(e.Result);
                    }

                    taskCompletionSource.TrySetResult(false);
                };

                recognizer.SessionStopped += (s, e) =>
                {
                    _logger.LogInformation("Continuous recognition session stopped.");
                    taskCompletionSource.TrySetResult(true);
                };

                // Start continuous recognition
                await recognizer.StartContinuousRecognitionAsync();

                // Wait for session to complete
                var success = await taskCompletionSource.Task;

                // Stop recognition
                await recognizer.StopContinuousRecognitionAsync();

                // Check if recognition was canceled due to invalid endpoint or other errors
                if (!success && cancellationDetails != null)
                {
                    _logger.LogWarning("Recognition failed: {Reason}, {ErrorCode}, {ErrorDetails}",
                        cancellationDetails.Reason, cancellationDetails.ErrorCode, cancellationDetails.ErrorDetails);
                    // Common error codes for invalid endpoints:
                    // ConnectionFailure: Network connectivity issues or invalid endpoint
                    // AuthenticationFailure: Invalid credentials or endpoint authentication issues
                    // Forbidden: Endpoint exists but access is denied
                    if (IsInvalidEndpointError(cancellationDetails))
                    {
                        var errorMessage = $"Invalid endpoint or connectivity issue. Reason: {cancellationDetails.Reason}, ErrorCode: {cancellationDetails.ErrorCode}, Details: {cancellationDetails.ErrorDetails}";
                        throw new InvalidOperationException(errorMessage);
                    }
                }

                // Handle case where no segments were recognized
                if (success && recognizedSegments.Count == 0)
                {
                    _logger.LogWarning("Recognition success, but no speech segments were recognized. Creating NoMatch result.");
                    var noMatchSegment = CreateNoMatchResult();
                    recognizedSegments.Add(noMatchSegment);
                }

                // Return the continuous recognition result
                return new RealtimeRecognitionContinuousResult
                {
                    FullText = recognizedText.ToString().Trim(),
                    Segments = recognizedSegments
                };
            }
            catch (ApplicationException ex) when (ex.Message.Contains("SPXERR_UNEXPECTED_EOF", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogError(ex, "The audio file appears to be empty or corrupted.");
                throw new InvalidOperationException("The audio file appears to be empty or corrupted. Please provide a valid audio file.", ex);
            }
            catch (Exception ex) when (IsGStreamerMissingError(ex))
            {
                throw new InvalidOperationException($"Cannot process compressed audio file '{filePath}' because GStreamer is not properly installed or configured.\n\n" +
                    "To use compressed audio formats (MP3, OGG, FLAC, etc.), you must install GStreamer:\n\n" +
                    "Windows:\n" +
                    "1. Download and install GStreamer from: https://gstreamer.freedesktop.org/download/\n" +
                    "2. Install both the runtime and development packages\n" +
                    "3. Add GStreamer\\bin to your system PATH environment variable\n" +
                    "4. Restart your application\n\n" +
                    "Linux (Ubuntu/Debian):\n" +
                    "sudo apt-get install gstreamer1.0-plugins-base gstreamer1.0-plugins-good gstreamer1.0-plugins-bad gstreamer1.0-plugins-ugly\n\n" +
                    "macOS:\n" +
                    "brew install gstreamer gst-plugins-base gst-plugins-good gst-plugins-bad gst-plugins-ugly\n\n" +
                    "For more information, see: https://learn.microsoft.com/en-us/azure/ai-services/speech-service/how-to-use-codec-compressed-audio-input-streams?pivots=programming-language-csharp#gstreamer-configuration\n\n" +
                    "Alternatively, convert your audio file to WAV format, which doesn't require GStreamer.", ex);
            }
            catch (Exception ex) when (IsRetryableException(ex) && attempt < maxRetries)
            {
                lastException = ex;
                _logger.LogWarning(ex, "Retryable error occurred during speech recognition attempt {Attempt}/{MaxAttempts}: {ErrorMessage}",
                    attempt + 1, maxRetries + 1, ex.Message);

                // Calculate delay for next attempt
                if (attempt < maxRetries)
                {
                    var delay = isExponentialBackoff
                        ? Math.Min(delaySeconds * Math.Pow(2, attempt), maxDelaySeconds)
                        : delaySeconds;

                    _logger.LogDebug("Waiting {DelaySeconds} seconds before retry attempt {NextAttempt}", delay, attempt + 2);
                    await Task.Delay(TimeSpan.FromSeconds(delay), cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Non-retryable error during speech recognition from file.");
                throw;
            }
        }

        // If we've exhausted all retries, throw the last exception
        _logger.LogError(lastException, "Speech recognition failed after {MaxRetries} retry attempts", maxRetries);
        throw lastException ?? new InvalidOperationException("Speech recognition failed after maximum retry attempts");
    }

    /// <summary>
    /// Determines if an exception is retryable for Speech SDK operations.
    /// </summary>
    /// <param name="ex">The exception to check</param>
    /// <returns>True if the exception indicates a retryable condition, false otherwise</returns>
    private static bool IsRetryableException(Exception ex)
    {
        return ex switch
        {
            // Network-related exceptions
            HttpRequestException => true,
            TaskCanceledException => true,
            System.Net.Sockets.SocketException => true,

            // Speech SDK specific retryable exceptions
            ApplicationException appEx when appEx.Message.Contains("SPXERR_TIMEOUT", StringComparison.OrdinalIgnoreCase) => true,
            ApplicationException appEx when appEx.Message.Contains("SPXERR_NETWORK", StringComparison.OrdinalIgnoreCase) => true,
            ApplicationException appEx when appEx.Message.Contains("SPXERR_CONNECTION_FAILURE", StringComparison.OrdinalIgnoreCase) => true,

            // Other transient errors
            InvalidOperationException ioEx when ioEx.Message.Contains("timeout", StringComparison.OrdinalIgnoreCase) => true,
            InvalidOperationException ioEx when ioEx.Message.Contains("network", StringComparison.OrdinalIgnoreCase) => true,
            InvalidOperationException ioEx when ioEx.Message.Contains("connection", StringComparison.OrdinalIgnoreCase) => true,

            _ => false
        };
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
            _ => throw new NotSupportedException($"Audio format '{extension}' is not supported. Supported formats are: .wav, .mp3, .ogg, .opus, .flac, .alaw, .mulaw, .mp4, .m4a, .aac")
        };
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

    private static readonly string[] GStreamerErrorPatterns =
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

        return GStreamerErrorPatterns.Any(pattern =>
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

    private static RealtimeRecognitionResult CreateNoMatchResult()
    {
        return new RealtimeRecognitionResult
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

    private static RealtimeRecognitionResult ConvertToSpeechRecognitionResult(SdkSpeechRecognitionResult speechResult, string? format)
    {
        return (format?.ToLowerInvariant() == "detailed")
            ? new RealtimeRecognitionDetailedResult
            {
                Text = speechResult.Text,
                Reason = speechResult.Reason.ToString(),
                Offset = (ulong)speechResult.OffsetInTicks,
                Duration = (ulong)speechResult.Duration.Ticks,
                NBest = ExtractNBestResults(speechResult)
            }
            : new RealtimeRecognitionResult
            {
                Text = speechResult.Text,
                Reason = speechResult.Reason.ToString(),
                Offset = (ulong)speechResult.OffsetInTicks,
                Duration = (ulong)speechResult.Duration.Ticks
            };
    }

    /// <summary>
    /// Extracts NBest results from speech recognition result properties.
    /// Parses the detailed JSON response to get confidence scores and alternative text candidates.
    /// </summary>
    /// <param name="speechResult">The speech recognition result</param>
    /// <returns>List of NBest results with actual confidence values</returns>
    private static List<RealtimeRecognitionNBestResult> ExtractNBestResults(SdkSpeechRecognitionResult speechResult)
    {
        var nbestResults = new List<RealtimeRecognitionNBestResult>();
        try
        {
            // Try to get the detailed JSON result from Properties
            var jsonProperty = speechResult.Properties.GetProperty(PropertyId.SpeechServiceResponse_JsonResult);

            if (!string.IsNullOrEmpty(jsonProperty))
            {
                using var jsonResult = JsonDocument.Parse(jsonProperty);
                if (jsonResult.RootElement.TryGetProperty("NBest", out var nbestArray))
                {
                    foreach (var item in nbestArray.EnumerateArray())
                    {
                        var confidence = item.TryGetProperty("Confidence", out var confidenceProp) ? confidenceProp.GetDouble() : 0.0;
                        var lexical = item.TryGetProperty("Lexical", out var lexicalProp) ? lexicalProp.GetString() : "";
                        var itn = item.TryGetProperty("ITN", out var itnProp) ? itnProp.GetString() : "";
                        var maskedITN = item.TryGetProperty("MaskedITN", out var maskedITNProp) ? maskedITNProp.GetString() : "";
                        var display = item.TryGetProperty("Display", out var displayProp) ? displayProp.GetString() : "";

                        // Extract words if available
                        List<RealtimeRecognitionWordResult>? words = null;
                        if (item.TryGetProperty("Words", out var wordsArray))
                        {
                            words = wordsArray.EnumerateArray().Select(wordItem => new RealtimeRecognitionWordResult
                            {
                                Word = wordItem.TryGetProperty("Word", out var wordProp) ? wordProp.GetString() : "",
                                Offset = wordItem.TryGetProperty("Offset", out var offsetProp) ? (ulong)offsetProp.GetInt64() : null,
                                Duration = wordItem.TryGetProperty("Duration", out var durationProp) ? (ulong)durationProp.GetInt64() : null
                            }).ToList();
                        }

                        nbestResults.Add(new RealtimeRecognitionNBestResult
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
}

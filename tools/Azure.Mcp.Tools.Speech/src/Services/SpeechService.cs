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
using SdkSpeechRecognitionResult = Microsoft.CognitiveServices.Speech.SpeechRecognitionResult;

namespace Azure.Mcp.Tools.Speech.Services;

public class SpeechService(ITenantService tenantService, ILogger<SpeechService> logger) : BaseAzureService(tenantService), ISpeechService
{
    private readonly ILogger<SpeechService> _logger = logger;
    /// <summary>
    /// Recognizes speech from an audio file using Azure AI Services Speech.
    /// </summary>
    /// <param name="endpoint">Azure AI Services endpoint (e.g., https://your-service.cognitiveservices.azure.com/)</param>
    /// <param name="filePath">Path to the audio file to process</param>
    /// <param name="language">Speech recognition language (default: en-US)</param>
    /// <param name="phrases">Optional phrases to improve recognition accuracy</param>
    /// <param name="format">Output format (simple or detailed)</param>
    /// <param name="profanity">Profanity filtering option (masked, removed, or raw)</param>
    /// <param name="retryPolicy">Optional retry policy for resilience</param>
    /// <returns>Speech recognition result containing recognized text and metadata</returns>
    public async Task<Models.SpeechRecognitionResult> RecognizeSpeechFromFile(
        string endpoint,
        string filePath,
        string? language = null,
        string[]? phrases = null,
        string? format = null,
        string? profanity = null,
        RetryPolicyOptions? retryPolicy = null)
    {
        ValidateRequiredParameters(endpoint, filePath);

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Audio file not found: {filePath}");
        }

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
        config.SpeechRecognitionLanguage = language ?? "en-US";

        // Configure profanity filtering
        if (!string.IsNullOrEmpty(profanity))
        {
            config.SetProfanity(GetProfanityOption(profanity));
        }

        // Create audio configuration from file (supports multiple formats)
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

        // Use streaming recognition for file-based speech recognition
        // This is more robust for various audio formats and file lengths
        var taskCompletionSource = new TaskCompletionSource<SdkSpeechRecognitionResult?>();
        var recognizedText = new System.Text.StringBuilder();
        SdkSpeechRecognitionResult? lastResult = null;
        CancellationDetails? cancellationDetails = null;

        // Subscribe to recognition events
        recognizer.Recognizing += (s, e) =>
        {
            // Intermediate results (optional for streaming)
        };

        recognizer.Recognized += (s, e) =>
        {
            if (e.Result.Reason == ResultReason.RecognizedSpeech)
            {
                recognizedText.Append(e.Result.Text);
                lastResult = e.Result;
            }
            else if (e.Result.Reason == ResultReason.NoMatch)
            {
                lastResult = e.Result;
            }
        };

        recognizer.Canceled += (s, e) =>
        {
            cancellationDetails = CancellationDetails.FromResult(e.Result);
            _logger.LogError("Recognition canceled: {Reason}, {ErrorCode}, {ErrorDetails}",
                cancellationDetails.Reason, cancellationDetails.ErrorCode, cancellationDetails.ErrorDetails);

            // Store the canceled result for analysis
            lastResult = e.Result;
        };

        recognizer.SessionStopped += (s, e) =>
        {
            // If we have a result, use it; otherwise signal completion with null
            taskCompletionSource.TrySetResult(lastResult);
        };

        // Start continuous recognition
        await recognizer.StartContinuousRecognitionAsync();

        // Wait for completion or timeout (30 seconds should be enough for most audio files)
        var timeoutTask = Task.Delay(30000);
        var completedTask = await Task.WhenAny(taskCompletionSource.Task, timeoutTask);

        // Stop recognition
        await recognizer.StopContinuousRecognitionAsync();

        if (completedTask == timeoutTask)
        {
            throw new TimeoutException("Speech recognition timed out after 30 seconds");
        }

        var result = await taskCompletionSource.Task;

        // Handle case where no recognition result was obtained
        if (result == null)
        {
            return CreateNoMatchResult();
        }

        // Check if recognition was canceled due to invalid endpoint or other errors
        if (result.Reason == ResultReason.Canceled && cancellationDetails != null)
        {
            // Common error codes for invalid endpoints:
            // - ConnectionFailure: Network connectivity issues or invalid endpoint
            // - AuthenticationFailure: Invalid credentials or endpoint authentication issues
            // - Forbidden: Endpoint exists but access is denied
            if (IsInvalidEndpointError(cancellationDetails))
            {
                var errorMessage = $"Invalid endpoint or connectivity issue. Reason: {cancellationDetails.Reason}, ErrorCode: {cancellationDetails.ErrorCode}, Details: {cancellationDetails.ErrorDetails}";
                throw new InvalidOperationException(errorMessage);
            }
        }

        // If we accumulated text from multiple recognition events, update the result
        if (recognizedText.Length > 0 && result.Text != recognizedText.ToString())
        {
            // For streaming recognition, we return the accumulated text in our model
            var streamingResult = ConvertToSpeechRecognitionResult(result, format);
            streamingResult.Text = recognizedText.ToString();
            return streamingResult;
        }

        return ConvertToSpeechRecognitionResult(result, format);
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
    private static AudioConfig CreateAudioConfigFromFile(string filePath)
    {
        var extension = Path.GetExtension(filePath).ToLowerInvariant();

        try
        {
            return extension switch
            {
                ".wav" => AudioConfig.FromWavFileInput(filePath),
                ".mp3" => CreateCompressedAudioConfig(filePath, AudioStreamContainerFormat.MP3),
                ".ogg" => CreateCompressedAudioConfig(filePath, AudioStreamContainerFormat.OGG_OPUS),
                ".opus" => CreateCompressedAudioConfig(filePath, AudioStreamContainerFormat.OGG_OPUS),
                ".flac" => CreateCompressedAudioConfig(filePath, AudioStreamContainerFormat.FLAC),
                ".alaw" => CreateCompressedAudioConfig(filePath, AudioStreamContainerFormat.ALAW),
                ".mulaw" => CreateCompressedAudioConfig(filePath, AudioStreamContainerFormat.MULAW),
                ".mp4" => CreateCompressedAudioConfig(filePath, AudioStreamContainerFormat.ANY),
                ".m4a" => CreateCompressedAudioConfig(filePath, AudioStreamContainerFormat.ANY),
                ".aac" => CreateCompressedAudioConfig(filePath, AudioStreamContainerFormat.ANY),
                _ => AudioConfig.FromWavFileInput(filePath) // Default to WAV for unsupported formats
            };
        }
        catch (Exception)
        {
            // Fallback to WAV if compressed format fails (e.g., GStreamer not available)
            return AudioConfig.FromWavFileInput(filePath);
        }
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
        var result = new Models.SpeechRecognitionResult
        {
            Text = speechResult.Text,
            Reason = speechResult.Reason.ToString(),
            Offset = (ulong)speechResult.OffsetInTicks,
            Duration = (ulong)speechResult.Duration.Ticks
        };

        // If the recognition was canceled, check for error details
        if (speechResult.Reason == ResultReason.Canceled)
        {
            var cancellation = CancellationDetails.FromResult(speechResult);
            var errorMessage = $"Speech recognition canceled. Reason: {cancellation.Reason}, ErrorCode: {cancellation.ErrorCode}, ErrorDetails: {cancellation.ErrorDetails}";

            // Add error details to the text field for debugging
            result.Text = errorMessage;
        }

        // For detailed format, we would typically parse the JSON result
        // But for now, we'll return the simple result
        if (format?.ToLowerInvariant() == "detailed")
        {
            // Parse speechResult.Properties for detailed info including NBest results
            return new Models.DetailedSpeechRecognitionResult
            {
                Text = result.Text,
                Reason = result.Reason,
                Offset = result.Offset,
                Duration = result.Duration,
                NBest = ExtractNBestResults(speechResult)
            };
        }

        return result;
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
            var jsonProperty = speechResult.Properties.GetProperty("SPEECHSDK_RECOGNITION_RESULT_JSON", null);

            if (!string.IsNullOrEmpty(jsonProperty))
            {
                var jsonResult = System.Text.Json.JsonDocument.Parse(jsonProperty);

                if (jsonResult.RootElement.TryGetProperty("NBest", out var nbestArray))
                {
                    foreach (var item in nbestArray.EnumerateArray())
                    {
                        var text = item.TryGetProperty("Display", out var displayProp) ? displayProp.GetString() :
                                   item.TryGetProperty("Lexical", out var lexicalProp) ? lexicalProp.GetString() : "";

                        var confidence = item.TryGetProperty("Confidence", out var confidenceProp) ? confidenceProp.GetDouble() : 0.0;

                        if (!string.IsNullOrEmpty(text))
                        {
                            nbestResults.Add(new NBestResult
                            {
                                Text = text,
                                Confidence = confidence
                            });
                        }
                    }
                }
            }
        }
        catch (System.Text.Json.JsonException)
        {
            // If JSON parsing fails, fall back to simple result
        }

        // If no NBest results were found, create a single result with the main text
        if (nbestResults.Count == 0)
        {
            nbestResults.Add(new NBestResult
            {
                Text = speechResult.Text,
                Confidence = speechResult.Reason == ResultReason.RecognizedSpeech ? 0.95 : 0.0 // Default confidence based on recognition success
            });
        }

        return nbestResults;
    }
}

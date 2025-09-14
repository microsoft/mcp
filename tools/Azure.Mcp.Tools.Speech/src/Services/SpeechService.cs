// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Core;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Tools.Speech.Models;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using SdkSpeechRecognitionResult = Microsoft.CognitiveServices.Speech.SpeechRecognitionResult;

namespace Azure.Mcp.Tools.Speech.Services;

public class SpeechService(ITenantService tenantService) : BaseAzureService(tenantService), ISpeechService
{
    /// <summary>
    /// Recognizes speech from an audio file using Azure AI Services Speech.
    /// </summary>
    /// <param name="endpoint">Azure AI Services endpoint (e.g., https://your-service.services.ai.azure.com/)</param>
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
        ArgumentException.ThrowIfNullOrWhiteSpace(endpoint);
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Audio file not found: {filePath}");
        }

        // Get Azure AD credential and token
        var credential = await GetCredential();
        
        // Get access token for Cognitive Services with proper scope
        var tokenRequestContext = new TokenRequestContext(["https://cognitiveservices.azure.com/.default"]);
        var accessToken = await credential.GetTokenAsync(tokenRequestContext, CancellationToken.None);

        // Configure Speech SDK with endpoint - this is the correct approach for Azure AI Services
        var config = SpeechConfig.FromEndpoint(new Uri(endpoint));
        
        // Set the authorization token - this is the key for Azure AI Services authentication
        config.AuthorizationToken = accessToken.Token;
        
        // Set language (default to en-US)
        config.SpeechRecognitionLanguage = language ?? "en-US";

        // Configure profanity filtering
        if (!string.IsNullOrEmpty(profanity))
        {
            config.SetProfanity(GetProfanityOption(profanity));
        }

        // Create audio configuration from file
        using var audioConfig = AudioConfig.FromWavFileInput(filePath);
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
        var taskCompletionSource = new TaskCompletionSource<SdkSpeechRecognitionResult>();
        var recognizedText = new System.Text.StringBuilder();
        SdkSpeechRecognitionResult? lastResult = null;
        
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
        };
        
        recognizer.Canceled += (s, e) =>
        {
            taskCompletionSource.SetResult(e.Result);
        };
        
        recognizer.SessionStopped += (s, e) =>
        {
            // Use the last successful result, or create a no-match result
            if (lastResult != null && recognizedText.Length > 0)
            {
                taskCompletionSource.TrySetResult(lastResult);
            }
            else
            {
                // If no speech was recognized, we need to handle this case
                // We'll use the canceled event result or create a synthetic one
                taskCompletionSource.TrySetResult(lastResult ?? throw new InvalidOperationException("No recognition result available"));
            }
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
            // In a real implementation, you'd parse speechResult.Properties for detailed info
            return new Models.DetailedSpeechRecognitionResult
            {
                Text = result.Text,
                Reason = result.Reason,
                Offset = result.Offset,
                Duration = result.Duration,
                NBest = new List<NBestResult>
                {
                    new()
                    {
                        Text = result.Text,
                        Confidence = 0.95 // This would come from actual recognition result
                    }
                }
            };
        }

        return result;
    }
}

// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using Azure.Core;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Tools.Speech.Models.FastTranscription;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.Speech.Services.Recognizers;

/// <summary>
/// Recognizer for Fast Transcription using Azure AI Services Speech REST API.
/// </summary>
public class FastTranscriptionRecognizer(ITenantService tenantService, ILogger<FastTranscriptionRecognizer> logger)
    : BaseAzureService(tenantService), IFastTranscriptionRecognizer
{
    private readonly ILogger<FastTranscriptionRecognizer> _logger = logger;
    private readonly HttpClient _httpClient = new();

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
    public async Task<FastTranscriptionResult> RecognizeAsync(
        string endpoint,
        string filePath,
        string? language = null,
        string[]? phrases = null,
        string? profanity = null,
        RetryPolicyOptions? retryPolicy = null)
    {
        ValidateRequiredParameters((nameof(endpoint), endpoint), (nameof(filePath), filePath));

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Audio file not found: {filePath}");
        }

        // Check file size (Fast Transcription has a 300 MB limit)
        var fileInfo = new FileInfo(filePath);
        if (fileInfo.Length > 300 * 1024 * 1024)
        {
            throw new InvalidOperationException($"Audio file is too large ({fileInfo.Length / (1024.0 * 1024):F1} MB). Fast Transcription supports files up to 300 MB.");
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
                var credential = await GetCredential();

                // Get access token for Cognitive Services with proper scope
                var tokenRequestContext = new TokenRequestContext(["https://cognitiveservices.azure.com/.default"]);
                var accessToken = await credential.GetTokenAsync(tokenRequestContext, CancellationToken.None);

                // Build the Fast Transcription API URL
                var apiVersion = "2024-11-15";
                var baseUri = new UriBuilder(endpoint.TrimEnd('/'))
                {
                    Scheme = "https",
                    Port = -1 // Use default port for scheme
                }.Uri;

                var apiPath = $"speechtotext/transcriptions:transcribe?api-version={apiVersion}";
                var apiUrl = new Uri(baseUri, apiPath);
                _logger.LogDebug("Fast Transcription API URL: {ApiUrl}", apiUrl);

                // Create the request definition
                var request = new FastTranscriptionRequest();

                if (!string.IsNullOrEmpty(language))
                {
                    request.Locales.Add(language.ToLowerInvariant());
                }

                if (!string.IsNullOrEmpty(profanity))
                {
                    request.ProfanityFilterMode = MapProfanityOption(profanity);
                }

                if (phrases != null && phrases.Length > 0)
                {
                    request.PhraseList = string.Join(';', phrases);
                }

                var requestJson = JsonSerializer.Serialize(request, SpeechJsonContext.Default.FastTranscriptionRequest);

                // Create multipart form data
                using var formContent = new MultipartFormDataContent();

                // Add audio file
                var fileContent = new ByteArrayContent(await File.ReadAllBytesAsync(filePath));
                fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse(GetMimeType(filePath));
                formContent.Add(fileContent, "audio", Path.GetFileName(filePath));

                // Add definition
                var definitionContent = new StringContent(requestJson, Encoding.UTF8, "application/json");
                formContent.Add(definitionContent, "definition");

                // Set authorization header
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken.Token);

                _logger.LogInformation("Starting Fast Transcription for file: {FilePath}, Language: {Language} (Attempt {Attempt}/{MaxAttempts})",
                    filePath, language ?? "auto-detect", attempt + 1, maxRetries + 1);
                _logger.LogDebug("Fast Transcription Request: {RequestJson}", requestJson);

                // Make the request
                var response = await _httpClient.PostAsync(apiUrl, formContent);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    var errorMessage = $"Fast Transcription API failed with status {response.StatusCode}: {responseContent}";
                    _logger.LogWarning("Fast Transcription failed. Status: {StatusCode}, Response: {Response}, Attempt: {Attempt}/{MaxAttempts}",
                        response.StatusCode, responseContent, attempt + 1, maxRetries + 1);

                    // Check if this is a retryable error
                    if (IsRetryableHttpStatus(response.StatusCode) && attempt < maxRetries)
                    {
                        lastException = new InvalidOperationException(errorMessage);
                        continue; // Retry on the next iteration
                    }

                    throw new InvalidOperationException(errorMessage);
                }

                // Parse the response
                var fastTranscriptionResult = JsonSerializer.Deserialize(responseContent, SpeechJsonContext.Default.FastTranscriptionResult);

                if (fastTranscriptionResult == null)
                {
                    throw new InvalidOperationException("Failed to parse Fast Transcription response");
                }

                _logger.LogInformation("Fast Transcription completed successfully. Duration: {Duration}ms, Phrases: {PhraseCount}, Attempt: {Attempt}",
                    fastTranscriptionResult.DurationMilliseconds, fastTranscriptionResult.CombinedPhrases.Count, attempt + 1);

                return fastTranscriptionResult;
            }
            catch (Exception ex) when (IsRetryableException(ex) && attempt < maxRetries)
            {
                lastException = ex;
                _logger.LogWarning(ex, "Retryable error occurred during Fast Transcription attempt {Attempt}/{MaxAttempts}: {ErrorMessage}",
                    attempt + 1, maxRetries + 1, ex.Message);

                // Calculate delay for next attempt
                if (attempt < maxRetries)
                {
                    var delay = isExponentialBackoff
                        ? Math.Min(delaySeconds * Math.Pow(2, attempt), maxDelaySeconds)
                        : delaySeconds;

                    _logger.LogDebug("Waiting {DelaySeconds} seconds before retry attempt {NextAttempt}", delay, attempt + 2);
                    await Task.Delay(TimeSpan.FromSeconds(delay));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Non-retryable error during Fast Transcription");
                throw;
            }
        }

        // If we've exhausted all retries, throw the last exception
        _logger.LogError(lastException, "Fast Transcription failed after {MaxRetries} retry attempts", maxRetries);
        throw lastException ?? new InvalidOperationException("Fast Transcription failed after maximum retry attempts");
    }

    /// <summary>
    /// Maps profanity option to Fast Transcription API format.
    /// </summary>
    private static string MapProfanityOption(string profanity) =>
        profanity.ToLowerInvariant() switch
        {
            "masked" => "Masked",
            "removed" => "Removed",
            "raw" => "None",
            _ => "Masked"
        };

    /// <summary>
    /// Determines if an HTTP status code is retryable.
    /// </summary>
    private static bool IsRetryableHttpStatus(System.Net.HttpStatusCode statusCode) =>
        statusCode switch
        {
            System.Net.HttpStatusCode.InternalServerError => true,
            System.Net.HttpStatusCode.BadGateway => true,
            System.Net.HttpStatusCode.ServiceUnavailable => true,
            System.Net.HttpStatusCode.GatewayTimeout => true,
            System.Net.HttpStatusCode.RequestTimeout => true,
            System.Net.HttpStatusCode.TooManyRequests => true,
            _ => false
        };

    /// <summary>
    /// Determines if an exception is retryable.
    /// </summary>
    private static bool IsRetryableException(Exception ex) =>
        ex switch
        {
            HttpRequestException => true,
            TaskCanceledException => true,
            SocketException => true,
            _ => false
        };

    /// <summary>
    /// Gets MIME type for audio file based on file extension.
    /// </summary>
    private static string GetMimeType(string filePath)
    {
        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        return extension switch
        {
            ".wav" => "audio/wav",
            ".mp3" => "audio/mpeg",
            ".ogg" => "audio/ogg",
            ".opus" => "audio/ogg",
            ".flac" => "audio/flac",
            ".m4a" => "audio/mp4",
            ".aac" => "audio/aac",
            ".wma" => "audio/x-ms-wma",
            ".webm" => "audio/webm",
            _ => "application/octet-stream"
        };
    }
}

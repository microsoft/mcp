// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net.Http.Headers;
using System.Text;
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

        try
        {
            // Get Azure AD credential and token
            var credential = await GetCredential();

            // Get access token for Cognitive Services with proper scope
            var tokenRequestContext = new TokenRequestContext(["https://cognitiveservices.azure.com/.default"]);
            var accessToken = await credential.GetTokenAsync(tokenRequestContext, CancellationToken.None);

            // Build the Fast Transcription API URL
            var apiVersion = "2024-11-15";
            var apiUrl = $"https://{endpoint}/speechtotext/transcriptions:transcribe?api-version={apiVersion}";

            // Create the request definition
            var request = new FastTranscriptionRequest();

            if (!string.IsNullOrEmpty(language))
            {
                request.Locales.Add(language);
            }

            if (!string.IsNullOrEmpty(profanity))
            {
                request.ProfanityFilterMode = MapProfanityOption(profanity);
            }

            if (phrases != null && phrases.Length > 0)
            {
                request.PhrasesList = phrases.ToList();
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

            _logger.LogInformation("Starting Fast Transcription for file: {FilePath}, Language: {Language}", filePath, language ?? "auto-detect");
            _logger.LogDebug("Fast Transcription Request: {RequestJson}", requestJson);

            // Make the request
            var response = await _httpClient.PostAsync(apiUrl, formContent);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Fast Transcription failed. Status: {StatusCode}, Response: {Response}",
                    response.StatusCode, responseContent);
                throw new InvalidOperationException($"Fast Transcription API failed with status {response.StatusCode}: {responseContent}");
            }

            // Parse the response
            var fastTranscriptionResult = JsonSerializer.Deserialize(responseContent, SpeechJsonContext.Default.FastTranscriptionResult);

            if (fastTranscriptionResult == null)
            {
                throw new InvalidOperationException("Failed to parse Fast Transcription response");
            }

            _logger.LogInformation("Fast Transcription completed successfully. Duration: {Duration}ms, Phrases: {PhraseCount}",
                fastTranscriptionResult.DurationMilliseconds, fastTranscriptionResult.CombinedPhrases.Count);

            return fastTranscriptionResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during Fast Transcription");
            throw;
        }
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

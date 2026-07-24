// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.Speech.Models;
using Azure.Mcp.Tools.Speech.Options.Stt;
using Azure.Mcp.Tools.Speech.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.Speech.Commands.Stt;

[CommandMetadata(
    Id = "c725eb52-ca2c-4fe4-9422-935e7557b701",
    Name = "recognize",
    Title = "Recognize Speech from Audio File",
    Description = """
        Recognize speech from an audio file using Azure AI Services Speech. This command takes an audio file and converts it to text using advanced speech recognition capabilities.
        You must provide an Azure AI Services endpoint (e.g., https://your-service.cognitiveservices.azure.com/) and a path to the audio file.
        Supported audio formats include WAV, MP3, OPUS/OGG, FLAC, ALAW, MULAW, MP4, M4A, and AAC. Compressed formats require GStreamer to be installed on the system.
        Optional parameters include language specification, phrase hints for better accuracy, output format (simple or detailed), and profanity filtering.
        """,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = true)]
public sealed class SttRecognizeCommand(ILogger<SttRecognizeCommand> logger, ISpeechService speechService)
    : BaseSpeechCommand<SttRecognizeOptions, SttRecognizeCommand.SttRecognizeCommandResult>()
{
    public sealed record SttRecognizeCommandResult(SpeechRecognitionResult Result);

    private readonly ILogger<SttRecognizeCommand> _logger = logger;
    private readonly ISpeechService _speechService = speechService;

    public override void ValidateOptions(SttRecognizeOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);

        // Canonicalize and validate the file path (rejects UNC/device paths, traversal)
        string canonicalPath;
        try
        {
            canonicalPath = FilePathValidator.ValidateAndCanonicalize(options.File);
        }
        catch (ArgumentException ex)
        {
            validationResult.Errors.Add($"Invalid audio file path: {ex.Message}");
            return;
        }

        // Validate file path exists
        if (!File.Exists(canonicalPath))
        {
            validationResult.Errors.Add($"Audio file not found: {canonicalPath}");
        }
        else
        {
            // Validate file extension
            var extension = Path.GetExtension(canonicalPath).ToLowerInvariant();
            var supportedExtensions = new[] { ".wav", ".mp3", ".ogg", ".flac", ".alaw", ".mulaw", ".mp4", ".m4a", ".aac" };
            if (!supportedExtensions.Contains(extension))
            {
                validationResult.Errors.Add($"Unsupported audio file format: {extension}. Only {string.Join(", ", supportedExtensions)} are supported.");
            }
        }

        // Validate format option if provided
        if (!string.IsNullOrEmpty(options.Format))
        {
            if (options.Format != "simple" && options.Format != "detailed")
            {
                validationResult.Errors.Add("Format must be 'simple' or 'detailed'.");
            }
        }

        // Validate profanity option if provided
        if (!string.IsNullOrEmpty(options.Profanity))
        {
            if (options.Profanity != "masked" && options.Profanity != "removed" && options.Profanity != "raw")
            {
                validationResult.Errors.Add("Profanity filter must be 'masked', 'removed', or 'raw'.");
            }
        }
    }

    public override void PostBindOptions(SttRecognizeOptions options)
    {
        base.PostBindOptions(options);
        // Process phrases to support comma-separated values
        if (options.Phrases != null && options.Phrases.Length > 0)
        {
            var processedPhrases = new List<string>();
            foreach (var phrase in options.Phrases)
            {
                if (string.IsNullOrWhiteSpace(phrase))
                    continue;

                // Split by comma and trim whitespace
                var splitPhrases = phrase.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(p => p.Trim())
                    .Where(p => !string.IsNullOrEmpty(p));

                processedPhrases.AddRange(splitPhrases);
            }
            options.Phrases = processedPhrases.Count > 0 ? processedPhrases.ToArray() : null;
        }
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, SttRecognizeOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _speechService.RecognizeSpeechFromFile(
                options.Endpoint,
                options.File,
                options.Language,
                options.Phrases,
                options.Format,
                options.Profanity,
                options.RetryPolicy,
                cancellationToken);

            _logger.LogInformation(
                "Successfully recognized speech from file: {File}. Full text length: {Length}, Segments: {SegmentCount}",
                options.File,
                result.RealtimeContinuousResult?.FullText?.Length ?? 0,
                result.RealtimeContinuousResult?.Segments.Count ?? 0);

            context.Response.Status = HttpStatusCode.OK;
            context.Response.Message = "Speech recognition completed successfully.";
            context.Response.Results = ResponseResult.Create(
                new(result),
                SpeechJsonContext.Default.SttRecognizeCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recognizing speech from file: {File}", options.File);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        FileNotFoundException fileEx => $"Audio file not found: {fileEx.Message}",
        ArgumentException argEx => $"Invalid parameter: {argEx.Message}",
        UnauthorizedAccessException => "Access denied. Check Azure AI Services credentials and permissions.",
        _ => base.GetErrorMessage(ex)
    };

    protected override HttpStatusCode GetStatusCode(Exception ex) => ex switch
    {
        ArgumentException => HttpStatusCode.BadRequest,
        FileNotFoundException => HttpStatusCode.NotFound,
        UnauthorizedAccessException => HttpStatusCode.Unauthorized,
        HttpRequestException => HttpStatusCode.ServiceUnavailable,
        TimeoutException => HttpStatusCode.GatewayTimeout,
        InvalidOperationException => HttpStatusCode.InternalServerError,
        _ => base.GetStatusCode(ex)
    };
}

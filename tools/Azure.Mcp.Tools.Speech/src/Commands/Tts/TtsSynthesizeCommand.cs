// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.Speech.Models;
using Azure.Mcp.Tools.Speech.Options.Tts;
using Azure.Mcp.Tools.Speech.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.Speech.Commands.Tts;

[CommandMetadata(
    Id = "d6f6687f-feee-4e15-9b98-71aea4076e04",
    Name = "synthesize",
    Title = "Synthesize Speech from Text",
    Description = """
        Convert text to speech using Azure AI Services Speech. This command takes text input and generates an audio file using advanced neural text-to-speech capabilities.
        You must provide an Azure AI Services endpoint (e.g., https://your-service.cognitiveservices.azure.com/), the text to convert, and an output file path.
        Optional parameters include language specification (default: en-US), voice selection, audio output format (default: Riff24Khz16BitMonoPcm), and custom voice endpoint ID.
        The command supports a wide variety of output formats and neural voices for natural-sounding speech synthesis.
        """,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = true)]
public sealed partial class TtsSynthesizeCommand(ILogger<TtsSynthesizeCommand> logger, ISpeechService speechService)
    : BaseSpeechCommand<TtsSynthesizeOptions, TtsSynthesizeCommand.TtsSynthesizeCommandResult>()
{
    public sealed record TtsSynthesizeCommandResult(SynthesisResult Result);

    private static readonly HashSet<string> s_supportedExtensions = [".wav", ".mp3", ".ogg", ".raw"];
    private readonly ILogger<TtsSynthesizeCommand> _logger = logger;
    private readonly ISpeechService _speechService = speechService;

    public override void ValidateOptions(TtsSynthesizeOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);

        // Validate text is not empty
        if (string.IsNullOrWhiteSpace(options.Text))
        {
            validationResult.Errors.Add("Text cannot be empty or whitespace.");
        }

        // Validate output file path
        if (string.IsNullOrWhiteSpace(options.OutputAudio))
        {
            validationResult.Errors.Add("Output file path cannot be empty.");
        }
        else
        {
            // Canonicalize and validate the output path (rejects UNC/device paths, traversal)
            try
            {
                var canonicalPath = FilePathValidator.ValidateAndCanonicalize(options.OutputAudio);
                // Check if file already exists (don't allow overwriting)
                if (File.Exists(canonicalPath))
                {
                    validationResult.Errors.Add($"Output file already exists: {canonicalPath}. Please specify a different file path or delete the existing file.");
                }

                // Validate file extension
                var extension = Path.GetExtension(canonicalPath).ToLowerInvariant();

                if (!s_supportedExtensions.Contains(extension))
                {
                    validationResult.Errors.Add($"Unsupported output file format: {extension}. Only {string.Join(", ", s_supportedExtensions)} are supported.");
                }
            }
            catch (ArgumentException ex)
            {
                validationResult.Errors.Add($"Invalid output file path: {ex.Message}");
            }
        }

        // Validate language format if provided
        if (!string.IsNullOrEmpty(options.Language))
        {
            // Basic validation: language should be in format like "en-US", "es-ES"
            if (!LanguageRegex().IsMatch(options.Language))
            {
                validationResult.Errors.Add($"Language must be in format 'xx-XX' (e.g., 'en-US', 'es-ES'). Got: {options.Language}");
            }
        }
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, TtsSynthesizeOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _speechService.SynthesizeSpeechToFile(
                options.Endpoint,
                options.Text,
                options.OutputAudio,
                options.Language,
                options.Voice,
                options.Format,
                options.EndpointId,
                options.RetryPolicy,
                cancellationToken);

            _logger.LogInformation(
                "Successfully synthesized speech to file: {File}. Audio size: {Size} bytes, Voice: {Voice}",
                result.FilePath,
                result.AudioSize,
                result.Voice);

            context.Response.Status = HttpStatusCode.OK;
            context.Response.Message = "Speech synthesis completed successfully.";
            context.Response.Results = ResponseResult.Create(
                new(result),
                SpeechJsonContext.Default.TtsSynthesizeCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error synthesizing speech to file: {File}", options.OutputAudio);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        ArgumentException argEx => $"Invalid parameter: {argEx.Message}",
        UnauthorizedAccessException => "Access denied. Check Azure AI Services credentials and permissions.",
        DirectoryNotFoundException => "Output directory not found. Ensure the directory exists before synthesizing.",
        IOException ioEx => $"File operation failed: {ioEx.Message}",
        _ => base.GetErrorMessage(ex)
    };

    protected override HttpStatusCode GetStatusCode(Exception ex) => ex switch
    {
        ArgumentException => HttpStatusCode.BadRequest,
        UnauthorizedAccessException => HttpStatusCode.Unauthorized,
        DirectoryNotFoundException => HttpStatusCode.NotFound,
        IOException => HttpStatusCode.InternalServerError,
        HttpRequestException => HttpStatusCode.ServiceUnavailable,
        TimeoutException => HttpStatusCode.GatewayTimeout,
        InvalidOperationException => HttpStatusCode.InternalServerError,
        _ => base.GetStatusCode(ex)
    };

    [System.Text.RegularExpressions.GeneratedRegex(@"^[a-z]{2}-[A-Z]{2}$")]
    private static partial System.Text.RegularExpressions.Regex LanguageRegex();
}

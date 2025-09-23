// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Core.Models.Option;
using Azure.Mcp.Tools.Speech.Models;
using Azure.Mcp.Tools.Speech.Options;
using Azure.Mcp.Tools.Speech.Options.Stt;
using Azure.Mcp.Tools.Speech.Services;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.Speech.Commands.Stt;

public sealed class SttRecognizeCommand(ILogger<SttRecognizeCommand> logger) : BaseSpeechCommand<SttRecognizeOptions>()
{
    internal record SttRecognizeCommandResult(ContinuousRecognitionResult Result);

    private const string CommandTitle = "Recognize Speech from Audio File";
    private readonly ILogger<SttRecognizeCommand> _logger = logger;

    public override string Name => "recognize";

    public override string Description =>
        """
        Recognize speech from an audio file using Azure AI Services Speech. This command takes an audio file and converts it to text using advanced speech recognition capabilities.
        You must provide an Azure AI Services endpoint (e.g., https://your-service.cognitiveservices.azure.com/) and a path to the audio file.
        Supported audio formats include WAV, MP3, OPUS/OGG, FLAC, ALAW, MULAW, MP4, M4A, and AAC. Compressed formats require GStreamer to be installed on the system.
        Optional parameters include language specification, phrase hints for better accuracy, output format (simple or detailed), and profanity filtering.
        """;

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new()
    {
        Destructive = false,
        Idempotent = true,
        OpenWorld = false,
        ReadOnly = true,
        LocalRequired = true, // Requires local audio file access
        Secret = false
    };

    protected override void RegisterOptions(Command command)
    {
        _logger.LogDebug("RegisterOptions");

        base.RegisterOptions(command);

        var fileOption = SpeechOptionDefinitions.File.AsRequired();
        command.Options.Add(fileOption);
        var languageOption = SpeechOptionDefinitions.Language.AsOptional();
        command.Options.Add(languageOption);
        var phrasesOption = SpeechOptionDefinitions.Phrases.AsOptional();
        command.Options.Add(phrasesOption);
        var formatOption = SpeechOptionDefinitions.Format.AsOptional();
        command.Options.Add(formatOption);
        var profanityOption = SpeechOptionDefinitions.Profanity.AsOptional();
        command.Options.Add(profanityOption);

        // Command-level validation for file-specific options
        command.Validators.Add(commandResult =>
        {
            // Validate file is provided
            if (!commandResult.HasOptionResult(SpeechOptionDefinitions.File.Name))
            {
                commandResult.AddError("Missing Required options: --file");
                return;
            }

            // Validate file path is not empty
            var fileValue = commandResult.GetValueOrDefault(fileOption);
            if (string.IsNullOrWhiteSpace(fileValue))
            {
                commandResult.AddError("Audio file path cannot be empty.");
                return;
            }

            // Validate file exists
            if (!File.Exists(fileValue))
            {
                commandResult.AddError($"Audio file not found: {fileValue}");
                return;
            }

            // Validate file extension
            var extension = Path.GetExtension(fileValue).ToLowerInvariant();
            var supportedExtensions = new[] { ".wav", ".mp3", ".ogg", ".flac", ".alaw", ".mulaw", ".mp4", ".m4a", ".aac" };
            if (!supportedExtensions.Contains(extension))
            {
                commandResult.AddError($"Unsupported audio file format: {extension}. Only {string.Join(", ", supportedExtensions)} are supported.");
                return;
            }


            // Validate format option if provided
            var formatValue = commandResult.GetValueOrDefault<string?>(formatOption);
            if (!string.IsNullOrEmpty(formatValue))
            {
                if (formatValue != "simple" && formatValue != "detailed")
                {
                    commandResult.AddError("Format must be 'simple' or 'detailed'.");
                    return;
                }
            }

            // Validate profanity option if provided
            var profanityValue = commandResult.GetValueOrDefault<string?>(profanityOption);
            if (!string.IsNullOrEmpty(profanityValue))
            {
                if (profanityValue != "masked" && profanityValue != "removed" && profanityValue != "raw")
                {
                    commandResult.AddError("Profanity filter must be 'masked', 'removed', or 'raw'.");
                }
            }
        });
    }
    protected override SttRecognizeOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.File = parseResult.GetValueOrDefault<string>(SpeechOptionDefinitions.File.Name);
        options.Language = parseResult.GetValueOrDefault<string?>(SpeechOptionDefinitions.Language.Name);

        // Process phrases to support comma-separated values
        var rawPhrases = parseResult.GetValueOrDefault<string[]?>(SpeechOptionDefinitions.Phrases.Name);
        if (rawPhrases != null && rawPhrases.Length > 0)
        {
            var processedPhrases = new List<string>();
            foreach (var phrase in rawPhrases)
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
        else
        {
            options.Phrases = rawPhrases;
        }

        options.Format = parseResult.GetValueOrDefault<string?>(SpeechOptionDefinitions.Format.Name);
        options.Profanity = parseResult.GetValueOrDefault<string?>(SpeechOptionDefinitions.Profanity.Name);

        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return context.Response;
        }

        var options = BindOptions(parseResult);

        try
        {
            var speechService = context.GetService<ISpeechService>();
            var result = await speechService.RecognizeSpeechFromFile(
                options.Endpoint!,
                options.File!,
                options.Language,
                options.Phrases,
                options.Format,
                options.Profanity,
                options.RetryPolicy);

            _logger.LogInformation(
                "Successfully recognized speech from file: {File}. Full text length: {Length}, Segments: {SegmentCount}",
                options.File,
                result.FullText?.Length ?? 0,
                result.Segments.Count);

            context.Response.Status = HttpStatusCode.OK;
            context.Response.Message = "Speech recognition completed successfully.";
            context.Response.Results = ResponseResult.Create(
                new SttRecognizeCommandResult(result),
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

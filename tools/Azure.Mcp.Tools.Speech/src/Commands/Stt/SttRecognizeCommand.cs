// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Extensions;
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

    private readonly Option<string> _fileOption = SpeechOptionDefinitions.File;
    private readonly Option<string?> _languageOption = SpeechOptionDefinitions.Language;
    private readonly Option<string[]?> _phrasesOption = SpeechOptionDefinitions.Phrases;
    private readonly Option<string?> _formatOption = SpeechOptionDefinitions.Format;
    private readonly Option<string?> _profanityOption = SpeechOptionDefinitions.Profanity;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(_fileOption);
        command.Options.Add(_languageOption);
        command.Options.Add(_phrasesOption);
        command.Options.Add(_formatOption);
        command.Options.Add(_profanityOption);

        // Command-level validation for file-specific options
        command.Validators.Add(commandResult =>
        {
            // Validate file is provided
            if (!commandResult.HasOptionResult(_fileOption))
            {
                commandResult.AddError("Audio file path is required.");
                return;
            }

            if (commandResult.TryGetValue(_fileOption, out var fileValue) && !string.IsNullOrWhiteSpace(fileValue))
            {
                // Validate file exists
                if (!File.Exists(fileValue))
                {
                    commandResult.AddError($"Audio file not found: {fileValue}");
                    return;
                }
            }

            // Validate format option if provided
            if (commandResult.TryGetValue(_formatOption, out var formatValue) && !string.IsNullOrEmpty(formatValue))
            {
                if (formatValue != "simple" && formatValue != "detailed")
                {
                    commandResult.AddError("Format must be 'simple' or 'detailed'.");
                    return;
                }
            }

            // Validate profanity option if provided
            if (commandResult.TryGetValue(_profanityOption, out var profanityValue) && !string.IsNullOrEmpty(profanityValue))
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
        options.File = parseResult.GetValueOrDefault(_fileOption);
        options.Language = parseResult.GetValueOrDefault(_languageOption);

        // Process phrases to support comma-separated values
        var rawPhrases = parseResult.GetValueOrDefault(_phrasesOption);
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

        options.Format = parseResult.GetValueOrDefault(_formatOption);
        options.Profanity = parseResult.GetValueOrDefault(_profanityOption);
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

            context.Response.Status = 200;
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

    protected override int GetStatusCode(Exception ex) => ex switch
    {
        ArgumentException => 400,
        FileNotFoundException => 404,
        UnauthorizedAccessException => 401,
        HttpRequestException => 503,
        TimeoutException => 504,
        InvalidOperationException => 500,
        _ => base.GetStatusCode(ex)
    };
}

// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Tools.Speech.Models;
using Azure.Mcp.Tools.Speech.Options;
using Azure.Mcp.Tools.Speech.Options.Tts;
using Azure.Mcp.Tools.Speech.Services;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.Speech.Commands.Tts;

public sealed class TtsSynthesizeCommand(ILogger<TtsSynthesizeCommand> logger) : BaseSpeechCommand<TtsSynthesizeOptions>()
{
    internal record TtsSynthesizeCommandResult(SynthesisResult Result);

    private const string CommandTitle = "Synthesize Speech from Text";
    private readonly ILogger<TtsSynthesizeCommand> _logger = logger;

    public override string Name => "synthesize";

    public override string Id => "d6f6687f-feee-4e15-9b98-71aea4076e04";

    public override string Description =>
        """
        Convert text to speech using Azure AI Services Speech. This command takes text input and generates an audio file using advanced neural text-to-speech capabilities.
        You must provide an Azure AI Services endpoint (e.g., https://your-service.cognitiveservices.azure.com/), the text to convert, and an output file path.
        Optional parameters include language specification (default: en-US), voice selection, audio output format (default: Riff24Khz16BitMonoPcm), and custom voice endpoint ID.
        The command supports a wide variety of output formats and neural voices for natural-sounding speech synthesis.
        """;

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new()
    {
        Destructive = false,
        Idempotent = true,
        OpenWorld = false,
        ReadOnly = false,
        LocalRequired = true, // Requires local file output
        Secret = false
    };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);

        command.Options.Add(SpeechOptionDefinitions.Text);
        command.Options.Add(SpeechOptionDefinitions.OutputAudio);
        command.Options.Add(SpeechOptionDefinitions.Language);
        command.Options.Add(SpeechOptionDefinitions.Voice);
        command.Options.Add(SpeechOptionDefinitions.Format);
        command.Options.Add(SpeechOptionDefinitions.EndpointId);

        // Command-level validation
        command.Validators.Add(commandResult =>
        {
            var textValue = commandResult.GetValueOrDefault<string>(SpeechOptionDefinitions.Text);

            // Validate text is not empty
            if (string.IsNullOrWhiteSpace(textValue))
            {
                commandResult.AddError("Text cannot be empty or whitespace.");
            }

            var fileValue = commandResult.GetValueOrDefault<string>(SpeechOptionDefinitions.OutputAudio);

            // Validate output file path
            if (string.IsNullOrWhiteSpace(fileValue))
            {
                commandResult.AddError("Output file path cannot be empty.");
            }
            else
            {
                // Validate file extension
                var extension = Path.GetExtension(fileValue).ToLowerInvariant();
                var supportedExtensions = new HashSet<string>
                {
                    ".wav", ".mp3", ".ogg", ".raw"
                };

                if (!supportedExtensions.Contains(extension))
                {
                    commandResult.AddError($"Unsupported output file format: {extension}. Only {string.Join(", ", supportedExtensions)} are supported.");
                }
            }

            // Validate language format if provided
            var languageValue = commandResult.GetValueOrDefault<string?>(SpeechOptionDefinitions.Language);
            if (!string.IsNullOrEmpty(languageValue))
            {
                // Basic validation: language should be in format like "en-US", "es-ES"
                if (!System.Text.RegularExpressions.Regex.IsMatch(languageValue, @"^[a-z]{2}-[A-Z]{2}$"))
                {
                    commandResult.AddError($"Language must be in format 'xx-XX' (e.g., 'en-US', 'es-ES'). Got: {languageValue}");
                }
            }
        });
    }

    protected override TtsSynthesizeOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Text = parseResult.GetValueOrDefault<string>(SpeechOptionDefinitions.Text.Name);
        options.OutputAudio = parseResult.GetValueOrDefault<string>(SpeechOptionDefinitions.OutputAudio.Name);
        options.Language = parseResult.GetValueOrDefault<string?>(SpeechOptionDefinitions.Language.Name);
        options.Voice = parseResult.GetValueOrDefault<string?>(SpeechOptionDefinitions.Voice.Name);
        options.Format = parseResult.GetValueOrDefault<string?>(SpeechOptionDefinitions.Format.Name);
        options.EndpointId = parseResult.GetValueOrDefault<string?>(SpeechOptionDefinitions.EndpointId.Name);

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
            var result = await speechService.SynthesizeSpeechToFile(
                options.Endpoint!,
                options.Text!,
                options.OutputAudio!,
                options.Language,
                options.Voice,
                options.Format,
                options.EndpointId,
                options.RetryPolicy);

            _logger.LogInformation(
                "Successfully synthesized speech to file: {File}. Audio size: {Size} bytes, Voice: {Voice}",
                result.FilePath,
                result.AudioSize,
                result.Voice);

            context.Response.Status = HttpStatusCode.OK;
            context.Response.Message = "Speech synthesis completed successfully.";
            context.Response.Results = ResponseResult.Create(
                new TtsSynthesizeCommandResult(result),
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
}

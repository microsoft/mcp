// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Net;
using System.Text.Json;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.Speech.Commands.Tts;
using Azure.Mcp.Tools.Speech.Models;
using Azure.Mcp.Tools.Speech.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Speech.UnitTests.Tts;

public class TtsSynthesizeCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ISpeechService _speechService;
    private readonly ILogger<TtsSynthesizeCommand> _logger;
    private readonly TtsSynthesizeCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;
    private readonly string _knownEndpoint = "https://eastus.cognitiveservices.azure.com/";
    private readonly string _knownSubscription = "sub123";

    public TtsSynthesizeCommandTests()
    {
        _speechService = Substitute.For<ISpeechService>();
        _logger = Substitute.For<ILogger<TtsSynthesizeCommand>>();

        var collection = new ServiceCollection().AddSingleton(_speechService);

        _serviceProvider = collection.BuildServiceProvider();
        _command = new(_logger);
        _context = new(_serviceProvider);
        _commandDefinition = _command.GetCommand();
    }

    [Fact]
    public void Constructor_WithValidLogger_ShouldCreateInstance()
    {
        var command = new TtsSynthesizeCommand(_logger);
        Assert.NotNull(command);
        Assert.Equal("synthesize", command.Name);
    }

    [Fact]
    public void Properties_ShouldHaveExpectedValues()
    {
        Assert.Equal("synthesize", _command.Name);
        Assert.Equal("Synthesize Speech from Text", _command.Title);
        Assert.NotEmpty(_command.Description);
        Assert.False(_command.Metadata.Destructive);
        Assert.True(_command.Metadata.Idempotent);
        Assert.False(_command.Metadata.OpenWorld);
        Assert.False(_command.Metadata.ReadOnly);
        Assert.True(_command.Metadata.LocalRequired);
        Assert.False(_command.Metadata.Secret);
    }

    [Theory]
    [InlineData("", false, "Missing Required options: --endpoint, --text, --file")]
    [InlineData("--subscription sub123", false, "Missing Required options: --endpoint, --text, --file")]
    [InlineData("--subscription sub123 --endpoint https://test.cognitiveservices.azure.com/", false, "Missing Required options: --text, --file")]
    [InlineData("--subscription sub123 --endpoint https://test.cognitiveservices.azure.com/ --text Hello", false, "Missing Required options: --file")]
    [InlineData("--subscription sub123 --endpoint https://test.cognitiveservices.azure.com/ --text Hello --file output.txt", false, "Output file must have .wav extension")]
    [InlineData("--subscription sub123 --endpoint https://test.cognitiveservices.azure.com/ --text Hello --file output.wav --language invalid", false, "Language must be in format 'xx-XX'")]
    public async Task ExecuteAsync_ValidatesInput(string args, bool shouldSucceed, string expectedError)
    {
        var parseResult = _commandDefinition.Parse(args.Split(' ', StringSplitOptions.RemoveEmptyEntries));
        var response = await _command.ExecuteAsync(_context, parseResult);

        if (shouldSucceed)
        {
            Assert.Equal(HttpStatusCode.OK, response.Status);
        }
        else
        {
            Assert.NotEqual(HttpStatusCode.OK, response.Status);
            Assert.Contains(expectedError, response.Message, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public async Task ExecuteAsync_WithValidParameters_ShouldSucceed()
    {
        // Arrange
        var text = "HelloWorld";
        var outputFile = "test-output.wav";

        var expectedResult = new SynthesisResult
        {
            FilePath = outputFile,
            Duration = 1000000,
            AudioLength = 48000,
            Format = "Riff24Khz16BitMonoPcm",
            Voice = "en-US-JennyNeural",
            Language = "en-US"
        };

        _speechService.SynthesizeSpeechToFile(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>())
            .Returns(expectedResult);

        try
        {
            // Act
            var args = $"--subscription {_knownSubscription} --endpoint {_knownEndpoint} --text {text} --file {outputFile}";
            var parseResult = _commandDefinition.Parse(args.Split(' ', StringSplitOptions.RemoveEmptyEntries));
            var response = await _command.ExecuteAsync(_context, parseResult);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.Status);
            Assert.NotNull(response.Results);

            var result = JsonSerializer.Deserialize(
                JsonSerializer.Serialize(response.Results), SpeechJsonContext.Default.TtsSynthesizeCommandResult);
            Assert.NotNull(result);
            Assert.Equal(outputFile, result.Result.FilePath);
            Assert.Equal(48000, result.Result.AudioLength);
        }
        finally
        {
            // Clean up
            if (File.Exists(outputFile))
            {
                File.Delete(outputFile);
            }
        }
    }

    [Fact]
    public async Task ExecuteAsync_WithAllOptionalParameters_ShouldPassThemCorrectly()
    {
        // Arrange
        var text = "HolaMundo";
        var outputFile = "test-output-spanish.wav";
        var language = "es-ES";
        var voice = "es-ES-ElviraNeural";
        var format = "Audio16Khz32KBitRateMonoMp3";
        var endpointId = "custom-endpoint-id";

        var expectedResult = new SynthesisResult
        {
            FilePath = outputFile,
            Duration = 1000000,
            AudioLength = 32000,
            Format = format,
            Voice = voice,
            Language = language
        };

        _speechService.SynthesizeSpeechToFile(
            Arg.Is(_knownEndpoint),
            Arg.Is(text),
            Arg.Is(outputFile),
            Arg.Is(language),
            Arg.Is(voice),
            Arg.Is(format),
            Arg.Is(endpointId),
            Arg.Any<RetryPolicyOptions?>())
            .Returns(expectedResult);

        try
        {
            // Act
            var args = $"--subscription {_knownSubscription} --endpoint {_knownEndpoint} --text {text} --file {outputFile} --language {language} --voice {voice} --format {format} --endpointId {endpointId}";
            var parseResult = _commandDefinition.Parse(args.Split(' ', StringSplitOptions.RemoveEmptyEntries));
            var response = await _command.ExecuteAsync(_context, parseResult);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.Status);

            await _speechService.Received(1).SynthesizeSpeechToFile(
                _knownEndpoint,
                text,
                outputFile,
                language,
                voice,
                format,
                endpointId,
                Arg.Any<RetryPolicyOptions?>());
        }
        finally
        {
            // Clean up
            if (File.Exists(outputFile))
            {
                File.Delete(outputFile);
            }
        }
    }

    [Fact]
    public async Task ExecuteAsync_ServiceThrowsException_ShouldHandleGracefully()
    {
        // Arrange
        var text = "HelloWorld";
        var outputFile = "test-output-error.wav";

        _speechService.SynthesizeSpeechToFile(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>())
            .ThrowsAsync(new InvalidOperationException("Synthesis failed"));

        try
        {
            // Act
            var args = $"--subscription {_knownSubscription} --endpoint {_knownEndpoint} --text {text} --file {outputFile}";
            var parseResult = _commandDefinition.Parse(args.Split(' ', StringSplitOptions.RemoveEmptyEntries));
            var response = await _command.ExecuteAsync(_context, parseResult);

            // Assert
            Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
            Assert.Contains("synthesis failed", response.Message.ToLower());
        }
        finally
        {
            // Clean up
            if (File.Exists(outputFile))
            {
                File.Delete(outputFile);
            }
        }
    }

    [Fact]
    public async Task ExecuteAsync_UnauthorizedException_ShouldReturnUnauthorizedStatus()
    {
        // Arrange
        var text = "HelloWorld";
        var outputFile = "test-output-unauth.wav";

        _speechService.SynthesizeSpeechToFile(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>())
            .ThrowsAsync(new UnauthorizedAccessException("Access denied"));

        try
        {
            // Act
            var args = $"--subscription {_knownSubscription} --endpoint {_knownEndpoint} --text {text} --file {outputFile}";
            var parseResult = _commandDefinition.Parse(args.Split(' ', StringSplitOptions.RemoveEmptyEntries));
            var response = await _command.ExecuteAsync(_context, parseResult);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.Status);
        }
        finally
        {
            // Clean up
            if (File.Exists(outputFile))
            {
                File.Delete(outputFile);
            }
        }
    }
}


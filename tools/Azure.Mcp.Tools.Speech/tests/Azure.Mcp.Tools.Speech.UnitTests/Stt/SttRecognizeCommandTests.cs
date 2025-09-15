// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Text.Json;
using Azure.Mcp.Core.Models;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.Speech.Commands.Stt;
using Azure.Mcp.Tools.Speech.Models;
using Azure.Mcp.Tools.Speech.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Speech.UnitTests.Stt;

public class SttRecognizeCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ISpeechService _speechService;
    private readonly ILogger<SttRecognizeCommand> _logger;
    private readonly SttRecognizeCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;
    private readonly string _knownEndpoint = "https://eastus.cognitiveservices.azure.com/";
    private readonly string _knownSubscription = "sub123";

    public SttRecognizeCommandTests()
    {
        _speechService = Substitute.For<ISpeechService>();
        _logger = Substitute.For<ILogger<SttRecognizeCommand>>();

        var collection = new ServiceCollection().AddSingleton(_speechService);

        _serviceProvider = collection.BuildServiceProvider();
        _command = new(_logger);
        _context = new(_serviceProvider);
        _commandDefinition = _command.GetCommand();
    }

    [Fact]
    public void Constructor_WithValidLogger_ShouldCreateInstance()
    {
        var command = new SttRecognizeCommand(_logger);
        Assert.NotNull(command);
        Assert.Equal("recognize", command.Name);
    }

    [Fact]
    public void Properties_ShouldHaveExpectedValues()
    {
        Assert.Equal("recognize", _command.Name);
        Assert.Equal("Recognize Speech from Audio File", _command.Title);
        Assert.NotEmpty(_command.Description);
        Assert.False(_command.Metadata.Destructive);
        Assert.True(_command.Metadata.Idempotent);
        Assert.False(_command.Metadata.OpenWorld);
        Assert.True(_command.Metadata.ReadOnly);
        Assert.True(_command.Metadata.LocalRequired);
        Assert.False(_command.Metadata.Secret);
    }

    [Theory]
    [InlineData("", false, "Missing Required options: --endpoint, --file")]
    [InlineData("--subscription sub123", false, "Missing Required options: --endpoint, --file")]
    [InlineData("--subscription sub123 --endpoint https://test.cognitiveservices.azure.com/", false, "Missing Required options: --file")]
    [InlineData("--subscription sub123 --endpoint https://test.cognitiveservices.azure.com/ --file nonexistent.wav", false, "Audio file not found")]
    [InlineData("--subscription sub123 --endpoint https://test.cognitiveservices.azure.com/ --file test.wav --format invalid", false, "Format must be 'simple' or 'detailed'")]
    [InlineData("--subscription sub123 --endpoint https://test.cognitiveservices.azure.com/ --file test.wav --profanity invalid", false, "Profanity filter must be 'masked', 'removed', or 'raw'")]
    public async Task ExecuteAsync_ValidatesInput(string args, bool shouldSucceed, string expectedError)
    {
        // Create a test file if needed
        if (args.Contains("test.wav"))
        {
            await File.WriteAllTextAsync("test.wav", "test content", TestContext.Current.CancellationToken);
        }

        try
        {
            var parseResult = _commandDefinition.Parse(args.Split(' ', StringSplitOptions.RemoveEmptyEntries));
            var response = await _command.ExecuteAsync(_context, parseResult);

            if (shouldSucceed)
            {
                Assert.Equal(200, response.Status);
            }
            else
            {
                Assert.NotEqual(200, response.Status);
                Assert.Contains(expectedError, response.Message, StringComparison.OrdinalIgnoreCase);
            }
        }
        finally
        {
            // Clean up test file
            if (File.Exists("test.wav"))
            {
                File.Delete("test.wav");
            }
        }
    }

    [Fact]
    public async Task ExecuteAsync_WithValidParameters_ShouldSucceed()
    {
        // Arrange
        var testFile = "test-audio.wav";
        await File.WriteAllTextAsync(testFile, "test audio content", TestContext.Current.CancellationToken);

        var expectedResult = new SpeechRecognitionResult
        {
            Text = "Hello world",
            Confidence = 0.95,
            Reason = "RecognizedSpeech"
        };

        _speechService.RecognizeSpeechFromFile(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string[]>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>())
            .Returns(expectedResult);

        try
        {
            // Act
            var args = $"--subscription {_knownSubscription} --endpoint {_knownEndpoint} --file {testFile}";
            var parseResult = _commandDefinition.Parse(args.Split(' ', StringSplitOptions.RemoveEmptyEntries));
            var response = await _command.ExecuteAsync(_context, parseResult);

            // Assert
            Assert.Equal(200, response.Status);
            Assert.NotNull(response.Results);

            var result = JsonSerializer.Deserialize<SttRecognizeCommand.SttRecognizeCommandResult>(
                JsonSerializer.Serialize(response.Results), SpeechJsonContext.Default.SttRecognizeCommandResult);
            Assert.NotNull(result);
            Assert.Equal("Hello world", result.Result.Text);
        }
        finally
        {
            // Clean up
            if (File.Exists(testFile))
            {
                File.Delete(testFile);
            }
        }
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceError()
    {
        // Arrange
        var testFile = "test-audio.wav";
        await File.WriteAllTextAsync(testFile, "test audio content", TestContext.Current.CancellationToken);

        _speechService.RecognizeSpeechFromFile(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string[]>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>())
            .ThrowsAsync(new UnauthorizedAccessException("Access denied"));

        try
        {
            // Act
            var args = $"--subscription {_knownSubscription} --endpoint {_knownEndpoint} --file {testFile}";
            var parseResult = _commandDefinition.Parse(args.Split(' ', StringSplitOptions.RemoveEmptyEntries));
            var response = await _command.ExecuteAsync(_context, parseResult);

            // Assert
            Assert.Equal(500, response.Status);
            Assert.Contains("Access denied", response.Message);
        }
        finally
        {
            // Clean up
            if (File.Exists(testFile))
            {
                File.Delete(testFile);
            }
        }
    }

    [Fact]
    public async Task ExecuteAsync_WithInvalidEndpoint_ShouldReturnBadRequest()
    {
        // Arrange
        var testFile = "test-audio.wav";
        await File.WriteAllTextAsync(testFile, "test audio content", TestContext.Current.CancellationToken);

        try
        {
            // Act - Use an invalid endpoint that's not Azure AI Services
            var args = $"--subscription {_knownSubscription} --endpoint https://example.com --file {testFile}";
            var parseResult = _commandDefinition.Parse(args.Split(' ', StringSplitOptions.RemoveEmptyEntries));
            var response = await _command.ExecuteAsync(_context, parseResult);

            // Assert
            Assert.Equal(400, response.Status);
            Assert.Contains("must be a valid Azure AI Services endpoint", response.Message);
        }
        finally
        {
            // Clean up
            if (File.Exists(testFile))
            {
                File.Delete(testFile);
            }
        }
    }

    [Fact]
    public async Task ExecuteAsync_WithValidAzureAiEndpoint_ShouldAcceptEndpoint()
    {
        // Arrange
        var testFile = "test-audio.wav";
        await File.WriteAllTextAsync(testFile, "test audio content", TestContext.Current.CancellationToken);

        var expectedResult = new SpeechRecognitionResult
        {
            Text = "Test result",
            Confidence = 0.9,
            Reason = "RecognizedSpeech"
        };

        _speechService.RecognizeSpeechFromFile(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string[]>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>())
            .Returns(expectedResult);

        try
        {
            // Act - Use a valid Azure AI endpoint
            var args = $"--subscription {_knownSubscription} --endpoint https://myservice.cognitiveservices.azure.com --file {testFile}";
            var parseResult = _commandDefinition.Parse(args.Split(' ', StringSplitOptions.RemoveEmptyEntries));
            var response = await _command.ExecuteAsync(_context, parseResult);

            // Assert
            Assert.Equal(200, response.Status);
        }
        finally
        {
            // Clean up
            if (File.Exists(testFile))
            {
                File.Delete(testFile);
            }
        }
    }
}

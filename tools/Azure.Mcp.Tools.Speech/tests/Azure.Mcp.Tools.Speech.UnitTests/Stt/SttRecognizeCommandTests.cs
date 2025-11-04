// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Net;
using System.Text.Json;
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
                Assert.Equal(HttpStatusCode.OK, response.Status);
            }
            else
            {
                Assert.NotEqual(HttpStatusCode.OK, response.Status);
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

        var expectedResult = CreateContinuousRecognitionResult("Hello world", "RecognizedSpeech");

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
            Assert.Equal(HttpStatusCode.OK, response.Status);
            Assert.NotNull(response.Results);

            var result = JsonSerializer.Deserialize(
                JsonSerializer.Serialize(response.Results), SpeechJsonContext.Default.SttRecognizeCommandResult);
            Assert.NotNull(result);
            Assert.Equal("Hello world", result.Result.FullText);
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
            Assert.Equal(HttpStatusCode.Unauthorized, response.Status);
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
            Assert.Equal(HttpStatusCode.BadRequest, response.Status);
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

        var expectedResult = CreateContinuousRecognitionResult("Test result", "RecognizedSpeech");

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
            Assert.Equal(HttpStatusCode.OK, response.Status);
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
    public async Task ExecuteAsync_WithDetailedFormat_ShouldReturnDetailedResult()
    {
        // Arrange
        var testFile = "test-audio-detailed.wav";
        await File.WriteAllTextAsync(testFile, "test audio content", TestContext.Current.CancellationToken);

        var expectedResult = CreateContinuousRecognitionResult("Hello world", "RecognizedSpeech", true);

        _speechService.RecognizeSpeechFromFile(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string[]>(),
            "detailed",
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>())
            .Returns(expectedResult);

        try
        {
            // Act
            var args = $"--subscription {_knownSubscription} --endpoint {_knownEndpoint} --file {testFile} --format detailed";
            var parseResult = _commandDefinition.Parse(args.Split(' ', StringSplitOptions.RemoveEmptyEntries));
            var response = await _command.ExecuteAsync(_context, parseResult);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.Status);
            Assert.NotNull(response.Results);

            var result = JsonSerializer.Deserialize(
                JsonSerializer.Serialize(response.Results), SpeechJsonContext.Default.SttRecognizeCommandResult);
            Assert.NotNull(result);
            Assert.Equal("Hello world", result.Result.FullText);

            // Verify it's a detailed result
            Assert.Single(result.Result.Segments);
            Assert.IsType<DetailedSpeechRecognitionResult>(result.Result.Segments[0]);
            var detailedResult = (DetailedSpeechRecognitionResult)result.Result.Segments[0];
            Assert.NotNull(detailedResult.NBest);
            Assert.Single(detailedResult.NBest);
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

    [Theory]
    [InlineData("masked")]
    [InlineData("removed")]
    [InlineData("raw")]
    public async Task ExecuteAsync_WithDifferentProfanityOptions_ShouldPassToService(string profanityOption)
    {
        // Arrange
        var testFile = "test-audio-profanity.wav";
        await File.WriteAllTextAsync(testFile, "test audio content", TestContext.Current.CancellationToken);

        var expectedResult = CreateContinuousRecognitionResult("Filtered text", "RecognizedSpeech");

        _speechService.RecognizeSpeechFromFile(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string[]>(),
            Arg.Any<string>(),
            profanityOption,
            Arg.Any<RetryPolicyOptions>())
            .Returns(expectedResult);

        try
        {
            // Act
            var args = $"--subscription {_knownSubscription} --endpoint {_knownEndpoint} --file {testFile} --profanity {profanityOption}";
            var parseResult = _commandDefinition.Parse(args.Split(' ', StringSplitOptions.RemoveEmptyEntries));
            var response = await _command.ExecuteAsync(_context, parseResult);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.Status);

            // Verify the service was called with correct profanity option
            await _speechService.Received(1).RecognizeSpeechFromFile(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string[]>(),
                Arg.Any<string>(),
                profanityOption,
                Arg.Any<RetryPolicyOptions>());
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
    public async Task ExecuteAsync_WithPhraseHints_ShouldPassToService()
    {
        // Arrange
        var testFile = "test-audio-phrases.wav";
        await File.WriteAllTextAsync(testFile, "test audio content", TestContext.Current.CancellationToken);

        var expectedResult = CreateContinuousRecognitionResult("Azure cognitive services", "RecognizedSpeech");

        // Capture what phrases are actually passed for verification
        string[]? capturedPhrases = null;
        _speechService.RecognizeSpeechFromFile(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Do<string[]>(phrases => capturedPhrases = phrases),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>())
            .Returns(expectedResult);

        try
        {
            // Act - Use a different approach to handle quoted arguments
            var args = new string[]
            {
                "--subscription", _knownSubscription,
                "--endpoint", _knownEndpoint,
                "--file", testFile,
                "--phrases", "Azure",
                "--phrases", "cognitive services"
            };
            var parseResult = _commandDefinition.Parse(args);
            var response = await _command.ExecuteAsync(_context, parseResult);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.Status);

            // Verify that phrases were captured and contain expected values
            Assert.NotNull(capturedPhrases);
            Assert.Equal(2, capturedPhrases.Length);
            Assert.Contains("Azure", capturedPhrases);
            Assert.Contains("cognitive services", capturedPhrases);

            // Verify the service was called with the expected phrases
            await _speechService.Received(1).RecognizeSpeechFromFile(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Is<string[]>(phrases =>
                    phrases != null &&
                    phrases.Length == 2 &&
                    phrases.Contains("Azure") &&
                    phrases.Contains("cognitive services")),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<RetryPolicyOptions>());
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

    [Theory]
    [InlineData("en-US")]
    [InlineData("es-ES")]
    [InlineData("fr-FR")]
    [InlineData("de-DE")]
    public async Task ExecuteAsync_WithDifferentLanguages_ShouldPassToService(string language)
    {
        // Arrange
        var testFile = "test-audio-language.wav";
        await File.WriteAllTextAsync(testFile, "test audio content", TestContext.Current.CancellationToken);

        var expectedResult = CreateContinuousRecognitionResult("Recognized text", "RecognizedSpeech");

        _speechService.RecognizeSpeechFromFile(
            Arg.Any<string>(),
            Arg.Any<string>(),
            language,
            Arg.Any<string[]>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>())
            .Returns(expectedResult);

        try
        {
            // Act
            var args = $"--subscription {_knownSubscription} --endpoint {_knownEndpoint} --file {testFile} --language {language}";
            var parseResult = _commandDefinition.Parse(args.Split(' ', StringSplitOptions.RemoveEmptyEntries));
            var response = await _command.ExecuteAsync(_context, parseResult);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.Status);

            // Verify the service was called with correct language
            await _speechService.Received(1).RecognizeSpeechFromFile(
                Arg.Any<string>(),
                Arg.Any<string>(),
                language,
                Arg.Any<string[]>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<RetryPolicyOptions>());
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
    public async Task ExecuteAsync_WithRetryPolicy_ShouldPassToService()
    {
        // Arrange
        var testFile = "test-audio-retry.wav";
        await File.WriteAllTextAsync(testFile, "test audio content", TestContext.Current.CancellationToken);

        var expectedResult = CreateContinuousRecognitionResult("Retry succeeded", "RecognizedSpeech");

        _speechService.RecognizeSpeechFromFile(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string[]>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Is<RetryPolicyOptions>(policy => policy.MaxRetries == 5))
            .Returns(expectedResult);

        try
        {
            // Act
            var args = $"--subscription {_knownSubscription} --endpoint {_knownEndpoint} --file {testFile} --retry-max-retries 5";
            var parseResult = _commandDefinition.Parse(args.Split(' ', StringSplitOptions.RemoveEmptyEntries));
            var response = await _command.ExecuteAsync(_context, parseResult);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.Status);

            // Verify the service was called with retry policy
            await _speechService.Received(1).RecognizeSpeechFromFile(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string[]>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Is<RetryPolicyOptions>(policy => policy.MaxRetries == 5));
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

    [Theory]
    [InlineData("HttpRequestException", HttpStatusCode.ServiceUnavailable, "Http request failed")]
    [InlineData("TimeoutException", HttpStatusCode.GatewayTimeout, "Speech recognition timed out")]
    [InlineData("InvalidOperationException", HttpStatusCode.InternalServerError, "Invalid operation")]
    [InlineData("ArgumentException", HttpStatusCode.BadRequest, "Invalid argument")]
    public async Task ExecuteAsync_HandlesSpecificExceptions(string exceptionType, HttpStatusCode expectedStatus, string expectedMessage)
    {
        // Arrange
        var testFile = "test-audio-exception.wav";
        await File.WriteAllTextAsync(testFile, "test audio content", TestContext.Current.CancellationToken);

        Exception exceptionToThrow = exceptionType switch
        {
            "HttpRequestException" => new HttpRequestException("Http request failed"),
            "TimeoutException" => new TimeoutException("Speech recognition timed out"),
            "InvalidOperationException" => new InvalidOperationException("Invalid operation"),
            "ArgumentException" => new ArgumentException("Invalid argument"),
            _ => new Exception("Unknown error")
        };

        _speechService.RecognizeSpeechFromFile(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string[]>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>())
            .ThrowsAsync(exceptionToThrow);

        try
        {
            // Act
            var args = $"--subscription {_knownSubscription} --endpoint {_knownEndpoint} --file {testFile}";
            var parseResult = _commandDefinition.Parse(args.Split(' ', StringSplitOptions.RemoveEmptyEntries));
            var response = await _command.ExecuteAsync(_context, parseResult);

            // Assert
            Assert.Equal(expectedStatus, response.Status);
            Assert.Contains(expectedMessage, response.Message);
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
    public async Task ExecuteAsync_WithEmptyAudioFile_ShouldHandleGracefully()
    {
        // Arrange
        var testFile = "empty-audio.wav";
        await File.WriteAllTextAsync(testFile, "", TestContext.Current.CancellationToken); // Empty file

        var expectedResult = CreateContinuousRecognitionResult("", "NoMatch");

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
            Assert.Equal(HttpStatusCode.OK, response.Status);
            Assert.NotNull(response.Results);

            var result = JsonSerializer.Deserialize<SttRecognizeCommand.SttRecognizeCommandResult>(
                JsonSerializer.Serialize(response.Results), SpeechJsonContext.Default.SttRecognizeCommandResult);
            Assert.NotNull(result);
            Assert.Equal("", result.Result.FullText);
            Assert.Equal("NoMatch", result.Result.Segments[0].Reason);
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
    public async Task ExecuteAsync_WithLargeAudioFile_ShouldHandleCorrectly()
    {
        // Arrange
        var testFile = "large-audio.wav";
        var largeContent = new string('A', 10000); // Create a large file
        await File.WriteAllTextAsync(testFile, largeContent, TestContext.Current.CancellationToken);

        var expectedResult = CreateContinuousRecognitionResult("Long audio content recognition result", "RecognizedSpeech");

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
            Assert.Equal(HttpStatusCode.OK, response.Status);
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
    public async Task ExecuteAsync_WithSemicolonSeparatedPhrases_ShouldTreatAsOnePhrase()
    {
        // Arrange
        var testFile = "test-audio-semicolon-phrases.wav";
        await File.WriteAllTextAsync(testFile, "test audio content", TestContext.Current.CancellationToken);

        var expectedResult = CreateContinuousRecognitionResult("Azure cognitive services", "RecognizedSpeech");

        // Capture what phrases are actually passed for verification
        string[]? capturedPhrases = null;
        _speechService.RecognizeSpeechFromFile(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Do<string[]>(phrases => capturedPhrases = phrases),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>())
            .Returns(expectedResult);

        try
        {
            // Act - Test semicolon-separated phrases in a single argument
            var args = new string[]
            {
                "--subscription", _knownSubscription,
                "--endpoint", _knownEndpoint,
                "--file", testFile,
                "--phrases", "Azure; cognitive services"
            };
            var parseResult = _commandDefinition.Parse(args);
            var response = await _command.ExecuteAsync(_context, parseResult);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.Status);

            // Check what phrases were actually captured
            Assert.NotNull(capturedPhrases);

            // The current implementation likely treats this as a single phrase
            // Let's see what we actually get
            var phrasesDebug = string.Join(", ", capturedPhrases.Select(p => $"\"{p}\""));

            // Based on System.CommandLine behavior, this should be treated as one phrase
            Assert.Single(capturedPhrases);
            Assert.Equal("Azure; cognitive services", capturedPhrases[0]);
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
    public async Task ExecuteAsync_WithCommaSeparatedPhrases_ShouldSplitIntoSeparatePhrases()
    {
        // Arrange
        var testFile = "test-audio-comma-phrases.wav";
        await File.WriteAllTextAsync(testFile, "test audio content", TestContext.Current.CancellationToken);

        var expectedResult = CreateContinuousRecognitionResult("Azure cognitive services", "RecognizedSpeech");

        // Capture what phrases are actually passed for verification
        string[]? capturedPhrases = null;
        _speechService.RecognizeSpeechFromFile(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Do<string[]>(phrases => capturedPhrases = phrases),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>())
            .Returns(expectedResult);

        try
        {
            // Act - Test comma-separated phrases in a single argument
            var args = new string[]
            {
                "--subscription", _knownSubscription,
                "--endpoint", _knownEndpoint,
                "--file", testFile,
                "--phrases", "Azure, cognitive services"
            };
            var parseResult = _commandDefinition.Parse(args);
            var response = await _command.ExecuteAsync(_context, parseResult);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.Status);

            // Check what phrases were actually captured
            Assert.NotNull(capturedPhrases);

            // The new implementation should split comma-separated phrases
            Assert.Equal(2, capturedPhrases.Length);
            Assert.Contains("Azure", capturedPhrases);
            Assert.Contains("cognitive services", capturedPhrases);
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
    public async Task ExecuteAsync_WithMixedPhrasesSyntax_ShouldCombineCorrectly()
    {
        // Arrange
        var testFile = "test-audio-mixed-phrases.wav";
        await File.WriteAllTextAsync(testFile, "test audio content", TestContext.Current.CancellationToken);

        var expectedResult = CreateContinuousRecognitionResult("Azure cognitive services machine learning", "RecognizedSpeech");

        // Capture what phrases are actually passed for verification
        string[]? capturedPhrases = null;
        _speechService.RecognizeSpeechFromFile(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Do<string[]>(phrases => capturedPhrases = phrases),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>())
            .Returns(expectedResult);

        try
        {
            // Act - Test combination of multiple arguments and comma-separated phrases
            var args = new string[]
            {
                "--subscription", _knownSubscription,
                "--endpoint", _knownEndpoint,
                "--file", testFile,
                "--phrases", "Azure, cognitive services",  // Comma-separated in first argument
                "--phrases", "machine learning"            // Single phrase in second argument
            };
            var parseResult = _commandDefinition.Parse(args);
            var response = await _command.ExecuteAsync(_context, parseResult);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.Status);

            // Check what phrases were actually captured
            Assert.NotNull(capturedPhrases);

            // Should have 3 phrases total: "Azure", "cognitive services", "machine learning"
            Assert.Equal(3, capturedPhrases.Length);
            Assert.Contains("Azure", capturedPhrases);
            Assert.Contains("cognitive services", capturedPhrases);
            Assert.Contains("machine learning", capturedPhrases);

            // Verify the service was called with all expected phrases
            await _speechService.Received(1).RecognizeSpeechFromFile(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Is<string[]>(phrases =>
                    phrases != null &&
                    phrases.Length == 3 &&
                    phrases.Contains("Azure") &&
                    phrases.Contains("cognitive services") &&
                    phrases.Contains("machine learning")),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<RetryPolicyOptions>());
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

    [Theory]
    [InlineData("test-audio.wav")]
    [InlineData("test-audio.mp3")]
    [InlineData("test-audio.m4a")]
    [InlineData("test-audio.flac")]
    [InlineData("test-audio.ogg")]
    public async Task ExecuteAsync_WithDifferentAudioFormats_ShouldSucceed(string fileName)
    {
        // Arrange
        await File.WriteAllTextAsync(fileName, "test audio content", TestContext.Current.CancellationToken);

        var expectedResult = CreateContinuousRecognitionResult("Hello world", "RecognizedSpeech");

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
            var args = $"--subscription {_knownSubscription} --endpoint {_knownEndpoint} --file {fileName}";
            var parseResult = _commandDefinition.Parse(args.Split(' ', StringSplitOptions.RemoveEmptyEntries));
            var response = await _command.ExecuteAsync(_context, parseResult);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.Status);
            Assert.NotNull(response.Results);

            var result = JsonSerializer.Deserialize<SttRecognizeCommand.SttRecognizeCommandResult>(
                JsonSerializer.Serialize(response.Results), SpeechJsonContext.Default.SttRecognizeCommandResult);
            Assert.NotNull(result);
            Assert.Equal("Hello world", result.Result.FullText);

            // Verify the service was called with the correct file path
            await _speechService.Received(1).RecognizeSpeechFromFile(
                Arg.Any<string>(),
                fileName,
                Arg.Any<string>(),
                Arg.Any<string[]>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<RetryPolicyOptions>());
        }
        finally
        {
            // Clean up
            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }
        }
    }

    [Theory]
    [InlineData("invalid-extension.txt")]
    [InlineData("audio-file-without-extension")]
    [InlineData("audio.unknown")]
    public async Task ExecuteAsync_WithInvalidFileExtensions_ShouldReturnValidationError(string fileName)
    {
        // Arrange - Create file with test content
        await File.WriteAllTextAsync(fileName, "test content", TestContext.Current.CancellationToken);

        try
        {
            // Act
            var args = $"--subscription {_knownSubscription} --endpoint {_knownEndpoint} --file {fileName}";
            var parseResult = _commandDefinition.Parse(args.Split(' ', StringSplitOptions.RemoveEmptyEntries));
            var response = await _command.ExecuteAsync(_context, parseResult);

            // Assert - The command should return validation error for invalid file extensions
            Assert.Equal(HttpStatusCode.BadRequest, response.Status);
            Assert.Contains("Unsupported audio file format", response.Message);

            // Verify the service was NOT called with invalid file extensions
            await _speechService.DidNotReceive().RecognizeSpeechFromFile(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string[]>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<RetryPolicyOptions>());
        }
        finally
        {
            // Clean up
            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }
        }
    }

    [Fact]
    public async Task ExecuteAsync_WithLargeAudioFile_ShouldHandleGracefully()
    {
        // Arrange
        var largeFileName = "large-audio-file.wav";
        var largeContent = new string('A', 10_000_000); // 10MB of content
        await File.WriteAllTextAsync(largeFileName, largeContent, TestContext.Current.CancellationToken);

        var expectedResult = CreateContinuousRecognitionResult("Large file processed", "RecognizedSpeech");

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
            var args = $"--subscription {_knownSubscription} --endpoint {_knownEndpoint} --file {largeFileName}";
            var parseResult = _commandDefinition.Parse(args.Split(' ', StringSplitOptions.RemoveEmptyEntries));
            var response = await _command.ExecuteAsync(_context, parseResult);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.Status);
            Assert.NotNull(response.Results);

            var result = JsonSerializer.Deserialize<SttRecognizeCommand.SttRecognizeCommandResult>(
                JsonSerializer.Serialize(response.Results), SpeechJsonContext.Default.SttRecognizeCommandResult);
            Assert.NotNull(result);
            Assert.Equal("Large file processed", result.Result.FullText);
        }
        finally
        {
            // Clean up
            if (File.Exists(largeFileName))
            {
                File.Delete(largeFileName);
            }
        }
    }

    private static ContinuousRecognitionResult CreateContinuousRecognitionResult(string text, string reason, bool isDetailed = false)
    {
        var segment = isDetailed
            ? new DetailedSpeechRecognitionResult
            {
                Text = text,
                Reason = reason,
                NBest = new List<NBestResult>
                {
                    new NBestResult { Display = text, Confidence = 0.95 }
                }
            }
            : new SpeechRecognitionResult
            {
                Text = text,
                Reason = reason
            };

        return new ContinuousRecognitionResult
        {
            FullText = text,
            Segments = new List<SpeechRecognitionResult> { segment }
        };
    }
}


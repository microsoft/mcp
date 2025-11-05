// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Net;
using System.Text.Json;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Tools.Speech.Commands.Stt;
using Azure.Mcp.Tools.Speech.Models;
using Azure.Mcp.Tools.Speech.Models.FastTranscription;
using Azure.Mcp.Tools.Speech.Models.Realtime;
using Azure.Mcp.Tools.Speech.Services;
using Azure.Mcp.Tools.Speech.Services.Recognizers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Speech.UnitTests.Stt;

public class SttRecognizeCommandTests : IDisposable
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ISpeechService _speechService;
    private readonly IFastTranscriptionRecognizer _fastTranscriptionRecognizer;
    private readonly IRealtimeTranscriptionRecognizer _realtimeTranscriptionRecognizer;
    private readonly ITenantService _tenantService;
    private readonly ILogger<SttRecognizeCommand> _logger;
    private readonly ILogger<SpeechService> _speechServiceLogger;
    private readonly SttRecognizeCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;
    private readonly string _knownEndpoint = "https://eastus.cognitiveservices.azure.com/";
    private readonly string _knownSubscription = "sub123";
    private readonly List<string> _testFilesToCleanup = new();

    public SttRecognizeCommandTests()
    {
        // Mock the recognizers and their dependencies
        _fastTranscriptionRecognizer = Substitute.For<IFastTranscriptionRecognizer>();
        _realtimeTranscriptionRecognizer = Substitute.For<IRealtimeTranscriptionRecognizer>();
        _tenantService = Substitute.For<ITenantService>();
        _logger = Substitute.For<ILogger<SttRecognizeCommand>>();
        _speechServiceLogger = Substitute.For<ILogger<SpeechService>>();

        // Create real SpeechService with mocked dependencies
        _speechService = new SpeechService(_tenantService, _speechServiceLogger, _fastTranscriptionRecognizer, _realtimeTranscriptionRecognizer);

        var collection = new ServiceCollection().AddSingleton(_speechService);

        _serviceProvider = collection.BuildServiceProvider();
        _command = new(_logger);
        _context = new(_serviceProvider);
        _commandDefinition = _command.GetCommand();
    }

    public void Dispose()
    {
        foreach (var file in _testFilesToCleanup.Where(File.Exists))
        {
            try
            { File.Delete(file); }
            catch { /* Ignore cleanup errors */ }
        }
        _testFilesToCleanup.Clear();
    }

    private async Task<string> CreateTestFileAsync(string fileName, string content = "test audio content")
    {
        await File.WriteAllTextAsync(fileName, content, TestContext.Current.CancellationToken);
        _testFilesToCleanup.Add(fileName);
        return fileName;
    }

    private void SetupFastTranscriptionMock(string expectedText = "Hello world")
    {
        var fastResult = new FastTranscriptionResult
        {
            CombinedPhrases = new List<FastTranscriptionCombinedPhrase>
            {
                new FastTranscriptionCombinedPhrase { Text = expectedText }
            },
            DurationMilliseconds = 5000
        };

        _fastTranscriptionRecognizer.RecognizeAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string[]>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>())
            .Returns(fastResult);
    }

    private void SetupRealtimeTranscriptionMock(string expectedText = "Hello world", string reason = "RecognizedSpeech")
    {
        var realtimeResult = new RealtimeRecognitionContinuousResult
        {
            FullText = expectedText,
            Segments = new List<RealtimeRecognitionResult>
            {
                new RealtimeRecognitionResult { Text = expectedText, Reason = reason }
            }
        };

        _realtimeTranscriptionRecognizer.RecognizeAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string[]>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>())
            .Returns(realtimeResult);
    }

    private void SetupRealtimeTranscriptionDetailedMock(string expectedText = "Hello world")
    {
        var realtimeResult = new RealtimeRecognitionContinuousResult
        {
            FullText = expectedText,
            Segments = new List<RealtimeRecognitionResult>
            {
                new RealtimeRecognitionDetailedResult
                {
                    Text = expectedText,
                    Reason = "RecognizedSpeech",
                    NBest = new List<RealtimeRecognitionNBestResult>
                    {
                        new RealtimeRecognitionNBestResult { Display = expectedText, Confidence = 0.95 }
                    }
                }
            }
        };

        _realtimeTranscriptionRecognizer.RecognizeAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string[]>(),
            "detailed",
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>())
            .Returns(realtimeResult);
    }

    private async Task<CommandResponse> ExecuteCommandAsync(string args)
    {
        var parseResult = _commandDefinition.Parse(args.Split(' ', StringSplitOptions.RemoveEmptyEntries));
        return await _command.ExecuteAsync(_context, parseResult);
    }

    private async Task<CommandResponse> ExecuteCommandAsync(string[] args)
    {
        var parseResult = _commandDefinition.Parse(args);
        return await _command.ExecuteAsync(_context, parseResult);
    }

    private static SttRecognizeCommand.SttRecognizeCommandResult DeserializeResult(CommandResponse response)
    {
        return JsonSerializer.Deserialize(
            JsonSerializer.Serialize(response.Results),
            SpeechJsonContext.Default.SttRecognizeCommandResult)!;
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
            await CreateTestFileAsync("test.wav");
        }

        var response = await ExecuteCommandAsync(args);

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

    [Theory]
    [InlineData("--subscription sub123 --endpoint https://test.cognitiveservices.azure.com/ --file test.wav", RecognizerType.Fast)]
    [InlineData("--subscription sub123 --endpoint https://test.cognitiveservices.azure.com/ --file test.wav --format detailed", RecognizerType.Fast)]
    [InlineData("--subscription sub123 --endpoint https://test.cognitiveservices.azure.com/ --file test.wav --language en-US --format detailed", RecognizerType.Realtime)]
    [InlineData("--subscription sub123 --endpoint https://test.cognitiveservices.azure.com/ --file test.wav --language fr-FR", RecognizerType.Fast)]
    [InlineData("--subscription sub123 --endpoint https://test.cognitiveservices.azure.com/ --file test.wav --language es-ES", RecognizerType.Fast)]
    [InlineData("--subscription sub123 --endpoint https://test.cognitiveservices.azure.com/ --file test.wav --language af-ZA", RecognizerType.Realtime)]
    [InlineData("--subscription sub123 --endpoint https://test.cognitiveservices.azure.com/ --file test.wav --language am-ET", RecognizerType.Realtime)]
    [InlineData("--subscription sub123 --endpoint https://test.cognitiveservices.azure.com/ --file test.wav --language as-IN", RecognizerType.Realtime)]
    public async Task ExecuteAsync_DifferentInput_ShouldUseDifferentRecognizer(string args, RecognizerType expectedRecognizer)
    {
        // Arrange
        await CreateTestFileAsync("test.wav");
        var expectedText = "Hello world";

        // Set up both mocks to always return results
        SetupFastTranscriptionMock(expectedText);
        SetupRealtimeTranscriptionMock(expectedText);

        // Act
        var response = await ExecuteCommandAsync(args);
        var result = DeserializeResult(response);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
        Assert.NotNull(result);

        Assert.Equal(expectedText, result.Result.Text);
        Assert.Equal(expectedRecognizer, result.Result.RecognizerType);

        // Verify the correct recognizer was called
        if (expectedRecognizer == RecognizerType.Fast)
        {
            await _fastTranscriptionRecognizer.Received(1).RecognizeAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string[]>(),
                Arg.Any<string>(),
                Arg.Any<RetryPolicyOptions>());

            await _realtimeTranscriptionRecognizer.DidNotReceive().RecognizeAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string[]>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<RetryPolicyOptions>());

            Assert.NotNull(result.Result.FastTranscriptionResult);
            Assert.Null(result.Result.RealtimeContinuousResult);
        }
        else
        {
            await _realtimeTranscriptionRecognizer.Received(1).RecognizeAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string[]>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<RetryPolicyOptions>());

            await _fastTranscriptionRecognizer.DidNotReceive().RecognizeAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string[]>(),
                Arg.Any<string>(),
                Arg.Any<RetryPolicyOptions>());

            Assert.NotNull(result.Result.RealtimeContinuousResult);
            Assert.Null(result.Result.FastTranscriptionResult);
        }
    }


    [Fact]
    public async Task ExecuteAsync_WithValidParameters_ShouldSucceed()
    {
        // Arrange
        var testFile = await CreateTestFileAsync("test-audio.wav");
        var expectedText = "Hello world";
        SetupRealtimeTranscriptionMock(expectedText);

        // Act
        var args = $"--subscription {_knownSubscription} --endpoint {_knownEndpoint} --file {testFile} --language en-US --format simple";
        var response = await ExecuteCommandAsync(args);
        var result = DeserializeResult(response);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
        Assert.NotNull(result);
        Assert.NotNull(result.Result.RealtimeContinuousResult);
        Assert.Equal(expectedText, result.Result.RealtimeContinuousResult.FullText);
        Assert.Equal(expectedText, result.Result.Text);
        Assert.Equal(RecognizerType.Realtime, result.Result.RecognizerType);
    }

    [Fact]
    public async Task ExecuteAsync_FastRecognizerFails_ShouldFallbackToRealtimeRecognizer()
    {
        // Arrange
        var testFile = await CreateTestFileAsync("test-audio.wav");

        // Mock fast transcription to throw error (since no language is specified, it should use fast transcription)
        _fastTranscriptionRecognizer.RecognizeAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string[]>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>())
            .ThrowsAsync(new UnauthorizedAccessException("Access denied"));

        // Mock realtime transcription to return valid result. When fast fails, it should fallback to realtime.
        SetupRealtimeTranscriptionMock("Hello world");

        // Act
        var args = $"--subscription {_knownSubscription} --endpoint {_knownEndpoint} --file {testFile}";
        var response = await ExecuteCommandAsync(args);

        // Assert
        // 1. Fast recognizer was called and failed because of UnauthorizedAccessException
        await _fastTranscriptionRecognizer
        .Received(1)
        .RecognizeAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string[]>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>());

        // 2. Realtime recognizer was called as a fallback
        await _realtimeTranscriptionRecognizer
       .Received(1)
       .RecognizeAsync(
           Arg.Any<string>(),
           Arg.Any<string>(),
           Arg.Any<string>(),
           Arg.Any<string[]>(),
           Arg.Any<string>(),
           Arg.Any<string>(),
           Arg.Any<RetryPolicyOptions>());

        Assert.Equal(HttpStatusCode.OK, response.Status);
    }


    [Fact]
    public async Task ExecuteAsync_HandleServiceErrorWhenRealtimeTranscription()
    {
        // Arrange
        var testFile = await CreateTestFileAsync("test-audio.wav");

        // Mock realtime transcription to return invalid result.
        _realtimeTranscriptionRecognizer.RecognizeAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string[]>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>())
            .ThrowsAsync(new UnauthorizedAccessException("Access denied"));

        // Act
        var args = $"--subscription {_knownSubscription} --endpoint {_knownEndpoint} --file {testFile} --language en-US --format simple";
        var response = await ExecuteCommandAsync(args);

        // Assert
        // 1. Fast recognizer was not called
        await _fastTranscriptionRecognizer
        .Received(0)
        .RecognizeAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string[]>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>());

        // 2. Realtime recognizer was called and failed
        await _realtimeTranscriptionRecognizer
       .Received(1)
       .RecognizeAsync(
           Arg.Any<string>(),
           Arg.Any<string>(),
           Arg.Any<string>(),
           Arg.Any<string[]>(),
           Arg.Any<string>(),
           Arg.Any<string>(),
           Arg.Any<RetryPolicyOptions>());

        Assert.Equal(HttpStatusCode.Unauthorized, response.Status);
        Assert.Contains("Access denied", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_WithInvalidEndpoint_ShouldReturnBadRequest()
    {
        // Arrange
        var testFile = await CreateTestFileAsync("test-audio.wav");

        // Act - Use an invalid endpoint that's not Azure AI Services
        var args = $"--subscription {_knownSubscription} --endpoint https://example.com --file {testFile}";
        var response = await ExecuteCommandAsync(args);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("must be a valid Azure AI Services endpoint", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidAzureAiEndpoint_ShouldAcceptEndpoint()
    {
        // Arrange
        var testFile = await CreateTestFileAsync("test-audio.wav");
        SetupRealtimeTranscriptionMock("Test result");

        // Act - Use a valid Azure AI endpoint
        var args = $"--subscription {_knownSubscription} --endpoint https://myservice.cognitiveservices.azure.com --file {testFile} --language en-US --format simple";
        var response = await ExecuteCommandAsync(args);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_WithDetailedFormat_ShouldReturnDetailedResult()
    {
        // Arrange
        var testFile = await CreateTestFileAsync("test-audio-detailed.wav");
        SetupRealtimeTranscriptionDetailedMock("Hello world");

        // Act
        var args = $"--subscription {_knownSubscription} --endpoint {_knownEndpoint} --file {testFile} --language en-US --format detailed";
        var response = await ExecuteCommandAsync(args);
        var result = DeserializeResult(response);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
        Assert.NotNull(result);
        Assert.NotNull(result.Result.RealtimeContinuousResult);
        Assert.Equal("Hello world", result.Result.RealtimeContinuousResult.FullText);

        // Verify it's a detailed result
        Assert.Single(result.Result.RealtimeContinuousResult.Segments);
        Assert.IsType<RealtimeRecognitionDetailedResult>(result.Result.RealtimeContinuousResult.Segments[0]);
        var detailedResult = (RealtimeRecognitionDetailedResult)result.Result.RealtimeContinuousResult.Segments[0];
        Assert.NotNull(detailedResult.NBest);
        Assert.Single(detailedResult.NBest);
    }

    [Theory]
    [InlineData("masked")]
    [InlineData("removed")]
    [InlineData("raw")]
    public async Task ExecuteAsync_WithDifferentProfanityOptions_ShouldPassToService(string profanityOption)
    {
        // Arrange
        var testFile = await CreateTestFileAsync("test-audio-profanity.wav");
        SetupFastTranscriptionMock("Hello world");

        // Act
        var args = $"--subscription {_knownSubscription} --endpoint {_knownEndpoint} --file {testFile} --profanity {profanityOption}";
        var response = await ExecuteCommandAsync(args);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);

        // Verify the service was called with correct profanity option
        await _fastTranscriptionRecognizer.Received(1).RecognizeAsync(
        Arg.Any<string>(),
        Arg.Any<string>(),
        Arg.Any<string>(),
        Arg.Any<string[]>(),
        profanityOption,
        Arg.Any<RetryPolicyOptions>());
    }

    [Fact]
    public async Task ExecuteAsync_WithPhraseHints_ShouldPassToService()
    {
        // Arrange
        var testFile = await CreateTestFileAsync("test-audio-phrases.wav");

        string[]? capturedPhrases = null;
        var fastResult = new FastTranscriptionResult
        {
            CombinedPhrases = new List<FastTranscriptionCombinedPhrase>
            {
                new FastTranscriptionCombinedPhrase { Text = "Azure cognitive services" }
            },
            DurationMilliseconds = 5000
        };

        _fastTranscriptionRecognizer.RecognizeAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Do<string[]>(phrases => capturedPhrases = phrases),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>())
            .Returns(fastResult);

        // Act - Use a different approach to handle quoted arguments
        var args = new string[]
        {
            "--subscription", _knownSubscription,
            "--endpoint", _knownEndpoint,
            "--file", testFile,
            "--phrases", "Azure",
            "--phrases", "cognitive services"
        };
        var response = await ExecuteCommandAsync(args);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);

        // Verify that phrases were captured and contain expected values
        Assert.NotNull(capturedPhrases);
        Assert.Equal(2, capturedPhrases.Length);
        Assert.Contains("Azure", capturedPhrases);
        Assert.Contains("cognitive services", capturedPhrases);

        // Verify the service was called with the expected phrases
        await _fastTranscriptionRecognizer.Received(1).RecognizeAsync(
        Arg.Any<string>(),
        Arg.Any<string>(),
        Arg.Any<string>(),
        Arg.Is<string[]>(phrases =>
            phrases != null &&
            phrases.Length == 2 &&
            phrases.Contains("Azure") &&
            phrases.Contains("cognitive services")),
        Arg.Any<string>(),
        Arg.Any<RetryPolicyOptions>());
    }

    [Theory]
    [InlineData("en-US")]
    [InlineData("es-ES")]
    [InlineData("fr-FR")]
    [InlineData("de-DE")]
    public async Task ExecuteAsync_WithDifferentLanguages_ShouldPassToService(string language)
    {
        // Arrange
        var testFile = await CreateTestFileAsync("test-audio-language.wav");
        SetupFastTranscriptionMock("Azure cognitive services");

        // Act
        var args = $"--subscription {_knownSubscription} --endpoint {_knownEndpoint} --file {testFile} --language {language}";
        var response = await ExecuteCommandAsync(args);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);

        // Verify the service was called with correct language
        await _fastTranscriptionRecognizer.Received(1).RecognizeAsync(
        Arg.Any<string>(),
        Arg.Any<string>(),
        language,
        Arg.Any<string[]>(),
        Arg.Any<string>(),
        Arg.Any<RetryPolicyOptions>());
    }

    [Fact]
    public async Task ExecuteAsync_WithRetryPolicy_ShouldPassToService()
    {
        // Arrange
        var testFile = await CreateTestFileAsync("test-audio-retry.wav");
        SetupFastTranscriptionMock("Hello with retry");

        // Act
        var args = $"--subscription {_knownSubscription} --endpoint {_knownEndpoint} --file {testFile} --retry-max-retries 5";
        var response = await ExecuteCommandAsync(args);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);

        // Verify the service was called with retry policy
        await _fastTranscriptionRecognizer.Received(1).RecognizeAsync(
        Arg.Any<string>(),
        Arg.Any<string>(),
        Arg.Any<string>(),
        Arg.Any<string[]>(),
        Arg.Any<string>(),
        Arg.Is<RetryPolicyOptions>(policy => policy.MaxRetries == 5));
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

        _realtimeTranscriptionRecognizer.RecognizeAsync(
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
            var args = $"--subscription {_knownSubscription} --endpoint {_knownEndpoint} --file {testFile} --language en-US --format simple";
            var parseResult = _commandDefinition.Parse(args.Split(' ', StringSplitOptions.RemoveEmptyEntries));
            var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

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

        var expectedText = ""; // empty result
        var realtimeResult = new RealtimeRecognitionContinuousResult
        {
            FullText = expectedText,
            Segments = new List<RealtimeRecognitionResult>
            {
                new RealtimeRecognitionResult { Text = expectedText, Reason = "NoMatch" }
            }
        };

        // Mock realtime transcription since en-US + format should use realtime
        _realtimeTranscriptionRecognizer.RecognizeAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string[]>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>())
            .Returns(realtimeResult);

        try
        {
            // Act
            var args = $"--subscription {_knownSubscription} --endpoint {_knownEndpoint} --file {testFile} --language en-US --format simple";
            var parseResult = _commandDefinition.Parse(args.Split(' ', StringSplitOptions.RemoveEmptyEntries));
            var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.Status);
            Assert.NotNull(response.Results);

            var result = JsonSerializer.Deserialize(
                JsonSerializer.Serialize(response.Results), SpeechJsonContext.Default.SttRecognizeCommandResult);
            Assert.NotNull(result);
            Assert.NotNull(result.Result.RealtimeContinuousResult);
            Assert.Equal("", result.Result.RealtimeContinuousResult.FullText);
            Assert.Equal("NoMatch", result.Result.RealtimeContinuousResult.Segments[0].Reason);
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

        var expectedText = "Long audio content recognition result";
        // Set up both mocks to always return results, then verify which one was called
        SetupFastTranscriptionMock(expectedText);

        try
        {
            // Act
            var args = $"--subscription {_knownSubscription} --endpoint {_knownEndpoint} --file {testFile}";
            var parseResult = _commandDefinition.Parse(args.Split(' ', StringSplitOptions.RemoveEmptyEntries));
            var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

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

        var expectedText = "Azure cognitive services";
        var fastResult = new FastTranscriptionResult
        {
            CombinedPhrases = new List<FastTranscriptionCombinedPhrase>
            {
                new FastTranscriptionCombinedPhrase { Text = expectedText }
            },
            DurationMilliseconds = 5000
        };

        string[]? capturedPhrases = null;
        _fastTranscriptionRecognizer.RecognizeAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Do<string[]>(phrases => capturedPhrases = phrases),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>())
            .Returns(fastResult);

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
            var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

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

        var expectedText = "Azure cognitive services";
        var fastResult = new FastTranscriptionResult
        {
            CombinedPhrases = new List<FastTranscriptionCombinedPhrase>
            {
                new FastTranscriptionCombinedPhrase { Text = expectedText }
            },
            DurationMilliseconds = 5000
        };

        string[]? capturedPhrases = null;
        _fastTranscriptionRecognizer.RecognizeAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Do<string[]>(phrases => capturedPhrases = phrases),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>())
            .Returns(fastResult);

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
            var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

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

        var expectedText = "Azure cognitive services machine learning";
        var fastResult = new FastTranscriptionResult
        {
            CombinedPhrases = new List<FastTranscriptionCombinedPhrase>
            {
                new FastTranscriptionCombinedPhrase { Text = expectedText }
            },
            DurationMilliseconds = 5000
        };

        string[]? capturedPhrases = null;
        _fastTranscriptionRecognizer.RecognizeAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Do<string[]>(phrases => capturedPhrases = phrases),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>())
            .Returns(fastResult);

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
            var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

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
            await _fastTranscriptionRecognizer.Received(1).RecognizeAsync(
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
    public async Task ExecuteAsync_WithDifferentAudioFormatsAndFastTranscription_ShouldSucceed(string fileName)
    {
        // Arrange
        await File.WriteAllTextAsync(fileName, "test audio content", TestContext.Current.CancellationToken);

        var expectedText = "Hello world";
        SetupFastTranscriptionMock(expectedText);

        try
        {
            // Act
            var args = $"--subscription {_knownSubscription} --endpoint {_knownEndpoint} --file {fileName}";
            var parseResult = _commandDefinition.Parse(args.Split(' ', StringSplitOptions.RemoveEmptyEntries));
            var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.Status);
            Assert.NotNull(response.Results);

            var result = JsonSerializer.Deserialize(
                JsonSerializer.Serialize(response.Results), SpeechJsonContext.Default.SttRecognizeCommandResult);
            Assert.NotNull(result);
            Assert.NotNull(result.Result.FastTranscriptionResult);
            Assert.Equal("Hello world", result.Result.FastTranscriptionResult.CombinedPhrases[0].Text);

            // Verify the service was called with the correct file path
            await _fastTranscriptionRecognizer.Received(1).RecognizeAsync(
            Arg.Any<string>(),
            fileName,
            Arg.Any<string>(),
            Arg.Any<string[]>(),
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
    [InlineData("test-audio.wav")]
    [InlineData("test-audio.mp3")]
    [InlineData("test-audio.m4a")]
    [InlineData("test-audio.flac")]
    [InlineData("test-audio.ogg")]
    public async Task ExecuteAsync_WithDifferentAudioFormatsAndRealtimeTranscription_ShouldSucceed(string fileName)
    {
        // Arrange
        await File.WriteAllTextAsync(fileName, "test audio content", TestContext.Current.CancellationToken);

        var expectedText = "Hello world";
        var realtimeResult = new RealtimeRecognitionContinuousResult
        {
            FullText = expectedText,
            Segments = new List<RealtimeRecognitionResult>
            {
                new RealtimeRecognitionResult { Text = expectedText, Reason = "RecognizedSpeech" }
            }
        };

        _realtimeTranscriptionRecognizer.RecognizeAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string[]>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>())
            .Returns(realtimeResult);

        try
        {
            // Act
            var args = $"--subscription {_knownSubscription} --endpoint {_knownEndpoint} --file {fileName} --language en-US --format simple";
            var parseResult = _commandDefinition.Parse(args.Split(' ', StringSplitOptions.RemoveEmptyEntries));
            var response = await _command.ExecuteAsync(_context, parseResult);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.Status);
            Assert.NotNull(response.Results);

            var result = JsonSerializer.Deserialize(
                JsonSerializer.Serialize(response.Results), SpeechJsonContext.Default.SttRecognizeCommandResult);
            Assert.NotNull(result);
            Assert.NotNull(result.Result.RealtimeContinuousResult);
            Assert.Equal("Hello world", result.Result.RealtimeContinuousResult.FullText);

            // Verify the service was called with the correct file path
            await _realtimeTranscriptionRecognizer.Received(1).RecognizeAsync(
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
            var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

            // Assert - The command should return validation error for invalid file extensions
            Assert.Equal(HttpStatusCode.BadRequest, response.Status);
            Assert.Contains("Unsupported audio file format", response.Message);

            // Verify the service was NOT called with invalid file extensions
            await _realtimeTranscriptionRecognizer.DidNotReceive().RecognizeAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string[]>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<RetryPolicyOptions>());

            await _fastTranscriptionRecognizer.DidNotReceive().RecognizeAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string[]>(),
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

        var expectedText = "Large file processed";
        SetupFastTranscriptionMock(expectedText);

        try
        {
            // Act
            var args = $"--subscription {_knownSubscription} --endpoint {_knownEndpoint} --file {largeFileName}";
            var parseResult = _commandDefinition.Parse(args.Split(' ', StringSplitOptions.RemoveEmptyEntries));
            var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.Status);
            Assert.NotNull(response.Results);

            var result = JsonSerializer.Deserialize(
                JsonSerializer.Serialize(response.Results), SpeechJsonContext.Default.SttRecognizeCommandResult);
            Assert.NotNull(result);
            Assert.NotNull(result.Result.FastTranscriptionResult);
            Assert.Equal("Large file processed", result.Result.FastTranscriptionResult.CombinedPhrases[0].Text);
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
}


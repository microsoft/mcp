// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.Speech.Commands.Tts;
using Azure.Mcp.Tools.Speech.Models;
using Azure.Mcp.Tools.Speech.Services;
using Microsoft.Mcp.Core.Options;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Speech.UnitTests.Tts;

public class TtsSynthesizeCommandTests : CommandUnitTestsBase<TtsSynthesizeCommand, ISpeechService>
{
    private readonly string _knownEndpoint = "https://eastus.cognitiveservices.azure.com/";
    private readonly string _knownSubscription = "sub123";

    [Theory]
    [InlineData("", false, "Missing Required options: --endpoint, --text, --outputAudio")]
    [InlineData("--subscription sub123", false, "Missing Required options: --endpoint, --text, --outputAudio")]
    [InlineData("--subscription sub123 --endpoint https://test.cognitiveservices.azure.com/", false, "Missing Required options: --text, --outputAudio")]
    [InlineData("--subscription sub123 --endpoint https://test.cognitiveservices.azure.com/ --text Hello", false, "Missing Required options: --outputAudio")]
    [InlineData("--subscription sub123 --endpoint https://test.cognitiveservices.azure.com/ --text Hello --outputAudio output.txt", false, "Unsupported output file format")]
    [InlineData("--subscription sub123 --endpoint https://test.cognitiveservices.azure.com/ --text Hello --outputAudio output.wav --language invalid", false, "Language must be in format 'xx-XX'")]
    public async Task ExecuteAsync_ValidatesInput(string args, bool shouldSucceed, string expectedError)
    {
        var response = await ExecuteCommandAsync(args.Split(' ', StringSplitOptions.RemoveEmptyEntries));

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
            AudioSize = 48000,
            Format = "Riff24Khz16BitMonoPcm",
            Voice = "en-US-JennyNeural",
            Language = "en-US"
        };

        Service.SynthesizeSpeechToFile(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        try
        {
            // Act
            var response = await ExecuteCommandAsync(
                "--subscription", _knownSubscription,
                "--endpoint", _knownEndpoint,
                "--text", text,
                "--outputAudio", outputFile);

            // Assert
            var result = ValidateAndDeserializeResponse(response, SpeechJsonContext.Default.TtsSynthesizeCommandResult);

            Assert.Equal(outputFile, result.Result.FilePath);
            Assert.Equal(48000, result.Result.AudioSize);
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
            AudioSize = 32000,
            Format = format,
            Voice = voice,
            Language = language
        };

        Service.SynthesizeSpeechToFile(
            Arg.Is(_knownEndpoint),
            Arg.Is(text),
            Arg.Is(outputFile),
            Arg.Is(language),
            Arg.Is(voice),
            Arg.Is(format),
            Arg.Is(endpointId),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        try
        {
            // Act
            var response = await ExecuteCommandAsync(
                "--subscription", _knownSubscription,
                "--endpoint", _knownEndpoint,
                "--text", text,
                "--outputAudio", outputFile,
                "--language", language,
                "--voice", voice,
                "--format", format,
                "--endpointId", endpointId);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.Status);

            await Service.Received(1).SynthesizeSpeechToFile(
                _knownEndpoint,
                text,
                outputFile,
                language,
                voice,
                format,
                endpointId,
                Arg.Any<RetryPolicyOptions?>(),
                Arg.Any<CancellationToken>());
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

        Service.SynthesizeSpeechToFile(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("Synthesis failed"));

        try
        {
            // Act
            var response = await ExecuteCommandAsync(
                "--subscription", _knownSubscription,
                "--endpoint", _knownEndpoint,
                "--text", text,
                "--outputAudio", outputFile);

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

        Service.SynthesizeSpeechToFile(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new UnauthorizedAccessException("Access denied"));

        try
        {
            // Act
            var response = await ExecuteCommandAsync(
                "--subscription", _knownSubscription,
                "--endpoint", _knownEndpoint,
                "--text", text,
                "--outputAudio", outputFile);

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

    [Theory]
    [InlineData(@"\\server\share\output.wav", "UNC")]
    [InlineData("//server/share/output.wav", "UNC")]
    public async Task ExecuteAsync_WithUncOutputPath_ShouldRejectPath(string outputPath, string expectedErrorFragment)
    {
        var response = await ExecuteCommandAsync(
            "--subscription", _knownSubscription,
            "--endpoint", _knownEndpoint,
            "--text", "HelloWorld",
            "--outputAudio", outputPath);

        Assert.NotEqual(HttpStatusCode.OK, response.Status);
        Assert.Contains(expectedErrorFragment, response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_WithPathTraversal_ShouldCanonicalizeOutputPath()
    {
        // A traversal path should be canonicalized; the command should not blindly pass it through.
        var response = await ExecuteCommandAsync(
            "--subscription", _knownSubscription,
            "--endpoint", _knownEndpoint,
            "--text", "HelloWorld",
            "--outputAudio", "../../../tmp/evil.wav");

        // The path should be canonicalized - either it succeeds after canonicalization
        // or fails validation, but it should never pass the raw "../../../" path through.
        // Since the file doesn't exist, the command should proceed (no overwrite check failure).
        // The key assertion is that the service receives a canonical path.
        if (response.Status == HttpStatusCode.OK)
        {
            await Service.Received(1).SynthesizeSpeechToFile(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Is<string>(p => !p.Contains("..")),
                Arg.Any<string?>(),
                Arg.Any<string?>(),
                Arg.Any<string?>(),
                Arg.Any<string?>(),
                Arg.Any<RetryPolicyOptions?>(),
                Arg.Any<CancellationToken>());
        }
    }
}

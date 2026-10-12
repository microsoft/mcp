// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Security;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tests.Helpers;
using Azure.Mcp.Tools.Speech.Services;
using Azure.Mcp.Tools.Speech.Services.Recognizers;
using Azure.Mcp.Tools.Speech.Services.Synthesizers;
using Azure.ResourceManager;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Services.Azure.Authentication;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.Speech.Tests.Services;

public sealed class SpeechEndpointBoundaryTests
{
    private const string DisallowedEndpoint = "https://speech.cognitiveservices.azure.com.evil.example/";

    [Fact]
    public async Task FastTranscriptionRecognizer_DisallowedEndpoint_RejectsBeforeCredentialAcquisition()
    {
        IAzureService azureService = CreateAzureService(ArmEnvironment.AzurePublicCloud);
        FastTranscriptionRecognizer recognizer = new(
            azureService,
            Substitute.For<ILogger<FastTranscriptionRecognizer>>());

        await Assert.ThrowsAsync<SecurityException>(() =>
            recognizer.RecognizeAsync(
                DisallowedEndpoint,
                "missing.wav",
                cancellationToken: TestContext.Current.CancellationToken));

        await azureService.DidNotReceive()
            .GetTokenCredentialAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RealtimeTranscriptionRecognizer_DisallowedEndpoint_RejectsBeforeCredentialAcquisition()
    {
        IAzureService azureService = CreateAzureService(ArmEnvironment.AzurePublicCloud);
        RealtimeTranscriptionRecognizer recognizer = new(
            azureService,
            Substitute.For<ILogger<RealtimeTranscriptionRecognizer>>());

        await Assert.ThrowsAsync<SecurityException>(() =>
            recognizer.RecognizeAsync(
                DisallowedEndpoint,
                "missing.wav",
                cancellationToken: TestContext.Current.CancellationToken));

        await azureService.DidNotReceive()
            .GetTokenCredentialAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RealtimeTtsSynthesizer_DisallowedEndpoint_RejectsBeforeCredentialAcquisition()
    {
        IAzureService azureService = CreateAzureService(ArmEnvironment.AzurePublicCloud);
        RealtimeTtsSynthesizer synthesizer = new(
            azureService,
            Substitute.For<ILogger<RealtimeTtsSynthesizer>>());

        await Assert.ThrowsAsync<SecurityException>(() =>
            synthesizer.SynthesizeToFileAsync(
                DisallowedEndpoint,
                "hello",
                "output.wav",
                cancellationToken: TestContext.Current.CancellationToken));

        await azureService.DidNotReceive()
            .GetTokenCredentialAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(DisallowedEndpoint)]
    [InlineData("https://speech.cognitiveservices.azure.com/")]
    public async Task SpeechService_DisallowedEndpoint_DoesNotFallbackToRealtime(string endpoint)
    {
        IAzureService azureService = CreateAzureService(ArmEnvironment.AzureChina);
        var fastRecognizer = new FastTranscriptionRecognizer(
            azureService,
            Substitute.For<ILogger<FastTranscriptionRecognizer>>());
        IRealtimeTranscriptionRecognizer realtimeRecognizer = Substitute.For<IRealtimeTranscriptionRecognizer>();
        var speechService = new SpeechService(
            azureService,
            Substitute.For<ILogger<SpeechService>>(),
            fastRecognizer,
            realtimeRecognizer,
            Substitute.For<IRealtimeTtsSynthesizer>());
        string filePath = $"speech-endpoint-{Guid.NewGuid():N}.wav";

        try
        {
            // The orchestration requires an existing file before selecting the real fast recognizer.
            await File.WriteAllBytesAsync(filePath, [], TestContext.Current.CancellationToken);

            await Assert.ThrowsAsync<SecurityException>(() =>
                speechService.RecognizeSpeechFromFile(
                    endpoint,
                    filePath,
                    cancellationToken: TestContext.Current.CancellationToken));

            await realtimeRecognizer.DidNotReceive().RecognizeAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string?>(),
                Arg.Any<string[]?>(),
                Arg.Any<string?>(),
                Arg.Any<string?>(),
                Arg.Any<CancellationToken>());
            await azureService.DidNotReceive()
                .GetTokenCredentialAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>());
        }
        finally
        {
            File.Delete(filePath);
        }
    }

    private static IAzureService CreateAzureService(ArmEnvironment armEnvironment)
    {
        IAzureService azureService = AzureServiceTestHelpers.CreateAzureService();
        IAzureCloudConfiguration cloudConfiguration = Substitute.For<IAzureCloudConfiguration>();
        cloudConfiguration.ArmEnvironment.Returns(armEnvironment);
        azureService.CloudConfiguration.Returns(cloudConfiguration);
        return azureService;
    }
}

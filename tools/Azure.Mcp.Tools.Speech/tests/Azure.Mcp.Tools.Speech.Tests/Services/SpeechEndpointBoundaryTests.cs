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
using Microsoft.Mcp.Core.Areas.Server;
using Microsoft.Mcp.Core.Services.Azure.Authentication;
using Microsoft.Mcp.Core.Services.Host;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;
using OptionsFactory = Microsoft.Extensions.Options.Options;

namespace Azure.Mcp.Tools.Speech.Tests.Services;

public sealed class SpeechEndpointBoundaryTests
{
    private static readonly IFileSystem s_fileSystem = new FileSystem(OptionsFactory.Create(new ServerRuntimeConfiguration()));
    private const string DisallowedEndpoint = "https://speech.cognitiveservices.azure.com.evil.example/";

    [Fact]
    public async Task HttpMode_DeniesAllFileOperationsBeforeCredentialAcquisition()
    {
        var azureService = CreateAzureService(ArmEnvironment.AzurePublicCloud);
        var fileSystem = new FileSystem(OptionsFactory.Create(new ServerRuntimeConfiguration { Transport = "http" }));
        const string endpoint = "https://speech.cognitiveservices.azure.com/";
        var fast = new FastTranscriptionRecognizer(azureService, Substitute.For<ILogger<FastTranscriptionRecognizer>>(), fileSystem);
        var realtime = new RealtimeTranscriptionRecognizer(azureService, Substitute.For<ILogger<RealtimeTranscriptionRecognizer>>(), fileSystem);
        var synthesizer = new RealtimeTtsSynthesizer(azureService, Substitute.For<ILogger<RealtimeTtsSynthesizer>>(), fileSystem);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            fast.RecognizeAsync(endpoint, "missing.wav", cancellationToken: TestContext.Current.CancellationToken));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            realtime.RecognizeAsync(endpoint, "missing.wav", cancellationToken: TestContext.Current.CancellationToken));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            synthesizer.SynthesizeToFileAsync(endpoint, "hello", "output.wav", cancellationToken: TestContext.Current.CancellationToken));

        await azureService.DidNotReceive()
            .GetTokenCredentialAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SpeechService_AccessDeniedDoesNotFallbackToRealtime()
    {
        var azureService = CreateAzureService(ArmEnvironment.AzurePublicCloud);
        var fast = Substitute.For<IFastTranscriptionRecognizer>();
        var realtime = Substitute.For<IRealtimeTranscriptionRecognizer>();
        var fileSystem = Substitute.For<IFileSystem>();
        fileSystem.FileExists(Arg.Any<string>()).Returns(true);
        fast.RecognizeAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(),
            Arg.Any<string[]?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new UnauthorizedAccessException("Access denied."));
        var service = new SpeechService(azureService, Substitute.For<ILogger<SpeechService>>(),
            fast, realtime, Substitute.For<IRealtimeTtsSynthesizer>(), fileSystem);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            service.RecognizeSpeechFromFile("https://speech.cognitiveservices.azure.com/", "missing.wav",
                cancellationToken: TestContext.Current.CancellationToken));
        Assert.Empty(realtime.ReceivedCalls());
    }

    [Fact]
    public async Task FastTranscriptionRecognizer_DisallowedEndpoint_RejectsBeforeCredentialAcquisition()
    {
        IAzureService azureService = CreateAzureService(ArmEnvironment.AzurePublicCloud);
        FastTranscriptionRecognizer recognizer = new(
            azureService,
            Substitute.For<ILogger<FastTranscriptionRecognizer>>(), s_fileSystem);

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
            Substitute.For<ILogger<RealtimeTranscriptionRecognizer>>(), s_fileSystem);

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
            Substitute.For<ILogger<RealtimeTtsSynthesizer>>(), s_fileSystem);

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
            Substitute.For<ILogger<FastTranscriptionRecognizer>>(), s_fileSystem);
        IRealtimeTranscriptionRecognizer realtimeRecognizer = Substitute.For<IRealtimeTranscriptionRecognizer>();
        var speechService = new SpeechService(
            azureService,
            Substitute.For<ILogger<SpeechService>>(),
            fastRecognizer,
            realtimeRecognizer,
            Substitute.For<IRealtimeTtsSynthesizer>(), s_fileSystem);
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

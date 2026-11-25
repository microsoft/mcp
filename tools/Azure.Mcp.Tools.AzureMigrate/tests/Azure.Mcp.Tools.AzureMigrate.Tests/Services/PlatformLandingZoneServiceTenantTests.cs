// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Core;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tools.AzureMigrate.Helpers;
using Azure.Mcp.Tools.AzureMigrate.Models;
using Azure.Mcp.Tools.AzureMigrate.Services;
using Azure.Mcp.Tools.AzureMigrate.Tests.TestSupport;
using Azure.ResourceManager;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Mcp.Core.Areas.Server;
using Microsoft.Mcp.Core.Services.Azure.Authentication;
using Microsoft.Mcp.Core.Services.Host;
using NSubstitute;
using Xunit;
using OptionsFactory = Microsoft.Extensions.Options.Options;

namespace Azure.Mcp.Tools.AzureMigrate.Tests.Services;

public sealed class PlatformLandingZoneServiceTenantTests
{
    private const string SubscriptionId = "00000000-0000-0000-0000-000000000001";
    private const string TenantName = "Contoso Ltd";
    private const string TenantId = "22222222-2222-2222-2222-222222222222";
    private const string DefaultToken = "default-tenant-token";
    private const string SelectedToken = "selected-tenant-token";
    private const string DownloadUrl = "https://download.example/landing-zone.zip";

    [Theory]
    [InlineData("check")]
    [InlineData("generate")]
    [InlineData("download")]
    public async Task Action_WithTenantDisplayName_UsesResolvedTenantCredential(string action)
    {
        var azureService = CreateAzureService();
        List<string?> armTokens = [];
        using var handler = new RecordingHttpMessageHandler(request => Respond(request, armTokens));
        azureService.GetClient(Arg.Any<string?>()).Returns(_ => new HttpClient(handler, disposeHandler: false));
        var service = new PlatformLandingZoneService(
            azureService,
            new AzureHttpHelper(azureService),
            NullLogger<PlatformLandingZoneService>.Instance,
            CreateLocalFileSystem());
        var context = new PlatformLandingZoneContext(SubscriptionId, "rg", $"project-{Guid.NewGuid():N}", TenantName);
        var cancellationToken = TestContext.Current.CancellationToken;

        switch (action)
        {
            case "check":
                Assert.True(await service.CheckExistingAsync(context, cancellationToken));
                break;
            case "generate":
                await service.UpdateParametersAsync(context, null, null, null, null, null, null, null, null, null, null, cancellationToken);
                Assert.NotNull(await service.GenerateAsync(context, cancellationToken));
                break;
            case "download":
                var outputPath = Directory.CreateTempSubdirectory().FullName;
                try
                {
                    Assert.True(File.Exists(await service.DownloadAsync(context, outputPath, cancellationToken)));
                }
                finally
                {
                    Directory.Delete(outputPath, recursive: true);
                }
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(action), action, null);
        }

        Assert.Equal(SelectedToken, Assert.Single(armTokens));
        await azureService.DidNotReceive().GetTokenCredentialAsync(null, Arg.Any<CancellationToken>());
        await azureService.DidNotReceive().GetTokenCredentialAsync(TenantName, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CheckExistingAsync_WithoutTenant_UsesDefaultCredentialWithoutResolvingTenant()
    {
        var azureService = CreateAzureService();
        List<string?> armTokens = [];
        using var handler = new RecordingHttpMessageHandler(request => Respond(request, armTokens));
        azureService.GetClient(Arg.Any<string?>()).Returns(_ => new HttpClient(handler, disposeHandler: false));
        var service = new PlatformLandingZoneService(
            azureService,
            new AzureHttpHelper(azureService),
            NullLogger<PlatformLandingZoneService>.Instance,
            CreateLocalFileSystem());
        var context = new PlatformLandingZoneContext(SubscriptionId, "rg", "project");

        Assert.True(await service.CheckExistingAsync(context, TestContext.Current.CancellationToken));

        Assert.Equal(DefaultToken, Assert.Single(armTokens));
        await azureService.DidNotReceive().ResolveTenantIdAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>());
    }

    private static HttpResponseMessage Respond(HttpRequestMessage request, List<string?> armTokens)
    {
        var uri = request.RequestUri!;
        if (uri.Host == new Uri(DownloadUrl).Host)
        {
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent([0x50, 0x4B]) };
        }

        armTokens.Add(request.Headers.Authorization?.Parameter);
        var body = uri.AbsolutePath switch
        {
            var path when path.EndsWith("/CheckPlatformLandingZone", StringComparison.Ordinal) => """{"exists":true}""",
            var path when path.EndsWith("/DownloadPlatformLandingZone", StringComparison.Ordinal) => $$"""{"downloadUrl":"{{DownloadUrl}}"}""",
            _ => "{}"
        };
        return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(body) };
    }

    private static IFileSystem CreateLocalFileSystem() =>
        new Microsoft.Mcp.Core.Services.Host.FileSystem(OptionsFactory.Create(new ServerRuntimeConfiguration()));

    private static IAzureService CreateAzureService()
    {
        var cloudConfiguration = Substitute.For<IAzureCloudConfiguration>();
        cloudConfiguration.ArmEnvironment.Returns(ArmEnvironment.AzurePublicCloud);
        var azureService = Substitute.For<IAzureService>();
        azureService.CloudConfiguration.Returns(cloudConfiguration);
        var defaultCredential = CreateCredential(DefaultToken);
        var selectedCredential = CreateCredential(SelectedToken);
        azureService.GetTokenCredentialAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(defaultCredential);
        azureService.GetTokenCredentialAsync(TenantId, Arg.Any<CancellationToken>())
            .Returns(selectedCredential);
        azureService.ResolveTenantIdAsync(TenantName, Arg.Any<CancellationToken>())
            .Returns(TenantId);
        return azureService;
    }

    private static TokenCredential CreateCredential(string token)
    {
        var credential = Substitute.For<TokenCredential>();
        credential.GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>())
            .Returns(new AccessToken(token, DateTimeOffset.UtcNow.AddHours(1)));
        return credential;
    }
}

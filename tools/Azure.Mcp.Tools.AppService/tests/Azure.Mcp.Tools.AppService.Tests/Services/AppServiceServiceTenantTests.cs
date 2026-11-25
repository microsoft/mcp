// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Core;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tests.Helpers;
using Azure.Mcp.Tools.AppService.Services;
using Azure.Mcp.Tools.AppService.Tests.TestSupport;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.AppService.Tests.Services;

public sealed class AppServiceServiceTenantTests
{
    private const string SubscriptionId = "00000000-0000-0000-0000-000000000001";
    private const string TenantName = "Contoso Ltd";
    private const string TenantId = "22222222-2222-2222-2222-222222222222";
    private const string DefaultToken = "default-tenant-token";
    private const string SelectedToken = "selected-tenant-token";

    [Fact]
    public async Task ListDetectorsAsync_WithTenantDisplayName_UsesResolvedTenantCredential()
    {
        var azureService = CreateAzureService();
        string? accessToken = null;
        using var handler = new RecordingHttpMessageHandler(request =>
        {
            accessToken = request.Headers.Authorization?.Parameter;
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("""{"value":[]}""") };
        });
        azureService.GetClient(Arg.Any<string?>()).Returns(_ => new HttpClient(handler, disposeHandler: false));
        var service = new AppServiceService(azureService, NullLogger<AppServiceService>.Instance);

        var detectors = await service.ListDetectorsAsync(
            SubscriptionId,
            "rg",
            "app",
            TenantName,
            TestContext.Current.CancellationToken);

        Assert.Empty(detectors);
        Assert.Single(handler.RequestUris);
        Assert.Equal(SelectedToken, accessToken);
        await azureService.Received(1).ResolveTenantIdAsync(TenantName, Arg.Any<CancellationToken>());
        await azureService.DidNotReceive().GetTokenCredentialAsync(TenantName, Arg.Any<CancellationToken>());
    }

    private static IAzureService CreateAzureService()
    {
        var azureService = AzureServiceTestHelpers.CreateAzureService();
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

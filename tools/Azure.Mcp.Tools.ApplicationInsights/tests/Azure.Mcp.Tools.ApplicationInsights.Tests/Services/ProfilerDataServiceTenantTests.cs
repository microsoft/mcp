// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Core;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tests.Helpers;
using Azure.Mcp.Tools.ApplicationInsights.Services;
using Azure.Mcp.Tools.ApplicationInsights.Tests.TestSupport;
using Azure.ResourceManager;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.ApplicationInsights.Tests.Services;

public sealed class ProfilerDataServiceTenantTests
{
    private const string TenantName = "Contoso Ltd";
    private const string TenantId = "22222222-2222-2222-2222-222222222222";
    private const string DefaultToken = "default-tenant-token";
    private const string SelectedToken = "selected-tenant-token";
    private const string AppId = "33333333-3333-3333-3333-333333333333";
    private const string ComponentId =
        "/subscriptions/00000000-0000-0000-0000-000000000001/resourceGroups/rg/providers/microsoft.insights/components/app";

    [Fact]
    public async Task GetInsightsAsync_WithTenantDisplayName_UsesResolvedTenantForArmAndDataPlane()
    {
        var azureService = CreateAzureService();
        List<string?> armTokens = [];
        using var armHandler = new RecordingHttpMessageHandler(request =>
        {
            armTokens.Add(request.Headers.Authorization?.Parameter);
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent($$$"""
                    {"id":"{{{ComponentId}}}","name":"app","type":"microsoft.insights/components",
                     "location":"eastus","kind":"web","properties":{"AppId":"{{{AppId}}}","Application_Type":"web"}}
                    """)
            };
        });
        using var armHttpClient = new HttpClient(armHandler, disposeHandler: false);
        AzureServiceTestHelpers.ConfigureArmClientOptions(azureService, armHttpClient, ArmEnvironment.AzurePublicCloud);
        string? dataPlaneToken = null;
        using var dataPlaneHandler = new RecordingHttpMessageHandler(request =>
        {
            dataPlaneToken = request.Headers.Authorization?.Parameter;
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("[]") };
        });
        azureService.GetClient(Arg.Any<string?>()).Returns(_ => new HttpClient(dataPlaneHandler, disposeHandler: false));
        var service = new ProfilerDataService(NullLogger<ProfilerDataService>.Instance, azureService);

        var insights = await service.GetInsightsAsync(
            [new ResourceIdentifier(ComponentId)],
            TenantName,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Empty(insights);
        Assert.NotEmpty(armTokens);
        Assert.All(armTokens, token => Assert.Equal(SelectedToken, token));
        Assert.Single(dataPlaneHandler.RequestUris);
        Assert.Equal(SelectedToken, dataPlaneToken);
        await azureService.DidNotReceive().GetTokenCredentialAsync(null, Arg.Any<CancellationToken>());
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
        credential.GetToken(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>())
            .Returns(new AccessToken(token, DateTimeOffset.UtcNow.AddHours(1)));
        return credential;
    }
}

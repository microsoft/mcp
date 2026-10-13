// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Security;
using System.Text.Json;
using Azure.Core;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tests.Helpers;
using Azure.Mcp.Tools.Monitor.Models.Traces;
using Azure.Mcp.Tools.Monitor.Services.Traces;
using Azure.ResourceManager.Models;
using Azure.ResourceManager.Resources;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Mcp.Core.Services.Caching;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.Monitor.Tests.Traces;

public sealed class MonitorTracesServiceTests()
{
    private const string ResourceGroup = "rg";

    [Theory]
    [InlineData(0, false)]
    [InlineData(0, true)]
    [InlineData(1, false)]
    [InlineData(1, true)]
    [InlineData(50, true)]
    public async Task ListAccountsAsync_PreservesDiscoveryTruncationAfterProbing(int workspaceCount, bool areResultsTruncated)
    {
        using var handler = new MonitorTracesTestHandler(workspaceCount, areResultsTruncated);
        (MonitorTracesService service, IAzureService _) = CreateService(handler);
        ResourceQueryResults<TraceAccountInfo> result = await service.ListAccountsAsync(
            MonitorTracesTestHandler.Subscription, cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(areResultsTruncated, result.AreResultsTruncated);
        Assert.Equal((workspaceCount + 1) / 2, result.Results.Count);
        Assert.All(result.Results, account => Assert.Equal(3600, account.TraceDurationWindowInSeconds));
    }

    [Theory]
    [InlineData("https://evil.example.com")]
    [InlineData("https://amw.data.monitor.azure.com.evil.example.com")]
    [InlineData("http://amw.data.monitor.azure.com")]
    public async Task ListFieldNamesAsync_RejectsEndpointBeforeDataPlaneTokenOrRequest(string endpoint)
    {
        using var handler = new MonitorTracesTestHandler(1, false, endpoint);
        (MonitorTracesService service, IAzureService azureService) = CreateService(handler);

        await Assert.ThrowsAsync<SecurityException>(() => service.ListFieldNamesAsync(
            MonitorTracesTestHandler.Subscription, ResourceGroup, "amw0", cancellationToken: TestContext.Current.CancellationToken));

        Assert.Equal(0, handler.DataPlaneCalls);
        TokenCredential credential = await azureService.GetTokenCredentialAsync(null, TestContext.Current.CancellationToken);
        await credential.DidNotReceive().GetTokenAsync(
            Arg.Is<TokenRequestContext>(context => context.Scopes.Contains(MonitorTracesService.TraceDataPlaneScope)),
            Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("rg-a")]
    [InlineData("rg-b")]
    public async Task ListFieldNamesAsync_UsesScopedWorkspaceAndValidatesEndpoints(string resourceGroup)
    {
        using var handler = new MonitorTracesTestHandler(1, false);
        (MonitorTracesService service, IAzureService azureService) = CreateService(handler);
        await service.ListFieldNamesAsync(
            MonitorTracesTestHandler.Subscription, resourceGroup, "amw0", cancellationToken: TestContext.Current.CancellationToken);

        Assert.Null(handler.LastGraphQuery);
        Assert.Equal(
            $"/subscriptions/{MonitorTracesTestHandler.Subscription}/resourceGroups/{resourceGroup}/providers/Microsoft.Monitor/accounts/amw0",
            handler.LastWorkspaceUri!.AbsolutePath);
        Assert.Contains("api-version=2025-10-03-preview", handler.LastWorkspaceUri.Query);
        await azureService.Received(1).GetResourceGroupResource(
            MonitorTracesTestHandler.Subscription, resourceGroup, Arg.Any<string?>(), Arg.Any<CancellationToken>());
        azureService.Received().ValidateAzureServiceEndpoint(
            "https://amw.data.monitor.azure.com", "monitor-traces");
        azureService.Received(1).ValidateAzureServiceEndpoint(
            handler.LastDataPlaneUri!.AbsoluteUri, "monitor-traces");
        Assert.Equal(1, handler.DataPlaneCalls);
    }

    [Theory]
    [InlineData("/subscriptions/sub/resourceGroups/rg/providers/Microsoft.Monitor/accounts/amw0")]
    [InlineData("https://evil.example.com")]
    [InlineData("amw0/traceContainers/default")]
    [InlineData("amw0?api-version=bad")]
    [InlineData("amw0#fragment")]
    [InlineData("amw0\\other")]
    public async Task ListFieldNamesAsync_RejectsNonNameWorkspaceBeforeAzureRequests(string workspace)
    {
        using var handler = new MonitorTracesTestHandler(1, false);
        (MonitorTracesService service, IAzureService azureService) = CreateService(handler);

        await Assert.ThrowsAsync<ArgumentException>(() => service.ListFieldNamesAsync(
            MonitorTracesTestHandler.Subscription, ResourceGroup, workspace, cancellationToken: TestContext.Current.CancellationToken));

        await azureService.DidNotReceive().GetResourceGroupResource(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<CancellationToken>());
        await azureService.DidNotReceive().GetTokenCredentialAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>());
        Assert.Equal(0, handler.DataPlaneCalls);
    }

    [Theory]
    [InlineData("", "unknown")]
    [InlineData(" ", "unknown")]
    [InlineData("{\"status\":\"Cancelled\"}", "Cancelled")]
    public async Task CancelQueryAsync_DistinguishesAbsentBodyFromStatus(string body, string expectedStatus)
    {
        using var handler = new MonitorTracesTestHandler(1, false, cancelBody: body);
        (MonitorTracesService service, IAzureService _) = CreateService(handler);

        TraceQueryCancelResult result = await service.CancelQueryAsync(
            MonitorTracesTestHandler.Subscription, ResourceGroup, "amw0", "op-1234", cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(expectedStatus, result.Status);
    }

    [Theory]
    [InlineData("not-json")]
    [InlineData("{\"status\":")]
    public async Task CancelQueryAsync_RejectsMalformedNonemptyBody(string body)
    {
        using var handler = new MonitorTracesTestHandler(1, false, cancelBody: body);
        (MonitorTracesService service, IAzureService _) = CreateService(handler);

        HttpRequestException exception = await Assert.ThrowsAsync<HttpRequestException>(() => service.CancelQueryAsync(
            MonitorTracesTestHandler.Subscription, ResourceGroup, "amw0", "op-1234", cancellationToken: TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.BadGateway, exception.StatusCode);
        Assert.IsAssignableFrom<JsonException>(exception.InnerException);
    }

    [Fact]
    public async Task QueryAndResume_KeepSubmissionAndPollSettingsSeparate()
    {
        using var handler = new MonitorTracesTestHandler(1, false);
        (MonitorTracesService service, IAzureService _) = CreateService(handler);
        DateTimeOffset end = DateTimeOffset.Parse("2026-10-12T00:00:00Z", System.Globalization.CultureInfo.InvariantCulture);
        const string query = "Traces | where trace_id == '\"quoted\"'";

        await service.RunQueryAsync(MonitorTracesTestHandler.Subscription, ResourceGroup, "amw0", query,
            end.AddMinutes(-30), end, 1000, true, cancellationToken: TestContext.Current.CancellationToken);
        using JsonDocument submit = JsonDocument.Parse(handler.LastSubmitBody!);
        Assert.Equal(query, submit.RootElement.GetProperty("query").GetString());
        Assert.Equal(1000, submit.RootElement.GetProperty("options").GetProperty("maxResults").GetInt32());

        await service.ResumeQueryAsync(MonitorTracesTestHandler.Subscription, ResourceGroup, "amw0", "op-1234",
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(1, handler.SubmitCalls);
        Assert.Contains("pageSize=100&", handler.LastDataPlaneUri!.Query);
        Assert.Contains("api-version=2026-01-10-preview", handler.LastDataPlaneUri.Query);
    }

    private static (MonitorTracesService Service, IAzureService AzureService) CreateService(MonitorTracesTestHandler handler)
    {
        var armClient = new HttpClient(handler, disposeHandler: false);
        IAzureService azureService = AzureServiceTestHelpers.CreateAzureService();
        AzureServiceTestHelpers.ConfigureArmClientOptions(azureService, armClient, azureService.CloudConfiguration.ArmEnvironment);
        azureService.GetClient(Arg.Any<string?>()).Returns(_ => new HttpClient(handler, disposeHandler: false));

        SubscriptionResource subscription = Substitute.For<SubscriptionResource>();
        subscription.Data.Returns(ResourceManagerModelFactory.SubscriptionData(
            subscriptionId: MonitorTracesTestHandler.Subscription,
            tenantId: Guid.Parse("00000000-0000-0000-0000-000000000002")));
        azureService.GetSubscription(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<CancellationToken>()).Returns(subscription);
        azureService.GetResourceGroupResource(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                string resourceGroup = call.ArgAt<string>(1);
                ResourceGroupResource group = Substitute.For<ResourceGroupResource>();
                group.Id.Returns(new ResourceIdentifier($"/subscriptions/{MonitorTracesTestHandler.Subscription}/resourceGroups/{resourceGroup}"));
                return group;
            });

        TokenCredential credential = Substitute.For<TokenCredential>();
        var token = new AccessToken("test-token", DateTimeOffset.UtcNow.AddHours(1));
        credential.GetToken(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>()).Returns(token);
        credential.GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>()).Returns(token);
        azureService.GetTokenCredentialAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>()).Returns(credential);

        var service = new MonitorTracesService(
            azureService, Substitute.For<ICacheService>(), NullLogger<MonitorTracesService>.Instance);
        return (service, azureService);
    }
}

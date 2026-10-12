// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Core;
using Azure.Mcp.Tests.Helpers;
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
    [Theory]
    [InlineData(0, false)]
    [InlineData(0, true)]
    [InlineData(1, false)]
    [InlineData(1, true)]
    [InlineData(50, true)]
    public async Task ListAccountsAsync_PreservesDiscoveryTruncationAfterProbing(int workspaceCount, bool areResultsTruncated)
    {
        using var handler = new TraceAccountListTestHandler(workspaceCount, areResultsTruncated);
        using var armClient = new HttpClient(handler, disposeHandler: false);
        var azureService = AzureServiceTestHelpers.CreateAzureService();
        AzureServiceTestHelpers.ConfigureArmClientOptions(azureService, armClient, azureService.CloudConfiguration.ArmEnvironment);
        azureService.GetClient(Arg.Any<string?>()).Returns(_ => new HttpClient(handler, disposeHandler: false));

        var subscription = Substitute.For<SubscriptionResource>();
        subscription.Data.Returns(ResourceManagerModelFactory.SubscriptionData(
            subscriptionId: TraceAccountListTestHandler.Subscription,
            tenantId: Guid.Parse("00000000-0000-0000-0000-000000000002")));
        azureService.GetSubscription(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<CancellationToken>()).Returns(subscription);

        var credential = Substitute.For<TokenCredential>();
        var token = new AccessToken("test-token", DateTimeOffset.UtcNow.AddHours(1));
        credential.GetToken(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>()).Returns(token);
        credential.GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>()).Returns(token);
        azureService.GetTokenCredentialAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>()).Returns(credential);

        var service = new MonitorTracesService(
            azureService, Substitute.For<ICacheService>(), NullLogger<MonitorTracesService>.Instance);
        var result = await service.ListAccountsAsync(
            TraceAccountListTestHandler.Subscription, cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(areResultsTruncated, result.AreResultsTruncated);
        Assert.Equal((workspaceCount + 1) / 2, result.Results.Count);
        Assert.All(result.Results, account => Assert.Equal(3600, account.TraceDurationWindowInSeconds));
    }
}

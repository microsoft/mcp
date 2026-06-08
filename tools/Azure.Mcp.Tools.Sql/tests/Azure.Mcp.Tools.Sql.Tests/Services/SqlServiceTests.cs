// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Core;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Tools.Sql.Services;
using Azure.ResourceManager;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Options;
using Microsoft.Mcp.Core.Services.Azure.Authentication;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Sql.Tests.Services;

public class SqlServiceTests
{
    private const string SubscriptionName = "my-subscription-name";

    // Sentinel thrown by the mocked subscription service so tests can prove the
    // service resolves the subscription via ISubscriptionService (the #449/#453 fix)
    // instead of building a SubscriptionResource directly from the raw value.
    private sealed class SubscriptionResolvedException : Exception;

    private readonly ISubscriptionService _subscriptionService;
    private readonly ITenantService _tenantService;
    private readonly ILogger<SqlService> _logger;
    private readonly SqlService _service;

    public SqlServiceTests()
    {
        _subscriptionService = Substitute.For<ISubscriptionService>();
        _tenantService = Substitute.For<ITenantService>();
        _logger = Substitute.For<ILogger<SqlService>>();

        var cloudConfig = Substitute.For<IAzureCloudConfiguration>();
        cloudConfig.CloudType.Returns(AzureCloudConfiguration.AzureCloud.AzurePublicCloud);
        cloudConfig.AuthorityHost.Returns(new Uri("https://login.microsoftonline.com"));
        cloudConfig.ArmEnvironment.Returns(ArmEnvironment.AzurePublicCloud);
        _tenantService.CloudConfiguration.Returns(cloudConfig);

        var credential = Substitute.For<TokenCredential>();
        _tenantService.GetTokenCredentialAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(credential));

        _subscriptionService.GetSubscription(
                Arg.Any<string>(),
                Arg.Any<string?>(),
                Arg.Any<RetryPolicyOptions?>(),
                Arg.Any<CancellationToken>())
            .ThrowsAsync<SubscriptionResolvedException>();

        _service = new SqlService(_subscriptionService, _tenantService, _logger);
    }

    [Fact]
    public async Task GetServerAsync_ResolvesSubscriptionThroughSubscriptionService()
    {
        await Assert.ThrowsAsync<SubscriptionResolvedException>(() =>
            _service.GetServerAsync("server1", "rg", SubscriptionName, null, TestContext.Current.CancellationToken));

        await AssertSubscriptionResolvedAsync();
    }

    [Fact]
    public async Task ListServersAsync_ResolvesSubscriptionThroughSubscriptionService()
    {
        await Assert.ThrowsAsync<SubscriptionResolvedException>(() =>
            _service.ListServersAsync("rg", SubscriptionName, null, TestContext.Current.CancellationToken));

        await AssertSubscriptionResolvedAsync();
    }

    [Fact]
    public async Task CreateServerAsync_ResolvesSubscriptionThroughSubscriptionService()
    {
        await Assert.ThrowsAsync<SubscriptionResolvedException>(() =>
            _service.CreateServerAsync(
                "server1",
                "rg",
                SubscriptionName,
                "eastus",
                "admin",
                "P@ssw0rd!",
                null,
                null,
                null,
                TestContext.Current.CancellationToken));

        await AssertSubscriptionResolvedAsync();
    }

    [Fact]
    public async Task RenameDatabaseAsync_ResolvesSubscriptionThroughSubscriptionService()
    {
        await Assert.ThrowsAsync<SubscriptionResolvedException>(() =>
            _service.RenameDatabaseAsync(
                "server1",
                "olddb",
                "newdb",
                "rg",
                SubscriptionName,
                null,
                TestContext.Current.CancellationToken));

        await AssertSubscriptionResolvedAsync();
    }

    private Task AssertSubscriptionResolvedAsync() =>
        _subscriptionService.Received(1).GetSubscription(
            SubscriptionName,
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>());
}

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
    private const string SubscriptionName = "my-subscription";
    private const string SubscriptionId = "12345678-1234-1234-1234-123456789012";
    private const string ResolveSentinel = "SqlServiceTests: subscription name resolved via ISubscriptionService";
    private const string ServerName = "server1";
    private const string ResourceGroup = "rg1";
    private const string DatabaseName = "db1";

    // Distinctive message thrown by the mocked subscription service so tests can prove
    // the service resolves the subscription via ISubscriptionService (the #449/#453 fix)
    // instead of building a SubscriptionResource directly from the raw value.
    private const string SubscriptionResolvedMessage = "SqlServiceTests: subscription resolved via ISubscriptionService";

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
            .ThrowsAsync(new InvalidOperationException(SubscriptionResolvedMessage));

        _service = new SqlService(_subscriptionService, _tenantService, _logger);
    }

    [Fact]
    public async Task GetServerAsync_ResolvesSubscriptionThroughSubscriptionService()
    {
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.GetServerAsync("server1", "rg", SubscriptionName, null, TestContext.Current.CancellationToken));

        Assert.Equal(SubscriptionResolvedMessage, ex.Message);
        await AssertSubscriptionResolvedAsync();
    }

    [Fact]
    public async Task ListServersAsync_ResolvesSubscriptionThroughSubscriptionService()
    {
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.ListServersAsync("rg", SubscriptionName, null, TestContext.Current.CancellationToken));

        Assert.Equal(SubscriptionResolvedMessage, ex.Message);
        await AssertSubscriptionResolvedAsync();
    }

    [Fact]
    public async Task CreateServerAsync_ResolvesSubscriptionThroughSubscriptionService()
    {
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
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

        Assert.Equal(SubscriptionResolvedMessage, ex.Message);
        await AssertSubscriptionResolvedAsync();
    }

    [Fact]
    public async Task RenameDatabaseAsync_ResolvesSubscriptionThroughSubscriptionService()
    {
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.RenameDatabaseAsync(
                "server1",
                "olddb",
                "newdb",
                "rg",
                SubscriptionName,
                null,
                TestContext.Current.CancellationToken));

        Assert.Equal(SubscriptionResolvedMessage, ex.Message);
        await AssertSubscriptionResolvedAsync();
    }

    private Task AssertSubscriptionResolvedAsync() =>
        _subscriptionService.Received(1).GetSubscription(
            SubscriptionName,
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>());

    [Fact]
    public async Task ListDatabasesAsync_WithSubscriptionName_ResolvesNameToId()
    {
        _subscriptionService.IsSubscriptionId(SubscriptionName).Returns(false);
        _subscriptionService.GetSubscriptionIdByName(SubscriptionName, Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException(ResolveSentinel));

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.ListDatabasesAsync(ServerName, ResourceGroup, SubscriptionName, null, TestContext.Current.CancellationToken));

        Assert.Equal(ResolveSentinel, exception.Message);
        await _subscriptionService.Received(1).GetSubscriptionIdByName(
            SubscriptionName, Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetElasticPoolsAsync_WithSubscriptionName_ResolvesNameToId()
    {
        _subscriptionService.IsSubscriptionId(SubscriptionName).Returns(false);
        _subscriptionService.GetSubscriptionIdByName(SubscriptionName, Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException(ResolveSentinel));

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.GetElasticPoolsAsync(ServerName, ResourceGroup, SubscriptionName, null, TestContext.Current.CancellationToken));

        Assert.Equal(ResolveSentinel, exception.Message);
        await _subscriptionService.Received(1).GetSubscriptionIdByName(
            SubscriptionName, Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ListDatabasesAsync_WithSubscriptionId_SkipsNameLookup()
    {
        _subscriptionService.IsSubscriptionId(SubscriptionId).Returns(true);
        var canceled = new CancellationToken(canceled: true);

        try
        {
            await _service.ListDatabasesAsync(ServerName, ResourceGroup, SubscriptionId, null, canceled);
        }
        catch
        {
            // The ARM hierarchy call is expected to fail/cancel; we only assert resolution behavior.
        }

        await _subscriptionService.DidNotReceive().GetSubscriptionIdByName(
            Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetElasticPoolsAsync_WithSubscriptionId_SkipsNameLookup()
    {
        _subscriptionService.IsSubscriptionId(SubscriptionId).Returns(true);
        var canceled = new CancellationToken(canceled: true);

        try
        {
            await _service.GetElasticPoolsAsync(ServerName, ResourceGroup, SubscriptionId, null, canceled);
        }
        catch
        {
            // The ARM hierarchy call is expected to fail/cancel; we only assert resolution behavior.
        }

        await _subscriptionService.DidNotReceive().GetSubscriptionIdByName(
            Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetDatabaseAsync_WithSubscriptionName_ResolvesNameToId()
    {
        _subscriptionService.IsSubscriptionId(SubscriptionName).Returns(false);
        _subscriptionService.GetSubscriptionIdByName(SubscriptionName, Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException(ResolveSentinel));

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.GetDatabaseAsync(ServerName, DatabaseName, ResourceGroup, SubscriptionName, null, TestContext.Current.CancellationToken));

        Assert.Equal(ResolveSentinel, exception.Message);
        await _subscriptionService.Received(1).GetSubscriptionIdByName(
            SubscriptionName, Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetDatabaseAsync_WithSubscriptionId_SkipsNameLookup()
    {
        _subscriptionService.IsSubscriptionId(SubscriptionId).Returns(true);
        var canceled = new CancellationToken(canceled: true);

        try
        {
            await _service.GetDatabaseAsync(ServerName, DatabaseName, ResourceGroup, SubscriptionId, null, canceled);
        }
        catch
        {
            // The ARM hierarchy call is expected to fail/cancel; we only assert resolution behavior.
        }

        await _subscriptionService.DidNotReceive().GetSubscriptionIdByName(
            Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>());
    }
}

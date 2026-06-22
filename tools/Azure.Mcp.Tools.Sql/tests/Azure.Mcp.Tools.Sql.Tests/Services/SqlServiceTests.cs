// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Tools.Sql.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Options;
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

    private readonly ISubscriptionService _subscriptionService;
    private readonly ITenantService _tenantService;
    private readonly ILogger<SqlService> _logger;
    private readonly SqlService _service;

    public SqlServiceTests()
    {
        _subscriptionService = Substitute.For<ISubscriptionService>();
        _tenantService = Substitute.For<ITenantService>();
        _logger = Substitute.For<ILogger<SqlService>>();
        _service = new SqlService(_subscriptionService, _tenantService, _logger);
    }

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

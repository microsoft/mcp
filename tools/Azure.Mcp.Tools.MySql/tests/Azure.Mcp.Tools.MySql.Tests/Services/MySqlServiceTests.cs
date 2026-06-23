// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Services.Azure.ResourceGroup;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Tools.MySql.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Options;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.MySql.Tests.Services;

public class MySqlServiceTests
{
    private readonly IResourceGroupService _resourceGroupService;
    private readonly ISubscriptionService _subscriptionService;
    private readonly ITenantService _tenantService;
    private readonly ILogger<MySqlService> _logger;
    private readonly MySqlService _mysqlService;

    public MySqlServiceTests()
    {
        _resourceGroupService = Substitute.For<IResourceGroupService>();
        _subscriptionService = Substitute.For<ISubscriptionService>();
        _tenantService = Substitute.For<ITenantService>();
        _logger = Substitute.For<ILogger<MySqlService>>();

        _mysqlService = new MySqlService(_resourceGroupService, _subscriptionService, _tenantService, _logger);
    }

    [Fact]
    public void Constructor_WithNullResourceGroupService_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new MySqlService(null!, _subscriptionService, _tenantService, _logger));
    }

    [Fact]
    public void Constructor_WithNullSubscriptionService_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new MySqlService(_resourceGroupService, null!, _tenantService, _logger));
    }

    [Fact]
    public void Constructor_WithValidDependencies_CreatesInstance()
    {
        var service = new MySqlService(_resourceGroupService, _subscriptionService, _tenantService, _logger);
        Assert.NotNull(service);
    }

    [Fact]
    public async Task ListServersAsync_WhenResourceGroupServiceThrows_RethrowsException()
    {
        var exception = new InvalidOperationException("Resource group not found");
        _resourceGroupService.GetResourceGroupResource("sub123", "rg1", Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>()).ThrowsAsync(exception);

        var thrownException = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _mysqlService.ListServersAsync("sub123", "rg1", TestContext.Current.CancellationToken));

        Assert.Equal(exception, thrownException);
    }

    [Fact]
    public async Task ListServersInSubscriptionAsync_WhenSubscriptionServiceThrows_RethrowsException()
    {
        var exception = new InvalidOperationException("Subscription not found");
        _subscriptionService.GetSubscription("sub123", Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>()).ThrowsAsync(exception);

        var thrownException = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _mysqlService.ListServersInSubscriptionAsync("sub123", TestContext.Current.CancellationToken));

        Assert.Equal(exception, thrownException);
    }

    [Fact]
    public async Task ListServersAsync_WhenResourceGroupNotFound_ThrowsKeyNotFoundException()
    {
        _resourceGroupService.GetResourceGroupResource("sub123", "missing-rg", Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Azure.ResourceManager.Resources.ResourceGroupResource?>(null));

        var ex = await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _mysqlService.ListServersAsync("sub123", "missing-rg", TestContext.Current.CancellationToken));

        Assert.Contains("missing-rg", ex.Message);
    }

    [Fact]
    public async Task GetServerConfigAsync_WhenResourceGroupNotFound_ThrowsKeyNotFoundException()
    {
        _resourceGroupService.GetResourceGroupResource("sub123", "missing-rg", Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Azure.ResourceManager.Resources.ResourceGroupResource?>(null));

        var ex = await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _mysqlService.GetServerConfigAsync("sub123", "missing-rg", "some-server", TestContext.Current.CancellationToken));

        Assert.Contains("missing-rg", ex.Message);
    }

    [Fact]
    public async Task GetServerParameterAsync_WhenResourceGroupNotFound_ThrowsKeyNotFoundException()
    {
        _resourceGroupService.GetResourceGroupResource("sub123", "missing-rg", Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Azure.ResourceManager.Resources.ResourceGroupResource?>(null));

        var ex = await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _mysqlService.GetServerParameterAsync("sub123", "missing-rg", "some-server", "some-param", TestContext.Current.CancellationToken));

        Assert.Contains("missing-rg", ex.Message);
    }

    [Fact]
    public async Task SetServerParameterAsync_WhenResourceGroupNotFound_ThrowsKeyNotFoundException()
    {
        _resourceGroupService.GetResourceGroupResource("sub123", "missing-rg", Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Azure.ResourceManager.Resources.ResourceGroupResource?>(null));

        var ex = await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _mysqlService.SetServerParameterAsync("sub123", "missing-rg", "some-server", "some-param", "some-value", TestContext.Current.CancellationToken));

        Assert.Contains("missing-rg", ex.Message);
    }
}

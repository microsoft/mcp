// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tools.MySql.Services;
using Microsoft.Mcp.Core.Options;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.MySql.Tests.Services;

public class MySqlServiceTests
{
    private readonly IAzureService _azureService;
    private readonly MySqlService _mysqlService;

    public MySqlServiceTests()
    {
        _azureService = Substitute.For<IAzureService>();

        _mysqlService = new MySqlService(_azureService);
    }

    [Fact]
    public void Constructor_WithNullAzureService_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new MySqlService(null!));
    }

    [Fact]
    public void Constructor_WithValidDependencies_CreatesInstance()
    {
        var service = new MySqlService(_azureService);
        Assert.NotNull(service);
    }

    [Fact]
    public async Task ListServersAsync_WhenAzureServiceThrows_RethrowsException()
    {
        var exception = new InvalidOperationException("Resource group not found");
        _azureService.GetResourceGroupResource("sub123", "rg1", Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>()).ThrowsAsync(exception);

        var thrownException = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _mysqlService.ListServersAsync("sub123", "rg1", null, TestContext.Current.CancellationToken));

        Assert.Equal(exception, thrownException);
    }

    [Fact]
    public async Task ListServersInSubscriptionAsync_WhenAzureServiceThrows_RethrowsException()
    {
        var exception = new InvalidOperationException("Subscription not found");
        _azureService.GetSubscription("sub123", Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>()).ThrowsAsync(exception);

        var thrownException = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _mysqlService.ListServersInSubscriptionAsync("sub123", null, TestContext.Current.CancellationToken));

        Assert.Equal(exception, thrownException);
    }

    [Fact]
    public async Task ListServersAsync_WhenResourceGroupNotFound_ThrowsKeyNotFoundException()
    {
        _azureService.GetResourceGroupResource("sub123", "missing-rg", Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Azure.ResourceManager.Resources.ResourceGroupResource?>(null));

        var ex = await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _mysqlService.ListServersAsync("sub123", "missing-rg", null, TestContext.Current.CancellationToken));

        Assert.Contains("missing-rg", ex.Message);
    }

    [Fact]
    public async Task GetServerConfigAsync_WhenResourceGroupNotFound_ThrowsKeyNotFoundException()
    {
        _azureService.GetResourceGroupResource("sub123", "missing-rg", Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Azure.ResourceManager.Resources.ResourceGroupResource?>(null));

        var ex = await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _mysqlService.GetServerConfigAsync("sub123", "missing-rg", "some-server", null, TestContext.Current.CancellationToken));

        Assert.Contains("missing-rg", ex.Message);
    }

    [Fact]
    public async Task GetServerParameterAsync_WhenResourceGroupNotFound_ThrowsKeyNotFoundException()
    {
        _azureService.GetResourceGroupResource("sub123", "missing-rg", Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Azure.ResourceManager.Resources.ResourceGroupResource?>(null));

        var ex = await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _mysqlService.GetServerParameterAsync("sub123", "missing-rg", "some-server", "some-param", null, TestContext.Current.CancellationToken));

        Assert.Contains("missing-rg", ex.Message);
    }

    [Fact]
    public async Task SetServerParameterAsync_WhenResourceGroupNotFound_ThrowsKeyNotFoundException()
    {
        _azureService.GetResourceGroupResource("sub123", "missing-rg", Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Azure.ResourceManager.Resources.ResourceGroupResource?>(null));

        var ex = await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _mysqlService.SetServerParameterAsync("sub123", "missing-rg", "some-server", "some-param", "some-value", null, TestContext.Current.CancellationToken));

        Assert.Contains("missing-rg", ex.Message);
    }
}
